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

// Use the same palette from your MyProfileScreen for consistency
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

/**
 * SwipeableCard
 * Fully replicates the "full-size" card style from MyProfileScreen
 * while preserving the Reanimated swipe logic for tinder-like gestures.
 */
export const SwipeableCard: React.FC<SwipeableCardProps> = ({
  plant,
  onSwipeLeft,
  onSwipeRight,
}) => {
  const translateX = useSharedValue(0);
  const rotateZ = useSharedValue(0);

  // Gesture handler to track drag & decide when to swipe
  const gestureHandler = useAnimatedGestureHandler<
    PanGestureHandlerGestureEvent,
    { startX: number }
  >({
    onStart: (_, ctx) => {
      ctx.startX = translateX.value;
    },
    onActive: (event, ctx) => {
      translateX.value = ctx.startX + event.translationX;
      rotateZ.value = (event.translationX / width) * 0.15; // Slight rotation effect
    },
    onEnd: (event) => {
      // If swiped beyond threshold, fling away with spring
      if (event.translationX > SWIPE_THRESHOLD) {
        // Swipe Right
        translateX.value = withSpring(width * 1.5, {}, (finished) => {
          if (finished) runOnJS(onSwipeRight)(plant.plantId);
        });
      } else if (event.translationX < -SWIPE_THRESHOLD) {
        // Swipe Left
        translateX.value = withSpring(-width * 1.5, {}, (finished) => {
          if (finished) runOnJS(onSwipeLeft)(plant.plantId);
        });
      } else {
        // Reset position & rotation
        translateX.value = withSpring(0);
        rotateZ.value = withSpring(0);
      }
    },
  });

  // Animate position & rotation
  const animatedStyle = useAnimatedStyle(() => ({
    transform: [
      { translateX: translateX.value },
      { rotateZ: `${rotateZ.value}rad` },
    ],
  }));

  return (
    <PanGestureHandler onGestureEvent={gestureHandler}>
      <Animated.View style={[styles.cardContainer, animatedStyle]}>
        {/* Full Image Container (replicating the "full-size" style from MyProfileScreen) */}
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

          {/* Overlay for tags & description */}
          <View style={styles.fullImageOverlay}>
            <LinearGradient
              colors={['rgba(0,0,0,0)', 'rgba(0,0,0,1)']}
              style={styles.overlayContent}
            >
              {/* Plant Name */}
              <Text style={styles.fullPlantName} numberOfLines={1}>
                {plant.speciesName}
              </Text>

              {/* Tags */}
              {plant.extras && plant.extras.length > 0 && (
                <View style={styles.tagRow}>
                  {plant.extras.map((tag) => (
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
            </LinearGradient>
          </View>
        </View>
      </Animated.View>
    </PanGestureHandler>
  );
};

const styles = StyleSheet.create({
  // Overall card container: animated for the swipe
  cardContainer: {
    position: 'absolute',
    alignSelf: 'center',
    width: width * 0.9,
    borderRadius: 8,
    overflow: 'hidden',
    // Center vertically
    
    marginVertical: 'auto',

    // Shadows
    backgroundColor: COLORS.cardBg,
    shadowColor: '#000',
    shadowOpacity: 0.12,
    shadowRadius: 5,
    shadowOffset: { width: 0, height: 3 },
    elevation: 3,
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
  fullImageOverlay: {
    ...StyleSheet.absoluteFillObject,
    justifyContent: 'flex-end',
  },
  overlayContent: {
    padding: 10,
    paddingTop: 100, // So the gradient starts higher
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
