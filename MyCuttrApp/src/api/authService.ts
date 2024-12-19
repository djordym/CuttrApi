import api from './axiosConfig';
import { 
  UserLoginRequest, 
  UserLoginResponse, 
  RefreshTokenRequest, 
  AuthTokenResponse,
  UserRegistrationRequest
} from '../types/apiTypes';

export const authService = {
  login: async (data: UserLoginRequest): Promise<UserLoginResponse> => {
    const response = await api.post<UserLoginResponse>('/auth/login', data);
    return response.data;
  },
  refreshToken: async (data: RefreshTokenRequest): Promise<AuthTokenResponse> => {
    const response = await api.post<AuthTokenResponse>('/auth/refresh', data);
    return response.data;
  },
  register: async (data: UserRegistrationRequest): Promise<UserLoginResponse> => {
    const response = await api.post<UserLoginResponse>('/users/register', data);
    return response.data;
  },
  logout: async (): Promise<void> => {
    await api.post('/api/auth/logout');
  },
};
