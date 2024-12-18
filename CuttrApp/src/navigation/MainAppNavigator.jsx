import React from "react";
import { createBottomTabNavigator } from "@react-navigation/bottom-tabs";
import { Image, StyleSheet } from "react-native";
import UserProfileScreen from "../screens/main/UserProfileScreen";
import LikerScreen from "../screens/main/LikerScreen";
import MatchesScreen from "../screens/main/MatchesScreen";
import SettingsScreen from "../screens/main/SettingsScreen";

const Tab = createBottomTabNavigator();

// Define your icon map
const icons = {
  UserProfile: require('../assets/profile.png'),
  LikerScreen: require('../assets/swiping.png'),
  Matches: require('../assets/match.png'),
  Settings: require('../assets/settings.png'),
};

export default function BottomTabNavigator() {
  return (
    <Tab.Navigator
      screenOptions={({ route }) => ({
        headerShown: false,
        tabBarIcon: ({ focused, color, size }) => {
          const iconSource = icons[route.name];
          return (
            <Image
              source={iconSource}
              style={[
                styles.icon,
                { tintColor: focused ? '#673ab7' : '#222' },
                { width: size, height: size },
              ]}
            />
          );
        },
        tabBarActiveTintColor: '#673ab7',
        tabBarInactiveTintColor: 'gray',
      })}
    >
      <Tab.Screen name="UserProfile" component={UserProfileScreen} />
      <Tab.Screen name="LikerScreen" component={LikerScreen} />
      <Tab.Screen name="Matches" component={MatchesScreen} />
      <Tab.Screen name="Settings" component={SettingsScreen} />
    </Tab.Navigator>
  );
}

const styles = StyleSheet.create({
  icon: {
    width: 24,
    height: 24,
  },
});
