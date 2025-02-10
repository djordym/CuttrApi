// components/profile/EditProfileModal.tsx
import React, { useState, useEffect, useCallback } from 'react';
import {
  Modal,
  View,
  Text,
  Image,
  TextInput,
  StyleSheet,
  TouchableOpacity,
  ActivityIndicator,
  Alert,
  Dimensions,
  ImageBackground,
} from 'react-native';
import { useTranslation } from 'react-i18next';
import { Ionicons, MaterialIcons } from '@expo/vector-icons';
import * as ImagePicker from 'expo-image-picker';
import * as ImageManipulator from 'expo-image-manipulator';
import * as Location from 'expo-location';
import { LinearGradient } from 'expo-linear-gradient';

import { ChangeLocationModal } from './ChangeLocationModal';
import { COLORS } from '../../../theme/colors';
import { profileCardStyles } from '../styles/profileCardStyles';
import { log } from '../../../utils/logger';
import {
  useMyProfile,
  useUpdateProfile,
  useUpdateProfilePicture,
} from '../hooks/useMyProfileHooks'; // Adjust the path as necessary
import { UserResponse, UserUpdateRequest } from '../../../types/apiTypes';

interface EditProfileModalProps {
  visible: boolean;
  userProfile: UserResponse;
  onClose: () => void;
  onUpdated: () => void; // callback after successful update (refetch in parent)
  cardLayout: { x: number; y: number; width: number; height: number };
}

