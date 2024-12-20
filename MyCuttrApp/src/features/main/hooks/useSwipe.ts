import { useQuery } from 'react-query';
import { swipeService } from '../../../api/swipeService';
import { PlantResponse } from '../../../types/apiTypes';

export const useLikablePlants = () => {
  const query = useQuery<PlantResponse[], Error>(
    ['likablePlants'],
    swipeService.getLikablePlants,
    {
      staleTime: 1000 * 60 * 1, // 1 min
      refetchOnWindowFocus: false,
      retry: 1,
    }
  );
  return query;
};
