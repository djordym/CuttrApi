import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  Modal,
  StyleSheet,
  FlatList,
  Image,
  TouchableOpacity,
  Alert,
  Platform,
} from 'react-native';
import { Plant } from '../types/types';
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

interface SelectPlantsModalProps {
  visible: boolean;
  onClose: () => void;
  ownedPlants: Plant[];
  onConfirm: (selectedPlants: Plant[]) => void;
}

const SelectPlantsModal: React.FC<SelectPlantsModalProps> = ({
  visible,
  onClose,
  ownedPlants,
  onConfirm,
}) => {
  const [selectedPlants, setSelectedPlants] = useState<Plant[]>([]);

  const [fontsLoaded] = useFonts({
    Montserrat_400Regular,
    Montserrat_700Bold,
  });

  useEffect(() => {
    if (fontsLoaded) {
      SplashScreen.hideAsync();
    }
    // Reset selected plants when modal is opened or closed
    if (!visible) {
      setSelectedPlants([]);
    }
  }, [fontsLoaded, visible]);

  const handlePlantSelection = (plant: Plant) => {
    setSelectedPlants((prevSelected) =>
      prevSelected.includes(plant)
        ? prevSelected.filter((p) => p.plantId !== plant.plantId)
        : [...prevSelected, plant]
    );
  };

  const handleConfirm = () => {
    if (selectedPlants.length === 0) {
      Alert.alert('Error', 'Please select at least one plant.');
      return;
    }
    onConfirm(selectedPlants);
  };

  if (!fontsLoaded) {
    return null;
  }

  return (
    <Modal visible={visible} animationType="slide" transparent={true} onRequestClose={onClose}>
      <View style={styles.modalOverlay}>
        <View style={styles.modalContainer}>
          <Text style={styles.modalTitle}>Select Plants</Text>
          <FlatList
            data={ownedPlants}
            keyExtractor={(item) => item.plantId.toString()}
            numColumns={3}
            contentContainerStyle={styles.flatListContent}
            renderItem={({ item }) => (
              <TouchableOpacity
                style={[
                  styles.plantContainer,
                  selectedPlants.includes(item) && styles.selectedPlantContainer,
                ]}
                onPress={() => handlePlantSelection(item)}
              >
                <Image source={{ uri: item.imageUrl }} style={styles.plantImage} />
                <Text style={styles.plantName}>{item.name}</Text>
                {selectedPlants.includes(item) && (
                  <Ionicons
                    name="checkmark-circle"
                    size={24}
                    color={COLORS.primary}
                    style={styles.checkIcon}
                  />
                )}
              </TouchableOpacity>
            )}
          />
          <View style={styles.buttonContainer}>
            <TouchableOpacity style={styles.cancelButton} onPress={onClose}>
              <Text style={styles.cancelButtonText}>Cancel</Text>
            </TouchableOpacity>
            <TouchableOpacity style={styles.confirmButton} onPress={handleConfirm}>
              <Text style={styles.confirmButtonText}>Confirm</Text>
            </TouchableOpacity>
          </View>
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
  modalContainer: {
    width: '90%',
    backgroundColor: COLORS.white,
    borderRadius: 15,
    paddingVertical: 20,
    paddingHorizontal: 15,
    maxHeight: '80%',
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
  modalTitle: {
    fontSize: 24,
    fontFamily: 'Montserrat_700Bold',
    color: COLORS.primary,
    textAlign: 'center',
    marginBottom: 15,
  },
  flatListContent: {
    paddingBottom: 20,
  },
  plantContainer: {
    flex: 1,
    margin: 5,
    alignItems: 'center',
    borderWidth: 2,
    borderColor: 'transparent',
    borderRadius: 15,
    padding: 10,
    position: 'relative',
  },
  selectedPlantContainer: {
    borderColor: COLORS.primary,
  },
  plantImage: {
    width: 80,
    height: 80,
    borderRadius: 10,
  },
  plantName: {
    textAlign: 'center',
    marginTop: 5,
    fontSize: 14,
    fontFamily: 'Montserrat_400Regular',
    color: COLORS.text,
  },
  checkIcon: {
    position: 'absolute',
    top: 5,
    right: 5,
  },
  buttonContainer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginTop: 15,
  },
  cancelButton: {
    backgroundColor: COLORS.accent,
    paddingVertical: 12,
    borderRadius: 10,
    alignItems: 'center',
    flex: 1,
    marginRight: 10,
  },
  cancelButtonText: {
    fontSize: 16,
    fontFamily: 'Montserrat_700Bold',
    color: COLORS.white,
  },
  confirmButton: {
    backgroundColor: COLORS.primary,
    paddingVertical: 12,
    borderRadius: 10,
    alignItems: 'center',
    flex: 1,
    marginLeft: 10,
  },
  confirmButtonText: {
    fontSize: 16,
    fontFamily: 'Montserrat_700Bold',
    color: COLORS.white,
  },
});

export default SelectPlantsModal;
