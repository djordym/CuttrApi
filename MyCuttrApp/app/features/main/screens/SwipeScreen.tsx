// File: SwipeScreen.tsx

import React, { useCallback, useEffect, useState } from 'react';
import {
  StyleSheet,
  View,
  Text,
  TouchableOpacity,
  ActivityIndicator,
  Alert,
  SafeAreaView,
  FlatList,
  Dimensions,
  Platform,
} from 'react-native';
import { Ionicons, MaterialIcons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { LinearGradient } from 'expo-linear-gradient';

// --- Hooks ---
import { useLikablePlants } from '../hooks/useSwipe';
import { useUserProfile } from '../hooks/useUser';
import { useUserPreferences } from '../hooks/usePreferences';
import { useMyPlants } from '../hooks/usePlants';

// --- Components & Services ---
import { SwipeableCard } from '../components/SwipeableCard';
import { SelectPlantsModal } from '../components/SelectPlantsModal';
import { swipeService } from '../../../api/swipeService';

// --- Types ---
import { PlantResponse, SwipeRequest } from '../../../types/apiTypes';

// Screen dimensions for layout references
const { width } = Dimensions.get('window');

// Feel free to tweak or unify these colors with your own theme.
const COLORS = {
  primary: '#1EAE98',
  primaryLight: '#5EE2C6',
  accent: '#FF6B6B',
  accentLight: '#FF9F9F',
  background: '#f8f8f8',
  textDark: '#333',
  textLight: '#fff',
  border: '#ddd',
};

interface SwipeScreenProps {}

const SwipeScreen: React.FC<SwipeScreenProps> = () => {
  const navigation = useNavigation();

  // ----- Data Fetching Hooks -----
  const {
    data: likablePlants,
    isLoading: loadingPlants,
    isError: errorPlants,
    refetch: refetchLikablePlants,
  } = useLikablePlants();

  const { data: userProfile } = useUserProfile();
  const {
    data: userPreferences,
    updatePreferences,
    isUpdating: updatingPrefs,
  } = useUserPreferences();

  const {
    data: myPlants,
    isLoading: loadingMyPlants,
    isError: errorMyPlants,
    refetch: refetchMyPlants,
  } = useMyPlants();

  // ----- Local State -----
  const [plantStack, setPlantStack] = useState<PlantResponse[]>([]);
  const [showSelectModal, setShowSelectModal] = useState(false);
  const [plantToLike, setPlantToLike] = useState<PlantResponse | null>(null);

  // Initialize plant stack when likablePlants change
  useEffect(() => {
    if (likablePlants) {
      setPlantStack(likablePlants);
    }
  }, [likablePlants]);

  // ----- Navigation: filter button -----
  const handleFilterPress = useCallback(() => {
    navigation.navigate('SetUserPreferences' as never);
  }, [navigation]);

  // ----- Removing Single Preference Tag -----
  const handleRemoveSinglePreference = useCallback(
    async (tagKey: string, valueToRemove: string) => {
      if (!userPreferences) return;

      const updatedPrefs = { ...userPreferences };

      switch (tagKey) {
        case 'Stage':
          updatedPrefs.preferedPlantStage = userPreferences.preferedPlantStage.filter(
            (val) => val !== valueToRemove
          );
          break;
        case 'Category':
          updatedPrefs.preferedPlantCategory = userPreferences.preferedPlantCategory.filter(
            (val) => val !== valueToRemove
          );
          break;
        case 'Light':
          updatedPrefs.preferedLightRequirement = userPreferences.preferedLightRequirement.filter(
            (val) => val !== valueToRemove
          );
          break;
        case 'Water':
          updatedPrefs.preferedWateringNeed = userPreferences.preferedWateringNeed.filter(
            (val) => val !== valueToRemove
          );
          break;
        case 'Size':
          updatedPrefs.preferedSize = userPreferences.preferedSize.filter(
            (val) => val !== valueToRemove
          );
          break;
        case 'IndoorOutdoor':
          updatedPrefs.preferedIndoorOutdoor = userPreferences.preferedIndoorOutdoor.filter(
            (val) => val !== valueToRemove
          );
          break;
        case 'PropagationEase':
          updatedPrefs.preferedPropagationEase = userPreferences.preferedPropagationEase.filter(
            (val) => val !== valueToRemove
          );
          break;
        case 'PetFriendly':
          updatedPrefs.preferedPetFriendly = userPreferences.preferedPetFriendly.filter(
            (val) => val !== valueToRemove
          );
          break;
        case 'Extras':
          updatedPrefs.preferedExtras = userPreferences.preferedExtras.filter(
            (val) => val !== valueToRemove
          );
          break;
        default:
          break;
      }

      try {
        await updatePreferences(updatedPrefs);
      } catch (err) {
        Alert.alert('Error', 'Could not remove preference.');
      }
    },
    [userPreferences, updatePreferences]
  );

  // ----- SWIPE ACTIONS -----
  const handleSwipeLeft = (swipedPlantId: number) => {
    if (!myPlants) return;
    const requests: SwipeRequest[] = myPlants.map((myPlant) => ({
      swiperPlantId: myPlant.plantId,
      swipedPlantId,
      isLike: false,
    }));

    swipeService
      .sendSwipes(requests)
      .then(() => {
        // Remove the top card from the stack
        setPlantStack((prevStack) => prevStack.slice(1));
      })
      .catch(() => {
        Alert.alert('Error', 'Failed to send swipes.');
      });
  };

  const handleSwipeRight = (swipedPlantId: number) => {
    const foundPlant = plantStack.find((p) => p.plantId === swipedPlantId) ?? null;
    if (foundPlant) {
      setPlantToLike(foundPlant);
      setShowSelectModal(true);
    }
  };

  const handleSelectConfirm = (selectedMyPlantIds: number[]) => {
    if (!plantToLike || !myPlants) return;
    const selectedSet = new Set(selectedMyPlantIds);

    const requests: SwipeRequest[] = myPlants.map((mp) => ({
      swiperPlantId: mp.plantId,
      swipedPlantId: plantToLike.plantId,
      isLike: selectedSet.has(mp.plantId),
    }));

    swipeService
      .sendSwipes(requests)
      .then(() => {
        setShowSelectModal(false);
        setPlantToLike(null);
        // Remove the top card after successful like
        setPlantStack((prevStack) => prevStack.slice(1));
      })
      .catch(() => {
        Alert.alert('Error', 'Failed to send swipes.');
      });
  };

  const handleSelectCancel = () => {
    setShowSelectModal(false);
    setPlantToLike(null);
  };

  const handlePassPress = () => {
    const topCard = plantStack[0];
    if (topCard) {
      handleSwipeLeft(topCard.plantId);
    }
  };

  const handleLikePress = () => {
    const topCard = plantStack[0];
    if (topCard) {
      handleSwipeRight(topCard.plantId);
    }
  };

  // ----- RENDERING THE HEADER WITH FILTER TAGS -----
  const renderHeader = () => {
    const prefTags: Array<{ key: string; value: string }> = [];

    if (userPreferences) {
      userPreferences.preferedPlantStage?.forEach((val) =>
        prefTags.push({ key: 'Stage', value: val })
      );
      userPreferences.preferedPlantCategory?.forEach((val) =>
        prefTags.push({ key: 'Category', value: val })
      );
      userPreferences.preferedLightRequirement?.forEach((val) =>
        prefTags.push({ key: 'Light', value: val })
      );
      userPreferences.preferedWateringNeed?.forEach((val) =>
        prefTags.push({ key: 'Water', value: val })
      );
      userPreferences.preferedSize?.forEach((val) =>
        prefTags.push({ key: 'Size', value: val })
      );
      userPreferences.preferedIndoorOutdoor?.forEach((val) =>
        prefTags.push({ key: 'IndoorOutdoor', value: val })
      );
      userPreferences.preferedPropagationEase?.forEach((val) =>
        prefTags.push({ key: 'PropagationEase', value: val })
      );
      userPreferences.preferedPetFriendly?.forEach((val) =>
        prefTags.push({ key: 'PetFriendly', value: val })
      );
      userPreferences.preferedExtras?.forEach((val) =>
        prefTags.push({ key: 'Extras', value: val })
      );
    }

    return (
      <LinearGradient
        colors={[COLORS.primary, COLORS.primaryLight]}
        style={styles.headerGradient}
      >
        <View style={styles.headerTopRow}>
          <Text style={styles.headerTitle}>Cuttr</Text>
          <TouchableOpacity
            onPress={handleFilterPress}
            style={styles.headerActionButton}
            accessible
            accessibilityLabel="Filter plants"
            accessibilityHint="Opens filter options"
          >
            <Ionicons name="options" size={24} color={COLORS.textLight} />
          </TouchableOpacity>
        </View>
        <View style={styles.filterContainer}>
        <View style={styles.filterInfoContainer}>
          {prefTags.length > 0 ? (
            <Text style={styles.filterInfoText}>Filters:</Text>
          ) : (
            <Text style={styles.noFilterText}>No filters applied</Text>
          )}
        </View>
        {prefTags.length > 0 && (
          <View style={styles.filterRow}>
            <FlatList
              data={prefTags}
              keyExtractor={(item, index) =>
                `${item.key}-${item.value}-${index}`
              }
              horizontal
              showsHorizontalScrollIndicator={false}
              renderItem={({ item }) => (
                <View style={styles.tagChip}>
                  <Text style={styles.tagChipText}>{item.value}</Text>
                  <TouchableOpacity
                    style={styles.removeTagButton}
                    onPress={() =>
                      handleRemoveSinglePreference(item.key, item.value)
                    }
                  >
                    <Ionicons name="close-circle" size={16} color="#fff" />
                  </TouchableOpacity>
                </View>
              )}
            />
          </View>
        )}
        </View>
      </LinearGradient>
    );
  };

  // ----- MAIN CARD STACK -----
  const renderCardStack = () => {
    if (loadingPlants || loadingMyPlants) {
      return (
        <View style={styles.loaderContainer}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={styles.loaderText}>Loading plants...</Text>
        </View>
      );
    }

    if (errorPlants || errorMyPlants) {
      return (
        <View style={styles.noPlantsContainer}>
          <Text style={styles.noPlantsText}>
            Failed to load plants or your gallery.
          </Text>
          <TouchableOpacity
            style={styles.reloadButton}
            onPress={() => {
              refetchLikablePlants();
              refetchMyPlants();
            }}
          >
            <Text style={styles.reloadButtonText}>Try Again</Text>
          </TouchableOpacity>
        </View>
      );
    }

    if (!plantStack || plantStack.length === 0) {
      return (
        <View style={styles.noPlantsContainer}>
          <Text style={styles.noPlantsText}>
            No more plants to show in your area.
          </Text>
          <TouchableOpacity
            style={styles.reloadButton}
            onPress={() => refetchLikablePlants()}
          >
            <Text style={styles.reloadButtonText}>Reload</Text>
          </TouchableOpacity>
        </View>
      );
    }

    // Show up to 3 cards for stacked effect
    const visibleCards = plantStack.slice(0, 3);
    return (
      <View style={styles.deckContainer}>
        {visibleCards.map((plant, index) => {
          // Calculate offset such that bottom-most card has the largest offset
          const offset = (visibleCards.length - 1 - index) * 5;
          // Only top card should handle swipe actions
          const isTop = index === 0;
          return (
            <View
              key={plant.plantId}
              style={[styles.cardWrapper, { top: -offset, right: offset }]}
            >
              <SwipeableCard
                plant={plant}
                onSwipeLeft={isTop ? handleSwipeLeft : undefined}
                onSwipeRight={isTop ? handleSwipeRight : undefined}
              />
            </View>
          );
        })}
      </View>
    );
  };

  return (
    <SafeAreaView style={styles.container}>
      {renderHeader()}
      {renderCardStack()}
      {plantStack && plantStack.length > 0 && (
        <View style={styles.bottomActionContainer}>
          <TouchableOpacity
            onPress={handlePassPress}
            style={styles.actionButtonWrapper}
            accessibilityRole="button"
            accessibilityLabel="Pass on this plant"
            accessibilityHint="Dislike and show next plant"
          >
            <LinearGradient
              colors={[COLORS.accent, COLORS.accentLight]}
              style={styles.actionButton}
            >
              <MaterialIcons name="close" size={32} color={COLORS.textLight} />
            </LinearGradient>
          </TouchableOpacity>
          <View style={styles.divider} />
          <TouchableOpacity
            onPress={handleLikePress}
            style={styles.actionButtonWrapper}
            accessibilityRole="button"
            accessibilityLabel="Like this plant"
            accessibilityHint="Show interest in this plant"
          >
            <LinearGradient
              colors={[COLORS.primary, COLORS.primaryLight]}
              style={styles.actionButton}
            >
              <MaterialIcons
                name="favorite"
                size={32}
                color={COLORS.textLight}
              />
            </LinearGradient>
          </TouchableOpacity>
        </View>
      )}
      <SelectPlantsModal
        visible={showSelectModal}
        onConfirm={handleSelectConfirm}
        onClose={handleSelectCancel}
      />
    </SafeAreaView>
  );
};

export default SwipeScreen;

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
  loaderContainer: {
    marginTop: 40,
    alignItems: 'center',
  },
  loaderText: {
    fontSize: 16,
    marginTop: 10,
    color: COLORS.textDark,
  },
  noPlantsContainer: {
    marginTop: 40,
    alignItems: 'center',
    paddingHorizontal: 20,
  },
  noPlantsText: {
    fontSize: 16,
    color: COLORS.textDark,
    marginBottom: 10,
    textAlign: 'center',
  },
  reloadButton: {
    backgroundColor: COLORS.primary,
    paddingVertical: 8,
    paddingHorizontal: 16,
    borderRadius: 8,
  },
  reloadButtonText: {
    color: '#fff',
    fontWeight: '600',
  },
  headerGradient: {
    paddingHorizontal: 16,
    paddingTop: 15,
    paddingBottom: 20,
    borderBottomLeftRadius: 20, // Smaller corner
    borderBottomRightRadius: 20, // Smaller corner
    marginBottom: 10,
  },
  headerTopRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  headerTitle: {
    fontSize: 26,
    fontWeight: '700',
    color: COLORS.textLight,
  },
  headerActionButton: {
    padding: 6,
  },
  filterContainer: {
    flexDirection: 'row',
  },
  filterInfoContainer: {
    marginTop: 10,
  },
  filterInfoText: {
    color: COLORS.textLight,
    fontSize: 14,
  },
  noFilterText: {
    color: COLORS.textLight,
    fontSize: 14,
  },
  filterRow: {
    marginTop: 10,
    marginLeft: 5,
    flexDirection: 'row',
    flex: 1,
  },
  tagChip: {
    flexDirection: 'row',
    backgroundColor: COLORS.accent,
    paddingVertical: 4,
    paddingHorizontal: 8,
    borderRadius: 16,
    marginRight: 8,
    alignItems: 'center',
  },
  tagChipText: {
    color: '#fff',
    marginRight: 4,
    fontSize: 12,
    fontWeight: '600',
  },
  removeTagButton: {
    paddingLeft: 2,
  },
  deckContainer: {
    marginTop: 15,
    flex: 1,
    justifyContent: 'flex-start',
    alignItems: 'center',
  },
  cardWrapper: {
    width: width * 0.9,
    },
  bottomActionContainer: {
    position: 'relative',
    backgroundColor: COLORS.textLight,
    borderRadius: 20,
    paddingHorizontal: 60,
    paddingTop: 20,
    paddingBottom: 20,
    margin: 20,
    marginHorizontal: 40,
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
    alignItems: 'center',
    justifyContent: 'space-between',
    flexDirection: 'row',
  },
  divider: {
    width: 1,
    height: '150%',
    backgroundColor: COLORS.border,
  },
  actionButtonWrapper: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  actionButton: {
    width: 50,
    height: 50,
    borderRadius: 25,
    alignItems: 'center',
    justifyContent: 'center',
    ...Platform.select({
      ios: {
        shadowColor: '#000',
        shadowOpacity: 0.25,
        shadowRadius: 6,
        shadowOffset: { width: 0, height: 3 },
      },
      android: {
        elevation: 6,
      },
    }),
  },
});
