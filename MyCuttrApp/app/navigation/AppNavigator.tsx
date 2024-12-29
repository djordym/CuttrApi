// File: app/navigation/AppNavigator.tsx

import React from 'react';
import AuthNavigator from './AuthNavigator';
import MainNavigator from './MainNavigator';
import { useSelector } from 'react-redux';
import { RootState } from '../store';
import { ActivityIndicator, View, StyleSheet } from 'react-native';

const AppNavigator = () => {
  const accessToken = useSelector((state: RootState) => state.auth.accessToken);
  
  // Optional: If you have an initialization/loading state (e.g., loading tokens from storage)
  // const isInitializing = useSelector((state: RootState) => state.auth.isInitializing);
  
  // Uncomment the following block if you implement an initialization state
  /*
  if (isInitializing) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color="#1EAE98" />
      </View>
    );
  }
  */

  // Show AuthNavigator if not authenticated
  if (!accessToken) {
    return <AuthNavigator />;
  }

  // Show MainNavigator if authenticated
  return <MainNavigator />;
};

export default AppNavigator;

const styles = StyleSheet.create({
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
});
