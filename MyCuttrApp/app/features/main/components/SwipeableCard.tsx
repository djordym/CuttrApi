import React, { useEffect } from 'react';
import { StyleSheet, View, Image, Text, Dimensions } from 'react-native';
import { PanGestureHandler, PanGestureHandlerGestureEvent } from 'react-native-gesture-handler';
import Animated, {
  useSharedValue,
  useAnimatedGestureHandler,
  useAnimatedStyle,
  runOnJS,
  withSpring
} from 'react-native-reanimated';
import { PlantResponse } from '../../../types/apiTypes';

const { width } = Dimensions.get('window');
const SWIPE_THRESHOLD = 0.25 * width;

interface SwipeableCardProps {
  plant: PlantResponse;
  onSwipeLeft: (plantId: number) => void;
  onSwipeRight: (plantId: number) => void;
}

export const SwipeableCard: React.FC<SwipeableCardProps> = ({ plant, onSwipeLeft, onSwipeRight }) => {
  const translateX = useSharedValue(0);
  const rotateZ = useSharedValue(0);

  const gestureHandler = useAnimatedGestureHandler<PanGestureHandlerGestureEvent, { startX: number }>({
    onStart: (_, ctx) => {
      ctx.startX = translateX.value;
    },
    onActive: (event, ctx) => {
      translateX.value = ctx.startX + event.translationX;
      rotateZ.value = (event.translationX / width) * 0.15; // slight rotation
    },
    onEnd: (event) => {
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
        // Reset
        translateX.value = withSpring(0);
        rotateZ.value = withSpring(0);
      }
    },
  });

  const animatedStyle = useAnimatedStyle(() => {
    return {
      transform: [
        { translateX: translateX.value },
        { rotateZ: `${rotateZ.value}rad` }
      ]
    };
  });

  return (
    <PanGestureHandler onGestureEvent={gestureHandler}>
      <Animated.View style={[styles.card, animatedStyle]}>
        <View style={styles.imageContainer}>
          {plant.imageUrl ? (
            <Image source={{ uri: plant.imageUrl }} style={styles.image} />
          ) : (
            <View style={styles.imagePlaceholder}>
              <Text style={styles.placeholderText}>No Image</Text>
            </View>
          )}
        </View>
        <View style={styles.contentContainer}>
          <Text style={styles.plantName}>{plant.speciesName}</Text>
          <Text style={styles.plantDescription} numberOfLines={3}>
            {plant.description}
          </Text>
        </View>
      </Animated.View>
    </PanGestureHandler>
  );
};

const styles = StyleSheet.create({
  card: {
    width: width * 0.9,
    backgroundColor: '#fff',
    borderRadius: 16,
    overflow: 'hidden',
    position: 'absolute',
    alignSelf: 'center',
    top: 0,
    bottom: 0,
    marginVertical: 'auto',
    shadowColor: '#000',
    shadowOpacity: 0.1,
    shadowRadius: 10,
    elevation: 5,
  },
  imageContainer: {
    width: '100%',
    height: 300,
    backgroundColor: '#fafafa',
  },
  image: {
    width: '100%',
    height: '100%',
    resizeMode: 'cover',
  },
  imagePlaceholder: {
    flex:1,
    justifyContent:'center',
    alignItems:'center',
    backgroundColor:'#eee',
  },
  placeholderText: {
    color:'#999'
  },
  contentContainer: {
    padding: 16,
  },
  plantName: {
    fontSize: 20,
    fontWeight: '700',
    color:'#333',
    marginBottom:8
  },
  plantDescription: {
    fontSize: 16,
    lineHeight: 22,
    color:'#555'
  },
});
