// app/i18n/index.ts
import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import * as Localization from 'expo-localization';

import en from './locales/en.json';
import fr from './locales/fr.json';

export async function initI18n() {
  await i18n
    .use(initReactI18next)
    .init({
      fallbackLng: 'en',
      // Use the device locale from expo-localization:
      lng: Localization.getLocales()[0].languageCode, 
      resources: {
        en: { translation: en },
        fr: { translation: fr },
      },
      interpolation: {
        escapeValue: false,
      },
    });
  return i18n;
}
