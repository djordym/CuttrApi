import React, { useState, useCallback, useEffect } from 'react';
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
import { Ionicons, MaterialIcons } from '@expo/vector-icons';
import { LinearGradient } from 'expo-linear-gradient';
import * as ImagePicker from 'expo-image-picker';
import * as ImageManipulator from 'expo-image-manipulator';
import * as Location from 'expo-location';

import { useUserProfile } from '../hooks/useUser';
import { useMyPlants } from '../hooks/usePlants';
import { useSearchRadius } from '../hooks/useSearchRadius';
import { PlantResponse } from '../../../types/apiTypes';
import { userService } from '../../../api/userService';
import { EditProfileModal } from '../components/EditProfileModal';
import { ChangeLocationModal } from '../components/ChangeLocationModal';
import { COLORS } from '../../../theme/colors';
import { PlantOverlay } from '../components/PlantOverlay';
import { headerStyles } from '../styles/headerStyles';

const { width } = Dimensions.get('window');

const MyProfileScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation();

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

  // Modal visibility
  const [editProfileVisible, setEditProfileVisible] = useState(false);
  const [changeLocationVisible, setChangeLocationVisible] = useState(false);

  // Toggle: Thumbnails or Full-size
  const [showFullSize, setShowFullSize] = useState(false);

  // Store city/country derived from lat/long
  const [cityCountry, setCityCountry] = useState<string>('');

  // Does user have lat/long set?
  const userHasLocation =
    userProfile?.locationLatitude !== undefined &&
    userProfile?.locationLongitude !== undefined;

  // Reverse-geocode to get city/country
  useEffect(() => {
    (async () => {
      if (userHasLocation) {
        try {
          const [geo] = await Location.reverseGeocodeAsync({
            latitude: userProfile!.locationLatitude!,
            longitude: userProfile!.locationLongitude!,
          });
          if (geo) {
            // Construct city/country string. Adjust as you like:
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

  // ----- IMAGE PICKER & UPDATE PFP -----
  const handleChangeProfilePicture = useCallback(() => {
    Alert.alert(
      t('profile_change_picture_title'),
      t('profile_change_picture_msg'),
      [
        {
          text: t('profile_picture_select_library'),
          onPress: pickImageFromLibrary,
        },
        { text: t('profile_picture_take_photo'), onPress: takePictureWithCamera },
        { text: t('profile_picture_cancel'), style: 'cancel' },
      ]
    );
  }, [t]);

  const pickImageFromLibrary = async () => {
    try {
      const result = await ImagePicker.launchImageLibraryAsync({
        mediaTypes: ImagePicker.MediaTypeOptions.Images,
        allowsEditing: true,
        aspect: [1, 1],
        quality: 0.7,
      });
      if (!result.canceled && result.assets[0].uri) {
        const resized = await resizeImage(result.assets[0].uri);
        await uploadProfilePicture(resized);
      }
    } catch (err) {
      console.error('pickImageFromLibrary error:', err);
      Alert.alert('Error', 'Could not open image library.');
    }
  };

  const takePictureWithCamera = async () => {
    try {
      const cameraPermission = await ImagePicker.requestCameraPermissionsAsync();
      if (!cameraPermission.granted) {
        Alert.alert('Error', 'Camera permission denied.');
        return;
      }
      const result = await ImagePicker.launchCameraAsync({
        allowsEditing: true,
        aspect: [1, 1],
        quality: 0.7,
      });
      if (!result.canceled && result.assets[0].uri) {
        const resized = await resizeImage(result.assets[0].uri);
        await uploadProfilePicture(resized);
      }
    } catch (err) {
      console.error('takePictureWithCamera error:', err);
      Alert.alert('Error', 'Could not open camera.');
    }
  };

  const resizeImage = async (uri: string) => {
    return await ImageManipulator.manipulateAsync(
      uri,
      [{ resize: { width: 800 } }],
      { compress: 0.7, format: ImageManipulator.SaveFormat.JPEG }
    );
  };

  const uploadProfilePicture = async (img: ImageManipulator.ImageResult) => {
    try {
      const photo = {
        uri: img.uri,
        name: 'profile.jpg',
        type: 'image/jpeg',
      } as any;
      await userService.updateProfilePicture({ image: photo });
      refetchProfile();
    } catch (err) {
      console.error('Error uploading profile picture:', err);
      Alert.alert('Error', 'Profile picture update failed.');
    }
  };
  // ----- END IMAGE PICKER & UPDATE PFP -----

  // Modal callbacks
  const handleEditProfile = useCallback(() => {
    setEditProfileVisible(true);
  }, []);
  const handleProfileUpdated = useCallback(() => {
    refetchProfile();
  }, [refetchProfile]);

  const handleChangeLocation = useCallback(() => {
    setChangeLocationVisible(true);
  }, []);
  const handleLocationUpdated = useCallback(() => {
    refetchProfile();
  }, [refetchProfile]);

  // Navigation
  const handleAddPlant = useCallback(() => {
    navigation.navigate('AddPlant' as never);
  }, [navigation]);

  // Rendering plants
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
            <Text style={styles.thumbPlantName} numberOfLines={1}>
              {item.speciesName}
            </Text>
          </View>
        </View>
      );
    } else {
      const alltags = [
        item.plantStage,
        item.plantCategory,
        item.wateringNeed,
        item.lightRequirement,
        item.size,
        item.indoorOutdoor,
        item.propagationEase,
        item.petFriendly,
        ...(item.extras ?? []),
      ].filter(Boolean);

      return (
        <View key={item.plantId} style={styles.plantCardFull}>
          <View style={styles.fullImageContainer}>
            {item.imageUrl ? (
              <Image
                source={{ uri: item.imageUrl }}
                style={styles.fullImage}
                resizeMode="contain"
              />
            ) : (
              <View style={styles.plantPlaceholder}>
                <Ionicons name="leaf" size={60} color={COLORS.primary} />
              </View>
            )}
            <View style={styles.fullImageOverlay}>
              <PlantOverlay
                speciesName={item.speciesName}
                description={item.description}
                tags={alltags}
              />
            </View>
          </View>
        </View>
      );
    }
  };

  // Main Content
  let content: JSX.Element;

  if (loadingProfile || loadingPlants || srLoading) {
    content = (
      <SafeAreaView style={styles.centerContainer}>
        <ActivityIndicator size="large" color={COLORS.primary} />
        <Text style={styles.loadingText}>{t('profile_loading_message')}</Text>
      </SafeAreaView>
    );
  } else if (errorProfile || errorPlants || srError) {
    content = (
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
  } else if (!userProfile) {
    content = (
      <SafeAreaView style={styles.centerContainer}>
        <Text style={styles.errorText}>
          {t('profile_no_user_profile_error')}
        </Text>
      </SafeAreaView>
    );
  } else {
    content = (
      <SafeAreaProvider style={styles.container}>
        <ScrollView style={{ flex: 1 }} contentContainerStyle={{ paddingBottom: 40 }}>
          
          <LinearGradient
            colors={[COLORS.primary, COLORS.secondary]}
            style={headerStyles.headerGradient}
          >
            <Text style={headerStyles.headerTitle}>
              {t('profile_title')}
            </Text>
          </LinearGradient>

          {/* ---- Profile Card Section ---- */}
          <View style={styles.profileCard}>
            {/* Edit Icon in top-right corner */}
            <TouchableOpacity
              onPress={handleEditProfile}
              style={styles.profileEditButton}
              accessibilityLabel={t('profile_edit_button')}
            >
              <MaterialIcons name="edit" size={20} color={COLORS.textDark} />
            </TouchableOpacity>

            {/* Profile Picture */}
            <TouchableOpacity
              onPress={handleChangeProfilePicture}
              activeOpacity={0.8}
              style={styles.profilePictureWrapper}
            >
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
              <View style={styles.cameraIconWrapper}>
                <Ionicons name="camera" size={18} color={COLORS.textLight} />
              </View>
            </TouchableOpacity>

            {/* User Name */}
            <Text style={styles.profileNameText}>
              {userProfile.name}
            </Text>

            {/* Location (City, Country) and a button to update location */}
            <View style={styles.locationRow}>
              {cityCountry ? (
                <Text style={styles.profileLocationText}>
                  {cityCountry}
                </Text>
              ) : (
                <Text style={styles.profileLocationText}>
                  {t('profile_no_location')}
                </Text>
              )}
              <TouchableOpacity
                onPress={handleChangeLocation}
                style={styles.locationButton}
              >
                <Ionicons name="location-outline" size={18} color={COLORS.accentGreen} />
                <Text style={styles.locationButtonText}>
                  {t('profile_change_location_button')}
                </Text>
              </TouchableOpacity>
            </View>

            {/* Bio */}
            {userProfile.bio ? (
              <Text style={styles.bioText} numberOfLines={6}>
                {userProfile.bio}
              </Text>
            ) : (
              <Text style={styles.bioPlaceholder} numberOfLines={3}>
                {t('profile_no_bio_placeholder')}
              </Text>
            )}
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

          {/* ---- Modals ---- */}
          <EditProfileModal
            visible={editProfileVisible}
            initialName={userProfile.name}
            initialBio={userProfile.bio || ''}
            onClose={() => setEditProfileVisible(false)}
            onUpdated={handleProfileUpdated}
          />
          <ChangeLocationModal
            visible={changeLocationVisible}
            initialLatitude={userProfile.locationLatitude}
            initialLongitude={userProfile.locationLongitude}
            onClose={() => setChangeLocationVisible(false)}
            onUpdated={handleLocationUpdated}
          />
        </ScrollView>
      </SafeAreaProvider>
    );
  }

  return content;
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
  /* ---- Profile Card ---- */
  profileCard: {
    marginTop: 0, // so it overlaps the gradient a bit if you like
    marginHorizontal: 16,
    backgroundColor: COLORS.primary,
    borderRadius: 12,
    padding: 20,
    position: 'relative',
    ...Platform.select({
      ios: {
        shadowColor: '#000',
        shadowOpacity: 0.12,
        shadowRadius: 6,
        shadowOffset: { width: 0, height: 3 },
      },
      android: {
        elevation: 3,
      },
    }),
  },
  profileEditButton: {
    position: 'absolute',
    top: 16,
    right: 16,
    zIndex: 2,
    padding: 6,
  },
  profilePictureWrapper: {
    alignSelf: 'center',
    position: 'relative',
    marginBottom: 12,
  },
  profilePicture: {
    width: (width - 100) / 2,
    height: (width - 100) / 2,
    borderRadius: (width - 100) / 4,
    backgroundColor: '#eee',
  },
  profilePlaceholder: {
    width: (width - 100) / 2,
    height: (width - 100) / 2,
    borderRadius: (width - 100) / 4,
    backgroundColor: '#eee',
    alignItems: 'center',
    justifyContent: 'center',
  },
  cameraIconWrapper: {
    position: 'absolute',
    bottom: 0,
    right: 15,
    backgroundColor: COLORS.accentGreen,
    borderRadius: 16,
    padding: 4,
  },
  profileNameText: {
    fontSize: 20,
    fontWeight: '700',
    color: COLORS.textDark,
    textAlign: 'center',
  },
  locationRow: {
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    marginVertical: 10,
    flexWrap: 'wrap',
  },
  profileLocationText: {
    fontSize: 14,
    color: COLORS.textDark,
    marginRight: 12,
  },
  locationButton: {
    flexDirection: 'row',
    alignItems: 'center',
    borderWidth: 1,
    borderColor: COLORS.accentGreen,
    borderRadius: 8,
    paddingHorizontal: 10,
    paddingVertical: 4,
  },
  locationButtonText: {
    color: COLORS.accentGreen,
    fontSize: 14,
    marginLeft: 5,
    fontWeight: '600',
  },
  bioText: {
    fontSize: 14,
    color: COLORS.textDark,
    textAlign: 'center',
    lineHeight: 20,
    marginTop: 8,
  },
  bioPlaceholder: {
    fontSize: 14,
    color: '#999',
    fontStyle: 'italic',
    textAlign: 'center',
    lineHeight: 20,
    marginTop: 8,
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
    backgroundColor: COLORS.cardBg,
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
  },
  fullViewContainer: {
    width: '100%',
  },
  plantCardFull: {
    marginBottom: 15,
    width: '100%',
    borderRadius: 8,
    overflow: 'hidden',
    ...Platform.select({
      ios: {
        shadowColor: '#000',
        shadowOpacity: 0.12,
        shadowRadius: 5,
        shadowOffset: { width: 0, height: 3 },
      },
      android: {
        elevation: 3,
      },
    }),
  },
  fullImageContainer: {
    width: '100%',
    position: 'relative',
  },
  fullImage: {
    width: '100%',
    aspectRatio: 3 / 4,
  },
  fullImageOverlay: {
    ...StyleSheet.absoluteFillObject,
    justifyContent: 'flex-end',
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
