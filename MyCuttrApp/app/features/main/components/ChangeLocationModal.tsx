import React, { useState, useEffect, useCallback } from 'react';
import {
  Modal,
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  ActivityIndicator,
  TextInput,
  Alert,
} from 'react-native';
import MapView, { Marker, MapPressEvent, Region } from 'react-native-maps';
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

  const [markerPosition, setMarkerPosition] = useState<{
    lat: number;
    lng: number;
  } | null>(null);
  const [region, setRegion] = useState<Region | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [searchInput, setSearchInput] = useState('');

  // Check if valid numeric coords (including zero).
  const hasInitialCoords =
    typeof initialLatitude === 'number' &&
    !Number.isNaN(initialLatitude) &&
    typeof initialLongitude === 'number' &&
    !Number.isNaN(initialLongitude);

  /**
   * If the modal is opening and we have valid initial coords, set them;
   * otherwise, reset so the user can place a marker.
   */
  useEffect(() => {
    if (visible) {
      if (hasInitialCoords) {
        // Create a region with a small delta so the user can see the marker
        setRegion({
          latitude: initialLatitude!,
          longitude: initialLongitude!,
          latitudeDelta: 0.05,
          longitudeDelta: 0.05,
        });
        setMarkerPosition({ lat: initialLatitude!, lng: initialLongitude! });
      } else {
        // Default region or fallback
        setRegion({
          latitude: 37.78825,
          longitude: -122.4324,
          latitudeDelta: 0.05,
          longitudeDelta: 0.05,
        });
        setMarkerPosition(null);
      }
      setSearchInput('');
      setError(null);
    }
  }, [visible, hasInitialCoords, initialLatitude, initialLongitude]);

  /**
   * Handler for tapping the map to place a marker.
   */
  const handleMapPress = (e: MapPressEvent) => {
    const { coordinate } = e.nativeEvent;
    setMarkerPosition({ lat: coordinate.latitude, lng: coordinate.longitude });
  };

  /**
   * Update the region state after the user pans/zooms.
   */
  const handleRegionChangeComplete = (newRegion: Region) => {
    setRegion(newRegion);
  };

  /**
   * Confirm the location: call the API and update user location.
   */
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

  /**
   * Cancel the modal without saving anything.
   */
  const handleCancel = () => {
    onClose();
  };

  /**
   * Use OpenStreetMap's Nominatim to geocode the user's text input,
   * then set the map region and marker accordingly.
   */
  const handleSearch = useCallback(async () => {
    if (!searchInput) return;

    setLoading(true);
    setError(null);

    try {
      // Use a free OSM/Nominatim endpoint to fetch location for given query
      // NOTE: Production apps should handle usage policies / user input carefully.
      const url = `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(
        searchInput
      )}&format=json&limit=1`;
      const response = await fetch(url);
      const data = await response.json();

      if (data && data.length > 0) {
        const { lat, lon } = data[0];
        // Convert strings to numbers
        const latNum = parseFloat(lat);
        const lonNum = parseFloat(lon);
        // Set the region so the map moves to that location
        const newRegion: Region = {
          latitude: latNum,
          longitude: lonNum,
          latitudeDelta: 0.05,
          longitudeDelta: 0.05,
        };
        setRegion(newRegion);
        setMarkerPosition({ lat: latNum, lng: lonNum });
      } else {
        setError(t('No results found for that location.'));
      }
    } catch (err) {
      console.error('Geocoding error:', err);
      setError(t('change_location_error_message'));
    } finally {
      setLoading(false);
    }
  }, [searchInput, t]);

  if (!visible) {
    return null;
  }

  return (
    <Modal visible={visible} animationType="slide" transparent>
      <View style={styles.overlay}>
        <View style={styles.modalContainer}>
          <Text style={styles.title}>{t('change_location_title')}</Text>
          <Text style={styles.subtitle}>{t('change_location_instructions')}</Text>

          {/* Search Box */}
          <View style={styles.searchRow}>
            <TextInput
              style={styles.searchInput}
              placeholder={t('Enter a city or country...')}
              value={searchInput}
              onChangeText={setSearchInput}
              returnKeyType="search"
              onSubmitEditing={handleSearch}
            />
            <TouchableOpacity onPress={handleSearch} style={styles.searchButton}>
              <Text style={styles.searchButtonText}>{t('Search')}</Text>
            </TouchableOpacity>
          </View>

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
            <ActivityIndicator size="small" color="#1EAE98" style={{ marginVertical: 10 }} />
          )}

          <View style={styles.actions}>
            <TouchableOpacity
              onPress={handleCancel}
              style={styles.cancelButton}
              accessibilityRole="button"
            >
              <Text style={styles.cancelButtonText}>
                {t('change_location_cancel_button')}
              </Text>
            </TouchableOpacity>
            <TouchableOpacity
              onPress={handleConfirm}
              style={styles.confirmButton}
              accessibilityRole="button"
            >
              <Text style={styles.confirmButtonText}>
                {t('change_location_confirm_button')}
              </Text>
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
  searchRow: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 10,
  },
  searchInput: {
    flex: 1,
    borderWidth: 1,
    borderColor: '#ccc',
    borderRadius: 8,
    paddingHorizontal: 10,
    paddingVertical: 6,
    marginRight: 8,
  },
  searchButton: {
    backgroundColor: '#1EAE98',
    paddingHorizontal: 15,
    paddingVertical: 10,
    borderRadius: 8,
  },
  searchButtonText: {
    color: '#fff',
    fontWeight: '600',
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
    backgroundColor: '#eee',
  },
  actions: {
    flexDirection: 'row',
    justifyContent: 'flex-end',
  },
  cancelButton: {
    marginRight: 10,
    backgroundColor: '#f0f0f0',
    paddingHorizontal: 15,
    paddingVertical: 10,
    borderRadius: 8,
  },
  cancelButtonText: {
    fontSize: 16,
    color: '#333',
    fontWeight: '600',
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
