// File: src/screens/OtherProfileScreen.tsx

import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ActivityIndicator,
  TouchableOpacity,
  Platform,
  ScrollView,
  Alert,
  Dimensions,
} from 'react-native';
import { SafeAreaView, SafeAreaProvider } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';
import { useNavigation, useRoute, RouteProp } from '@react-navigation/native';
import { Ionicons } from '@expo/vector-icons';
import { LinearGradient } from 'expo-linear-gradient';
import * as Location from 'expo-location';

import { PlantCardWithInfo } from '../components/PlantCardWithInfo';
import { useOtherProfile } from '../hooks/useOtherProfile';
import { useMyPlants } from '../hooks/usePlantHooks';
import { useSearchRadius } from '../hooks/useSearchRadius';
import { PlantResponse, UserResponse } from '../../../types/apiTypes';
import { COLORS } from '../../../theme/colors';
import PlantThumbnail from '../components/PlantThumbnail';
import { headerStyles } from '../styles/headerStyles';
import { ProfileCard } from '../components/ProfileCard';

// Define navigation parameters
type RootStackParamList = {
  OtherProfile: { userId: number };
};

type OtherProfileScreenRouteProp = RouteProp<RootStackParamList, 'OtherProfile'>;

const OtherProfileScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation();
  const route = useRoute<OtherProfileScreenRouteProp>();
  const { userId } = route.params;
  const screenWidth = Dimensions.get('window').width;

  // Fetch the other user's profile
  const {
    data: otherUserProfile,
    isLoading: loadingProfile,
    isError: errorProfile,
    refetch: refetchProfile,
  } = useOtherProfile(userId);

  // Fetch current user's plants (if needed to show related plants)
  const {
    data: myPlants,
    isLoading: loadingPlants,
    isError: errorPlants,
    refetch: refetchPlants,
  } = useMyPlants();

  // Fetch search radius (if used for geolocation display)
  const {
    searchRadius,
    isLoading: srLoading,
    isError: srError,
  } = useSearchRadius();

  // For showing city, country based on location
  const [cityCountry, setCityCountry] = useState<string>('');

  // Toggle between Thumbnails and Full-size for plants
  const [showFullSize, setShowFullSize] = useState(false);

  // Determine if the other user has location info
  const userHasLocation =
    otherUserProfile?.locationLatitude !== undefined &&
    otherUserProfile?.locationLongitude !== undefined;

  // Reverse-geocode for city / country
  useEffect(() => {
    (async () => {
      if (userHasLocation) {
        try {
          const [geo] = await Location.reverseGeocodeAsync({
            latitude: otherUserProfile!.locationLatitude!,
            longitude: otherUserProfile!.locationLongitude!,
          });
          if (geo) {
            const city = geo.city || geo.subregion || '';
            const country = geo.country || '';
            setCityCountry(
              city && country ? `${city}, ${country}` : city || country
            );
          }
        } catch (error) {
          console.log('Reverse geocoding error:', error);
          setCityCountry('');
        }
      } else {
        setCityCountry('');
      }
    })();
  }, [userHasLocation, otherUserProfile]);

  // Handler for sending a message to the user
  const handleSendMessage = () => {
    navigation.navigate('Chat' as never, { otherUserId: userId } as never);
  };

  // Render each plant item from current user's plants (or other data if needed)
  const renderPlantItem = (item: PlantResponse) => {
    if (!showFullSize) {
      return (
        <PlantThumbnail
          key={item.plantId}
          plant={item}
        />
      );
    } else {
      return (
        <View key={item.plantId} style={styles.plantCardWrapper}>
          <PlantCardWithInfo plant={item} />
        </View>
      );
    }
  };

  // Loading states
  if (loadingProfile || loadingPlants || srLoading) {
    return (
      <SafeAreaView style={styles.centerContainer}>
        <ActivityIndicator size="large" color={COLORS.primary} />
        <Text style={styles.loadingText}>{t('profile_loading_message')}</Text>
      </SafeAreaView>
    );
  }

  if (errorProfile || errorPlants || srError) {
    return (
      <SafeAreaView style={styles.centerContainer}>
        <Text style={styles.errorText}>{t('profile_error_message')}</Text>
        <TouchableOpacity
          onPress={() => {
            refetchProfile();
            refetchPlants();
          }}
          style={styles.retryButton}
        >
          <Text style={styles.retryButtonText}>{t('profile_retry_button')}</Text>
        </TouchableOpacity>
      </SafeAreaView>
    );
  }

  if (!otherUserProfile) {
    return (
      <SafeAreaView style={styles.centerContainer}>
        <Text style={styles.errorText}>{t('profile_no_user_profile_error')}</Text>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaProvider style={styles.container}>
      <ScrollView style={{ flex: 1 }}>
        {/* Header */}
        <LinearGradient
          colors={[COLORS.primary, COLORS.secondary]}
          style={headerStyles.headerGradient}
        >
          <View style={headerStyles.headerColumn1}>
            <TouchableOpacity
              style={headerStyles.headerBackButton}
              onPress={() => navigation.goBack()}
            >
              <Ionicons name="chevron-back" size={30} color={COLORS.textLight} />
            </TouchableOpacity>
            <View style={styles.headerUserInfo}>
              <Text style={headerStyles.headerTitle}>{t('other_profile_title')}</Text>
            </View>
          </View>
        </LinearGradient>

        {/* Profile Card */}
        <View style={styles.cardContainer}>
          <ProfileCard
            userProfile={otherUserProfile}
            isEditable={false}
            screenWidth={screenWidth}
          />
        </View>

        {/* User's Plants Section */}
        <View style={styles.plantsSectionWrapper}>
          <View style={styles.plantsSectionHeader}>
            <Text style={styles.plantsSectionTitle}>
              {t('profile_my_plants_section')}
            </Text>
          </View>

          {/* Toggle between Thumbnails and Full view */}
          <View style={styles.viewToggleRow}>
            <TouchableOpacity
              onPress={() => setShowFullSize(false)}
              style={[
                styles.segmentButton,
                !showFullSize && styles.segmentButtonActive,
              ]}
            >
              <Text
                style={[
                  styles.segmentButtonText,
                  !showFullSize && styles.segmentButtonTextActive,
                ]}
              >
                {t('Thumbnails')}
              </Text>
            </TouchableOpacity>
            <TouchableOpacity
              onPress={() => setShowFullSize(true)}
              style={[
                styles.segmentButton,
                showFullSize && styles.segmentButtonActive,
              ]}
            >
              <Text
                style={[
                  styles.segmentButtonText,
                  showFullSize && styles.segmentButtonTextActive,
                ]}
              >
                {t('Full Size')}
              </Text>
            </TouchableOpacity>
          </View>

          {/* Plants List */}
          {/** Adjust the plant list source as needed (e.g. other user's plants if available) **/}
          {myPlants && myPlants.length > 0 ? (
            <View style={showFullSize ? styles.fullViewContainer : styles.thumbViewContainer}>
              {myPlants.map((plant) => renderPlantItem(plant))}
            </View>
          ) : (
            <View style={styles.noPlantsContainer}>
              <Text style={styles.noPlantsText}>
                {t('profile_no_plants_message')}
              </Text>
            </View>
          )}
        </View>
      </ScrollView>
    </SafeAreaProvider>
  );
};

