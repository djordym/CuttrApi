import React, { useState, useEffect } from 'react';
import {
  Modal,
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  ScrollView,
  Image,
  ActivityIndicator,
  Platform,
  Dimensions,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useMyPlants } from '../hooks/usePlantHooks'; // Adjust import path if necessary
import { PlantResponse } from '../../../types/apiTypes';
import { t } from 'i18next';
import { COLORS } from '../../../theme/colors';
import ConfirmCancelButtons from '../components/ConfirmCancelButtons';
import { PlantThumbnail } from '../components/PlantThumbnail';

interface SelectPlantsModalProps {
  visible: boolean;
  /**
   * Called when the user confirms their selection.
   * Passes an array of plant IDs that the user selected.
   */
  onConfirm: (selectedPlantIds: number[]) => void;
  /**
   * Called when the user cancels/closes the modal (no selection returned).
   */
  onClose: () => void;
}

const { width } = Dimensions.get('window');

export const SelectPlantsModal: React.FC<SelectPlantsModalProps> = ({
  visible,
  onConfirm,
  onClose,
}) => {
  // Fetch the user’s plants
  const {
    data: myPlants,
    isLoading,
    isError,
    refetch,
  } = useMyPlants();

  // Track which plants have been selected
  const [selectedPlantIds, setSelectedPlantIds] = useState<number[]>([]);

  // Reset selection each time the modal opens
  useEffect(() => {
    if (visible) {
      setSelectedPlantIds([]);
      // Optionally refresh user plants each time the modal opens
      refetch();
    }
  }, [visible, refetch]);

  const handleToggleSelect = (plantId: number) => {
    setSelectedPlantIds((prev) => {
      if (prev.includes(plantId)) {
        // Already selected => deselect
        return prev.filter((id) => id !== plantId);
      } else {
        // Not selected => select
        return [...prev, plantId];
      }
    });
  };

  const handleConfirm = () => {
    onConfirm(selectedPlantIds);
  };

  if (!visible) {
    return null;
  }

  return (
    <Modal visible={visible} transparent animationType="slide">
      <View style={styles.modalContainer}>
        <View style={styles.modalContent}>
          

          {isLoading && (
            <View style={styles.loadingContainer}>
              <ActivityIndicator size="large" color={COLORS.primary} />
              <Text style={styles.loadingText}>Loading your plants...</Text>
            </View>
          )}

          {isError && (
            <View style={styles.errorContainer}>
              <Text style={styles.errorText}>
                Could not load your plants. Please try again.
              </Text>
              <TouchableOpacity style={styles.retryButton} onPress={refetch}>
                <Text style={styles.retryButtonText}>Retry</Text>
              </TouchableOpacity>
            </View>
          )}

          {!isLoading && !isError && myPlants && (
            <>
              {myPlants.length === 0 ? (
                <View style={styles.emptyStateContainer}>
                  <Text style={styles.emptyStateText}>
                    You have no plants in your collection yet.
                  </Text>
                </View>
              ) : (
                <ScrollView style={styles.scrollArea}>
                  <Text style={styles.modalTitle}>Select Plants</Text>
                  <Text style={styles.titleText}>
                    Select the plants you want to trade for. Tap on a plant to select it.
                  </Text>
                  <View style={styles.gridContainer}>
                    {myPlants.map((plant: PlantResponse) => {
                      const isSelected = selectedPlantIds.includes(plant.plantId);
                      return (
                        <PlantThumbnail
                          key={plant.plantId}
                          plant={plant}
                          isSelected={isSelected}
                          selectable={true}
                          onPress={() => handleToggleSelect(plant.plantId)}
                        />
                      );
                    })}
                  </View>
                  <View style={styles.actionbuttons}>
                  <ConfirmCancelButtons
                    onConfirm={handleConfirm}
                    onCancel={onClose}
                    confirmButtonText="Confirm"
                    cancelButtonText="Cancel"
                  />
                  </View>
                </ScrollView>
              )}
            </>
          )}

          

        </View>
      </View>
    </Modal>
  );
};

export default SelectPlantsModal;

// Replicating (and adapting) styles from your MyProfileScreen thumbnails:
const styles = StyleSheet.create({
  modalContainer: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.5)',
    justifyContent: 'flex-end',
  },
  modalContent: {
    backgroundColor: COLORS.textLight,
    borderTopLeftRadius: 16,
    borderTopRightRadius: 16,
    maxHeight: '85%',
    ...Platform.select({
      ios: {
        shadowColor: '#000',
        shadowOpacity: 0.15,
        shadowRadius: 10,
        shadowOffset: { width: 0, height: -4 },
      },
      android: {
        elevation: 10,
      },
    }),
  },
  modalTitle: {
    fontSize: 18,
    fontWeight: '700',
    color: COLORS.textDark,
    textAlign: 'center',
    marginTop: 15,
  },
  titleText: {
    fontSize: 14,
    color: COLORS.textDark,
    margin: 10,
    textAlign: 'center',
  },
  loadingContainer: {
    alignItems: 'center',
    marginVertical: 20,
  },
  loadingText: {
    marginTop: 10,
    fontSize: 14,
    color: COLORS.textDark,
  },
  errorContainer: {
    alignItems: 'center',
    marginVertical: 20,
  },
  errorText: {
    fontSize: 14,
    color: COLORS.textDark,
    marginBottom: 10,
    textAlign: 'center',
  },
  retryButton: {
    backgroundColor: COLORS.primary,
    borderRadius: 6,
    paddingHorizontal: 16,
    paddingVertical: 8,
  },
  retryButtonText: {
    color: '#fff',
    fontWeight: '600',
  },
  emptyStateContainer: {
    alignItems: 'center',
    marginVertical: 20,
  },
  emptyStateText: {
    fontSize: 14,
    color: COLORS.textDark,
    textAlign: 'center',
  },
  scrollArea: {
  },
  gridContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'center',
  },
  actionbuttons: {
    marginHorizontal: 17,
    marginBottom: 17,
    marginTop: 5,
  },
});
