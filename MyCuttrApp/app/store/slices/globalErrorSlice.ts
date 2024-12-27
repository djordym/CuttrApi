import { createSlice, PayloadAction } from '@reduxjs/toolkit';

interface GlobalErrorState {
  message: string | null;
}

const initialState: GlobalErrorState = {
  message: null,
};

export const globalErrorSlice = createSlice({
  name: 'globalError',
  initialState,
  reducers: {
    setGlobalError(state, action: PayloadAction<string>) {
      state.message = action.payload;
    },
    clearGlobalError(state) {
      state.message = null;
    },
  },
});

export const { setGlobalError, clearGlobalError } = globalErrorSlice.actions;
