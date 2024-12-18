import React, { useContext, useState, useEffect } from 'react';
import {
  ScrollView,
  View,
  Text,
  Image,
  StyleSheet,
  Platform,
  Dimensions,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import AuthContext from '../../context/AuthContext';
import { Match } from '../../types/types';
import { getMatches } from '../../api/plantExhangeService';
import { useIsFocused } from '@react-navigation/native';
import { useFonts, Montserrat_400Regular, Montserrat_700Bold } from '@expo-google-fonts/montserrat';
import * as SplashScreen from 'expo-splash-screen';
import { Ionicons } from '@expo/vector-icons';

const COLORS = {
  primary: '#3EB489',
  secondary: '#FDCB6E',
  background: '#F2F2F2',
  accent: '#FF6F61',
  text: '#2F4F4F',
  white: '#FFFFFF',
};

const windowWidth = Dimensions.get('window').width;

SplashScreen.preventAutoHideAsync();

const MatchesScreen = () => {
  const { state } = useContext(AuthContext);
  const [matches, setMatches] = useState<Match[]>([]);
  const isFocused = useIsFocused();

  const [fontsLoaded] = useFonts({
    Montserrat_400Regular,
    Montserrat_700Bold,
  });

  useEffect(() => {
    if (fontsLoaded) {
      SplashScreen.hideAsync();
    }
  }, [fontsLoaded]);

  useEffect(() => {
    const fetchMatches = async () => {
      try {
        const data = await getMatches(state.userToken);
        setMatches(data);
        console.log('Matches fetched successfully:', data);
      } catch (error) {
        console.error('Failed to fetch matches:', error);
      }
    };
    fetchMatches();
  }, [isFocused]);

  if (!fontsLoaded) {
    return null;
  }

  return (
    <SafeAreaView style={styles.container}>
      <ScrollView contentContainerStyle={styles.scrollContent}>
        <Text style={styles.header}>Matches</Text>
        {matches.map((match, index) => (
          <View key={index} style={styles.matchCard}>
            <View style={styles.matchDetails}>
              <View style={styles.userContainer}>
                <Image source={{ uri: match.plant1.imageUrl }} style={styles.plantImage} />
                <Text style={styles.plantTitle}>{match.plant1.name}</Text>
              </View>
              <Ionicons name="swap-horizontal" size={30} color={COLORS.primary} />
              <View style={styles.userContainer}>
                <Image source={{ uri: match.plant2.imageUrl }} style={styles.plantImage} />
                <Text style={styles.plantTitle}>{match.plant2.name}</Text>
              </View>
            </View>
          </View>
        ))}
      </ScrollView>
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
  scrollContent: {
    paddingBottom: 20,
  },
  header: {
    fontSize: 28,
    fontFamily: 'Montserrat_700Bold',
    marginTop: 30,
    marginBottom: 20,
    marginHorizontal: 20,
    color: COLORS.text,
  },
  matchCard: {
    backgroundColor: COLORS.white,
    borderRadius: 15,
    marginHorizontal: 20,
    marginBottom: 20,
    paddingVertical: 20,
    paddingHorizontal: 15,
    alignItems: 'center',
    ...Platform.select({
      ios: {
        shadowColor: '#000',
        shadowOpacity: 0.1,
        shadowRadius: 5,
      },
      android: {
        elevation: 3,
      },
    }),
  },
  matchDetails: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  userContainer: {
    alignItems: 'center',
    width: (windowWidth - 120) / 2,
  },
  plantImage: {
    width: 100,
    height: 150,
    borderRadius: 10,
    marginBottom: 10,
  },
  plantTitle: {
    fontSize: 16,
    fontFamily: 'Montserrat_700Bold',
    color: COLORS.primary,
    textAlign: 'center',
  },
});

export default MatchesScreen;
