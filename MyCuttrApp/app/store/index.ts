import { configureStore } from '@reduxjs/toolkit';
import { authSlice } from '../features/auth/store/authSlice';
import { globalErrorSlice } from './slices/globalErrorSlice';
import { loggingMiddleware } from './middlewares/loggingMiddleware';

export const store = configureStore({
  reducer: {
    auth: authSlice.reducer,
    globalError: globalErrorSlice.reducer,
  },
  middleware: (getDefaultMiddleware) => getDefaultMiddleware().concat(loggingMiddleware),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
