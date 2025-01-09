// File: app/features/onboarding/screens/OnboardingWelcomeScreen.tsx
import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity } from 'react-native';
import { useNavigation } from '@react-navigation/native';

const OnboardingWelcomeScreen: React.FC = () => {
  const navigation = useNavigation();

  const handleNextPress = () => {
    // Go to the location screen next
    navigation.navigate('OnboardingBio' as never);
  };

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Welcome to Cuttr!</Text>
      <Text style={styles.subtitle}>
        Here’s a quick intro to how you can swap plants safely and easily.
      </Text>
      
      {/* More instructions, slides, or content can be shown here. */}
      
      <TouchableOpacity onPress={handleNextPress} style={styles.button}>
        <Text style={styles.buttonText}>Next: Choose Location</Text>
      </TouchableOpacity>
    </View>
  );
};

export default OnboardingWelcomeScreen;

const styles = StyleSheet.create({
  container: {
    flex: 1,
    padding: 20,
    justifyContent: 'center',
  },
  title: {
    fontSize: 28,
    fontWeight: '700',
    marginBottom: 10,
  },
  subtitle: {
    fontSize: 16,
    marginBottom: 40,
  },
  button: {
    backgroundColor: '#1EAE98',
    padding: 14,
    borderRadius: 8,
  },
  buttonText: {
    color: '#fff',
    fontWeight: '600',
    textAlign: 'center',
  },
});
