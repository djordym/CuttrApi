import { createApiClient } from './apiClient';
import { Plant, Match } from '../types/types';

export const fetchPlantsToLike = async (token: string, amount: number): Promise<Plant[]> => {
  try {
    const apiClient = await createApiClient(token);
    const url = `/PlantAndExchange/LikablePlants/${amount}`;
    console.debug("Attempted URL: ", url);

    const response = await apiClient.get<Plant[]>(url);
    console.log("fetchPlantsToLike response: ", response.data);
    return response.data;
  } catch (error: any) {
    console.error('Error fetching plants to like:', error.response?.data || error.message);
    throw error;
  }
};

export const fetchMatches = async (token: string): Promise<Match> => {
  try {
    const apiClient = await createApiClient(token);
    const url = `/PlantAndExchange/Matches`;
    console.debug("Attempted URL: ", url);

    const response = await apiClient.get<Match>(url);
    console.log("fetchMatches response: ", response.data);
    return response.data;
  } catch (error: any) {
    console.error('Error fetching matches:', error.response?.data || error.message);
    throw error;
  }
};


export const putPossiblePlantExchange = async (token: string, exchange: any) => {
  try {
    const apiClient = await createApiClient(token);
    const url = `/PlantAndExchange/PossibleExchanges`;
    console.debug("Attempted URL: ", url);
    const response = await apiClient.put(url, exchange);
    console.log("putPossiblePlantExchange response: ", response.data);
    return response.data;
  } catch (error: any) {
    console.error('Error putting possible plant exchange:', error.response?.data || error.message);
    throw error;
  }
};

export const getMatches = async (token: string): Promise<Match[]> => {
  try {
    const apiClient = await createApiClient(token);
    const url = `/PlantAndExchange/Matches`;
    console.debug("Attempted URL: ", url);

    const response = await apiClient.get<Match[]>(url);
    console.log("getMatches response: ", response.data);
    return response.data;
  } catch (error: any) {
    console.error('Error fetching matches:', error.response?.data || error.message);
    throw error;
  }
}