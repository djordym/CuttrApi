import React, { useState } from 'react';
import { Modal, View, Text, StyleSheet, TouchableOpacity, ScrollView, ActivityIndicator } from 'react-native';
import { useMyPlants } from '../hooks/usePlants';
import { PlantResponse } from '../../../types/apiTypes';
import { Ionicons } from '@expo/vector-icons';

interface SelectPlantsModalProps {
  visible: boolean;
  onClose: () => void;
  onConfirm: (selectedPlantIds: number[]) => void;
}

export const SelectPlantsModal: React.FC<SelectPlantsModalProps> = ({ visible, onClose, onConfirm }) => {
  const { data: myPlants, isLoading, isError, refetch } = useMyPlants();
  const [selectedPlantIds, setSelectedPlantIds] = useState<number[]>([]);

  const toggleSelection = (id: number) => {
    setSelectedPlantIds((prev) =>
      prev.includes(id) ? prev.filter((pid) => pid !== id) : [...prev, id]
    );
  };

  const handleConfirm = () => {
    onConfirm(selectedPlantIds);
    setSelectedPlantIds([]);
  };

  const handleClose = () => {
    setSelectedPlantIds([]);
    onClose();
  };

  return (
    <Modal visible={visible} animationType="slide" transparent>
      <View style={styles.overlay}>
        <View style={styles.modalContainer}>
          <Text style={styles.title}>Select Your Plants</Text>
            <Text style={styles.subtitle}>Select the plants you want to offer for swapping.</Text>
          {isLoading && (
            <View style={styles.centerContent}>
              <ActivityIndicator size="large" color="#1EAE98" />
            </View>
          )}
          {isError && (
            <View style={styles.centerContent}>
              <Text style={styles.errorText}>Failed to load your plants.</Text>
              <TouchableOpacity onPress={() => refetch()} style={styles.retryButton}>
                <Text style={styles.retryButtonText}>Try Again</Text>
              </TouchableOpacity>
            </View>
          )}
          
          {myPlants && (
            <ScrollView style={styles.list}>
              {myPlants.map((plant: PlantResponse) => {
                const selected = selectedPlantIds.includes(plant.PlantId);
                return (
                  <TouchableOpacity 
                    key={plant.PlantId} 
                    onPress={() => toggleSelection(plant.PlantId)} 
                    style={styles.listItem}
                    accessibilityRole="checkbox"
                    accessibilityState={{ checked: selected }}
                  >
                    <Ionicons 
                      name={selected ? "checkbox-outline" : "checkbox"} 
                      size={24} 
                      color={selected ? "#1EAE98" : "#ccc"} 
                      style={{marginRight:10}} 
                    />
                    <Text style={styles.listItemText}>{plant.SpeciesName}</Text>
                  </TouchableOpacity>
                );
              })}
            </ScrollView>
          )}

          <View style={styles.actions}>
            <TouchableOpacity onPress={handleClose} style={styles.cancelButton} accessibilityRole="button" accessibilityLabel="Cancel" accessibilityHint="Close modal without changes">
              <Text style={styles.cancelButtonText}>Cancel</Text>
            </TouchableOpacity>
            <TouchableOpacity onPress={handleConfirm} style={styles.confirmButton} accessibilityRole="button" accessibilityLabel="Confirm selection" accessibilityHint="Send swipe requests for selected and non-selected plants">
              <Text style={styles.confirmButtonText}>Confirm</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>
    </Modal>
  );
};


const styles = StyleSheet.create({
  overlay: {
    flex:1,
    backgroundColor:'rgba(0,0,0,0.5)',
    justifyContent:'center',
    alignItems:'center'
  },
  modalContainer: {
    width:'90%',
    maxHeight:'80%',
    backgroundColor:'#fff',
    borderRadius:16,
    padding:20,
    justifyContent:'flex-start'
  },
  title:{
    fontSize:20,
    fontWeight:'700',
    color:'#333',
    marginBottom:10
  },
  subtitle:{
    fontSize:14,
    color:'#555',
    marginBottom:20
  },
  centerContent:{
    alignItems:'center',
    justifyContent:'center',
    paddingVertical:20
  },
  errorText:{
    fontSize:16,
    color:'#333',
    marginBottom:10
  },
  retryButton:{
    backgroundColor:'#1EAE98',
    paddingVertical:10,
    paddingHorizontal:20,
    borderRadius:8
  },
  retryButtonText:{
    color:'#fff',
    fontWeight:'600'
  },
  list:{
    maxHeight:300,
    marginBottom:20
  },
  listItem:{
    flexDirection:'row',
    alignItems:'center',
    paddingVertical:10,
    borderBottomWidth:1,
    borderBottomColor:'#eee'
  },
  listItemText:{
    fontSize:16,
    color:'#333'
  },
  actions:{
    flexDirection:'row',
    justifyContent:'flex-end',
    marginTop:10
  },
  cancelButton:{
    paddingHorizontal:15,
    paddingVertical:10,
    marginRight:10
  },
  cancelButtonText:{
    fontSize:16,
    color:'#333'
  },
  confirmButton:{
    backgroundColor:'#1EAE98',
    paddingHorizontal:15,
    paddingVertical:10,
    borderRadius:8
  },
  confirmButtonText:{
    fontSize:16,
    color:'#fff',
    fontWeight:'600'
  }
});
