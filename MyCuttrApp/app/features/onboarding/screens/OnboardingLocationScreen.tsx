// File: app/features/onboarding/screens/OnboardingLocationScreen.tsx
import React, { useState } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Alert, Dimensions } from 'react-native';
import MapView, { Marker, MapPressEvent } from 'react-native-maps';
import { userService } from '../../../api/userService';
import { useUpdateLocation } from '../../main/hooks/useMyProfileHooks';


const { width, height } = Dimensions.get('window');

const OnboardingLocationScreen: React.FC = () => {
  // **New Hook: Update User Location**
  const updateLocation = useUpdateLocation();
  
  // Some default region or your user’s approximate location
  const [region, setRegion] = useState({
    latitude: 37.78825, // Example lat
    longitude: -122.4324, // Example lng
    latitudeDelta: 0.0922,
    longitudeDelta: 0.0421,
  });

  const [selectedLocation, setSelectedLocation] = useState<{
    latitude: number;
    longitude: number;
  } | null>(null);

  const handleMapPress = (e: MapPressEvent) => {
    const { latitude, longitude } = e.nativeEvent.coordinate;
    setSelectedLocation({ latitude, longitude });
    setRegion((prev) => ({
      ...prev,
      latitude,
      longitude,
    }));
  };

  const handleConfirmLocation = async () => {
    if (!selectedLocation) {
      Alert.alert('No location selected', 'Please tap on the map to select your approximate location');
      return;
    }
    try {
      await updateLocation.mutateAsync({
        latitude: selectedLocation.latitude,
        longitude: selectedLocation.longitude,
      });
      
    } catch (error) {
      Alert.alert('Error', 'Failed to update location. Please try again.');
    }
  };

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Set Your Location</Text>
      <Text style={styles.subtitle}>
        Tap on the map to drop a pin at your approximate location.
      </Text>

      <MapView
        style={styles.map}
        initialRegion={region}
        onPress={handleMapPress}
      >
        {selectedLocation && (
          <Marker coordinate={selectedLocation} />
        )}
      </MapView>

      <TouchableOpacity onPress={handleConfirmLocation} style={styles.confirmButton}>
        <Text style={styles.confirmButtonText}>Confirm & Continue</Text>
      </TouchableOpacity>
    </View>
  );
};

export default OnboardingLocationScreen;

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'flex-start',
    paddingTop: 50,
    backgroundColor: '#fff',
  },
  title: {
    fontSize: 20,
    fontWeight: '700',
    marginHorizontal: 20,
    marginBottom: 8,
  },
  subtitle: {
    fontSize: 14,
    marginHorizontal: 20,
    marginBottom: 16,
    color: '#555',
  },
  map: {
    width: width,
    height: height * 0.55,
  },
  confirmButton: {
    backgroundColor: '#1EAE98',
    padding: 16,
    margin: 20,
    borderRadius: 8,
  },
  confirmButtonText: {
    color: '#fff',
    fontWeight: '600',
    textAlign: 'center',
  },
});
