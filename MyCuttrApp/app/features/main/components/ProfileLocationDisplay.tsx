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

  useEffect(() => {
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
  }, [latitude, longitude]);

  return (
      <Text style={profileCardStyles.profileLocationText}>{locationName || "Location not set"}</Text>
  );
};

export default ProfileLocationDisplay;
