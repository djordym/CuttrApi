// File: app/features/main/components/ProfileLocationDisplay.tsx
import React, { useEffect, useState } from 'react';
import { View, Text, StyleSheet } from 'react-native';
import * as Location from 'expo-location';
import { profileCardStyles } from '../styles/profileCardStyles';

interface ProfileLocationDisplayProps {
  latitude: number;
  longitude: number;
}

const ProfileLocationDisplay: React.FC<ProfileLocationDisplayProps> = ({ latitude, longitude }) => {
  const [locationName, setLocationName] = useState<string>('');
  const [permissionStatus, setPermissionStatus] = useState<Location.PermissionStatus | null>(null);

  useEffect(() => {
    (async () => {
      const { status } = await Location.getForegroundPermissionsAsync();
      if (status !== Location.PermissionStatus.GRANTED) {
        const { status: newStatus } = await Location.requestForegroundPermissionsAsync();
        setPermissionStatus(newStatus);
      } else {
        setPermissionStatus(status);
      }
    })();
  }, []);

  useEffect(() => {
    if (permissionStatus === Location.PermissionStatus.GRANTED) {
      const fetchLocationName = async () => {
        try {
          const [result] = await Location.reverseGeocodeAsync({ latitude, longitude });
          const city = result.city || result.subregion || '';
          const country = result.country || '';
          setLocationName(city && country ? `${city}, ${country}` : city || country);
        } catch (error) {
          console.error("Reverse geocoding error:", error);
          setLocationName('');
        }
      };
      fetchLocationName();
    }
  }, [latitude, longitude, permissionStatus]);

  return (
      <Text style={profileCardStyles.profileLocationText}>{locationName || "Location not set"}</Text>
  );
};

export default ProfileLocationDisplay;
