import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { MessageResponse } from '../../../types/apiTypes';
import { COLORS } from '../../../theme/colors';

interface BubbleProps {
  message: MessageResponse;
  isMine: boolean;
}
export const  MessageBubble: React.FC<BubbleProps> = ({ message, isMine }) => {
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

const styles = StyleSheet.create({
  bubbleContainer: {
    marginVertical: 6,
    paddingHorizontal: 8,
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
});
