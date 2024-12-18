import React, { useEffect, useState, useContext } from 'react';
import {
  ScrollView,
  View,
  Text,
  Image,
  StyleSheet,
  TextInput,
  Alert,
  TouchableOpacity,
  Platform,
  Dimensions,
} from 'react-native';
import {
  getCurrentUserProfile,
  patchUserProfile,
  uploadProfilePicture,
  addPlantWithImage,
} from '../../api/profileService';
import { Plant, UserProfile } from '../../types/types';
import { SafeAreaView } from 'react-native-safe-area-context';
import SplashScreen from '../auth/SplashScreen';
import AuthContext from '../../context/AuthContext';
import * as ImagePicker from 'expo-image-picker';
import AddPlantModal from '../../modals/AddPlantModal';
import { Ionicons } from '@expo/vector-icons';
import { useFonts, Montserrat_400Regular, Montserrat_700Bold } from '@expo-google-fonts/montserrat';

const COLORS = {
  primary: '#3EB489',
  secondary: '#FDCB6E',
  background: '#F2F2F2',
  accent: '#FF6F61',
  text: '#2F4F4F',
  white: '#FFFFFF',
};

const windowWidth = Dimensions.get('window').width;

const UserProfileScreen = () => {
  const [userData, setUserData] = useState<UserProfile | null>(null);
  const { state, signOut } = useContext(AuthContext);
  const [userName, setUserName] = useState('');
  const [description, setDescription] = useState('');
  const [quote, setQuote] = useState('');
  const [profilePhotoPath, setProfilePhotoPath] = useState('');
  const [modalVisible, setModalVisible] = useState(false);
  const [refreshToggle, setRefreshToggle] = useState(false);

  let [fontsLoaded] = useFonts({
    Montserrat_400Regular,
    Montserrat_700Bold,
  });

  useEffect(() => {
    const fetchUserData = async () => {
      try {
        const data = await getCurrentUserProfile(state.userToken);
        setUserData(data);
        setUserName(data.userName);
        setDescription(data.description);
        setQuote(data.quote);
        setProfilePhotoPath(data.profilePhotoPath);
      } catch (error) {
        console.error('Failed to fetch user data:', error);
        signOut();
      }
    };
    fetchUserData();
  }, [state.userToken, refreshToggle]);

  const updateProfile = async (field: string, value: string) => {
    try {
      const patchDocument = [{ op: 'replace', path: `/${field}`, value }];
      await patchUserProfile(state.userToken, patchDocument);
    } catch (error) {
      console.error(`Failed to update ${field}:`, error);
    }
  };

  const pickProfileImage = async () => {
    let result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ImagePicker.MediaTypeOptions.Images,
      allowsEditing: true,
      aspect: [3, 3],
      quality: 1,
    });
    console.debug('result: ', result);
    if (!result.canceled && result.assets && result.assets.length > 0) {
      let asset = result.assets[0];
      setProfilePhotoPath(asset.uri);
      await uploadProfilePicture(state.userToken, asset);
    }
  };

  const handleAddPlant = async (
    plantinfo: { name: string; description: string; imageUrl: string },
    asset: any
  ) => {
    try {
      if (userData) {
        let plantObj: Plant = {
          plantId: -1,
          name: plantinfo.name,
          description: plantinfo.description,
          imageUrl: plantinfo.imageUrl,
          userId: userData.userId,
          exchangesAsInitiating: [],
          exchangesAsResponding: [],
        };
        userData.ownedPlants.push(plantObj);
      }
      await addPlantWithImage(state.userToken, plantinfo, asset);
      setRefreshToggle(!refreshToggle);
    } catch (error) {
      console.error('Failed to add plant:', error);
      Alert.alert('Failed to add plant');
    }
  };

  if (!userData) {
    return <SplashScreen />;
  }

  return (
    <SafeAreaView style={styles.container}>
      <ScrollView contentContainerStyle={styles.scrollContent}>
        <View style={styles.profileHeader}>
          <TouchableOpacity style={styles.profilePicContainer} onPress={pickProfileImage}>
            <Image
              source={
                profilePhotoPath
                  ? { uri: profilePhotoPath }
                  : require('../../assets/defaultprofilepic.png')
              }
              style={styles.profilePic}
            />
            <View style={styles.editIconContainer}>
              <Ionicons name="camera" size={20} color="#fff" />
            </View>
          </TouchableOpacity>
          <TextInput
            style={styles.userName}
            value={userName}
            onChangeText={(text) => {
              setUserName(text);
              updateProfile('userName', text);
            }}
            placeholder="Your Name"
            placeholderTextColor="#fff"
          />
          <TextInput
            style={styles.quote}
            value={quote}
            onChangeText={(text) => {
              setQuote(text);
              updateProfile('quote', text);
            }}
            placeholder="Your Favorite Quote"
            placeholderTextColor="#f0f0f0"
          />
        </View>

        <Text style={styles.sectionTitle}>About Me</Text>
        <TextInput
          style={styles.description}
          value={description}
          onChangeText={(text) => {
            setDescription(text);
            updateProfile('description', text);
          }}
          placeholder="Tell us about yourself"
          placeholderTextColor="#888"
          multiline
        />

        <Text style={styles.sectionTitle}>My Plants</Text>
        <View style={styles.plantList}>
          {userData.ownedPlants.map((plant, index) => (
            <View key={index} style={styles.plantCard}>
              <Image source={{ uri: plant.imageUrl }} style={styles.plantImage} />
              <View style={styles.plantInfo}>
                <Text style={styles.plantTitle}>{plant.name}</Text>
                <Text style={styles.plantDescription}>{plant.description}</Text>
              </View>
            </View>
          ))}
        </View>

        <TouchableOpacity onPress={() => setModalVisible(true)} style={styles.addButton}>
          <Ionicons name="add" size={30} color="#fff" />
        </TouchableOpacity>

        <AddPlantModal
          visible={modalVisible}
          onClose={() => {
            setModalVisible(false);
            setRefreshToggle(!refreshToggle);
          }}
          onAddPlant={handleAddPlant}
        />
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
    paddingBottom: 80,
  },
  profileHeader: {
    backgroundColor: COLORS.primary,
    paddingBottom: 40,
    alignItems: 'center',
    borderBottomLeftRadius: 30,
    borderBottomRightRadius: 30,
    paddingTop: 80,
  },
  profilePicContainer: {
    position: 'absolute',
    top: 30,
    alignItems: 'center',
  },
  profilePic: {
    width: 120,
    height: 120,
    borderRadius: 60,
    borderWidth: 4,
    borderColor: COLORS.white,
    backgroundColor: '#ccc',
  },
  editIconContainer: {
    position: 'absolute',
    bottom: 0,
    right: 0,
    backgroundColor: COLORS.accent,
    borderRadius: 15,
    padding: 5,
  },
  userName: {
    marginTop: 80,
    fontSize: 24,
    fontFamily: 'Montserrat_700Bold',
    color: COLORS.white,
    textAlign: 'center',
    width: '80%',
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
    borderRadius: 10,
    padding: 10,
  },
  quote: {
    marginTop: 10,
    fontSize: 16,
    fontFamily: 'Montserrat_400Regular',
    color: COLORS.white,
    fontStyle: 'italic',
    textAlign: 'center',
    width: '80%',
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
    borderRadius: 10,
    padding: 10,
  },
  sectionTitle: {
    fontSize: 20,
    fontFamily: 'Montserrat_700Bold',
    color: COLORS.text,
    marginTop: 30,
    marginHorizontal: 20,
  },
  description: {
    backgroundColor: COLORS.white,
    borderRadius: 15,
    padding: 15,
    marginHorizontal: 20,
    marginTop: 10,
    fontSize: 16,
    fontFamily: 'Montserrat_400Regular',
    color: COLORS.text,
    textAlignVertical: 'top',
    minHeight: 80,
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
  plantList: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'space-between',
    marginHorizontal: 20,
    marginTop: 20,
  },
  plantCard: {
    width: (windowWidth - 60) / 2,
    backgroundColor: COLORS.white,
    borderRadius: 15,
    marginBottom: 15,
    overflow: 'hidden',
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
  plantImage: {
    width: '100%',
    height: 120,
  },
  plantInfo: {
    padding: 10,
  },
  plantTitle: {
    fontSize: 16,
    fontFamily: 'Montserrat_700Bold',
    color: COLORS.primary,
    marginBottom: 5,
  },
  plantDescription: {
    fontSize: 14,
    fontFamily: 'Montserrat_400Regular',
    color: COLORS.text,
  },
  addButton: {
    backgroundColor: COLORS.accent,
    marginTop: 20,
    width: 60,
    height: 60,
    borderRadius: 30,
    alignItems: 'center',
    justifyContent: 'center',
    alignSelf: 'center',
    ...Platform.select({
      ios: {
        shadowColor: '#000',
        shadowOpacity: 0.3,
        shadowRadius: 5,
      },
      android: {
        elevation: 5,
      },
    }),
  },
});

export default UserProfileScreen;
