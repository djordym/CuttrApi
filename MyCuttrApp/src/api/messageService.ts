import api from './axiosConfig';
import { MessageRequest, MessageResponse } from '../types/apiTypes';

export const messageService = {
  sendMessage: async (data: MessageRequest): Promise<MessageResponse> => {
    const response = await api.post<MessageResponse>('/messages/me', data);
    return response.data;
  },
  getMessagesForMatch: async (matchId: number): Promise<MessageResponse[]> => {
    const response = await api.get<MessageResponse[]>(`/matches/${matchId}/messages`);
    return response.data;
  }
};
