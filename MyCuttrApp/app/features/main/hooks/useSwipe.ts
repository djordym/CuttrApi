// useLikablePlants.ts
import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from 'react-query';
import { swipeService } from '../../../api/swipeService';
import { plantService } from '../../../api/plantService';
import {
  PlantResponse,
  SwipeRequest,
  SwipeResponse,
} from '../../../types/apiTypes';

export const useLikablePlants = () => {
  const queryClient = useQueryClient();
  const [matches, setMatches] = useState<SwipeResponse[]>([]);

  const query = useQuery<PlantResponse[], Error>(
    ['likablePlants'],
    plantService.getLikablePlants,
    {
      staleTime: 1000 * 60, // 1 minute
      refetchOnWindowFocus: true,
      retry: 1,
    }
  );

  const mutation = useMutation(
    (data: SwipeRequest[]) => swipeService.sendSwipes(data),
    {
      onSuccess: (swipeResponses: SwipeResponse[]) => {
        // Filter and store only the responses that indicate a match.
        const newMatches = swipeResponses.filter((resp) => resp.isMatch);
        if (newMatches.length > 0) {
          setMatches((prev) => [...prev, ...newMatches]);
          queryClient.invalidateQueries(['myConnections']);
        }
      },
    }
  );

  const clearMatches = () => setMatches([]);

  return {
    ...query,
    sendSwipes: mutation.mutate,
    isSending: mutation.isLoading,
    matches,
    clearMatches,
  };
};


export default useLikablePlants;