import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { authService } from '../../../api/authService';
import { storage } from '../../../utils/storage';
import { RootState } from '../../../store';
import { 
  UserLoginRequest, 
  UserLoginResponse, 
  UserRegistrationRequest, 
  RefreshTokenRequest, 
  AuthTokenResponse
} from '../../../types/apiTypes';

import { log } from '../../../utils/logger';

interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  userId: number | null;
  email: string | null;
  status: 'idle' | 'loading' | 'error';
  error: string | null;
}

const initialState: AuthState = {
  accessToken: null,
  refreshToken: null,
  userId: null,
  email: null,
  status: 'idle',
  error: null
};

export const loginThunk = createAsyncThunk<
  UserLoginResponse,    // On success, returns UserLoginResponse
  UserLoginRequest,     // The argument type is UserLoginRequest
  { rejectValue: string }
>(
  'auth/login',
  async (credentials, { rejectWithValue }) => {
    try {
      const data = await authService.login(credentials);
      // Store tokens securely
      log.debug('loginThunk data:', data);
      await storage.saveTokens(data.tokens.accessToken, data.tokens.refreshToken);
      return data;
    } catch (error: any) {
      // 1) Log everything about the error before rejecting
      log.error("Login error details:", error);

      return rejectWithValue(error.response?.data?.message || 'Login failed');
    }
  }
);

export const registerThunk = createAsyncThunk<
  UserLoginResponse,
  UserRegistrationRequest,
  { rejectValue: string }
>(
  'auth/register',
  async (payload, { rejectWithValue }) => {
    try {
      const data = await authService.register(payload);
      await storage.saveTokens(data.tokens.accessToken, data.tokens.refreshToken);
      return data;
    } catch (error: any) {
      // 1) Log everything about the error before rejecting
      log.error("Registration error details:", error);

      // 2) If we have an error.response, log its data
      if (error.response) {
        log.error("Registration error response data:", error.response.data);
        log.error("Registration error response status:", error.response.status);
        log.error("Registration error response headers:", error.response.headers);
      } else {
        log.error("Registration error: no response object", error.message);
      }
      return rejectWithValue(error.response?.data || 'Registration failed');
    }
  }
);

export const refreshTokenThunk = createAsyncThunk<
  AuthTokenResponse,
  void,
  { state: RootState; rejectValue: string }
>(
  'auth/refreshToken',
  async (_, { getState, rejectWithValue }) => {
    const state = getState();
    const refreshToken = state.auth.refreshToken;
    if (!refreshToken) {
      return rejectWithValue('No refresh token available');
    }
    const payload: RefreshTokenRequest = { refreshToken: refreshToken };
    try {
      const data = await authService.refreshToken(payload);
      await storage.saveTokens(data.accessToken, data.refreshToken);
      return data;
    } catch (error: any) {
      return rejectWithValue('Token refresh failed');
    }
  }
);

export const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setInitialTokens(
      state,
      action: PayloadAction<{ accessToken: string | null; refreshToken: string | null; userId: number | null; email: string | null }>
    ) {
      state.accessToken = action.payload.accessToken;
      state.refreshToken = action.payload.refreshToken;
      state.userId = action.payload.userId;
      state.email = action.payload.email;
    },
    logout(state) {
      state.accessToken = null;
      state.refreshToken = null;
      state.userId = null;
      state.email = null;
      state.error = null;
      state.status = 'idle';
      storage.clearTokens();
    }
  },
  extraReducers: (builder) => {
    builder
      // loginThunk
      .addCase(loginThunk.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(loginThunk.fulfilled, (state, action) => {
        state.status = 'idle';
        // action.payload: UserLoginResponse
        state.accessToken = action.payload.tokens.accessToken;
        state.refreshToken = action.payload.tokens.refreshToken;
        state.userId = action.payload.userId;
        state.email = action.payload.email;
      })
      .addCase(loginThunk.rejected, (state, action) => {
        state.status = 'error';
        state.error = action.payload || 'Unknown error';
      })

      // registerThunk
      .addCase(registerThunk.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(registerThunk.fulfilled, (state, action) => {
        state.status = 'idle';
        // action.payload: UserLoginResponse
        state.accessToken = action.payload.tokens.accessToken;
        state.refreshToken = action.payload.tokens.refreshToken;
        state.userId = action.payload.userId;
        state.email = action.payload.email;
      })
      .addCase(registerThunk.rejected, (state, action) => {
        state.status = 'error';
        state.error = action.payload || 'Unknown error';
      })

      // refreshTokenThunk
      .addCase(refreshTokenThunk.fulfilled, (state, action) => {
        // action.payload: AuthTokenResponse
        state.accessToken = action.payload.accessToken;
        state.refreshToken = action.payload.refreshToken;
      })
      .addCase(refreshTokenThunk.rejected, (state) => {
        // If refresh fails, clear credentials
        state.accessToken = null;
        state.refreshToken = null;
        state.userId = null;
        state.email = null;
      });
  }
});

export const { logout, setInitialTokens } = authSlice.actions;