export default OtherProfileScreen;

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
  centerContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 20,
  },
  loadingText: {
    fontSize: 16,
    color: COLORS.textDark,
    marginTop: 10,
  },
  errorText: {
    fontSize: 16,
    color: COLORS.textDark,
    marginBottom: 20,
    textAlign: 'center',
  },
  retryButton: {
    backgroundColor: COLORS.primary,
    paddingVertical: 10,
    paddingHorizontal: 20,
    borderRadius: 8,
  },
  retryButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
  // Profile Card
  cardContainer: {
    marginHorizontal: 16,
    marginTop: 20,
  },
  // Plants Section
  plantsSectionWrapper: {
    paddingTop: 20,
    paddingBottom: 15,
  },
  plantsSectionHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 10,
    paddingHorizontal: 20,
  },
  plantsSectionTitle: {
    fontSize: 20,
    fontWeight: '700',
    color: COLORS.textDark,
  },
  viewToggleRow: {
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: COLORS.textLight,
    borderRadius: 20,
    alignSelf: 'center',
    padding: 3,
    marginBottom: 15,
  },
  segmentButton: {
    paddingHorizontal: 20,
    paddingVertical: 8,
    borderRadius: 18,
  },
  segmentButtonActive: {
    backgroundColor: COLORS.accentGreen,
  },
  segmentButtonText: {
    fontSize: 14,
    fontWeight: '600',
    color: COLORS.textDark,
  },
  segmentButtonTextActive: {
    color: COLORS.textLight,
  },
  // Layouts for plant items
  thumbViewContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'center',
  },
  fullViewContainer: {
    width: '100%',
    paddingHorizontal: 20,
  },
  plantCardWrapper: {
    marginBottom: 15,
  },
  noPlantsContainer: {
    justifyContent: 'center',
    alignItems: 'center',
    padding: 20,
  },
  noPlantsText: {
    fontSize: 16,
    color: '#555',
    textAlign: 'center',
  },
  // Header User Info
  headerUserInfo: {
    flexDirection: 'row',
    alignItems: 'center',
    marginLeft: 10,
  },
});
