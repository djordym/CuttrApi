import React from "react";
import { View, Text, TouchableOpacity, Alert } from "react-native";
import { TradeProposalResponse } from "../../../types/apiTypes";
import { TradeProposalStatus } from "../../../types/enums";
import { COLORS } from "../../../theme/colors";
import { StyleSheet } from "react-native";



interface TradeProposalCardProps {
    proposal: TradeProposalResponse;
    currentUserId: number;
    onUpdateStatus: (proposalId: number, newStatus: TradeProposalStatus) => void;
}

export const TradeProposalCard: React.FC<TradeProposalCardProps> = ({
    proposal,
    currentUserId,
    onUpdateStatus,
}) => {
    // Assume that in our connection the sender is user1.
    const isSender = currentUserId === proposal.connection.user1.userId;
    // My offer comes from the side that belongs to the current user.
    const myPlants = isSender ? proposal.plantsProposedByUser1 : proposal.plantsProposedByUser2;
    const otherPlants = isSender ? proposal.plantsProposedByUser2 : proposal.plantsProposedByUser1;

    // Format creation date
    const createdDate = new Date(proposal.createdAt).toLocaleDateString();

    // Determine available actions based on the proposal status.
    let actions = null;
    if (proposal.tradeProposalStatus === TradeProposalStatus.Pending) {
        if (isSender) {
            // Sender can cancel the proposal.
            actions = (
                <TouchableOpacity
                    style={[styles.actionButton, styles.cancelButton]}
                    onPress={() =>
                        Alert.alert(
                            "Cancel Proposal",
                            "Are you sure you want to cancel this proposal?",
                            [
                                { text: "No" },
                                {
                                    text: "Yes",
                                    onPress: () => onUpdateStatus(proposal.tradeProposalId, TradeProposalStatus.Rejected),
                                    style: "destructive",
                                },
                            ]
                        )
                    }
                >
                    <Text style={styles.actionButtonText}>Cancel</Text>
                </TouchableOpacity>
            );
        } else {
            // Receiver can accept or decline the proposal.
            actions = (
                <View style={styles.actionRow}>
                    <TouchableOpacity
                        style={[styles.actionButton, styles.acceptButton]}
                        onPress={() =>
                            Alert.alert(
                                "Accept Proposal",
                                "Do you want to accept this proposal?",
                                [
                                    { text: "No" },
                                    { text: "Yes", onPress: () => onUpdateStatus(proposal.tradeProposalId, TradeProposalStatus.Accepted) },
                                ]
                            )
                        }
                    >
                        <Text style={styles.actionButtonText}>Accept</Text>
                    </TouchableOpacity>
                    <TouchableOpacity
                        style={[styles.actionButton, styles.rejectButton]}
                        onPress={() =>
                            Alert.alert(
                                "Decline Proposal",
                                "Do you want to decline this proposal?",
                                [
                                    { text: "No" },
                                    { text: "Yes", onPress: () => onUpdateStatus(proposal.tradeProposalId, TradeProposalStatus.Rejected) },
                                ]
                            )
                        }
                    >
                        <Text style={styles.actionButtonText}>Decline</Text>
                    </TouchableOpacity>
                </View>
            );
        }
    } else if (proposal.tradeProposalStatus === TradeProposalStatus.Accepted) {
        // Allow marking the proposal as completed.
        actions = (
            <TouchableOpacity
                style={[styles.actionButton, styles.completeButton]}
                onPress={() =>
                    Alert.alert(
                        "Complete Trade",
                        "Mark this trade as completed?",
                        [
                            { text: "No" },
                            { text: "Yes", onPress: () => onUpdateStatus(proposal.tradeProposalId, TradeProposalStatus.Completed) },
                        ]
                    )
                }
            >
                <Text style={styles.actionButtonText}>Mark Completed</Text>
            </TouchableOpacity>
        );
    }

    return (
        <View style={styles.card}>
            <Text style={styles.cardTitle}>Proposal #{proposal.tradeProposalId}</Text>
            <Text style={styles.cardSubtitle}>Created on: {createdDate}</Text>
            <View style={styles.plantSection}>
                <View style={styles.plantColumn}>
                    <Text style={styles.columnTitle}>{isSender ? "Your Offer" : "Their Offer"}</Text>
                    <View style={styles.plantList}>
                        {myPlants.map((plant) => (
                            <Text key={plant.plantId} style={styles.plantText}>
                                {plant.speciesName}
                            </Text>
                        ))}
                    </View>
                </View>
                <View style={styles.plantColumn}>
                    <Text style={styles.columnTitle}>{isSender ? "Their Offer" : "Your Offer"}</Text>
                    <View style={styles.plantList}>
                        {otherPlants.map((plant) => (
                            <Text key={plant.plantId} style={styles.plantText}>
                                {plant.speciesName}
                            </Text>
                        ))}
                    </View>
                </View>
            </View>
            <Text style={styles.statusText}>Status: {proposal.tradeProposalStatus}</Text>
            {actions}
        </View>
    );
};


export default TradeProposalCard;

const styles = StyleSheet.create({
    card: {
        backgroundColor: "#fff",
        borderRadius: 12,
        padding: 16,
        marginBottom: 16,
        shadowColor: "#000",
        shadowOpacity: 0.12,
        shadowRadius: 6,
        shadowOffset: { width: 0, height: 3 },
        elevation: 3,
    },
    cardTitle: {
        fontSize: 18,
        fontWeight: "700",
        color: COLORS.textDark,
        marginBottom: 4,
    },
    cardSubtitle: {
        fontSize: 14,
        color: COLORS.textDark,
        marginBottom: 12,
    },
    plantSection: {
        flexDirection: "row",
        justifyContent: "space-between",
        marginBottom: 12,
    },
    plantColumn: {
        flex: 1,
        alignItems: "center",
    },
    columnTitle: {
        fontSize: 14,
        fontWeight: "600",
        color: COLORS.textDark,
        marginBottom: 4,
    },
    plantList: {
        // This is a placeholder styling – you could replace this with a horizontal scroll of thumbnails.
    },
    plantText: {
        fontSize: 13,
        color: COLORS.textDark,
    },
    statusText: {
        fontSize: 14,
        fontWeight: "600",
        color: COLORS.textDark,
        marginBottom: 12,
        textAlign: "center",
    },
    actionRow: {
        flexDirection: "row",
        justifyContent: "space-around",
    },
    actionButton: {
        flex: 1,
        paddingVertical: 10,
        marginHorizontal: 4,
        borderRadius: 8,
        alignItems: "center",
        justifyContent: "center",
    },
    acceptButton: {
        backgroundColor: COLORS.primary,
    },
    rejectButton: {
        backgroundColor: COLORS.accentRed,
    },
    cancelButton: {
        backgroundColor: COLORS.accentRed,
    },
    completeButton: {
        backgroundColor: COLORS.secondary,
    },
    actionButtonText: {
        color: "#fff",
        fontWeight: "600",
        fontSize: 14,
    },

})