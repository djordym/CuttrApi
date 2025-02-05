import React from "react";
import { ActivityIndicator, FlatList, StyleSheet, Text, TouchableOpacity, View } from "react-native";
import { SafeAreaProvider } from "react-native-safe-area-context";
import { useNavigation, useRoute } from "@react-navigation/native";
import { LinearGradient } from "expo-linear-gradient";
import { COLORS } from "../../../theme/colors";
import { useMyProfile } from "../hooks/useMyProfileHooks";
import { useTradeProposals } from "../hooks/useTradeProposalHooks";
import { TradeProposalResponse } from "../../../types/apiTypes";
import { TradeProposalCard } from "../components/TradeProposalComponent";
import { headerStyles } from "../styles/headerStyles";
import { useUpdateTradeProposalStatus } from "../hooks/useTradeProposalHooks";
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

    // Mutation hook to update a proposal's status.
    const { mutate: updateStatus } = useUpdateTradeProposalStatus(connectionId);

    // Handler to update the status.
    const handleUpdateStatus = (proposalId: number, newStatus: TradeProposalStatus) => {
        updateStatus({ proposalId, newStatus });
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

    // Render each proposal using TradeProposalCard.
    const renderItem = ({ item }: { item: TradeProposalResponse }) => {
        return (
            <TradeProposalCard
                proposal={item}
                currentUserId={myProfile!.userId}
                onUpdateStatus={handleUpdateStatus}
            />
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

});