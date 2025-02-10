import React, { useEffect, useState } from 'react';
import { ActivityIndicator, View } from 'react-native';
import { Provider } from 'react-redux';
import { QueryClient, QueryClientProvider } from 'react-query';
import { I18nextProvider } from 'react-i18next';
import { GestureHandlerRootView } from 'react-native-gesture-handler';
import ErrorBoundary from './ErrorBoundary';
import { store } from './store';
import AppNavigator from './navigation/AppNavigator';
import { initI18n } from './i18n';
import { log } from './utils/logger';
import { StatusBar } from 'expo-status-bar';
import { SafeAreaProvider, SafeAreaView } from 'react-native-safe-area-context';
import { COLORS } from './theme/colors';

const queryClient = new QueryClient();

export default function App() {
  log.debug('App.tsx rendering...');

  const [i18nInstance, setI18nInstance] = useState<any>(null);

  useEffect(() => {
    const setupI18n = async () => {
      const i18n = await initI18n();   // now uses expo-localization under the hood
      setI18nInstance(i18n);
    };
    setupI18n();
  }, []);

  if (!i18nInstance) {
    // Still initializing i18n, show a loading indicator
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
        <ActivityIndicator size="large" />
      </View>
    );
  }

  return (
    <ErrorBoundary>
      <Provider store={store}>
        <QueryClientProvider client={queryClient}>
          {/* Provide the i18n instance to the app */}
          <I18nextProvider i18n={i18nInstance}>
            <GestureHandlerRootView>
                <StatusBar
                  style="dark"
                  backgroundColor={COLORS.primary} // allow content to appear behind status bar
                />
                <AppNavigator />
            </GestureHandlerRootView>
          </I18nextProvider>
        </QueryClientProvider>
      </Provider>
    </ErrorBoundary>
  );
}
