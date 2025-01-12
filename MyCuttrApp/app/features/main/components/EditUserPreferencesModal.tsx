import React, { useState, useEffect } from 'react';
import {
  Modal,
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  ScrollView,
  Alert,
  TextInput,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { SafeAreaView } from 'react-native-safe-area-context';

import { useUserPreferences } from '../hooks/usePreferences';
import { UserPreferencesResponse } from '../../../types/apiTypes';

type EditUserPreferencesModalProps = {
  visible: boolean;
  onClose: () => void;
};

export const EditUserPreferencesModal: React.FC<EditUserPreferencesModalProps> = ({
  visible,
  onClose,
}) => {
  const { data: prefs, updatePreferences, isUpdating } = useUserPreferences();

  // We keep local copies of the arrays we want to edit
  const [localStages, setLocalStages] = useState<string[]>([]);
  const [localCategories, setLocalCategories] = useState<string[]>([]);

  // For adding new items
  const [newStageText, setNewStageText] = useState('');
  const [newCategoryText, setNewCategoryText] = useState('');

  // Sync local state with server data on open
  useEffect(() => {
    if (prefs && visible) {
      setLocalStages(prefs.preferedPlantStage || []);
      setLocalCategories(prefs.preferedPlantCategory || []);
      setNewStageText('');
      setNewCategoryText('');
    }
  }, [prefs, visible]);

  // Remove item from local array
  const removeStage = (item: string) => {
    setLocalStages((prev) => prev.filter((v) => v !== item));
  };
  const removeCategory = (item: string) => {
    setLocalCategories((prev) => prev.filter((v) => v !== item));
  };

  // Add item to local array
  const addStage = () => {
    if (newStageText.trim()) {
      setLocalStages((prev) => [...prev, newStageText.trim()]);
      setNewStageText('');
    }
  };
  const addCategory = () => {
    if (newCategoryText.trim()) {
      setLocalCategories((prev) => [...prev, newCategoryText.trim()]);
      setNewCategoryText('');
    }
  };

  // On save, call updatePreferences
  const handleSave = async () => {
    if (!prefs) return;

    const updated: UserPreferencesResponse = {
      ...prefs,
      preferedPlantStage: localStages,
      preferedPlantCategory: localCategories,
      // repeat for other arrays (if any), or just keep them unchanged
    };

    try {
      await updatePreferences(updated);
      onClose();
    } catch (err) {
      console.error('Failed to update preferences:', err);
      Alert.alert('Error', 'Could not update preferences at this time.');
    }
  };

  // Body UI for listing chips
  const renderChip = (item: string, onRemove: (val: string) => void) => {
    return (
      <View style={styles.chip} key={item}>
        <Text style={styles.chipText}>{item}</Text>
        <TouchableOpacity onPress={() => onRemove(item)} style={styles.removeChipBtn}>
          <Ionicons name="close-circle" size={18} color="#fff" />
        </TouchableOpacity>
      </View>
    );
  };

  return (
    <Modal
      visible={visible}
      animationType="slide"
      onRequestClose={onClose}
      transparent
    >
      <SafeAreaView style={styles.modalOverlay}>
        <View style={styles.modalContainer}>
          {/* Header */}
          <View style={styles.header}>
            <Text style={styles.headerTitle}>Edit Filters</Text>
            <TouchableOpacity style={styles.closeButton} onPress={onClose}>
              <Ionicons name="close" size={24} color="#333" />
            </TouchableOpacity>
          </View>

          {/* Body */}
          <ScrollView style={styles.body}>
            {/* PREFERED STAGES */}
            <Text style={styles.sectionTitle}>Preferred Plant Stages</Text>
            <View style={styles.chipsContainer}>
              {localStages.map((stage) => renderChip(stage, removeStage))}
            </View>
            <View style={styles.addRow}>
              <TextInput
                style={styles.textInput}
                placeholder="Add a stage (e.g. 'Seedling')"
                value={newStageText}
                onChangeText={setNewStageText}
              />
              <TouchableOpacity onPress={addStage} style={styles.addButton}>
                <Ionicons name="add-circle" size={28} color="#1EAE98" />
              </TouchableOpacity>
            </View>

            {/* PREFERED CATEGORIES */}
            <Text style={styles.sectionTitle}>Preferred Plant Categories</Text>
            <View style={styles.chipsContainer}>
              {localCategories.map((cat) => renderChip(cat, removeCategory))}
            </View>
            <View style={styles.addRow}>
              <TextInput
                style={styles.textInput}
                placeholder="Add category (e.g. 'Herbs')"
                value={newCategoryText}
                onChangeText={setNewCategoryText}
              />
              <TouchableOpacity onPress={addCategory} style={styles.addButton}>
                <Ionicons name="add-circle" size={28} color="#1EAE98" />
              </TouchableOpacity>
            </View>

            {/* Repeat for WateringNeed, LightRequirement, etc. if you want */}
          </ScrollView>

          {/* Footer */}
          <View style={styles.footer}>
            <TouchableOpacity
              style={[styles.button, styles.cancelButton]}
              onPress={onClose}
              disabled={isUpdating}
            >
              <Text style={styles.buttonText}>Cancel</Text>
            </TouchableOpacity>
            <TouchableOpacity
              style={[styles.button, styles.saveButton]}
              onPress={handleSave}
              disabled={isUpdating}
            >
              <Text style={styles.buttonText}>
                {isUpdating ? 'Saving...' : 'Save'}
              </Text>
            </TouchableOpacity>
          </View>
        </View>
      </SafeAreaView>
    </Modal>
  );
};

const styles = StyleSheet.create({
  modalOverlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.4)', // translucent overlay
    justifyContent: 'center',
  },
  modalContainer: {
    marginHorizontal: 16,
    backgroundColor: '#fff',
    borderRadius: 12,
    padding: 16,
    maxHeight: '90%',
    flex: 1,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  headerTitle: {
    fontSize: 18,
    fontWeight: '700',
    color: '#333',
  },
  closeButton: {
    padding: 4,
  },
  body: {
    marginTop: 10,
    marginBottom: 10,
  },
  sectionTitle: {
    marginTop: 12,
    marginBottom: 6,
    fontSize: 16,
    fontWeight: '600',
    color: '#333',
  },
  chipsContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    marginBottom: 8,
  },
  chip: {
    flexDirection: 'row',
    backgroundColor: '#1EAE98',
    paddingHorizontal: 10,
    paddingVertical: 5,
    borderRadius: 16,
    alignItems: 'center',
    marginRight: 8,
    marginBottom: 8,
  },
  chipText: {
    color: '#fff',
    fontWeight: '600',
  },
  removeChipBtn: {
    marginLeft: 6,
  },
  addRow: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 12,
  },
  textInput: {
    flex: 1,
    borderWidth: 1,
    borderColor: '#ccc',
    borderRadius: 8,
    paddingHorizontal: 12,
    paddingVertical: 6,
    marginRight: 8,
  },
  addButton: {
    padding: 4,
  },
  footer: {
    flexDirection: 'row',
    justifyContent: 'flex-end',
    marginTop: 8,
  },
  button: {
    borderRadius: 6,
    paddingHorizontal: 16,
    paddingVertical: 10,
    marginLeft: 10,
  },
  cancelButton: {
    backgroundColor: '#ccc',
  },
  saveButton: {
    backgroundColor: '#1EAE98',
  },
  buttonText: {
    color: '#fff',
    fontWeight: '600',
  },
});
