import React, { useEffect } from 'react';
import { NavigationContainer } from '@react-navigation/native';
import AuthNavigator from './AuthNavigator';
import MainNavigator from './MainNavigator';
import { useSelector, useDispatch } from 'react-redux';
import { RootState } from '../store';
import { setInitialTokens } from '../features/auth/store/authSlice';
import { useAuthToken } from '../hooks/useAuthToken';
import { storage } from '../utils/storage';
import { ActivityIndicator, View } from 'react-native';

const AppNavigator = () => {
  const { initializing, accessToken, refreshToken } = useAuthToken();
  const { user } = useSelector((state: RootState) => state.auth);
  const dispatch = useDispatch();

  useEffect(() => {
    if (!initializing) {
      dispatch(setInitialTokens({ accessToken, refreshToken, user: user || null }));
    }
  }, [initializing]);

  if (initializing) {
    return (
      <View style={{flex:1,justifyContent:'center',alignItems:'center'}}>
        <ActivityIndicator size="large" />
      </View>
    );
  }

  const isLoggedIn = !!accessToken;

  return (
    <NavigationContainer>
      {isLoggedIn ? <MainNavigator /> : <AuthNavigator />}
    </NavigationContainer>
  );
};

export default AppNavigator;
