import React, { useEffect } from 'react';
import { View, Image, StyleSheet, Dimensions } from 'react-native';
import LottieView from 'lottie-react-native';
import { useFonts, Montserrat_400Regular } from '@expo-google-fonts/montserrat';
import * as SplashScreenExpo from 'expo-splash-screen';

const COLORS = {
  primary: '#3EB489',
  background: '#F2F2F2',
  white: '#FFFFFF',
};

const windowWidth = Dimensions.get('window').width;
const windowHeight = Dimensions.get('window').height;

SplashScreenExpo.preventAutoHideAsync();

function SplashScreen() {
  const [fontsLoaded] = useFonts({
    Montserrat_400Regular,
  });

  useEffect(() => {
    if (fontsLoaded) {
      SplashScreenExpo.hideAsync();
    }
  }, [fontsLoaded]);

  if (!fontsLoaded) {
    return null;
  }

  return (
    <View style={styles.container}>
      <Image source={require('../../assets/app_logo.png')} style={styles.logo} />
      <LottieView
        source={require('../../assets/loading_animation.json')}
        autoPlay
        loop
        style={styles.animation}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
    alignItems: 'center',
    justifyContent: 'center',
  },
  logo: {
    width: windowWidth * 0.6,
    height: windowWidth * 0.6,
    marginBottom: 30,
    resizeMode: 'contain',
  },
  animation: {
    width: windowWidth * 0.2,
    height: windowWidth * 0.2,
  },
});

export default SplashScreen;
