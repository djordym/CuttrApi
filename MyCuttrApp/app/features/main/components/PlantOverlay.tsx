import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { COLORS } from '../../../theme/colors'; 

interface PlantOverlayProps {
  speciesName: string;
  description?: string;
  tags?: string[];
  compact?: boolean;
}

export const PlantOverlay: React.FC<PlantOverlayProps> = ({ speciesName, description, tags = [], compact=false }) => (
  <LinearGradient colors={['rgba(0, 0, 0, 0)', 'rgba(0, 0, 0, 1)']} style={[styles.overlayContent, compact && styles.overlayContentCompact]}>
    <Text style={[styles.fullPlantName, compact && styles.fullPlantNameCompact]}>{speciesName}</Text>
    {tags.length > 0 && (
      <View style={styles.tagRow}>
        {tags.map((tag) => (
          <View key={tag} style={[styles.tag, compact && styles.tagCompact]}>
            <Text style={[styles.tagText, compact && styles.tagTextCompact]}>{tag}</Text>
          </View>
        ))}
      </View>
    )}
    {description ? (
      <Text style={[styles.fullDescription, compact && styles.fullDescriptionCompact]}>{description}</Text>
    ) : null}
  </LinearGradient>
);

const styles = StyleSheet.create({
  overlayContent: {
    padding: 10,
    paddingTop: 100,
  },
  overlayContentCompact: {
    padding: 10,
    paddingTop: 60,
  },
  fullPlantName: {
    fontSize: 18,
    fontWeight: '700',
    color: '#fff',
    marginBottom: 6,
  },
  fullPlantNameCompact: {
    fontSize: 14,
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
  tagCompact: {
    backgroundColor: COLORS.primary,
    borderRadius: 10,
    paddingHorizontal: 6,
    paddingVertical: 2,
    marginRight: 3,
    marginBottom: 3,
  },
  tagText: {
    color: '#fff',
    fontSize: 12,
    fontWeight: '600',
  },
  tagTextCompact: {
    color: '#fff',
    fontSize: 10,
    fontWeight: '600',
  },
  fullDescription: {
    color: '#fff',
    fontSize: 14,
    fontWeight: '400',
  },
  fullDescriptionCompact: {
    fontSize: 12,
  },
});
