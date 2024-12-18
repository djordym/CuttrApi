import React, { useState, useContext } from 'react';
import {
  View,
  Text,
  TextInput,
  StyleSheet,
  TouchableOpacity,
  Image,
  Alert,
  Platform,
  ScrollView,
} from 'react-native';
import AuthContext from '../../context/AuthContext';
import { useFonts, Montserrat_400Regular, Montserrat_700Bold } from '@expo-google-fonts/montserrat';
import * as SplashScreen from 'expo-splash-screen';

const COLORS = {
  primary: '#3EB489',
  background: '#F2F2F2',
  accent: '#FF6F61',
  text: '#2F4F4F',
  white: '#FFFFFF',
};

SplashScreen.preventAutoHideAsync();

const LoginScreen = ({ navigation }) => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const { signIn } = useContext(AuthContext);

  const [fontsLoaded] = useFonts({
    Montserrat_400Regular,
    Montserrat_700Bold,
  });

  React.useEffect(() => {
    if (fontsLoaded) {
      SplashScreen.hideAsync();
    }
  }, [fontsLoaded]);

  const handleSignIn = () => {
    try {
      signIn({ email, password });
    } catch (error) {
      Alert.alert('Error', 'Error signing in');
    }
  };

  if (!fontsLoaded) {
    return null;
  }

  return (
    <ScrollView contentContainerStyle={styles.container}>
      <Image source={require('../../assets/app_logo.png')} style={styles.logo} />
      <Text style={styles.title}>Cuttr</Text>
      <Text style={styles.signInText}>Sign In</Text>
      <TextInput
        style={styles.input}
        placeholder="Email"
        value={email}
        onChangeText={setEmail}
        placeholderTextColor="#888"
        keyboardType="email-address"
        autoCapitalize="none"
      />
      <TextInput
        style={styles.input}
        placeholder="Password"
        secureTextEntry
        value={password}
        onChangeText={setPassword}
        placeholderTextColor="#888"
      />
      <TouchableOpacity style={styles.signInButton} onPress={handleSignIn}>
        <Text style={styles.signInButtonText}>Sign In</Text>
      </TouchableOpacity>
      <TouchableOpacity onPress={() => navigation.navigate('SignUp')}>
        <Text style={styles.switchText}>Don't have an account yet? Sign Up</Text>
      </TouchableOpacity>
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  container: {
    flexGrow: 1,
    justifyContent: 'center',
    padding: 20,
    backgroundColor: COLORS.background,
  },
  logo: {
    width: 120,
    height: 120,
    alignSelf: 'center',
    marginBottom: 20,
    resizeMode: 'contain',
  },
  title: {
    fontSize: 32,
    fontFamily: 'Montserrat_700Bold',
    color: COLORS.primary,
    textAlign: 'center',
    marginBottom: 10,
  },
  signInText: {
    fontSize: 24,
    fontFamily: 'Montserrat_700Bold',
    marginBottom: 20,
    color: COLORS.text,
    textAlign: 'center',
  },
  input: {
    marginBottom: 15,
    borderWidth: 1,
    borderColor: COLORS.primary,
    padding: 15,
    borderRadius: 10,
    backgroundColor: COLORS.white,
    fontSize: 16,
    fontFamily: 'Montserrat_400Regular',
    color: COLORS.text,
  },
  signInButton: {
    backgroundColor: COLORS.primary,
    paddingVertical: 15,
    borderRadius: 10,
    alignItems: 'center',
    marginTop: 10,
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
  signInButtonText: {
    fontSize: 18,
    fontFamily: 'Montserrat_700Bold',
    color: COLORS.white,
  },
  switchText: {
    marginTop: 20,
    color: COLORS.primary,
    textAlign: 'center',
    fontSize: 16,
    fontFamily: 'Montserrat_400Regular',
  },
});

export default LoginScreen;
