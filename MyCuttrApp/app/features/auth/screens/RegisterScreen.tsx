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
import { registerThunk } from '../store/authSlice';
import TextInputField from '../../../components/common/TextInputField';
import ErrorMessage from '../../../components/feedback/ErrorMessage';

import { COLORS } from '../../../theme/colors';
import { log } from '../../../utils/logger';
import { UserRegistrationRequest } from '../../../types/apiTypes';

// Import the shared auth styles
import { authStyles as styles } from '../styles/authStyles'; // <- Adjust path if needed

const RegisterScreen = () => {
  const { t } = useTranslation();
  const dispatch = useAppDispatch();
  const navigation = useNavigation();
  const [loading, setLoading] = useState(false);
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const { status, error } = useAppSelector((state: RootState) => state.auth);

  const handleRegister = async () => {
    if (!name.trim() || !email.trim() || !password.trim()) {
      Alert.alert(t('error_title'), t('Please fill all fields.'));
      return;
    }
    const userRegistrationRequest: UserRegistrationRequest = {
      email: email.trim(),
      password: password.trim(),
      name: name.trim(),
    };
    log.debug('Pressed register button, userRegistrationRequest:', userRegistrationRequest);
    await dispatch(registerThunk(userRegistrationRequest));
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
            <Text style={styles.title}>{t('create_account')}</Text>

            <ErrorMessage message={error} />

            {/* Name */}
            <TextInputField
              value={name}
              onChangeText={setName}
              placeholder={t('name')}
            />

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
              onPress={handleRegister}
              disabled={loading}
            >
              {loading ? (
                <ActivityIndicator size="small" color={COLORS.accentGreen} />
              ) : (
                <Text style={styles.confirmButtonText}>
                  {t('register_button')}
                </Text>
              )}
            </TouchableOpacity>

            {/* Already have account */}
            <TouchableOpacity
              onPress={() => navigation.navigate('Login' as never)}
              style={styles.navLinkContainer}
            >
              <Text style={styles.navLinkText}>
                {t('have_account_login')}
              </Text>
            </TouchableOpacity>
          </View>
        </KeyboardAvoidingView>
      </ScrollView>
    </LinearGradient>
  );
};

export default RegisterScreen;
