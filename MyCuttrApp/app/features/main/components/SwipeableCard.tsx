import React, { forwardRef, useImperativeHandle } from 'react';
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
import { COLORS } from '../../../theme/colors';
import { PlantOverlay } from './PlantOverlay';

const { width } = Dimensions.get('window');
const SWIPE_THRESHOLD = 0.1 * width;

/**
 * Methods the parent can call on this card
 * e.g. to finalize a right-swipe or reset it to center.
 */
export interface SwipeableCardRef {
  /** Animate the card flying off to the right, then call onSwipeRight. */
  flyOffRight: () => void;

  /** Reset the card back to center if user cancels. */
  resetPosition: () => void;
}

interface SwipeableCardProps {
  plant: PlantResponse;

  /** Called when the card is actually removed to the left. */
  onSwipeLeft: (plantId: number) => void;

  /** Called when the card has fully flown off the right side. */
  onSwipeRight: (plantId: number) => void;

  /**
   * Called as soon as the user crosses the swipe threshold on the right.
   * This is where you open the "SelectPlantsModal" without waiting.
   */
  onLikeGestureBegin: (plant: PlantResponse) => void;
}

/**
 * forwardRef so the parent can call flyOffRight or resetPosition
 */
export const SwipeableCard = forwardRef<SwipeableCardRef, SwipeableCardProps>(
  ({ plant, onSwipeLeft, onSwipeRight, onLikeGestureBegin }, ref) => {
    const translateX = useSharedValue(0);
    const rotateZ = useSharedValue(0);

    /**
     * Expose some methods to the parent, so it can:
     *  - finish the right-swipe
     *  - reset the card position
     */
    useImperativeHandle(ref, () => ({
      flyOffRight: () => {
        'worklet';
        // Animate card fully to the right, then call onSwipeRight
        translateX.value = withSpring(width * 1.5, {}, (finished) => {
          if (finished) {
            runOnJS(onSwipeRight)(plant.plantId);
          }
        });
      },
      resetPosition: () => {
        'worklet';
        translateX.value = withSpring(0);
        rotateZ.value = withSpring(0);
      },
    }));

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
          // DO NOT fly off yet. Instead, partially move it aside
          // and immediately open the modal (parent triggers it).
          translateX.value = withSpring(150);
          runOnJS(onLikeGestureBegin)(plant);
        } else if (event.translationX < -SWIPE_THRESHOLD) {
          // Dislike: animate off to the left
          translateX.value = withSpring(-width * 1.5, {}, (finished) => {
            if (finished) {
              runOnJS(onSwipeLeft)(plant.plantId);
            }
          });
        } else {
          // Snap back to center
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

    // Compute all tags from various properties
    const allTags = [
      plant.plantStage,
      plant.plantCategory,
      plant.wateringNeed,
      plant.lightRequirement,
      plant.size,
      plant.indoorOutdoor,
      plant.propagationEase,
      plant.petFriendly,
      ...(plant.extras ?? []),
    ].filter(Boolean);

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
              />
            </View>
          </View>

          <View style={styles.plantInfoContainer}>
            <PlantOverlay
              speciesName={plant.speciesName}
              description={plant.description}
              tags={allTags}
            />
          </View>

          <View style={styles.underImageExtension} />
        </Animated.View>
      </PanGestureHandler>
    );
  }
);

export default SwipeableCard;

const styles = StyleSheet.create({
  cardContainer: {
    position: 'absolute',
    maxWidth: width * 0.9,
    borderRadius: 8,
    overflow: 'hidden',
    marginVertical: 'auto',

    backgroundColor: COLORS.cardBg1,
    shadowColor: '#000',
    shadowOpacity: 0.12,
    shadowRadius: 5,
    shadowOffset: { width: 0, height: 3 },
    elevation: 3,
  },
  fullImageContainer: {
    width: '100%',
    position: 'relative',
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
  imageOverlay: {
    ...StyleSheet.absoluteFillObject,
    justifyContent: 'flex-end',
  },
  overlayContent: {
    position: 'relative',
    bottom: 0,
    paddingTop: 200,
  },
  underImageExtension: {
    backgroundColor: 'black',
    zIndex: -1,
    height: 50,
  },
  plantInfoContainer: {
    padding: 5,
    position: 'absolute',
    bottom: 0,
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
    marginBottom: 4,
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
