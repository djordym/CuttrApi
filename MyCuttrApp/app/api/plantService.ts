import api from "./axiosConfig";
import {
  PlantCreateRequest,
  PlantResponse,
  PlantRequest,
} from "../types/apiTypes";

export const plantService = {
  addMyPlant: async (data: PlantCreateRequest): Promise<PlantResponse> => {
    const formData = new FormData();

    // Use the prefix "PlantDetails." for fields of the nested PlantDetails object
    formData.append("Image", data.image); // Keep top-level if matching the property name in PlantCreateRequest
    formData.append("PlantDetails.SpeciesName", data.plantDetails.speciesName);
    formData.append(
      "PlantDetails.PlantStage",
      String(data.plantDetails.plantStage)
    );

    formData.append(
      "PlantDetails.Description",
      data.plantDetails.description ?? ""
    );
    formData.append(
      "PlantDetails.PlantCategory",
      data.plantDetails.plantCategory != null
        ? String(data.plantDetails.plantCategory)
        : ""
    );
    formData.append(
      "PlantDetails.WateringNeed",
      data.plantDetails.wateringNeed != null
        ? String(data.plantDetails.wateringNeed)
        : ""
    );
    formData.append(
      "PlantDetails.LightRequirement",
      data.plantDetails.lightRequirement != null
        ? String(data.plantDetails.lightRequirement)
        : ""
    );
    formData.append(
      "PlantDetails.Size",
      data.plantDetails.size != null ? String(data.plantDetails.size) : ""
    );
    formData.append(
      "PlantDetails.IndoorOutdoor",
      data.plantDetails.indoorOutdoor != null
        ? String(data.plantDetails.indoorOutdoor)
        : ""
    );
    formData.append(
      "PlantDetails.PropagationEase",
      data.plantDetails.propagationEase != null
        ? String(data.plantDetails.propagationEase)
        : ""
    );
    formData.append(
      "PlantDetails.PetFriendly",
      data.plantDetails.petFriendly != null
        ? String(data.plantDetails.petFriendly)
        : ""
    );

    if (data.plantDetails.extras && data.plantDetails.extras.length > 0) {
      data.plantDetails.extras.forEach((extra) => {
        // Append each extra with the same key using the dot notation prefix
        formData.append("PlantDetails.Extras", String(extra));
      });
    }

    const response = await api.post<PlantResponse>("/plants/me", formData, {
      headers: { "Content-Type": "multipart/form-data" },
    });

    return response.data;
  },

  getPlantById: async (plantId: number): Promise<PlantResponse> => {
    const response = await api.get<PlantResponse>(`/plants/${plantId}`);
    return response.data;
  },

  updateMyPlant: async (
    plantId: number,
    data: PlantRequest
  ): Promise<PlantResponse> => {
    // Here just sending JSON request body
    const response = await api.put<PlantResponse>(
      `/plants/me/${plantId}`,
      data
    );
    return response.data;
  },

  deleteMyPlant: async (plantId: number): Promise<void> => {
    await api.delete(`/plants/me/${plantId}`);
  },

  getUserPlants: async (userId: number): Promise<PlantResponse[]> => {
    const response = await api.get<PlantResponse[]>(`/plants/users/${userId}`);
    return response.data;
  },

  getMyPlants: async (): Promise<PlantResponse[]> => {
    const response = await api.get<PlantResponse[]>("/plants/users/me");
    return response.data;
  },

  getLikablePlants: async (): Promise<PlantResponse[]> => {
    const response = await api.get<PlantResponse[]>('/plants/likable');
    return response.data;
  },

  getPlantsLikedByMeFromUser: async (otherUserId: number): Promise<PlantResponse[]> => {
    const response = await api.get<PlantResponse[]>(`/plants/liked-by-me/from/${otherUserId}`);
    return response.data;
  },

  getPlantsLikedByUserFromMe: async (otherUserId: number): Promise<PlantResponse[]> => {
    const response = await api.get<PlantResponse[]>(`/plants/liked-by/${otherUserId}/from-me`);
    return response.data;
  }
};
