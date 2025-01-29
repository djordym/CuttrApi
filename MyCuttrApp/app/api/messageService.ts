import api from './axiosConfig';
import { MessageRequest, MessageResponse } from '../types/apiTypes';

export const messageService = {
  getMessagesForConnection: async (connectionId: number): Promise<MessageResponse[]> => {
    const response = await api.get<MessageResponse[]>(`/connections/${connectionId}/messages`);
    return response.data;
  },
  
  sendMessage: async (connectionId: number, data: MessageRequest): Promise<MessageResponse> => {
    const response = await api.post<MessageResponse>(`/connections/${connectionId}/messages/user/me`, data);
    return response.data;
  },
};
