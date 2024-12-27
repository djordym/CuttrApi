import React, { useEffect } from 'react';
import { NavigationContainer } from '@react-navigation/native';
import AuthNavigator from './AuthNavigator';
import MainNavigator from './MainNavigator';
import { useSelector, useDispatch } from 'react-redux';
import { RootState } from '../store';
import { ActivityIndicator, View } from 'react-native';
import { fetchUserProfile } from '../features/main/store/userSlice';

const AppNavigator = () => {
  const dispatch = useDispatch();
  const { accessToken, userId } = useSelector((state: RootState) => state.auth);
  const { profile, status: userStatus } = useSelector((state: RootState) => state.user);

  useEffect(() => {
    // If we have tokens and a userId but no profile yet, fetch the user profile
    if (accessToken && userId && !profile && userStatus === 'idle') {
      dispatch(fetchUserProfile(userId) as any);
    }
  }, [accessToken, userId, profile, userStatus, dispatch]);

  // Determine what to show:
  // 1. If no accessToken => show AuthNavigator
  if (!accessToken) {
    return (
        <AuthNavigator />
    );
  }

  // 2. If we have tokens but are still fetching user profile => show a loading indicator
  if (userStatus === 'loading' || (!profile && userId)) {
    return (
      <View style={{ flex:1, justifyContent:'center', alignItems:'center' }}>
        <ActivityIndicator size="large" />
      </View>
    );
  }

  // 3. If we have tokens and profile is loaded => show MainNavigator
  return (
      <MainNavigator />
  );
};

export default AppNavigator;
