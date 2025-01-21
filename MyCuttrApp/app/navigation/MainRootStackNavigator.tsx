// RootStackNavigator.tsx
import React from 'react';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import SetUserPreferencesScreen from '../features/main/screens/SetUserPreferencesScreen';
import MainTabNavigator from './MainTabNavigator';
import ChatScreen from '../features/main/screens/ChatScreen';
import AddPlantScreen from '../features/main/screens/AddPlantScreen';

const RootStack = createNativeStackNavigator();

const MainRootStackNavigator = () => (
  <RootStack.Navigator screenOptions={{ headerShown: false }}>
    {/* Main Tabs of the app */}
    <RootStack.Screen name="MainTabs" component={MainTabNavigator} />
    {/* Modal screen presented over tabs */}
    <RootStack.Screen 
      name="SetUserPreferences" 
      component={SetUserPreferencesScreen}
      options={{ presentation: 'modal' }} 
    />
    <RootStack.Screen
        name="Chat"
        component={ChatScreen}
        options={{ presentation: 'modal' }}
    />
    <RootStack.Screen
        name="AddPlant"
        component={AddPlantScreen}
        options={{ presentation: 'modal' }}
    />
  </RootStack.Navigator>
);

export default MainRootStackNavigator;
