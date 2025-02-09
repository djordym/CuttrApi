// src/components/ProfileCard.tsx

import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Image, ImageBackground } from 'react-native';
import { Ionicons, MaterialIcons } from '@expo/vector-icons';
import { LinearGradient } from 'expo-linear-gradient';
import { useTranslation } from 'react-i18next';

import { COLORS } from '../../../theme/colors';
import { profileCardStyles } from '../styles/profileCardStyles';
import { UserResponse } from '../../../types/apiTypes'; // Adjust the path based on your project structure
import ProfileLocationDisplay from './ProfileLocationDisplay'; // Adjust the path based on your project structure

interface ProfileCardProps {
  userProfile: UserResponse;
  isEditable?: boolean;
  onEditPress?: () => void;
}

export const ProfileCard: React.FC<ProfileCardProps> = ({
  userProfile,
  isEditable = false,
  onEditPress,
}) => {
  const { t } = useTranslation();

  return (
    <View style={[profileCardStyles.profileCardContainer]}>
      <LinearGradient colors={[COLORS.cardBg1, COLORS.cardBg2]} style={profileCardStyles.profileCardInner}>
        <View style={profileCardStyles.profileTopContainer}>
          <ImageBackground
            source={require('../../../../assets/images/profileBackground.png')}
            style={profileCardStyles.profileBackgroundImage}
          />
          <View style={profileCardStyles.profilePictureContainer}>
            {userProfile.profilePictureUrl ? (
              <Image
                source={{ uri: userProfile.profilePictureUrl }}
                style={profileCardStyles.profilePicture}
              />
            ) : (
              <View style={profileCardStyles.profilePlaceholder}>
                <Ionicons name="person-circle-outline" size={90} color="#ccc" />
              </View>
            )}
          </View>

          {/* Conditionally render the Edit button */}
          {isEditable && onEditPress && (
            <TouchableOpacity
              onPress={onEditPress}
              style={profileCardStyles.profileEditButton}
              accessibilityLabel={t('profile_edit_button')}
            >
              <MaterialIcons name="edit" size={20} color={COLORS.textLight} />
            </TouchableOpacity>
          )}
        </View>

        {/* Name, location, bio */}
        <View style={profileCardStyles.profileInfoContainer}>
          <View style={profileCardStyles.nameContainer}>
            <Text style={profileCardStyles.profileNameText}>{userProfile.name}</Text>
          </View>
          <View style={profileCardStyles.profileLocationRow}>
            <Ionicons
              name="location-sharp"
              size={16}
              color={COLORS.accentLightRed}
              style={profileCardStyles.locationIcon}
            />
            {userProfile.locationLatitude && userProfile.locationLongitude && (
              <ProfileLocationDisplay
                latitude={userProfile.locationLatitude}
                longitude={userProfile.locationLongitude}
              />
            )}
          </View>
        </View>
        <View style={profileCardStyles.bioContainer}>
          <Text
            style={[
              profileCardStyles.bioText,
              !userProfile.bio && profileCardStyles.bioPlaceholder,
            ]}
          >
            {userProfile.bio ? userProfile.bio : t('profile_no_bio_placeholder')}
          </Text>
        </View>
      </LinearGradient>
    </View>
  );
};
