import React, { useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  ScrollView,
  Image,
  Alert,
  TextInput,
  ActivityIndicator,
  Platform,
} from 'react-native';
import { COLORS } from '../../../theme/colors';
import { LinearGradient } from 'expo-linear-gradient';
import { Ionicons, MaterialIcons } from '@expo/vector-icons';
import { useTranslation } from 'react-i18next';

import * as ImagePicker from 'expo-image-picker';
import * as ImageManipulator from 'expo-image-manipulator';

import { plantService } from '../../../api/plantService';
import {
  PlantCreateRequest,
  PlantRequest,
} from '../../../types/apiTypes';
import { useNavigation } from '@react-navigation/native';

import { useQueryClient } from 'react-query';

import {
  PlantCategory,
  PlantStage,
  WateringNeed,
  LightRequirement,
  Size,
  IndoorOutdoor,
  PropagationEase,
  PetFriendly,
  Extras,
} from '../../../types/enums';
import { SafeAreaProvider } from 'react-native-safe-area-context';

/**
 * Reusable component for single-select tags (with optional deselection).
 */
const SingleSelectTagGroup = <T extends string | number>({
  values,
  selectedValue,
  onSelect,
  isRequired,
}: {
  values: T[];
  selectedValue: T | null;
  onSelect: (val: T | null) => void;
  /**
   * If `isRequired` is true, we won't allow unselecting the already-selected tag.
   * If `isRequired` is false, tapping the same tag again unselects it (set to null).
   */
  isRequired?: boolean;
}) => {
  return (
    <View style={styles.tagGroupContainer}>
      {values.map((val) => {
        const isSelected = val === selectedValue;
        return (
          <TouchableOpacity
            key={String(val)}
            style={[
              styles.singleTag,
              isSelected && styles.singleTagSelected,
            ]}
            onPress={() => {
              if (isRequired) {
                // If required, once selected, we can't deselect to null.
                onSelect(val);
              } else {
                // If optional, allow tapping the same chip to unselect.
                onSelect(isSelected ? null : val);
              }
            }}
          >
            <Text
              style={[
                styles.singleTagText,
                isSelected && styles.singleTagTextSelected,
              ]}
            >
              {val}
            </Text>
          </TouchableOpacity>
        );
      })}
    </View>
  );
};

/**
 * AddPlantScreen
 * - speciesName (required)
 * - stage (required)
 * - image (required)
 * - All other fields optional, pass `null` if not selected.
 */
const AddPlantScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation();
  // Required fields
  const [speciesName, setSpeciesName] = useState('');
  const [stage, setStage] = useState<PlantStage | null>(null);
  const [image, setImage] = useState<any>(null);
  const queryClient = useQueryClient();

  // Optional fields
  const [category, setCategory] = useState<PlantCategory | null>(null);
  const [watering, setWatering] = useState<WateringNeed | null>(null);
  const [light, setLight] = useState<LightRequirement | null>(null);
  const [size, setSize] = useState<Size | null>(null);
  const [indoorOutdoor, setIndoorOutdoor] = useState<IndoorOutdoor | null>(null);
  const [propagationEase, setPropagationEase] = useState<PropagationEase | null>(
    null
  );
  const [petFriendly, setPetFriendly] = useState<PetFriendly | null>(null);
  const [selectedExtras, setSelectedExtras] = useState<Extras[]>([]);
  const [description, setDescription] = useState('');

  // UI
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  /**
   * handleSelectImageOption
   * Prompts the user for how to select an image (camera vs. library).
   */
  const handleSelectImageOption = async () => {
    Alert.alert(
      t('add_plant_select_image_title'),
      t('add_plant_select_image_desc'),
      [
        {
          text: t('add_plant_select_picture_button'),
          onPress: pickImageFromLibrary,
        },
        {
          text: t('add_plant_take_picture_button'),
          onPress: takePictureWithCamera,
        },
        {
          text: t('add_plant_cancel_button'),
          style: 'cancel',
        },
      ]
    );
  };

  /**
   * pickImageFromLibrary
   * Opens the photo library picker.
   */
  const pickImageFromLibrary = async () => {
    try {
      const result = await ImagePicker.launchImageLibraryAsync({
        mediaTypes: ImagePicker.MediaTypeOptions.Images,
        allowsEditing: true,
        aspect: [3, 4],
        quality: 0.7,
      });
      if (!result.canceled) {
        const resized = await resizeImage(result.assets[0].uri);
        setImage(resized);
      }
    } catch (err) {
      console.error('pickImageFromLibrary error:', err);
      Alert.alert('Error', 'Could not open image library.');
    }
  };

  /**
   * takePictureWithCamera
   * Opens the camera to take a new picture.
   */
  const takePictureWithCamera = async () => {
    try {
      const cameraPermission = await ImagePicker.requestCameraPermissionsAsync();
      if (!cameraPermission.granted) {
        Alert.alert('Error', 'Camera permission denied.');
        return;
      }
      const result = await ImagePicker.launchCameraAsync({
        allowsEditing: true,
        aspect: [3, 4],
        quality: 0.7,
      });
      if (!result.canceled) {
        const resized = await resizeImage(result.assets[0].uri);
        setImage(resized);
      }
    } catch (err) {
      console.error('takePictureWithCamera error:', err);
      Alert.alert('Error', 'Could not open camera.');
    }
  };

  /**
   * resizeImage
   * Resizes to width=800, compresses to ~70% quality, and saves as JPEG.
   */
  const resizeImage = async (uri: string) => {
    return await ImageManipulator.manipulateAsync(
      uri,
      [{ resize: { width: 800 } }],
      { compress: 0.7, format: ImageManipulator.SaveFormat.JPEG }
    );
  };

  /**
   * handleExtraToggle
   * Multi-select toggle for Extras.
   */
  const handleExtraToggle = (extra: Extras) => {
    setSelectedExtras((prev) =>
      prev.includes(extra)
        ? prev.filter((e) => e !== extra)
        : [...prev, extra]
    );
  };

  const isExtraSelected = (extra: Extras) => selectedExtras.includes(extra);
  const handleCancel = () => {
    navigation.goBack();
  }
  /**
   * handleSave
   * Validates required fields, constructs request, calls service.
   */
  const handleSave = async () => {
    // Required: speciesName, plantStage, image
    if (!speciesName.trim()) {
      Alert.alert('Validation Error', 'Species name is required.');
      return;
    }
    if (!stage) {
      Alert.alert('Validation Error', 'Plant stage is required.');
      return;
    }
    if (!image) {
      Alert.alert('Validation Error', 'An image is required.');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const plantRequest: PlantRequest = {
        // required
        speciesName: speciesName.trim(),
        plantStage: stage,

        // optional (null if empty)
        description: description.trim() ? description : null,
        plantCategory: category,
        wateringNeed: watering,
        lightRequirement: light,
        size: size,
        indoorOutdoor: indoorOutdoor,
        propagationEase: propagationEase,
        petFriendly: petFriendly,
        extras: selectedExtras, // if no extras selected, this will just be an empty array
      };

      // Construct the CreateRequest with an IFormFile (Image is required)
      const photo = {
        uri: image.uri,
        name: 'plant.jpg',
        type: 'image/jpeg',
      } as any;

      const plantCreateRequest: PlantCreateRequest = {
        plantDetails: plantRequest,
        image: photo,
      };

      await plantService.addMyPlant(plantCreateRequest);
      queryClient.invalidateQueries('myPlants');
      navigation.goBack();
    } catch (err) {
      console.error('Error adding plant:', err);
      setError(t('add_plant_error_message'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <View style={styles.container}>
      {/* Full Screen Gradient */}
      <LinearGradient
        colors={[COLORS.primary, COLORS.secondary]}
        style={styles.gradientBackground}
      >
          <View style={styles.headerRow}>
            <Text style={styles.headerTitle}>{t('add_plant_title')}</Text>
            <MaterialIcons name="local_florist" size={24} color="#fff" />
          </View>
        <ScrollView
          contentContainerStyle={styles.scrollContent}
          showsVerticalScrollIndicator={false}
        >

          <View style={styles.formContainer}>
            {error && <Text style={styles.errorText}>{error}</Text>}

            {/* SPECIES NAME (required) */}
            <Text style={styles.label}>{t('add_plant_species_name_label')}:</Text>
            <TextInput
              style={styles.input}
              value={speciesName}
              onChangeText={setSpeciesName}
              placeholder="e.g. Monstera Deliciosa"
            />

            {/* DESCRIPTION (optional) */}
            <Text style={styles.label}>{t('add_plant_description_label')}:</Text>
            <TextInput
              style={[styles.input, { height: 80 }]}
              value={description}
              onChangeText={setDescription}
              multiline
            />

            {/* PLANT STAGE (required) */}
            <Text style={styles.label}>{t('add_plant_stage_label')}:</Text>
            <SingleSelectTagGroup<PlantStage>
              values={Object.values(PlantStage)} // only actual stage values
              selectedValue={stage}
              onSelect={(val) => setStage(val)}
              isRequired={true} // once selected, can't unselect (it's required)
            />

            {/* CATEGORY (optional) */}
            <Text style={styles.label}>{t('add_plant_category_label')}:</Text>
            <SingleSelectTagGroup<PlantCategory>
              values={Object.values(PlantCategory)}
              selectedValue={category}
              onSelect={(val) => {
                // if user taps the same chip again => deselect
                setCategory(val === category ? null : val);
              }}
              isRequired={false} // optional => user can unselect
            />

            {/* WATERING NEED (optional) */}
            <Text style={styles.label}>{t('add_plant_watering_label')}:</Text>
            <SingleSelectTagGroup<WateringNeed>
              values={Object.values(WateringNeed)}
              selectedValue={watering}
              onSelect={(val) => setWatering(val === watering ? null : val)}
            />

            {/* LIGHT REQUIREMENT (optional) */}
            <Text style={styles.label}>{t('add_plant_light_label')}:</Text>
            <SingleSelectTagGroup<LightRequirement>
              values={Object.values(LightRequirement)}
              selectedValue={light}
              onSelect={(val) => setLight(val === light ? null : val)}
            />

            {/* SIZE (optional) */}
            <Text style={styles.label}>{t('add_plant_size_question')}:</Text>
            <SingleSelectTagGroup<Size>
              values={Object.values(Size)}
              selectedValue={size}
              onSelect={(val) => setSize(val === size ? null : val)}
            />

            {/* INDOOR/OUTDOOR (optional) */}
            <Text style={styles.label}>{t('add_plant_indoor_outdoor_question')}:</Text>
            <SingleSelectTagGroup<IndoorOutdoor>
              values={Object.values(IndoorOutdoor)}
              selectedValue={indoorOutdoor}
              onSelect={(val) => setIndoorOutdoor(val === indoorOutdoor ? null : val)}
            />

            {/* PROPAGATION EASE (optional) */}
            <Text style={styles.label}>{t('add_plant_propagation_ease_question')}:</Text>
            <SingleSelectTagGroup<PropagationEase>
              values={Object.values(PropagationEase)}
              selectedValue={propagationEase}
              onSelect={(val) => setPropagationEase(val === propagationEase ? null : val)}
            />

            {/* PET FRIENDLY (optional) */}
            <Text style={styles.label}>{t('add_plant_pet_friendly_question')}:</Text>
            <SingleSelectTagGroup<PetFriendly>
              values={Object.values(PetFriendly)}
              selectedValue={petFriendly}
              onSelect={(val) => setPetFriendly(val === petFriendly ? null : val)}
            />

            {/* EXTRAS (optional - MULTI-SELECT) */}
            <Text style={styles.label}>{t('add_plant_extras_question')}:</Text>
            <View style={styles.extrasContainer}>
              {Object.values(Extras).map((extra) => {
                const selected = isExtraSelected(extra);
                return (
                  <TouchableOpacity
                    key={extra}
                    style={[
                      styles.extraTag,
                      selected && styles.extraTagSelected,
                    ]}
                    onPress={() => handleExtraToggle(extra)}
                  >
                    <Text
                      style={[
                        styles.extraTagText,
                        selected && styles.extraTagTextSelected,
                      ]}
                    >
                      {extra}
                    </Text>
                  </TouchableOpacity>
                );
              })}
            </View>

            {/* IMAGE (required) */}
            <Text style={styles.label}>{t('add_plant_select_image_title')}:</Text>
            <TouchableOpacity style={styles.imageButton} onPress={handleSelectImageOption}>
              <Ionicons name="image" size={24} color="#fff" />
              <Text style={styles.imageButtonText}>
                {t('add_plant_select_image_title')}
              </Text>
            </TouchableOpacity>
            {image ? (
              <Image source={{ uri: image.uri }} style={styles.previewImage} />
            ) : (
              <Text style={styles.noImageText}>
                {t('add_plant_no_image_selected')}
              </Text>
            )}

            {loading && (
              <ActivityIndicator
                size="small"
                color={COLORS.primary}
                style={{ marginVertical: 10 }}
              />
            )}

            {/* ACTIONS */}
            <View style={styles.actions}>
              <TouchableOpacity
                style={styles.cancelButton}
                onPress={handleCancel}
              >
                <Text style={styles.cancelButtonText}>
                  {t('add_plant_cancel_button')}
                </Text>
              </TouchableOpacity>
              <TouchableOpacity style={styles.saveButton} onPress={handleSave}>
                <Text style={styles.saveButtonText}>
                  {t('add_plant_save_button')}
                </Text>
              </TouchableOpacity>
            </View>
          </View>
        </ScrollView>
      </LinearGradient>
    </View>
  );
};

export default AddPlantScreen;

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  gradientBackground: {
    flex: 1,
    
  },
  scrollContent: {
    paddingTop: 0,
    paddingBottom: 30,
  },
  headerRow: {
    position: 'sticky',
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginHorizontal: 20,
    marginBottom: 10,
  },
  headerTitle: {
    fontSize: 24,
    fontWeight: 'bold',
    color: COLORS.textLight,
  },
  formContainer: {
    backgroundColor: '#fff',
    marginHorizontal: 20,
    borderRadius: 12,
    padding: 16,
    ...Platform.select({
      ios: {
        shadowColor: '#000',
        shadowOpacity: 0.1,
        shadowRadius: 5,
        shadowOffset: { width: 0, height: 4 },
      },
      android: {
        elevation: 3,
      },
    }),
  },
  errorText: {
    color: COLORS.accent,
    marginBottom: 10,
    fontWeight: '600',
  },
  label: {
    fontSize: 14,
    fontWeight: '600',
    color: COLORS.textDark,
    marginTop: 12,
    marginBottom: 4,
  },
  input: {
    borderWidth: 1,
    borderColor: '#ccc',
    borderRadius: 8,
    paddingHorizontal: 10,
    paddingVertical: 8,
    fontSize: 14,
  },

  // SINGLE SELECT TAG GROUP
  tagGroupContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    marginBottom: 6,
  },
  singleTag: {
    borderWidth: 1,
    borderColor: COLORS.primary,
    borderRadius: 20,
    paddingVertical: 6,
    paddingHorizontal: 12,
    marginRight: 8,
    marginBottom: 8,
  },
  singleTagSelected: {
    backgroundColor: COLORS.primary,
  },
  singleTagText: {
    fontSize: 12,
    color: COLORS.primary,
  },
  singleTagTextSelected: {
    color: '#fff',
    fontWeight: '600',
  },

  // EXTRAS (MULTI-SELECT)
  extrasContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    marginTop: 4,
  },
  extraTag: {
    borderWidth: 1,
    borderColor: COLORS.primary,
    borderRadius: 20,
    paddingVertical: 6,
    paddingHorizontal: 12,
    marginRight: 8,
    marginBottom: 8,
  },
  extraTagSelected: {
    backgroundColor: COLORS.primary,
  },
  extraTagText: {
    fontSize: 12,
    color: COLORS.primary,
  },
  extraTagTextSelected: {
    color: '#fff',
    fontWeight: '600',
  },

  // IMAGE
  imageButton: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: COLORS.primary,
    padding: 10,
    borderRadius: 8,
    marginTop: 6,
  },
  imageButtonText: {
    fontSize: 14,
    color: '#fff',
    marginLeft: 6,
    fontWeight: '600',
  },
  previewImage: {
    width: '100%',
    aspectRatio: 3 / 4,
    borderRadius: 8,
    marginTop: 6,
    marginBottom: 10,
    resizeMode: 'cover',
  },
  noImageText: {
    fontSize: 14,
    color: '#555',
    marginTop: 6,
    marginBottom: 10,
  },

  // ACTIONS
  actions: {
    flexDirection: 'row',
    justifyContent: 'flex-end',
    marginTop: 16,
  },
  cancelButton: {
    borderWidth: 1,
    borderColor: COLORS.primary,
    borderRadius: 8,
    paddingVertical: 10,
    paddingHorizontal: 16,
    marginRight: 10,
  },
  cancelButtonText: {
    fontSize: 14,
    color: COLORS.primary,
    fontWeight: '600',
  },
  saveButton: {
    backgroundColor: COLORS.primary,
    borderRadius: 8,
    paddingVertical: 10,
    paddingHorizontal: 16,
  },
  saveButtonText: {
    fontSize: 14,
    color: '#fff',
    fontWeight: '600',
  },
});
