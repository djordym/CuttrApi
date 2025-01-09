import axios from "axios";
import { store } from "../store"; // your Redux store
import { refreshTokenThunk, logout } from "../features/auth/store/authSlice";
import { setGlobalError } from "../store/slices/globalErrorSlice"; // hypothetical slice
import { RootState } from "../store"; // your root state type
import { AuthTokenResponse } from "../types/apiTypes";
import {log} from '../utils/logger';

let isRefreshing = false;
let pendingRequests: Array<(token: string) => void> = [];

const api = axios.create({
  baseURL: "http://192.168.137.1:5020/api",
  timeout: 10000,
});

// ────────────────────────────────────────────────────────────────────────────────
// REQUEST INTERCEPTOR
// ────────────────────────────────────────────────────────────────────────────────
api.interceptors.request.use(
  async (config) => {
    const state: RootState = store.getState();
    const token = state.auth.accessToken; // adapt to your actual auth slice
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    
    log.debug("API Request", {
      baseUrl : api.defaults.baseURL,
      url: config.url,
      method: config.method,
      data: config.data,
      headers: config.headers,
    });
    return config;
  },
  (error) => {
    log.error("API Request Error", error);
    return Promise.reject(error);
  }
);

// ────────────────────────────────────────────────────────────────────────────────
// RESPONSE INTERCEPTOR
// ────────────────────────────────────────────────────────────────────────────────
api.interceptors.response.use(
  (response) => {
    log.debug("API Response", {
      url: response.config.url,
      status: response.status,
      data: response.data,
      header: response.headers,
    });
    // If the response is successful, just return it.
    return response;
  },

  async (error) => {
    log.error("API Response Error", {
      url: error.config.url,
      message: error.message,
      status: error.response.status,
      data: error.response.data,
      headers: error.response.headers,
    });

    // If we don't have an actual HTTP response (network error, CORS issue, etc.)
    if (!error.response) {
      store.dispatch(setGlobalError("Network Error: Unable to connect."));
      return Promise.reject(error);
    }

    

    const { status } = error.response;
    const originalRequest = error.config;

    // ──────────────────────────────────────────────────────────────────────
    // 1. Handle 401 (Unauthorized) for token refresh
    // ──────────────────────────────────────────────────────────────────────
    if (status === 401 && !originalRequest._retry) {
      if (!isRefreshing) {
        isRefreshing = true;
        originalRequest._retry = true;
        try {
          // Attempt to refresh
          const result = await store.dispatch(refreshTokenThunk());
          //initialise authtokenresponse
          let newTokens: AuthTokenResponse | undefined;

          if (refreshTokenThunk.fulfilled.match(result)) {
            // The action was fulfilled, and we can safely access the payload
            newTokens = result.payload;
            console.log("New tokens:", newTokens);
          } else {
            // The action was rejected, handle the error
            console.error(
              "Token refresh failed:",
              result.payload || result.error.message
            );
          }

          // Reset refresh state
          isRefreshing = false;

          pendingRequests.forEach((cb) => cb(newTokens?.accessToken || ""));
          pendingRequests = [];

          // Retry the original request with the new token
          return api(originalRequest);
        } catch (refreshError) {
          // Refresh token failed, forcibly logout
          isRefreshing = false;
          pendingRequests = [];
          store.dispatch(logout()); // This should remove tokens & navigate to login
          return Promise.reject(refreshError);
        }
      }

      // We are already refreshing; queue requests
      return new Promise((resolve) => {
        pendingRequests.push((token: string) => {
          originalRequest.headers.Authorization = `Bearer ${token}`;
          resolve(api(originalRequest));
        });
      });
    }

    // ──────────────────────────────────────────────────────────────────────
    // 2. Other errors (e.g., 404, 403, 500, etc.)
    // ──────────────────────────────────────────────────────────────────────

    // Extract a user-friendly message
    let errorMessage: string;

    if (typeof error.response.data === "string") {
      // If your backend returns a plain text error, use it directly.
      errorMessage = error.response.data;
    } else if (error.response.data?.message) {
      // Alternatively, if your backend returns { message: 'something' }
      errorMessage = error.response.data.message;
    } else {
      // Fallback if the response format is unknown
      errorMessage = "An error occurred. Please try again later.";
    }

    // Dispatch to a global error state (e.g., displayed in a toast or modal)
    store.dispatch(setGlobalError(errorMessage));

    return Promise.reject(error);
  }
);

export default api;
