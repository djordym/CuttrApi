import React, { useState, useEffect } from 'react';
import { Modal, View, Text, StyleSheet, TouchableOpacity, ActivityIndicator } from 'react-native';
import MapView, { Marker, MapPressEvent } from 'react-native-maps';
import { useTranslation } from 'react-i18next';
import { userService } from '../../../api/userService';
import { UpdateLocationRequest } from '../../../types/apiTypes';

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
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Explicitly determine if we have valid numeric coordinates (including zero).
  const hasInitialCoords =
    typeof initialLatitude === 'number' &&
    !Number.isNaN(initialLatitude) &&
    typeof initialLongitude === 'number' &&
    !Number.isNaN(initialLongitude);

  /**
   * If the modal opens and we have valid initial coords, set them;
   * otherwise, reset to null so the user can place a marker.
   */
  useEffect(() => {
    if (visible) {
      if (hasInitialCoords) {
        setMarkerPosition({ lat: initialLatitude!, lng: initialLongitude! });
      } else {
        setMarkerPosition(null);
      }
    }
  }, [visible, hasInitialCoords, initialLatitude, initialLongitude]);

  const handleMapPress = (e: MapPressEvent) => {
    const { coordinate } = e.nativeEvent;
    setMarkerPosition({ lat: coordinate.latitude, lng: coordinate.longitude });
  };

  const handleConfirm = async () => {
    if (!markerPosition) {
      return onClose();
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

  const handleCancel = () => {
    onClose();
  };

  return (
    <Modal visible={visible} animationType="slide" transparent>
      <View style={styles.overlay}>
        <View style={styles.modalContainer}>
          <Text style={styles.title}>{t('change_location_title')}</Text>
          <Text style={styles.subtitle}>{t('change_location_instructions')}</Text>
          {error && <Text style={styles.errorText}>{error}</Text>}

          <View style={styles.mapContainer}>
            <MapView
              style={{ flex: 1 }}
              initialRegion={{
                latitude: hasInitialCoords ? initialLatitude! : 37.78825,
                longitude: hasInitialCoords ? initialLongitude! : -122.4324,
                latitudeDelta: 0.0922,
                longitudeDelta: 0.0421,
              }}
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
          </View>

          {loading && (
            <ActivityIndicator size="small" color="#1EAE98" style={{ marginVertical: 10 }} />
          )}

          <View style={styles.actions}>
            <TouchableOpacity
              onPress={handleCancel}
              style={styles.cancelButton}
              accessibilityRole="button"
            >
              <Text style={styles.cancelButtonText}>{t('change_location_cancel_button')}</Text>
            </TouchableOpacity>
            <TouchableOpacity
              onPress={handleConfirm}
              style={styles.confirmButton}
              accessibilityRole="button"
            >
              <Text style={styles.confirmButtonText}>{t('change_location_confirm_button')}</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>
    </Modal>
  );
};

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
  },
  title: {
    fontSize: 18,
    fontWeight: '700',
    color: '#333',
    marginBottom: 10,
  },
  subtitle: {
    fontSize: 14,
    color: '#555',
    marginBottom: 10,
  },
  errorText: {
    color: '#FF6B6B',
    marginBottom: 10,
  },
  mapContainer: {
    flex: 1,
    borderRadius: 8,
    overflow: 'hidden',
    marginBottom: 10,
  },
  actions: {
    flexDirection: 'row',
    justifyContent: 'flex-end',
  },
  cancelButton: {
    marginRight: 10,
  },
  cancelButtonText: {
    fontSize: 16,
    color: '#333',
  },
  confirmButton: {
    backgroundColor: '#1EAE98',
    paddingHorizontal: 15,
    paddingVertical: 10,
    borderRadius: 8,
  },
  confirmButtonText: {
    fontSize: 16,
    color: '#fff',
    fontWeight: '600',
  },
});
