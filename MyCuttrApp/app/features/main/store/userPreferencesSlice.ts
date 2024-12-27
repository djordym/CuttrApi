import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { userPreferencesService } from '../../../api/userPreferencesService';
import { UserPreferencesResponse, UserPreferencesRequest } from '../../../types/apiTypes';
import { RootState } from '../../../store';

interface UserPreferencesState {
  data: UserPreferencesResponse | null;
  status: 'idle' | 'loading' | 'error';
  error: string | null;
}

const initialState: UserPreferencesState = {
  data: null,
  status: 'idle',
  error: null,
};

export const fetchUserPreferencesThunk = createAsyncThunk<UserPreferencesResponse, void, { rejectValue: string }>(
  'userPreferences/fetch',
  async (_, { rejectWithValue }) => {
    try {
      const preferences = await userPreferencesService.getPreferences();
      return preferences;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch user preferences');
    }
  }
);

export const updateUserPreferencesThunk = createAsyncThunk<UserPreferencesResponse, UserPreferencesRequest, { rejectValue: string }>(
  'userPreferences/update',
  async (data, { rejectWithValue }) => {
    try {
      const updated = await userPreferencesService.updatePreferences(data);
      return updated;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to update user preferences');
    }
  }
);

export const userPreferencesSlice = createSlice({
  name: 'userPreferences',
  initialState,
  reducers: {
    clearUserPreferences(state) {
      state.data = null;
      state.status = 'idle';
      state.error = null;
    }
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchUserPreferencesThunk.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(fetchUserPreferencesThunk.fulfilled, (state, action: PayloadAction<UserPreferencesResponse>) => {
        state.status = 'idle';
        state.data = action.payload;
      })
      .addCase(fetchUserPreferencesThunk.rejected, (state, action) => {
        state.status = 'error';
        state.error = action.payload as string;
      })
      .addCase(updateUserPreferencesThunk.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(updateUserPreferencesThunk.fulfilled, (state, action: PayloadAction<UserPreferencesResponse>) => {
        state.status = 'idle';
        state.data = action.payload;
      })
      .addCase(updateUserPreferencesThunk.rejected, (state, action) => {
        state.status = 'error';
        state.error = action.payload as string;
      });
  }
});

export const { clearUserPreferences } = userPreferencesSlice.actions;

export const selectUserPreferences = (state: RootState) => state.userPreferences.data;
export const selectUserPreferencesStatus = (state: RootState) => state.userPreferences.status;
export const selectUserPreferencesError = (state: RootState) => state.userPreferences.error;

export default userPreferencesSlice.reducer;
