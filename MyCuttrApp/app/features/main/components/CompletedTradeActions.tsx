import React from "react";
import {
  StyleSheet,
  Text,
  View,
  ScrollView,
  TouchableOpacity,
} from "react-native";
import { COLORS } from "../../../theme/colors";
import PlantThumbnail from "./PlantThumbnail";

type CompletedTradeActionsProps = {
  plants: any[];
  proposalId: number;
  confirmCompletion: (proposalId: number) => void;
  // New optional callback for opening the plant info modal.
  onPlantInfoPress?: (plant: any) => void;
};

const CompletedTradeActions: React.FC<CompletedTradeActionsProps> = ({
  plants,
  proposalId,
  confirmCompletion,
  onPlantInfoPress,
}) => {
  const [decisions, setDecisions] = React.useState<{
    [plantId: number]: "deleted" | "kept";
  }>({});

  const markPlant = (plantId: number, decision: "deleted" | "kept") => {
    setDecisions((prev) => ({ ...prev, [plantId]: decision }));
  };

  // Automatically confirm completion when all plants have a decision.
  React.useEffect(() => {
    if (plants.length > 0 && plants.every((p) => decisions[p.plantId] !== undefined)) {
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

  // "Keep All" marks all plants as kept.
  const handleKeepAll = () => {
    const newDecisions: { [plantId: number]: "deleted" | "kept" } = {};
    plants.forEach((plant) => {
      newDecisions[plant.plantId] = "kept";
    });
    setDecisions(newDecisions);
  };

  const hasUndecidedPlants = plants.some(
    (plant) => decisions[plant.plantId] === undefined
  );

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
                // Call the provided onPlantInfoPress when info is pressed.
                onInfoPress={() => onPlantInfoPress && onPlantInfoPress(plant)}
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
      {hasUndecidedPlants && (
        <View style={styles.allActionsRow}>
          <TouchableOpacity
            style={[styles.actionButton, styles.keepAllButton]}
            onPress={handleKeepAll}
          >
            <Text style={styles.actionButtonText}>Keep All</Text>
          </TouchableOpacity>
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
  plantScroll: {
    paddingHorizontal: 10,
  },
  plantThumbnailContainer: {
    marginRight: 10,
    alignItems: "center",
  },
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
  allActionsRow: {
    flexDirection: "row",
    marginTop: 10,
    justifyContent: "center",
    alignItems: "center",
    width: "100%",
  },
  actionButton: {
    flex: 1,
    paddingVertical: 10,
    marginHorizontal: 2,
    borderRadius: 8,
    alignItems: "center",
    justifyContent: "center",
  },
  actionButtonText: {
    color: "#fff",
    fontWeight: "600",
    fontSize: 14,
  },
  keepAllButton: {
    backgroundColor: COLORS.accentGreen,
  },
  deleteAllButton: {
    backgroundColor: COLORS.accentRed,
  },
});
