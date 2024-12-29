import React, { useState } from 'react';
import { View, Text, Image, StyleSheet, TouchableOpacity, KeyboardAvoidingView } from 'react-native';
import { useSelector, useDispatch } from 'react-redux';
import TextInputField from '../../../components/common/TextInputField';
import BrandedButton from '../../../components/common/BrandedButton';
import ErrorMessage from '../../../components/feedback/ErrorMessage';
import { loginThunk } from '../store/authSlice';
import { RootState } from '../../../store';
import { useNavigation } from '@react-navigation/native';
import { useTranslation } from 'react-i18next';
import { UserLoginRequest } from '../../../../app/types/apiTypes';
import { useAppDispatch, useAppSelector } from '../../../store/hooks';
import { log } from '../../../utils/logger';

const LoginScreen = () => {
  const { t } = useTranslation();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const { status, error } = useAppSelector((state: RootState) => state.auth);
  const dispatch = useAppDispatch();
  const navigation = useNavigation();

  const handleLogin = async () => {
    let userLoginRequest: UserLoginRequest = {
      email: email,
      password: password
    };
    log.debug('Pressed login button, userLoginRequest:', userLoginRequest);
    await dispatch(loginThunk(userLoginRequest));
  };

  return (
    <KeyboardAvoidingView style={styles.container} behavior="padding">
      <Image source={require('../../../../assets/images/logo.png')} style={styles.logo} />
      <Text style={styles.title}>{t('welcome_back')}</Text>
      <ErrorMessage message={error} />

      <TextInputField 
        value={email}
        onChangeText={setEmail}
        placeholder={t('email')}
      />
      <TextInputField 
        value={password}
        onChangeText={setPassword}
        placeholder={t('password')}
        secureTextEntry
      />

      <BrandedButton title={t('login')} onPress={handleLogin} disabled={status === 'loading'} />

      <TouchableOpacity onPress={() => navigation.navigate('Register' as never)}>
        <Text style={styles.link}>{t('no_account_register')}</Text>
      </TouchableOpacity>
    </KeyboardAvoidingView>
  );
};

export default LoginScreen;

const styles = StyleSheet.create({
  container: {
    flex:1,
    justifyContent:'center',
    padding:20,
    backgroundColor:'#fff'
  },
  logo: {
    width:100,
    height:100,
    alignSelf:'center',
    marginBottom:20
  },
  title: {
    fontSize:24,
    fontWeight:'700',
    textAlign:'center',
    marginBottom:20
  },
  link: {
    color:'#1EAE98',
    textAlign:'center',
    marginTop:20
  }
});
