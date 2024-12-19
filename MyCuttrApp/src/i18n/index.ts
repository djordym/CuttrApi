import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import * as RNLocalize from 'react-native-localize';
import en from './locales/en.json';
import fr from './locales/fr.json';
import { storage } from '../utils/storage';

export const initI18n = async () => {
  let chosenLanguage = await storage.getLanguage();
  if (!chosenLanguage) {
    const locales = RNLocalize.getLocales();
    const deviceLang = locales.length > 0 ? locales[0].languageCode : 'en';
    // Default to English if unsupported
    chosenLanguage = ['en', 'fr'].includes(deviceLang) ? deviceLang : 'en';
    await storage.saveLanguage(chosenLanguage);
  }

  i18n
    .use(initReactI18next)
    .init({
      compatibilityJSON: 'v3',
      lng: chosenLanguage,
      fallbackLng: 'en',
      resources: {
        en: { translation: en },
        fr: { translation: fr },
      },
      interpolation: {
        escapeValue: false
      }
    });

  return i18n;
};
