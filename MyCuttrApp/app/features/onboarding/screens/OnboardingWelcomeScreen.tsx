import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity } from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { useNavigation } from '@react-navigation/native';
import { COLORS } from '../../../theme/colors';

const OnboardingWelcomeScreen: React.FC = () => {
  const navigation = useNavigation();

  const handleNextPress = () => {
    navigation.navigate('OnboardingBio' as never);
  };

  return (
    <LinearGradient colors={[COLORS.primary, COLORS.secondary]} style={styles.container}>
      <View style={styles.content}>
        <Text style={styles.title}>Welcome to Cuttr!</Text>
        <Text style={styles.subtitle}>
          Expand and diversify your plant collection effortlessly. Cuttr lets you trade unique, free plant cuttings from the coolest, hard‐to‐find species – no more relying on big greenhouses that aren’t eco–friendly. Share your cuttings and discover new gems to brighten up your space!
        </Text>
        <TouchableOpacity onPress={handleNextPress} style={styles.button}>
          <Text style={styles.buttonText}>Next: Tell Us About You</Text>
        </TouchableOpacity>
      </View>
    </LinearGradient>
  );
};

export default OnboardingWelcomeScreen;

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  content: {
    flex: 1,
    paddingHorizontal: 20,
    justifyContent: 'center',
  },
  title: {
    fontSize: 32,
    fontWeight: '700',
    color: COLORS.textLight,
    textAlign: 'center',
    marginBottom: 20,
  },
  subtitle: {
    fontSize: 18,
    color: COLORS.textLight,
    textAlign: 'center',
    marginBottom: 40,
    lineHeight: 26,
  },
  button: {
    backgroundColor: COLORS.accentGreen,
    paddingVertical: 14,
    paddingHorizontal: 20,
    borderRadius: 8,
    alignSelf: 'center',
  },
  buttonText: {
    color: COLORS.textLight,
    fontSize: 18,
    fontWeight: '600',
  },
});
