import React from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import AuthContext from './src/context/AuthContext';
import AuthProvider from './src/context/AuthProvider';
import SplashScreen from './src/screens/auth/SplashScreen';
import AuthNavigator from './src/navigation/AuthNavigator';
import MainAppNavigator from './src/navigation/MainAppNavigator';
import { useContext } from 'react';
import { signOut } from './src/context/AuthProvider';
import { useEffect, useState } from 'react';
import { validateToken } from './src/api/authService';


export default function App() {
  return (
    <SafeAreaProvider>
      <AuthProvider>
        <NavigationContainer>
          <AppContent />
        </NavigationContainer>
      </AuthProvider>
    </SafeAreaProvider>
  );
}

const AppContent = () => {
  const { state } = useContext(AuthContext);
  const [isTokenValid, setIsTokenValid] = useState(false);

  if (state.isLoading) {
    return <SplashScreen />;
  }

  return state.userToken ? <MainAppNavigator /> : <AuthNavigator />;
};

