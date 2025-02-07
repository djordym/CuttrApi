// File: app/features/main/components/PlantThumbnail.tsx
import React from 'react';
import { TouchableOpacity, View, Image, Text, StyleSheet, Platform, Dimensions, StyleProp, ViewStyle } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { COLORS } from '../../../theme/colors';
import { PlantResponse } from '../../../types/apiTypes';

const screenWidth = Dimensions.get('window').width;

export interface PlantThumbnailProps {
  plant: PlantResponse;
  onPress?: () => void;
  isSelected?: boolean;
  containerStyle?: StyleProp<ViewStyle>;
  selectable?: boolean;
  deletable?: boolean;
  OnDelete?: () => void;
  // <-- NEW prop for info icon press:
  onInfoPress?: () => void;
}

const PlantThumbnail: React.FC<PlantThumbnailProps> = ({
  plant,
  onPress,
  isSelected = false,
  containerStyle,
  selectable = false,
  deletable = false,
  OnDelete,
  onInfoPress,
}) => {
  return (
    <View style={[styles.outerContainer, isSelected && styles.selected]}>
      <TouchableOpacity
        style={[styles.mainContainer, containerStyle]}
        onPress={onPress}
        disabled={!selectable}
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
        {/* Render an info icon if onInfoPress is provided */}
        {onInfoPress && (
          <TouchableOpacity style={styles.infoIconContainer} onPress={onInfoPress}>
            <Ionicons name="information-circle-outline" size={20} color={COLORS.textLight} />
          </TouchableOpacity>
        )}
        <View style={styles.thumbTextWrapper}>
          <Text style={styles.thumbPlantName}>{plant.speciesName}</Text>
        </View>
      </TouchableOpacity>
      {deletable && (
        <TouchableOpacity style={styles.deleteButton} onPress={OnDelete}>
          <Ionicons name="close-circle" size={24} color={COLORS.accentRed} />
        </TouchableOpacity>
      )}
    </View>
  );
};

export default PlantThumbnail;

const styles = StyleSheet.create({
  outerContainer: {
    margin: 5,
    alignSelf: 'flex-start',
  },
  selected: {
    backgroundColor: COLORS.accentGreen,
    borderRadius: 10,
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
  mainContainer: {
    width: (screenWidth - 70) / 3,
    backgroundColor: COLORS.cardBg1,
    borderRadius: 8,
    margin: 3,
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
  deleteButton: {
    position: 'absolute',
    top: -8,
    right: -8,
    borderRadius: 50,
    backgroundColor: 'white',
  },
  infoIconContainer: {
    position: 'absolute',
    top: 5,
    right: 5,
    backgroundColor: 'rgba(0,0,0,0.5)',
    borderRadius: 12,
    padding: 2,
    zIndex: 2,
    alignItems: 'center',
    justifyContent: 'center',
  },
});
