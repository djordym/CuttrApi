import { useMutation, useQueryClient } from 'react-query';
import { connectionService } from '../../../api/connectionService';
import { TradeProposalRequest, TradeProposalResponse } from '../../../types/apiTypes';
import { useQuery } from 'react-query';
import { TradeProposalStatus } from '../../../types/enums';

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

export const useTradeProposals = (connectionId: number) => {
  return useQuery<TradeProposalResponse[], Error>(
    ["tradeProposals", connectionId],
    () => connectionService.getTradeProposals(connectionId),
    { staleTime: 1000 * 60 } // 1 minute stale time
  );
};

// Hook to update the status of a trade proposal.
export const useUpdateTradeProposalStatus = (connectionId: number) => {
  const queryClient = useQueryClient();
  return useMutation(
    ({
      proposalId,
      newStatus,
    }: {
      proposalId: number;
      newStatus: TradeProposalStatus;
    }) => connectionService.updateTradeProposalStatus(connectionId, proposalId, { newStatus }),
    {
      onSuccess: () => {
        queryClient.invalidateQueries(["tradeProposals", connectionId]);
      },
    }
  );
};