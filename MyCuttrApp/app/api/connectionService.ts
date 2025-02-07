import api from "./axiosConfig";
import {
  ConnectionResponse,
  TradeProposalRequest,
  TradeProposalResponse,
  UpdateTradeProposalStatusRequest,
} from "../types/apiTypes";

export const connectionService = {

  getMyConnections: async (): Promise<ConnectionResponse[]> => {
    const response = await api.get<ConnectionResponse[]>("/connections/me");
    return response.data;
  },

  getConnectionById: async (connectionId: number): Promise<ConnectionResponse> => {
    const response = await api.get<ConnectionResponse>(`/connections/${connectionId}`);
    return response.data;
  },

  getTradeProposals: async (connectionId: number): Promise<TradeProposalResponse[]> => {
    const response = await api.get<TradeProposalResponse[]>(`/connections/${connectionId}/proposals`);
    return response.data;
  },

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

  updateTradeProposalStatus: async (
    connectionId: number,
    proposalId: number,
    data: UpdateTradeProposalStatusRequest
  ): Promise<void> => {
    await api.put(`/connections/${connectionId}/proposals/${proposalId}/status`, data);
  },

  confirmTradeProposalCompletion: async (
    connectionId: number,
    proposalId: number
  ): Promise<void> => {
    await api.put(`/connections/${connectionId}/proposals/${proposalId}/confirm-completion`);
  },
};
