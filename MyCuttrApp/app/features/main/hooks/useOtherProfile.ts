import {useQuery} from 'react-query';
import { userService } from '../../../api/userService';
import { UserResponse, UserUpdateRequest } from '../../../types/apiTypes';
import { useSelector } from 'react-redux';
import { RootState } from '../../../store';

export const useOtherProfile = (userId: number) => {
        // Only run the query if we have a userId
    return useQuery<UserResponse, Error>(
      ['userProfile', userId],
      () => {
        if (!userId) {
          throw new Error('No userId available');
        }
        return userService.getUser(userId);
      },
      {
        enabled: !!userId,
        staleTime: 1000 * 60 * 5 // 5 minutes
      }
    );  
  };


export default useOtherProfile;