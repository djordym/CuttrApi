import { useMutation, useQueryClient } from 'react-query';
import { connectionService } from '../../../api/connectionService';
import { TradeProposalRequest, TradeProposalResponse } from '../../../types/apiTypes';

export const useCreateTradeProposal = (connectionId: number) => {
  const queryClient = useQueryClient();

  return useMutation<TradeProposalResponse, Error, TradeProposalRequest>(
    (payload: TradeProposalRequest) => connectionService.createTradeProposal(connectionId, payload),
    {
      onSuccess: () => {
        // Invalidate or refetch relevant queries if needed
        queryClient.invalidateQueries(['tradeProposals', connectionId]);
        // Or queryClient.invalidateQueries(['connections']); etc.
      },
    }
  );
};
