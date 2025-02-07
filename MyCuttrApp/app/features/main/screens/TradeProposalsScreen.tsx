// File: app/features/main/screens/TradeProposalsScreen.tsx

import React from "react";
import {
  ActivityIndicator,
  Alert,
  FlatList,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import { SafeAreaProvider } from "react-native-safe-area-context";
import { useNavigation, useRoute } from "@react-navigation/native";
import { LinearGradient } from "expo-linear-gradient";
import { COLORS } from "../../../theme/colors";
import { useMyProfile } from "../hooks/useMyProfileHooks";
import {
  useTradeProposals,
  useUpdateTradeProposalStatus,
  useConfirmTradeProposalCompletion,
} from "../hooks/useTradeProposalHooks";
import { TradeProposalResponse } from "../../../types/apiTypes";
import { TradeProposalStatus } from "../../../types/enums";
import PlantThumbnail from "../components/PlantThumbnail";
import InfoModal from "../modals/InfoModal";
import { headerStyles } from "../styles/headerStyles";

type RouteParams = {
  connectionId: number;
};

// ----- NEW COMPONENT: CompletedTradeActions -----
// This component renders the plant thumbnails for a completed trade
// and allows the user to individually mark each plant as "deleted" or "kept."
// Once all plants have a decision, the provided confirmCompletion function is called.
const CompletedTradeActions: React.FC<{
  plants: any[];
  proposalId: number;
  confirmCompletion: (proposalId: number) => void;
}> = ({ plants, proposalId, confirmCompletion }) => {
  const [decisions, setDecisions] = React.useState<{ [plantId: number]: "deleted" | "kept" }>({});

  const markPlant = (plantId: number, decision: "deleted" | "kept") => {
    setDecisions((prev) => ({ ...prev, [plantId]: decision }));
  };

  // If all plants have been marked, automatically confirm completion.
  React.useEffect(() => {
    if (plants.length > 0 && plants.every((plant) => decisions[plant.plantId] !== undefined)) {
      confirmCompletion(proposalId);
    }
  }, [decisions, plants, proposalId, confirmCompletion]);

  // "Delete All" marks all plants as deleted.
  const handleDeleteAll = () => {
    const newDecisions: { [plantId: number]: "deleted" | "kept" } = {};
    plants.forEach((plant) => {
      newDecisions[plant.plantId] = "deleted";
    });
    setDecisions(newDecisions);
  };

  return (
    <View style={styles.completedSection}>
      <Text style={styles.completedPrompt}>
        Trade complete – choose your action for your plants:
      </Text>
      <ScrollView horizontal contentContainerStyle={styles.plantScroll}>
        {plants.map((plant) => {
          const decision = decisions[plant.plantId];
          return (
            <View key={plant.plantId} style={styles.plantThumbnailContainer}>
              <PlantThumbnail
                plant={plant}
                selectable={false}
                onInfoPress={() => {}}
              />
              {decision ? (
                <View style={styles.decisionLabelContainer}>
                  <Text style={styles.decisionLabelText}>
                    {decision === "deleted" ? "Deleted" : "Kept"}
                  </Text>
                </View>
              ) : (
                <View style={styles.plantActions}>
                  <TouchableOpacity
                    style={[styles.plantActionButton, styles.individualDeleteButton]}
                    onPress={() => markPlant(plant.plantId, "deleted")}
                  >
                    <Text style={styles.plantActionText}>Delete</Text>
                  </TouchableOpacity>
                  <TouchableOpacity
                    style={[styles.plantActionButton, styles.individualKeepButton]}
                    onPress={() => markPlant(plant.plantId, "kept")}
                  >
                    <Text style={styles.plantActionText}>Keep</Text>
                  </TouchableOpacity>
                </View>
              )}
            </View>
          );
        })}
      </ScrollView>
      {/* Show the "Delete All" button only if at least one plant is not yet decided */}
      {plants.some((plant) => decisions[plant.plantId] === undefined) && (
        <View style={styles.allActionsRow}>
          <TouchableOpacity
            style={[styles.actionButton, styles.deleteAllButton]}
            onPress={handleDeleteAll}
          >
            <Text style={styles.actionButtonText}>Delete All</Text>
          </TouchableOpacity>
        </View>
      )}
    </View>
  );
};

// ----- END NEW COMPONENT

const TradeProposalsScreen: React.FC = () => {
  const route = useRoute();
  const navigation = useNavigation();
  const { connectionId } = route.params as RouteParams;

  const { data: myProfile, isLoading: profileLoading, isError: profileError } =
    useMyProfile();

  const {
    data: proposals,
    isLoading,
    isError,
    refetch,
  } = useTradeProposals(connectionId);

  const { mutate: updateStatus } = useUpdateTradeProposalStatus(connectionId);
  const { mutate: confirmCompletion } = useConfirmTradeProposalCompletion(connectionId);

  // Helper: update status for transitions like Accepted, Rejected, etc.
  const handleUpdateStatus = (proposalId: number, newStatus: TradeProposalStatus) => {
    updateStatus({ proposalId, newStatus });
  };

  // Render each trade proposal card.
  const renderItem = ({ item }: { item: TradeProposalResponse }) => {
    const isOwner = myProfile!.userId === item.proposalOwnerUserId;
    const myPlants = isOwner ? item.plantsProposedByUser1 : item.plantsProposedByUser2;
    const otherPlants = isOwner ? item.plantsProposedByUser2 : item.plantsProposedByUser1;
    const hasConfirmed = isOwner
      ? item.ownerCompletionConfirmed
      : item.responderCompletionConfirmed;

    // Handlers for pending proposals.
    const handleAccept = () =>
      Alert.alert("Accept Proposal", "Do you want to accept this proposal?", [
        { text: "No" },
        {
          text: "Yes",
          onPress: () =>
            handleUpdateStatus(item.tradeProposalId, TradeProposalStatus.Accepted),
        },
      ]);
    const handleDecline = () =>
      Alert.alert("Decline Proposal", "Do you want to decline this proposal?", [
        { text: "No" },
        {
          text: "Yes",
          onPress: () =>
            handleUpdateStatus(item.tradeProposalId, TradeProposalStatus.Rejected),
        },
      ]);
    const handleCancel = () =>
      Alert.alert("Cancel Proposal", "Do you want to cancel this proposal?", [
        { text: "No" },
        {
          text: "Yes",
          onPress: () =>
            handleUpdateStatus(item.tradeProposalId, TradeProposalStatus.Rejected),
        },
      ]);
    // For accepted proposals.
    const handleMarkCompleted = () =>
      Alert.alert("Complete Trade", "Mark this trade as completed?", [
        { text: "No" },
        {
          text: "Yes",
          onPress: () =>
            handleUpdateStatus(item.tradeProposalId, TradeProposalStatus.Completed),
        },
      ]);

    let actions = null;
    if (item.tradeProposalStatus === TradeProposalStatus.Pending) {
      actions = isOwner ? (
        <TouchableOpacity
          style={[styles.actionButton, styles.cancelButton]}
          onPress={handleCancel}
        >
          <Text style={styles.actionButtonText}>Cancel Proposal</Text>
        </TouchableOpacity>
      ) : (
        <View style={styles.actionRow}>
          <TouchableOpacity
            style={[styles.actionButton, styles.acceptButton]}
            onPress={handleAccept}
          >
            <Text style={styles.actionButtonText}>Accept</Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={[styles.actionButton, styles.rejectButton]}
            onPress={handleDecline}
          >
            <Text style={styles.actionButtonText}>Decline</Text>
          </TouchableOpacity>
        </View>
      );
    } else if (item.tradeProposalStatus === TradeProposalStatus.Accepted) {
      actions = (
        <TouchableOpacity
          style={[styles.actionButton, styles.completeButton]}
          onPress={handleMarkCompleted}
        >
          <Text style={styles.actionButtonText}>Mark as Completed</Text>
        </TouchableOpacity>
      );
    } else if (item.tradeProposalStatus === TradeProposalStatus.Completed) {
      // When completed: if the user has not yet confirmed their decision,
      // render the CompletedTradeActions component; otherwise, show a simple label.
      actions = !hasConfirmed ? (
        <CompletedTradeActions
          plants={isOwner ? myPlants : otherPlants}
          proposalId={item.tradeProposalId}
          confirmCompletion={confirmCompletion}
        />
      ) : (
        <View style={styles.completedSection}>
          <Text style={styles.completedMessage}>Trade Completed</Text>
        </View>
      );
    }

    return (
      <View style={styles.card}>
        <Text style={styles.cardTitle}>Proposal #{item.tradeProposalId}</Text>
        <Text style={styles.cardSubtitle}>
          Created: {new Date(item.createdAt).toLocaleString()}
        </Text>
        <View style={styles.offersSection}>
          <View style={styles.offerColumn}>
            <Text style={styles.columnTitle}>
              {isOwner ? "Your Offer" : "Their Offer"}
            </Text>
            <ScrollView horizontal contentContainerStyle={styles.offerScroll}>
              {myPlants.map((plant) => (
                <PlantThumbnail
                  key={plant.plantId}
                  plant={plant}
                  selectable={false}
                  onInfoPress={() => {}}
                />
              ))}
            </ScrollView>
          </View>
          <View style={styles.offerColumn}>
            <Text style={styles.columnTitle}>
              {isOwner ? "Their Offer" : "Your Offer"}
            </Text>
            <ScrollView horizontal contentContainerStyle={styles.offerScroll}>
              {otherPlants.map((plant) => (
                <PlantThumbnail
                  key={plant.plantId}
                  plant={plant}
                  selectable={false}
                  onInfoPress={() => {}}
                />
              ))}
            </ScrollView>
          </View>
        </View>
        <Text style={styles.statusText}>Status: {item.tradeProposalStatus}</Text>
        {actions}
      </View>
    );
  };

  if (isLoading || profileLoading) {
    return (
      <SafeAreaProvider style={styles.loadingContainer}>
        <ActivityIndicator size="large" color={COLORS.primary} />
      </SafeAreaProvider>
    );
  }
  if (isError || profileError) {
    return (
      <SafeAreaProvider style={styles.errorContainer}>
        <Text style={styles.errorText}>Failed to load trade proposals.</Text>
        <TouchableOpacity
          style={styles.retryButton}
          onPress={() => refetch()}
        >
          <Text style={styles.retryButtonText}>Retry</Text>
        </TouchableOpacity>
      </SafeAreaProvider>
    );
  }
  return (
    <SafeAreaProvider style={styles.container}>
      <LinearGradient
        colors={[COLORS.primary, COLORS.secondary]}
        style={headerStyles.headerGradient}
      >
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
  card: {
    backgroundColor: "#fff",
    borderRadius: 10,
    padding: 16,
    marginBottom: 12,
    shadowColor: "#000",
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 3,
  },
  cardTitle: {
    fontSize: 18,
    fontWeight: "bold",
    marginBottom: 4,
  },
  cardSubtitle: {
    fontSize: 14,
    marginBottom: 12,
  },
  offersSection: {
    flexDirection: "row",
    justifyContent: "space-between",
    marginBottom: 12,
  },
  offerColumn: {
    flex: 1,
    alignItems: "center",
  },
  columnTitle: {
    fontSize: 14,
    fontWeight: "600",
    marginBottom: 4,
  },
  offerScroll: {
    paddingHorizontal: 5,
  },
  statusText: {
    fontSize: 14,
    fontWeight: "600",
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
  // Completed trade section styles:
  completedSection: {
    marginTop: 10,
    alignItems: "center",
  },
  completedPrompt: {
    fontSize: 14,
    marginBottom: 6,
    color: COLORS.textDark,
  },
  completedMessage: {
    fontSize: 16,
    fontWeight: "600",
    color: COLORS.textDark,
    paddingVertical: 10,
  },
  plantScroll: {
    paddingHorizontal: 10,
  },
  plantThumbnailContainer: {
    marginRight: 10,
    alignItems: "center",
  },
  plantActions: {
    flexDirection: "row",
    marginTop: 4,
  },
  plantActionButton: {
    backgroundColor: COLORS.primary,
    paddingVertical: 6,
    paddingHorizontal: 8,
    borderRadius: 6,
    marginHorizontal: 2,
  },
  plantActionText: {
    color: "#fff",
    fontSize: 12,
  },
  // New styles for the inline CompletedTradeActions component:
  decisionLabelContainer: {
    marginTop: 4,
    paddingVertical: 4,
    paddingHorizontal: 8,
    backgroundColor: "#ddd",
    borderRadius: 6,
  },
  decisionLabelText: {
    fontSize: 12,
    color: "#555",
    fontWeight: "600",
  },
  individualDeleteButton: {
    backgroundColor: COLORS.accentRed,
  },
  individualKeepButton: {
    backgroundColor: COLORS.primary,
  },
  allActionsRow: {
    flexDirection: "row",
    marginTop: 10,
    justifyContent: "space-around",
    width: "100%",
  },
});
