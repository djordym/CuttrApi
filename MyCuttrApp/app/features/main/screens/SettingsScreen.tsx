// File: src/features/main/screens/SettingsScreen.tsx

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
  Keyboard,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { Ionicons, MaterialIcons } from '@expo/vector-icons';
import { useTranslation } from 'react-i18next';

import { SafeAreaProvider } from 'react-native-safe-area-context';
import { storage } from '../../../utils/storage';
import { userService } from '../../../api/userService';
import { useMyProfile } from '../hooks/useMyProfileHooks';
import { useNavigation } from '@react-navigation/native';
import { logout } from '../../auth/store/authSlice';
import { store } from '../../../store';
import { COLORS } from '../../../theme/colors';
import { headerStyles } from '../styles/headerStyles';
import { authService } from '../../../api/authService';
import { useQueryClient } from 'react-query';

const SettingsScreen: React.FC = () => {
  const { t, i18n } = useTranslation();
  const navigation = useNavigation();
  const queryClient = useQueryClient();

  // For user data
  const {
    data: userProfile,
    isLoading: userLoading,
    refetch: refetchUserProfile,
  } = useMyProfile();

  // Language management
  const [currentLang, setCurrentLang] = useState(i18n.language);

  // Toggles
  const [pushNotificationsEnabled, setPushNotificationsEnabled] = useState(false);
  const [darkModeEnabled, setDarkModeEnabled] = useState(false);

  // Edit states for email/password
  const [isEditingEmail, setIsEditingEmail] = useState(false);
  const [isEditingPassword, setIsEditingPassword] = useState(false);

  // Local states for new email/password input
  const [newEmail, setNewEmail] = useState('');
  const [newPassword, setNewPassword] = useState('');

  // "Saving" spinner
  const [saving, setSaving] = useState(false);

  // Handle language change
  const handleLanguageChange = async (lang: string) => {
    await i18n.changeLanguage(lang);
    await storage.saveLanguage(lang);
    setCurrentLang(lang);
  };

  // Stub for changing email
  const handleChangeEmail = async () => {
    if (!newEmail.trim()) {
      Alert.alert(t('Validation Error'), t('settings_enter_valid_email'));
      return;
    }
    setSaving(true);
    Keyboard.dismiss();
    try {
      // Example: await userService.updateProfile({ email: newEmail });
      Alert.alert(t('Success'), t('settings_email_changed_successfully'));
      setNewEmail('');
      refetchUserProfile();
      setIsEditingEmail(false);
    } catch (err) {
      console.error('Failed to change email:', err);
      Alert.alert(t('Error'), t('settings_could_not_change_email'));
    } finally {
      setSaving(false);
    }
  };

  // Stub for changing password
  const handleChangePassword = async () => {
    if (!newPassword.trim()) {
      Alert.alert(t('Validation Error'), t('settings_enter_valid_password'));
      return;
    }
    setSaving(true);
    Keyboard.dismiss();
    try {
      // Example: await userService.updateProfile({ password: newPassword });
      Alert.alert(t('Success'), t('settings_password_changed_successfully'));
      setNewPassword('');
      refetchUserProfile();
      setIsEditingPassword(false);
    } catch (err) {
      console.error('Failed to change password:', err);
      Alert.alert(t('Error'), t('settings_could_not_change_password'));
    } finally {
      setSaving(false);
    }
  };

  // Logout
  const handleLogout = async () => {
    Alert.alert(
      t('Logout'),
      t('settings_logout_confirmation'),
      [
        { text: t('Cancel'), style: 'cancel' },
        {
          text: t('Yes'),
          style: 'destructive',
          onPress: async () => {
            queryClient.clear();
            await storage.clearTokens();
            authService.logout();
            store.dispatch(logout());
          },
        },
      ],
    );
  };

  // Delete account
  const handleDeleteAccount = async () => {
    Alert.alert(
      t('Delete Account'),
      t('settings_delete_account_not_implemented'),
      [
        { text: t('Cancel'), style: 'cancel' },
        {
          // Uncomment and implement when ready:
          // text: t('Yes, Delete'),
          // style: 'destructive',
          // onPress: async () => { ... },
        },
      ],
    );
  };

  // Not implemented yet
  const handleNotImplemented = () => {
    Alert.alert(t('Not Implemented'), t('settings_feature_not_implemented'));
  };

  // Save toggles
  const handleSaveNotificationSettings = async (value: boolean) => {
    handleNotImplemented(); 
    // e.g. userPreferencesService.updateNotifications(value);
  };

  const handleSaveDarkMode = async (value: boolean) => {
    handleNotImplemented();
    // e.g. userPreferencesService.updateDarkMode(value);
  };

  if (userLoading) {
    return (
      <View style={styles.centered}>
        <ActivityIndicator size="large" color={COLORS.primary} />
      </View>
    );
  }

  // RENDER ROWS

  // Email Row
  const renderEmailRow = () => {
    if (!isEditingEmail) {
      return (
        <View style={styles.row}>
          <View style={styles.rowLeft}>
            <Text style={styles.rowLabel}>{t('email')}</Text>
            <Text style={styles.rowValue}>
              {userProfile?.email || t('settings_no_email_found')}
            </Text>
          </View>
        </View>
      );
    }

    return (
      <View style={styles.row}>
        <View style={{ flex: 1 }}>
          <Text style={styles.rowLabel}>{t('Edit Email')}</Text>
          <TextInput
            style={styles.editInput}
            value={newEmail}
            onChangeText={setNewEmail}
            autoCapitalize="none"
            keyboardType="email-address"
          />
        </View>

        <View style={styles.rowActions}>
          <TouchableOpacity
            onPress={handleChangeEmail}
            style={{ marginRight: 12 }}
            disabled={saving}
          >
            {saving ? (
              <ActivityIndicator size="small" color={COLORS.accentGreen} />
            ) : (
              <Ionicons name="checkmark-circle" size={24} color={COLORS.accentGreen} />
            )}
          </TouchableOpacity>
          <TouchableOpacity
            onPress={() => {
              setIsEditingEmail(false);
              setNewEmail('');
            }}
          >
            <Ionicons name="close-circle" size={24} color={COLORS.accentRed} />
          </TouchableOpacity>
        </View>
      </View>
    );
  };

  // Password Row
  const renderPasswordRow = () => {
    if (!isEditingPassword) {
      return (
        <View style={styles.row}>
          <View style={styles.rowLeft}>
            <Text style={styles.rowLabel}>{t('Password')}</Text>
            <Text style={styles.rowValue}>********</Text>
          </View>
          <TouchableOpacity
            onPress={() => {
              setIsEditingPassword(true);
              setNewPassword('');
            }}
          >
            <Ionicons name="pencil-outline" size={18} color={COLORS.accentGreen} />
          </TouchableOpacity>
        </View>
      );
    }

    return (
      <View style={styles.row}>
        <View style={{ flex: 1 }}>
          <Text style={styles.rowLabel}>{t('New Password')}</Text>
          <TextInput
            style={styles.editInput}
            value={newPassword}
            onChangeText={setNewPassword}
            secureTextEntry
            autoCapitalize="none"
          />
        </View>

        <View style={styles.rowActions}>
          <TouchableOpacity
            onPress={handleChangePassword}
            style={{ marginRight: 12 }}
            disabled={saving}
          >
            {saving ? (
              <ActivityIndicator size="small" color={COLORS.accentGreen} />
            ) : (
              <Ionicons name="checkmark-circle" size={24} color={COLORS.accentGreen} />
            )}
          </TouchableOpacity>
          <TouchableOpacity
            onPress={() => {
              setIsEditingPassword(false);
              setNewPassword('');
            }}
          >
            <Ionicons name="close-circle" size={24} color={COLORS.accentRed} />
          </TouchableOpacity>
        </View>
      </View>
    );
  };

  return (
    <SafeAreaProvider style={styles.safeArea}>
      {/* Header */}
      <LinearGradient
        colors={[COLORS.primary, COLORS.secondary]}
        style={headerStyles.headerGradient}
      >
        <View style={headerStyles.headerRow}>
          <Text style={headerStyles.headerTitle}>{t('Settings')}</Text>
          <MaterialIcons name="settings" size={24} color="#fff" />
        </View>
      </LinearGradient>

      {/* Content */}
      <ScrollView contentContainerStyle={styles.scrollContent}>
        {/* ACCOUNT INFO SECTION */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>{t('Account')}</Text>
          {renderEmailRow()}
          {/* Uncomment to allow password editing */}
          {/* {renderPasswordRow()} */}

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
              thumbColor={pushNotificationsEnabled ? COLORS.accentGreen : '#ccc'}
              trackColor={{ true: COLORS.primary, false: '#ddd' }}
            />
          </View>
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
              thumbColor={darkModeEnabled ? COLORS.accentGreen : '#ccc'}
              trackColor={{ true: COLORS.primary, false: '#ddd' }}
            />
          </View>
        </View>

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
  centered: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },

  /* Sections */
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

  /* Row for email, password, etc. */
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 16,
  },
  rowLeft: {
    flex: 1,
  },
  rowLabel: {
    fontSize: 14,
    color: COLORS.textDark,
    fontWeight: '600',
    marginBottom: 2,
  },
  rowValue: {
    fontSize: 14,
    color: '#555',
  },
  rowActions: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  editInput: {
    borderWidth: 1,
    borderColor: '#ccc',
    borderRadius: 8,
    paddingHorizontal: 10,
    paddingVertical: 6,
    fontSize: 14,
    marginTop: 6,
  },

  /* Logout + Delete buttons */
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

  /* Language */
  label: {
    fontSize: 14,
    color: COLORS.textDark,
    fontWeight: '600',
    marginBottom: 4,
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

  /* Notifications / Appearance toggles */
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
});
