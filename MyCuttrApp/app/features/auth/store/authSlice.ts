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
      await storage.saveTokens(data.Tokens.AccessToken, data.Tokens.RefreshToken);
      return data;
    } catch (error: any) {
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
      await storage.saveTokens(data.Tokens.AccessToken, data.Tokens.RefreshToken);
      return data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Registration failed');
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
    const payload: RefreshTokenRequest = { RefreshToken: refreshToken };
    try {
      const data = await authService.refreshToken(payload);
      await storage.saveTokens(data.AccessToken, data.RefreshToken);
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
        state.accessToken = action.payload.Tokens.AccessToken;
        state.refreshToken = action.payload.Tokens.RefreshToken;
        state.userId = action.payload.UserId;
        state.email = action.payload.Email;
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
        state.accessToken = action.payload.Tokens.AccessToken;
        state.refreshToken = action.payload.Tokens.RefreshToken;
        state.userId = action.payload.UserId;
        state.email = action.payload.Email;
      })
      .addCase(registerThunk.rejected, (state, action) => {
        state.status = 'error';
        state.error = action.payload || 'Unknown error';
      })

      // refreshTokenThunk
      .addCase(refreshTokenThunk.fulfilled, (state, action) => {
        // action.payload: AuthTokenResponse
        state.accessToken = action.payload.AccessToken;
        state.refreshToken = action.payload.RefreshToken;
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
