// File: app/features/onboarding/screens/OnboardingLocationScreen.tsx
import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  Alert,
  Dimensions,
} from 'react-native';
import MapView, { Marker, MapPressEvent, Region } from 'react-native-maps';
import * as Location from 'expo-location';
import { LinearGradient } from 'expo-linear-gradient';
import { useNavigation } from '@react-navigation/native';
import { COLORS } from '../../../theme/colors';
import { useUpdateLocation } from '../../main/hooks/useMyProfileHooks';

const { width, height } = Dimensions.get('window');

const OnboardingLocationScreen: React.FC = () => {
  const navigation = useNavigation();
  const updateLocation = useUpdateLocation();

  const [region, setRegion] = useState<Region | null>(null);
  const [selectedLocation, setSelectedLocation] = useState<{
    latitude: number;
    longitude: number;
  } | null>(null);
  const [permissionStatus, setPermissionStatus] =
    useState<Location.PermissionStatus | null>(null);

  // Request permission and possibly fetch current location on mount.
  useEffect(() => {
    (async () => {
      const { status } = await Location.requestForegroundPermissionsAsync();
      setPermissionStatus(status);

      if (status === Location.PermissionStatus.GRANTED) {
        try {
          const loc = await Location.getCurrentPositionAsync({});
          const newRegion: Region = {
            latitude: loc.coords.latitude,
            longitude: loc.coords.longitude,
            latitudeDelta: 0.05,
            longitudeDelta: 0.05,
          };
          setRegion(newRegion);
          setSelectedLocation({
            latitude: loc.coords.latitude,
            longitude: loc.coords.longitude,
          });
        } catch (err) {
          // If we fail to fetch current location, fallback to a default (e.g. SF).
          setRegion({
            latitude: 37.78825,
            longitude: -122.4324,
            latitudeDelta: 0.05,
            longitudeDelta: 0.05,
          });
        }
      } else {
        // Permission denied => fallback region
        setRegion({
          latitude: 37.78825,
          longitude: -122.4324,
          latitudeDelta: 0.05,
          longitudeDelta: 0.05,
        });
      }
    })();
  }, []);

  // Handle taps on the map to place marker
  const handleMapPress = (e: MapPressEvent) => {
    const { latitude, longitude } = e.nativeEvent.coordinate;
    setSelectedLocation({ latitude, longitude });
    setRegion((prev) =>
      prev
        ? { ...prev, latitude, longitude }
        : {
            latitude,
            longitude,
            latitudeDelta: 0.05,
            longitudeDelta: 0.05,
          }
    );
  };

  // Confirm the chosen location
  const handleConfirmLocation = async () => {
    if (!selectedLocation) {
      Alert.alert('No Location', 'Please tap on the map to choose a location.');
      return;
    }
    try {
      await updateLocation.mutateAsync({
        latitude: selectedLocation.latitude,
        longitude: selectedLocation.longitude,
      });
      // Navigate to the next onboarding step
      navigation.navigate('OnboardingWelcome' as never);
    } catch (error) {
      Alert.alert('Error', 'Failed to update location. Please try again.');
    }
  };

  return (
    <View style={styles.container}>
      <LinearGradient colors={[COLORS.primary, COLORS.secondary]} style={styles.header}>
        <Text style={styles.headerTitle}>Set Your Location</Text>
      </LinearGradient>

      <Text style={styles.instructions}>
        {permissionStatus === Location.PermissionStatus.DENIED
          ? 'Permission denied. Tap on the map to choose a location.'
          : 'Tap on the map to set your approximate location.'}
      </Text>

      <View style={styles.mapContainer}>
        {region && (
          <MapView
            style={StyleSheet.absoluteFill}
            region={region}
            onPress={handleMapPress}
          >
            {selectedLocation && (
              <Marker
                coordinate={selectedLocation}
                title="Selected Location"
              />
            )}
          </MapView>
        )}
      </View>

      <TouchableOpacity style={styles.confirmButton} onPress={handleConfirmLocation}>
        <Text style={styles.confirmButtonText}>Confirm & Continue</Text>
      </TouchableOpacity>
    </View>
  );
};

export default OnboardingLocationScreen;

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
  },
  header: {
    padding: 20,
    paddingHorizontal: 20,
    alignItems: 'center',
  },
  headerTitle: {
    fontSize: 26,
    fontWeight: '700',
    color: '#fff',
  },
  instructions: {
    fontSize: 16,
    color: '#333',
    textAlign: 'center',
    marginHorizontal: 20,
    marginVertical: 10,
  },
  mapContainer: {
    flex: 1,
    marginHorizontal: 20,
    borderRadius: 12,
    overflow: 'hidden',
    backgroundColor: '#fff',
    elevation: 4,
    shadowColor: '#000',
    shadowOpacity: 0.1,
    shadowRadius: 6,
    shadowOffset: { width: 0, height: 3 },
    marginBottom: 10,
  },
  confirmButton: {
    backgroundColor: COLORS.accentGreen,
    margin: 20,
    paddingVertical: 16,
    borderRadius: 8,
    alignItems: 'center',
  },
  confirmButtonText: {
    color: '#fff',
    fontSize: 18,
    fontWeight: '600',
  },
});
