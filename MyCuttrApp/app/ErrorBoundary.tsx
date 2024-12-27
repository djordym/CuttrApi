// File: app/ErrorBoundary.tsx
import React from 'react';
import { View, Text } from 'react-native';
import { log } from './utils/logger';

interface ErrorBoundaryProps {
  children: React.ReactNode;
}

export class ErrorBoundary extends React.Component<ErrorBoundaryProps> {
  state = { hasError: false };

  static getDerivedStateFromError(error: Error) {
    return { hasError: true };
  }

  componentDidCatch(error: Error, info: any) {
    log.error("Uncaught error in component tree", { error, info });
    // If you use Sentry:
    // Sentry.captureException(error);
  }

  render() {
    if (this.state.hasError) {
      // Render fallback UI
      return (
        <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
          <Text>An unexpected error occurred.</Text>
        </View>
      );
    }
    return this.props.children;
  }
}
