import React, { useCallback, useEffect, useRef, useState } from 'react';
import {
  StyleSheet,
  View,
  Text,
  TouchableOpacity,
  ActivityIndicator,
  Alert,
  Dimensions,
  Platform,
  FlatList,
} from 'react-native';
import { Ionicons, MaterialIcons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { LinearGradient } from 'expo-linear-gradient';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';

// --- Hooks ---
import { useLikablePlants } from '../hooks/useSwipe';
import { useMyProfile } from '../hooks/useMyProfileHooks';
import { useUserPreferences } from '../hooks/usePreferences';
import { useMyPlants } from '../hooks/usePlantHooks';

// --- Components & Services ---
import { SwipeableCard, SwipeableCardRef } from '../components/SwipeableCard';
import { SelectPlantsModal } from '../modals/SelectPlantsModal';
import ItsAMatchModal from '../modals/ItsAMatchModal';

// --- Types & Utils ---
import { PlantResponse, SwipeRequest } from '../../../types/apiTypes';
import { log } from '../../../utils/logger';
import { COLORS } from '../../../theme/colors';
import { headerStyles } from '../styles/headerStyles';

const { width } = Dimensions.get('window');

interface SwipeScreenProps {}

const SwipeScreen: React.FC<SwipeScreenProps> = () => {
  const { t } = useTranslation();
  const navigation = useNavigation();

  // Data hooks
  const {
    data: likablePlants,
    isLoading: loadingPlants,
    isError: errorPlants,
    refetch: refetchLikablePlants,
    sendSwipes,
    isSending: sendingSwipes,
    matches,
    clearMatches,
  } = useLikablePlants();

  const { data: userProfile } = useMyProfile();
  const {
    data: userPreferences,
    updatePreferences,
  } = useUserPreferences();

  const {
    data: myPlants,
    isLoading: loadingMyPlants,
    isError: errorMyPlants,
    refetch: refetchMyPlants,
  } = useMyPlants();

  // The main card stack
  const [plantStack, setPlantStack] = useState<PlantResponse[]>([]);

  // Modal / "like" flow
  const [showSelectModal, setShowSelectModal] = useState(false);
  const [plantToLike, setPlantToLike] = useState<PlantResponse | null>(null);

  // Reference to the top card so we can reset or finalize the right-swipe
  const topCardRef = useRef<SwipeableCardRef>(null);

  // Initialize stack on fetch
  useEffect(() => {
    if (likablePlants) {
      log.debug('Likable plants fetched:', likablePlants);
      setPlantStack(likablePlants);
    }
  }, [likablePlants]);

  // ----- Navigation: Filter Button -----
  const handleFilterPress = useCallback(() => {
    navigation.navigate('SetUserPreferences' as never);
  }, [navigation]);

  // ----- Single Preference Removal -----
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
        Alert.alert(
          t('swipeScreen.alert.error'),
          t('swipeScreen.error.removePreference')
        );
      }
    },
    [userPreferences, updatePreferences, t]
  );

  // ----- SWIPE ACTIONS -----

  /**
   * Dislike / Swipe Left: immediately removes the top card.
   */
  const handleDislike = (plantId: number) => {
    setPlantStack((prev) => prev.slice(1));
    if (!myPlants) return;
    const topCard = likablePlants?.find((p) => p.plantId === plantId);
    if (!topCard) return;

    const requests: SwipeRequest[] = myPlants.map((myPlant) => ({
      swiperPlantId: myPlant.plantId,
      swipedPlantId: topCard.plantId,
      isLike: false,
    }));

    sendSwipes(requests, {
      onError: () => {
        Alert.alert(
          t('swipeScreen.alert.error'),
          t('swipeScreen.error.sendSwipes')
        );
      },
    });
  };

  /**
   * Called after the card animates off screen.
   */
  const handleSwipeRight = (plantId: number) => {
    setPlantStack((prev) => prev.slice(1));
    // The like request is sent in handleSelectConfirm.
  };

  /**
   * Begins the like gesture by opening the select modal.
   */
  const handleLikeGestureBegin = (plant: PlantResponse) => {
    setPlantToLike(plant);
    setShowSelectModal(true);
  };

  /**
   * When the user confirms the selection, sends the like request.
   */
  const handleSelectConfirm = (selectedMyPlantIds: number[]) => {
    if (!plantToLike || !myPlants) return;
    const requests: SwipeRequest[] = myPlants.map((mp) => ({
      swiperPlantId: mp.plantId,
      swipedPlantId: plantToLike.plantId,
      isLike: selectedMyPlantIds.includes(mp.plantId),
    }));

    sendSwipes(requests, {
      onSuccess: () => {
        topCardRef.current?.flyOffRight();
      },
      onError: () => {
        Alert.alert(
          t('swipeScreen.alert.error'),
          t('swipeScreen.error.sendSwipes')
        );
      },
      onSettled: () => {
        setShowSelectModal(false);
        setPlantToLike(null);
      },
    });
  };

  /**
   * Cancels the selection and resets the card position.
   */
  const handleSelectCancel = () => {
    topCardRef.current?.resetPosition();
    setShowSelectModal(false);
    setPlantToLike(null);
  };

  // ----- Bottom Action Buttons -----
  const handlePassPress = () => {
    if (!plantStack.length) return;
    const topCard = plantStack[0];
    handleDislike(topCard.plantId);
  };

  const handleLikePress = () => {
    if (!plantStack.length) return;
    const topCard = plantStack[0];
    topCardRef.current?.resetPosition();
    setTimeout(() => {
      handleLikeGestureBegin(topCard);
    }, 10);
  };

  // ----- Header with Filters -----
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
        colors={[COLORS.primary, COLORS.secondary]}
        style={headerStyles.headerGradient}
      >
        <View style={headerStyles.headerRow}>
          <Text style={headerStyles.headerTitle}>
            {t('swipeScreen.header.explore')}
          </Text>
          <TouchableOpacity
            onPress={handleFilterPress}
            style={headerStyles.headerActionButton}
          >
            <Ionicons name="options" size={24} color={COLORS.textLight} />
          </TouchableOpacity>
        </View>
        <View style={styles.filterContainer}>
          <View style={styles.filterInfoContainer}>
            {prefTags.length > 0 ? (
              <Text style={styles.filterInfoTextColumn}>
                {t('swipeScreen.header.filters')}
              </Text>
            ) : (
              <Text style={styles.noFilterText}>
                {t('swipeScreen.header.noFilters')}
              </Text>
            )}
          </View>
          {prefTags.length > 0 && (
            <View style={styles.filterColumn}>
              <FlatList
                data={prefTags}
                keyExtractor={(item, index) =>
                  `${item.key}-${item.value}-${index}`
                }
                horizontal
                showsHorizontalScrollIndicator={false}
                renderItem={({ item }) => (
                  <TouchableOpacity
                    style={styles.removeTagButton}
                    onPress={() =>
                      handleRemoveSinglePreference(item.key, item.value)
                    }
                  >
                    <View style={styles.tagChip}>
                      <Text style={styles.tagChipText}>{item.value}</Text>
                      <Ionicons name="close-circle" size={16} color="#fff" />
                    </View>
                  </TouchableOpacity>
                )}
              />
            </View>
          )}
        </View>
      </LinearGradient>
    );
  };

  // ----- Main Card Stack Rendering -----
  const renderCardStack = () => {
    if (loadingPlants || loadingMyPlants) {
      return (
        <View style={styles.loaderContainer}>
          <ActivityIndicator size="large" color={COLORS.primary} />
          <Text style={styles.loaderText}>
            {t('swipeScreen.loader.loadingPlants')}
          </Text>
        </View>
      );
    }

    if (errorPlants || errorMyPlants) {
      return (
        <View style={styles.noPlantsContainer}>
          <Text style={styles.noPlantsText}>
            {t('swipeScreen.error.loadPlants')}
          </Text>
          <TouchableOpacity
            style={styles.reloadButton}
            onPress={() => {
              refetchLikablePlants();
              refetchMyPlants();
            }}
          >
            <Text style={styles.reloadButtonText}>
              {t('swipeScreen.button.tryAgain')}
            </Text>
          </TouchableOpacity>
        </View>
      );
    }

    if (myPlants && myPlants.length === 0) {
      return (
        <View style={styles.noPlantsContainer}>
          <Text style={styles.noPlantsText}>
            {t('swipeScreen.error.noPlantsGallery')}
          </Text>
          <TouchableOpacity
            style={styles.reloadButton}
            onPress={() => navigation.navigate('AddPlant' as never)}
          >
            <Text style={styles.reloadButtonText}>
              {t('swipeScreen.button.addPlant')}
            </Text>
          </TouchableOpacity>
        </View>
      );
    }

    if (!plantStack || plantStack.length === 0) {
      return (
        <View style={styles.noPlantsContainer}>
          <Text style={styles.noPlantsText}>
            {t('swipeScreen.error.noMorePlants')}
          </Text>
          <TouchableOpacity
            style={styles.reloadButton}
            onPress={() => refetchLikablePlants()}
          >
            <Text style={styles.reloadButtonText}>
              {t('swipeScreen.button.reload')}
            </Text>
          </TouchableOpacity>
        </View>
      );
    }

    const visibleCards = plantStack.slice(0, 3);
    return (
      <View style={styles.deckContainer}>
        {visibleCards
          .map((plant, index) => {
            const isTopCard = index === 0;
            const offset = (visibleCards.length - 1 - index) * 5;
            return (
              <View
                key={plant.plantId}
                style={[styles.cardWrapper, { top: offset, left: offset }]}
              >
                <SwipeableCard
                  ref={isTopCard ? topCardRef : null}
                  plant={plant}
                  onSwipeLeft={handleDislike}
                  onSwipeRight={handleSwipeRight}
                  onLikeGestureBegin={handleLikeGestureBegin}
                />
              </View>
            );
          })
          .reverse()}
      </View>
    );
  };

  return (
    <SafeAreaProvider style={styles.container}>
      {renderHeader()}
      {renderCardStack()}

      {/* Bottom Action Buttons */}
      {plantStack && plantStack.length > 0 && (
        <View style={styles.bottomActionContainer}>
          <TouchableOpacity
            onPress={handlePassPress}
            style={styles.actionButtonWrapper}
            disabled={sendingSwipes}
          >
            <LinearGradient
              colors={[COLORS.accentRed, COLORS.accentLightRed]}
              style={styles.actionButton}
            >
              <MaterialIcons name="close" size={32} color={COLORS.textLight} />
            </LinearGradient>
          </TouchableOpacity>

          <View style={styles.divider} />

          <TouchableOpacity
            onPress={handleLikePress}
            style={styles.actionButtonWrapper}
            disabled={sendingSwipes}
          >
            <LinearGradient
              colors={[COLORS.primary, COLORS.secondary]}
              style={styles.actionButton}
            >
              <MaterialIcons
                name="favorite"
                size={32}
                color={COLORS.textLight}
              />
            </LinearGradient>
          </TouchableOpacity>

          <SelectPlantsModal
            visible={showSelectModal}
            onConfirm={handleSelectConfirm}
            onClose={handleSelectCancel}
          />
          {matches.length > 0 && userProfile && (
            <ItsAMatchModal
              visible={matches.length > 0}
              matches={matches}
              currentUserId={userProfile.userId}
              onClose={clearMatches}
            />
          )}
        </View>
      )}
    </SafeAreaProvider>
  );
};

