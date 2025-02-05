// File: app/features/main/hooks/useConnections.ts

import { useQuery } from 'react-query';
import { connectionService } from '../../../api/connectionService';
import { ConnectionResponse } from '../../../types/apiTypes';

export function useConnections() {
  return useQuery<ConnectionResponse[], Error>('myConnections', () =>
    connectionService.getMyConnections()
  );
}

export function useConnection(connectionId: number) {
  return useQuery<ConnectionResponse, Error>(['connection', connectionId], () =>
    connectionService.getConnectionById(connectionId)
  );
}
