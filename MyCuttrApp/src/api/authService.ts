import api from './axiosConfig';

interface LoginRequest {
  email: string;
  password: string;
}

interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  user: {
    userId: number;
    email: string;
    name: string;
    profilePictureUrl?: string;
  };
}

export const authService = {
  login: async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await api.post('/auth/login', data);
    return response.data;
  },
  refreshToken: async (refreshToken: string): Promise<{ accessToken: string; refreshToken: string; }> => {
    const response = await api.post('/auth/refresh', { refreshToken });
    return response.data;
  },
  register: async (data: { email: string; password: string; name: string }): Promise<LoginResponse> => {
    const response = await api.post('/users/register', data);
    return response.data;
  }
};
