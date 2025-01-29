// hooks/useProfileHooks.ts
import { useQuery, useMutation, useQueryClient } from 'react-query';
import { userService } from '../../../api/userService';
import { UserResponse, UserUpdateRequest, UpdateLocationRequest, UserProfileImageUpdateRequest } from '../../../types/apiTypes';

// Fetch User Profile
export const useMyProfile = () => {
  return useQuery<UserResponse, Error>(
    ['userProfile'],
    userService.getCurrentUserProfile,
    {
      staleTime: 1000 * 60 * 5, // 5 minutes
      cacheTime: 1000 * 60 * 10, // 10 minutes
      retry: 1, // Retry once on failure
    }
  );
};

// Update User Profile (Name, Bio, etc.)
export const useUpdateProfile = () => {
  const queryClient = useQueryClient();

  return useMutation(
    (payload: UserUpdateRequest) => userService.updateMe(payload),
    {
      onSuccess: () => {
        // Invalidate and refetch
        queryClient.invalidateQueries(['userProfile']);
      },
      onError: (error: any) => {
        console.error('Error updating profile:', error);
      },
    }
  );
};

// Update Profile Picture
export const useUpdateProfilePicture = () => {
  const queryClient = useQueryClient();

  return useMutation(
    (photo: { uri: string; name: string; type: string }) => {
      const image: UserProfileImageUpdateRequest = { image: new File([photo.uri], photo.name, { type: photo.type }) };
      return userService.updateProfilePicture(image);
    },
    {
      onSuccess: () => {
        // Invalidate and refetch
        queryClient.invalidateQueries(['userProfile']);
      },
      onError: (error: any) => {
        console.error('Error updating profile picture:', error);
      },
    }
  );
};

// **New Hook: Update User Location**
export const useUpdateLocation = () => {
  const queryClient = useQueryClient();

  return useMutation(
    (payload: UpdateLocationRequest) => userService.updateLocation(payload),
    {
      onSuccess: () => {
        // Invalidate and refetch
        queryClient.invalidateQueries(['userProfile']);
      },
      onError: (error: any) => {
        console.error('Error updating location:', error);
      },
    }
  );
};
