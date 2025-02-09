import React from "react";
import {
  StyleSheet,
  Text,
  View,
  TouchableOpacity,
} from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { COLORS } from "../../../theme/colors";
import PlantThumbnail from "./PlantThumbnail";

type CompletedTradeActionsProps = {
  plants: any[];
  proposalId: number;
  onConfirmDecisions: (proposalId: number, plantsToDelete: number[]) => void;
  onPlantInfoPress?: (plant: any) => void;
  isConfirming?: boolean; // optional loading state if you'd like to disable the button
};

const CompletedTradeActions: React.FC<CompletedTradeActionsProps> = ({
  plants,
  proposalId,
  onConfirmDecisions,
  onPlantInfoPress,
  isConfirming = false,
}) => {
  // "deleted" or "keep" or undefined if not yet decided
  const [decisions, setDecisions] = React.useState<{
    [plantId: number]: "delete" | "keep" | undefined;
  }>({});

  const markPlant = (
    plantId: number,
    decision: "delete" | "keep" | undefined
  ) => {
    setDecisions((prev) => ({ ...prev, [plantId]: decision }));
  };

  // Determine if all plants have a decision
  const allDecided =
    plants.length > 0 &&
    plants.every((p) => decisions[p.plantId] !== undefined);

  // Handle confirm (call the parent’s callback)
  const handleConfirm = () => {
    // Gather plant IDs marked for deletion
    const plantsToDelete = plants
      .filter((p) => decisions[p.plantId] === "delete")
      .map((p) => p.plantId);

    onConfirmDecisions(proposalId, plantsToDelete);
  };

  return (
    <View style={styles.completedSection}>
      <Text style={styles.completedPrompt}>
        Trade complete – choose your action for your plants:
      </Text>
      <View style={styles.plantGrid}>
        {plants.map((plant) => {
          const decision = decisions[plant.plantId];
          return (
            <View key={plant.plantId} style={styles.plantThumbnailContainer}>
              <PlantThumbnail
                plant={plant}
                selectable={false}
                onInfoPress={() =>
                  onPlantInfoPress && onPlantInfoPress(plant)
                }
              />
              {decision ? (
                <View style={styles.decisionRow}>
                  <View style={styles.decisionTag}>
                    <Text style={styles.decisionLabelText}>
                      {decision === "delete" ? "Delete" : "Keep"}
                    </Text>
                  </View>
                  <TouchableOpacity
                    style={styles.undoButton}
                    onPress={() => markPlant(plant.plantId, undefined)}
                  >
                    <Ionicons name="arrow-undo" size={14} color="#fff" />
                  </TouchableOpacity>
                </View>
              ) : (
                <View style={styles.plantActions}>
                  <TouchableOpacity
                    style={[
                      styles.plantActionButton,
                      styles.individualDeleteButton,
                    ]}
                    onPress={() => markPlant(plant.plantId, "delete")}
                  >
                    <Text style={styles.plantActionText}>Delete</Text>
                  </TouchableOpacity>
                  <TouchableOpacity
                    style={[
                      styles.plantActionButton,
                      styles.individualKeepButton,
                    ]}
                    onPress={() => markPlant(plant.plantId, "keep")}
                  >
                    <Text style={styles.plantActionText}>Keep</Text>
                  </TouchableOpacity>
                </View>
              )}
            </View>
          );
        })}
      </View>

      {/* "Confirm" button - only enable if all plants decided */}
      <TouchableOpacity
        style={[
          styles.confirmButton,
          (!allDecided || isConfirming) && styles.confirmButtonDisabled,
        ]}
        onPress={handleConfirm}
        disabled={!allDecided || isConfirming}
      >
        <Text style={styles.confirmButtonText}>
          {isConfirming ? "Processing..." : "Confirm"}
        </Text>
      </TouchableOpacity>
    </View>
  );
};

export default CompletedTradeActions;

const styles = StyleSheet.create({
  completedSection: {
    marginTop: 10,
    alignItems: "center",
  },
  completedPrompt: {
    fontSize: 14,
    marginBottom: 6,
    color: COLORS.textDark,
  },
  // Container for the plant grid instead of a scroll view
  plantGrid: {
    flexDirection: "row",
    flexWrap: "wrap",
    justifyContent: "center",
    paddingHorizontal: 10,
  },
  plantThumbnailContainer: {
    margin: 5, // Increased margin to provide spacing between grid items
    alignItems: "center",
  },
  plantActions: {
    flexDirection: "row",
    marginTop: 4,
  },
  plantActionButton: {
    paddingVertical: 6,
    paddingHorizontal: 8,
    borderRadius: 6,
    marginHorizontal: 2,
  },
  plantActionText: {
    color: "#fff",
    fontSize: 12,
  },
  individualDeleteButton: {
    backgroundColor: COLORS.accentRed,
  },
  individualKeepButton: {
    backgroundColor: COLORS.accentGreen,
  },
  decisionRow: {
    flexDirection: "row",
    alignItems: "center",
    marginTop: 4,
  },
  decisionTag: {
    backgroundColor: "#ddd",
    borderRadius: 6,
    paddingHorizontal: 6,
    paddingVertical: 2,
  },
  decisionLabelText: {
    fontSize: 12,
    color: "#555",
    fontWeight: "600",
  },
  undoButton: {
    marginLeft: 6,
    backgroundColor: COLORS.accentOrange,
    paddingHorizontal: 6,
    paddingVertical: 4,
    borderRadius: 6,
  },
  confirmButton: {
    marginTop: 10,
    backgroundColor: COLORS.accentGreen,
    paddingHorizontal: 20,
    paddingVertical: 10,
    borderRadius: 8,
  },
  confirmButtonDisabled: {
    backgroundColor: "#ccc",
  },
  confirmButtonText: {
    color: "#fff",
    fontWeight: "600",
    fontSize: 16,
  },
});
