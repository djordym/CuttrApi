import { configureStore } from '@reduxjs/toolkit';
import { authSlice } from '../features/auth/store/authSlice';
import { userSlice } from '../features/main/store/userSlice';
import { userPreferencesSlice } from '../features/main/store/userPreferencesSlice';
import { Provider } from 'react-redux';

export const store = configureStore({
  reducer: {
    auth: authSlice.reducer,
    user: userSlice.reducer,
    userPreferences: userPreferencesSlice.reducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
