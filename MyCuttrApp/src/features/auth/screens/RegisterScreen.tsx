import React, { useState } from 'react';
import { View, Text, StyleSheet, KeyboardAvoidingView, Image, TouchableOpacity } from 'react-native';
import { useSelector, useDispatch } from 'react-redux';
import TextInputField from '../../../components/common/TextInputField';
import BrandedButton from '../../../components/common/BrandedButton';
import ErrorMessage from '../../../components/feedback/ErrorMessage';
import { registerThunk } from '../store/authSlice';
import { RootState } from '../../../store';
import { useNavigation } from '@react-navigation/native';
import { useTranslation } from 'react-i18next';

const RegisterScreen = () => {
  const { t } = useTranslation();
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const { status, error } = useSelector((state: RootState) => state.auth);
  const dispatch = useDispatch();
  const navigation = useNavigation();

  const handleRegister = async () => {
    await dispatch(registerThunk({ email, password, name }));
  };

  return (
    <KeyboardAvoidingView style={styles.container} behavior="padding">
      <Image source={require('../../../../assets/images/logo.png')} style={styles.logo} />
      <Text style={styles.title}>{t('create_account')}</Text>
      <ErrorMessage message={error} />

      <TextInputField
        value={name}
        onChangeText={setName}
        placeholder={t('name')}
      />
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

      <BrandedButton title={t('register')} onPress={handleRegister} disabled={status === 'loading'} />
      <TouchableOpacity onPress={() => navigation.navigate('Login' as never)}>
        <Text style={styles.link}>{t('have_account_login')}</Text>
      </TouchableOpacity>
    </KeyboardAvoidingView>
  );
};

export default RegisterScreen;

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
