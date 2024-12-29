import React, { useState, useMemo, useCallback } from 'react';
import { View, Text, StyleSheet, ActivityIndicator, TouchableOpacity, Dimensions, Alert } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialIcons, Ionicons } from '@expo/vector-icons';
import { useLikablePlants } from '../hooks/useSwipe';
import { SwipeableCard } from '../components/SwipeableCard';
import { SwipeRequest, PlantResponse } from '../../../types/apiTypes';
import { swipeService } from '../../../api/swipeService';
import { useMyPlants } from '../hooks/usePlants';
import { SelectPlantsModal } from '../components/SelectPlantsModal';

const { width } = Dimensions.get('window');

const SwipeScreen: React.FC = () => {
  const { data: plants, isLoading, isError, refetch } = useLikablePlants();
  const { data: myPlants, isLoading: loadingMyPlants, isError: errorMyPlants, refetch: refetchMyPlants } = useMyPlants();

  const [localPlants, setLocalPlants] = useState<PlantResponse[]>(plants || []);
  const [modalVisible, setModalVisible] = useState(false);
  const [pendingRightSwipePlant, setPendingRightSwipePlant] = useState<PlantResponse | null>(null);

  React.useEffect(() => {
    if (plants) {
      setLocalPlants(plants);
    }
  }, [plants]);

  const topCard = useMemo(() => {
    return localPlants && localPlants.length > 0 ? localPlants[0] : null;
  }, [localPlants]);

  const removeTopCard = (plantId: number) => {
    setLocalPlants((prev) => prev.filter((p) => p.plantId !== plantId));
  };

  const handleSwipeLeft = useCallback(async (plantId: number) => {
    if (!myPlants) {
      Alert.alert("Error", "Your plants are not loaded yet. Try again.");
      return;
    }
    const swipeRequests: SwipeRequest[] = myPlants.map((userPlant) => ({
      swiperPlantId: userPlant.plantId,
      swipedPlantId: plantId,
      isLike: false
    }));
    try {
      await swipeService.sendSwipes(swipeRequests);
    } catch {
      Alert.alert("Error", "Failed to register swipe.");
    }
    removeTopCard(plantId);
  }, [myPlants]);

  const handleRightSwipeInitiation = useCallback((plantId: number) => {
    // Open modal to select plants
    const plantToLike = localPlants.find(p => p.plantId === plantId);
    if (!plantToLike) return;
    setPendingRightSwipePlant(plantToLike);
    setModalVisible(true);
  }, [localPlants]);

  const handleRightSwipeConfirm = useCallback(async (selectedPlantIds: number[]) => {
    if (!pendingRightSwipePlant || !myPlants) {
      return;
    }
    const plantId = pendingRightSwipePlant.plantId;
    const swipeRequests: SwipeRequest[] = myPlants.map((userPlant) => ({
      swiperPlantId: userPlant.plantId,
      swipedPlantId: plantId,
      isLike: selectedPlantIds.includes(userPlant.plantId)
    }));

    try {
      await swipeService.sendSwipes(swipeRequests);
    } catch {
      Alert.alert("Error", "Failed to send swipe requests.");
    }
    removeTopCard(plantId);
    setPendingRightSwipePlant(null);
    setModalVisible(false);
  }, [myPlants, pendingRightSwipePlant]);

  const handleModalClose = () => {
    // User canceled without confirming
    setPendingRightSwipePlant(null);
    setModalVisible(false);
  };

  const handlePassPress = () => {
    if (topCard) {
      handleSwipeLeft(topCard.plantId);
    }
  };

  const handleLikePress = () => {
    if (topCard) {
      handleRightSwipeInitiation(topCard.plantId);
    }
  };

  const handleFilterPress = () => {
    Alert.alert("Filters", "Filter screen or modal would appear here.");
  };

  if (isLoading || loadingMyPlants) {
    return (
      <SafeAreaView style={styles.loadingContainer}>
        <ActivityIndicator size="large" color="#1EAE98" />
        <Text style={styles.loadingText}>Loading Plants...</Text>
      </SafeAreaView>
    );
  }

  if (isError || errorMyPlants) {
    return (
      <SafeAreaView style={styles.errorContainer}>
        <Text style={styles.errorText}>Failed to load plants or your gallery.</Text>
        <TouchableOpacity onPress={() => {refetch(); refetchMyPlants();}} style={styles.retryButton}>
          <Text style={styles.retryButtonText}>Try Again</Text>
        </TouchableOpacity>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.headerTitle}>Cuttr</Text>
        <TouchableOpacity onPress={handleFilterPress} style={styles.headerFilterButton} accessible accessibilityLabel="Filter plants" accessibilityHint="Opens filter options">
          <Ionicons name="options" size={24} color="#333" />
        </TouchableOpacity>
      </View>

      <View style={styles.cardsContainer}>
        {localPlants.map((plant, index) => {
          const isTopCard = index === 0;
          return (
            <SwipeableCard
              key={plant.plantId}
              plant={plant}
              onSwipeLeft={handleSwipeLeft}
              onSwipeRight={handleRightSwipeInitiation}
            />
          );
        })}
        {localPlants.length === 0 && (
          <View style={styles.emptyState}>
            <Text style={styles.emptyStateText}>No more plants to show.</Text>
            <Text style={styles.emptyStateSubText}>Try adjusting your filters or come back later.</Text>
          </View>
        )}
      </View>

      {localPlants.length > 0 && (
        <View style={styles.actionsContainer}>
          <TouchableOpacity onPress={handlePassPress} style={[styles.actionButton, styles.passButton]} accessibilityRole="button" accessibilityLabel="Pass on this plant" accessibilityHint="Dislike and show next plant">
            <MaterialIcons name="close" size={32} color="#fff" />
          </TouchableOpacity>

          <TouchableOpacity onPress={handleLikePress} style={[styles.actionButton, styles.likeButton]} accessibilityRole="button" accessibilityLabel="Like this plant" accessibilityHint="Show interest and match with this plant">
            <MaterialIcons name="favorite" size={32} color="#fff" />
          </TouchableOpacity>
        </View>
      )}

      {myPlants && pendingRightSwipePlant && (
        <SelectPlantsModal 
          visible={modalVisible} 
          onClose={handleModalClose} 
          onConfirm={handleRightSwipeConfirm} 
        />
      )}

    </SafeAreaView>
  );
};

