// src/features/main/hooks/useUserPreferences.ts
import { useQuery, useMutation, useQueryClient } from 'react-query';
import { userPreferencesService } from '../../../api/userPreferencesService';
import { UserPreferencesResponse, UserPreferencesRequest } from '../../../types/apiTypes';

export const useUserPreferences = () => {
  const queryClient = useQueryClient();

  const query = useQuery<UserPreferencesResponse, Error>(
    ['userPreferences'],
    userPreferencesService.getPreferences,
    {
      staleTime: 1000 * 60 * 5
    }
  );

  const mutation = useMutation(
    (data: UserPreferencesRequest) => userPreferencesService.updatePreferences(data),
    {
      onSuccess: () => {
        queryClient.invalidateQueries(['userPreferences']);
      }
    }
  );

  return {
    ...query,
    updatePreferences: mutation.mutate,
    isUpdating: mutation.isLoading
  };
};
