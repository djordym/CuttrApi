// File: app/features/main/screens/BrowseMatchesScreen.tsx

import React from "react";
import {
  View,
  Text,
  ActivityIndicator,
  FlatList,
  TouchableOpacity,
  StyleSheet,
} from "react-native";
import { SafeAreaProvider } from "react-native-safe-area-context";
import { useRoute, useNavigation } from "@react-navigation/native";
import { LinearGradient } from "expo-linear-gradient";

// Import your custom hooks and components
import { useUserMatches } from "../hooks/useUserMatches";
import { useMyProfile } from "../hooks/useMyProfileHooks";
import MatchCard from "../components/MatchCard";
import { headerStyles } from "../styles/headerStyles";
import { COLORS } from "../../../theme/colors";
import { Ionicons } from "@expo/vector-icons";

// Define the route params type
type RouteParams = {
  connectionId: number;
};

const BrowseMatchesScreen: React.FC = () => {
  // Get the connectionId from the route parameters
  const route = useRoute();
  const { connectionId } = route.params as RouteParams;

  const navigation = useNavigation();

  // Use react-query hook to fetch matches for this connection
  const { data: matches, isLoading, isError, refetch } = useUserMatches(connectionId);

  // Get the current user's profile (to pass currentUserId to MatchCard)
  const { data: myProfile, isLoading: profileLoading, isError: profileError } = useMyProfile();

  // Display a loading indicator if either query is loading
  if (isLoading || profileLoading) {
    return (
      <SafeAreaProvider style={styles.loadingContainer}>
        <ActivityIndicator size="large" color={COLORS.primary} />
      </SafeAreaProvider>
    );
  }

  // Display an error view if there is an error loading matches or profile
  if (isError || profileError) {
    return (
      <SafeAreaProvider style={styles.errorContainer}>
        <Text style={styles.errorText}>Failed to load matches.</Text>
        <TouchableOpacity style={styles.retryButton} onPress={() => refetch()}>
          <Text style={styles.retryButtonText}>Retry</Text>
        </TouchableOpacity>
      </SafeAreaProvider>
    );
  }

  // Render each match using the MatchCard component
  const renderItem = ({ item }: { item: any }) => (
    <MatchCard match={item} currentUserId={myProfile!.userId} />
  );

  return (
    <SafeAreaProvider style={styles.container}>
      <LinearGradient
        colors={[COLORS.primary, COLORS.secondary]}
        style={headerStyles.headerGradient}
      >
        <View style={headerStyles.headerColumn1}>
          <TouchableOpacity
            style={headerStyles.headerBackButton}
            onPress={() => navigation.goBack()}
          >
            <Ionicons name="chevron-back" size={30} color={COLORS.textLight} />
            
          </TouchableOpacity>
          <Text style={headerStyles.headerTitle}>Browse Matches</Text>
        </View>
      </LinearGradient>
      <FlatList
        data={matches}
        keyExtractor={(item) => item.matchId.toString()}
        renderItem={renderItem}
        contentContainerStyle={styles.listContent}
      />
    </SafeAreaProvider>
  );
};

export default BrowseMatchesScreen;

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
  },
  errorContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    paddingHorizontal: 20,
  },
  errorText: {
    fontSize: 16,
    color: COLORS.textDark,
    marginBottom: 10,
    textAlign: "center",
  },
  retryButton: {
    backgroundColor: COLORS.primary,
    paddingVertical: 10,
    paddingHorizontal: 20,
    borderRadius: 8,
  },
  retryButtonText: {
    color: "#fff",
    fontWeight: "600",
    fontSize: 16,
  },
  listContent: {
    paddingHorizontal: 16,
  },
});
