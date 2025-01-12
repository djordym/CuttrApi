import React, { useState, useMemo, useCallback, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ActivityIndicator,
  TouchableOpacity,
  Alert,
  Platform,
  ScrollView,
} from 'react-native';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { MaterialIcons, Ionicons } from '@expo/vector-icons';
import { LinearGradient } from 'expo-linear-gradient';

import { useLikablePlants } from '../hooks/useSwipe';
import { useMyPlants } from '../hooks/usePlants';
import { useUserPreferences } from '../hooks/usePreferences';

import { SwipeableCard } from '../components/SwipeableCard';
import { SelectPlantsModal } from '../components/SelectPlantsModal';
import { EditUserPreferencesModal } from '../components/EditUserPreferencesModal';

import { swipeService } from '../../../api/swipeService';
import {
  SwipeRequest,
  PlantResponse,
  UserPreferencesResponse,
} from '../../../types/apiTypes';

/**
 * Convert the entire UserPreferencesResponse into a list of "chips," each containing:
 * - the raw string (e.g. "Herbs", "Indoor", etc.)
 * - the preference array name it belongs to (e.g. "preferedPlantCategory")
 *
 * This is so we can display them and remove them if the user taps the “X.”
 */
type FilterChipData = {
  id: string;             // Unique ID, e.g. preferedPlantCategory:Herbs
  text: string;           // The actual string, e.g. "Herbs"
  arrayName:
    | 'preferedPlantStage'
    | 'preferedPlantCategory'
    | 'preferedWateringNeed'
    | 'preferedLightRequirement'
    | 'preferedSize'
    | 'preferedIndoorOutdoor'
    | 'preferedPropagationEase'
    | 'preferedPetFriendly'
    | 'preferedExtras';
};

function buildActiveFilters(prefs: UserPreferencesResponse): FilterChipData[] {
  const chips: FilterChipData[] = [];

  const pushItems = (
    arr: string[] | undefined,
    arrayName: FilterChipData['arrayName']
  ) => {
    if (!arr) return;
    arr.forEach((item) => {
      // create a stable ID
      chips.push({
        id: `${arrayName}:${item}`,
        text: item,
        arrayName,
      });
    });
  };

  pushItems(prefs.preferedPlantStage, 'preferedPlantStage');
  pushItems(prefs.preferedPlantCategory, 'preferedPlantCategory');
  pushItems(prefs.preferedWateringNeed, 'preferedWateringNeed');
  pushItems(prefs.preferedLightRequirement, 'preferedLightRequirement');
  pushItems(prefs.preferedSize, 'preferedSize');
  pushItems(prefs.preferedIndoorOutdoor, 'preferedIndoorOutdoor');
  pushItems(prefs.preferedPropagationEase, 'preferedPropagationEase');
  pushItems(prefs.preferedPetFriendly, 'preferedPetFriendly');
  pushItems(prefs.preferedExtras, 'preferedExtras');

  return chips;
}

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

