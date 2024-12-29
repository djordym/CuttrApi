// File: app/navigation/AppNavigator.tsx
import React from 'react';
import { ActivityIndicator, View, StyleSheet } from 'react-native';
import { useSelector } from 'react-redux';
import { RootState } from '../store';
import AuthNavigator from './AuthNavigator';
import MainNavigator from './MainNavigator';
import OnboardingNavigator from './OnboardingNavigator';
import { useUserProfile } from '../features/main/hooks/useUser'; // Already defined in your code

const AppNavigator = () => {
  const { accessToken } = useSelector((state: RootState) => state.auth);
  // Custom React Query hook that fetches user profile
  const { data: userProfile, isLoading: userProfileLoading } = useUserProfile();

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
  return <MainNavigator />;
};

export default AppNavigator;

const styles = StyleSheet.create({
  loadingContainer: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
});
