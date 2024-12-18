import React, { useContext, useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  Switch,
  TouchableOpacity,
  Platform,
} from 'react-native';
import AuthContext from '../../context/AuthContext';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import {
  useFonts,
  Montserrat_400Regular,
  Montserrat_700Bold,
} from '@expo-google-fonts/montserrat';
import * as SplashScreen from 'expo-splash-screen';

const COLORS = {
  primary: '#3EB489',
  secondary: '#FDCB6E',
  background: '#F2F2F2',
  accent: '#FF6F61',
  text: '#2F4F4F',
  white: '#FFFFFF',
};

SplashScreen.preventAutoHideAsync();

const SettingsScreen = () => {
  const { signOut } = useContext(AuthContext);
  const [isPushNotificationsEnabled, setIsPushNotificationsEnabled] = useState(false);
  const [isLocationServicesEnabled, setIsLocationServicesEnabled] = useState(false);

  const [fontsLoaded] = useFonts({
    Montserrat_400Regular,
    Montserrat_700Bold,
  });

  React.useEffect(() => {
    if (fontsLoaded) {
      SplashScreen.hideAsync();
    }
  }, [fontsLoaded]);

  const togglePushNotifications = () =>
    setIsPushNotificationsEnabled((previousState) => !previousState);
  const toggleLocationServices = () =>
    setIsLocationServicesEnabled((previousState) => !previousState);

  if (!fontsLoaded) {
    return null;
  }

  return (
    <SafeAreaView style={styles.container}>
      <ScrollView contentContainerStyle={styles.scrollContent}>
        <Text style={styles.header}>Settings</Text>

        <View style={styles.section}>
          <Text style={styles.sectionHeader}>Account</Text>
          <TouchableOpacity style={styles.settingItem} onPress={() => {}}>
            <Text style={styles.settingText}>Change Email</Text>
            <Ionicons name="chevron-forward" size={24} color={COLORS.text} />
          </TouchableOpacity>
          <TouchableOpacity style={styles.settingItem} onPress={() => {}}>
            <Text style={styles.settingText}>Change Password</Text>
            <Ionicons name="chevron-forward" size={24} color={COLORS.text} />
          </TouchableOpacity>
        </View>

        <View style={styles.section}>
          <Text style={styles.sectionHeader}>Notifications</Text>
          <View style={styles.settingItem}>
            <Text style={styles.settingText}>Push Notifications</Text>
            <Switch
              trackColor={{ false: '#767577', true: COLORS.primary }}
              thumbColor={isPushNotificationsEnabled ? COLORS.accent : '#f4f3f4'}
              ios_backgroundColor="#3e3e3e"
              onValueChange={togglePushNotifications}
              value={isPushNotificationsEnabled}
            />
          </View>
        </View>

        <View style={styles.section}>
          <Text style={styles.sectionHeader}>Privacy</Text>
          <View style={styles.settingItem}>
            <Text style={styles.settingText}>Location Services</Text>
            <Switch
              trackColor={{ false: '#767577', true: COLORS.primary }}
              thumbColor={isLocationServicesEnabled ? COLORS.accent : '#f4f3f4'}
              ios_backgroundColor="#3e3e3e"
              onValueChange={toggleLocationServices}
              value={isLocationServicesEnabled}
            />
          </View>
        </View>

        <TouchableOpacity style={styles.logoutButton} onPress={signOut}>
          <Text style={styles.logoutButtonText}>Logout</Text>
        </TouchableOpacity>
      </ScrollView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
  scrollContent: {
    paddingBottom: 20,
  },
  header: {
    fontSize: 28,
    fontFamily: 'Montserrat_700Bold',
    marginTop: 30,
    marginBottom: 20,
    marginHorizontal: 20,
    color: COLORS.text,
  },
  section: {
    marginBottom: 24,
    backgroundColor: COLORS.white,
    paddingVertical: 15,
    paddingHorizontal: 20,
    borderRadius: 15,
    marginHorizontal: 20,
    ...Platform.select({
      ios: {
        shadowColor: '#000',
        shadowOpacity: 0.1,
        shadowRadius: 5,
      },
      android: {
        elevation: 3,
      },
    }),
  },
  sectionHeader: {
    fontSize: 20,
    fontFamily: 'Montserrat_700Bold',
    marginBottom: 12,
    color: COLORS.primary,
  },
  settingItem: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: 15,
    borderBottomWidth: 1,
    borderBottomColor: COLORS.background,
  },
  settingText: {
    fontSize: 16,
    fontFamily: 'Montserrat_400Regular',
    color: COLORS.text,
  },
  logoutButton: {
    marginTop: 32,
    marginHorizontal: 20,
    backgroundColor: COLORS.accent,
    paddingVertical: 15,
    borderRadius: 10,
    alignItems: 'center',
    ...Platform.select({
      ios: {
        shadowColor: '#000',
        shadowOpacity: 0.2,
        shadowRadius: 5,
      },
      android: {
        elevation: 4,
      },
    }),
  },
  logoutButtonText: {
    fontSize: 18,
    fontFamily: 'Montserrat_700Bold',
    color: COLORS.white,
  },
});

export default SettingsScreen;
