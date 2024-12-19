import api from './axiosConfig';
import { UserResponse, UserUpdateRequest, UserProfileImageUpdateRequest, UpdateLocationRequest } from '../types/apiTypes';

export const userService = {
  register: async (data: { Email: string; Password: string; Name: string }): Promise<UserResponse> => {
    const response = await api.post<UserResponse>('/users/register', data);
    return response.data;
  },
  getUser: async (userId: number): Promise<UserResponse> => {
    const response = await api.get<UserResponse>(`/users/${userId}`);
    return response.data;
  },
  updateMe: async (data: UserUpdateRequest): Promise<UserResponse> => {
    const response = await api.put<UserResponse>('/users/me', data);
    return response.data;
  },
  deleteMe: async (): Promise<void> => {
    await api.delete('/users/me');
  },
  updateProfilePicture: async (data: UserProfileImageUpdateRequest): Promise<UserResponse> => {
    const formData = new FormData();
    formData.append('Image', data.Image);
    const response = await api.put<UserResponse>('/users/me/profile-picture', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    return response.data;
  },
  updateLocation: async (data: UpdateLocationRequest): Promise<void> => {
    await api.put('/users/me/location', data);
  }
};
