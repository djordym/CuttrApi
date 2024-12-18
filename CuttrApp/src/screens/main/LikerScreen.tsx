import React, { useState, useEffect, useContext } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ActivityIndicator,
  Image,
  Alert,
  Dimensions,
  Platform,
} from 'react-native';
import Swiper from 'react-native-deck-swiper';
import { Plant, PlantExchange, UserProfile } from '../../types/types';
import {
  fetchPlantsToLike,
  putPossiblePlantExchange,
} from '../../api/plantExhangeService';
import { getCurrentUserProfile } from '../../api/profileService';
import AuthContext from '../../context/AuthContext';
import SelectPlantsModal from '../../modals/SelectPlantsModal';
import { SafeAreaView } from 'react-native-safe-area-context';
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
const windowHeight = Dimensions.get('window').height;

SplashScreen.preventAutoHideAsync();

const createOrUpdatePossibleExchanges = async (
  userToken: string,
  ownedPlants: Plant[],
  targetPlant: Plant,
  userApproval: boolean
) => {
  const exchangesToSend = [];
  for (const ownedPlant of ownedPlants) {
    const existingExchange: PlantExchange | undefined = ownedPlant.exchangesAsResponding.find(
      (exchange: PlantExchange) => exchange.initiatingPlantId === targetPlant.plantId
    );

    if (existingExchange) {
      existingExchange.respondingUserApproval = userApproval;
      exchangesToSend.push(existingExchange);
      if (existingExchange.initiatingUserApproval && existingExchange.respondingUserApproval) {
        Alert.alert('Match!', 'You have a match! Go check it out!');
      }
    } else {
      const newExchange: PlantExchange = {
        plantExchangeResponseId: 0,
        initiatingPlantId: ownedPlant.plantId,
        respondingPlantId: targetPlant.plantId,
        initiatingUserApproval: userApproval,
        respondingUserApproval: null,
      };
      exchangesToSend.push(newExchange);
    }
  }

  await putPossiblePlantExchange(userToken, exchangesToSend);
};

const LikerScreen = () => {
  const [userData, setUserData] = useState<UserProfile | null>(null);
  const [plants, setPlants] = useState<Plant[]>([]);
  const [loading, setLoading] = useState(true);
  const { state } = useContext(AuthContext);
  const [modalVisible, setModalVisible] = useState(false);
  const [currentPlant, setCurrentPlant] = useState<Plant | null>(null);

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
    const loadInitialPlants = async () => {
      try {
        const initialPlants = await fetchPlantsToLike(state.userToken, 5);
        setPlants(initialPlants);
      } catch (error) {
        console.error('Failed to load plants:', error);
      } finally {
        setLoading(false);
      }
    };
    loadInitialPlants();

    const fetchUserData = async () => {
      try {
        const data = await getCurrentUserProfile(state.userToken);
        setUserData(data);
      } catch (error) {
        console.error('Failed to fetch user data:', error);
      }
    };
    fetchUserData();
  }, [state.userToken]);

  const handleSwiped = async (cardIndex: number) => {
    if (cardIndex >= plants.length - 1) {
      const newPlant = await fetchPlantsToLike(state.userToken, 1);
      setPlants((prevPlants) => [...prevPlants, ...newPlant]);
    }
  };

  const handleDislike = async (plant: Plant) => {
    try {
      if (userData)
        await createOrUpdatePossibleExchanges(state.userToken, userData.ownedPlants, plant, false);
      else Alert.alert('Error', 'User data not loaded yet');
    } catch (error) {
      console.error('Failed to dislike plant:', error);
    }
  };

  const handleLike = (plant: Plant) => {
    setCurrentPlant(plant);
    setModalVisible(true);
  };

  const handlePlantSelection = async (selectedPlants: Plant[]) => {
    if (!currentPlant) return;
    try {
      await createOrUpdatePossibleExchanges(state.userToken, selectedPlants, currentPlant, true);
    } catch (error) {
      console.error('Failed to like plant:', error);
      Alert.alert('Error', 'Failed to submit plant exchange request.');
    } finally {
      setModalVisible(false);
    }
  };

  if (!fontsLoaded) {
    return null;
  }

  if (loading) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color={COLORS.primary} />
      </View>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.swiperContainer}>
        <Swiper
          cards={plants}
          renderCard={(card) => {
            if (!card) return null;
            return (
              <View style={styles.card}>
                <Image source={{ uri: card.imageUrl }} style={styles.plantImage} />
                <View style={styles.cardDetails}>
                  <Text style={styles.plantTitle}>{card.name}</Text>
                  <Text style={styles.plantDescription}>{card.description}</Text>
                </View>
              </View>
            );
          }}
          onSwiped={(cardIndex) => handleSwiped(cardIndex)}
          onSwipedAll={() => console.log('All cards have been swiped.')}
          cardIndex={0}
          backgroundColor={'transparent'}
          stackSize={3}
          onSwipedLeft={(cardIndex) => handleDislike(plants[cardIndex])}
          onSwipedRight={(cardIndex) => handleLike(plants[cardIndex])}
          overlayLabels={{
            left: {
              title: 'NOPE',
              style: {
                label: {
                  backgroundColor: 'transparent',
                  borderColor: COLORS.accent,
                  color: COLORS.accent,
                  borderWidth: 2,
                  fontSize: 32,
                  fontFamily: 'Montserrat_700Bold',
                  padding: 10,
                },
                wrapper: {
                  flexDirection: 'column',
                  alignItems: 'flex-end',
                  justifyContent: 'flex-start',
                  marginTop: 20,
                  marginLeft: -20,
                },
              },
            },
            right: {
              title: 'LIKE',
              style: {
                label: {
                  backgroundColor: 'transparent',
                  borderColor: COLORS.primary,
                  color: COLORS.primary,
                  borderWidth: 2,
                  fontSize: 32,
                  fontFamily: 'Montserrat_700Bold',
                  padding: 10,
                },
                wrapper: {
                  flexDirection: 'column',
                  alignItems: 'flex-start',
                  justifyContent: 'flex-start',
                  marginTop: 20,
                  marginLeft: 20,
                },
              },
            },
          }}
        />
      </View>
      {userData && (
        <SelectPlantsModal
          visible={modalVisible}
          onClose={() => setModalVisible(false)}
          ownedPlants={userData.ownedPlants}
          onConfirm={handlePlantSelection}
        />
      )}
    </SafeAreaView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: COLORS.background,
  },
  swiperContainer: {
    flex: 1,
    paddingTop: 20,
  },
  card: {
    flex: 0.75,
    borderRadius: 15,
    backgroundColor: COLORS.white,
    shadowColor: '#000',
    shadowOpacity: 0.1,
    shadowRadius: 5,
    elevation: 3,
    overflow: 'hidden',
  },
  plantImage: {
    width: '100%',
    height: '65%',
  },
  cardDetails: {
    padding: 15,
  },
  plantTitle: {
    fontSize: 24,
    fontFamily: 'Montserrat_700Bold',
    color: COLORS.primary,
    marginBottom: 10,
  },
  plantDescription: {
    fontSize: 16,
    fontFamily: 'Montserrat_400Regular',
    color: COLORS.text,
  },
});

export default LikerScreen;
