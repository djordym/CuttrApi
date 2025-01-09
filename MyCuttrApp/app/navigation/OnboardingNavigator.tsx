// File: app/navigation/OnboardingNavigator.tsx
import React from 'react';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import OnboardingWelcomeScreen from '../features/onboarding/screens/OnboardingWelcomeScreen';
import OnboardingLocationScreen from '../features/onboarding/screens/OnboardingLocationScreen';
import OnboardingBioScreen from '../features/onboarding/screens/OnboardingBioScreen';

const Stack = createNativeStackNavigator();

const OnboardingNavigator = () => {
  return (
    <Stack.Navigator screenOptions={{ headerShown: false }}>
      {/* 1. A welcome/info screen (optional). */}
      <Stack.Screen name="OnboardingWelcome" component={OnboardingWelcomeScreen} />
      <Stack.Screen name="OnboardingBio" component={OnboardingBioScreen} />
      {/* 2. A screen to set the user's location. */}
      <Stack.Screen name="OnboardingLocation" component={OnboardingLocationScreen} />
    </Stack.Navigator>
  );
};

export default OnboardingNavigator;
