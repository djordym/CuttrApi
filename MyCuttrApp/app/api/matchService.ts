import api from './axiosConfig';
import { MatchResponse } from '../types/apiTypes';

export const matchService = {
  getMyMatchesForConnectionId: async (connectionId: number): Promise<MatchResponse[]> => {
    const response = await api.get<MatchResponse[]>(`/matches/connection/${connectionId}`);
    return response.data;
  },
};

export default matchService;