// src/features/main/screens/MakeTradeProposalScreen.tsx

import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ActivityIndicator,
  TouchableOpacity,
  Alert,
  Dimensions,
  ScrollView,
  Modal,
  Pressable,
} from 'react-native';
import { useRoute, useNavigation } from '@react-navigation/native';
import { LinearGradient } from 'expo-linear-gradient';
import { Ionicons } from '@expo/vector-icons';

import {
  usePlantsLikedByMeFromUser,
  usePlantsLikedByUserFromMe,
} from '../hooks/usePlantHooks';
import { useCreateTradeProposal } from '../hooks/useTradeProposalHooks';
import { PlantResponse } from '../../../types/apiTypes';
import PlantThumbnail from '../components/PlantThumbnail';
import PlantCardWithInfo from '../components/PlantCardWithInfo';
import { COLORS } from '../../../theme/colors';

const { width, height } = Dimensions.get('window');

interface MakeTradeProposalRouteParams {
  connectionId: number;
  otherUserId: number;
}

const MakeTradeProposalScreen: React.FC = () => {
  const navigation = useNavigation();
  const route = useRoute();
  const { connectionId, otherUserId } = route.params as MakeTradeProposalRouteParams;

  // Fetching liked plants
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

  // Mutation to create the trade proposal
  const { mutate: createTradeProposal, isLoading: creatingProposal } =
    useCreateTradeProposal(connectionId);

  // State for selections
  const [selectedOtherPlantIds, setSelectedOtherPlantIds] = useState<number[]>([]);
  const [selectedMyPlantIds, setSelectedMyPlantIds] = useState<number[]>([]);

  // State to handle previewing a full plant card on long press
  const [previewPlant, setPreviewPlant] = useState<PlantResponse | null>(null);

  // Toggle selection functions
  const toggleOtherPlantSelection = (plantId: number) => {
    setSelectedOtherPlantIds((prev) =>
      prev.includes(plantId) ? prev.filter((id) => id !== plantId) : [...prev, plantId]
    );
  };

  const toggleMyPlantSelection = (plantId: number) => {
    setSelectedMyPlantIds((prev) =>
      prev.includes(plantId) ? prev.filter((id) => id !== plantId) : [...prev, plantId]
    );
  };

  // Handle Trade
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
          { text: 'OK', onPress: () => navigation.goBack() },
        ]);
      },
      onError: (err) => {
        console.error('Failed to create proposal:', err);
        Alert.alert('Error', 'Could not create proposal. Try again.');
      },
    });
  };

  // Function to render a horizontal scroll of thumbnails
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
        <ScrollView
          horizontal
          showsHorizontalScrollIndicator={false}
          contentContainerStyle={styles.horizontalScrollContent}
        >
          {data.map((plant) => {
            const isSelected = selectedIds.includes(plant.plantId);

            return (
              <Pressable
                key={plant.plantId}
                onLongPress={() => setPreviewPlant(plant)}
                // We'll hide preview on "pressOut"
                onPressOut={() => setPreviewPlant(null)}
                style={{ marginRight: 12 }}
              >
                <PlantThumbnail
                  plant={plant}
                  isSelected={isSelected}
                  selectable
                  onPress={() => onToggle(plant.plantId)}
                />
              </Pressable>
            );
          })}
        </ScrollView>
      </View>
    );
  };

  // Handle Loading and Error States
  if (loadingOtherPlants || loadingMyPlants) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color={COLORS.primary} />
        <Text style={styles.loadingText}>Loading liked plants...</Text>
      </View>
    );
  }

  if (errorOtherPlants || errorMyPlants) {
    return (
      <View style={styles.errorContainer}>
        <Text style={styles.errorText}>Couldn’t load plants. Please retry.</Text>
        <TouchableOpacity
          style={styles.closeButton}
          onPress={() => navigation.goBack()}
        >
          <Ionicons name="close" size={24} color="#fff" />
        </TouchableOpacity>
      </View>
    );
  }

  return (
    <View style={styles.modalBackground}>
      {/* Semi-transparent dark background */}
      <LinearGradient
        style={styles.modalContainer}
        colors={[COLORS.primary, COLORS.secondary]}
      >
        {/* Close (X) button */}
        <TouchableOpacity
          style={styles.closeButton}
          onPress={() => navigation.goBack()}
        >
          <Ionicons name="close" size={24} color="#fff" />
        </TouchableOpacity>

        {/* "They’re Offering?" */}
        {renderHorizontalSection(
          "They’re Offering?",
          othersPlantsILiked,
          selectedOtherPlantIds,
          toggleOtherPlantSelection
        )}

        {/* Trade Divider with trade button */}
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

        {/* "You’re Offering?" */}
        {renderHorizontalSection(
          "You’re Offering?",
          myPlantsTheyLiked,
          selectedMyPlantIds,
          toggleMyPlantSelection
        )}
      </LinearGradient>

      {/* Modal-like overlay for previewing the full card when long pressed */}
      <Modal visible={!!previewPlant} transparent animationType="fade">
        <View style={styles.previewOverlayContainer}>
          <View style={styles.previewCardWrapper}>
            {previewPlant && (
              <PlantCardWithInfo plant={previewPlant} compact={false} />
            )}
          </View>
        </View>
      </Modal>
    </View>
  );
};

export default MakeTradeProposalScreen;

// -------------------------------------------------------
// Styles
// -------------------------------------------------------
const styles = StyleSheet.create({
  modalBackground: {
    flex: 1,
    backgroundColor: 'rgba(0, 0, 0, 0.5)', // Semi-transparent overlay
    justifyContent: 'center',
    alignItems: 'center',
  },
  modalContainer: {
    width: width * 0.95,
    height: height * 0.97,
    borderRadius: 20,
    padding: 20,
    alignItems: 'center',
    position: 'relative',
    shadowColor: '#000',
    shadowOpacity: 0.25,
    shadowRadius: 10,
    shadowOffset: { width: 0, height: 4 },
    elevation: 10,
  },
  closeButton: {
    position: 'absolute',
    top: 15,
    right: 15,
    backgroundColor: 'rgba(255,255,255,0.3)',
    borderRadius: 20,
    padding: 6,
    zIndex: 5,
  },
  sectionWrapper: {
    width: '100%',
    marginVertical: 10,
  },
  sectionTitle: {
    fontSize: 18,
    color: '#fff',
    fontWeight: '700',
    textAlign: 'center',
    marginBottom: 8,
  },
  horizontalScrollContent: {
    paddingHorizontal: 10,
  },
  tradeDividerContainer: {
    width: '100%',
    height: 70,
    alignItems: 'center',
    justifyContent: 'center',
    marginVertical: 10,
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
    width: 100,
    height: 40,
    borderRadius: 20,
    backgroundColor: COLORS.primary,
    alignItems: 'center',
    justifyContent: 'center',
    zIndex: 2, // On top of the divider
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
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  loadingText: {
    marginTop: 10,
    color: '#fff',
    fontSize: 16,
  },
  errorContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: 20,
  },
  errorText: {
    fontSize: 16,
    color: '#fff',
    textAlign: 'center',
    marginBottom: 10,
  },

  // Preview overlay (long press)
  previewOverlayContainer: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.6)',
    justifyContent: 'center',
    alignItems: 'center',
    zIndex: 10,
  },
  previewCardWrapper: {
    width: width * 0.9,
    borderRadius: 12,
    overflow: 'hidden',
    zIndex: 10,
  },
});
