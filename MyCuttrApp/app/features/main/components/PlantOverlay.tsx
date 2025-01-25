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
  <View style={[styles.overlayContent, compact && styles.overlayContentCompact]}>
    <Text style={[styles.fullPlantName, compact && styles.fullPlantNameCompact]}>{speciesName}</Text>
    {tags.length > 0 && (
      <View style={[styles.tagRow, compact && styles.tagRowCompact]}>
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
  </View>
);

const styles = StyleSheet.create({
  overlayContent: {
    padding: 10,
    bottom: 0,
    marginBottom: 2,
    marginLeft: 4,
  },
  overlayContentCompact: {
    padding: 6,
    bottom: 0,
    marginBottom: 1,
    marginLeft: 2,
  },
  fullPlantName: {
    fontSize: 18,
    fontWeight: '700',
    color: '#fff',
    marginBottom: 6,
  },
  fullPlantNameCompact: {
    fontSize: 12,
    fontWeight: '700',
    color: '#fff',
    marginBottom: 3,
  },
  tagRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    marginLeft: -6,
  },
  tagRowCompact: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    marginLeft: -3,
  },
  tag: {
    backgroundColor: COLORS.accentGreen,
    borderRadius: 12,
    paddingHorizontal: 8,
    paddingVertical: 4,
    marginRight: 6,
    marginBottom: 6,
  },
  tagCompact: {
    backgroundColor: COLORS.accentGreen,
    borderRadius: 10,
    paddingHorizontal: 4,
    paddingVertical: 2,
    marginRight: 3,
    marginBottom: 3,
  },
  tagText: {
    color: COLORS.textLight,
    fontSize: 12,
    fontWeight: '600',
  },
  tagTextCompact: {
    color: COLORS.textLight,
    fontSize: 8,
    fontWeight: '600',
  },
  fullDescription: {
    color: COLORS.textLight,
    fontSize: 14,
    fontWeight: '400',
  },
  fullDescriptionCompact: {
    fontSize: 10,
    color: COLORS.textLight,
    fontWeight: '400',
  },
});
