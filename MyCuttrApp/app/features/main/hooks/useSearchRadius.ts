// File: app/features/main/hooks/useSearchRadius.ts
import { useUserPreferences } from './usePreferences';

export const useSearchRadius = () => {
  const { data: preferences, isLoading, isError } = useUserPreferences();

  return {
    searchRadius: preferences?.searchRadius ?? 0, // default to 0 if undefined
    isLoading,
    isError,
  };
};
