import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  ScrollView,
  ActivityIndicator,
  Platform,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { MaterialIcons } from '@expo/vector-icons';
import { useNavigation } from '@react-navigation/native';
import { useUserPreferences } from '../hooks/usePreferences';

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
import { COLORS } from '../../../theme/colors';
import { UserPreferencesRequest } from '../../../types/apiTypes';

/** 
 * Simple multi-select tag group 
 */
const MultiSelectTagGroup = <T extends string>({
  values,
  selectedValues,
  onToggle,
}: {
  values: T[];
  selectedValues: T[];
  onToggle: (val: T) => void;
}) => {
  return (
    <View style={styles.tagGroupContainer}>
      {values.map((val) => {
        const isSelected = selectedValues.includes(val);
        return (
          <TouchableOpacity
            key={String(val)}
            style={[
              styles.singleTag,
              isSelected && styles.singleTagSelected,
            ]}
            onPress={() => onToggle(val)}
          >
            <Text
              style={[
                styles.singleTagText,
                isSelected && styles.singleTagTextSelected,
              ]}
            >
              {val}
            </Text>
          </TouchableOpacity>
        );
      })}
    </View>
  );
};

/**
 * Redesigned screen to set user preferences
 * - Styled similarly to AddPlantScreen
 * - Multi-select tags for each preference category
 */
