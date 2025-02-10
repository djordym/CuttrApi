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
import Slider from '@react-native-community/slider';

import TagGroup from '../components/TagGroup';
import ConfirmCancelButtons from '../components/ConfirmCancelButtons';
import { COLORS } from '../../../theme/colors';
import { headerStyles } from '../styles/headerStyles';

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
import { useUserPreferences } from '../hooks/usePreferences';
import { useTranslation } from 'react-i18next';

const SetUserPreferencesScreen: React.FC = () => {
  const navigation = useNavigation();
  const { t } = useTranslation();
  const {
    data: preferences,
    isLoading,
    isError,
    updatePreferences,
    isUpdating,
  } = useUserPreferences();

  // Local states for each preference
  const [searchRadius, setSearchRadius] = useState<number>(10);
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

  // Load existing preferences on mount (or when preferences update)
  useEffect(() => {
    if (preferences) {
      setSearchRadius(preferences.searchRadius ?? 10);
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

  const handleSetSearchRadius = (val: number) => { 
    if (val === 500) setSearchRadius(40000);
    else setSearchRadius(val);
  };

  const handleCancel = () => {
    navigation.goBack();
  };

  const handleSave = async () => {
    if (!preferences) return;
    setError(null);

    const updated: UserPreferencesRequest = {
      ...preferences,
      searchRadius,
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
      setError(t('set_preferences_update_error'));
    }
  };

  if (isLoading) {
    return (
      <View style={styles.center}>
        <Text>{t('set_preferences_loading')}</Text>
      </View>
    );
  }

  if (isError || !preferences) {
    return (
      <View style={styles.center}>
        <Text>{t('set_preferences_error')}</Text>
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
            <Text style={headerStyles.headerTitle}>{t('set_preferences_title')}</Text>
          </View>
          <MaterialIcons name="settings" size={24} color="#fff" />
        </View>

        <ScrollView
          contentContainerStyle={styles.scrollContent}
          showsVerticalScrollIndicator={false}
        >
          <View style={styles.formContainer}>
            {error && <Text style={styles.errorText}>{error}</Text>}

            {/* Slider for Search Radius */}
            <Text style={styles.label}>
              {t('set_preferences_search_radius', { radius: searchRadius })}
            </Text>
            <Slider
              style={styles.slider}
              minimumValue={1}
              maximumValue={500}
              step={1}
              value={searchRadius}
              onSlidingComplete={handleSetSearchRadius}
              minimumTrackTintColor={COLORS.accentGreen}
              maximumTrackTintColor="#999"
              thumbTintColor={COLORS.accentGreen}
            />

            {/* Preferred Plant Stages */}
            <Text style={styles.label}>{t('set_preferences_preferred_plant_stages')}</Text>
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
              getLabel={(val) => t('plantStage.' + val)}
            />

            {/* Preferred Categories */}
            <Text style={styles.label}>{t('set_preferences_preferred_categories')}</Text>
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
              getLabel={(val) => t('plantCategory.' + val)}
            />

            {/* Watering Need */}
            <Text style={styles.label}>{t('set_preferences_watering_need')}</Text>
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
              getLabel={(val) => t('wateringNeed.' + val)}
            />

            {/* Light Requirement */}
            <Text style={styles.label}>{t('set_preferences_light_requirement')}</Text>
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
              getLabel={(val) => t('lightRequirement.' + val)}
            />

            {/* Size */}
            <Text style={styles.label}>{t('set_preferences_size')}</Text>
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
              getLabel={(val) => t('size.' + val)}
            />

            {/* Indoor/Outdoor */}
            <Text style={styles.label}>{t('set_preferences_indoor_outdoor')}</Text>
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
              getLabel={(val) => t('indoorOutdoor.' + val)}
            />

            {/* Propagation Ease */}
            <Text style={styles.label}>{t('set_preferences_propagation_ease')}</Text>
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
              getLabel={(val) => t('propagationEase.' + val)}
            />

            {/* Pet Friendly */}
            <Text style={styles.label}>{t('set_preferences_pet_friendly')}</Text>
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
              getLabel={(val) => t('petFriendly.' + val)}
            />

            {/* Extras */}
            <Text style={styles.label}>{t('set_preferences_extras')}</Text>
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
              getLabel={(val) => t('extras.' + val)}
            />

            <ConfirmCancelButtons
              onConfirm={handleSave}
              confirmButtonText={t('common.save')}
              onCancel={handleCancel}
              cancelButtonText={t('common.cancel')}
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
  slider: {
    width: '100%',
    height: 40,
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
