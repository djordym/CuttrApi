// File: src/features/main/components/ProfileCardShelf.tsx

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
import { log } from '../../../utils/logger';
import { UserResponse } from '../../../types/apiTypes';
import { ProfileCard } from './ProfileCard';

/**
 * Exposes imperative methods for opening, closing, toggling.
 */
export interface ProfileCardShelfRef {
  toggleShelf: () => void;
  openShelf: () => void;
  closeShelf: () => void;
}

interface ProfileCardShelfProps {
  /**
   * The user profile to display inside the shelf.
   */
  userProfile?: UserResponse;
}

const ProfileCardShelf = forwardRef<ProfileCardShelfRef, ProfileCardShelfProps>(
  function ProfileCardShelf({ userProfile }, ref) {
    // Start in open state
    const [isShelfOpen, setIsShelfOpen] = useState(true);
    // Store the maximum shelf height once measured
    const [maxShelfHeight, setMaxShelfHeight] = useState<number>(0);

    // This animated value transitions between minHeight and maxShelfHeight
    const shelfAnim = useRef(new Animated.Value(0)).current;

    /**
     * On layout, measure the full height so we can animate from that in the future.
     * If it's bigger than any previously recorded height, save it.
     * If the shelf is still open, set the animated value to that full height.
     */
    const handleShelfLayout = useCallback(
      (event: LayoutChangeEvent) => {
        const { height } = event.nativeEvent.layout;
        log.debug('Measured shelf layout height:', height);

        if (height > maxShelfHeight) {
          setMaxShelfHeight(height);

          // If we’re meant to be open, show the full shelf right now
          if (isShelfOpen) {
            shelfAnim.setValue(height);
          }
        }
      },
      [isShelfOpen, maxShelfHeight, shelfAnim]
    );

    /**
     * A simple function to animate the shelfAnim to a given height.
     */
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

    /**
     * Toggle between fully open (maxShelfHeight) and collapsed (~15 px).
     */
    const toggleShelf = useCallback(() => {
      if (!maxShelfHeight) return; // Not measured yet, so skip

      // The small height for the toggle handle only
      const collapsedHeight = styles.shelfToggleButton.height;
      const targetValue = isShelfOpen ? collapsedHeight : maxShelfHeight;

      animateTo(targetValue);
      setIsShelfOpen(!isShelfOpen);
    }, [isShelfOpen, maxShelfHeight, animateTo]);

    /**
     * Explicitly close the shelf (collapse).
     */
    const closeShelf = useCallback(() => {
      if (!maxShelfHeight) return; // Not measured yet

      if (isShelfOpen) {
        const collapsedHeight = styles.shelfToggleButton.height;
        animateTo(collapsedHeight);
        setIsShelfOpen(false);
      }
    }, [isShelfOpen, maxShelfHeight, animateTo]);

    /**
     * Explicitly open the shelf (expand).
     */
    const openShelf = useCallback(() => {
      if (!maxShelfHeight) return; // Not measured yet

      if (!isShelfOpen) {
        animateTo(maxShelfHeight);
        setIsShelfOpen(true);
      }
    }, [isShelfOpen, maxShelfHeight, animateTo]);

    // Expose these methods for the parent (ChatScreen, etc.)
    useImperativeHandle(ref, () => ({
      toggleShelf,
      closeShelf,
      openShelf,
    }));

    /**
     * If we've already measured, we animate the container to shelfAnim.
     * If not measured yet, we let it wrap content naturally (so we can measure).
     */
    const containerStyle =
      maxShelfHeight > 0
        ? { height: shelfAnim }
        : undefined;

    return (
      <Animated.View style={[styles.animatedShelf, containerStyle]}>
        <View style={styles.shelfInnerContainer} onLayout={handleShelfLayout}>
          {userProfile && (
            <View style={styles.profileCardWrapper}>
              <ProfileCard
                userProfile={userProfile}
              />
            </View>
          )}
        </View>

        {/* The toggle "handle" pinned at the bottom */}
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

export default memo(ProfileCardShelf);

const styles = StyleSheet.create({
  animatedShelf: {
    overflow: 'hidden',
    backgroundColor: '#f1f1f1',
  },
  shelfInnerContainer: {
    paddingHorizontal: 12,
    paddingVertical: 8,
    paddingBottom: 23,
  },
  profileCardWrapper: {
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
