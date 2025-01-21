import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { COLORS } from '../../../theme/colors'; 

interface PlantOverlayProps {
  speciesName: string;
  description?: string;
  tags?: string[];
}

export const PlantOverlay: React.FC<PlantOverlayProps> = ({ speciesName, description, tags = [] }) => (
  <LinearGradient colors={['rgba(0, 0, 0, 0)', 'rgba(0, 0, 0, 1)']} style={styles.overlayContent}>
    <Text style={styles.fullPlantName}>{speciesName}</Text>
    {tags.length > 0 && (
      <View style={styles.tagRow}>
        {tags.map((tag) => (
          <View key={tag} style={styles.tag}>
            <Text style={styles.tagText}>{tag}</Text>
          </View>
        ))}
      </View>
    )}
    {description ? (
      <Text style={styles.fullDescription}>{description}</Text>
    ) : null}
  </LinearGradient>
);

const styles = StyleSheet.create({
  overlayContent: {
    padding: 10,
    paddingTop: 100,
  },
  fullPlantName: {
    fontSize: 18,
    fontWeight: '700',
    color: '#fff',
    marginBottom: 6,
  },
  tagRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    marginBottom: 6,
  },
  tag: {
    backgroundColor: COLORS.primary,
    borderRadius: 12,
    paddingHorizontal: 8,
    paddingVertical: 4,
    marginRight: 6,
    marginBottom: 6,
  },
  tagText: {
    color: '#fff',
    fontSize: 12,
    fontWeight: '600',
  },
  fullDescription: {
    color: '#fff',
    fontSize: 14,
    fontWeight: '400',
  },
});
