import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  Platform,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { MaterialIcons, Ionicons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { useUserPreferences } from '../hooks/usePreferences';

import TagGroup from '../components/TagGroup';
import { COLORS } from '../../../theme/colors';
import { headerStyles } from '../styles/headerStyles';
import ConfirmCancelButtons from '../components/ConfirmCancelButtons';

import {
  PlantStage,
  PlantCategory,
  WateringNeed,
  LightRequirement,
  Size,
  IndoorOutdoor,
  PropagationEase,
  PetFriendly,
  Extras,
} from '../../../types/enums';
import { UserPreferencesRequest } from '../../../types/apiTypes';

const SetUserPreferencesScreen: React.FC = () => {
  const navigation = useNavigation();
  const {
    data: preferences,
    isLoading,
    isError,
    updatePreferences,
    isUpdating,
  } = useUserPreferences();

  const [selectedStages, setSelectedStages] = useState<PlantStage[]>([]);
  const [selectedCategories, setSelectedCategories] = useState<PlantCategory[]>([]);
  const [selectedWatering, setSelectedWatering] = useState<WateringNeed[]>([]);
  const [selectedLightReq, setSelectedLightReq] = useState<LightRequirement[]>([]);
  const [selectedSize, setSelectedSize] = useState<Size[]>([]);
  const [selectedIndoorOutdoor, setSelectedIndoorOutdoor] = useState<IndoorOutdoor[]>([]);
  const [selectedPropagationEase, setSelectedPropagationEase] = useState<PropagationEase[]>([]);
  const [selectedPetFriendly, setSelectedPetFriendly] = useState<PetFriendly[]>([]);
  const [selectedExtras, setSelectedExtras] = useState<Extras[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (preferences) {
      setSelectedStages(preferences.preferedPlantStage || []);
      setSelectedCategories(preferences.preferedPlantCategory || []);
      setSelectedWatering(preferences.preferedWateringNeed || []);
      setSelectedLightReq(preferences.preferedLightRequirement || []);
      setSelectedSize(preferences.preferedSize || []);
      setSelectedIndoorOutdoor(preferences.preferedIndoorOutdoor || []);
      setSelectedPropagationEase(preferences.preferedPropagationEase || []);
      setSelectedPetFriendly(preferences.preferedPetFriendly || []);
      setSelectedExtras(preferences.preferedExtras || []);
    }
  }, [preferences]);

  const handleCancel = () => {
    navigation.goBack();
  };

  const handleSave = async () => {
    if (!preferences) return;
    setError(null);

    const updated: UserPreferencesRequest = {
      ...preferences,
      preferedPlantStage: selectedStages,
      preferedPlantCategory: selectedCategories,
      preferedWateringNeed: selectedWatering,
      preferedLightRequirement: selectedLightReq,
      preferedSize: selectedSize,
      preferedIndoorOutdoor: selectedIndoorOutdoor,
      preferedPropagationEase: selectedPropagationEase,
      preferedPetFriendly: selectedPetFriendly,
      preferedExtras: selectedExtras,
    };

    try {
      await updatePreferences(updated);
      navigation.goBack();
    } catch (err) {
      console.error('Error updating preferences:', err);
      setError('Could not update preferences.');
    }
  };

  if (isLoading) {
    return (
      <View style={styles.center}>
        <Text>Loading preferences...</Text>
      </View>
    );
  }

  if (isError || !preferences) {
    return (
      <View style={styles.center}>
        <Text>Error loading preferences</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <LinearGradient
        colors={[COLORS.primary, COLORS.secondary]}
        style={styles.gradientBackground}
      >
        <View style={headerStyles.headerAboveScroll}>
          <View style={headerStyles.headerColumn1}>
            <Ionicons
              name="chevron-back"
              size={30}
              color={COLORS.textLight}
              style={headerStyles.headerBackButton}
              onPress={() => navigation.goBack()}
            />
            <Text style={headerStyles.headerTitle}>User Preferences</Text>
          </View>
          <MaterialIcons name="settings" size={24} color="#fff" />
        </View>

        <ScrollView
          contentContainerStyle={styles.scrollContent}
          showsVerticalScrollIndicator={false}
        >
          <View style={styles.formContainer}>
            {error && <Text style={styles.errorText}>{error}</Text>}

            <Text style={styles.label}>Preferred Plant Stages:</Text>
            <TagGroup
              mode="multiple"
              values={Object.values(PlantStage)}
              selectedValues={selectedStages}
              onToggleMulti={(val) =>
                setSelectedStages((prev) =>
                  prev.includes(val)
                    ? prev.filter((v) => v !== val)
                    : [...prev, val]
                )
              }
            />

            <Text style={styles.label}>Preferred Categories:</Text>
            <TagGroup
              mode="multiple"
              values={Object.values(PlantCategory)}
              selectedValues={selectedCategories}
              onToggleMulti={(val) =>
                setSelectedCategories((prev) =>
                  prev.includes(val)
                    ? prev.filter((v) => v !== val)
                    : [...prev, val]
                )
              }
            />

            <Text style={styles.label}>Watering Need:</Text>
            <TagGroup
              mode="multiple"
              values={Object.values(WateringNeed)}
              selectedValues={selectedWatering}
              onToggleMulti={(val) =>
                setSelectedWatering((prev) =>
                  prev.includes(val)
                    ? prev.filter((v) => v !== val)
                    : [...prev, val]
                )
              }
            />

            <Text style={styles.label}>Light Requirement:</Text>
            <TagGroup
              mode="multiple"
              values={Object.values(LightRequirement)}
              selectedValues={selectedLightReq}
              onToggleMulti={(val) =>
                setSelectedLightReq((prev) =>
                  prev.includes(val)
                    ? prev.filter((v) => v !== val)
                    : [...prev, val]
                )
              }
            />

            <Text style={styles.label}>Size:</Text>
            <TagGroup
              mode="multiple"
              values={Object.values(Size)}
              selectedValues={selectedSize}
              onToggleMulti={(val) =>
                setSelectedSize((prev) =>
                  prev.includes(val)
                    ? prev.filter((v) => v !== val)
                    : [...prev, val]
                )
              }
            />

            <Text style={styles.label}>Indoor/Outdoor:</Text>
            <TagGroup
              mode="multiple"
              values={Object.values(IndoorOutdoor)}
              selectedValues={selectedIndoorOutdoor}
              onToggleMulti={(val) =>
                setSelectedIndoorOutdoor((prev) =>
                  prev.includes(val)
                    ? prev.filter((v) => v !== val)
                    : [...prev, val]
                )
              }
            />

            <Text style={styles.label}>Propagation Ease:</Text>
            <TagGroup
              mode="multiple"
              values={Object.values(PropagationEase)}
              selectedValues={selectedPropagationEase}
              onToggleMulti={(val) =>
                setSelectedPropagationEase((prev) =>
                  prev.includes(val)
                    ? prev.filter((v) => v !== val)
                    : [...prev, val]
                )
              }
            />

            <Text style={styles.label}>Pet Friendly:</Text>
            <TagGroup
              mode="multiple"
              values={Object.values(PetFriendly)}
              selectedValues={selectedPetFriendly}
              onToggleMulti={(val) =>
                setSelectedPetFriendly((prev) =>
                  prev.includes(val)
                    ? prev.filter((v) => v !== val)
                    : [...prev, val]
                )
              }
            />

            <Text style={styles.label}>Extras:</Text>
            <TagGroup
              mode="multiple"
              values={Object.values(Extras)}
              selectedValues={selectedExtras}
              onToggleMulti={(val) =>
                setSelectedExtras((prev) =>
                  prev.includes(val)
                    ? prev.filter((v) => v !== val)
                    : [...prev, val]
                )
              }
            />

            <ConfirmCancelButtons
              onConfirm={handleSave}
              confirmButtonText="Save"
              onCancel={handleCancel}
              cancelButtonText="Cancel"
              loading={isUpdating}
            />
          </View>
        </ScrollView>
      </LinearGradient>
    </View>
  );
};

export default SetUserPreferencesScreen;

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  gradientBackground: {
    flex: 1,
  },
  scrollContent: {
    paddingTop: 0,
    paddingBottom: 30,
  },
  formContainer: {
    backgroundColor: '#fff',
    marginHorizontal: 20,
    borderRadius: 12,
    padding: 16,
    ...Platform.select({
      ios: {
        shadowColor: '#000',
        shadowOpacity: 0.1,
        shadowRadius: 5,
        shadowOffset: { width: 0, height: 4 },
      },
      android: {
        elevation: 3,
      },
    }),
  },
  label: {
    fontSize: 14,
    fontWeight: '600',
    color: '#333',
    marginTop: 12,
    marginBottom: 6,
  },
  errorText: {
    color: '#FF6F61',
    marginBottom: 10,
    fontWeight: '600',
  },
  center: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
});
