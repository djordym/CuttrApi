// File: src/features/main/screens/ChatScreen.tsx

import React, { useMemo, useState, useEffect, useRef, useCallback } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ActivityIndicator,
  TextInput,
  TouchableOpacity,
  FlatList,
  KeyboardAvoidingView,
  Platform,
  Alert,
} from 'react-native';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { useTranslation } from 'react-i18next';
import { useRoute, useNavigation } from '@react-navigation/native';

// Hooks
import { useUserProfile } from '../hooks/useUser';
import { useUserMatches } from '../hooks/useUserMatches';
import { useMatchConversation } from '../hooks/useMatchConversation';
import { COLORS } from '../../../theme/colors';
// Types
import { MatchResponse, MessageResponse } from '../../../types/apiTypes';
import { LinearGradient } from 'expo-linear-gradient';
import { headerStyles } from '../styles/headerStyles';

const ChatScreen: React.FC = () => {
  const { t } = useTranslation();
  const route = useRoute();
  const navigation = useNavigation();

  // We'll receive otherUserId from ConnectionsScreen
  const { otherUserId } = route.params as { otherUserId: number };

  // Current user
  const {
    data: userProfile,
    isLoading: loadingProfile,
    isError: errorProfile,
  } = useUserProfile();

  // All matches for me
  const {
    data: myMatches,
    isLoading: loadingMatches,
    isError: errorMatches,
  } = useUserMatches();

  // Filter to find the relevant matches with otherUser
  const relevantMatches = useMemo<MatchResponse[]>(() => {
    if (!myMatches || !userProfile) return [];
    const myUserId = userProfile.userId;

    return myMatches.filter((m) => {
      return (
        (m.user1.userId === myUserId && m.user2.userId === otherUserId) ||
        (m.user2.userId === myUserId && m.user1.userId === otherUserId)
      );
    });
  }, [myMatches, userProfile, otherUserId]);

  // Active matchId (the sub-channel tab)
  const [activeMatchId, setActiveMatchId] = useState<number | null>(null);

  // If we have matches but no activeMatchId yet, default to the first one
  useEffect(() => {
    if (relevantMatches.length > 0 && !activeMatchId) {
      setActiveMatchId(relevantMatches[0].matchId);
    }
  }, [relevantMatches, activeMatchId]);

  // Now use the combined hook for the active match
  const {
    messages,
    isLoadingMessages,
    isErrorMessages,
    refetchMessages,
    sendMessage,
    isSending,
  } = useMatchConversation(activeMatchId ?? 0);

  // Sort messages by timestamp ascending
  const sortedMessages = useMemo(() => {
    if (!messages) return [];
    return [...messages].sort(
      (a, b) => new Date(a.sentAt).getTime() - new Date(b.sentAt).getTime()
    );
  }, [messages]);

  // For scrolling to bottom
  const flatListRef = useRef<FlatList<MessageResponse>>(null);

  // Auto-scroll on new messages
  useEffect(() => {
    if (sortedMessages.length > 0) {
      setTimeout(() => {
        flatListRef.current?.scrollToEnd({ animated: true });
      }, 300);
    }
  }, [sortedMessages]);

  // Input for sending
  const [inputText, setInputText] = useState('');

  const handleSendMessage = useCallback(() => {
    if (!inputText.trim()) return;
    if (!activeMatchId) {
      Alert.alert('Error', 'No active match selected to send messages.');
      return;
    }
    const text = inputText.trim();
    setInputText('');

    // Use our combined hook's mutation
    sendMessage(
      { messageText: text },
      {
        onError: (error) => {
          console.error('Error sending message:', error);
          Alert.alert('Error', 'Failed to send message');
        },
        onSuccess: () => {
          // Possibly do any success logic here
        },
      }
    );
  }, [activeMatchId, inputText, sendMessage]);

  // Tab row for multiple matches
  const renderTabs = () => {
    if (relevantMatches.length <= 1) return null;
    return (
      <View style={styles.tabContainer}>
        {relevantMatches.map((match) => {
          const isActive = match.matchId === activeMatchId;
          // Label with "Plant1 ↔ Plant2"
          const p1 = match.plant1?.speciesName || 'Plant1';
          const p2 = match.plant2?.speciesName || 'Plant2';
          const label = `${p1} ↔ ${p2}`;

          return (
            <TouchableOpacity
              key={match.matchId}
              onPress={() => setActiveMatchId(match.matchId)}
              style={[styles.tabButton, isActive && styles.tabButtonActive]}
            >
              <Text
                style={[styles.tabButtonText, isActive && styles.tabButtonTextActive]}
                numberOfLines={1}
              >
                {label}
              </Text>
            </TouchableOpacity>
          );
        })}
      </View>
    );
  };

  // Render states
  if (loadingProfile || loadingMatches || (activeMatchId && isLoadingMessages)) {
    return (
      <SafeAreaProvider style={styles.centerContainer}>
        <ActivityIndicator size="large" color={COLORS.primary} />
        <Text style={styles.loadingText}>{t('chat_loading_conversation')}</Text>
      </SafeAreaProvider>
    );
  }
  if (errorProfile || errorMatches || isErrorMessages) {
    return (
      <SafeAreaProvider style={styles.centerContainer}>
        <Text style={styles.errorText}>{t('chat_error_message')}</Text>
        <TouchableOpacity style={styles.retryButton} onPress={refetchMessages}>
          <Text style={styles.retryButtonText}>{t('chat_retry_button')}</Text>
        </TouchableOpacity>
      </SafeAreaProvider>
    );
  }
  if (relevantMatches.length === 0) {
    // No matches with this user
    return (
      <SafeAreaProvider style={styles.centerContainer}>
        <Text style={styles.noMessagesText}>{t('chat_no_matches_with_user')}</Text>
      </SafeAreaProvider>
    );
  }

  const hasMessages = messages && messages.length > 0;

  return (
    <SafeAreaProvider style={styles.container}>
      {/* Header */}
      <LinearGradient style={headerStyles.headerGradient} colors={[COLORS.primary, COLORS.secondary]}>
        <View style={headerStyles.headerRowChat}>
        <TouchableOpacity style={headerStyles.headerBackButton} onPress={() => navigation.goBack()}>
          <Ionicons name="chevron-back" size={30} color={COLORS.textLight}/>
        </TouchableOpacity>

        <Text style={headerStyles.headerTitle}>{t('chat_title')}</Text>
        </View>
      </LinearGradient>

      {/* Tab row if multiple matches */}
      {renderTabs()}

      {/* Chat feed */}
      {!hasMessages ? (
        <View style={styles.emptyChatContainer}>
          <Text style={styles.noMessagesText}>{t('chat_no_messages_yet')}</Text>
        </View>
      ) : (
        <FlatList
          ref={flatListRef}
          data={sortedMessages}
          keyExtractor={(item) => item.messageId.toString()}
          contentContainerStyle={styles.listContent}
          renderItem={({ item }) => {
            const isMine = item.senderUserId === userProfile?.userId;
            return <MessageBubble message={item} isMine={isMine} />;
          }}
        />
      )}

      {/* Input field */}
      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
        keyboardVerticalOffset={10}
      >
        <View style={styles.inputContainer}>
          <TextInput
            style={styles.textInput}
            value={inputText}
            onChangeText={setInputText}
            placeholder={t('chat_message_placeholder')}
            multiline
          />
          {isSending ? (
            <ActivityIndicator style={{ marginRight: 12 }} color={COLORS.primary} />
          ) : (
            <TouchableOpacity onPress={handleSendMessage} style={styles.sendButton}>
              <Ionicons name="send" size={20} color={COLORS.background} />
            </TouchableOpacity>
          )}
        </View>
      </KeyboardAvoidingView>
    </SafeAreaProvider>
  );
};

