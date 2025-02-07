// MatchCard.tsx
import React from 'react';
import { View, StyleSheet } from 'react-native';
import { MatchResponse } from '../../../types/apiTypes';
import PlantThumbnail from './PlantThumbnail';

interface MatchCardProps {
  match: MatchResponse;
  currentUserId: number;
}

const MatchCard: React.FC<MatchCardProps> = ({ match, currentUserId }) => {
  // Determine which plant is yours and which is your connection's.
  const currentUserPlant =
    match.plant1.userId === currentUserId ? match.plant1 : match.plant2;
  const connectionPlant =
    match.plant1.userId === currentUserId ? match.plant2 : match.plant1;

  return (
    <View style={styles.card}>
      <PlantThumbnail plant={currentUserPlant} selectable={false} />
      <PlantThumbnail plant={connectionPlant} selectable={false} />
    </View>
  );
};

export default MatchCard;

const styles = StyleSheet.create({
  card: {
    flexDirection: 'row',
    justifyContent: 'space-around',
    marginVertical: 8,
    padding: 8,
    backgroundColor: '#fff',
    borderRadius: 12,
    shadowColor: '#000',
    shadowOpacity: 0.1,
    shadowRadius: 6,
    shadowOffset: { width: 0, height: 3 },
    elevation: 3,
  },
});
