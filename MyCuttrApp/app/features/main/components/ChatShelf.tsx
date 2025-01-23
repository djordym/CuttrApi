// File: src/features/main/components/ChatShelf.tsx

import React, { useState, useRef, useCallback, memo } from 'react';
import {
  Animated,
  Easing,
  View,
  TouchableOpacity,
  StyleSheet,
  LayoutChangeEvent,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';

// Theme & Components
import { COLORS } from '../../../theme/colors';
import { PlantCardWithInfo } from './PlantCardWithInfo';
import { PlantResponse } from '../../../types/apiTypes';
import { log } from '../../../utils/logger';

interface ChatShelfProps {
  plant1?: PlantResponse;
  plant2?: PlantResponse;
}

/**
 * A collapsible/expandable shelf that shows two PlantCardWithInfo side by side.
 * It measures its full height so it can animate from fully open to collapsed.
 */
const ChatShelf: React.FC<ChatShelfProps> = memo(({ plant1, plant2 }) => {
  // Track whether shelf is expanded or collapsed.
  const [isShelfOpen, setIsShelfOpen] = useState(true);

  // The maximum content height measured so far.
  // We accumulate the largest measured height to cover dynamic changes.
  const [maxShelfHeight, setMaxShelfHeight] = useState<number>(0);

  // Animated value controlling the shelf height
  const shelfAnim = useRef(new Animated.Value(0)).current;

  /**
   * Whenever the shelf content re-renders (due to dynamic content like images/text),
   * onLayout will fire. We always store the largest measured height so we
   * can animate properly even if the shelf grows after initial render.
   */
  const handleShelfLayout = useCallback(
    (event: LayoutChangeEvent) => {
      const { height } = event.nativeEvent.layout;
      log.debug('Measured layout height:', height);

      // Only update if we have a new maximum.
      // If height is bigger than anything we've had, store it and set the animation value if open.
      if (height > maxShelfHeight) {
        setMaxShelfHeight(height);

        // If the shelf is currently open, we also reset the shelfAnim to the new max height
        // so the UI doesn't jump or show partial content.
        if (isShelfOpen) {
          shelfAnim.setValue(height);
        }
      }
    },
    [isShelfOpen, maxShelfHeight, shelfAnim]
  );

  /**
   * Toggle between fully open (maxShelfHeight) and collapsed (toggleButtonHeight).
   */
  const toggleShelf = useCallback(() => {
    // We don’t toggle if we haven’t measured anything yet
    // (i.e., maxShelfHeight = 0 likely means not measured).
    if (!maxShelfHeight) return;

    // Collapsed height is the toggle button’s height.
    const collapsedHeight = styles.shelfToggleButton.height;
    const targetValue = isShelfOpen ? collapsedHeight : maxShelfHeight;

    Animated.timing(shelfAnim, {
      toValue: targetValue,
      duration: 300,
      easing: Easing.ease,
      useNativeDriver: false, // We’re animating height, so it must be false
    }).start(() => {
      // Flip the shelf state after the animation completes
      setIsShelfOpen((prev) => !prev);
    });
  }, [isShelfOpen, maxShelfHeight, shelfAnim]);

  /**
   * If we haven't measured yet (maxShelfHeight === 0), we let the container "shrink-wrap"
   * until onLayout can measure. Once measured, containerStyle enforces the animated height.
   */
  const containerStyle =
    maxShelfHeight > 0 ? { height: shelfAnim } : undefined;

  return (
    <Animated.View style={[styles.animatedShelf, containerStyle]}>
      {/* 
        The content we measure. Use onLayout here so we can know the full height 
        once it has rendered or changed. 
      */}
      <View style={styles.shelfInnerContainer} onLayout={handleShelfLayout}>
        <View style={styles.plantCardWrapper}>
          {plant1 && <PlantCardWithInfo plant={plant1} compact />}
        </View>
        <View style={styles.plantCardWrapper}>
          {plant2 && <PlantCardWithInfo plant={plant2} compact />}
        </View>
      </View>

      {/* The toggle handle pinned at the bottom */}
      <TouchableOpacity
        style={styles.shelfToggleButton}
        onPress={toggleShelf}
        activeOpacity={0.7}
      >
        <Ionicons
          name={isShelfOpen ? 'chevron-up-outline' : 'chevron-down-outline'}
          size={10}
          color="#fff"
        />
      </TouchableOpacity>
    </Animated.View>
  );
});

export default ChatShelf;

const styles = StyleSheet.create({
  animatedShelf: {
    overflow: 'hidden',
    backgroundColor: '#f1f1f1',
  },
  shelfInnerContainer: {
    flexDirection: 'row',
    justifyContent: 'space-evenly',
    paddingHorizontal: 12,
    paddingVertical: 8,
    paddingBottom: 25,
  },
  plantCardWrapper: {
    width: '45%',
    borderRadius: 8,
    backgroundColor: '#fff',
    marginBottom: 20,
  },
  shelfToggleButton: {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    height: 15,
    backgroundColor: COLORS.accentGreen,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
  },
});
