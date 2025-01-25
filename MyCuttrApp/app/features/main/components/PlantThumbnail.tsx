// PlantThumbnail.tsx
import React from 'react';
import {
  TouchableOpacity,
  View,
  Image,
  Text,
  StyleSheet,
  Platform,
  StyleProp,
  ViewStyle,
    Dimensions,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { COLORS } from '../../../theme/colors';
import { PlantResponse } from '../../../types/apiTypes';

interface PlantThumbnailProps {
  plant: PlantResponse;
  onPress?: () => void;
  isSelected?: boolean;
  containerStyle?: StyleProp<ViewStyle>;
}

const screenWidth = Dimensions.get('window').width;

export const PlantThumbnail: React.FC<PlantThumbnailProps> = ({
  plant,
  onPress,
  isSelected,
  containerStyle,
}) => {
  return (
    <TouchableOpacity
      activeOpacity={0.9}
      style={[
        styles.container,
        containerStyle,
        isSelected && styles.selected,
      ]}
      onPress={onPress}
    >
      {plant.imageUrl ? (
        <Image
          source={{ uri: plant.imageUrl }}
          style={styles.thumbImage}
          resizeMode="contain"
        />
      ) : (
        <View style={styles.plantPlaceholder}>
          <Ionicons name="leaf" size={40} color={COLORS.accentGreen} />
        </View>
      )}
      <View style={styles.thumbTextWrapper}>
        <Text style={styles.thumbPlantName}>
          {plant.speciesName}
        </Text>
      </View>
    </TouchableOpacity>
  );
};

const styles = StyleSheet.create({
  container: {
    width: (screenWidth-70)/3,
    backgroundColor: COLORS.cardBg1,
    borderRadius: 8,
    margin: 8,
    overflow: 'hidden',
    ...Platform.select({
      ios: {
        shadowColor: '#000',
        shadowOpacity: 0.1,
        shadowRadius: 5,
      },
      android: {
        elevation: 3,
      },
    }),
  },
  selected: {
    borderWidth: 3,
    borderColor: COLORS.accentGreen,
  },
  thumbImage: {
    width: '100%',
    aspectRatio: 3 / 4,
  },
  plantPlaceholder: {
    width: '100%',
    height: 120,
    backgroundColor: '#eee',
    justifyContent: 'center',
    alignItems: 'center',
  },
  thumbTextWrapper: {
    padding: 8,
    alignItems: 'center',
  },
  thumbPlantName: {
    fontSize: 14,
    fontWeight: '600',
    color: COLORS.textDark,
    textAlign: 'center',
  },
});
