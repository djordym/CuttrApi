import React from 'react';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import MyProfileScreen from '../features/main/screens/MyProfileScreen';
import SettingsScreen from '../features/main/screens/SettingsScreen';
import SwipeScreen from '../features/main/screens/SwipeScreen';

const Tab = createBottomTabNavigator();

const MainNavigator = () => {
  return (
    <Tab.Navigator>
      <Tab.Screen name="Swipe" component={SwipeScreen} options={{ headerShown: false }} />
      <Tab.Screen name="Profile" component={MyProfileScreen} options={{ headerShown: false }} />
      <Tab.Screen name="Settings" component={SettingsScreen} options={{ headerShown: false }} />
    </Tab.Navigator>
  );
};

export default MainNavigator;
