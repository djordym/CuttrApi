import React from 'react';
import {
  StyleSheet,
  View,
  Image,
  Text,
  Dimensions,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { PanGestureHandler, PanGestureHandlerGestureEvent } from 'react-native-gesture-handler';
import Animated, {
  useSharedValue,
  useAnimatedGestureHandler,
  useAnimatedStyle,
  runOnJS,
  withSpring,
} from 'react-native-reanimated';
import { LinearGradient } from 'expo-linear-gradient';
import { PlantResponse } from '../../../types/apiTypes';

const { width } = Dimensions.get('window');
const SWIPE_THRESHOLD = 0.25 * width;

const COLORS = {
  primary: '#1EAE98',
  accent: '#FF6F61',
  background: '#F2F2F2',
  textDark: '#2F4F4F',
  textLight: '#FFFFFF',
  cardBg: '#FFFFFF',
  border: '#ddd',
};

interface SwipeableCardProps {
  plant: PlantResponse;
  onSwipeLeft: (plantId: number) => void;
  onSwipeRight: (plantId: number) => void;
}

export const SwipeableCard: React.FC<SwipeableCardProps> = ({
  plant,
  onSwipeLeft,
  onSwipeRight,
}) => {
  const translateX = useSharedValue(0);
  const rotateZ = useSharedValue(0);

  // Compute all tags from various plant properties and extras
  const allTags = [
    plant.plantStage,
    plant.plantCategory,
    plant.wateringNeed,
    plant.lightRequirement,
    plant.size,
    plant.indoorOutdoor,
    plant.propagationEase,
    plant.petFriendly,
    ...(plant.extras ?? [])
  ].filter(Boolean);

  const gestureHandler = useAnimatedGestureHandler<
    PanGestureHandlerGestureEvent,
    { startX: number }
  >({
    onStart: (_, ctx) => {
      ctx.startX = translateX.value;
    },
    onActive: (event, ctx) => {
      translateX.value = ctx.startX + event.translationX;
      rotateZ.value = (event.translationX / width) * 0.15;
    },
    onEnd: (event) => {
      if (event.translationX > SWIPE_THRESHOLD) {
        translateX.value = withSpring(width * 1.5, {}, (finished) => {
          if (finished) runOnJS(onSwipeRight)(plant.plantId);
        });
      } else if (event.translationX < -SWIPE_THRESHOLD) {
        translateX.value = withSpring(-width * 1.5, {}, (finished) => {
          if (finished) runOnJS(onSwipeLeft)(plant.plantId);
        });
      } else {
        translateX.value = withSpring(0);
        rotateZ.value = withSpring(0);
      }
    },
  });

  const animatedStyle = useAnimatedStyle(() => ({
    transform: [
      { translateX: translateX.value },
      { rotateZ: `${rotateZ.value}rad` },
    ],
  }));

  return (
    <PanGestureHandler onGestureEvent={gestureHandler}>
      <Animated.View style={[styles.cardContainer, animatedStyle]}>
        <View style={styles.fullImageContainer}>
          {plant.imageUrl ? (
            <Image
              source={{ uri: plant.imageUrl }}
              style={styles.fullImage}
              resizeMode="contain"
            />
          ) : (
            <View style={styles.plantPlaceholder}>
              <Ionicons name="leaf" size={60} color={COLORS.primary} />
            </View>
          )}
        <View style={styles.imageOverlay}>
          <LinearGradient
            colors={['rgba(0,0,0,0)', 'rgba(0,0,0,1)']}
            style={styles.overlayContent}
          >
          </LinearGradient>
        </View>
        </View>
        <View style={styles.tagContainer}>
            {/* Plant Name */}
            <Text style={styles.fullPlantName} numberOfLines={1}>
              {plant.speciesName}
            </Text>

            {/* Render all tags */}
            {allTags.length > 0 && (
              <View style={styles.tagRow}>
                {allTags.map((tag) => (
                  <View key={tag} style={styles.tag}>
                    <Text style={styles.tagText}>{tag}</Text>
                  </View>
                ))}
              </View>
            )}

            {/* Description */}
            {plant.description ? (
              <Text style={styles.fullDescription} numberOfLines={3}>
                {plant.description}
              </Text>
            ) : null}
        </View>
        <View style={styles.underImageExtension} />
      </Animated.View>
    </PanGestureHandler>
  );
};

const styles = StyleSheet.create({
  // Overall card container: animated for the swipe
  cardContainer: {
    position: 'absolute',
    width: width * 0.9,
    borderRadius: 8,
    overflow: 'hidden',
  
    marginVertical: 'auto',

    // Shadows
    backgroundColor: COLORS.cardBg,
    shadowColor: '#000',
    shadowOpacity: 0.12,
    shadowRadius: 5,
    shadowOffset: { width: 0, height: 3 },
    elevation: 3,
  },

  innerContainer: {
    flex: 1,
    borderRadius: 8,
    overflow: 'hidden',
  },

  // Full-size style from MyProfileScreen
  fullImageContainer: {
    width: '100%',
    position: 'relative',
    // Maintain aspect ratio
    aspectRatio: 3 / 4,
  },
  fullImage: {
    ...StyleSheet.absoluteFillObject,
  },
  plantPlaceholder: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#eee',
  },

  // Overlay at bottom of card
  imageOverlay: {
    ...StyleSheet.absoluteFillObject,
    justifyContent: 'flex-end',
  },
  overlayContent: {
    position:'relative',
    bottom: 0,
    paddingTop: 200, // So the gradient starts higher
  },

  // Extension below the image
  underImageExtension: {
    backgroundColor: 'black',
    zIndex: -1,
    height: 50,
    
  },

  // Tags & description
  tagContainer: {
    padding: 10,
    position: 'absolute',
    bottom: 0,
  },

  // Title, tags, etc.
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
