import React, { useState, useCallback } from 'react';
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
import MapView, { Circle } from 'react-native-maps';

import * as ImagePicker from 'expo-image-picker';
import * as ImageManipulator from 'expo-image-manipulator';

import { useUserProfile } from '../hooks/useUser';
import { useMyPlants } from '../hooks/usePlants';
import { useSearchRadius } from '../hooks/useSearchRadius';
import { PlantResponse } from '../../../types/apiTypes';
import { userService } from '../../../api/userService'; // for updating the profile picture
import { EditProfileModal } from '../components/EditProfileModal';
import { ChangeLocationModal } from '../components/ChangeLocationModal';
import { rgbaColor } from 'react-native-reanimated/lib/typescript/Colors';
import {log} from '../../../utils/logger';
const { width } = Dimensions.get('window');
const COLORS = {
  primary: '#1EAE98',
  accent: '#FF6F61',
  background: '#F2F2F2',
  textDark: '#2F4F4F',
  textLight: '#FFFFFF',
  cardBg: '#FFFFFF',
  border: '#ddd',
};

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

  // For modals
  const [editProfileVisible, setEditProfileVisible] = useState(false);
  const [changeLocationVisible, setChangeLocationVisible] = useState(false);

  // For toggling between thumbnail vs. fullsize
  const [showFullSize, setShowFullSize] = useState(false);

  // -- IMAGE PICKER & UPDATE PFP --
  const handleChangeProfilePicture = useCallback(() => {
    Alert.alert(
      t('profile_change_picture_title'),
      t('profile_change_picture_msg'),
      [
        { text: t('profile_picture_select_library'), onPress: pickImageFromLibrary },
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
        aspect: [1, 1], // for a square/circle profile
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
      refetchProfile(); // refresh the user data to show updated pfp
    } catch (err) {
      console.error('Error uploading profile picture:', err);
      Alert.alert('Error', 'Profile picture update failed.');
    }
  };
  // -- END IMAGE PICKER & UPDATE PFP --

  // Modals
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

  const handleAddPlant = useCallback(() => {
    navigation.navigate('AddPlant' as never);
  }, [navigation]);

  // Check if user has location
  const userHasLocation =
    userProfile?.locationLatitude !== undefined &&
    userProfile?.locationLongitude !== undefined;

  const region = userHasLocation
    ? {
      latitude: userProfile.locationLatitude!,
      longitude: userProfile.locationLongitude!,
      latitudeDelta: 0.05,
      longitudeDelta: 0.05,
    }
    : undefined;

  // -- PLANT CARD RENDERING --
  const renderPlantItem = (item: PlantResponse) => {
    if (!showFullSize) {
      // THUMBNAIL VIEW
      
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
              <Ionicons name="leaf" size={40} color={COLORS.primary} />
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
      // FULLSIZE VIEW
      const alltags = [
        item.plantStage,
        item.plantCategory,
        item.wateringNeed,
        item.lightRequirement,
        item.size,
        item.indoorOutdoor,
        item.propagationEase,
        item.petFriendly,
        ...(item.extras ?? [])
      ].filter(Boolean);
      log.debug('alltags', alltags);
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
            {/* Overlay for tags & description */}
            <View style={styles.fullImageOverlay}>
              <LinearGradient
                colors={['rgba(0, 0, 0, 0)', 'rgba(0, 0, 0, 1)']}
                style={styles.overlayContent}>
                <Text style={styles.fullPlantName}>{item.speciesName}</Text>
                {/* Show tags if item.extras or other categories are present */}
                {alltags.length > 0 && (
                  <View style={styles.tagRow}>
                  {alltags.map((tag) => (
                    <View key={tag} style={styles.tag}>
                    <Text style={styles.tagText}>{tag}</Text>
                    </View>
                  ))}
                  </View>
                )}
                {/* description */}
                {item.description ? (
                  <Text style={styles.fullDescription}>{item.description}</Text>
                ) : null}
              </LinearGradient>
            </View>
          </View>
        </View>
      );
    }
  };
  // -- END RENDERING --

  // Decide main content
  let content: JSX.Element;

  if (loadingProfile || loadingPlants || srLoading) {
    // Loading
    content = (
      <SafeAreaView style={styles.centerContainer}>
        <ActivityIndicator size="large" color={COLORS.primary} />
        <Text style={styles.loadingText}>{t('profile_loading_message')}</Text>
      </SafeAreaView>
    );
  } else if (errorProfile || errorPlants || srError) {
    // Error
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
    // No profile
    content = (
      <SafeAreaView style={styles.centerContainer}>
        <Text style={styles.errorText}>
          {t('profile_no_user_profile_error')}
        </Text>
      </SafeAreaView>
    );
  } else {
    // Normal UI
    content = (
      <SafeAreaProvider style={styles.container}>
        <ScrollView
          style={{ flex: 1 }}
          contentContainerStyle={{ paddingBottom: 40 }}
        >
          {/* HEADER: GRADIENT */}
          <LinearGradient
            colors={[COLORS.primary, '#5EE2C6']}
            style={styles.headerContainer}
          >
            <View style={styles.headerTopRow}>
              <Text style={styles.headerTitle}>{t('profile_title')}</Text>
              <TouchableOpacity
                onPress={handleEditProfile}
                style={styles.headerActionButton}
                accessibilityLabel={t('profile_edit_button')}
              >
                <MaterialIcons name="edit" size={24} color={COLORS.textLight} />
              </TouchableOpacity>
            </View>

            {/* PROFILE INFO */}
            <View style={styles.profileInfoContainer}>
              {/* Tap on pfp or "Edit" icon to change profile picture */}
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

              <View style={styles.profileTextSection}>
                <Text style={styles.profileName}>{userProfile.name}</Text>
                {userProfile.bio ? (
                  <>
                    <Text style={styles.profileLabel}>
                      {t('profile_bio_label')}:
                    </Text>
                    <Text style={styles.profileValue} numberOfLines={3}>
                      {userProfile.bio}
                    </Text>
                  </>
                ) : null}

                {/* LOCATION */}
                <View style={styles.profileLocationSection}>
                  <Text style={styles.profileLabel}>
                    {t('profile_location_label')}:
                  </Text>
                  {userHasLocation ? (
                    <View style={styles.mapContainer}>
                      <MapView style={styles.map} initialRegion={region}>
                        <Circle
                          center={{
                            latitude: region.latitude,
                            longitude: region.longitude,
                          }}
                          radius={searchRadius * 1000} // searchRadius in km -> convert to meters
                          strokeWidth={1.5}
                          strokeColor="#1EAE98"
                          fillColor="rgba(30, 174, 152, 0.2)"
                        />
                      </MapView>
                    </View>
                  ) : (
                    <Text style={styles.profileValue}>
                      {t('profile_no_location')}
                    </Text>
                  )}
                </View>

                <TouchableOpacity
                  onPress={handleChangeLocation}
                  style={styles.locationButton}
                  accessibilityRole="button"
                  accessibilityLabel={t('profile_change_location_button')}
                >
                  <Ionicons
                    name="location-outline"
                    size={18}
                    color={COLORS.primary}
                  />
                  <Text style={styles.locationButtonText}>
                    {t('profile_change_location_button')}
                  </Text>
                </TouchableOpacity>
              </View>
            </View>
          </LinearGradient>

          {/* MY PLANTS SECTION */}
          <View style={styles.plantsSectionWrapper}>
            <View style={styles.plantsSectionHeader}>
              <Text style={styles.plantsSectionTitle}>
                {t('profile_my_plants_section')}
              </Text>
              <TouchableOpacity
                onPress={handleAddPlant}
                style={styles.addPlantButton}
                accessibilityRole="button"
                accessibilityLabel={t('profile_add_plant_button')}
              >
                <Ionicons name="add-circle" size={24} color={COLORS.primary} />
                <Text style={styles.addPlantButtonText}>
                  {t('profile_add_plant_button')}
                </Text>
              </TouchableOpacity>
            </View>

            {/* CUSTOM TOGGLE (instead of Switch) */}
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

            {/* PLANTS DISPLAY */}
            {myPlants && myPlants.length > 0 ? (
              <View
                style={[
                  showFullSize
                    ? styles.fullViewContainer
                    : styles.thumbViewContainer,
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

          {/* MODALS */}
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

  // HEADER
  headerContainer: {
    paddingHorizontal: 20,
    paddingVertical: 20,
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
    fontSize: 24,
    fontWeight: '700',
    color: COLORS.textLight,
  },
  headerActionButton: {
    padding: 8,
  },

  // PROFILE INFO
  profileInfoContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginTop: 15,
  },
  profilePictureWrapper: {
    position: 'relative',
    marginRight: 15,
  },
  profilePicture: {
    width: 100,
    height: 100,
    borderRadius: 50,
    backgroundColor: '#eee',
    borderWidth: 2,
    borderColor: '#fff',
  },
  profilePlaceholder: {
    width: 100,
    height: 100,
    borderRadius: 50,
    backgroundColor: '#eee',
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 2,
    borderColor: '#fff',
  },
  cameraIconWrapper: {
    position: 'absolute',
    bottom: 0,
    right: 0,
    backgroundColor: COLORS.primary,
    borderRadius: 16,
    padding: 4,
  },
  profileTextSection: {
    flexShrink: 1,
  },
  profileName: {
    fontSize: 18,
    fontWeight: '700',
    color: COLORS.textLight,
    marginBottom: 6,
  },
  profileLabel: {
    fontSize: 14,
    fontWeight: '600',
    color: COLORS.textLight,
    marginBottom: 2,
  },
  profileValue: {
    fontSize: 14,
    color: COLORS.textLight,
    marginBottom: 8,
  },
  profileLocationSection: {
    marginTop: 6,
  },

  // MAP
  mapContainer: {
    marginTop: 6,
    width: 200,
    height: 120,
    borderRadius: 8,
    overflow: 'hidden',
    backgroundColor: '#ddd',
  },
  map: {
    width: '100%',
    height: '100%',
  },

  // CHANGE LOCATION BUTTON
  locationButton: {
    flexDirection: 'row',
    alignItems: 'center',
    marginTop: 8,
    paddingHorizontal: 10,
    paddingVertical: 6,
    borderWidth: 1,
    borderColor: COLORS.primary,
    borderRadius: 8,
    alignSelf: 'flex-start',
    backgroundColor: '#fff',
  },
  locationButtonText: {
    color: COLORS.primary,
    fontSize: 14,
    marginLeft: 5,
    fontWeight: '600',
  },

  // PLANTS SECTION
  plantsSectionWrapper: {
    paddingHorizontal: 10,
    paddingTop: 10,
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
    color: COLORS.primary,
    marginLeft: 5,
    fontWeight: '600',
  },

  // CUSTOM TOGGLE
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
    backgroundColor: COLORS.primary,
  },
  viewToggleText: {
    fontSize: 14,
    color: COLORS.textDark,
    fontWeight: '600',
  },
  viewToggleTextActive: {
    color: '#fff',
  },

  // THUMBNAIL VIEW
  thumbViewContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'space-between',
  },
  plantCardThumbnail: {
    width: (width - 50) / 3,
    backgroundColor: COLORS.cardBg,
    borderRadius: 8,
    marginBottom: 15,
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

  // FULL-SIZE VIEW
  fullViewContainer: {
    // a simple vertical stack
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
    // remove forced aspect ratio so the image can keep its own ratio via "contain"
    position: 'relative',
  },
  fullImage: {
    width: '100%',
    aspectRatio: 3 / 4,
    //here I want an automatic height based on the width
  },
  fullImageOverlay: {
    ...StyleSheet.absoluteFillObject,
    justifyContent: 'flex-end',
  },
  overlayContent: {
    padding: 10,
    paddingTop: 100,
  },
  fullPlantName: {
    fontSize: 18,
    fontWeight: '700',
    color: '#fff',
    marginBottom: 6,
  },
  tagRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    marginBottom: 6,
  },
  tag: {
    backgroundColor: COLORS.primary,
    borderRadius: 12,
    paddingHorizontal: 8,
    paddingVertical: 4,
    marginRight: 6,
    marginBottom: 6,
  },
  tagText: {
    color: '#fff',
    fontSize: 12,
    fontWeight: '600',
  },
  fullDescription: {
    color: '#fff',
    fontSize: 14,
    fontWeight: '400',
  },

  // NO PLANTS
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