export const SwipeScreen: React.FC = () => {
  // -------------------------------------------------------------------------
  // QUERIES: Fetch plants, user plants, and user preferences
  // -------------------------------------------------------------------------
  const { data: plants, isLoading, isError, refetch } = useLikablePlants();
  const {
    data: myPlants,
    isLoading: loadingMyPlants,
    isError: errorMyPlants,
    refetch: refetchMyPlants,
  } = useMyPlants();
  const {
    data: userPreferences,
    isLoading: prefLoading,
    isError: prefError,
    refetch: refetchPreferences,
    updatePreferences,
  } = useUserPreferences();

  // -------------------------------------------------------------------------
  // LOCAL STATE
  // -------------------------------------------------------------------------
  const [localPlants, setLocalPlants] = useState<PlantResponse[]>(plants || []);
  const [modalVisible, setModalVisible] = useState(false);
  const [pendingRightSwipePlant, setPendingRightSwipePlant] =
    useState<PlantResponse | null>(null);

  // For opening/closing the "EditUserPreferencesModal"
  const [showPreferencesModal, setShowPreferencesModal] = useState(false);

  // -------------------------------------------------------------------------
  // EFFECTS
  // -------------------------------------------------------------------------
  // Keep local plants synced with server data
  useEffect(() => {
    if (plants) {
      setLocalPlants(plants);
    }
  }, [plants]);

  // -------------------------------------------------------------------------
  // BUILD "ACTIVE FILTER" CHIPS
  // -------------------------------------------------------------------------
  const activeFilterChips = useMemo(() => {
    if (!userPreferences) return [];
    return buildActiveFilters(userPreferences);
  }, [userPreferences]);

  // -------------------------------------------------------------------------
  // CARD SWIPE LOGIC
  // -------------------------------------------------------------------------
  const removeTopCard = (plantId: number) => {
    setLocalPlants((prev) => prev.filter((p) => p.plantId !== plantId));
  };

  const handleSwipeLeft = useCallback(
    async (plantId: number) => {
      if (!myPlants) {
        Alert.alert(
          'Error',
          'Add some plants or cuttings if you want to start trading.'
        );
        return;
      }
      const swipeRequests: SwipeRequest[] = myPlants.map((userPlant) => ({
        swiperPlantId: userPlant.plantId,
        swipedPlantId: plantId,
        isLike: false,
      }));
      try {
        await swipeService.sendSwipes(swipeRequests);
      } catch (err) {
        console.error(err);
        Alert.alert('Error', 'Failed to register swipe.');
      }
      removeTopCard(plantId);
    },
    [myPlants]
  );

  const handleRightSwipeInitiation = useCallback(
    (plantId: number) => {
      if (!myPlants) {
        Alert.alert(
          'Error',
          'Add some plants or cuttings if you want to start trading.'
        );
        return;
      }
      const plantToLike = localPlants.find((p) => p.plantId === plantId);
      if (!plantToLike) return;
      setPendingRightSwipePlant(plantToLike);
      setModalVisible(true);
    },
    [localPlants, myPlants]
  );

  const handleRightSwipeConfirm = useCallback(
    async (selectedPlantIds: number[]) => {
      if (!pendingRightSwipePlant || !myPlants) return;

      const plantId = pendingRightSwipePlant.plantId;
      const swipeRequests: SwipeRequest[] = myPlants.map((userPlant) => ({
        swiperPlantId: userPlant.plantId,
        swipedPlantId: plantId,
        isLike: selectedPlantIds.includes(userPlant.plantId),
      }));

      try {
        await swipeService.sendSwipes(swipeRequests);
      } catch (err) {
        console.error(err);
        Alert.alert('Error', 'Failed to send swipe requests.');
        return;
      }
      removeTopCard(plantId);
      setPendingRightSwipePlant(null);
      setModalVisible(false);
    },
    [myPlants, pendingRightSwipePlant]
  );

  const handleModalClose = () => {
    setPendingRightSwipePlant(null);
    setModalVisible(false);
  };

  const topCard = useMemo(() => {
    return localPlants.length > 0 ? localPlants[0] : null;
  }, [localPlants]);

  const handlePassPress = () => {
    if (topCard) {
      handleSwipeLeft(topCard.plantId);
    }
  };

  const handleLikePress = () => {
    if (topCard) {
      handleRightSwipeInitiation(topCard.plantId);
    }
  };

  // -------------------------------------------------------------------------
  // REMOVING A FILTER DIRECTLY FROM A CHIP
  // -------------------------------------------------------------------------
  const handleRemoveFilter = (chip: FilterChipData) => {
    if (!userPreferences) return;

    // We create a new copy, then remove the text from that array
    const newPrefs = { ...userPreferences };

    switch (chip.arrayName) {
      case 'preferedPlantStage': {
        newPrefs.preferedPlantStage = newPrefs.preferedPlantStage?.filter(
          (val) => val !== chip.text
        );
        break;
      }
      case 'preferedPlantCategory': {
        newPrefs.preferedPlantCategory = newPrefs.preferedPlantCategory?.filter(
          (val) => val !== chip.text
        );
        break;
      }
      case 'preferedWateringNeed': {
        newPrefs.preferedWateringNeed = newPrefs.preferedWateringNeed?.filter(
          (val) => val !== chip.text
        );
        break;
      }
      case 'preferedLightRequirement': {
        newPrefs.preferedLightRequirement =
          newPrefs.preferedLightRequirement?.filter(
            (val) => val !== chip.text
          );
        break;
      }
      case 'preferedSize': {
        newPrefs.preferedSize = newPrefs.preferedSize?.filter(
          (val) => val !== chip.text
        );
        break;
      }
      case 'preferedIndoorOutdoor': {
        newPrefs.preferedIndoorOutdoor =
          newPrefs.preferedIndoorOutdoor?.filter((val) => val !== chip.text);
        break;
      }
      case 'preferedPropagationEase': {
        newPrefs.preferedPropagationEase =
          newPrefs.preferedPropagationEase?.filter((val) => val !== chip.text);
        break;
      }
      case 'preferedPetFriendly': {
        newPrefs.preferedPetFriendly = newPrefs.preferedPetFriendly?.filter(
          (val) => val !== chip.text
        );
        break;
      }
      case 'preferedExtras': {
        newPrefs.preferedExtras = newPrefs.preferedExtras?.filter(
          (val) => val !== chip.text
        );
        break;
      }
      default:
        break;
    }

    updatePreferences(newPrefs); // triggers refetch on success
  };

  // -------------------------------------------------------------------------
  // OPEN/CLOSE THE MODAL FOR EDITING FILTERS
  // -------------------------------------------------------------------------
  const handleFilterPress = () => {
    setShowPreferencesModal(true);
  };

  // -------------------------------------------------------------------------
  // ERROR/LOADING STATES
  // -------------------------------------------------------------------------
  const anyLoading = isLoading || loadingMyPlants || prefLoading;
  const anyError = isError || errorMyPlants || prefError;

  if (anyLoading) {
    return (
      <SafeAreaProvider style={styles.centerContainer}>
        <ActivityIndicator size="large" color={COLORS.primary} />
        <Text style={styles.loadingText}>Loading Plants & Filters...</Text>
      </SafeAreaProvider>
    );
  }

  if (anyError) {
    return (
      <SafeAreaProvider style={styles.centerContainer}>
        <Text style={styles.errorText}>
          Failed to load plants or preferences.
        </Text>
        <TouchableOpacity
          onPress={() => {
            refetch();
            refetchMyPlants();
            refetchPreferences();
          }}
          style={styles.retryButton}
        >
          <Text style={styles.retryButtonText}>Try Again</Text>
        </TouchableOpacity>
      </SafeAreaProvider>
    );
  }

  // -------------------------------------------------------------------------
  // MAIN RENDER
  // -------------------------------------------------------------------------
  return (
    <SafeAreaProvider style={styles.container}>
      {/* Top Header with gradient */}
      <LinearGradient
        colors={[COLORS.primary, COLORS.primaryLight]}
        style={styles.headerGradient}
      >
        {/* Header Row */}
        <View style={styles.headerTopRow}>
          <Text style={styles.headerTitle}>Explore</Text>
          <TouchableOpacity
            onPress={handleFilterPress} // open filters modal
            style={styles.headerActionButton}
            accessible
            accessibilityLabel="Filter plants"
            accessibilityHint="Opens filter options"
          >
            <Ionicons name="options" size={24} color={COLORS.textLight} />
          </TouchableOpacity>
        </View>

        {/* Active Filters Section */}
        <View style={styles.activeFiltersSection}>
          <Text style={styles.activeFiltersLabel}>Active Filters:</Text>
          {activeFilterChips.length > 0 ? (
            <ScrollView
              horizontal
              showsHorizontalScrollIndicator={false}
              style={styles.filterChipsScroll}
              contentContainerStyle={{ paddingHorizontal: 4 }}
            >
              {activeFilterChips.map((chip) => (
                <View style={styles.filterChip} key={chip.id}>
                  <Text style={styles.filterChipText}>{chip.text}</Text>
                  <TouchableOpacity
                    style={styles.removeFilterButton}
                    onPress={() => handleRemoveFilter(chip)}
                  >
                    <Ionicons
                      name="close-circle"
                      size={18}
                      color="#fff"
                      style={{ marginLeft: 6 }}
                    />
                  </TouchableOpacity>
                </View>
              ))}
            </ScrollView>
          ) : (
            <Text style={styles.noFiltersText}>No filters selected</Text>
          )}
        </View>
      </LinearGradient>

      {/* Cards Container */}
      <View style={styles.cardsContainer}>
        {localPlants.map((plant) => (
          <SwipeableCard
            key={plant.plantId}
            plant={plant}
            onSwipeLeft={handleSwipeLeft}
            onSwipeRight={handleRightSwipeInitiation}
          />
        ))}

        {localPlants.length === 0 && (
          <View style={styles.emptyState}>
            <Text style={styles.emptyStateText}>No more plants to show.</Text>
            <Text style={styles.emptyStateSubText}>
              Try adjusting your filters or come back later.
            </Text>
          </View>
        )}
      </View>

      {/* Bottom Action Card (Pass / Like Buttons) */}
      {localPlants.length > 0 && (
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

          <TouchableOpacity
            onPress={handleLikePress}
            style={styles.actionButtonWrapper}
            accessibilityRole="button"
            accessibilityLabel="Like this plant"
            accessibilityHint="Show interest and match with this plant"
          >
            <LinearGradient
              colors={[COLORS.primary, COLORS.primaryLight]}
              style={styles.actionButton}
            >
              <MaterialIcons name="favorite" size={32} color={COLORS.textLight} />
            </LinearGradient>
          </TouchableOpacity>
        </View>
      )}

      {/* Modal for selecting your own plants to offer */}
      {myPlants && pendingRightSwipePlant && (
        <SelectPlantsModal
          visible={modalVisible}
          onClose={handleModalClose}
          onConfirm={handleRightSwipeConfirm}
        />
      )}

      {/* Modal to edit user preferences/filters */}
      <EditUserPreferencesModal
        visible={showPreferencesModal}
        onClose={() => setShowPreferencesModal(false)}
      />
    </SafeAreaProvider>
  );
};

