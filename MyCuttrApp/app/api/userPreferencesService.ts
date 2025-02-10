import api from './axiosConfig';
import { UserPreferencesResponse, UserPreferencesRequest } from '../types/apiTypes';

export const userPreferencesService = {
  getPreferences: async (): Promise<UserPreferencesResponse> => {
    const response = await api.get<UserPreferencesResponse>('/userpreferences');
    return response.data;
  },
  updatePreferences: async (data: UserPreferencesRequest): Promise<UserPreferencesResponse> => {
    const response = await api.post<UserPreferencesResponse>('/userpreferences', data);
    return response.data;
  }
};

export default userPreferencesService;