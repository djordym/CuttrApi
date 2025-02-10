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
  Platform,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { Ionicons, MaterialIcons } from '@expo/vector-icons';
import { useTranslation } from 'react-i18next';
import { useNavigation } from '@react-navigation/native';
import { useQueryClient } from 'react-query';
import * as ImagePicker from 'expo-image-picker';
import * as ImageManipulator from 'expo-image-manipulator';

import TagGroup from '../components/TagGroup';
import ConfirmCancelButtons from '../components/ConfirmCancelButtons';
import { COLORS } from '../../../theme/colors';
import { headerStyles } from '../styles/headerStyles';

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
import {
  PlantCreateRequest,
  PlantRequest,
} from '../../../types/apiTypes';
import { plantService } from '../../../api/plantService';

const AddPlantScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation();
  const queryClient = useQueryClient();

  const [speciesName, setSpeciesName] = useState('');
  const [stage, setStage] = useState<PlantStage | null>(null);
  const [image, setImage] = useState<any>(null);

  const [category, setCategory] = useState<PlantCategory | null>(null);
  const [watering, setWatering] = useState<WateringNeed | null>(null);
  const [light, setLight] = useState<LightRequirement | null>(null);
  const [size, setSize] = useState<Size | null>(null);
  const [indoorOutdoor, setIndoorOutdoor] = useState<IndoorOutdoor | null>(null);
  const [propagationEase, setPropagationEase] = useState<PropagationEase | null>(null);
  const [petFriendly, setPetFriendly] = useState<PetFriendly | null>(null);
  const [selectedExtras, setSelectedExtras] = useState<Extras[]>([]);
  const [description, setDescription] = useState('');

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

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

  const resizeImage = async (uri: string) => {
    return await ImageManipulator.manipulateAsync(
      uri,
      [{ resize: { width: 800 } }],
      { compress: 0.7, format: ImageManipulator.SaveFormat.JPEG }
    );
  };

  const handleExtraToggle = (extra: Extras) => {
    setSelectedExtras((prev) =>
      prev.includes(extra)
        ? prev.filter((e) => e !== extra)
        : [...prev, extra]
    );
  };

  const handleCancel = () => {
    navigation.goBack();
  };

  const handleSave = async () => {
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
        speciesName: speciesName.trim(),
        plantStage: stage,
        description: description.trim() ? description : null,
        plantCategory: category,
        wateringNeed: watering,
        lightRequirement: light,
        size: size,
        indoorOutdoor: indoorOutdoor,
        propagationEase: propagationEase,
        petFriendly: petFriendly,
        extras: selectedExtras,
      };

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
      <LinearGradient
        colors={[COLORS.primary, COLORS.secondary]}
        style={styles.gradientBackground}
      >
        <View style={headerStyles.headerAboveScroll}>
          <View style={headerStyles.headerColumn1}>
            <Ionicons
              name="chevron-back"
              size={30}
              color={COLORS.textLight}
              style={headerStyles.headerBackButton}
              onPress={() => navigation.goBack()}
            />
            <Text style={headerStyles.headerTitle}>{t('add_plant_title')}</Text>
          </View>
          <MaterialIcons name="info" size={24} color="#fff" />
        </View>
        <ScrollView
          contentContainerStyle={styles.scrollContent}
          showsVerticalScrollIndicator={false}
        >
          <View style={styles.formContainer}>
            {error && <Text style={styles.errorText}>{error}</Text>}

            <Text style={styles.label}>{t('add_plant_species_name_label')}:</Text>
            <TextInput
              style={styles.input}
              value={speciesName}
              onChangeText={setSpeciesName}
              placeholder="e.g. Monstera Deliciosa"
            />

            <Text style={styles.label}>{t('add_plant_description_label')}:</Text>
            <TextInput
              style={[styles.input, { height: 80 }]}
              value={description}
              onChangeText={setDescription}
              multiline
            />

            <Text style={styles.label}>{t('add_plant_stage_label')}:</Text>
            <TagGroup
              mode="single"
              values={Object.values(PlantStage)}
              selectedValue={stage}
              onSelectSingle={(val) => setStage(val)}
              isRequired={true}
              getLabel={(val) => t(`plantStage.${val}`)}
            />

            <Text style={styles.label}>{t('add_plant_category_label')}:</Text>
            <TagGroup
              mode="single"
              values={Object.values(PlantCategory)}
              selectedValue={category}
              onSelectSingle={(val) => setCategory(val === category ? null : val)}
              isRequired={false}
              getLabel={(val) => t(`plantCategory.${val}`)}
            />

            <Text style={styles.label}>{t('add_plant_watering_label')}:</Text>
            <TagGroup
              mode="single"
              values={Object.values(WateringNeed)}
              selectedValue={watering}
              onSelectSingle={(val) => setWatering(val === watering ? null : val)}
              isRequired={false}
              getLabel={(val) => t(`wateringNeed.${val}`)}
            />

            <Text style={styles.label}>{t('add_plant_light_label')}:</Text>
            <TagGroup
              mode="single"
              values={Object.values(LightRequirement)}
              selectedValue={light}
              onSelectSingle={(val) => setLight(val === light ? null : val)}
              isRequired={false}
              getLabel={(val) => t(`lightRequirement.${val}`)}
            />

            <Text style={styles.label}>{t('add_plant_size_question')}:</Text>
            <TagGroup
              mode="single"
              values={Object.values(Size)}
              selectedValue={size}
              onSelectSingle={(val) => setSize(val === size ? null : val)}
              isRequired={false}
              getLabel={(val) => t(`size.${val}`)}
            />

            <Text style={styles.label}>{t('add_plant_indoor_outdoor_question')}:</Text>
            <TagGroup
              mode="single"
              values={Object.values(IndoorOutdoor)}
              selectedValue={indoorOutdoor}
              onSelectSingle={(val) => setIndoorOutdoor(val === indoorOutdoor ? null : val)}
              isRequired={false}
              getLabel={(val) => t(`indoorOutdoor.${val}`)}
            />

            <Text style={styles.label}>{t('add_plant_propagation_ease_question')}:</Text>
            <TagGroup
              mode="single"
              values={Object.values(PropagationEase)}
              selectedValue={propagationEase}
              onSelectSingle={(val) => setPropagationEase(val === propagationEase ? null : val)}
              isRequired={false}
              getLabel={(val) => t(`propagationEase.${val}`)}
            />

            <Text style={styles.label}>{t('add_plant_pet_friendly_question')}:</Text>
            <TagGroup
              mode="single"
              values={Object.values(PetFriendly)}
              selectedValue={petFriendly}
              onSelectSingle={(val) => setPetFriendly(val === petFriendly ? null : val)}
              isRequired={false}
              getLabel={(val) => t(`petFriendly.${val}`)}
            />

            <Text style={styles.label}>{t('add_plant_extras_question')}:</Text>
            <TagGroup
              mode="multiple"
              values={Object.values(Extras)}
              selectedValues={selectedExtras}
              onToggleMulti={(val) => handleExtraToggle(val)}
              getLabel={(val) => t(`extras.${val}`)}
            />

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

            <ConfirmCancelButtons
              onConfirm={handleSave}
              onCancel={handleCancel}
              confirmButtonText={t('add_plant_save_button')}
              cancelButtonText={t('add_plant_cancel_button')}
              loading={loading}
            />
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
    color: COLORS.textDark,
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
  imageButton: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: COLORS.accentLightRed,
    padding: 10,
    borderRadius: 8,
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
    marginTop: 3,
    marginLeft: 13,
    marginBottom: 15,
  },
});
