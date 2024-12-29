import React, { useState } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, TextInput, ActivityIndicator, ScrollView, Image, Alert } from 'react-native';
import { useTranslation } from 'react-i18next';
import { Ionicons } from '@expo/vector-icons';
import * as ImagePicker from 'expo-image-picker';
import { plantService } from '../../../api/plantService';
import { PlantCreateRequest, PlantRequest } from '../../../types/apiTypes';

import { PlantCategory, PlantStage, WateringNeed, LightRequirement } from '../../../types/enums';

const AddPlantScreen: React.FC = () => {
  const { t } = useTranslation();
  const [speciesName, setSpeciesName] = useState('');
  const [description, setDescription] = useState('');
  
  // For simplicity, we'll just have text inputs for the enums. In a real app, use dropdowns or pickers:
  const [stage, setStage] = useState<PlantStage>(PlantStage.Cutting);
  const [category, setCategory] = useState<PlantCategory>(PlantCategory.Other);
  const [watering, setWatering] = useState<WateringNeed>(WateringNeed.ModerateWater);
  const [light, setLight] = useState<LightRequirement>(LightRequirement.PartialShade);

  const [image, setImage] = useState<any>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSelectImage = async () => {
    let result = await ImagePicker.launchImageLibraryAsync({
        mediaTypes: ImagePicker.MediaTypeOptions.Images,
        allowsEditing: true,
        quality: 0.7, // Reduce quality to limit size
        aspect: [3, 4],
      });
      
      if (!result.canceled) {
      setImage(result.assets[0]);
    }
  };

  const handleSave = async () => {
    if (!speciesName || !description || !image) {
      Alert.alert("Error", "Please fill all required fields and select an image.");
      return;
    }
    setLoading(true);
    setError(null);

    const formData = new FormData();
    formData.append('image', {
      uri: image,
      name: 'plant.jpg',
      type: 'image/jpeg'
    } as any);

    formData.append('speciesName', speciesName);
    formData.append('description', description);
    formData.append('plantStage', stage.toString());
    formData.append('plantCategory', category.toString());
    formData.append('wateringNeed', watering.toString());
    formData.append('lightRequirement', light.toString());

    try {
      await plantService.addMyPlant({ plantDetails: { speciesName: speciesName, description: description, plantStage: stage, plantCategory: category, wateringNeed: watering, lightRequirement: light }, image: formData.get('image') as any });
      Alert.alert("Success", "Plant added successfully.");
    } catch {
      setError(t('add_plant_error_message'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <ScrollView style={styles.container} contentContainerStyle={{padding:20}}>
      <Text style={styles.title}>{t('add_plant_title')}</Text>
      {error && <Text style={styles.errorText}>{error}</Text>}

      <Text style={styles.label}>{t('add_plant_species_name_label')}:</Text>
      <TextInput style={styles.input} value={speciesName} onChangeText={setSpeciesName} />

      <Text style={styles.label}>{t('add_plant_description_label')}:</Text>
      <TextInput style={[styles.input, {height:80}]} value={description} onChangeText={setDescription} multiline />

      <Text style={styles.label}>{t('add_plant_stage_label')}:</Text>
      <TextInput style={styles.input} value={PlantStage[stage]} onChangeText={(txt) => setStage(PlantStage[txt as keyof typeof PlantStage] || PlantStage.Cutting)} />

      <Text style={styles.label}>{t('add_plant_category_label')}:</Text>
      <TextInput style={styles.input} value={PlantCategory[category]} onChangeText={(txt) => setCategory(PlantCategory[txt as keyof typeof PlantCategory] || PlantCategory.Other)} />

      <Text style={styles.label}>{t('add_plant_watering_label')}:</Text>
      <TextInput style={styles.input} value={WateringNeed[watering]} onChangeText={(txt) => setWatering(WateringNeed[txt as keyof typeof WateringNeed] || WateringNeed.ModerateWater)} />

      <Text style={styles.label}>{t('add_plant_light_label')}:</Text>
      <TextInput style={styles.input} value={LightRequirement[light]} onChangeText={(txt) => setLight(LightRequirement[txt as keyof typeof LightRequirement] || LightRequirement.PartialShade)} />

      <Text style={styles.label}>{t('add_plant_select_image_button')}:</Text>
      <TouchableOpacity onPress={handleSelectImage} style={styles.imageButton}>
        <Ionicons name="image" size={24} color="#fff" />
        <Text style={styles.imageButtonText}>{t('add_plant_select_image_button')}</Text>
      </TouchableOpacity>
      {image ? (
        <Image source={{ uri: image }} style={styles.previewImage} />
      ) : (
        <Text style={styles.noImageText}>{t('add_plant_no_image_selected')}</Text>
      )}

      {loading && <ActivityIndicator size="small" color="#1EAE98" style={{marginVertical:10}}/>}

      <View style={styles.actions}>
        <TouchableOpacity style={styles.cancelButton} onPress={() => Alert.alert("Canceled", "Plant addition canceled.")}>
          <Text style={styles.cancelButtonText}>{t('add_plant_cancel_button')}</Text>
        </TouchableOpacity>
        <TouchableOpacity style={styles.saveButton} onPress={handleSave}>
          <Text style={styles.saveButtonText}>{t('add_plant_save_button')}</Text>
        </TouchableOpacity>
      </View>

    </ScrollView>
  );
};

export default AddPlantScreen;

const styles = StyleSheet.create({
  container:{
    flex:1,
    backgroundColor:'#f8f8f8'
  },
  title:{
    fontSize:20,
    fontWeight:'700',
    color:'#333',
    marginBottom:20
  },
  errorText:{
    color:'#FF6B6B',
    marginBottom:10
  },
  label:{
    fontSize:14,
    fontWeight:'600',
    color:'#333',
    marginBottom:4
  },
  input:{
    borderWidth:1,
    borderColor:'#ccc',
    borderRadius:8,
    padding:10,
    marginBottom:10,
    fontSize:14,
    backgroundColor:'#fff'
  },
  imageButton:{
    flexDirection:'row',
    alignItems:'center',
    backgroundColor:'#1EAE98',
    padding:10,
    borderRadius:8,
    marginBottom:10
  },
  imageButtonText:{
    color:'#fff',
    fontSize:14,
    marginLeft:5
  },
  previewImage:{
    width:'100%',
    height:200,
    resizeMode:'cover',
    borderRadius:8,
    marginBottom:10
  },
  noImageText:{
    fontSize:14,
    color:'#555',
    marginBottom:10
  },
  actions:{
    flexDirection:'row',
    justifyContent:'flex-end',
    marginTop:10
  },
  cancelButton:{
    marginRight:10
  },
  cancelButtonText:{
    fontSize:16,
    color:'#333'
  },
  saveButton:{
    backgroundColor:'#1EAE98',
    paddingHorizontal:15,
    paddingVertical:10,
    borderRadius:8
  },
  saveButtonText:{
    fontSize:16,
    color:'#fff',
    fontWeight:'600'
  }
});
