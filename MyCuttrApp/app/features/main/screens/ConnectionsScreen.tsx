// File: app/features/main/screens/ConnectionsScreen.tsx

import React, { useMemo, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ActivityIndicator,
  TouchableOpacity,
  FlatList,
  Image,
  Dimensions,
} from 'react-native';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { useTranslation } from 'react-i18next';
import { useNavigation } from '@react-navigation/native';

// Hooks
import { useUserMatches } from '../hooks/useUserMatches';
import { useUserProfile } from '../hooks/useUser';

// Types
import { MatchResponse, UserResponse } from '../../../types/apiTypes';
import { COLORS } from '../../../theme/colors';


const { width } = Dimensions.get('window');

const ConnectionsScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation();

  const {
    data: userProfile,
    isLoading: loadingProfile,
    isError: errorProfile,
  } = useUserProfile();

  const {
    data: matches,
    isLoading: loadingMatches,
    isError: errorMatches,
    refetch: refetchMatches,
  } = useUserMatches();

  if (!userProfile) {
    // Handle loading/error state for user profile
    return (
      <SafeAreaProvider style={styles.centerContainer}>
        <ActivityIndicator size="large" color={COLORS.primary} />
        <Text style={styles.loadingText}>{t('connections_loading')}</Text>
      </SafeAreaProvider>
    );
  }
  
  const myUserId = userProfile.userId;

  /**
   * Group matches by "other user" so we can show a single conversation entry per unique user.
   */
  const groupedConversations = useMemo(() => {
    if (!matches || !myUserId) return [];

    // Map<otherUserId, MatchResponse[]>
    const map = new Map<number, MatchResponse[]>();

    for (const m of matches) {
      // Identify which user is "other"
      const otherUserId =
        m.user1.userId === myUserId ? m.user2.userId : m.user1.userId;
      if (!map.has(otherUserId)) {
        map.set(otherUserId, []);
      }
      map.get(otherUserId)!.push(m);
    }

    // Convert the map to a simple array for FlatList
    return Array.from(map.entries()).map(([otherUserId, matchList]) => ({
      otherUserId,
      matchList,
    }));
  }, [matches, myUserId]);

  /**
   * Navigate to a details or conversation screen for the tapped user.
   */
  const handleConversationPress = useCallback(
    (otherUserId: number) => {
      navigation.navigate('Chat' as never, { otherUserId } as never);
    },
    [navigation]
  );

  // ---------- RENDER LOGIC ----------
  if (loadingProfile || loadingMatches) {
    return (
      <SafeAreaProvider style={styles.centerContainer}>
        <ActivityIndicator size="large" color={COLORS.primary} />
        <Text style={styles.loadingText}>{t('connections_loading')}</Text>
      </SafeAreaProvider>
    );
  }

  if (errorProfile || errorMatches) {
    return (
      <SafeAreaProvider style={styles.centerContainer}>
        <Text style={styles.errorText}>{t('connections_error')}</Text>
        <TouchableOpacity style={styles.retryButton} onPress={refetchMatches}>
          <Text style={styles.retryButtonText}>{t('connections_retry_button')}</Text>
        </TouchableOpacity>
      </SafeAreaProvider>
    );
  }

  const renderEmptyState = () => {
    return (
      <View style={styles.emptyStateContainer}>
        {/* Optionally use a placeholder illustration or icon */}
        <Ionicons name="people-outline" size={64} color={COLORS.accent} style={{ marginBottom: 20 }} />
        
        <Text style={styles.emptyStateTitle}>{t('connections_none_title')}</Text>
        <Text style={styles.emptyStateMessage}>
          {t('connections_none_message')}
        </Text>

        <TouchableOpacity
          style={styles.emptyStateButton}
          onPress={() => {
            // Possibly navigate to the Swipe screen or adjust filters
            navigation.navigate('SwipeScreen' as never);
          }}
        >
          <Text style={styles.emptyStateButtonText}>
            {t('connections_none_action')}
          </Text>
        </TouchableOpacity>
      </View>
    );
  };

  return (
    <SafeAreaProvider style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <Text style={styles.headerTitle}>
          {t('connections_title', 'Connections')}
        </Text>
      </View>

      {/* If no matches or empty grouping, show empty state */}
      {(!groupedConversations || groupedConversations.length === 0) && (
        renderEmptyState()
      )}

      {/* If we have data, render the list */}
      {groupedConversations && groupedConversations.length > 0 && (
        <FlatList
          data={groupedConversations}
          keyExtractor={(item) => item.otherUserId.toString()}
          contentContainerStyle={{ paddingBottom: 20 }}
          renderItem={({ item }) => {
            const { otherUserId, matchList } = item;
            const firstMatch = matchList[0];

            // Identify the "other user" object for display
            const isUser1Me = firstMatch.user1.userId === myUserId;
            const otherUser: UserResponse = isUser1Me
              ? firstMatch.user2
              : firstMatch.user1;

            // For future: you could find the "latest message" in this conversation, etc.
            return (
              <TouchableOpacity
                style={styles.rowContainer}
                onPress={() => handleConversationPress(otherUserId)}
                activeOpacity={0.8}
              >
                <Image
                  source={
                    otherUser.profilePictureUrl
                      ? { uri: otherUser.profilePictureUrl }
                      : require('../../../../assets/images/icon.png') // fallback placeholder
                  }
                  style={styles.avatar}
                />
                <View style={styles.textSection}>
                  <Text style={styles.userName}>{otherUser.name}</Text>
                  <Text style={styles.matchCount}>
                    {t('connections_matches_label', { count: matchList.length })}
                  </Text>
                </View>
                <Ionicons
                  name="chevron-forward"
                  size={24}
                  color={COLORS.textDark}
                />
              </TouchableOpacity>
            );
          }}
        />
      )}
    </SafeAreaProvider>
  );
};

