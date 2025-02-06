// src/screens/MyProfileScreen.tsx

import React, { useState, useEffect, useRef } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ActivityIndicator,
  TouchableOpacity,
  Platform,
  ScrollView,
} from 'react-native';
import { SafeAreaView, SafeAreaProvider } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';
import { useNavigation } from '@react-navigation/native';
import { Ionicons } from '@expo/vector-icons';
import { LinearGradient } from 'expo-linear-gradient';
import * as Location from 'expo-location';

import { PlantCardWithInfo } from '../components/PlantCardWithInfo';
import { useMyProfile } from '../hooks/useMyProfileHooks';
import { useMyPlants } from '../hooks/usePlantHooks';
import { useSearchRadius } from '../hooks/useSearchRadius';
import { PlantResponse } from '../../../types/apiTypes';
import { COLORS } from '../../../theme/colors';
import { EditProfileModal } from '../modals/EditProfileModal';
import { PlantThumbnail } from '../components/PlantThumbnail';
import { headerStyles } from '../styles/headerStyles';
import { ProfileCard } from '../components/ProfileCard';
import { ToggleButton } from '../components/ToggleButton'; // Import the new ToggleButton component

const MyProfileScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation();

  // Hooks
  const {
    data: userProfile,
    isLoading: loadingProfile,
    isError: errorProfile,
    refetch: refetchProfile,
  } = useMyProfile();
  const {
    data: myPlants,
    isLoading: loadingPlants,
    isError: errorPlants,
    refetch: refetchPlants,
  } = useMyPlants();
  const {
    searchRadius,
    isLoading: srLoading,
    isError: srError,
  } = useSearchRadius();

  // For showing city, country
  const [cityCountry, setCityCountry] = useState<string>('');
  const [editProfileVisible, setEditProfileVisible] = useState(false);

  // Toggle: Thumbnails or Full-size for plants
  const [viewOption, setViewOption] = useState<'Thumbnails' | 'Full Size'>('Thumbnails');

  // Position for the EditProfileModal
  const [editCardLayout, setEditCardLayout] = useState({
    x: 0,
    y: 0,
    width: 0,
    height: 0,
  });
  const cardRef = useRef<View>(null);

  // If the user has location
  const userHasLocation =
    userProfile?.locationLatitude !== undefined &&
    userProfile?.locationLongitude !== undefined;

  // Handler for measuring the card and opening the modal
  const openEditModal = () => {
    cardRef.current?.measureInWindow((x, y, width, height) => {
      setEditCardLayout({ x, y, width, height });
      setEditProfileVisible(true);
    });
  };

  const OnDelete = () => {
    // Implement delete functionality here
  };

  // Reverse-geocode for city / country
  useEffect(() => {
    (async () => {
      if (userHasLocation) {
        try {
          const [geo] = await Location.reverseGeocodeAsync({
            latitude: userProfile!.locationLatitude!,
            longitude: userProfile!.locationLongitude!,
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
  }, [userHasLocation, userProfile]);

  // Navigation to AddPlant
  const handleAddPlant = () => {
    navigation.navigate('AddPlant' as never);
  };

  // Rendering plants
  const renderPlantItem = (item: PlantResponse) => {
    if (viewOption === 'Thumbnails') {
      return (
        <PlantThumbnail
          key={item.plantId}
          plant={item}
          selectable
          deletable
          onPress={OnDelete}
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

  if (!userProfile) {
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
          <Text style={headerStyles.headerTitle}>{t('profile_title')}</Text>
        </LinearGradient>

        {/* --- Profile Card --- */}
        <View ref={cardRef} style={styles.cardContainer}>
          <ProfileCard
            userProfile={userProfile}
            isEditable={true} // Set to true to show the edit button
            onEditPress={openEditModal}
          />
        </View>

        {/* ---- My Plants Section ---- */}
        <View style={styles.plantsSectionWrapper}>
          <View style={styles.plantsSectionHeader}>
            <Text style={styles.plantsSectionTitle}>
              {userProfile.name} {t('profile_my_plants_section')}
            </Text>
            <TouchableOpacity
              onPress={handleAddPlant}
              style={styles.addPlantButton}
              accessibilityRole="button"
              accessibilityLabel={t('profile_add_plant_button')}
            >
              <Ionicons name="add-circle" size={24} color={COLORS.textLight} />
              <Text style={styles.addPlantButtonText}>
                {t('profile_add_plant_button')}
              </Text>
            </TouchableOpacity>
          </View>

          {/* Toggle between Thumbnails and Full view */}
          <ToggleButton
            options={[t('Thumbnails'), t('Full Size')]}
            selected={viewOption}
            onToggle={(option) => setViewOption(option as 'Thumbnails' | 'Full Size')}
          />

          {/* Plants List */}
          {myPlants && myPlants.length > 0 ? (
            <View style={viewOption === 'Full Size' ? styles.fullViewContainer : styles.thumbViewContainer}>
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

        {/* Edit Profile Modal */}
        <EditProfileModal
          visible={editProfileVisible}
          userProfile={userProfile}
          onClose={() => setEditProfileVisible(false)}
          onUpdated={() => {
            refetchProfile();
          }}
          cardLayout={editCardLayout}
        />
      </ScrollView>
    </SafeAreaProvider>
  );
};

export default MyProfileScreen;

// Updated Styles

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

  // ---- Plants Section ----
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
  addPlantButton: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: COLORS.accentGreen, // or any brand color
    paddingVertical: 8,
    paddingHorizontal: 12,
    borderRadius: 20,
    ...Platform.select({
      ios: {
        shadowColor: "#000",
        shadowOpacity: 0.1,
        shadowOffset: { width: 0, height: 3 },
        shadowRadius: 4,
      },
      android: {
        elevation: 3,
      },
    }),
  },
  addPlantButtonText: {
    color: COLORS.textLight,
    fontSize: 14,
    fontWeight: '600',
    marginLeft: 6,
  },
  // Different layouts for the plant items
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
});