export default SwipeScreen;

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
  centerContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 20,
  },
  loadingText: {
    marginTop: 10,
    fontSize: 16,
    color: COLORS.textDark,
  },
  errorText: {
    fontSize: 16,
    color: COLORS.textDark,
    marginBottom: 20,
    textAlign: 'center',
  },
  retryButton: {
    backgroundColor: COLORS.primary,
    paddingVertical: 12,
    paddingHorizontal: 20,
    borderRadius: 8,
  },
  retryButtonText: {
    color: COLORS.textLight,
    fontSize: 16,
    fontWeight: '600',
  },

  // HEADER
  headerGradient: {
    paddingHorizontal: 20,
    paddingTop: 15,
    paddingBottom: 20,
    borderBottomLeftRadius: 20,
    borderBottomRightRadius: 20,
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
    padding: 8,
  },

  // ACTIVE FILTERS
  activeFiltersSection: {
    marginTop: 8,
  },
  activeFiltersLabel: {
    color: COLORS.textLight,
    fontSize: 14,
    fontWeight: '700',
    marginBottom: 4,
  },
  filterChipsScroll: {
    maxHeight: 40,
    marginTop: 2,
  },
  filterChip: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: 'rgba(255,255,255,0.2)',
    marginRight: 8,
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 16,
  },
  filterChipText: {
    color: '#fff',
    fontSize: 14,
    fontWeight: '600',
  },
  removeFilterButton: {
    marginLeft: 4,
    padding: 0,
  },
  noFiltersText: {
    color: '#fff',
    fontStyle: 'italic',
    fontSize: 13,
  },

  // CARDS
  cardsContainer: {
    flex: 1,
  },

  // EMPTY STATE
  emptyState: {
    alignItems: 'center',
    padding: 20,
  },
  emptyStateText: {
    fontSize: 20,
    fontWeight: '700',
    color: COLORS.textDark,
    marginBottom: 8,
    textAlign: 'center',
  },
  emptyStateSubText: {
    fontSize: 16,
    color: '#555',
    textAlign: 'center',
    paddingHorizontal: 10,
  },

  // BOTTOM ACTION CONTAINER
  bottomActionContainer: {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    backgroundColor: COLORS.textLight,
    borderRadius: 40,
    margin: 20,
    paddingHorizontal: 40,
    paddingTop: 20,
    paddingBottom: 30,
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
  actionButtonWrapper: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  actionButton: {
    width: 64,
    height: 64,
    borderRadius: 32,
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
