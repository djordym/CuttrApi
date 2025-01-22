// File: src/features/main/hooks/useMatchConversation.ts

import { useQuery, useMutation, useQueryClient } from 'react-query';
import { messageService } from '../../../api/messageService';
import { MessageRequest, MessageResponse } from '../../../types/apiTypes';

export const useMatchConversation = (matchId: number) => {
  const queryClient = useQueryClient();

  // 1) Query: fetch messages for this match
  const messagesQuery = useQuery<MessageResponse[], Error>(
    ['messages', matchId],
    () => messageService.getMessagesForMatch(matchId),
    {
      enabled: matchId > 0, // only fetch if matchId is valid
      staleTime: 1000 * 30, // example: 30s
    }
  );

  // 2) Mutation: send a message to this match
  const sendMessageMutation = useMutation(
    (data: MessageRequest) => messageService.sendMessage(data),
    {
      onSuccess: () => {
        // re-fetch messages on success
        queryClient.invalidateQueries(['messages', matchId]);
      },
    }
  );

  return {
    // Query states
    messages: messagesQuery.data,
    isLoadingMessages: messagesQuery.isLoading,
    isErrorMessages: messagesQuery.isError,
    refetchMessages: messagesQuery.refetch,

    // Mutation states
    sendMessage: sendMessageMutation.mutate,
    isSending: sendMessageMutation.isLoading,
  };
};