export const EditProfileModal: React.FC<EditProfileModalProps> = ({
  visible,
  userProfile,
  onClose,
  onUpdated,
  cardLayout,
}) => {
  const { t } = useTranslation();
  const screenWidth = Dimensions.get('window').width;

  // Local states for name, bio, and location display
  const [name, setName] = useState(userProfile.name);
  const [bio, setBio] = useState(userProfile.bio || '');
  const [cityCountry, setCityCountry] = useState<string>('');
  const [error, setError] = useState<string | null>(null);

  // For nested ChangeLocationModal
  const [locationModalVisible, setLocationModalVisible] = useState(false);

  // Reverse-geocode city/country for display
  const userHasLocation =
    userProfile.locationLatitude !== undefined &&
    userProfile.locationLongitude !== undefined;

  useEffect(() => {
    (async () => {
      if (userHasLocation) {
        try {
          const [geo] = await Location.reverseGeocodeAsync({
            latitude: userProfile.locationLatitude!,
            longitude: userProfile.locationLongitude!,
          });
          if (geo) {
            const city = geo.city || geo.subregion || '';
            const country = geo.country || '';
            setCityCountry(
              city && country ? `${city}, ${country}` : city || country
            );
          }
        } catch (err) {
          console.log('Reverse geocoding error:', err);
          setCityCountry('');
        }
      } else {
        setCityCountry('');
      }
    })();
  }, [userHasLocation, userProfile]);

  // ----------- Hooks for Mutations -----------
  const {
    mutate: updateProfile,
    isLoading: isUpdatingProfile,
    isError: isUpdateProfileError,
    error: updateProfileError,
  } = useUpdateProfile();

  const {
    mutate: updateProfilePicture,
    isLoading: isUpdatingPicture,
    isError: isUpdatePictureError,
    error: updatePictureError,
  } = useUpdateProfilePicture();

  const isUpdating = isUpdatingProfile || isUpdatingPicture;

  // ----------- Handle Profile Picture Updates -----------
  const handleChangeProfilePicture = useCallback(() => {
    Alert.alert(
      t('profile_change_picture_title'),
      t('profile_change_picture_msg'),
      [
        {
          text: t('profile_picture_select_library'),
          onPress: pickImageFromLibrary,
        },
        {
          text: t('profile_picture_take_photo'),
          onPress: takePictureWithCamera,
        },
        { text: t('profile_picture_cancel'), style: 'cancel' },
      ]
    );
  }, [t]);

  const pickImageFromLibrary = async () => {
    try {
      const permissionResult = await ImagePicker.requestMediaLibraryPermissionsAsync();
      if (!permissionResult.granted) {
        Alert.alert(t('error_title'), t('error_media_library_permission_denied'));
        return;
      }

      const result = await ImagePicker.launchImageLibraryAsync({
        mediaTypes: ImagePicker.MediaTypeOptions.Images,
        allowsEditing: true,
        aspect: [1, 1],
        quality: 0.7,
      });
      if (!result.canceled && result.assets[0].uri) {
        const resized = await resizeImage(result.assets[0].uri);
        handleUploadProfilePicture(resized.uri);
      }
    } catch (err) {
      console.error('pickImageFromLibrary error:', err);
      Alert.alert(t('error_title'), t('error_could_not_open_image_library'));
    }
  };

  const takePictureWithCamera = async () => {
    try {
      const cameraPermission = await ImagePicker.requestCameraPermissionsAsync();
      if (!cameraPermission.granted) {
        Alert.alert(t('error_title'), t('error_camera_permission_denied'));
        return;
      }
      const result = await ImagePicker.launchCameraAsync({
        allowsEditing: true,
        aspect: [1, 1],
        quality: 0.7,
      });
      if (!result.canceled && result.assets[0].uri) {
        const resized = await resizeImage(result.assets[0].uri);
        handleUploadProfilePicture(resized.uri);
      }
    } catch (err) {
      console.error('takePictureWithCamera error:', err);
      Alert.alert(t('error_title'), t('error_could_not_open_camera'));
    }
  };

  const resizeImage = async (uri: string) => {
    return await ImageManipulator.manipulateAsync(
      uri,
      [{ resize: { width: 800 } }],
      { compress: 0.7, format: ImageManipulator.SaveFormat.JPEG }
    );
  };

  const handleUploadProfilePicture = async (uri: string) => {
    try {
      const img = await resizeImage(uri);
      const photo = {
        uri: img.uri,
        name: 'profile.jpg',
        type: 'image/jpeg',
      } as any;
      updateProfilePicture({ image: photo });
    } catch (err) {
      console.error('Error preparing profile picture:', err);
      Alert.alert(t('error_title'), t('error_profile_picture_update_failed'));
    }
  };

  // ----------- Handle “Save” -----------
  const handleSave = async () => {
    setError(null);

    const payload: UserUpdateRequest = {
      name: name.trim(),
      bio: bio.trim(),
    };

    if (!payload.name) {
      setError(t('edit_profile_error_name_required'));
      return;
    }

    updateProfile(payload, {
      onError: (error: any) => {
        setError(error?.message || t('edit_profile_error_message'));
      },
      onSuccess: () => {
        onUpdated();
        onClose();
      },
    });
  };

  // ----------- Handle Location Modal -----------
  const handleOpenSetLocationModal = () => {
    log.debug('Opening location modal');
    log.debug('userProfile.locationLatitude', userProfile.locationLatitude);
    log.debug('userProfile.locationLongitude', userProfile.locationLongitude);
    setLocationModalVisible(true);
  };

  // ----------- Effect to Handle Mutation Errors -----------
  useEffect(() => {
    if (isUpdateProfileError && updateProfileError) {
      Alert.alert(t('error_title'), updateProfileError.message || t('error_update_profile_failed'));
    }
    if (isUpdatePictureError && updatePictureError) {
      Alert.alert(t('error_title'), updatePictureError.message || t('error_update_profile_picture_failed'));
    }
  }, [isUpdateProfileError, updateProfileError, isUpdatePictureError, updatePictureError, t]);

  return (
    <Modal visible={visible} animationType="fade" transparent>
      <View style={styles.modalOverlay}>
        {/* Card positioned absolutely where the user’s profile card was */}
        <View
          style={[
            profileCardStyles.profileCardContainer,
            {
              position: 'absolute',
              top: cardLayout.y,
              left: cardLayout.x,
              width: cardLayout.width,
            },
          ]}
        >
          <LinearGradient
            colors={[COLORS.cardBg1, COLORS.cardBg2]}
            style={profileCardStyles.profileCardInner}
          >
            {/* Top portion + background image + picture */}
            <View style={profileCardStyles.profileTopContainer}>
              <ImageBackground
                source={require('../../../../assets/images/profileBackground.png')}
                style={profileCardStyles.profileBackgroundImage}
              />
              <View style={profileCardStyles.profilePictureContainer}>
                {userProfile.profilePictureUrl ? (
                  <Image
                    source={{ uri: userProfile.profilePictureUrl }}
                    style={profileCardStyles.profilePicture}
                  />
                ) : (
                  <View style={profileCardStyles.profilePlaceholder}>
                    <Ionicons name="person-circle-outline" size={90} color="#ccc" />
                  </View>
                )}

                {/* Camera icon to change picture */}
                <TouchableOpacity
                  style={profileCardStyles.cameraIconWrapper}
                  onPress={handleChangeProfilePicture}
                  disabled={isUpdating}
                >
                  <Ionicons name="camera" size={18} color="#fff" />
                </TouchableOpacity>
              </View>

              {/* “Save” checkmark in the top-right corner */}
              <TouchableOpacity
                onPress={handleSave}
                style={profileCardStyles.profileEditButton}
                disabled={isUpdating}
              >
                {isUpdating ? (
                  <ActivityIndicator size="small" color={COLORS.textLight} />
                ) : (
                  <MaterialIcons name="check" size={20} color={COLORS.textLight} />
                )}
              </TouchableOpacity>
            </View>

            {/* Middle portion: name / location */}
            <View style={profileCardStyles.profileInfoContainer}>
              <View
                style={[
                  profileCardStyles.nameContainer,
                  profileCardStyles.editNameContainer,
                  { maxWidth: screenWidth * 0.9 - 215 },
                ]}
              >
                <TextInput
                  style={[
                    profileCardStyles.profileNameText,
                    profileCardStyles.editableTextName,
                  ]}
                  value={name}
                  maxLength={16}
                  onChangeText={setName}
                  placeholder={t('edit_profile_name_label')}
                  placeholderTextColor="#999"
                />
              </View>
              <View style={[profileCardStyles.profileLocationRow, { maxWidth: screenWidth * 0.9 - 215 }]}>
                {/* Tapping location opens the location modal */}
                <Ionicons
                  name="location-sharp"
                  size={16}
                  color={COLORS.accentLightRed}
                  style={profileCardStyles.locationIcon}
                />
                <TouchableOpacity onPress={handleOpenSetLocationModal} style={{ flex: 1 }}>
                  <Text
                    style={[
                      profileCardStyles.profileLocationText,
                      { textDecorationLine: 'underline', color: COLORS.accentRed },
                    ]}
                  >
                    {cityCountry || t('profile_no_location')}
                  </Text>
                </TouchableOpacity>
              </View>
            </View>

            {/* Bio: multiline text input */}
            <View
              style={[
                profileCardStyles.bioContainer,
                profileCardStyles.bioContainerEdit,
              ]}
            >
              <TextInput
                style={[
                  profileCardStyles.bioText,
                  profileCardStyles.editableTextBio,
                  !bio && profileCardStyles.bioPlaceholder,
                ]}
                multiline
                maxLength={1000}
                value={bio}
                onChangeText={setBio}
                placeholder={t('profile_no_bio_placeholder')}
                placeholderTextColor="#999"
              />
            </View>

            {/* Error Message */}
            {error && <Text style={profileCardStyles.errorText}>{error}</Text>}
          </LinearGradient>
        </View>
      </View>

      {/* Loading Indicator Overlay */}
      {isUpdating && (
        <View style={styles.loadingOverlay}>
          <ActivityIndicator size="large" color={COLORS.accentRed} />
        </View>
      )}

      {/* Location modal */}
      <ChangeLocationModal
        visible={locationModalVisible}
        initialLatitude={userProfile.locationLatitude}
        initialLongitude={userProfile.locationLongitude}
        onClose={() => setLocationModalVisible(false)}
        onUpdated={() => {
          setLocationModalVisible(false);
          onUpdated();
        }}
      />
    </Modal>
  );
};

export default EditProfileModal;

const styles = StyleSheet.create({
  modalOverlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.45)',
  },
  loadingOverlay: {
    ...StyleSheet.absoluteFillObject,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: 'rgba(0,0,0,0.5)',
  },
});
