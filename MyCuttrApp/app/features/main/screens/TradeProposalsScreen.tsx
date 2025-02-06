// File: app/features/main/screens/TradeProposalsScreen.tsx
import React from "react";
import { ActivityIndicator, FlatList, StyleSheet, Text, TouchableOpacity, View, Alert } from "react-native";
import { SafeAreaProvider } from "react-native-safe-area-context";
import { useNavigation, useRoute } from "@react-navigation/native";
import { LinearGradient } from "expo-linear-gradient";
import { COLORS } from "../../../theme/colors";
import { useMyProfile } from "../hooks/useMyProfileHooks";
import { useTradeProposals, useUpdateTradeProposalStatus } from "../hooks/useTradeProposalHooks";
import { TradeProposalResponse } from "../../../types/apiTypes";
import { headerStyles } from "../styles/headerStyles";

// Assume TradeProposalStatus is an enum that includes e.g. Accepted and Completed.
import { TradeProposalStatus } from "../../../types/enums";

type RouteParams = {
    connectionId: number;
};

const TradeProposalsScreen: React.FC = () => {
    const route = useRoute();
    const navigation = useNavigation();

    // Extract the connectionId from route parameters.
    const { connectionId } = route.params as RouteParams;

    // Get current user profile.
    const { data: myProfile, isLoading: profileLoading, isError: profileError } = useMyProfile();

    // Fetch trade proposals for this connection.
    const {
        data: proposals,
        isLoading,
        isError,
        refetch,
    } = useTradeProposals(connectionId);

    // Mutation hook to update a proposal's status (or, in our new logic, to confirm completion).
    const { mutate: updateStatus } = useUpdateTradeProposalStatus(connectionId);

    // Handler to update the status.
    const handleUpdateStatus = (proposalId: number, newStatus: TradeProposalStatus) => {
        updateStatus({ proposalId, newStatus });
    };

    // Handler for when the user confirms that the trade is complete.
    const handleConfirmCompletion = (proposal: TradeProposalResponse) => {
        // Here you might send a status update such as "ConfirmCompletion"
        // The backend should detect which confirmation flag to set based on the current user.
        // For simplicity, we assume that sending TradeProposalStatus.Completed will
        // update the appropriate flag and, if both are true, mark the proposal as completed.
        handleUpdateStatus(proposal.tradeProposalId, TradeProposalStatus.Completed);
    };

    // Handler for deleting a traded plant. (Implementation depends on your deletion hook/API.)
    const handleDeletePlant = (plantId: number) => {
        //iun here we will mark the confirmed its to prompt the user in case he has multilple cuttings or something so that he can choose wether or not to delete his plant entry
        Alert.alert(
            "Delete Plant",
            "Are you sure you want to remove this plant from your inventory?",
            [
                { text: "Cancel", style: "cancel" },
                { text: "Delete", onPress: () => console.log(`Deleting plant ${plantId}`) },
            ]
        );
    };

    // Display a loading indicator if queries are loading.
    if (isLoading || profileLoading) {
        return (
            <SafeAreaProvider style={styles.loadingContainer}>
                <ActivityIndicator size="large" color={COLORS.primary} />
            </SafeAreaProvider>
        );
    }

    // Display an error message if an error occurred.
    if (isError || profileError) {
        return (
            <SafeAreaProvider style={styles.errorContainer}>
                <Text style={styles.errorText}>Failed to load trade proposals.</Text>
                <TouchableOpacity style={styles.retryButton} onPress={() => refetch()}>
                    <Text style={styles.retryButtonText}>Retry</Text>
                </TouchableOpacity>
            </SafeAreaProvider>
        );
    }

    // Render each proposal.
    const renderItem = ({ item }: { item: TradeProposalResponse }) => {
        // Determine if the current user is the proposal owner.
        const isOwner = myProfile!.userId === item.proposalOwnerUserId;

        // Determine whether the current user has confirmed completion.
        const hasConfirmed = isOwner ? item.ownerCompletionConfirmed : item.responderCompletionConfirmed;

        return (
            <View style={styles.proposalCard}>
                {/* Render proposal details – you might have a dedicated TradeProposalCard component */}
                <Text style={styles.proposalTitle}>Trade Proposal #{item.tradeProposalId}</Text>
                <Text>Status: {item.tradeProposalStatus}</Text>
                <Text>Created: {new Date(item.createdAt).toLocaleString()}</Text>
                
                {/* Show a "Confirm Completion" button if:
                      - The proposal has been accepted (but not yet fully completed)
                      - The current user has not yet confirmed completion */}
                {item.tradeProposalStatus === TradeProposalStatus.Accepted && !hasConfirmed && (
                    <TouchableOpacity
                        style={styles.confirmButton}
                        onPress={() => handleConfirmCompletion(item)}
                    >
                        <Text style={styles.buttonText}>Confirm Completion</Text>
                    </TouchableOpacity>
                )}

                {/* If the proposal is completed, show an option to delete one’s own traded plant.
                    Assume that if you are the owner your traded plants are in plantsProposedByUser1;
                    otherwise in plantsProposedByUser2. (And only if the plant still exists in your inventory.) */}
                {item.tradeProposalStatus === TradeProposalStatus.Completed && (
                    <View style={styles.deleteSection}>
                        <Text style={styles.deletePrompt}>Trade complete – remove your traded plants:</Text>
                        {(isOwner ? item.plantsProposedByUser1 : item.plantsProposedByUser2).map((plant) => (
                            <TouchableOpacity
                                key={plant.plantId}
                                style={styles.deleteButton}
                                onPress={() => handleDeletePlant(plant.plantId)}
                            >
                                <Text style={styles.deleteButtonText}>Delete {plant.speciesName}</Text>
                            </TouchableOpacity>
                        ))}
                    </View>
                )}
            </View>
        );
    };

    return (
        <SafeAreaProvider style={styles.container}>
            <LinearGradient colors={[COLORS.primary, COLORS.secondary]} style={headerStyles.headerGradient}>
                <View style={headerStyles.headerRow}>
                    <TouchableOpacity
                        style={headerStyles.headerBackButton}
                        onPress={() => navigation.goBack()}
                    >
                        <Text style={headerStyles.headerTitle}>Back</Text>
                    </TouchableOpacity>
                    <Text style={headerStyles.headerTitle}>Trade Proposals</Text>
                    <View style={{ width: 50 }} />
                </View>
            </LinearGradient>
            {proposals && proposals.length > 0 ? (
                <FlatList
                    data={proposals}
                    keyExtractor={(item) => item.tradeProposalId.toString()}
                    renderItem={renderItem}
                    contentContainerStyle={styles.listContent}
                />
            ) : (
                <View style={styles.emptyContainer}>
                    <Text style={styles.emptyText}>No trade proposals found.</Text>
                </View>
            )}
        </SafeAreaProvider>
    );
};

