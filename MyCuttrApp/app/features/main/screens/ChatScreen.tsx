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
  Image,
  Animated,
} from 'react-native';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { useTranslation } from 'react-i18next';
import { useRoute, useNavigation } from '@react-navigation/native';
import { LinearGradient } from 'expo-linear-gradient';

// Hooks
import { useMyProfile } from '../hooks/useMyProfileHooks';
import { useMessages } from '../hooks/useMessages';
import { useOtherProfile } from '../hooks/useOtherProfile';

// For fetching trade proposals
import { useQuery } from 'react-query';
import { connectionService } from '../../../api/connectionService';
import { TradeProposalResponse } from '../../../types/apiTypes';
import { TradeProposalStatus } from '../../../types/enums';

// Types
import { MessageResponse, MessageRequest } from '../../../types/apiTypes';

// Theme & Styles
import { COLORS } from '../../../theme/colors';
import { headerStyles } from '../styles/headerStyles';

// Components
import { MessageBubble } from '../components/MessageBubble';
import ProfileCardShelf, { ProfileCardShelfRef } from '../components/ProfileCardShelf';

// ---------- Custom hook for trade proposals ----------
const useTradeProposals = (connectionId: number) => {
  return useQuery<TradeProposalResponse[], Error>(
    ['tradeProposals', connectionId],
    () => connectionService.getTradeProposals(connectionId),
    { staleTime: 1000 * 60 }
  );
};

