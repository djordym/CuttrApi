import { useQuery } from 'react-query';
import { swipeService } from '../../../api/swipeService';
import { PlantResponse, SwipeRequest } from '../../../types/apiTypes';
import { useMutation, useQueryClient } from 'react-query';
export const useLikablePlants = () => {
  const queryClient = useQueryClient();

  const query = useQuery<PlantResponse[], Error>(
    ['likablePlants'],
    swipeService.getLikablePlants,
    {
      staleTime: 1000 * 60 * 1, // 1 min
      refetchOnWindowFocus: true,
      retry: 1,
    }
  );

  const mutation = useMutation( 
    (data: SwipeRequest[]) => swipeService.sendSwipes(data),
    {
      onSuccess: () => {
        queryClient.invalidateQueries(['userMatches']);
      }
    }
  );

  return {
    ...query,
    sendSwipes: mutation.mutate,
    isSending: mutation.isLoading
  };
};


