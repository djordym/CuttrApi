// src/features/main/hooks/usePlants.ts
import { useQuery } from 'react-query';
import { plantService } from '../../../api/plantService';
import { PlantResponse } from '../../../types/apiTypes';
import { useSelector } from 'react-redux';
import { RootState } from '../../../store';

export const useUserPlants = (userId: number) => {
  return useQuery(['userPlants', userId], () => plantService.getUserPlants(userId), {
    enabled: !!userId, // only fetch if userId is available
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
};

export const useMyPlants = () => {
    const { userId } = useSelector((state: RootState) => state.auth);
    return useQuery<PlantResponse[], Error>(
      ['myPlants', userId],
      () => {
        if (!userId) throw new Error('User not logged in');
        return plantService.getMyPlants();
      },
      {
        enabled: !!userId,
        staleTime: 1000 * 60 * 5
      }
    );
  };