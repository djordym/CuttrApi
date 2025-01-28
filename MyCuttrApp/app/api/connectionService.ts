import api from "./axiosConfig";
import {
  ConnectionResponse,
  TradeProposalRequest,
  TradeProposalResponse,
  UpdateTradeProposalStatusRequest,
} from "../types/apiTypes";

export const connectionService = {
  /**
   * Retrieves the current user's connections.
   * GET /api/connections/me
   */
  getMyConnections: async (): Promise<ConnectionResponse[]> => {
    const response = await api.get<ConnectionResponse[]>("/connections/me");
    return response.data;
  },

  /**
   * Retrieves a specific connection by its ID.
   * GET /api/connections/{connectionId}
   * @param connectionId - The ID of the connection to retrieve.
   */
  getConnectionById: async (connectionId: number): Promise<ConnectionResponse> => {
    const response = await api.get<ConnectionResponse>(`/connections/${connectionId}`);
    return response.data;
  },

  /**
   * Retrieves trade proposals for a specific connection.
   * GET /api/connections/{connectionId}/proposals
   * @param connectionId - The ID of the connection whose trade proposals to retrieve.
   */
  getTradeProposals: async (connectionId: number): Promise<TradeProposalResponse[]> => {
    const response = await api.get<TradeProposalResponse[]>(`/connections/${connectionId}/proposals`);
    return response.data;
  },

  /**
   * Creates a new trade proposal for a specific connection.
   * POST /api/connections/{connectionId}/proposals
   * @param connectionId - The ID of the connection for which to create the trade proposal.
   * @param data - The trade proposal request data.
   */
  createTradeProposal: async (
    connectionId: number,
    data: TradeProposalRequest
  ): Promise<TradeProposalResponse> => {
    const response = await api.post<TradeProposalResponse>(
      `/connections/${connectionId}/proposals`,
      data
    );
    return response.data;
  },

  /**
   * Updates the status of an existing trade proposal.
   * PUT /api/connections/{connectionId}/proposals/{proposalId}/status
   * @param connectionId - The ID of the connection related to the trade proposal.
   * @param proposalId - The ID of the trade proposal to update.
   * @param data - The status update request data.
   */
  updateTradeProposalStatus: async (
    connectionId: number,
    proposalId: number,
    data: UpdateTradeProposalStatusRequest
  ): Promise<void> => {
    await api.put(
      `/connections/${connectionId}/proposals/${proposalId}/status`,
      data
    );
  },
};
