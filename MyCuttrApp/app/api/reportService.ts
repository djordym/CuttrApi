import api from './axiosConfig';
import { ReportRequest, ReportResponse } from '../types/apiTypes';

export const reportService = {
  createReport: async (data: ReportRequest): Promise<ReportResponse> => {
    const response = await api.post<ReportResponse>('/reports', data);
    return response.data;
  }
};

export default reportService;