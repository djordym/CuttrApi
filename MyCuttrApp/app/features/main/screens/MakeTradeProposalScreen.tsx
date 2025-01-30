import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ActivityIndicator,
  TouchableOpacity,
  FlatList,
  Alert,
  Modal,            // <-- Import Modal
  Image,
} from 'react-native';
import { useRoute, useNavigation } from '@react-navigation/native';
import { LinearGradient } from 'expo-linear-gradient';
import { Ionicons } from '@expo/vector-icons';

import { usePlantsLikedByMeFromUser, usePlantsLikedByUserFromMe } from '../hooks/usePlantHooks';
import { useCreateTradeProposal } from '../hooks/useTradeProposalHooks';
import { PlantResponse } from '../../../types/apiTypes';
import { PlantThumbnail } from '../components/PlantThumbnail';
import { PlantCardWithInfo } from '../components/PlantCardWithInfo';
import { COLORS } from '../../../theme/colors';

// Example route params for navigation
interface MakeTradeProposalRouteParams {
  visible: boolean;        // We control modal visibility externally
  onClose: () => void;     // Handler to close the modal
  connectionId: number;
  otherUserId: number;
}

const MakeTradeProposalScreen: React.FC = () => {
  // If you’re navigating with react-navigation, you can rely on route.params
  // to get `visible` and `onClose` if you want. Or use your own approach.
  const navigation = useNavigation();
  const route = useRoute();
  const {
    visible = true, // default to open if not passed
    onClose = () => navigation.goBack(),
    connectionId,
    otherUserId,
  } = route.params as MakeTradeProposalRouteParams;

  // Queries for "liked plants" in both directions
  const {
    data: othersPlantsILiked,
    isLoading: loadingOtherPlants,
    isError: errorOtherPlants,
  } = usePlantsLikedByMeFromUser(otherUserId);

  const {
    data: myPlantsTheyLiked,
    isLoading: loadingMyPlants,
    isError: errorMyPlants,
  } = usePlantsLikedByUserFromMe(otherUserId);

  // Mutation for creating the trade proposal
  const { mutate: createTradeProposal, isLoading: creatingProposal } =
    useCreateTradeProposal(connectionId);

  // State for selected plants
  const [selectedOtherPlantIds, setSelectedOtherPlantIds] = useState<number[]>([]);
  const [selectedMyPlantIds, setSelectedMyPlantIds] = useState<number[]>([]);

  // Toggle between a thumbnail or a "compact card" view
  const [useCompactView, setUseCompactView] = useState(false);

  // Toggling selection from other user’s plants
  const toggleOtherPlantSelection = (plantId: number) => {
    setSelectedOtherPlantIds((prev) =>
      prev.includes(plantId) ? prev.filter((id) => id !== plantId) : [...prev, plantId]
    );
  };

  // Toggling selection from my plants
  const toggleMyPlantSelection = (plantId: number) => {
    setSelectedMyPlantIds((prev) =>
      prev.includes(plantId) ? prev.filter((id) => id !== plantId) : [...prev, plantId]
    );
  };

  // Confirm trade
  const handleTrade = () => {
    if (selectedOtherPlantIds.length === 0 && selectedMyPlantIds.length === 0) {
      Alert.alert('Empty Trade', 'Select at least one plant to trade!');
      return;
    }

    const payload = {
      userPlantIds: selectedMyPlantIds,
      otherPlantIds: selectedOtherPlantIds,
    };

    createTradeProposal(payload, {
      onSuccess: () => {
        Alert.alert('Success', 'Trade proposal created!', [
          { text: 'OK', onPress: onClose },
        ]);
      },
      onError: (err) => {
        console.error('Failed to create proposal:', err);
        Alert.alert('Error', 'Could not create proposal. Try again.');
      },
    });
  };

  // Horizontal scroller with either thumbnails or compact cards
  const renderHorizontalSection = (
    label: string,
    data: PlantResponse[] | undefined,
    selectedIds: number[],
    onToggle: (id: number) => void
  ) => {
    if (!data) return null;

    return (
      <View style={styles.sectionWrapper}>
        <Text style={styles.sectionTitle}>{label}</Text>
        <FlatList
          data={data}
          keyExtractor={(item) => item.plantId.toString()}
          horizontal
          showsHorizontalScrollIndicator={false}
          contentContainerStyle={styles.horizontalScrollContent}
          renderItem={({ item }) => {
            const isSelected = selectedIds.includes(item.plantId);

            if (useCompactView) {
              return (
                <TouchableOpacity
                  onPress={() => onToggle(item.plantId)}
                  style={[styles.compactItemWrapper, isSelected && styles.selectedOutline]}
                >
                  <PlantCardWithInfo plant={item} compact={true} />
                </TouchableOpacity>
              );
            } else {
              return (
                <PlantThumbnail
                  plant={item}
                  isSelected={isSelected}
                  selectable
                  onPress={() => onToggle(item.plantId)}
                  containerStyle={styles.thumbnailContainer}
                />
              );
            }
          }}
        />
      </View>
    );
  };

  // Loading / Error states
  if (loadingOtherPlants || loadingMyPlants) {
    return (
      <Modal visible={visible} transparent animationType="fade" onRequestClose={onClose}>
        <View style={styles.modalOverlay}>
          <View style={styles.modalContent}>
            <ActivityIndicator size="large" color={COLORS.primary} />
            <Text style={styles.loadingText}>Loading liked plants...</Text>
          </View>
        </View>
      </Modal>
    );
  }

  if (errorOtherPlants || errorMyPlants) {
    return (
      <Modal visible={visible} transparent animationType="slide" onRequestClose={onClose}>
        <View style={styles.modalOverlay}>
          <View style={styles.modalContent}>
            <Text style={styles.errorText}>Couldn’t load plants. Please retry.</Text>
            <TouchableOpacity style={styles.retryButton} onPress={() => {}}>
              <Text style={styles.retryButtonText}>Retry</Text>
            </TouchableOpacity>

            <TouchableOpacity style={[styles.closeButton, { marginTop: 20 }]} onPress={onClose}>
              <Ionicons name="close" size={24} color="#fff" />
            </TouchableOpacity>
          </View>
        </View>
      </Modal>
    );
  }

  // Main content modal
  return (
    <Modal visible={visible} transparent animationType="slide" onRequestClose={onClose}>
      <LinearGradient
        colors={[COLORS.primary, COLORS.secondary]}  // Example fun gradient
        style={styles.modalOverlay}
      >
        {/* The container that holds our “modal content” but in a “gamified” style */}
        <View style={styles.gamifiedContainer}>
          {/* Close (X) button in top-right corner */}
          <TouchableOpacity style={styles.closeButton} onPress={onClose}>
            <Ionicons name="close" size={24} color="#fff" />
          </TouchableOpacity>

          {/* Toggle between view modes */}
          <TouchableOpacity
            style={styles.viewToggleButton}
            onPress={() => setUseCompactView((prev) => !prev)}
          >
            <Text style={styles.viewToggleButtonText}>
              {useCompactView ? 'Show Thumbnails' : 'Show Compact'}
            </Text>
          </TouchableOpacity>

          {/* Top: Other user’s plants */}
          {renderHorizontalSection(
            "They're Offering?",
            othersPlantsILiked,
            selectedOtherPlantIds,
            toggleOtherPlantSelection
          )}

          {/* Divider with big “Trade” button in the center */}
          <View style={styles.tradeDividerContainer}>
            <View style={styles.dividerLine} />
            <TouchableOpacity style={styles.tradeButton} onPress={handleTrade}>
              {creatingProposal ? (
                <ActivityIndicator size="small" color="#fff" />
              ) : (
                <Text style={styles.tradeButtonText}>TRADE</Text>
              )}
            </TouchableOpacity>
          </View>

          {/* Bottom: My plants */}
          {renderHorizontalSection(
            'You’re Offering?',
            myPlantsTheyLiked,
            selectedMyPlantIds,
            toggleMyPlantSelection
          )}
        </View>
      </LinearGradient>
    </Modal>
  );
};

