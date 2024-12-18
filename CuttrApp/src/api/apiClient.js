import axios from 'axios';
import { getApiUrl } from './apiConfig';


export const createApiClient = async (token) => {
  const baseURL = await getApiUrl();
  return axios.create({
    baseURL,
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
};
