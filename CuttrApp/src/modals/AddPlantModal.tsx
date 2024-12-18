import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  TextInput,
  StyleSheet,
  Image,
  Modal,
  TouchableOpacity,
  Alert,
  ScrollView,
  Platform,
} from 'react-native';
import * as ImagePicker from 'expo-image-picker';
import { Ionicons } from '@expo/vector-icons';
import { useFonts, Montserrat_400Regular, Montserrat_700Bold } from '@expo-google-fonts/montserrat';
import * as SplashScreen from 'expo-splash-screen';

const COLORS = {
  primary: '#3EB489',
  background: '#F2F2F2',
  accent: '#FF6F61',
  text: '#2F4F4F',
  white: '#FFFFFF',
};

SplashScreen.preventAutoHideAsync();

const AddPlantModal = ({
  visible,
  onClose,
  onAddPlant,
}: {
  visible: boolean;
  onClose: () => void;
  onAddPlant: (
    plantinfo: { name: string; description: string; imageUrl: string },
    asset: any
  ) => void;
}) => {
  const [plantName, setPlantName] = useState('');
  const [plantDescription, setPlantDescription] = useState('');
  const [plantImage, setPlantImage] = useState('');
  const [asset, setAsset] = useState<any>(null);

  const [fontsLoaded] = useFonts({
    Montserrat_400Regular,
    Montserrat_700Bold,
  });

  useEffect(() => {
    if (fontsLoaded) {
      SplashScreen.hideAsync();
    }
  }, [fontsLoaded]);

  const handleImagePick = async () => {
    // Request permission to access media library
    const { status } = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (status !== 'granted') {
      Alert.alert('Permission Denied', 'Permission to access media library is required!');
      return;
    }

    let result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ImagePicker.MediaTypeOptions.Images,
      allowsEditing: true,
      quality: 0.7, // Reduce quality to limit size
      aspect: [3, 4],
    });

    if (!result.canceled && result.assets && result.assets.length > 0) {
      setPlantImage(result.assets[0].uri);
      setAsset(result.assets[0]);
    }
  };

  const handleAddPlant = () => {
    if (!plantName || !plantDescription || !plantImage) {
      Alert.alert('Error', 'Please fill all fields and select an image.');
      return;
    }
    onAddPlant({ name: plantName, description: plantDescription, imageUrl: plantImage }, asset);
    setPlantName('');
    setPlantDescription('');
    setPlantImage('');
    onClose();
  };

  if (!fontsLoaded) {
    return null;
  }

  return (
    <Modal animationType="slide" transparent={true} visible={visible} onRequestClose={onClose}>
      <View style={styles.modalOverlay}>
        <View style={styles.modalView}>
          <Text style={styles.modalTitle}>Add New Plant</Text>
          <ScrollView contentContainerStyle={styles.scrollContent}>
            <TextInput
              style={styles.input}
              placeholder="Plant Name"
              value={plantName}
              onChangeText={setPlantName}
              placeholderTextColor="#888"
            />
            <TextInput
              style={[styles.input, styles.descriptionInput]}
              placeholder="Plant Description"
              value={plantDescription}
              onChangeText={setPlantDescription}
              placeholderTextColor="#888"
              multiline
            />
            <TouchableOpacity style={styles.imagePicker} onPress={handleImagePick}>
              {plantImage ? (
                <Image source={{ uri: plantImage }} style={styles.imagePreview} />
              ) : (
                <Ionicons name="camera" size={50} color={COLORS.primary} />
              )}
            </TouchableOpacity>
            <Text style={styles.imageText}>
              {plantImage ? 'Tap to change image' : 'Tap to select image'}
            </Text>
            <TouchableOpacity style={styles.addButton} onPress={handleAddPlant}>
              <Text style={styles.addButtonText}>Add Plant</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.cancelButton} onPress={onClose}>
              <Text style={styles.cancelButtonText}>Cancel</Text>
            </TouchableOpacity>
          </ScrollView>
        </View>
      </View>
    </Modal>
  );
};

const styles = StyleSheet.create({
  modalOverlay: {
    flex: 1,
    backgroundColor: 'rgba(0, 0, 0, 0.5)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  modalView: {
    width: '90%',
    backgroundColor: COLORS.white,
    borderRadius: 15,
    paddingVertical: 20,
    paddingHorizontal: 20,
    ...Platform.select({
      ios: {
        shadowColor: '#000',
        shadowOpacity: 0.2,
        shadowRadius: 5,
      },
      android: {
        elevation: 5,
      },
    }),
  },
  scrollContent: {
    alignItems: 'center',
  },
  modalTitle: {
    fontSize: 24,
    fontFamily: 'Montserrat_700Bold',
    color: COLORS.primary,
    textAlign: 'center',
    marginBottom: 20,
  },
  input: {
    width: '100%',
    borderWidth: 1,
    borderColor: COLORS.primary,
    borderRadius: 10,
    padding: 15,
    marginBottom: 15,
    backgroundColor: COLORS.white,
    fontSize: 16,
    fontFamily: 'Montserrat_400Regular',
    color: COLORS.text,
  },
  descriptionInput: {
    height: 100,
    textAlignVertical: 'top',
  },
  imagePicker: {
    width: 120,
    height: 160,
    borderRadius: 15,
    borderWidth: 1,
    borderColor: COLORS.primary,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: COLORS.background,
    marginBottom: 10,
  },
  imagePreview: {
    width: '100%',
    height: '100%',
    borderRadius: 15,
  },
  imageText: {
    fontSize: 14,
    fontFamily: 'Montserrat_400Regular',
    color: COLORS.text,
    marginBottom: 20,
  },
  addButton: {
    backgroundColor: COLORS.primary,
    paddingVertical: 15,
    borderRadius: 10,
    alignItems: 'center',
    width: '100%',
    marginBottom: 10,
  },
  addButtonText: {
    fontSize: 18,
    fontFamily: 'Montserrat_700Bold',
    color: COLORS.white,
  },
  cancelButton: {
    backgroundColor: COLORS.accent,
    paddingVertical: 15,
    borderRadius: 10,
    alignItems: 'center',
    width: '100%',
  },
  cancelButtonText: {
    fontSize: 18,
    fontFamily: 'Montserrat_700Bold',
    color: COLORS.white,
  },
});

export default AddPlantModal;