export default SwipeScreen;

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor:'#f8f8f8',
    alignItems:'center'
  },
  loadingContainer: {
    flex:1,
    justifyContent:'center',
    alignItems:'center'
  },
  loadingText:{
    marginTop:10,
    fontSize:16,
    color:'#333'
  },
  errorContainer: {
    flex:1,
    justifyContent:'center',
    alignItems:'center',
    padding:20
  },
  errorText:{
    fontSize:18,
    color:'#333',
    marginBottom:20,
    textAlign:'center'
  },
  retryButton:{
    backgroundColor:'#1EAE98',
    paddingVertical:12,
    paddingHorizontal:20,
    borderRadius:8
  },
  retryButtonText:{
    color:'#fff',
    fontSize:16,
    fontWeight:'600'
  },
  header: {
    width:'100%',
    flexDirection:'row',
    justifyContent:'space-between',
    alignItems:'center',
    paddingHorizontal:20,
    paddingVertical:10,
    borderBottomColor:'#ddd',
    borderBottomWidth:1,
    backgroundColor:'#fff'
  },
  headerTitle:{
    fontSize:24,
    fontWeight:'700',
    color:'#333'
  },
  headerFilterButton:{
    padding:8
  },
  cardsContainer:{
    flex:1,
    width:'100%',
    justifyContent:'center',
    alignItems:'center',
    paddingTop:20
  },
  actionsContainer:{
    flexDirection:'row',
    justifyContent:'space-evenly',
    alignItems:'center',
    width:'100%',
    paddingVertical:20,
    backgroundColor:'#fff',
    borderTopWidth:1,
    borderTopColor:'#ddd'
  },
  actionButton:{
    width:60,
    height:60,
    borderRadius:30,
    justifyContent:'center',
    alignItems:'center'
  },
  passButton:{
    backgroundColor:'#FF6B6B'
  },
  likeButton:{
    backgroundColor:'#1EAE98'
  },
  emptyState:{
    alignItems:'center',
    padding:20
  },
  emptyStateText:{
    fontSize:20,
    fontWeight:'700',
    color:'#333',
    marginBottom:8
  },
  emptyStateSubText:{
    fontSize:16,
    color:'#555',
    textAlign:'center'
  }
});
