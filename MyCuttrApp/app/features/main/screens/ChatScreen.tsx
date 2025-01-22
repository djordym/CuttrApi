// File: src/features/main/screens/ChatScreen.tsx
import React, {
  useMemo,
  useState,
  useEffect,
  useRef,
  useCallback,
} from 'react';
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
  Animated,
  Easing,
  ScrollView,
  Image,
} from 'react-native';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { useTranslation } from 'react-i18next';
import { useRoute, useNavigation } from '@react-navigation/native';
import { LinearGradient } from 'expo-linear-gradient';

// Hooks
import { useUserProfile } from '../hooks/useUser';
import { useUserMatches } from '../hooks/useUserMatches';
import { useMatchConversation } from '../hooks/useMatchConversation';

// Components
import { COLORS } from '../../../theme/colors';
import { headerStyles } from '../styles/headerStyles';
import { PlantCardWithInfo } from  '../components/PlantCardWithInfo';
import { MessageBubble } from '../components/MessageBubble';

// Types
import { MessageRequest } from '../../../types/apiTypes';
import { MatchResponse, MessageResponse } from '../../../types/apiTypes';

const SHELF_MAX_HEIGHT = 300; // Adjust to your taste

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

  // Hook for the active match's conversation
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

  // Auto-scroll on new messages
  const flatListRef = useRef<FlatList<MessageResponse>>(null);
  useEffect(() => {
    if (sortedMessages.length > 0) {
      setTimeout(() => {
        flatListRef.current?.scrollToEnd({ animated: true });
      }, 300);
    }
  }, [sortedMessages]);

  // Sending messages
  const [inputText, setInputText] = useState('');
  const handleSendMessage = useCallback(() => {
    if (!inputText.trim()) return;
    if (!activeMatchId) {
      Alert.alert('Error', 'No active match selected to send messages.');
      return;
    }
    const text = inputText.trim();
    setInputText('');
    const messageData: MessageRequest = {
      matchId: activeMatchId,
      messageText: text,
    };
    sendMessage(messageData, {
      onError: (error) => {
        console.error('Error sending message:', error);
        Alert.alert('Error', 'Failed to send message');
      },
    });
  }, [activeMatchId, inputText, sendMessage]);

  // ========= Match Tabs (replaces the old messy circle row) ========
  const renderMatchTabs = () => {
    if (!relevantMatches || relevantMatches.length === 0) return null;
    return (
      <ScrollView
        horizontal
        showsHorizontalScrollIndicator={false}
        style={styles.tabsContainer}
        contentContainerStyle={styles.tabsContent}
      >
        {relevantMatches.map((match) => {
          const isActive = match.matchId === activeMatchId;

          // We'll show the two plant images side by side or overlapped.
          const plant1Uri = match.plant1?.imageUrl;
          const plant2Uri = match.plant2?.imageUrl;

          return (
            <TouchableOpacity
              key={match.matchId}
              onPress={() => setActiveMatchId(match.matchId)}
              style={[
                styles.tabItem,
                isActive && styles.tabItemActive,
              ]}
              activeOpacity={0.8}
            >
              <View style={styles.overlappingContainer}>
                <View style={[styles.plantCircle, styles.plantCircleFront]}>
                  {plant1Uri ? (
                    <Image
                      source={{ uri: plant1Uri }}
                      style={styles.circleImage}
                    />
                  ) : (
                    <Ionicons name="leaf" size={22} color={COLORS.primary} />
                  )}
                </View>
                <View style={[styles.plantCircle, styles.plantCircleBack]}>
                  {plant2Uri ? (
                    <Image
                      source={{ uri: plant2Uri }}
                      style={styles.circleImage}
                    />
                  ) : (
                    <Ionicons name="leaf" size={22} color={COLORS.primary} />
                  )}
                </View>
              </View>

              <Text style={styles.tabLabel}>
                {match.plant1?.speciesName} & {match.plant2?.speciesName}
              </Text>
            </TouchableOpacity>
          );
        })}
      </ScrollView>
    );
  };

  // ============ Animated Shelf ============
  const [showShelf, setShowShelf] = useState<boolean>(true);
  const shelfAnim = useRef(new Animated.Value(SHELF_MAX_HEIGHT)).current; // start open

  const toggleShelf = useCallback(() => {
    const finalValue = showShelf ? 0 : SHELF_MAX_HEIGHT;
    Animated.timing(shelfAnim, {
      toValue: finalValue,
      duration: 300,
      easing: Easing.ease,
      useNativeDriver: false,
    }).start(() => {
      setShowShelf((prev) => !prev);
    });
  }, [showShelf, shelfAnim]);

  // For the active match, get the two plants
  const currentMatch = useMemo(() => {
    if (!relevantMatches || !activeMatchId) return null;
    return relevantMatches.find((m) => m.matchId === activeMatchId) || null;
  }, [relevantMatches, activeMatchId]);

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
        <Text style={styles.noMessagesText}>
          {t('chat_no_matches_with_user')}
        </Text>
      </SafeAreaProvider>
    );
  }

  const hasMessages = messages && messages.length > 0;

  return (
    <SafeAreaProvider style={styles.container}>
      {/* Header */}
      <LinearGradient
        style={headerStyles.headerGradient}
        colors={[COLORS.primary, COLORS.secondary]}
      >
        <View style={headerStyles.headerColumn1}>
          <TouchableOpacity
            style={headerStyles.headerBackButton}
            onPress={() => navigation.goBack()}
          >
            <Ionicons name="chevron-back" size={30} color={COLORS.textLight} />
          </TouchableOpacity>

          <Text style={headerStyles.headerTitle}>{t('chat_title')}</Text>
        </View>
      </LinearGradient>

      {/* Match "tabs" row */}
      <View style={styles.tabWrapper}>
      {renderMatchTabs()}
      </View>
      {/* Animated Shelf */}
      <Animated.View
        style={[
          styles.animatedShelf,
          {
            height: shelfAnim, // Animate the height
          },
        ]}
      >
        {/* Actual shelf content */}
        <View style={styles.shelfContent}>
          {currentMatch && (
            <View style={styles.shelfContainer}>
              {/* Each card is in a 50%-width container with aspect ratio */}
              <View style={styles.plantCardWrapper}>
                <PlantCardWithInfo
                  plant={currentMatch.plant1}
                  compact={true}
                />
              </View>
              <View style={styles.plantCardWrapper}>
                <PlantCardWithInfo
                  plant={currentMatch.plant2}
                  compact={true}
                />
              </View>
            </View>
          )}
        </View>

        {/* The "grabber" toggle button pinned to the bottom of the shelf */}
        <TouchableOpacity
          style={styles.shelfToggleButton}
          onPress={toggleShelf}
          activeOpacity={0.7}
        >
          <Ionicons
            name={showShelf ? 'chevron-up-outline' : 'chevron-down-outline'}
            size={22}
            color="#fff"
          />
          <Text style={styles.shelfToggleButtonText}>
            {showShelf ? t('Hide plants') : t('Show plants')}
          </Text>
        </TouchableOpacity>
      </Animated.View>

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
  noMessagesText: {
    fontSize: 16,
    color: COLORS.textDark,
    textAlign: 'center',
  },
  emptyChatContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },

  // ====== TABS (Replaces Overlapping Circle Row) ======
  tabWrapper: {
    // marginVertical: 8,
    backgroundColor: '#f9f9f9',
    borderBottomWidth: 1,
    borderBottomColor: '#ddd',
  },
  tabsContainer: {
    marginTop: 0,
    paddingVertical: 8,
    backgroundColor: '#f9f9f9',
  },
  tabsContent: {
    paddingHorizontal: 12,
    alignItems: 'center',
  },
  tabItem: {
    padding: 6,
    marginRight: 12,
    borderRadius: 6,
    backgroundColor: '#fff',
    alignItems: 'center',
    minWidth: 80,
    // shadow/elevation for tab item
    ...Platform.select({
      ios: {
        shadowColor: '#000',
        shadowOpacity: 0.08,
        shadowRadius: 3,
        shadowOffset: { width: 0, height: 2 },
      },
      android: {
        elevation: 2,
      },
    }),
  },
  tabItemActive: {
    borderColor: COLORS.accentGreen,
    borderWidth: 2,
  },
  overlappingContainer: {
    width: 40,
    height: 30,
    marginBottom: 4,
    position: 'relative',
  },
  plantCircle: {
    width: 28,
    height: 28,
    borderRadius: 14,
    backgroundColor: '#fff',
    position: 'absolute',
    alignItems: 'center',
    justifyContent: 'center',
    overflow: 'hidden',
  },
  plantCircleFront: {
    left: 0,
    zIndex: 2,
  },
  plantCircleBack: {
    left: 20,
    zIndex: 1,
  },
  circleImage: {
    width: '100%',
    height: '100%',
    resizeMode: 'cover',
  },
  tabLabel: {
    fontSize: 10,
    color: '#333',
    textAlign: 'center',
  },

  // ========== Animated Shelf ==========
  animatedShelf: {
    // pinned below the tabs
    overflow: 'hidden',
    backgroundColor: '#f1f1f1',
  },
  shelfContent: {
    flex: 1,
    paddingVertical: 8,
  },
  // Container that holds the 2 plant cards side by side
  shelfContainer: {
    flexDirection: 'row',
    justifyContent: 'space-evenly',
    paddingHorizontal: 12,
    paddingTop: 8,
  },
  // each half is 45% width, with an aspect ratio
  plantCardWrapper: {
    width: '45%',
    aspectRatio: 3 / 4,
    borderRadius: 8,
    overflow: 'hidden',
    backgroundColor: '#fff',
  },
  shelfToggleButton: {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    height: 32,
    backgroundColor: COLORS.accentGreen,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
  },
  shelfToggleButtonText: {
    marginLeft: 6,
    color: '#fff',
    fontWeight: '600',
    fontSize: 14,
  },

  // ========== Chat bubbles & list ==========
  listContent: {
    padding: 10,
    paddingBottom: 60, // space for input
  },

  // ========== Input ==========
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
