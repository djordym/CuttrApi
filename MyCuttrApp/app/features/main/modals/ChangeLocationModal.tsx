// File: app/features/onboarding/screens/ChangeLocationModal.tsx

import React, { useState, useEffect } from 'react';
import {
  Modal,
  View,
  Text,
  StyleSheet,
  ActivityIndicator,
  Dimensions,
} from 'react-native';
import MapView, { Marker, MapPressEvent, Region } from 'react-native-maps';
import * as Location from 'expo-location'; // If using Expo
import { useTranslation } from 'react-i18next';
import { log } from '../../../utils/logger';
import ConfirmCancelButtons from '../components/ConfirmCancelButtons';
import { userService } from '../../../api/userService';
import { UpdateLocationRequest } from '../../../types/apiTypes';
import { COLORS } from '../../../theme/colors';

interface ChangeLocationModalProps {
  visible: boolean;
  initialLatitude?: number;
  initialLongitude?: number;
  onClose: () => void;
  onUpdated: () => void;
}

export const ChangeLocationModal: React.FC<ChangeLocationModalProps> = ({
  visible,
  initialLatitude,
  initialLongitude,
  onClose,
  onUpdated,
}) => {
  const { t } = useTranslation();
  const [markerPosition, setMarkerPosition] = useState<{ lat: number; lng: number } | null>(null);
  const [region, setRegion] = useState<Region | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [permissionStatus, setPermissionStatus] =
    useState<Location.PermissionStatus | null>(null);

  // Check if valid numeric coords (including zero).
  const hasInitialCoords =
    typeof initialLatitude === 'number' &&
    !Number.isNaN(initialLatitude) &&
    typeof initialLongitude === 'number' &&
    !Number.isNaN(initialLongitude);

  // When the modal opens, handle location permission + region setup
  useEffect(() => {
    if (!visible) return;

    (async () => {
      const { status } = await Location.requestForegroundPermissionsAsync();
      setPermissionStatus(status);

      // If granted and no initial coords, try fetching current location
      if (status === Location.PermissionStatus.GRANTED && !hasInitialCoords) {
        try {
          const loc = await Location.getCurrentPositionAsync({});
          setRegion({
            latitude: loc.coords.latitude,
            longitude: loc.coords.longitude,
            latitudeDelta: 0.05,
            longitudeDelta: 0.05,
          });
          setMarkerPosition({ lat: loc.coords.latitude, lng: loc.coords.longitude });
        } catch (error) {
          // fallback to a default region if location fails
          setRegion({
            latitude: 37.78825,
            longitude: -122.4324,
            latitudeDelta: 0.05,
            longitudeDelta: 0.05,
          });
        }
      } else if (hasInitialCoords) {
        setRegion({
          latitude: initialLatitude!,
          longitude: initialLongitude!,
          latitudeDelta: 0.05,
          longitudeDelta: 0.05,
        });
        setMarkerPosition({ lat: initialLatitude!, lng: initialLongitude! });
      } else {
        // If no coords and permission denied, default
        setRegion({
          latitude: 37.78825,
          longitude: -122.4324,
          latitudeDelta: 0.05,
          longitudeDelta: 0.05,
        });
      }

      setError(null);
    })();
  }, [visible]);

  // If not visible, do not render
  if (!visible) {
    return null;
  }

  // Handle map tap
  const handleMapPress = (e: MapPressEvent) => {
    const { latitude, longitude } = e.nativeEvent.coordinate;
    setMarkerPosition({ lat: latitude, lng: longitude });
  };

  // Update region after user panning/zooming
  const handleRegionChangeComplete = (newRegion: Region) => {
    setRegion(newRegion);
  };

  // Confirm location => call API
  const handleConfirm = async () => {
    if (!markerPosition) {
      onClose();
      return;
    }
    setLoading(true);
    setError(null);

    const payload: UpdateLocationRequest = {
      latitude: markerPosition.lat,
      longitude: markerPosition.lng,
    };

    try {
      await userService.updateLocation(payload);
      onUpdated();
      onClose();
    } catch {
      setError(t('change_location_error_message'));
    } finally {
      setLoading(false);
    }
  };

  // Cancel
  const handleCancel = () => {
    onClose();
  };

  return (
    <Modal visible={visible} animationType="slide" transparent>
      <View style={styles.overlay}>
        <View style={styles.modalContainer}>
          <Text style={styles.title}>{t('change_location_title')}</Text>
          <Text style={styles.subtitle}>
            {permissionStatus === Location.PermissionStatus.DENIED
              ? t('change_location_permission_denied')
              : t('change_location_instruction')}
          </Text>

          {error && <Text style={styles.errorText}>{error}</Text>}

          <View style={styles.mapContainer}>
            {region && (
              <MapView
                style={{ flex: 1 }}
                region={region}
                onRegionChangeComplete={handleRegionChangeComplete}
                onPress={handleMapPress}
              >
                {markerPosition && (
                  <Marker
                    coordinate={{
                      latitude: markerPosition.lat,
                      longitude: markerPosition.lng,
                    }}
                  />
                )}
              </MapView>
            )}
          </View>

          {loading && (
            <ActivityIndicator
              size="small"
              color={COLORS.accentGreen}
              style={{ marginVertical: 10 }}
            />
          )}

          <ConfirmCancelButtons
            onCancel={handleCancel}
            onConfirm={handleConfirm}
            confirmButtonText="confirmCancelButtons.confirm"
            cancelButtonText="confirmCancelButtons.cancel"
          />
        </View>
      </View>
    </Modal>
  );
};

export default ChangeLocationModal;

const styles = StyleSheet.create({
  overlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.5)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  modalContainer: {
    width: '90%',
    height: '80%',
    backgroundColor: '#fff',
    borderRadius: 16,
    padding: 20,
    // iOS shadow
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 3 },
    shadowOpacity: 0.1,
    shadowRadius: 4,
    // Android elevation
    elevation: 5,
  },
  title: {
    fontSize: 18,
    fontWeight: '700',
    color: '#333',
    marginBottom: 6,
    textAlign: 'center',
  },
  subtitle: {
    fontSize: 14,
    color: '#555',
    marginBottom: 10,
    textAlign: 'center',
  },
  errorText: {
    color: '#FF6B6B',
    marginBottom: 10,
    textAlign: 'center',
  },
  mapContainer: {
    flex: 1,
    borderRadius: 8,
    overflow: 'hidden',
    marginBottom: 10,
    backgroundColor: '#eee',
  },
});
