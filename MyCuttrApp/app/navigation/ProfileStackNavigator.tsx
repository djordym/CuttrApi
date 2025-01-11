import React from 'react';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import MyProfileScreen from '../features/main/screens/MyProfileScreen';
import AddPlantScreen from '../features/main/screens/AddPlantScreen'; // Import your AddPlant screen

export type ProfileStackParamList = {
  ProfileHome: undefined;  // Main profile screen
  AddPlant: undefined;     // Screen for adding a new plant
};

const Stack = createNativeStackNavigator<ProfileStackParamList>();

const ProfileStackNavigator = () => {
  return (
    <Stack.Navigator screenOptions={{ headerShown: false }}>
      <Stack.Screen name="ProfileHome" component={MyProfileScreen} />
      <Stack.Screen name="AddPlant" component={AddPlantScreen} />
    </Stack.Navigator>
  );
};

export default ProfileStackNavigator;