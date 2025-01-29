// File: app/navigation/AppNavigator.tsx
import React, { useEffect, useState } from 'react';
import { ActivityIndicator, View, StyleSheet } from 'react-native';
import { useDispatch, useSelector } from 'react-redux';
import { RootState } from '../store';
import AuthNavigator from './AuthNavigator';
import MainTabNavigator from './MainTabNavigator';
import OnboardingNavigator from './OnboardingNavigator';
import { useMyProfile } from '../features/main/hooks/useMyProfileHooks';
import { storage } from '../utils/storage';
import { setInitialTokens } from '../features/auth/store/authSlice';
import { userService } from '../api/userService';
import { logout } from '../features/auth/store/authSlice';
import { store } from '../store';
import MainRootStackNavigator from './MainRootStackNavigator';

const AppNavigator = () => {
  const dispatch = useDispatch();
  const { accessToken } = useSelector((state: RootState) => state.auth);
  const { refreshToken } = useSelector((state: RootState) => state.auth);
  const [initializing, setInitializing] = useState(true);
  const { data: userProfile, isLoading: userProfileLoading } = useMyProfile();

// 1. Attempt to load tokens from storage
useEffect(() => {
  const initializeAuth = async () => {
    const storedAccessToken = await storage.getAccessToken();
    const storedRefreshToken = await storage.getRefreshToken();
    
    if (storedAccessToken && storedRefreshToken) {
      // Put tokens in Redux so subsequent requests include Auth headers
      dispatch(setInitialTokens({
        accessToken: storedAccessToken,
        refreshToken: storedRefreshToken,
        userId: null, // userId is unknown until we fetch profile
        email: null,
      }));

      // 2. Now fetch /me using these tokens
      try {
        const profile = await userService.getCurrentUserProfile();
        // Once we have the profile, save userId and email in Redux
        dispatch(setInitialTokens({
          accessToken: store.getState().auth.accessToken,
          refreshToken: store.getState().auth.refreshToken,
          userId: profile.userId,
          email: profile.email,
        }));
      } catch (err) {
        // If /me fails, log out
        dispatch(logout());
      }
    }

    setInitializing(false);
  };

  initializeAuth();
}, [dispatch]);

  // Show spinner while initializing tokens
  if (initializing) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color="#1EAE98" />
      </View>
    );
  }

  // 1. If no token -> show Auth flow
  if (!accessToken) {
    return <AuthNavigator />;
  }

  // 2. If we are still loading the user profile, show a spinner
  if (userProfileLoading) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color="#1EAE98" />
      </View>
    );
  }

  // 3. If location is NOT set, show onboarding flow
  if (!userProfile?.locationLatitude || !userProfile?.locationLongitude) {
    return <OnboardingNavigator />;
  }

  // 4. Otherwise, user has location -> show main app
  return <MainRootStackNavigator />;
};

export default AppNavigator;

const styles = StyleSheet.create({
  loadingContainer: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
});
