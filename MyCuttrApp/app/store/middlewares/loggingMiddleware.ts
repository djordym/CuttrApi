// File: app/store/middlewares/loggingMiddleware.ts
import { log } from '../../utils/logger';
import { Middleware } from '@reduxjs/toolkit';

export const loggingMiddleware: Middleware = (storeAPI) => (next) => (action: any) => {
  if (__DEV__) {
    log.debug("Redux Action Dispatched", { type: action.type, payload: action.payload });
  }
  const result = next(action);
  return result;
};

export default loggingMiddleware;