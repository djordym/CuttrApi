import api from './axiosConfig';
import { PlantCreateRequest, PlantResponse, PlantRequest } from '../types/apiTypes';

export const plantService = {
  addMyPlant: async (data: PlantCreateRequest): Promise<PlantResponse> => {
    const formData = new FormData();
    formData.append('Image', data.Image);
    formData.append('SpeciesName', data.PlantDetails.SpeciesName);
    formData.append('Description', data.PlantDetails.Description);
    formData.append('PlantStage', data.PlantDetails.PlantStage);
    formData.append('PlantCategory', data.PlantDetails.PlantCategory);
    formData.append('WateringNeed', data.PlantDetails.WateringNeed);
    formData.append('LightRequirement', data.PlantDetails.LightRequirement);
    if (data.PlantDetails.Size) formData.append('Size', data.PlantDetails.Size);
    if (data.PlantDetails.IndoorOutdoor) formData.append('IndoorOutdoor', data.PlantDetails.IndoorOutdoor);
    if (data.PlantDetails.PropagationEase) formData.append('PropagationEase', data.PlantDetails.PropagationEase);
    if (data.PlantDetails.PetFriendly) formData.append('PetFriendly', data.PlantDetails.PetFriendly);
    if (data.PlantDetails.Extras) {
      data.PlantDetails.Extras.forEach((extra) => formData.append('Extras', extra));
    }

    const response = await api.post<PlantResponse>('/plants/me', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    return response.data;
  },

  getPlantById: async (plantId: number): Promise<PlantResponse> => {
    const response = await api.get<PlantResponse>(`/plants/${plantId}`);
    return response.data;
  },

  updateMyPlant: async (plantId: number, data: PlantRequest): Promise<PlantResponse> => {
    // Here just sending JSON request body
    const response = await api.put<PlantResponse>(`/plants/me/${plantId}`, data);
    return response.data;
  },

  deleteMyPlant: async (plantId: number): Promise<void> => {
    await api.delete(`/plants/me/${plantId}`);
  },

  getUserPlants: async (userId: number): Promise<PlantResponse[]> => {
    const response = await api.get<PlantResponse[]>(`/users/${userId}/plants`);
    return response.data;
  },

  getMyPlants: async (): Promise<PlantResponse[]> => {
    const response = await api.get<PlantResponse[]>('/users/me/plants');
    return response.data;
  }
};
