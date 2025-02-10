// File: src/features/main/screens/ConnectionsScreen.tsx

import React, { useCallback } from 'react';
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

import { LinearGradient } from 'expo-linear-gradient';

import { useMyProfile } from '../hooks/useMyProfileHooks';
import { useConnections } from '../hooks/useConnections';

// Types
import { ConnectionResponse, UserResponse } from '../../../types/apiTypes';

// Theme & Styles
import { COLORS } from '../../../theme/colors';
import { headerStyles } from '../styles/headerStyles';

const { width } = Dimensions.get('window');

const ConnectionsScreen: React.FC = () => {
  const { t } = useTranslation();
  const navigation = useNavigation();

  // Fetch the current user's profile to identify myUserId
  const {
    data: userProfile,
    isLoading: loadingProfile,
    isError: errorProfile,
  } = useMyProfile();

  // Fetch the user's connections
  const {
    data: connections,
    isLoading: loadingConnections,
    isError: errorConnections,
    refetch: refetchConnections,
  } = useConnections();

  // If we haven't loaded the user yet, handle loading state
  if (!userProfile) {
    return (
      <SafeAreaProvider style={styles.centerContainer}>
        <ActivityIndicator size="large" color={COLORS.primary} />
        <Text style={styles.loadingText}>{t('connections_loading')}</Text>
      </SafeAreaProvider>
    );
  }
  
  const myUserId = userProfile.userId;

  /**
   * Navigate to a details or conversation screen for the selected connection.
   * Now passes both connectionId and otherUserId to ChatScreen.
   */
  const handleConversationPress = useCallback(
    (connectionId: number, otherUserId: number) => {
      navigation.navigate('Chat' as never, { connectionId, otherUserId } as never);
    },
    [navigation]
  );

  // ------------ RENDER LOGIC ------------
  if (loadingProfile || loadingConnections) {
    return (
      <SafeAreaProvider style={styles.centerContainer}>
        <ActivityIndicator size="large" color={COLORS.primary} />
        <Text style={styles.loadingText}>{t('connections_loading')}</Text>
      </SafeAreaProvider>
    );
  }

  if (errorProfile || errorConnections) {
    return (
      <SafeAreaProvider style={styles.centerContainer}>
        <Text style={styles.errorText}>{t('connections_error')}</Text>
        <TouchableOpacity style={styles.retryButton} onPress={refetchConnections}>
          <Text style={styles.retryButtonText}>{t('connections_retry_button')}</Text>
        </TouchableOpacity>
      </SafeAreaProvider>
    );
  }

  // If no connections, show empty state
  if (!connections || connections.length === 0) {
    return (
      <SafeAreaProvider style={styles.container}>
        <LinearGradient
          style={headerStyles.headerGradient}
          colors={[COLORS.primary, COLORS.secondary]}
        >
          <Text style={headerStyles.headerTitle}>
            {t('connections_title')}
          </Text>
        </LinearGradient>

        <View style={styles.emptyStateContainer}>
          <Ionicons
            name="people-outline"
            size={64}
            color={COLORS.accentGreen}
            style={{ marginBottom: 20 }}
          />
          <Text style={styles.emptyStateTitle}>{t('connections_none_title')}</Text>
          <Text style={styles.emptyStateMessage}>
            {t('connections_none_message')}
          </Text>
            <Text style={styles.emptyStateButtonText}>
              {t('connections_none_action')}
            </Text>
        </View>
      </SafeAreaProvider>
    );
  }

  return (
    <SafeAreaProvider style={styles.container}>
      {/* Header */}
      <LinearGradient
        style={headerStyles.headerGradient}
        colors={[COLORS.primary, COLORS.secondary]}
      >
        <Text style={headerStyles.headerTitle}>
          {t('connections_title', 'Connections')}
        </Text>
      </LinearGradient>

      {/* Connections list */}
      <FlatList
        data={connections}
        keyExtractor={(item) => item.connectionId.toString()}
        contentContainerStyle={{ paddingBottom: 20 }}
        renderItem={({ item }) => {
          const connection: ConnectionResponse = item;
          
          // Identify the other user
          const isUser1Me = connection.user1.userId === myUserId;
          const otherUser: UserResponse = isUser1Me
            ? connection.user2
            : connection.user1;

          return (
            <TouchableOpacity
              style={styles.rowContainer}
              onPress={() => handleConversationPress(connection.connectionId, otherUser.userId)}
              activeOpacity={0.8}
            >
              <Image
                source={
                  otherUser.profilePictureUrl
                    ? { uri: otherUser.profilePictureUrl }
                    : require('../../../../assets/images/icon.png') // fallback
                }
                style={styles.avatar}
              />
              <View style={styles.textSection}>
                <Text style={styles.userName}>{otherUser.name}</Text>
                {/* Using connection.numberOfMatches to show match info if relevant */}
                <Text style={styles.matchCount}>
                    {t(connection.numberOfMatches === 1 ? 'connections_match_label' : 'connections_matches_label', {
                    count: connection.numberOfMatches,
                    })}
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
    backgroundColor: COLORS.accentGreen,
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