export default MakeTradeProposalScreen;

const styles = StyleSheet.create({
  modalOverlay: {
    flex: 1,
    justifyContent: 'center',
    // We use a gradient background for a fun effect
  },
  modalContent: {
    marginHorizontal: 20,
    backgroundColor: '#fff',
    borderRadius: 16,
    padding: 20,
    alignItems: 'center',
  },
  gamifiedContainer: {
    flex: 1,
    paddingTop: 50,
    paddingBottom: 30,
    paddingHorizontal: 10,
  },

  // Close button top-right
  closeButton: {
    position: 'absolute',
    top: 40,
    right: 20,
    backgroundColor: 'rgba(255,255,255,0.3)',
    borderRadius: 20,
    padding: 6,
  },

  // If loading or error:
  loadingText: {
    marginTop: 10,
    color: COLORS.textDark,
  },
  errorText: {
    fontSize: 16,
    color: COLORS.textDark,
    textAlign: 'center',
    marginBottom: 10,
  },
  retryButton: {
    backgroundColor: COLORS.primary,
    borderRadius: 8,
    paddingHorizontal: 16,
    paddingVertical: 10,
    marginTop: 10,
  },
  retryButtonText: {
    color: '#fff',
    fontWeight: '600',
  },

  // Toggle button between compact vs thumbnail
  viewToggleButton: {
    alignSelf: 'center',
    backgroundColor: 'rgba(255,255,255,0.3)',
    borderRadius: 16,
    paddingVertical: 8,
    paddingHorizontal: 12,
    marginBottom: 10,
  },
  viewToggleButtonText: {
    color: '#fff',
    fontWeight: '600',
  },

  // Each horizontal scroller
  sectionWrapper: {
    marginVertical: 10,
  },
  sectionTitle: {
    fontSize: 17,
    color: '#fff',
    fontWeight: '700',
    textAlign: 'center',
    marginBottom: 6,
  },
  horizontalScrollContent: {
    paddingHorizontal: 10,
  },
  thumbnailContainer: {
    marginRight: 12,
  },
  compactItemWrapper: {
    marginRight: 12,
    width: 200,
    borderRadius: 12,
    overflow: 'hidden',
  },
  selectedOutline: {
    borderWidth: 3,
    borderColor: '#00ffea',
  },

  // Divider & Trade button in the middle
  tradeDividerContainer: {
    width: '100%',
    height: 70,
    alignItems: 'center',
    justifyContent: 'center',
    marginVertical: 4,
    position: 'relative',
  },
  dividerLine: {
    position: 'absolute',
    top: '50%',
    left: 10,
    right: 10,
    height: 2,
    backgroundColor: '#fff',
    opacity: 0.7,
  },
  tradeButton: {
    width: 70,
    height: 70,
    borderRadius: 35,
    backgroundColor: '#ff0058',
    alignItems: 'center',
    justifyContent: 'center',
    zIndex: 2, // on top of line
    shadowColor: '#000',
    shadowOpacity: 0.3,
    shadowRadius: 4,
    shadowOffset: { width: 0, height: 2 },
    elevation: 5,
  },
  tradeButtonText: {
    color: '#fff',
    fontWeight: '700',
    fontSize: 16,
  },
});