export default ConnectionsScreen;

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
  centerContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  loadingText: {
    marginTop: 10,
    fontSize: 16,
    color: COLORS.textDark,
  },
  errorText: {
    fontSize: 16,
    color: COLORS.textDark,
    marginBottom: 10,
    textAlign: 'center',
  },
  retryButton: {
    paddingHorizontal: 16,
    paddingVertical: 10,
    backgroundColor: COLORS.primary,
    borderRadius: 8,
  },
  retryButtonText: {
    color: '#fff',
    fontWeight: '600',
  },
  header: {
    paddingHorizontal: 20,
    paddingVertical: 15,
    backgroundColor: COLORS.primary,
    borderBottomLeftRadius: 20,
    borderBottomRightRadius: 20,
    marginBottom: 10,
    elevation: 3,
  },
  headerTitle: {
    fontSize: 22,
    fontWeight: '700',
    color: COLORS.textLight,
  },
  rowContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#fff',
    marginHorizontal: 10,
    marginVertical: 6,
    borderRadius: 8,
    padding: 10,
    // Shadow/elevation for iOS/Android
    shadowColor: '#000',
    shadowOpacity: 0.1,
    shadowRadius: 5,
    shadowOffset: { width: 0, height: 2 },
    elevation: 2,
  },
  avatar: {
    width: 52,
    height: 52,
    borderRadius: 26,
    marginRight: 12,
    backgroundColor: '#eee',
  },
  textSection: {
    flex: 1,
    justifyContent: 'center',
  },
  userName: {
    fontSize: 16,
    fontWeight: '600',
    color: COLORS.textDark,
    marginBottom: 4,
  },
  matchCount: {
    fontSize: 14,
    color: '#777',
  },

  // Empty state
  emptyStateContainer: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 30,
  },
  emptyStateTitle: {
    fontSize: 20,
    fontWeight: 'bold',
    color: COLORS.textDark,
    marginBottom: 8,
    textAlign: 'center',
  },
  emptyStateMessage: {
    fontSize: 14,
    color: '#555',
    textAlign: 'center',
    marginBottom: 20,
  },
  emptyStateButton: {
    backgroundColor: COLORS.accent,
    paddingHorizontal: 16,
    paddingVertical: 10,
    borderRadius: 8,
    marginTop: 10,
  },
  emptyStateButtonText: {
    color: '#fff',
    fontWeight: '600',
    fontSize: 14,
  },
});
