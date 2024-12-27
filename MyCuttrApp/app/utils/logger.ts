// File: app/utils/logger.ts
import createLogger from 'react-native-logs';
import { logger } from 'react-native-logs';

const defaultConfig = {
  severity: __DEV__ ? 'debug' : 'warn',  
//   transportOptions: {
//     // You can configure a custom transport here, e.g., sending logs to your backend or Sentry
//   },
//   // You can override formatting, date/time formatting, etc.
};

export const log = logger.createLogger(defaultConfig);