const SetUserPreferencesScreen: React.FC = () => {
  const navigation = useNavigation();
  const {
    data: preferences,
    isLoading,
    isError,
    updatePreferences,
    isUpdating,
  } = useUserPreferences();

  // Local states for multi-select preferences
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

  // Load existing preferences
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

  // Multi-select toggle helper
  const handleToggle = <T extends string>(
    value: T,
    selectedList: T[],
    setList: React.Dispatch<React.SetStateAction<T[]>>
  ) => {
    if (selectedList.includes(value)) {
      setList(selectedList.filter((v) => v !== value));
    } else {
      setList([...selectedList, value]);
    }
  };

  const handleCancel = () => {
    navigation.goBack();
  };

  const handleSave = async () => {
    if (!preferences) return;
    setError(null);

    // Construct the updated preferences request without search radius
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

  // Loading/ Error states
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
      {/* Full Screen Gradient */}
      <LinearGradient
        colors={[COLORS.primary, COLORS.secondary]}
        style={styles.gradientBackground}
      >
        {/* Header */}
        <View style={styles.headerRow}>
          <Text style={styles.headerTitle}>User Preferences</Text>
          <MaterialIcons name="settings" size={24} color="#fff" />
        </View>

        <ScrollView
          contentContainerStyle={styles.scrollContent}
          showsVerticalScrollIndicator={false}
        >
          <View style={styles.formContainer}>
            {/* Error */}
            {error && <Text style={styles.errorText}>{error}</Text>}

            {/* PREFERENCES MULTI-SELECT SECTIONS */}
            {/* Plant Stages */}
            <Text style={styles.label}>Preferred Plant Stages:</Text>
            <MultiSelectTagGroup
              values={Object.values(PlantStage)}
              selectedValues={selectedStages}
              onToggle={(val) =>
                handleToggle(val, selectedStages, setSelectedStages)
              }
            />

            {/* Plant Categories */}
            <Text style={styles.label}>Preferred Categories:</Text>
            <MultiSelectTagGroup
              values={Object.values(PlantCategory)}
              selectedValues={selectedCategories}
              onToggle={(val) =>
                handleToggle(val, selectedCategories, setSelectedCategories)
              }
            />

            {/* Watering Need */}
            <Text style={styles.label}>Watering Need:</Text>
            <MultiSelectTagGroup
              values={Object.values(WateringNeed)}
              selectedValues={selectedWatering}
              onToggle={(val) =>
                handleToggle(val, selectedWatering, setSelectedWatering)
              }
            />

            {/* Light Requirement */}
            <Text style={styles.label}>Light Requirement:</Text>
            <MultiSelectTagGroup
              values={Object.values(LightRequirement)}
              selectedValues={selectedLightReq}
              onToggle={(val) =>
                handleToggle(val, selectedLightReq, setSelectedLightReq)
              }
            />

            {/* Size */}
            <Text style={styles.label}>Size:</Text>
            <MultiSelectTagGroup
              values={Object.values(Size)}
              selectedValues={selectedSize}
              onToggle={(val) =>
                handleToggle(val, selectedSize, setSelectedSize)
              }
            />

            {/* Indoor/Outdoor */}
            <Text style={styles.label}>Indoor/Outdoor:</Text>
            <MultiSelectTagGroup
              values={Object.values(IndoorOutdoor)}
              selectedValues={selectedIndoorOutdoor}
              onToggle={(val) =>
                handleToggle(val, selectedIndoorOutdoor, setSelectedIndoorOutdoor)
              }
            />

            {/* Propagation Ease */}
            <Text style={styles.label}>Propagation Ease:</Text>
            <MultiSelectTagGroup
              values={Object.values(PropagationEase)}
              selectedValues={selectedPropagationEase}
              onToggle={(val) =>
                handleToggle(val, selectedPropagationEase, setSelectedPropagationEase)
              }
            />

            {/* Pet Friendly */}
            <Text style={styles.label}>Pet Friendly:</Text>
            <MultiSelectTagGroup
              values={Object.values(PetFriendly)}
              selectedValues={selectedPetFriendly}
              onToggle={(val) =>
                handleToggle(val, selectedPetFriendly, setSelectedPetFriendly)
              }
            />

            {/* Extras */}
            <Text style={styles.label}>Extras:</Text>
            <MultiSelectTagGroup
              values={Object.values(Extras)}
              selectedValues={selectedExtras}
              onToggle={(val) =>
                handleToggle(val, selectedExtras, setSelectedExtras)
              }
            />

            {/* LOADING INDICATOR */}
            {isUpdating && (
              <ActivityIndicator
                size="small"
                color={COLORS.primary}
                style={{ marginVertical: 10 }}
              />
            )}

            {/* ACTIONS */}
            <View style={styles.actions}>
              <TouchableOpacity style={styles.saveButton} onPress={handleSave} disabled={isUpdating}>
                <Text style={styles.saveButtonText}>
                  {isUpdating ? 'Saving...' : 'Save'}
                </Text>
              </TouchableOpacity>
              <TouchableOpacity
                style={styles.cancelButton}
                onPress={handleCancel}
              >
                <Text style={styles.cancelButtonText}>Cancel</Text>
              </TouchableOpacity>
            </View>
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
  headerRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginHorizontal: 20,
    marginBottom: 10,
  },
  headerTitle: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#fff',
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
  tagGroupContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    marginBottom: 6,
  },
  singleTag: {
    borderWidth: 1,
    borderColor: COLORS.primary,
    borderRadius: 20,
    paddingVertical: 6,
    paddingHorizontal: 12,
    marginRight: 8,
    marginBottom: 8,
  },
  singleTagSelected: {
    backgroundColor: COLORS.primary,
  },
  singleTagText: {
    fontSize: 12,
    color: COLORS.primary,
    fontWeight: '600',
  },
  singleTagTextSelected: {
    color: '#fff',
  },
  actions: {
    flexDirection: 'column',
    justifyContent: 'flex-end',
    marginTop: 16,
  },
  cancelButton: {
    borderWidth: 1,
    borderColor: COLORS.primary,
    borderRadius: 8,
    paddingVertical: 10,
    paddingHorizontal: 16,
    margin: 10,
    marginTop: 0,
  },
  cancelButtonText: {
    fontSize: 14,
    color: COLORS.primary,
    fontWeight: '600',
    textAlign: 'center',
  },
  saveButton: {
    backgroundColor: COLORS.primary,
    borderRadius: 8,
    paddingVertical: 10,
    paddingHorizontal: 16,
    margin: 10,
  },
  saveButtonText: {
    fontSize: 14,
    color: '#fff',
    fontWeight: '600',
    textAlign: 'center',
  },
  center: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
});
