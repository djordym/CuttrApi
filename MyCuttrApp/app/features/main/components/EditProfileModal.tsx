import React, { useState, useCallback } from 'react';
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
  Platform,
} from 'react-native';
import { useTranslation } from 'react-i18next';
import { Ionicons } from '@expo/vector-icons';
import * as ImagePicker from 'expo-image-picker';
import * as ImageManipulator from 'expo-image-manipulator';
import { userService } from '../../../api/userService';
import { UserResponse, UserUpdateRequest } from '../../../types/apiTypes';
import { ChangeLocationModal } from './ChangeLocationModal';
import { COLORS } from '../../../theme/colors';

const { width } = Dimensions.get('window');

interface EditProfileModalProps {
  visible: boolean;
  userProfile: UserResponse;
  onClose: () => void;
  onUpdated: () => void; // callback after successful update (refetch in parent)
}

export const EditProfileModal: React.FC<EditProfileModalProps> = ({
  visible,
  userProfile,
  onClose,
  onUpdated,
}) => {
  const { t } = useTranslation();

  // Local states for name & bio
  const [name, setName] = useState(userProfile.name);
  const [bio, setBio] = useState(userProfile.bio || '');
  const [updating, setUpdating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Location modal
  const [locationModalVisible, setLocationModalVisible] = useState(false);

  // Handle “Change Picture”
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
    setUpdating(true);
    try {
      const photo = {
        uri: img.uri,
        name: 'profile.jpg',
        type: 'image/jpeg',
      } as any;
      await userService.updateProfilePicture({ image: photo });
      onUpdated();
    } catch (err) {
      console.error('Error uploading profile picture:', err);
      Alert.alert('Error', 'Profile picture update failed.');
    } finally {
      setUpdating(false);
    }
  };

  // Handle “Go Back” (auto-save name & bio)
  const handleGoBack = async () => {
    setUpdating(true);
    setError(null);

    const payload: UserUpdateRequest = {
      name: name.trim(),
      bio: bio.trim(),
    };

    try {
      await userService.updateMe(payload);
      onUpdated();
      onClose();
    } catch {
      setError(t('edit_profile_error_message'));
    } finally {
      setUpdating(false);
    }
  };

  return (
    <Modal visible={visible} animationType="fade" transparent>
      <View style={styles.overlay}>
        {/* This container visually replicates the “profile card” style */}
        <View style={styles.modalCard}>
          {/* Scrollable content if you have smaller screens */}
          <View style={styles.contentWrapper}>
            {/* Top portion: background accent + “profile pic” */}
            <View style={styles.modalTopContainer}>
              <View style={styles.modalPictureContainer}>
                {userProfile.profilePictureUrl ? (
                  <Image
                    source={{ uri: userProfile.profilePictureUrl }}
                    style={styles.modalPicture}
                  />
                ) : (
                  <View style={styles.modalPlaceholder}>
                    <Ionicons name="person-circle-outline" size={90} color="#ccc" />
                  </View>
                )}
                {/* Button to change picture */}
                <TouchableOpacity
                  style={styles.cameraIconWrapper}
                  onPress={handleChangeProfilePicture}
                >
                  <Ionicons name="camera" size={18} color="#fff" />
                </TouchableOpacity>
              </View>
            </View>

            {/* Middle portion: name, bio, location line */}
            <View style={styles.modalBody}>
              {/* Name input */}
              <Text style={styles.label}>{t('edit_profile_name_label')}</Text>
              <TextInput
                style={styles.input}
                value={name}
                onChangeText={setName}
                accessibilityLabel={t('edit_profile_name_label')}
              />

              {/* Bio input */}
              <Text style={styles.label}>{t('edit_profile_bio_label')}</Text>
              <TextInput
                style={[styles.input, { height: 80 }]}
                value={bio}
                onChangeText={setBio}
                multiline
                accessibilityLabel={t('edit_profile_bio_label')}
              />

              {/* Show any error here */}
              {error && <Text style={styles.errorText}>{error}</Text>}

              {/* “Change Location” button */}
              <View style={styles.locationContainer}>
                <Ionicons name="location-outline" size={18} color={COLORS.accentGreen} />
                <TouchableOpacity
                  onPress={() => setLocationModalVisible(true)}
                  style={styles.locationButton}
                >
                  <Text style={styles.locationButtonText}>
                    {t('profile_change_location_button')}
                  </Text>
                </TouchableOpacity>
              </View>
            </View>
          </View>

          {/* Updating indicator */}
          {updating && (
            <ActivityIndicator
              size="small"
              color={COLORS.accentGreen}
              style={{ marginTop: 10 }}
            />
          )}

          {/* “Go Back” = auto-save */}
          <TouchableOpacity
            style={styles.goBackButton}
            onPress={handleGoBack}
            disabled={updating}
          >
            <Ionicons name="arrow-back-outline" size={18} color="#fff" />
            <Text style={styles.goBackButtonText}>{t('go_back_button_label')}</Text>
          </TouchableOpacity>
        </View>
      </View>

      {/* Nested ChangeLocationModal */}
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

const styles = StyleSheet.create({
  overlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.45)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  modalCard: {
    width: '90%',
    borderRadius: 16,
    backgroundColor: '#FFF',
    overflow: 'hidden',
    ...Platform.select({
      ios: {
        shadowColor: '#000',
        shadowOpacity: 0.25,
        shadowRadius: 6,
        shadowOffset: { width: 0, height: 2 },
      },
      android: {
        elevation: 5,
      },
    }),
    alignItems: 'center',
    paddingBottom: 16,
  },
  contentWrapper: {
    width: '100%',
  },
  modalTopContainer: {
    backgroundColor: COLORS.primary,
    height: 100,
    justifyContent: 'center',
    alignItems: 'center',
    position: 'relative',
  },
  modalPictureContainer: {
    position: 'absolute',
    bottom: -40,
  },
  modalPicture: {
    width: 80,
    height: 80,
    borderRadius: 40,
    backgroundColor: '#eee',
    borderWidth: 3,
    borderColor: '#fff',
  },
  modalPlaceholder: {
    width: 80,
    height: 80,
    borderRadius: 40,
    backgroundColor: '#eee',
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 3,
    borderColor: '#fff',
  },
  cameraIconWrapper: {
    position: 'absolute',
    bottom: 0,
    right: -10,
    backgroundColor: COLORS.accentGreen,
    borderRadius: 16,
    padding: 6,
  },
  modalBody: {
    marginTop: 50,
    paddingHorizontal: 20,
    paddingTop: 8,
  },
  label: {
    fontSize: 14,
    fontWeight: '600',
    color: COLORS.textDark,
    marginBottom: 4,
  },
  input: {
    borderWidth: 1,
    borderColor: '#ccc',
    borderRadius: 8,
    padding: 10,
    marginBottom: 16,
    fontSize: 14,
  },
  errorText: {
    color: '#FF6B6B',
    marginBottom: 10,
    textAlign: 'center',
  },
  locationContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginTop: 6,
    marginBottom: 10,
  },
  locationButton: {
    marginLeft: 6,
    borderWidth: 1,
    borderColor: COLORS.accentGreen,
    borderRadius: 8,
    paddingHorizontal: 10,
    paddingVertical: 4,
  },
  locationButtonText: {
    color: COLORS.accentGreen,
    fontSize: 14,
    fontWeight: '600',
  },
  goBackButton: {
    marginTop: 10,
    backgroundColor: COLORS.accentGreen,
    borderRadius: 20,
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 16,
    paddingVertical: 8,
  },
  goBackButtonText: {
    marginLeft: 6,
    color: '#fff',
    fontWeight: '600',
    fontSize: 14,
  },
});
