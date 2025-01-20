// File: app/features/main/hooks/useUserMatches.ts
import { useQuery } from 'react-query';
import { matchService } from '../../../api/matchService'; // Or wherever your service lives
import { MatchResponse } from '../../../types/apiTypes';

export const useUserMatches = () => {
  return useQuery<MatchResponse[]>(
    'userMatches',
    () => matchService.getMyMatches(),
    {
      // You can configure staleTime, refetchOnWindowFocus, etc. here
      staleTime: 1000 * 60, // 1 minute
    }
  );
};
