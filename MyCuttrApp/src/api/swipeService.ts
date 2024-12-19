import api from './axiosConfig';
import { SwipeRequest, SwipeResponse, PlantResponse } from '../types/apiTypes';

export const swipeService = {
  sendSwipes: async (swipes: SwipeRequest[]): Promise<SwipeResponse> => {
    const response = await api.post<SwipeResponse>('/swipes/me', swipes);
    return response.data;
  },
  getLikablePlants: async (): Promise<PlantResponse[]> => {
    const response = await api.get<PlantResponse[]>('/swipes/me/likable-plants');
    return response.data;
  }
};