export default SwipeScreen;

const styles = StyleSheet.create({
  container: {
    flex: 1,
    flexDirection: 'column',
    backgroundColor: COLORS.background,
  },
  loaderContainer: {
    flex: 1,
    alignItems: 'center',
  },
  loaderText: {
    fontSize: 16,
    marginTop: 10,
    color: COLORS.textDark,
  },
  noPlantsContainer: {
    flex: 1,
    alignItems: 'center',
    paddingHorizontal: 20,
    justifyContent: 'center',
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
  filterContainer: {
    flexDirection: 'row',
  },
  filterInfoContainer: {
    marginTop: 10,
  },
  filterInfoTextColumn: {
    color: COLORS.textLight,
    fontSize: 14,
  },
  noFilterText: {
    color: COLORS.textLight,
    fontSize: 14,
  },
  filterColumn: {
    marginLeft: 5,
    flexDirection: 'row',
    flex: 1,
    alignItems: 'center',
  },
  tagChip: {
    flexDirection: 'row',
    backgroundColor: COLORS.accentRed,
    paddingVertical: 2,
    paddingHorizontal: 8,
    borderRadius: 16,
    marginRight: 8,
    marginTop: 10,
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
    marginBottom: 10,
    flex: 1,
    alignItems: 'center',
    right: 0,
  },
  cardWrapper: {
    width: width * 0.9,
  },
  bottomActionContainer: {
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
