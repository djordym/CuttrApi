// src/features/main/hooks/useMatches.ts
import { useQuery } from 'react-query';
import { matchService } from '../../../api/matchService';

export const useMyMatches = () => {
  return useQuery('myMatches', matchService.getMyMatches, {
    staleTime: 0,
    refetchOnWindowFocus: true,
  });
};