const ChatScreen: React.FC = () => {
  const { t } = useTranslation();
  const route = useRoute();
  const navigation = useNavigation();

  // Expect connectionId and otherUserId from route parameters
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

  // Trade proposals for this connection (to compute pending proposals)
  const { data: proposals } = useTradeProposals(connectionId);
  const pendingProposals = proposals?.filter(
    (proposal) => proposal.tradeProposalStatus === TradeProposalStatus.Pending
  ) || [];
  const pendingCount = pendingProposals.length;

  // --- Animated values for the trade proposals button ---
  const buttonWidth = useRef(new Animated.Value(56)).current;
  const glimmerAnim = useRef(new Animated.Value(1)).current;

  useEffect(() => {
    if (pendingCount > 0) {
      // Expand the background to reveal text
      Animated.timing(buttonWidth, {
        toValue: 210, // or whatever max width you want
        duration: 300,
        useNativeDriver: false,
      }).start();

      // Pulse
      Animated.loop(
        Animated.sequence([
          Animated.timing(glimmerAnim, {
            toValue: 0.8,
            duration: 500,
            useNativeDriver: false,
          }),
          Animated.timing(glimmerAnim, {
            toValue: 1,
            duration: 500,
            useNativeDriver: false,
          }),
        ]),
        { resetBeforeIteration: true }
      ).start();
    } else {
      // Shrink back to icon-only
      Animated.timing(buttonWidth, {
        toValue: 56,
        duration: 300,
        useNativeDriver: false,
      }).start();

      // Stop pulsing by resetting
      glimmerAnim.setValue(1);
    }
  }, [pendingCount]);

  // Animated style for the button
  const buttonAnimatedStyle = {
    width: buttonWidth,
    opacity: glimmerAnim,
  };

  // Sort messages by ascending sent time
  const sortedMessages = useMemo(() => {
    if (!messages) return [];
    return [...messages].sort(
      (a, b) => new Date(a.sentAt).getTime() - new Date(b.sentAt).getTime()
    );
  }, [messages]);

  // Auto-scroll to bottom when messages update
  const flatListRef = useRef<FlatList<MessageResponse>>(null);
  useEffect(() => {
    if (sortedMessages.length > 0) {
      setTimeout(() => {
        flatListRef.current?.scrollToEnd({ animated: true });
      }, 300);
    }
  }, [sortedMessages]);

  // Input state
  const [inputText, setInputText] = useState('');

  // Handle sending a message
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

  // Ref for ProfileCardShelf (if used) to close it on input focus
  const shelfRef = useRef<ProfileCardShelfRef>(null);
  const handleInputFocus = useCallback(() => {
    shelfRef.current?.closeShelf();
  }, []);

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

  // Navigate to "Browse Matches" screen
  const handleBrowseMatches = () => {
    navigation.navigate('BrowseMatches' as never, { connectionId } as never);
  };

  // Navigate to Trade Proposals screen
  const handleOpenTradeProposals = () => {
    navigation.navigate('TradeProposals' as never, { connectionId } as never);
  };

  // Navigate to Make Trade Proposal screen (for creating a new proposal)
  const handleOpenTradeProposal = () => {
    navigation.navigate('MakeTradeProposal' as never, { connectionId, otherUserId } as never);
  };

  // Navigate to the other user's profile screen
  const handleNavigateToProfile = () => {
    navigation.navigate('OtherProfile' as never, { userId: otherUserProfile.userId } as never);
  };

  return (
    <SafeAreaProvider style={styles.container}>
      {/* Header */}
      <LinearGradient
        style={[headerStyles.headerGradient, { marginBottom: 0 }]}
        colors={[COLORS.primary, COLORS.secondary]}
      >
        <View style={headerStyles.headerColumn1}>
          <TouchableOpacity style={headerStyles.headerBackButton} onPress={() => navigation.goBack()}>
            <Ionicons name="chevron-back" size={30} color={COLORS.textLight} />
          </TouchableOpacity>
          {otherUserProfile && (
            <TouchableOpacity style={styles.headerUserInfo} onPress={handleNavigateToProfile}>
              <Image
                source={{ uri: otherUserProfile.profilePictureUrl }}
                style={styles.headerUserImage}
              />
              <Text style={headerStyles.headerTitle}>{otherUserProfile.name}</Text>
            </TouchableOpacity>
          )}
        </View>
      </LinearGradient>

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

      <View style={styles.floatingButtonsContainer}>
        <Animated.View
          style={[
            styles.pendingButtonContainer,
            { width: buttonWidth, opacity: glimmerAnim },
          ]}
        >
          {/* Single Touchable: tapping text or circle calls the same onPress */}
          <TouchableOpacity
            style={styles.pendingButtonTouchable}
            onPress={handleOpenTradeProposals}
            activeOpacity={0.8}
          >
            {/* The “pill” background behind everything */}
            <View style={styles.backgroundBar}>
              {pendingCount > 0 && (
                <Text style={styles.pendingButtonText}>
                  {`${pendingCount} pending proposals`}
                </Text>
              )}
            </View>

            {/* The circle button (with icon + shadow) pinned on the left */}
            <View style={styles.circleButton}>
              <Ionicons name="document-text-outline" size={24} color="#fff" />
            </View>
          </TouchableOpacity>
        </Animated.View>

        {/* Floating Trade Proposals Button (right bottom) */}
        <TouchableOpacity style={styles.tradeFab} onPress={handleOpenTradeProposal}>
          <Ionicons name="swap-horizontal" size={26} color="#fff" />
        </TouchableOpacity>

      </View>

      {/* Message Input */}
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
            onFocus={handleInputFocus}
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
  // Header User Info
  headerUserInfo: {
    flexDirection: 'row',
    alignItems: 'center',
    marginLeft: 10,
  },
  headerUserImage: {
    borderColor: COLORS.accentGreen,
    borderWidth: 3,
    width: 60,
    height: 60,
    borderRadius: 30,
    marginRight: 8,
    backgroundColor: '#ccc',
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
    paddingBottom: 60,
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
  // Floating Buttons
  floatingButtonsContainer: {
    position: 'relative',
    flexDirection: 'row',
  },
  // Floating Trade Proposals Button (right bottom)
  tradeFab: {
    position: 'absolute',
    bottom: 20,
    right: 20,
    backgroundColor: COLORS.accentGreen,
    width: 56,
    height: 56,
    borderRadius: 28,
    alignItems: 'center',
    justifyContent: 'center',
    shadowColor: '#000',
    shadowOpacity: 0.1,
    shadowRadius: 4,
    elevation: 4,
  },

  // Pending Proposals Button (bottom-left)
  // The container we animate in width (and keep a fixed height).
  // We use overflow: 'hidden' so text slides out from under the circle.
  pendingButtonContainer: {
    position: 'absolute',
    bottom: 20,
    left: 20,
    height: 56,
    borderRadius: 28,
    overflow: 'hidden',
  },

  // Make the entire area clickable by wrapping in a single Touchable.
  pendingButtonTouchable: {
    flex: 1,
    borderRadius: 28,
  },

  // The pill behind the circle and text:
  backgroundBar: {
    ...StyleSheet.absoluteFillObject,
    backgroundColor: COLORS.accentGreen,
    borderRadius: 28,
    paddingLeft: 56, // Leave room for the circle on the left
    justifyContent: 'center', 
    paddingRight: 16, 
  },

  // The text that appears when expanded:
  pendingButtonText: {
    color: '#fff',
    fontWeight: '600',
    fontSize: 14,
    alignSelf: 'flex-end',
  },

  // The circle with the icon on top, pinned on the left:
  circleButton: {
    position: 'absolute',
    left: 0,
    top: 0,
    width: 56,
    height: 56,
    borderRadius: 28,
    backgroundColor: COLORS.accentGreen,
    alignItems: 'center',
    justifyContent: 'center',

    // Shadow (iOS + Android)
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.3,
    shadowRadius: 4,
    elevation: 5,
  },
});
