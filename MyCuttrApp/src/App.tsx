import React, { useEffect, useState } from 'react';
import { Provider } from 'react-redux';
import { store } from './store';
import AppNavigator from './navigation/AppNavigator';
import { QueryClient, QueryClientProvider } from 'react-query';
import { initI18n } from './i18n';
import { ActivityIndicator, View } from 'react-native';
import { I18nextProvider } from 'react-i18next';
import { GestureHandlerRootView } from 'react-native-gesture-handler';

const queryClient = new QueryClient();

const App = () => {
  const [i18nInstance, setI18nInstance] = useState<any>(null);

  useEffect(() => {
    const setupI18n = async () => {
      const i18n = await initI18n();
      setI18nInstance(i18n);
    };
    setupI18n();
  }, []);

  if (!i18nInstance) {
    return (
      <View style={{flex:1,justifyContent:'center',alignItems:'center'}}>
        <ActivityIndicator size="large" />
      </View>
    );
  }

  return (
    <Provider store={store}>
      <QueryClientProvider client={queryClient}>
        <I18nextProvider i18n={i18nInstance}>
          <GestureHandlerRootView>
            <AppNavigator />
          </GestureHandlerRootView>
        </I18nextProvider>
      </QueryClientProvider>
    </Provider>
  );
};

export default App;
