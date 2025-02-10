import * as SecureStore from "expo-secure-store";

const ACCESS_TOKEN_KEY = "ACCESS_TOKEN_KEY";
const REFRESH_TOKEN_KEY = "REFRESH_TOKEN_KEY";
const LANGUAGE_KEY = "LANGUAGE_KEY";

export const storage = {
  saveTokens: async (accessToken: string, refreshToken: string) => {
    await SecureStore.setItemAsync(ACCESS_TOKEN_KEY, accessToken);
    await SecureStore.setItemAsync(REFRESH_TOKEN_KEY, refreshToken);
  },
  getAccessToken: async () => SecureStore.getItemAsync(ACCESS_TOKEN_KEY),
  getRefreshToken: async () => SecureStore.getItemAsync(REFRESH_TOKEN_KEY),
  clearTokens: async () => {
    await SecureStore.deleteItemAsync(ACCESS_TOKEN_KEY);
    await SecureStore.deleteItemAsync(REFRESH_TOKEN_KEY);
  },
  // Language Handling
  saveLanguage: async (language: string) => {
    await SecureStore.setItemAsync(LANGUAGE_KEY, language);
  },
  getLanguage: async () => SecureStore.getItemAsync(LANGUAGE_KEY),
};

export default storage;