import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ActivityIndicator,
  TouchableOpacity,
  Image,
  Dimensions,
  Platform,
  ScrollView,
  Alert,
} from 'react-native';
import { SafeAreaView, SafeAreaProvider } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';
import { useNavigation } from '@react-navigation/native';
import { Ionicons } from '@expo/vector-icons';
import { LinearGradient } from 'expo-linear-gradient';
import * as Location from 'expo-location';
import { MaterialIcons } from '@expo/vector-icons';
import { ImageBackground } from 'react-native';
import { PlantCardWithInfo } from '../components/PlantCardWithInfo';
import { useUserProfile } from '../hooks/useUser';
import { useMyPlants } from '../hooks/usePlants';
import { useSearchRadius } from '../hooks/useSearchRadius';
import { PlantResponse } from '../../../types/apiTypes';
import { COLORS } from '../../../theme/colors';
import { EditProfileModal } from '../components/EditProfileModal';
import { PlantOverlay } from '../components/PlantOverlay';
import { headerStyles } from '../styles/headerStyles';

const { width } = Dimensions.get('window');

const MyProfileScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation();

  // Hooks
  const {
    data: userProfile,
    isLoading: loadingProfile,
    isError: errorProfile,
    refetch: refetchProfile,
  } = useUserProfile();
  const {
    data: myPlants,
    isLoading: loadingPlants,
    isError: errorPlants,
    refetch: refetchPlants,
  } = useMyPlants();
  const {
    searchRadius,
    isLoading: srLoading,
    isError: srError,
  } = useSearchRadius();

  // Derived state
  const [cityCountry, setCityCountry] = useState<string>('');
  const [editProfileVisible, setEditProfileVisible] = useState(false);

  // Toggle: Thumbnails or Full-size for plants
  const [showFullSize, setShowFullSize] = useState(false);

  // Check if user has location
  const userHasLocation =
    userProfile?.locationLatitude !== undefined &&
    userProfile?.locationLongitude !== undefined;

  // Reverse-geocode city/country
  useEffect(() => {
    (async () => {
      if (userHasLocation) {
        try {
          const [geo] = await Location.reverseGeocodeAsync({
            latitude: userProfile!.locationLatitude!,
            longitude: userProfile!.locationLongitude!,
          });
          if (geo) {
            const city = geo.city || geo.subregion || '';
            const country = geo.country || '';
            setCityCountry(
              city && country ? `${city}, ${country}` : city || country
            );
          }
        } catch (error) {
          console.log('Reverse geocoding error:', error);
          setCityCountry('');
        }
      } else {
        setCityCountry('');
      }
    })();
  }, [userHasLocation, userProfile]);

  // Navigation
  const handleAddPlant = () => {
    navigation.navigate('AddPlant' as never);
  };

  // Plants rendering
  const renderPlantItem = (item: PlantResponse) => {
    if (!showFullSize) {
      return (
        <View key={item.plantId} style={styles.plantCardThumbnail}>
          {item.imageUrl ? (
            <Image
              source={{ uri: item.imageUrl }}
              style={styles.thumbImage}
              resizeMode="contain"
            />
          ) : (
            <View style={styles.plantPlaceholder}>
              <Ionicons name="leaf" size={40} color={COLORS.accentGreen} />
            </View>
          )}
          <View style={styles.thumbTextWrapper}>
            <Text style={styles.thumbPlantName}>
              {item.speciesName}
            </Text>
          </View>
        </View>
      );
    } else {
      return (
        <View style={styles.plantCardWrapper}>
        <PlantCardWithInfo
          key={item.plantId}
          plant={item}
        />
      </View>
      );
    }
  };

  // Content states
  if (loadingProfile || loadingPlants || srLoading) {
    return (
      <SafeAreaView style={styles.centerContainer}>
        <ActivityIndicator size="large" color={COLORS.primary} />
        <Text style={styles.loadingText}>{t('profile_loading_message')}</Text>
      </SafeAreaView>
    );
  }

  if (errorProfile || errorPlants || srError) {
    return (
      <SafeAreaView style={styles.centerContainer}>
        <Text style={styles.errorText}>{t('profile_error_message')}</Text>
        <TouchableOpacity
          onPress={() => {
            refetchProfile();
            refetchPlants();
          }}
          style={styles.retryButton}
        >
          <Text style={styles.retryButtonText}>
            {t('profile_retry_button')}
          </Text>
        </TouchableOpacity>
      </SafeAreaView>
    );
  }

  if (!userProfile) {
    return (
      <SafeAreaView style={styles.centerContainer}>
        <Text style={styles.errorText}>
          {t('profile_no_user_profile_error')}
        </Text>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaProvider style={styles.container}>
      <ScrollView
        style={{ flex: 1 }}
        contentContainerStyle={{ paddingBottom: 40 }}
      >
        {/* Header */}
        <LinearGradient
          colors={[COLORS.primary, COLORS.secondary]}
          style={headerStyles.headerGradient}
        >
          <Text style={headerStyles.headerTitle}>{t('profile_title')}</Text>
        </LinearGradient>

        {/* --- Profile Card --- */}
        <View style={styles.profileCardContainer}>
          <LinearGradient
            colors={[COLORS.cardBg1, COLORS.cardBg2]}
            style={styles.profileCardInner}
          >
            <View style={styles.profileTopContainer}>
              <ImageBackground
              source={require('../../../../assets/images/profileBackground.png')}
              style={styles.profileBackgroundImage}
              />
              <View style={styles.profilePictureContainer}>
                {userProfile.profilePictureUrl ? (
                  <Image
                    source={{ uri: userProfile.profilePictureUrl }}
                    style={styles.profilePicture}
                  />
                ) : (
                  <View style={styles.profilePlaceholder}>
                    <Ionicons name="person-circle-outline" size={90} color="#ccc" />
                  </View>
                )}
              </View>
              <TouchableOpacity
                onPress={() => setEditProfileVisible(true)}
                style={styles.profileEditButton}
                accessibilityLabel={t('profile_edit_button')}
              >
                <MaterialIcons name="edit" size={20} color={COLORS.textLight} />
              </TouchableOpacity>
            </View>

            {/* Middle portion (Name, Location, Bio) */}
            <View style={styles.profileInfoContainer}>
              <Text style={styles.profileNameText}>{userProfile.name}</Text>
              <View style={styles.profileLocationRow}>
                <Ionicons
                  name="location-sharp"
                  size={16}
                  color={COLORS.accentLightRed}
                  style={styles.locationIcon}
                />
                <Text style={styles.profileLocationText}>
                  {cityCountry || t('profile_no_location')}
                </Text>
              </View>
            </View>
            
              <Text
                style={[
                  styles.bioText,
                  !userProfile.bio && styles.bioPlaceholder,
                ]}
              >
                {userProfile.bio
                  ? userProfile.bio
                  : t('profile_no_bio_placeholder')}
              </Text>
          </LinearGradient>
        </View>

        {/* ---- My Plants Section ---- */}
        <View style={styles.plantsSectionWrapper}>
          <View style={styles.plantsSectionHeader}>
            <Text style={styles.plantsSectionTitle}>
              {userProfile.name}
              {t('profile_my_plants_section')}
            </Text>
            <TouchableOpacity
              onPress={handleAddPlant}
              style={styles.addPlantButton}
              accessibilityRole="button"
              accessibilityLabel={t('profile_add_plant_button')}
            >
              <Ionicons name="add-circle" size={24} color={COLORS.accentGreen} />
              <Text style={styles.addPlantButtonText}>
                {t('profile_add_plant_button')}
              </Text>
            </TouchableOpacity>
          </View>

          {/* Toggle Thumbnails / Full */}
          <View style={styles.viewToggleContainer}>
            <TouchableOpacity
              onPress={() => setShowFullSize(false)}
              style={[
                styles.viewToggleOption,
                !showFullSize && styles.viewToggleOptionActive,
              ]}
              activeOpacity={0.9}
            >
              <Text
                style={[
                  styles.viewToggleText,
                  !showFullSize && styles.viewToggleTextActive,
                ]}
              >
                {t('Thumbnails')}
              </Text>
            </TouchableOpacity>
            <TouchableOpacity
              onPress={() => setShowFullSize(true)}
              style={[
                styles.viewToggleOption,
                showFullSize && styles.viewToggleOptionActive,
              ]}
              activeOpacity={0.9}
            >
              <Text
                style={[
                  styles.viewToggleText,
                  showFullSize && styles.viewToggleTextActive,
                ]}
              >
                {t('Full Size')}
              </Text>
            </TouchableOpacity>
          </View>

          {/* Plants List */}
          {myPlants && myPlants.length > 0 ? (
            <View
              style={[
                showFullSize ? styles.fullViewContainer : styles.thumbViewContainer,
              ]}
            >
              {myPlants.map((plant) => renderPlantItem(plant))}
            </View>
          ) : (
            <View style={styles.noPlantsContainer}>
              <Text style={styles.noPlantsText}>
                {t('profile_no_plants_message')}
              </Text>
            </View>
          )}
        </View>

        {/* --- EditProfileModal (consolidated) --- */}
        <EditProfileModal
          visible={editProfileVisible}
          userProfile={userProfile}
          onClose={() => setEditProfileVisible(false)}
          onUpdated={() => {
            refetchProfile();
          }}
        />
      </ScrollView>
    </SafeAreaProvider>
  );
};

