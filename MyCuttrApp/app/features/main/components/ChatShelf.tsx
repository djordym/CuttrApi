import React, {
  useState,
  useRef,
  useCallback,
  memo,
  forwardRef,
  useImperativeHandle,
} from 'react';
import {
  Animated,
  Easing,
  View,
  TouchableOpacity,
  StyleSheet,
  LayoutChangeEvent,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';

import { COLORS } from '../../../theme/colors';
import { PlantCardWithInfo } from './PlantCardWithInfo';
import { PlantResponse } from '../../../types/apiTypes';
import { log } from '../../../utils/logger';

export interface ChatShelfRef {
  toggleShelf: () => void;
  openShelf: () => void;
  closeShelf: () => void;
}

interface ChatShelfProps {
  plant1?: PlantResponse;
  plant2?: PlantResponse;
}

// Wrap with forwardRef to expose imperative methods
const ChatShelf = forwardRef<ChatShelfRef, ChatShelfProps>(
  function ChatShelf({ plant1, plant2 }, ref) {
    const [isShelfOpen, setIsShelfOpen] = useState(true);
    const [maxShelfHeight, setMaxShelfHeight] = useState<number>(0);
    const shelfAnim = useRef(new Animated.Value(0)).current;

    const handleShelfLayout = useCallback(
      (event: LayoutChangeEvent) => {
        const { height } = event.nativeEvent.layout;
        log.debug('Measured layout height:', height);
        if (height > maxShelfHeight) {
          setMaxShelfHeight(height);
          // Keep shelf fully open if we are currently open
          if (isShelfOpen) {
            shelfAnim.setValue(height);
          }
        }
      },
      [isShelfOpen, maxShelfHeight, shelfAnim]
    );

    // The actual animation
    const animateTo = useCallback(
      (toValue: number) => {
        Animated.timing(shelfAnim, {
          toValue,
          duration: 300,
          easing: Easing.ease,
          useNativeDriver: false,
        }).start();
      },
      [shelfAnim]
    );

    const toggleShelf = useCallback(() => {
      if (!maxShelfHeight) return; // if not measured, skip
      const collapsedHeight = styles.shelfToggleButton.height;
      const targetValue = isShelfOpen ? collapsedHeight : maxShelfHeight;
      animateTo(targetValue);
      setIsShelfOpen(!isShelfOpen);
    }, [isShelfOpen, maxShelfHeight, animateTo]);

    const closeShelf = useCallback(() => {
      if (!maxShelfHeight) return;
      if (isShelfOpen) {
        const collapsedHeight = styles.shelfToggleButton.height;
        animateTo(collapsedHeight);
        setIsShelfOpen(false);
      }
    }, [isShelfOpen, maxShelfHeight, animateTo]);

    const openShelf = useCallback(() => {
      if (!maxShelfHeight) return;
      if (!isShelfOpen) {
        animateTo(maxShelfHeight);
        setIsShelfOpen(true);
      }
    }, [isShelfOpen, maxShelfHeight, animateTo]);

    /**
     * Expose these methods via useImperativeHandle
     */
    useImperativeHandle(ref, () => ({
      toggleShelf,
      closeShelf,
      openShelf,
    }));

    // If not yet measured, don't force any height. Otherwise, animate the height with shelfAnim.
    const containerStyle = maxShelfHeight > 0 ? { height: shelfAnim } : undefined;

    return (
      <Animated.View style={[styles.animatedShelf, containerStyle]}>
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
  }
);

export default memo(ChatShelf);

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
    paddingBottom: 23,
  },
  plantCardWrapper: {
    width: '45%',
    borderRadius: 8,
    alignSelf: 'center',
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
