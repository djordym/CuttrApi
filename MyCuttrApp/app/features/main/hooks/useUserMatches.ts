// File: app/features/main/hooks/useUserMatches.ts
import { useQuery } from 'react-query';
import { matchService } from '../../../api/matchService'; // Or wherever your service lives
import { MatchResponse } from '../../../types/apiTypes';
import { connect } from 'react-redux';

export const useUserMatches = (connectionId: number) => {
  return useQuery<MatchResponse[]>(
    ['connectionMatches', connectionId],
    () => matchService.getMyMatchesForConnectionId(connectionId),
    {
      // You can configure staleTime, refetchOnWindowFocus, etc. here
      staleTime: 1000 * 60, // 1 minute
      //refetch on changing connectionId
      refetchOnWindowFocus: true,
    }
  );
};
