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
import { COLORS } from './theme/colors';
import * as Sentry from '@sentry/react-native';
import { isRunningInExpoGo } from 'expo';

// Construct a new integration instance. This is needed to communicate between the integration and React
const navigationIntegration = Sentry.reactNavigationIntegration({
  enableTimeToInitialDisplay: !isRunningInExpoGo(),
});

Sentry.init({
  dsn: 'https://257bb6061b5f62855c8c40b523c87628@o4508793864978432.ingest.de.sentry.io/4508793930317904',
  debug: true,
});


const queryClient = new QueryClient();

export function App() {
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

export default Sentry.wrap(App);