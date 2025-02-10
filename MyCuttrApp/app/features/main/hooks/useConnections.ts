// File: app/features/main/hooks/useConnections.ts

import { useQuery } from 'react-query';
import { connectionService } from '../../../api/connectionService';
import { ConnectionResponse } from '../../../types/apiTypes';

export const useConnections = () =>{
  return useQuery<ConnectionResponse[], Error>('myConnections', () =>
    connectionService.getMyConnections()
  );
}

export const useConnection = (connectionId: number) => {
  return useQuery<ConnectionResponse, Error>(['connection', connectionId], () =>
    connectionService.getConnectionById(connectionId)
  );
}

export default useConnections;