import React, { useState, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ActivityIndicator,
  TouchableOpacity,
  FlatList,
  Image,
  Dimensions,
  Platform,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';
import { useNavigation } from '@react-navigation/native';
import { Ionicons, MaterialIcons } from '@expo/vector-icons';
import { LinearGradient } from 'expo-linear-gradient';
import MapView, { Circle } from 'react-native-maps';
import { useUserProfile } from '../hooks/useUser';
import { useMyPlants } from '../hooks/usePlants';
import { useSearchRadius } from '../hooks/useSearchRadius';
import { PlantResponse } from '../../../types/apiTypes';
import { EditProfileModal } from '../components/EditProfileModal';
import { ChangeLocationModal } from '../components/ChangeLocationModal';

const { width } = Dimensions.get('window');
const COLORS = {
  primary: '#1EAE98',
  accent: '#FF6F61',
  background: '#F2F2F2',
  textDark: '#2F4F4F',
  textLight: '#FFFFFF',
  cardBg: '#FFFFFF',
  border: '#ddd',
};

const MyProfileScreen: React.FC = () => {
  // 1. Declare Hooks at top
  const { t } = useTranslation();
  const navigation = useNavigation();

  const {
    data: userProfile,
    isLoading: loadingProfile,
    isError: errorProfile,
    refetch: refetchProfile,
  } = useUserProfile();

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

  const [editProfileVisible, setEditProfileVisible] = useState(false);
  const [changeLocationVisible, setChangeLocationVisible] = useState(false);

  const handleEditProfile = useCallback(() => {
    setEditProfileVisible(true);
  }, []);

  const handleProfileUpdated = useCallback(() => {
    refetchProfile();
  }, [refetchProfile]);

  const handleChangeLocation = useCallback(() => {
    setChangeLocationVisible(true);
  }, []);

  const handleLocationUpdated = useCallback(() => {
    refetchProfile();
  }, [refetchProfile]);

  const handleAddPlant = useCallback(() => {
    navigation.navigate('AddPlant' as never);
  }, [navigation]);

  // 2. Check if user has location
  const userHasLocation =
    userProfile?.locationLatitude !== undefined &&
    userProfile?.locationLongitude !== undefined;

  const region = userHasLocation
    ? {
        latitude: userProfile.locationLatitude!,
        longitude: userProfile.locationLongitude!,
        latitudeDelta: 0.05,
        longitudeDelta: 0.05,
      }
    : undefined;

  // 3. Render each plant
  const renderPlantItem = ({ item }: { item: PlantResponse }) => (
    <View style={styles.plantCard}>
      {item.imageUrl ? (
        <Image source={{ uri: item.imageUrl }} style={styles.plantImage} />
      ) : (
        <View style={styles.plantPlaceholder}>
          <Ionicons name="leaf" size={40} color={COLORS.primary} />
        </View>
      )}
      <View style={styles.plantDetails}>
        <Text style={styles.plantName} numberOfLines={1}>
          {item.speciesName}
        </Text>
      </View>
    </View>
  );

  // 4. Decide main content
  let content: JSX.Element;

  if (loadingProfile || loadingPlants || srLoading) {
    // Loading
    content = (
      <SafeAreaView style={styles.centerContainer}>
        <ActivityIndicator size="large" color={COLORS.primary} />
        <Text style={styles.loadingText}>{t('profile_loading_message')}</Text>
      </SafeAreaView>
    );
  } else if (errorProfile || errorPlants || srError) {
    // Error
    content = (
      <SafeAreaView style={styles.centerContainer}>
        <Text style={styles.errorText}>{t('profile_error_message')}</Text>
        <TouchableOpacity
          onPress={() => {
            refetchProfile();
            refetchPlants();
          }}
          style={styles.retryButton}
        >
          <Text style={styles.retryButtonText}>
            {t('profile_retry_button')}
          </Text>
        </TouchableOpacity>
      </SafeAreaView>
    );
  } else if (!userProfile) {
    // No profile
    content = (
      <SafeAreaView style={styles.centerContainer}>
        <Text style={styles.errorText}>
          {t('profile_no_user_profile_error')}
        </Text>
      </SafeAreaView>
    );
  } else {
    // Normal UI
    content = (
      <SafeAreaView style={styles.container}>
        {/* HEADER: GRADIENT */}
        <LinearGradient
          colors={[COLORS.primary, '#5EE2C6']}
          style={styles.headerContainer}
        >
          <View style={styles.headerTopRow}>
            <Text style={styles.headerTitle}>{t('profile_title')}</Text>
            <TouchableOpacity
              onPress={handleEditProfile}
              style={styles.headerActionButton}
              accessibilityLabel={t('profile_edit_button')}
            >
              <MaterialIcons name="edit" size={24} color={COLORS.textLight} />
            </TouchableOpacity>
          </View>

          <View style={styles.profileInfoContainer}>
            {userProfile.profilePictureUrl ? (
              <Image
                source={{ uri: userProfile.profilePictureUrl }}
                style={styles.profilePicture}
              />
            ) : (
              <View style={styles.profilePlaceholder}>
                <Ionicons name="person-circle-outline" size={90} color="#ccc" />
              </View>
            )}

            <View style={styles.profileTextSection}>
              <Text style={styles.profileName}>{userProfile.name}</Text>

              {userProfile.bio ? (
                <>
                  <Text style={styles.profileLabel}>
                    {t('profile_bio_label')}:
                  </Text>
                  <Text style={styles.profileValue} numberOfLines={3}>
                    {userProfile.bio}
                  </Text>
                </>
              ) : null}

              {/* LOCATION */}
              <View style={styles.profileLocationSection}>
                <Text style={styles.profileLabel}>
                  {t('profile_location_label')}:
                </Text>
                {userHasLocation ? (
                  <View style={styles.mapContainer}>
                    <MapView style={styles.map} initialRegion={region}>
                      <Circle
                        center={{
                          latitude: region.latitude,
                          longitude: region.longitude,
                        }}
                        radius={searchRadius * 1000} // searchRadius in km -> convert to meters
                        strokeWidth={1.5}
                        strokeColor="#1EAE98"
                        fillColor="rgba(30, 174, 152, 0.2)"
                      />
                    </MapView>
                  </View>
                ) : (
                  <Text style={styles.profileValue}>
                    {t('profile_no_location')}
                  </Text>
                )}
              </View>

              <TouchableOpacity
                onPress={handleChangeLocation}
                style={styles.locationButton}
                accessibilityRole="button"
                accessibilityLabel={t('profile_change_location_button')}
              >
                <Ionicons
                  name="location-outline"
                  size={18}
                  color={COLORS.primary}
                />
                <Text style={styles.locationButtonText}>
                  {t('profile_change_location_button')}
                </Text>
              </TouchableOpacity>
            </View>
          </View>
        </LinearGradient>

        {/* MY PLANTS SECTION */}
        <View style={styles.plantsSectionWrapper}>
          <View style={styles.plantsSectionHeader}>
            <Text style={styles.plantsSectionTitle}>
              {t('profile_my_plants_section')}
            </Text>
            <TouchableOpacity
              onPress={handleAddPlant}
              style={styles.addPlantButton}
              accessibilityRole="button"
              accessibilityLabel={t('profile_add_plant_button')}
            >
              <Ionicons name="add-circle" size={24} color={COLORS.primary} />
              <Text style={styles.addPlantButtonText}>
                {t('profile_add_plant_button')}
              </Text>
            </TouchableOpacity>
          </View>

          <View style={styles.plantsContainer}>
            {myPlants && myPlants.length > 0 ? (
              <FlatList
                data={myPlants}
                keyExtractor={(item) => item.plantId.toString()}
                renderItem={renderPlantItem}
                numColumns={2}
                columnWrapperStyle={{ justifyContent: 'space-between' }}
                contentContainerStyle={styles.plantListContent}
              />
            ) : (
              <View style={styles.noPlantsContainer}>
                <Text style={styles.noPlantsText}>
                  {t('profile_no_plants_message')}
                </Text>
              </View>
            )}
          </View>
        </View>

        {/* MODALS */}
        <EditProfileModal
          visible={editProfileVisible}
          initialName={userProfile.name}
          initialBio={userProfile.bio || ''}
          onClose={() => setEditProfileVisible(false)}
          onUpdated={handleProfileUpdated}
        />
        {/**
         * IMPORTANT: We pass userProfile.locationLatitude and locationLongitude
         * directly without the `|| undefined` fallback.
         */}
        <ChangeLocationModal
          visible={changeLocationVisible}
          initialLatitude={userProfile.locationLatitude}
          initialLongitude={userProfile.locationLongitude}
          onClose={() => setChangeLocationVisible(false)}
          onUpdated={handleLocationUpdated}
        />
      </SafeAreaView>
    );
  }

  return content;
};

export default MyProfileScreen;

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

  // HEADER
  headerContainer: {
    paddingHorizontal: 20,
    paddingVertical: 20,
    borderBottomLeftRadius: 40,
    borderBottomRightRadius: 40,
    marginBottom: 10,
  },
  headerTopRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  headerTitle: {
    fontSize: 24,
    fontWeight: '700',
    color: COLORS.textLight,
  },
  headerActionButton: {
    padding: 8,
  },

  // PROFILE INFO
  profileInfoContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginTop: 15,
  },
  profilePicture: {
    width: 100,
    height: 100,
    borderRadius: 50,
    backgroundColor: '#eee',
    marginRight: 15,
    borderWidth: 2,
    borderColor: '#fff',
  },
  profilePlaceholder: {
    width: 100,
    height: 100,
    borderRadius: 50,
    backgroundColor: '#eee',
    alignItems: 'center',
    justifyContent: 'center',
    marginRight: 15,
  },
  profileTextSection: {
    flexShrink: 1,
  },
  profileName: {
    fontSize: 18,
    fontWeight: '700',
    color: COLORS.textLight,
    marginBottom: 6,
  },
  profileLabel: {
    fontSize: 14,
    fontWeight: '600',
    color: COLORS.textLight,
    marginBottom: 2,
  },
  profileValue: {
    fontSize: 14,
    color: COLORS.textLight,
    marginBottom: 8,
  },
  profileLocationSection: {
    marginTop: 6,
  },

  // MAP
  mapContainer: {
    marginTop: 6,
    width: 200,
    height: 120,
    borderRadius: 8,
    overflow: 'hidden',
    backgroundColor: '#ddd',
  },
  map: {
    width: '100%',
    height: '100%',
  },

  // CHANGE LOCATION BUTTON
  locationButton: {
    flexDirection: 'row',
    alignItems: 'center',
    marginTop: 8,
    paddingHorizontal: 10,
    paddingVertical: 6,
    borderWidth: 1,
    borderColor: COLORS.primary,
    borderRadius: 8,
    alignSelf: 'flex-start',
    backgroundColor: '#fff',
  },
  locationButtonText: {
    color: COLORS.primary,
    fontSize: 14,
    marginLeft: 5,
    fontWeight: '600',
  },

  // PLANTS SECTION
  plantsSectionWrapper: {
    flex: 1,
    paddingHorizontal: 10,
    paddingTop: 10,
  },
  plantsSectionHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 10,
    paddingHorizontal: 10,
  },
  plantsSectionTitle: {
    fontSize: 20,
    fontWeight: '700',
    color: COLORS.textDark,
  },
  addPlantButton: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  addPlantButtonText: {
    fontSize: 14,
    color: COLORS.primary,
    marginLeft: 5,
    fontWeight: '600',
  },
  plantsContainer: {
    flex: 1,
  },
  plantListContent: {
    paddingHorizontal: 10,
    paddingBottom: 20,
  },
  noPlantsContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 20,
  },
  noPlantsText: {
    fontSize: 16,
    color: '#555',
    textAlign: 'center',
  },

  // PLANT CARD
  plantCard: {
    width: (width - 60) / 2,
    backgroundColor: COLORS.cardBg,
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
    resizeMode: 'cover',
  },
  plantPlaceholder: {
    width: '100%',
    height: 120,
    backgroundColor: '#eee',
    justifyContent: 'center',
    alignItems: 'center',
  },
  plantDetails: {
    padding: 10,
  },
  plantName: {
    fontSize: 14,
    fontWeight: '600',
    color: COLORS.textDark,
    textAlign: 'center',
  },
});

