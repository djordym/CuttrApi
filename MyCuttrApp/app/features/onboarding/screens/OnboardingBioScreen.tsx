import React, { useState } from 'react';
import { View, Text, StyleSheet, TextInput, TouchableOpacity, Alert } from 'react-native';
import { useNavigation } from '@react-navigation/native';
import { useTranslation } from 'react-i18next';

// Import userService (and possible utility code)
import { userService } from '../../../api/userService';
import { useUserProfile } from '../../main/hooks/useUser';
import { log } from '../../../utils/logger';


const OnboardingBioScreen: React.FC = () => {
    const { t } = useTranslation();
    const navigation = useNavigation();
  
    // If needed, fetch userProfile to get user’s current name or other details
    const { data: userProfile } = useUserProfile();
  
    // State for the bio
    const [bio, setBio] = useState(userProfile?.bio ?? '');
  
    const handleSkip = () => {
      // Let them skip and navigate to the next onboarding screen (or main flow if this is last)
      // Usually you'd do something like:
      navigation.navigate('OnboardingLocation' as never);
      // or if the location screen is already behind us, navigate to next step.
    };
  
    const handleSubmitBio = async () => {
      if (!bio) {
        // They can submit an empty bio or show an alert to confirm
        // For the sake of example, we allow an empty bio
        // Or you could do: Alert.alert('Bio is empty', 'Please type something or skip.');
      }
  
      try {
        // We assume the userService.updateMe uses the shape { name, bio }
        await userService.updateMe({ name: userProfile?.name ?? '', bio });
        log.debug('Bio updated successfully');
        // Navigate to the next step in the onboarding
        navigation.navigate('OnboardingLocation' as never);
      } catch (error) {
        Alert.alert('Error', 'Failed to update bio, please try again later.');
        log.error('OnboardingBioScreen handleSubmitBio error:', error);
      }
    };
  
    return (
      <View style={styles.container}>
        <Text style={styles.title}>{t('onboarding_bio_title') /* "Tell Us About Yourself" */}</Text>
        <Text style={styles.subtitle}>
          {t('onboarding_bio_subtitle') /* e.g. "Add a brief bio so others can know more about you." */}
        </Text>
  
        <TextInput
          style={styles.textInput}
          value={bio}
          onChangeText={setBio}
          placeholder={t('onboarding_bio_placeholder') /* "Your bio here..." */}
          multiline
        />
  
        <View style={styles.buttonContainer}>
          <TouchableOpacity onPress={handleSkip} style={[styles.button, styles.skipButton]}>
            <Text style={styles.buttonText}>{t('onboarding_bio_skip_button') /* "Skip" */}</Text>
          </TouchableOpacity>
  
          <TouchableOpacity onPress={handleSubmitBio} style={[styles.button, styles.submitButton]}>
            <Text style={styles.buttonText}>{t('onboarding_bio_submit_button') /* "Save & Continue" */}</Text>
          </TouchableOpacity>
        </View>
  
        <Text style={styles.note}>
          {t('onboarding_bio_note') /* e.g. "You can always add or edit your bio later from your profile." */}
        </Text>
      </View>
    );
  };
  
  export default OnboardingBioScreen;
  
  const styles = StyleSheet.create({
    container: {
      flex: 1,
      backgroundColor: '#fff',
      padding: 20,
      justifyContent: 'flex-start',
    },
    title: {
      fontSize: 24,
      fontWeight: '700',
      marginBottom: 10,
      color: '#333',
    },
    subtitle: {
      fontSize: 16,
      marginBottom: 20,
      color: '#555',
    },
    textInput: {
      borderWidth: 1,
      borderColor: '#ccc',
      borderRadius: 8,
      padding: 10,
      fontSize: 14,
      minHeight: 80,
      marginBottom: 20,
      textAlignVertical: 'top',
      backgroundColor: '#fafafa',
    },
    buttonContainer: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      marginBottom: 10,
    },
    button: {
      flex: 0.45,
      paddingVertical: 12,
      borderRadius: 8,
      alignItems: 'center',
      justifyContent: 'center',
    },
    skipButton: {
      backgroundColor: '#ccc',
    },
    submitButton: {
      backgroundColor: '#1EAE98',
    },
    buttonText: {
      color: '#fff',
      fontWeight: '600',
      fontSize: 16,
    },
    note: {
      marginTop: 20,
      fontSize: 14,
      color: '#777',
      textAlign: 'center',
    },
  });
  