/** Single message bubble component */
interface BubbleProps {
  message: MessageResponse;
  isMine: boolean;
}
const MessageBubble: React.FC<BubbleProps> = ({ message, isMine }) => {
  return (
    <View
      style={[
        styles.bubbleContainer,
        isMine ? styles.bubbleRightContainer : styles.bubbleLeftContainer,
      ]}
    >
      <View style={[styles.bubble, isMine ? styles.bubbleRight : styles.bubbleLeft]}>
        <Text style={styles.bubbleText}>{message.messageText}</Text>
        <Text style={styles.timestamp}>
          {new Date(message.sentAt).toLocaleTimeString([], {
            hour: '2-digit',
            minute: '2-digit',
          })}
        </Text>
      </View>
    </View>
  );
};

export default ChatScreen;

/* ----------------------- STYLES ----------------------- */

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: COLORS.background,
  },
  centerContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: 20,
  },
  loadingText: {
    fontSize: 16,
    color: COLORS.textDark,
    marginTop: 10,
    textAlign: 'center',
  },
  errorText: {
    fontSize: 16,
    color: COLORS.textDark,
    marginBottom: 10,
    textAlign: 'center',
  },
  retryButton: {
    backgroundColor: COLORS.accentGreen,
    borderRadius: 8,
    paddingHorizontal: 16,
    paddingVertical: 10,
    marginTop: 10,
  },
  retryButtonText: {
    color: '#fff',
    fontWeight: '600',
  },
  noMessagesText: {
    fontSize: 16,
    color: COLORS.textDark,
    textAlign: 'center',
  },
  tabContainer: {
    flexDirection: 'row',
    backgroundColor: '#fff',
    marginHorizontal: 10,
    marginTop: 10,
    borderRadius: 8,
    elevation: 2,
    overflow: 'hidden',
  },
  tabButton: {
    paddingHorizontal: 12,
    paddingVertical: 8,
  },
  tabButtonActive: {
    backgroundColor: COLORS.accentGreen,
  },
  tabButtonText: {
    fontSize: 14,
    color: COLORS.textDark,
  },
  tabButtonTextActive: {
    color: COLORS.textLight,
    fontWeight: '600',
  },

  emptyChatContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },

  listContent: {
    padding: 10,
    paddingBottom: 60, // space for input
  },
  bubbleContainer: {
    marginVertical: 6,
  },
  bubbleLeftContainer: {
    alignSelf: 'flex-start',
  },
  bubbleRightContainer: {
    alignSelf: 'flex-end',
  },
  bubble: {
    maxWidth: '80%',
    borderRadius: 8,
    paddingHorizontal: 12,
    paddingVertical: 8,
  },
  bubbleLeft: {
    backgroundColor: COLORS.bubbleLeft,
    borderTopLeftRadius: 0,
  },
  bubbleRight: {
    backgroundColor: COLORS.bubbleRight,
    borderTopRightRadius: 0,
  },
  bubbleText: {
    fontSize: 14,
    color: '#333',
  },
  timestamp: {
    marginTop: 5,
    fontSize: 10,
    color: '#777',
    textAlign: 'right',
  },

  inputContainer: {
    flexDirection: 'row',
    paddingVertical: 8,
    paddingHorizontal: 10,
    backgroundColor: '#fff',
    borderTopWidth: 1,
    borderTopColor: '#ddd',
    alignItems: 'flex-end',
  },
  textInput: {
    flex: 1,
    minHeight: 40,
    maxHeight: 100,
    backgroundColor: '#f2f2f2',
    borderRadius: 20,
    paddingHorizontal: 12,
    paddingVertical: 8,
    marginRight: 8,
    fontSize: 14,
  },
  sendButton: {
    backgroundColor: COLORS.accentGreen,
    borderRadius: 20,
    padding: 10,
  },
});
