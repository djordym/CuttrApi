// src/features/main/hooks/usePlantHooks.ts
import { useQuery } from "react-query";
import { plantService } from "../../../api/plantService";
import { PlantResponse } from "../../../types/apiTypes";
import { useSelector } from "react-redux";
import { RootState } from "../../../store";
import { useMutation, useQueryClient } from "react-query";

export const useUserPlants = (userId: number) => {
  return useQuery(
    ["userPlants", userId],
    () => plantService.getUserPlants(userId),
    {
      enabled: !!userId, // only fetch if userId is available
      staleTime: 1000 * 60 * 5, // 5 minutes
    }
  );
};

export const useMyPlants = () => {
  const queryClient = useQueryClient();
  const { userId } = useSelector((state: RootState) => state.auth);
  const query = useQuery<PlantResponse[], Error>(
    ["myPlants", userId],
    () => {
      if (!userId) throw new Error("User not logged in");
      return plantService.getMyPlants();
    },
    {
      enabled: !!userId,
      staleTime: 1000 * 60 * 5,
    }
  );

  const mutation = useMutation(
    (plantId: number) => plantService.deleteMyPlant(plantId),
    {
      onSettled: () => queryClient.invalidateQueries(["myPlants", userId]),
    }
  );

  return {
    ...query,
    deletePlant: mutation.mutate,
    isDeleting: mutation.isLoading,
  };
};

export const usePlantsLikedByMeFromUser = (otherUserId: number) => {
  return useQuery<PlantResponse[], Error>(
    ["plantsLikedByMeFromUser", otherUserId],
    () => plantService.getPlantsLikedByMeFromUser(otherUserId),
    {
      enabled: !!otherUserId,
    }
  );
};

export const usePlantsLikedByUserFromMe = (otherUserId: number) => {
  return useQuery<PlantResponse[], Error>(
    ["plantsLikedByUserFromMe", otherUserId],
    () => plantService.getPlantsLikedByUserFromMe(otherUserId),
    {
      enabled: !!otherUserId,
    }
  );
};
