import React, { useState } from 'react';
import {
  View,
  Text,
  Image,
  KeyboardAvoidingView,
  TouchableOpacity,
  ActivityIndicator,
  Alert,
  ScrollView,
  Platform,
} from 'react-native';
import { useNavigation } from '@react-navigation/native';
import { useTranslation } from 'react-i18next';
import { LinearGradient } from 'expo-linear-gradient';

import { useAppSelector, useAppDispatch } from '../../../store/hooks';
import { RootState } from '../../../store';
import { loginThunk } from '../store/authSlice';
import TextInputField from '../../../components/common/TextInputField';
import ErrorMessage from '../../../components/feedback/ErrorMessage';

import { COLORS } from '../../../theme/colors';
import { log } from '../../../utils/logger';
import { UserLoginRequest } from '../../../types/apiTypes';

// Import the shared auth styles
import { authStyles as styles } from '../styles/authStyles'; // <- Adjust path if needed

const LoginScreen = () => {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const navigation = useNavigation();
  const [loading, setLoading] = useState(false);

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const { status, error } = useAppSelector((state: RootState) => state.auth);

  const handleLogin = async () => {
    if (!email.trim() || !password.trim()) {
      Alert.alert(t('error_title'), t('Please enter your email and password.'));
      return;
    }
    const userLoginRequest: UserLoginRequest = {
      email: email.trim(),
      password: password.trim(),
    };
    log.debug('Pressed login button, userLoginRequest:', userLoginRequest);
    await dispatch(loginThunk(userLoginRequest));
  };

  return (
    <LinearGradient
      colors={[COLORS.primary, COLORS.secondary]}
      style={styles.gradientBackground}
    >
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        keyboardShouldPersistTaps="handled"
      >
        <KeyboardAvoidingView
          behavior={Platform.OS === 'ios' ? 'padding' : undefined}
          style={styles.keyboardAvoid}
        >
          <View style={styles.logoContainer}>
            <Image
              source={require('../../../../assets/images/logo.png')}
              style={styles.logo}
            />
          </View>

          <View style={styles.cardContainer}>
            <Text style={styles.title}>{t('welcome_back')}</Text>

            <ErrorMessage message={error} />

            {/* Email */}
            <TextInputField
              value={email}
              onChangeText={setEmail}
              placeholder={t('email')}
            />

            {/* Password */}
            <TextInputField
              value={password}
              onChangeText={setPassword}
              placeholder={t('password')}
              secureTextEntry
            />

            {/* Submit */}
            <TouchableOpacity
              style={styles.confirmButton}
              onPress={handleLogin}
              disabled={loading}
            >
              {loading ? (
                <ActivityIndicator size="small" color={COLORS.accentGreen} />
              ) : (
                <Text style={styles.confirmButtonText}>{t('login_button')}</Text>
              )}
            </TouchableOpacity>

            {/* Register link */}
            <TouchableOpacity
              onPress={() => navigation.navigate('Register' as never)}
              style={styles.navLinkContainer}
            >
              <Text style={styles.navLinkText}>
                {t('no_account_register')}
              </Text>
            </TouchableOpacity>
          </View>
        </KeyboardAvoidingView>
      </ScrollView>
    </LinearGradient>
  );
};

export default LoginScreen;
