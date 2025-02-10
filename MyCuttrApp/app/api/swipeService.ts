import api from './axiosConfig';
import { SwipeRequest, SwipeResponse, PlantResponse } from '../types/apiTypes';

export const swipeService = {
  sendSwipes: async (swipes: SwipeRequest[]): Promise<SwipeResponse[]> => {
    const response = await api.post<SwipeResponse[]>('/swipes/me', swipes);
    return response.data;
  },
  
};

export default swipeService;