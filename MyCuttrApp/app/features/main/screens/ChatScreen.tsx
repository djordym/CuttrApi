// File: src/features/main/screens/ChatScreen.tsx

import React, { useEffect, useMemo, useRef, useState, useCallback } from 'react';
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
  Image, // <-- Added Image import
} from 'react-native';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { useTranslation } from 'react-i18next';
import { useRoute, useNavigation } from '@react-navigation/native';
import { LinearGradient } from 'expo-linear-gradient';

// Hooks
import { useMyProfile } from '../hooks/useMyProfileHooks';
import { useMessages } from '../hooks/useMessages'; 
import { useOtherProfile } from '../hooks/useOtherProfile'; // Example custom hook

// Types
import { MessageResponse, MessageRequest } from '../../../types/apiTypes';

// Theme & Styles
import { COLORS } from '../../../theme/colors';
import { headerStyles } from '../styles/headerStyles';

// Components
import { MessageBubble } from '../components/MessageBubble';
import ProfileCardShelf, { ProfileCardShelfRef } from '../components/ProfileCardShelf';

const ChatScreen: React.FC = () => {
  const { t } = useTranslation();
  const route = useRoute();
  const navigation = useNavigation();

  // We expect both a connectionId and an otherUserId from the previous screen (ConnectionsScreen).
  // Adjust as needed based on your app’s navigation structure.
  const { connectionId, otherUserId } = route.params as {
    connectionId: number;
    otherUserId: number;
  };

  // Current user (to distinguish our messages)
  const {
    data: myProfile,
    isLoading: loadingMyProfile,
    isError: errorMyProfile,
  } = useMyProfile();

  // Other user's profile (for the ProfileCardShelf)
  const {
    data: otherUserProfile,
    isLoading: loadingOtherUser,
    isError: errorOtherUser,
  } = useOtherProfile(otherUserId);

  // Messages for this connection
  const {
    messages,
    isLoadingMessages,
    isErrorMessages,
    refetchMessages,
    sendMessage,
    isSending,
  } = useMessages(connectionId);

  // Sort messages by ascending timestamp
  const sortedMessages = useMemo(() => {
    if (!messages) return [];
    return [...messages].sort(
      (a, b) => new Date(a.sentAt).getTime() - new Date(b.sentAt).getTime()
    );
  }, [messages]);

  // Scroll to bottom on new messages
  const flatListRef = useRef<FlatList<MessageResponse>>(null);
  useEffect(() => {
    if (sortedMessages.length > 0) {
      setTimeout(() => {
        flatListRef.current?.scrollToEnd({ animated: true });
      }, 300);
    }
  }, [sortedMessages]);

  // Input field state
  const [inputText, setInputText] = useState('');

  // Sending messages
  const handleSendMessage = useCallback(() => {
    const text = inputText.trim();
    if (!text) return;

    setInputText('');
    const payload: MessageRequest = { messageText: text };
    sendMessage(payload, {
      onError: (error) => {
        console.error('Error sending message:', error);
        Alert.alert(t('chat_error'), t('chat_send_failed'));
      },
    });
  }, [inputText, sendMessage, t]);

  // Shelf ref - to close it when user focuses on the text input
  const shelfRef = useRef<ProfileCardShelfRef>(null);

  const handleInputFocus = useCallback(() => {
    // Closes the shelf if it’s open
    shelfRef.current?.closeShelf();
  }, []);

  // Loading / error states
  if (loadingMyProfile || isLoadingMessages || loadingOtherUser) {
    return (
      <SafeAreaProvider style={styles.centerContainer}>
        <ActivityIndicator size="large" color={COLORS.primary} />
        <Text style={styles.loadingText}>{t('chat_loading_conversation')}</Text>
      </SafeAreaProvider>
    );
  }

  if (errorMyProfile || isErrorMessages || errorOtherUser) {
    return (
      <SafeAreaProvider style={styles.centerContainer}>
        <Text style={styles.errorText}>{t('chat_error_message')}</Text>
        <TouchableOpacity style={styles.retryButton} onPress={() => refetchMessages()}>
          <Text style={styles.retryButtonText}>{t('chat_retry_button')}</Text>
        </TouchableOpacity>
      </SafeAreaProvider>
    );
  }

  // Navigate to a "Browse Matches" screen for this connection
  const handleBrowseMatches = () => {
    //navigation.navigate('BrowseMatches' as never, { connectionId } as never);
  };

  // Navigate to a screen to create a trade proposal
  const handleOpenTradeProposal = () => {
    navigation.navigate('MakeTradeProposal' as never, { connectionId, otherUserId } as never);
  };

  const handleNavigateToProfile = () => {
    navigation.navigate('OtherProfile' as never, { userId: otherUserProfile.userId } as never);
  }

  return (
    <SafeAreaProvider style={styles.container}>
      {/* Header */}
      <LinearGradient
        style={[headerStyles.headerGradient, { marginBottom: 0 }]}
        colors={[COLORS.primary, COLORS.secondary]}
      >
        <View style={headerStyles.headerColumn1}>
          <TouchableOpacity
            style={headerStyles.headerBackButton}
            onPress={() => navigation.goBack()}
          >
            <Ionicons name="chevron-back" size={30} color={COLORS.textLight} />
          </TouchableOpacity>
          {otherUserProfile && (
            <TouchableOpacity style={styles.headerUserInfo} onPress={handleNavigateToProfile}>
              <Image
                source={{ uri: otherUserProfile.profilePictureUrl }}
                style={styles.headerUserImage}
              />
              <Text style={headerStyles.headerTitle}>
                {otherUserProfile.name}
              </Text>
            </TouchableOpacity>
          )}
        </View>
      </LinearGradient>

      {/* Shelf showing the other user's profile */}
      {/* <View style={styles.shelfWrapper}>
        <ProfileCardShelf
          ref={shelfRef}
          userProfile={otherUserProfile}
        />
      </View> */}

      {/* "Browse Matches" button */}
      <TouchableOpacity style={styles.browseButton} onPress={handleBrowseMatches}>
        <Text style={styles.browseButtonText}>
          {t('chat_browse_matches_button', 'Browse Matches')}
        </Text>
      </TouchableOpacity>

      {/* Chat messages */}
      {sortedMessages.length === 0 ? (
        <View style={styles.emptyChatContainer}>
          <Text style={styles.noMessagesText}>{t('chat_no_messages_yet')}</Text>
        </View>
      ) : (
        <View style={styles.listContainer}>
          <FlatList
            ref={flatListRef}
            data={sortedMessages}
            keyExtractor={(item) => item.messageId.toString()}
            contentContainerStyle={styles.listContent}
            renderItem={({ item }) => {
              const isMine = item.senderUserId === myProfile?.userId;
              return <MessageBubble message={item} isMine={isMine} />;
            }}
          />
        </View>
      )}

      {/* Floating button for trade proposals */}
      <TouchableOpacity style={styles.tradeFab} onPress={handleOpenTradeProposal}>
        <Ionicons name="swap-horizontal" size={26} color="#fff" />
      </TouchableOpacity>

      {/* Message input */}
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
            onFocus={handleInputFocus} // <-- Close the shelf on focus
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

export default ChatScreen;

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

  // Shelf
  shelfWrapper: {
    // A simple wrapper around the shelf at the top
  },

  // "Browse Matches" button
  browseButton: {
    margin: 10,
    padding: 12,
    borderRadius: 8,
    backgroundColor: '#fff',
    alignSelf: 'center',
    minWidth: '60%',
    alignItems: 'center',
    shadowColor: '#000',
    shadowOpacity: 0.1,
    shadowRadius: 5,
    shadowOffset: { width: 0, height: 2 },
    elevation: 2,
  },
  browseButtonText: {
    color: COLORS.textDark,
    fontSize: 16,
    fontWeight: '600',
  },

  // If no messages
  emptyChatContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  noMessagesText: {
    fontSize: 16,
    color: COLORS.textDark,
    textAlign: 'center',
    paddingHorizontal: 20,
  },

  // Chat messages list
  listContainer: {
    flex: 1,
  },
  listContent: {
    padding: 8,
    paddingBottom: 60, // space above the input
  },

  // Input
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

  // Trade Proposal FAB
  tradeFab: {
    position: 'relative',
    alignSelf: 'flex-end',
    bottom: 20,
    right: 20,
    backgroundColor: COLORS.accentGreen,
    width: 56,
    height: 56,
    borderRadius: 28,
    alignItems: 'center',
    justifyContent: 'center',
    // Light shadow
    shadowColor: '#000',
    shadowOpacity: 0.1,
    shadowRadius: 4,
    shadowOffset: { width: 0, height: 2 },
    elevation: 4,
  },

  // New Styles for Header User Info
  headerUserInfo: {
    flexDirection: 'row',
    alignItems: 'center',
    marginLeft: 10, // Adjust as needed
  },
  headerUserImage: {
    borderColor: COLORS.accentGreen,
    borderWidth: 3,
    width: 60,
    height: 60,
    borderRadius: 30,
    marginRight: 8,
    backgroundColor: '#ccc', // Placeholder color in case image fails to load
  },
});
