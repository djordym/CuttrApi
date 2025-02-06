// File: app/features/main/screens/MakeTradeProposalScreen.tsx
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
} from 'react-native';
import { useNavigation, useRoute } from '@react-navigation/native';
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
import InfoModal from '../modals/InfoModal';
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

  const { mutate: createTradeProposal, isLoading: creatingProposal } =
    useCreateTradeProposal(connectionId);

  const [selectedOtherPlantIds, setSelectedOtherPlantIds] = useState<number[]>([]);
  const [selectedMyPlantIds, setSelectedMyPlantIds] = useState<number[]>([]);
  const [plantInfo, setPlantInfo] = useState<PlantResponse | null>(null);

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
              <PlantThumbnail
                key={plant.plantId}
                plant={plant}
                isSelected={isSelected}
                selectable
                onPress={() => onToggle(plant.plantId)}
                onInfoPress={() => setPlantInfo(plant)}
              />
            );
          })}
        </ScrollView>
      </View>
    );
  };

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
        <TouchableOpacity style={styles.closeButton} onPress={() => navigation.goBack()}>
          <Ionicons name="close" size={24} color="#fff" />
        </TouchableOpacity>
      </View>
    );
  }

  return (
    <View style={styles.modalBackground}>
      <LinearGradient style={styles.modalContainer} colors={[COLORS.primary, COLORS.secondary]}>
        <TouchableOpacity style={styles.closeButton} onPress={() => navigation.goBack()}>
          <Ionicons name="close" size={24} color="#fff" />
        </TouchableOpacity>

        {renderHorizontalSection("They’re Offering?", othersPlantsILiked, selectedOtherPlantIds, toggleOtherPlantSelection)}

        {/* Enhanced Divider with a Cool Gradient Trade Button */}
        <View style={styles.tradeDividerContainer}>
          <View style={styles.dividerLine} />
          <TouchableOpacity style={styles.tradeButtonWrapper} onPress={handleTrade}>
            <LinearGradient colors={['#ff8c00', '#ff4500']} style={styles.tradeButton}>
              {creatingProposal ? (
                <ActivityIndicator size="small" color="#fff" />
              ) : (
                <View style={styles.tradeButtonContent}>
                  <Ionicons name="swap-horizontal" size={20} color="#fff" />
                  <Text style={styles.tradeButtonText}> TRADE</Text>
                </View>
              )}
            </LinearGradient>
          </TouchableOpacity>
          <View style={styles.dividerLine} />
        </View>

        {renderHorizontalSection("You’re Offering?", myPlantsTheyLiked, selectedMyPlantIds, toggleMyPlantSelection)}
      </LinearGradient>

      {/* Reusable InfoModal */}
      <InfoModal visible={!!plantInfo} onClose={() => setPlantInfo(null)}>
        {plantInfo && <PlantCardWithInfo plant={plantInfo} compact={false} />}
      </InfoModal>
    </View>
  );
};

export default MakeTradeProposalScreen;

const styles = StyleSheet.create({
  modalBackground: {
    flex: 1,
    backgroundColor: 'rgba(0, 0, 0, 0.5)',
    // Center content vertically and horizontally
    justifyContent: 'center',
    alignItems: 'center',
  },
  modalContainer: {
    margin: 20,
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
  // New Divider and Trade Button Styles
  tradeDividerContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginVertical: 20,
    width: '100%',
  },
  dividerLine: {
    flex: 1,
    height: 1,
    backgroundColor: '#fff',
    opacity: 0.7,
  },
  tradeButtonWrapper: {
    marginHorizontal: 10,
  },
  tradeButton: {
    paddingVertical: 10,
    paddingHorizontal: 20,
    borderRadius: 25,
    shadowColor: '#000',
    shadowOpacity: 0.3,
    shadowRadius: 4,
    shadowOffset: { width: 0, height: 2 },
    elevation: 5,
  },
  tradeButtonContent: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  tradeButtonText: {
    color: '#fff',
    fontWeight: '700',
    fontSize: 16,
    marginLeft: 5,
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
});