export default TradeProposalsScreen;

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: COLORS.background,
    },
    loadingContainer: {
        flex: 1,
        justifyContent: "center",
        alignItems: "center",
    },
    errorContainer: {
        flex: 1,
        justifyContent: "center",
        alignItems: "center",
        paddingHorizontal: 20,
    },
    errorText: {
        fontSize: 16,
        color: COLORS.textDark,
        marginBottom: 10,
        textAlign: "center",
    },
    retryButton: {
        backgroundColor: COLORS.primary,
        paddingVertical: 10,
        paddingHorizontal: 20,
        borderRadius: 8,
    },
    retryButtonText: {
        color: "#fff",
        fontWeight: "600",
        fontSize: 16,
    },
    listContent: {
        padding: 16,
    },
    emptyContainer: {
        flex: 1,
        justifyContent: "center",
        alignItems: "center",
        paddingHorizontal: 20,
    },
    emptyText: {
        fontSize: 16,
        color: COLORS.textDark,
    },
    proposalCard: {
        backgroundColor: "#fff",
        borderRadius: 10,
        padding: 16,
        marginBottom: 12,
        shadowColor: "#000",
        shadowOpacity: 0.1,
        shadowRadius: 4,
        shadowOffset: { width: 0, height: 2 },
        elevation: 3,
    },
    proposalTitle: {
        fontSize: 18,
        fontWeight: "bold",
        marginBottom: 8,
    },
    confirmButton: {
        backgroundColor: COLORS.primary,
        paddingVertical: 10,
        paddingHorizontal: 16,
        borderRadius: 8,
        marginTop: 10,
    },
    buttonText: {
        color: "#fff",
        fontSize: 16,
        textAlign: "center",
    },
    deleteSection: {
        marginTop: 10,
    },
    deletePrompt: {
        fontSize: 14,
        marginBottom: 6,
        color: COLORS.textDark,
    },
    deleteButton: {
        backgroundColor: "#d9534f",
        paddingVertical: 8,
        paddingHorizontal: 12,
        borderRadius: 6,
        marginBottom: 6,
    },
    deleteButtonText: {
        color: "#fff",
        fontSize: 14,
    },
});
