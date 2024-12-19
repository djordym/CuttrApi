import axios from 'axios';
import { store } from '../store';
import { refreshTokenThunk } from '../features/auth/store/authSlice';

const api = axios.create({
  baseURL: 'YOUR_BASE_URL',
  timeout: 10000,
});

api.interceptors.request.use(
  async (config) => {
    const state = store.getState();
    const token = state.auth.accessToken;
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

let isRefreshing = false;
let pendingRequests: Array<(token: string) => void> = [];

api.interceptors.response.use(
  response => response,
  async (error) => {
    const originalRequest = error.config;
    if (error.response?.status === 401 && !originalRequest._retry) {
      // Attempt token refresh
      if (!isRefreshing) {
        isRefreshing = true;
        originalRequest._retry = true;
        try {
          const { payload: newTokens } = await store.dispatch(refreshTokenThunk());
          isRefreshing = false;
          pendingRequests.forEach(cb => cb(newTokens?.accessToken || ''));
          pendingRequests = [];
          return api(originalRequest);
        } catch (refreshError) {
          isRefreshing = false;
          pendingRequests = [];
          // If refresh fails, logout
          // Dispatch a logout action if needed
          return Promise.reject(refreshError);
        }
      }

      // If a refresh is already in progress, queue the request
      return new Promise((resolve) => {
        pendingRequests.push((token: string) => {
          originalRequest.headers.Authorization = 'Bearer ' + token;
          resolve(api(originalRequest));
        });
      });
    }
    return Promise.reject(error);
  }
);

export default api;
