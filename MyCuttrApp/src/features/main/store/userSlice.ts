import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { RootState } from '../../../store';
import api from '../../../api/axiosConfig';
import { UserResponse } from '../../../types/apiTypes';

export const fetchUserProfile = createAsyncThunk<
  UserResponse,
  number,        // userId as argument
  { rejectValue: string }
>(
  'user/fetchUserProfile',
  async (userId, { rejectWithValue }) => {
    try {
      const response = await api.get<UserResponse>(`/users/${userId}`);
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch user profile');
    }
  }
);

interface UserState {
  profile: UserResponse | null;
  status: 'idle' | 'loading' | 'error';
  error: string | null;
}

const initialState: UserState = {
  profile: null,
  status: 'idle',
  error: null
};

export const userSlice = createSlice({
  name: 'user',
  initialState,
  reducers: {
    clearUserProfile(state) {
      state.profile = null;
      state.status = 'idle';
      state.error = null;
    }
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchUserProfile.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(fetchUserProfile.fulfilled, (state, action: PayloadAction<UserResponse>) => {
        state.status = 'idle';
        state.profile = action.payload;
      })
      .addCase(fetchUserProfile.rejected, (state, action) => {
        state.status = 'error';
        state.error = action.payload as string;
      });
  }
});

export const { clearUserProfile } = userSlice.actions;
