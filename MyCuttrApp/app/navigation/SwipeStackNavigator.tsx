import React from 'react';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import SwipeScreen from '../features/main/screens/SwipeScreen';
import SetUserPreferencesScreen from '../features/main/screens/SetUserPreferencesScreen'; // Import your SetUserPreferences screen

export type SwipeStackParamList = {
    SwipeHome: undefined;  // Main swipe screen
    SetUserPreferences: undefined;  // Screen for setting user preferences
};

const Stack = createNativeStackNavigator<SwipeStackParamList>();

const SwipeStackNavigator = () => {
    return (
        <Stack.Navigator screenOptions={{ headerShown: false }}>
            <Stack.Screen name="SwipeHome" component={SwipeScreen} />
            <Stack.Screen name="SetUserPreferences" component={SetUserPreferencesScreen} />
        </Stack.Navigator>
    );
};

export default SwipeStackNavigator;