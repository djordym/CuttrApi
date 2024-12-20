// ProfileScreen.tsx (updated)
import React, { useState, useCallback } from 'react';
import { View, Text, StyleSheet, ActivityIndicator, TouchableOpacity, FlatList, Image, Alert } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useTranslation } from 'react-i18next';
import { useUserProfile } from '../hooks/useUser';
import { useMyPlants } from '../hooks/usePlants';
import { Ionicons, MaterialIcons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { PlantResponse } from '../../../types/apiTypes';
import { EditProfileModal } from '../components/EditProfileModal';
import { ChangeLocationModal } from '../components/ChangeLocationModal';

const ProfileScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation();
  const { data: userProfile, isLoading: loadingProfile, isError: errorProfile, refetch: refetchProfile } = useUserProfile();
  const { data: myPlants, isLoading: loadingPlants, isError: errorPlants, refetch: refetchPlants } = useMyPlants();

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

  const renderPlantItem = ({ item }: { item: PlantResponse }) => (
    <View style={styles.plantItem}>
      {item.ImageUrl ? (
        <Image source={{ uri: item.ImageUrl }} style={styles.plantImage} />
      ) : (
        <View style={styles.plantPlaceholder}>
          <Ionicons name="leaf" size={40} color="#1EAE98" />
        </View>
      )}
      <Text style={styles.plantName} numberOfLines={1}>{item.SpeciesName}</Text>
    </View>
  );

  if (loadingProfile || loadingPlants) {
    return (
      <SafeAreaView style={styles.centerContainer}>
        <ActivityIndicator size="large" color="#1EAE98" />
        <Text style={styles.loadingText}>{t('profile_loading_message')}</Text>
      </SafeAreaView>
    );
  }

  if (errorProfile || errorPlants) {
    return (
      <SafeAreaView style={styles.centerContainer}>
        <Text style={styles.errorText}>{t('profile_error_message')}</Text>
        <TouchableOpacity onPress={() => { refetchProfile(); refetchPlants(); }} style={styles.retryButton}>
          <Text style={styles.retryButtonText}>{t('profile_retry_button')}</Text>
        </TouchableOpacity>
      </SafeAreaView>
    );
  }

  if (!userProfile) {
    return null;
  }

  const userLocation = userProfile.LocationLatitude && userProfile.LocationLongitude
    ? `Lat: ${userProfile.LocationLatitude.toFixed(2)}, Lng: ${userProfile.LocationLongitude.toFixed(2)}`
    : t('profile_no_location');

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.headerContainer}>
        <Text style={styles.headerTitle}>{t('profile_title')}</Text>
        <TouchableOpacity onPress={handleEditProfile} style={styles.headerActionButton} accessibilityLabel={t('profile_edit_button')}>
          <MaterialIcons name="edit" size={24} color="#333" />
        </TouchableOpacity>
      </View>

      <View style={styles.profileContainer}>
        <View style={styles.profileInfo}>
          {userProfile.ProfilePictureUrl ? (
            <Image source={{ uri: userProfile.ProfilePictureUrl }} style={styles.profilePicture} />
          ) : (
            <View style={styles.profilePlaceholder}>
              <Ionicons name="person-circle-outline" size={80} color="#ccc" />
            </View>
          )}
          <Text style={styles.profileName}>{userProfile.Name}</Text>
          <Text style={styles.profileLabel}>{t('profile_bio_label')}:</Text>
          <Text style={styles.profileValue} numberOfLines={3}>{userProfile.Bio || ''}</Text>

          <Text style={[styles.profileLabel, { marginTop:10 }]}>{t('profile_location_label')}:</Text>
          <Text style={styles.profileValue}>{userLocation}</Text>
          <TouchableOpacity onPress={handleChangeLocation} style={styles.locationButton} accessibilityRole="button" accessibilityLabel={t('profile_change_location_button')}>
            <Ionicons name="location-outline" size={20} color="#1EAE98" />
            <Text style={styles.locationButtonText}>{t('profile_change_location_button')}</Text>
          </TouchableOpacity>
        </View>
      </View>

      <View style={styles.plantsSectionHeader}>
        <Text style={styles.plantsSectionTitle}>{t('profile_my_plants_section')}</Text>
        <TouchableOpacity onPress={handleAddPlant} style={styles.addPlantButton} accessibilityRole="button" accessibilityLabel={t('profile_add_plant_button')}>
          <Ionicons name="add-circle" size={24} color="#1EAE98" />
          <Text style={styles.addPlantButtonText}>{t('profile_add_plant_button')}</Text>
        </TouchableOpacity>
      </View>

      <View style={styles.plantsContainer}>
        {myPlants && myPlants.length > 0 ? (
          <FlatList 
            data={myPlants}
            keyExtractor={(item) => item.PlantId.toString()}
            renderItem={renderPlantItem}
            numColumns={2}
            columnWrapperStyle={styles.plantRow}
            contentContainerStyle={{ paddingHorizontal:20, paddingBottom:20 }}
          />
        ) : (
          <View style={styles.noPlantsContainer}>
            <Text style={styles.noPlantsText}>{t('profile_no_plants_message')}</Text>
          </View>
        )}
      </View>

      <EditProfileModal
        visible={editProfileVisible}
        initialName={userProfile.Name}
        initialBio={userProfile.Bio || ''}
        onClose={() => setEditProfileVisible(false)}
        onUpdated={handleProfileUpdated}
      />

      <ChangeLocationModal
        visible={changeLocationVisible}
        initialLatitude={userProfile.LocationLatitude || undefined}
        initialLongitude={userProfile.LocationLongitude || undefined}
        onClose={() => setChangeLocationVisible(false)}
        onUpdated={handleLocationUpdated}
      />
    </SafeAreaView>
  );
};