export default MyProfileScreen;

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
    fontSize: 16,
    color: COLORS.textDark,
    marginTop: 10,
  },
  errorText: {
    fontSize: 16,
    color: COLORS.textDark,
    marginBottom: 20,
    textAlign: 'center',
  },
  retryButton: {
    backgroundColor: COLORS.primary,
    paddingVertical: 10,
    paddingHorizontal: 20,
    borderRadius: 8,
  },
  retryButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },

  /* --- Profile Card --- */
  profileCardContainer: {
    marginHorizontal: 16,
    marginTop: 20,
    borderRadius: 18,
    overflow: 'hidden',
    ...Platform.select({
      ios: {
        shadowColor: '#000',
        shadowOpacity: 0.12,
        shadowRadius: 6,
        shadowOffset: { width: 0, height: 3 },
      },
      android: {
        elevation: 4,
      },
    }),
  },
  profileCardInner: {
    borderRadius: 18,
  },
  profileTopContainer: {
    backgroundColor: COLORS.primary,
    height: 120,
    position: 'relative',
  },
  profileBackgroundImage: {
    height: '100%',
    resizeMode: 'cover',
    position: 'absolute',
    bottom: 0,
    right: 0,
    left: -200,
  },
  profilePictureContainer: {
    position: 'absolute',
    bottom: -75,
    left: 25,
  },
  profilePicture: {
    width: 170,
    height: 170,
    borderRadius: 40,
    borderWidth: 3,
    borderColor: '#fff',
  },
  profilePlaceholder: {
    width: 150,
    height: 150,
    borderRadius: 40,
    backgroundColor: '#eee',
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 3,
    borderColor: '#fff',
  },
  profileInfoContainer: {
    paddingHorizontal: 20,
    right: -190,
  },
  profileNameText: {
    fontSize: 20,
    fontWeight: '700',
    color: COLORS.textDark,
    marginTop: 8,
  },
  profileLocationRow: {
    flexDirection: 'row',
    alignItems: 'center',
    marginTop: 4,
  },
  locationIcon: {
    marginRight: 4,
    top: 1,
  },
  profileLocationText: {
    fontSize: 14,
    color: COLORS.textDark,
    marginTop: 4,
  },
  bioText: {
    fontSize: 14,
    color: COLORS.textDark,
    lineHeight: 20,
    margin: 25,
    marginBottom: 25,
  },
  bioPlaceholder: {
    color: '#999',
    fontStyle: 'italic',
    marginBottom: 20,
  },
  profileEditButton: {
    position: 'absolute',
    top: 16,
    right: 16,
    width: 35,
    height: 35,
    borderRadius: 20,
    backgroundColor: COLORS.accentGreen,
    justifyContent: 'center',
    alignItems: 'center',
    ...Platform.select({
      ios: {
        shadowColor: '#000',
        shadowOpacity: 0.1,
        shadowRadius: 5,
        shadowOffset: { width: 0, height: 3 },
      },
      android: {
        elevation: 3,
      },
    }),
  },

  /* ---- Plants Section ---- */
  plantsSectionWrapper: {
    paddingHorizontal: 10,
    paddingTop: 20,
  },
  plantsSectionHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 10,
    paddingHorizontal: 10,
  },
  plantsSectionTitle: {
    fontSize: 20,
    fontWeight: '700',
    color: COLORS.textDark,
  },
  addPlantButton: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  addPlantButtonText: {
    fontSize: 14,
    color: COLORS.accentGreen,
    marginLeft: 5,
    fontWeight: '600',
  },
  viewToggleContainer: {
    flexDirection: 'row',
    alignSelf: 'center',
    borderColor: COLORS.border,
    borderWidth: 1,
    borderRadius: 20,
    overflow: 'hidden',
    marginBottom: 12,
  },
  viewToggleOption: {
    paddingVertical: 8,
    paddingHorizontal: 20,
    backgroundColor: '#fff',
  },
  viewToggleOptionActive: {
    backgroundColor: COLORS.accentGreen,
  },
  viewToggleText: {
    fontSize: 14,
    color: COLORS.textDark,
    fontWeight: '600',
  },
  viewToggleTextActive: {
    color: '#fff',
  },
  thumbViewContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'center',
  },
  plantCardThumbnail: {
    width: (width - 80) / 3,
    backgroundColor: COLORS.cardBg1,
    borderRadius: 8,
    margin: 8,
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
    textAlign: 'center',
  },
  fullViewContainer: {
    width: '100%',
  },
  plantCardWrapper: {
    marginBottom: 15,
  },
  noPlantsContainer: {
    justifyContent: 'center',
    alignItems: 'center',
    padding: 20,
  },
  noPlantsText: {
    fontSize: 16,
    color: '#555',
    textAlign: 'center',
  },
});
