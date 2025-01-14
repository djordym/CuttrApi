import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  ScrollView,
  Alert,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
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
import COLORS from '../../../theme/colors';
import { useNavigation } from '@react-navigation/native';
import { log } from '../../../utils/logger';

/** 
 * A simple multi-select component for your enums.
 * Toggle selected state in local state, then persist with updatePreferences.
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

  // local states for each array
  const [selectedStages, setSelectedStages] = useState<PlantStage[]>([]);
  const [selectedCategories, setSelectedCategories] = useState<PlantCategory[]>([]);
  const [selectedLightReq, setSelectedLightReq] = useState<LightRequirement[]>([]);
  const [selectedWatering, setSelectedWatering] = useState<WateringNeed[]>([]);
  const [selectedSize, setSelectedSize] = useState<Size[]>([]);
  const [selectedIndoorOutdoor, setSelectedIndoorOutdoor] = useState<IndoorOutdoor[]>([]);
  const [selectedPropagationEase, setSelectedPropagationEase] = useState<PropagationEase[]>([]);
  const [selectedPetFriendly, setSelectedPetFriendly] = useState<PetFriendly[]>([]);
  const [selectedExtras, setSelectedExtras] = useState<Extras[]>([]);

  useEffect(() => {
    if (preferences) {
      setSelectedStages(preferences.preferedPlantStage || []);
      setSelectedCategories(preferences.preferedPlantCategory || []);
      setSelectedLightReq(preferences.preferedLightRequirement || []);
      setSelectedWatering(preferences.preferedWateringNeed || []);
      setSelectedSize(preferences.preferedSize || []);
      setSelectedIndoorOutdoor(preferences.preferedIndoorOutdoor || []);
      setSelectedPropagationEase(preferences.preferedPropagationEase || []);
      setSelectedPetFriendly(preferences.preferedPetFriendly || []);
      setSelectedExtras(preferences.preferedExtras || []);
    }

  }, [preferences]);

  const toggleSelection = <T extends string>(
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

  const handleSave = async () => {
    if (!preferences) return;
    const updated = {
      ...preferences,
      preferedPlantStage: selectedStages,
      preferedPlantCategory: selectedCategories,
      preferedLightRequirement: selectedLightReq,
      preferedWateringNeed: selectedWatering,
      preferedSize: selectedSize,
      preferedIndoorOutdoor: selectedIndoorOutdoor,
      preferedPropagationEase: selectedPropagationEase,
      preferedPetFriendly: selectedPetFriendly,
      preferedExtras: selectedExtras,
    };
    try {
      updatePreferences(updated);
      navigation.goBack();
    } catch (err) {
      Alert.alert('Error', 'Could not update preferences.');
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
      <ScrollView contentContainerStyle={styles.scrollContainer}>
        <Text style={styles.title}>Set Preferences</Text>

        {/* Example for selecting PlantStage */}
        <Text style={styles.label}>Preferred Plant Stage</Text>
        <View style={styles.multiRow}>
          {Object.values(PlantStage).map((stg) => {
            const selected = selectedStages.includes(stg);
            return (
              <TouchableOpacity
                key={stg}
                style={[styles.tag, selected && styles.tagSelected]}
                onPress={() => toggleSelection(stg, selectedStages, setSelectedStages)}
              >
                <Text style={[styles.tagText, selected && styles.tagTextSelected]}>
                  {stg}
                </Text>
              </TouchableOpacity>
            );
          })}
        </View>

        {/* Light Requirement */}
        <Text style={styles.label}>Light Requirement</Text>
        <View style={styles.multiRow}>
          {Object.values(LightRequirement).map((lreq) => {
            const selected = selectedLightReq.includes(lreq);
            return (
              <TouchableOpacity
                key={lreq}
                style={[styles.tag, selected && styles.tagSelected]}
                onPress={() => toggleSelection(lreq, selectedLightReq, setSelectedLightReq)}
              >
                <Text style={[styles.tagText, selected && styles.tagTextSelected]}>{lreq}</Text>
              </TouchableOpacity>
            );
          })}
        </View>

        {/* Watering */}
        <Text style={styles.label}>Watering Need</Text>
        <View style={styles.multiRow}>
          {Object.values(WateringNeed).map((wn) => {
            const selected = selectedWatering.includes(wn);
            return (
              <TouchableOpacity
                key={wn}
                style={[styles.tag, selected && styles.tagSelected]}
                onPress={() => toggleSelection(wn, selectedWatering, setSelectedWatering)}
              >
                <Text style={[styles.tagText, selected && styles.tagTextSelected]}>{wn}</Text>
              </TouchableOpacity>
            );
          })}
        </View>

        {/* Size */}
        <Text style={styles.label}>Size</Text>
        <View style={styles.multiRow}>
          {Object.values(Size).map((sz) => {
            const selected = selectedSize.includes(sz);
            return (
              <TouchableOpacity
                key={sz}
                style={[styles.tag, selected && styles.tagSelected]}
                onPress={() => toggleSelection(sz, selectedSize, setSelectedSize)}
              >
                <Text style={[styles.tagText, selected && styles.tagTextSelected]}>{sz}</Text>
              </TouchableOpacity>
            );
          })}
        </View>
        
        {/* Indoor/Outdoor */}
        <Text style={styles.label}>Indoor/Outdoor</Text>
        <View style={styles.multiRow}>
          {Object.values(IndoorOutdoor).map((io) => {
            const selected = selectedIndoorOutdoor.includes(io);
            return (
              <TouchableOpacity
                key={io}
                style={[styles.tag, selected && styles.tagSelected]}
                onPress={() => toggleSelection(io, selectedIndoorOutdoor, setSelectedIndoorOutdoor)}
              >
                <Text style={[styles.tagText, selected && styles.tagTextSelected]}>{io}</Text>
              </TouchableOpacity>
            );
          })}
        </View>

        {/* Propagation Ease */}
        <Text style={styles.label}>Propagation Ease</Text>
        <View style={styles.multiRow}>
          {Object.values(PropagationEase).map((pe) => {
            const selected = selectedPropagationEase.includes(pe);
            return (
              <TouchableOpacity
                key={pe}
                style={[styles.tag, selected && styles.tagSelected]}
                onPress={() => toggleSelection(pe, selectedPropagationEase, setSelectedPropagationEase)}
              >
                <Text style={[styles.tagText, selected && styles.tagTextSelected]}>{pe}</Text>
              </TouchableOpacity>
            );
          })}
        </View>
        
        {/* Pet Friendly */}
        <Text style={styles.label}>Pet Friendly</Text>
        <View style={styles.multiRow}>
          {Object.values(PetFriendly).map((pf) => {
            const selected = selectedPetFriendly.includes(pf);
            return (
              <TouchableOpacity
                key={pf}
                style={[styles.tag, selected && styles.tagSelected]}
                onPress={() => toggleSelection(pf, selectedPetFriendly, setSelectedPetFriendly)}
              >
                <Text style={[styles.tagText, selected && styles.tagTextSelected]}>{pf}</Text>
              </TouchableOpacity>
            );
          })}
        </View>

        {/* Extras */}
        <Text style={styles.label}>Extras</Text>
        <View style={styles.multiRow}>
          {Object.values(Extras).map((ex) => {
            const selected = selectedExtras.includes(ex);
            return (
              <TouchableOpacity
                key={ex}
                style={[styles.tag, selected && styles.tagSelected]}
                onPress={() => toggleSelection(ex, selectedExtras, setSelectedExtras)}
              >
                <Text style={[styles.tagText, selected && styles.tagTextSelected]}>{ex}</Text>
              </TouchableOpacity>
            );
          })}
        </View>

        <TouchableOpacity
          style={[styles.saveButton, isUpdating && { opacity: 0.5 }]}
          onPress={handleSave}
          disabled={isUpdating}
        >
          <Text style={styles.saveButtonText}>
            {isUpdating ? 'Saving...' : 'Save Preferences'}
          </Text>
        </TouchableOpacity>
      </ScrollView>
    </View>
  );
};

export default SetUserPreferencesScreen;

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
  scrollContainer: {
    padding: 16,
    paddingBottom: 40,
  },
  center: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  title: {
    fontSize: 20,
    fontWeight: '700',
    color: COLORS.textDark,
    marginBottom: 12,
  },
  label: {
    fontSize: 14,
    fontWeight: '600',
    color: COLORS.textDark,
    marginTop: 10,
    marginBottom: 6,
  },
  multiRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    marginBottom: 8,
  },
  tag: {
    paddingHorizontal: 10,
    paddingVertical: 6,
    borderWidth: 1,
    borderColor: COLORS.primary,
    borderRadius: 16,
    marginRight: 8,
    marginBottom: 8,
  },
  tagSelected: {
    backgroundColor: COLORS.primary,
  },
  tagText: {
    color: COLORS.primary,
    fontSize: 12,
    fontWeight: '600',
  },
  tagTextSelected: {
    color: '#fff',
  },
  saveButton: {
    backgroundColor: COLORS.primary,
    padding: 14,
    borderRadius: 8,
    alignItems: 'center',
    marginTop: 16,
  },
  saveButtonText: {
    color: '#fff',
    fontWeight: '700',
  },
});