export default ProfileScreen;

const styles = StyleSheet.create({
  container: {
    flex:1,
    backgroundColor:'#f8f8f8'
  },
  centerContainer: {
    flex:1,
    justifyContent:'center',
    alignItems:'center',
    padding:20
  },
  loadingText: {
    fontSize:16,
    color:'#333',
    marginTop:10
  },
  errorText: {
    fontSize:16,
    color:'#333',
    marginBottom:20,
    textAlign:'center'
  },
  retryButton: {
    backgroundColor:'#1EAE98',
    paddingVertical:10,
    paddingHorizontal:20,
    borderRadius:8
  },
  retryButtonText:{
    color:'#fff',
    fontSize:16,
    fontWeight:'600'
  },
  headerContainer:{
    flexDirection:'row',
    justifyContent:'space-between',
    alignItems:'center',
    paddingHorizontal:20,
    paddingVertical:10,
    borderBottomWidth:1,
    borderBottomColor:'#ddd',
    backgroundColor:'#fff'
  },
  headerTitle:{
    fontSize:24,
    fontWeight:'700',
    color:'#333'
  },
  headerActionButton:{
    padding:8
  },
  profileContainer:{
    padding:20,
    backgroundColor:'#fff',
    marginBottom:10
  },
  profileInfo:{
    alignItems:'center'
  },
  profilePicture:{
    width:100,
    height:100,
    borderRadius:50,
    marginBottom:10,
    backgroundColor:'#eee'
  },
  profilePlaceholder:{
    width:100,
    height:100,
    borderRadius:50,
    backgroundColor:'#eee',
    alignItems:'center',
    justifyContent:'center',
    marginBottom:10
  },
  profileName:{
    fontSize:20,
    fontWeight:'700',
    color:'#333',
    marginBottom:10,
    textAlign:'center'
  },
  profileLabel:{
    fontSize:14,
    fontWeight:'600',
    color:'#333',
    marginBottom:4,
    alignSelf:'flex-start'
  },
  profileValue:{
    fontSize:14,
    color:'#555',
    textAlign:'center'
  },
  locationButton:{
    flexDirection:'row',
    alignItems:'center',
    marginTop:8,
    paddingHorizontal:10,
    paddingVertical:6,
    borderWidth:1,
    borderColor:'#1EAE98',
    borderRadius:8
  },
  locationButtonText:{
    color:'#1EAE98',
    fontSize:14,
    marginLeft:5
  },
  plantsSectionHeader:{
    flexDirection:'row',
    justifyContent:'space-between',
    alignItems:'center',
    paddingHorizontal:20,
    paddingVertical:10,
    borderBottomWidth:1,
    borderBottomColor:'#ddd',
    backgroundColor:'#fff'
  },
  plantsSectionTitle:{
    fontSize:18,
    fontWeight:'700',
    color:'#333'
  },
  addPlantButton:{
    flexDirection:'row',
    alignItems:'center'
  },
  addPlantButtonText:{
    fontSize:14,
    color:'#1EAE98',
    marginLeft:5
  },
  plantsContainer:{
    flex:1,
    backgroundColor:'#f8f8f8',
  },
  noPlantsContainer:{
    flex:1,
    justifyContent:'center',
    alignItems:'center',
    padding:20
  },
  noPlantsText:{
    fontSize:16,
    color:'#555',
    textAlign:'center'
  },
  plantRow:{
    justifyContent:'space-between',
    marginBottom:20
  },
  plantItem:{
    width:'48%',
    backgroundColor:'#fff',
    borderRadius:8,
    overflow:'hidden',
    alignItems:'center',
    padding:10
  },
  plantImage:{
    width:'100%',
    height:100,
    marginBottom:10,
    borderRadius:8,
    backgroundColor:'#eee',
    resizeMode:'cover'
  },
  plantPlaceholder:{
    width:'100%',
    height:100,
    borderRadius:8,
    backgroundColor:'#eee',
    justifyContent:'center',
    alignItems:'center',
    marginBottom:10
  },
  plantName:{
    fontSize:14,
    fontWeight:'600',
    color:'#333',
    textAlign:'center'
  }
});
