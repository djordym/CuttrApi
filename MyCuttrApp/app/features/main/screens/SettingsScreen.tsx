import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  Switch,
  ScrollView,
  Alert,
  ActivityIndicator,
  TextInput,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { Ionicons, MaterialIcons } from '@expo/vector-icons';
import { useTranslation } from 'react-i18next';

import { SafeAreaProvider} from 'react-native-safe-area-context';
import { storage } from '../../../utils/storage';
// Suppose you have some hooks/services:
import { userService } from '../../../api/userService';
import { useMyProfile } from '../hooks/useMyProfileHooks';
import { useNavigation } from '@react-navigation/native';
import { logout } from '../../auth/store/authSlice';
import { store } from '../../../store';
import { COLORS } from '../../../theme/colors';
import { headerStyles } from '../styles/headerStyles';
import { authService } from '../../../api/authService';

const SettingsScreen: React.FC = () => {
  const { t, i18n } = useTranslation();
  const navigation = useNavigation();

  // For user data (so we can display user’s email, for instance)
  const {
    data: userProfile,
    isLoading: userLoading,
    refetch: refetchUserProfile,
  } = useMyProfile();

//query client

  // Language management
  const [currentLang, setCurrentLang] = useState(i18n.language);


  // Toggles for push notifications, dark mode, etc.
  const [pushNotificationsEnabled, setPushNotificationsEnabled] = useState<boolean>(true); 
  const [darkModeEnabled, setDarkModeEnabled] = useState<boolean>(false);

  // Local state for changing email/password (in a real app, you might use modals)
  const [newEmail, setNewEmail] = useState<string>('');
  const [newPassword, setNewPassword] = useState<string>('');
  const [isUpdatingEmail, setIsUpdatingEmail] = useState<boolean>(false);
  const [isUpdatingPassword, setIsUpdatingPassword] = useState<boolean>(false);

  // Simulate loading state if necessary
  const [saving, setSaving] = useState<boolean>(false);

  // Handle language change and persist the selection
  const handleLanguageChange = async (lang: string) => {
    await i18n.changeLanguage(lang);
    await storage.saveLanguage(lang);
    setCurrentLang(lang);
  };

  // Stub for changing email
  const handleChangeEmail = async () => {
    if (!newEmail.trim()) {
      Alert.alert('Validation Error', t('Please enter a valid email.'));
      return;
    }
    setSaving(true);
    try {
      // Example: userService.updateEmail or userService.updateProfile
      //await userService.updateProfile({ email: newEmail });
      Alert.alert(t('Email changed successfully!'));
      setNewEmail('');
      refetchUserProfile();
    } catch (err) {
      console.error('Failed to change email:', err);
      Alert.alert(t('Error'), t('Could not change email.'));
    } finally {
      setSaving(false);
      setIsUpdatingEmail(false);
    }
  };

  // Stub for changing password
  const handleChangePassword = async () => {
    if (!newPassword.trim()) {
      Alert.alert('Validation Error', t('Please enter a valid password.'));
      return;
    }
    setSaving(true);
    try {
      // Example: userService.updatePassword
      // Some backend endpoints might require old password, too
      //await userService.updateProfile({ password: newPassword });
      Alert.alert(t('Password changed successfully!'));
      setNewPassword('');
      refetchUserProfile();
    } catch (err) {
      console.error('Failed to change password:', err);
      Alert.alert(t('Error'), t('Could not change password.'));
    } finally {
      setSaving(false);
      setIsUpdatingPassword(false);
    }
  };

  // Example logout
  const handleLogout = async () => {
    // Suppose you have an authService that handles removing tokens
    // e.g. await authService.logout()
    // Then navigate or reset navigation to Auth screen
    Alert.alert(
      t('Logout'),
      t('Are you sure you want to log out?'),
      [
        { text: t('Cancel'), style: 'cancel' },
        {
          text: t('Yes'),
          style: 'destructive',
          onPress: async () => {
            authService.logout();
            store.dispatch(logout());
          }
        },
      ]
    );
  };

  // Example delete account
  const handleDeleteAccount = async () => {
    Alert.alert(
      t('Delete Account'),
      t('This action cannot be undone. Are you sure?'),
      [
        { text: t('Cancel'), style: 'cancel' },
        {
          text: t('Yes, Delete'),
          style: 'destructive',
          onPress: async () => {
            try {
              // e.g. await userService.deleteAccount();
              navigation.reset({
                index: 0,
                routes: [{ name: 'AuthNavigator' as never }],
              });
            } catch (err) {
              console.error('Failed to delete account:', err);
              Alert.alert(t('Error'), t('Could not delete account.'));
            }
          },
        },
      ]
    );
  };

  // Save toggles to your preferences
  const handleSaveNotificationSettings = async (value: boolean) => {
    // e.g. userPreferencesService.updateNotifications(value)
    setPushNotificationsEnabled(value);
  };

  const handleSaveDarkMode = async (value: boolean) => {
    // e.g. userPreferencesService.updateDarkMode(value)
    setDarkModeEnabled(value);
  };


  if (userLoading) {
    return (
      <View style={styles.centered}>
        <ActivityIndicator size="large" color={COLORS.primary} />
      </View>
    );
  }

  return (
    <SafeAreaProvider style={styles.safeArea}>
      <LinearGradient
        colors={[COLORS.primary, COLORS.secondary]}
        style={headerStyles.headerGradient}
      >
        <View style={headerStyles.headerRow}>
          <Text style={headerStyles.headerTitle}>{t('Settings')}</Text>
          <MaterialIcons name="settings" size={24} color="#fff" />
        </View>
      </LinearGradient>

      <ScrollView contentContainerStyle={styles.scrollContent}>
        {/* ACCOUNT INFO SECTION */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>{t('Account')}</Text>

          {/* Email */}
          <Text style={styles.label}>{t('Email')}:</Text>
          <Text style={styles.value}>
            {userProfile?.email || t('No email found')}
          </Text>
          {isUpdatingEmail ? (
            <View style={styles.changeContainer}>
              <TextInput
                style={styles.input}
                placeholder={t('New email')}
                onChangeText={setNewEmail}
                value={newEmail}
                keyboardType="email-address"
                autoCapitalize="none"
              />
              <View style={styles.buttonRow}>
                <TouchableOpacity
                  style={[styles.smallButton, styles.cancelBtn]}
                  onPress={() => {
                    setIsUpdatingEmail(false);
                    setNewEmail('');
                  }}
                >
                  <Text style={styles.smallButtonText}>{t('Cancel')}</Text>
                </TouchableOpacity>
                <TouchableOpacity
                  style={[styles.smallButton, styles.confirmBtn]}
                  onPress={handleChangeEmail}
                  disabled={saving}
                >
                  {saving ? (
                    <ActivityIndicator size="small" color="#fff" />
                  ) : (
                    <Text style={styles.smallButtonText}>{t('Save')}</Text>
                  )}
                </TouchableOpacity>
              </View>
            </View>
          ) : (
            <TouchableOpacity
              style={styles.changeButton}
              onPress={() => setIsUpdatingEmail(true)}
            >
              <Text style={styles.changeButtonText}>{t('Change email')}</Text>
            </TouchableOpacity>
          )}

          {/* Password */}
          {isUpdatingPassword ? (
            <>
              <Text style={[styles.label, { marginTop: 10 }]}>
                {t('New Password')}:
              </Text>
              <View style={styles.changeContainer}>
                <TextInput
                  style={styles.input}
                  placeholder={t('Enter new password')}
                  onChangeText={setNewPassword}
                  value={newPassword}
                  secureTextEntry
                  autoCapitalize="none"
                />
                <View style={styles.buttonRow}>
                  <TouchableOpacity
                    style={[styles.smallButton, styles.cancelBtn]}
                    onPress={() => {
                      setIsUpdatingPassword(false);
                      setNewPassword('');
                    }}
                  >
                    <Text style={styles.smallButtonText}>{t('Cancel')}</Text>
                  </TouchableOpacity>
                  <TouchableOpacity
                    style={[styles.smallButton, styles.confirmBtn]}
                    onPress={handleChangePassword}
                    disabled={saving}
                  >
                    {saving ? (
                      <ActivityIndicator size="small" color="#fff" />
                    ) : (
                      <Text style={styles.smallButtonText}>{t('Save')}</Text>
                    )}
                  </TouchableOpacity>
                </View>
              </View>
            </>
          ) : (
            <TouchableOpacity
              style={styles.changeButton}
              onPress={() => setIsUpdatingPassword(true)}
            >
              <Text style={styles.changeButtonText}>{t('Change password')}</Text>
            </TouchableOpacity>
          )}

          {/* Logout */}
          <TouchableOpacity
            style={styles.logoutButton}
            onPress={handleLogout}
          >
            <Ionicons name="log-out-outline" size={18} color="#fff" />
            <Text style={styles.logoutButtonText}>{t('Log Out')}</Text>
          </TouchableOpacity>

          {/* Delete Account */}
          <TouchableOpacity
            style={styles.deleteButton}
            onPress={handleDeleteAccount}
          >
            <Ionicons name="trash-outline" size={18} color="#fff" />
            <Text style={styles.deleteButtonText}>{t('Delete Account')}</Text>
          </TouchableOpacity>
        </View>

        {/* LANGUAGE SECTION */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>{t('Language')}</Text>
          <Text style={styles.label}>
            {t('Selected Language')}: {currentLang.toUpperCase()}
          </Text>
          <View style={styles.langButtons}>
            <TouchableOpacity
              onPress={() => handleLanguageChange('en')}
              style={[
                styles.langButton,
                currentLang === 'en' && styles.langButtonSelected,
              ]}
            >
              <Text
                style={[
                  styles.langButtonText,
                  currentLang === 'en' && styles.langButtonTextSelected,
                ]}
              >
                English
              </Text>
            </TouchableOpacity>
            <TouchableOpacity
              onPress={() => handleLanguageChange('fr')}
              style={[
                styles.langButton,
                currentLang === 'fr' && styles.langButtonSelected,
              ]}
            >
              <Text
                style={[
                  styles.langButtonText,
                  currentLang === 'fr' && styles.langButtonTextSelected,
                ]}
              >
                Français
              </Text>
            </TouchableOpacity>
          </View>
        </View>

        {/* NOTIFICATIONS SECTION */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>{t('Notifications')}</Text>
          <View style={styles.toggleRow}>
            <Text style={styles.toggleLabel}>{t('Push Notifications')}</Text>
            <Switch
              value={pushNotificationsEnabled}
              onValueChange={(val) => {
                setPushNotificationsEnabled(val);
                handleSaveNotificationSettings(val);
              }}
              thumbColor={pushNotificationsEnabled ? COLORS.primary : '#ccc'}
              trackColor={{ true: COLORS.primary, false: '#ddd' }}
            />
          </View>
          {/* If more notification settings are needed, add them here */}
        </View>

        {/* DISPLAY / THEME SECTION */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>{t('Appearance')}</Text>
          <View style={styles.toggleRow}>
            <Text style={styles.toggleLabel}>{t('Dark Mode')}</Text>
            <Switch
              value={darkModeEnabled}
              onValueChange={(val) => {
                setDarkModeEnabled(val);
                handleSaveDarkMode(val);
              }}
              thumbColor={darkModeEnabled ? COLORS.primary : '#ccc'}
              trackColor={{ true: COLORS.primary, false: '#ddd' }}
            />
          </View>
        </View>

        

        {/* ANY ADDITIONAL SETTINGS SECTIONS, E.G. PREFERENCES, ETC. */}
        {/* For example, manage advanced filters, e.g., prefered categories, watering needs, etc. */}
        {/* ... */}

        <View style={{ height: 40 }} />
      </ScrollView>
    </SafeAreaProvider>
  );
};

export default SettingsScreen;

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: COLORS.background,
  },

  scrollContent: {
    paddingBottom: 40,
    paddingHorizontal: 20,
  },
  section: {
    backgroundColor: '#fff',
    borderRadius: 12,
    padding: 16,
    marginBottom: 12,
  },
  sectionTitle: {
    fontSize: 18,
    color: COLORS.textDark,
    fontWeight: '600',
    marginBottom: 10,
  },
  label: {
    fontSize: 14,
    color: COLORS.textDark,
    fontWeight: '600',
    marginBottom: 4,
  },
  value: {
    fontSize: 14,
    color: '#555',
    marginBottom: 6,
  },
  langButtons: {
    flexDirection: 'row',
    marginTop: 8,
  },
  langButton: {
    paddingVertical: 8,
    paddingHorizontal: 12,
    borderWidth: 1,
    borderColor: COLORS.accentGreen,
    borderRadius: 20,
    marginRight: 10,
  },
  langButtonSelected: {
    backgroundColor: COLORS.accentGreen,
  },
  langButtonText: {
    fontSize: 14,
    color: COLORS.accentGreen,
  },
  langButtonTextSelected: {
    color: COLORS.textLight,
    fontWeight: '600',
  },
  toggleRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginVertical: 8,
  },
  toggleLabel: {
    fontSize: 14,
    color: COLORS.textDark,
  },
  changeButton: {
    borderWidth: 1,
    borderColor: COLORS.accentGreen,
    borderRadius: 8,
    paddingVertical: 8,
    paddingHorizontal: 12,
    alignSelf: 'flex-start',
    marginTop: 6,
  },
  changeButtonText: {
    color: COLORS.accentGreen,
    fontSize: 14,
    fontWeight: '600',
  },
  // For changing email/password
  changeContainer: {
    marginVertical: 8,
  },
  input: {
    borderWidth: 1,
    borderColor: '#ccc',
    borderRadius: 8,
    paddingHorizontal: 10,
    paddingVertical: 6,
    marginBottom: 6,
    fontSize: 14,
  },
  buttonRow: {
    flexDirection: 'row',
    justifyContent: 'flex-end',
  },
  smallButton: {
    borderRadius: 8,
    paddingVertical: 8,
    paddingHorizontal: 12,
    marginLeft: 10,
  },
  cancelBtn: {
    borderWidth: 1,
    borderColor: COLORS.primary,
  },
  confirmBtn: {
    backgroundColor: COLORS.primary,
  },
  smallButtonText: {
    fontSize: 14,
    fontWeight: '600',
    color: '#fff',
  },
  // Logout and Delete
  logoutButton: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: COLORS.accentGreen,
    borderRadius: 8,
    paddingVertical: 8,
    paddingHorizontal: 12,
    marginTop: 12,
  },
  logoutButtonText: {
    marginLeft: 6,
    fontSize: 14,
    color: '#fff',
    fontWeight: '600',
  },
  deleteButton: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: COLORS.accentRed,
    borderRadius: 8,
    paddingVertical: 8,
    paddingHorizontal: 12,
    marginTop: 12,
  },
  deleteButtonText: {
    marginLeft: 6,
    fontSize: 14,
    color: '#fff',
    fontWeight: '600',
  },
  centered: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
});
