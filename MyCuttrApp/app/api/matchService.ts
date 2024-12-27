import api from './axiosConfig';
import { MatchResponse } from '../types/apiTypes';

export const matchService = {
  getMyMatches: async (): Promise<MatchResponse[]> => {
    const response = await api.get<MatchResponse[]>('/matches/me');
    return response.data;
  },
  getMatchById: async (matchId: number): Promise<MatchResponse> => {
    const response = await api.get<MatchResponse>(`/matches/${matchId}`);
    return response.data;
  }
};
