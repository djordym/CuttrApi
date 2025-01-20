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
import { useMyPlants } from '../hooks/usePlants'; // Adjust import path if necessary
import { PlantResponse } from '../../../types/apiTypes';
import { t } from 'i18next';

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

/** Adapted color palette (same references as your other screens). */
const COLORS = {
  primary: '#1EAE98',
  primaryLight: '#5EE2C6',
  accent: '#FF6B6B',
  accentLight: '#FF9F9F',
  cardBg: '#FFFFFF',
  background: '#f8f8f8',
  textDark: '#333',
  textLight: '#fff',
  border: '#ddd',
};

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
          <Text style={styles.modalTitle}>Select Plants</Text>

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
                  <Text style={styles.titleText}>
                    Select the plants you want to trade for. Tap on a plant to select it.
                  </Text>
                  <View style={styles.gridContainer}>
                    {myPlants.map((plant: PlantResponse) => {
                      const isSelected = selectedPlantIds.includes(plant.plantId);
                      return (
                        <TouchableOpacity
                          key={plant.plantId}
                          onPress={() => handleToggleSelect(plant.plantId)}
                          activeOpacity={0.9}
                          style={[
                            styles.plantCardThumbnail,
                            isSelected && styles.plantCardThumbnailSelected,
                          ]}
                        >
                          {plant.imageUrl ? (
                            <Image
                              source={{ uri: plant.imageUrl }}
                              style={styles.thumbImage}
                              resizeMode="contain"
                            />
                          ) : (
                            <View style={styles.plantPlaceholder}>
                              <Ionicons name="leaf" size={40} color={COLORS.primary} />
                            </View>
                          )}
                          <View style={styles.thumbTextWrapper}>
                            <Text style={styles.thumbPlantName} numberOfLines={1}>
                              {plant.speciesName}
                            </Text>
                          </View>
                        </TouchableOpacity>
                      );
                    })}
                  </View>
                </ScrollView>
              )}
            </>
          )}

          <View style={styles.buttonRow}>
            <TouchableOpacity
              style={[styles.actionButton, styles.confirmButton]}
              onPress={handleConfirm}
              disabled={selectedPlantIds.length === 0}
            >
              <Text style={styles.confirmButtonText}>Confirm</Text>
            </TouchableOpacity>
            <TouchableOpacity
              style={[styles.actionButton, styles.cancelButton]}
              onPress={onClose}
            >
              <Text style={styles.cancelButtonText}>Cancel</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>
    </Modal>
  );
};

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
    paddingTop: 20,
    paddingBottom: 10,
    paddingHorizontal: 16,
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
    marginBottom: 12,
    textAlign: 'center',
  },
  titleText: {
    fontSize: 14,
    color: COLORS.textDark,
    marginBottom: 10,
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
    color: COLORS.accent,
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
    marginVertical: 10,
  },
  gridContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'center',
  },
  plantCardThumbnail: {
    width: (width - 70) / 3, // 3 items across, with some spacing
    backgroundColor: COLORS.cardBg,
    borderRadius: 8,
    marginBottom: 15,
    marginRight: 10,
    overflow: 'hidden',
    ...Platform.select({
      ios: {
        shadowColor: '#000',
        shadowOpacity: 0.1,
        shadowRadius: 5,
      },
      android: {
        elevation: 3,
      },
    }),
  },
  plantCardThumbnailSelected: {
    borderWidth: 2,
    borderColor: COLORS.primary,
  },
  thumbImage: {
    width: '100%',
    aspectRatio: 3 / 4,
  },
  plantPlaceholder: {
    width: '100%',
    height: 120,
    backgroundColor: '#eee',
    justifyContent: 'center',
    alignItems: 'center',
  },
  thumbTextWrapper: {
    padding: 8,
    alignItems: 'center',
  },
  thumbPlantName: {
    fontSize: 14,
    fontWeight: '600',
    color: COLORS.textDark,
  },
  buttonRow: {
    flexDirection: 'column',
    justifyContent: 'flex-end',
    marginTop: 8,
  },
  actionButton: {
    borderRadius: 8,
    paddingVertical: 10,
    paddingHorizontal: 16,
    margin: 4, 
  },
  cancelButton: {
    backgroundColor: '#fff',
    borderWidth: 1,
    borderColor: COLORS.primary,
    
  },
  cancelButtonText: {
    color: COLORS.primary,
    fontWeight: '600',
    textAlign: 'center',
  },
  confirmButton: {
    backgroundColor: COLORS.primary,
  },
  confirmButtonText: {
    color: '#fff',
    fontWeight: '600',
    textAlign: 'center',
  },
});
