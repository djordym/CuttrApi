import { createApiClient } from "./apiClient";
import { UserProfile } from "../types/types";
import mime from "mime";

export const getCurrentUserProfile = async (
    token: string
): Promise<UserProfile> => {
    try {
        const apiClient = await createApiClient(token);
        const url = `/Profile/CurrentUserdata`;
        console.debug("Attempted URL: ", url);

        const response = await apiClient.get<UserProfile>(url);
        console.log("Profile retrieval successful:", response.data);
        return response.data;
    } catch (error: any) {
        console.error(
            "Profile retrieval error:",
            error.response?.data || error.message
        );
        throw error;
    }
};

export const patchUserProfile = async (token: string, patchDocument: any) => {
    try {
        const apiClient = await createApiClient(token);
        const url = `/Profile`;
        console.debug("Attempted URL: ", url);
        const response = await apiClient.patch(url, patchDocument);
        console.log("Profile update successful:", response.data);
        return response.data;
    } catch (error: any) {
        console.error("Failed to patch user profile:", error);
        throw error;
    }
};

export const uploadProfilePicture = async (token: string, asset: any) => {
    const formData = new FormData();
    // @ts-ignore
    formData.append('photo', {
        uri: asset.uri,
        name: asset.fileName,
        type: asset.mimeType,
      });
    try {
        const apiClient = await createApiClient(token);
        const url = "/UploadFile/ProfilePic";
        console.debug("Attempted URL: ", apiClient.defaults.baseURL + url);
        const response = await apiClient.post(url, formData, {
            headers: {
                "Content-Type": "multipart/form-data",
            },
        });
        console.log("Profile picture upload successful:", response.data);
        return response.data;
    } catch (error: any) {
        if (error.response) {
            // Request made and server responded
            console.error("Error response data:", error.response.data);
            console.error("Error response status:", error.response.status);
            console.error("Error response headers:", error.response.headers);
        } else if (error.request) {
            // Request made but no response received
            console.error("No response received:", error.request);
        } else {
            // Something happened in setting up the request that triggered an Error
            console.error("Error in setting up request:", error.message);
        }
        throw error;
    }
};

export const addPlantWithImage = async (
    token: string,
    plant: { name: string; description: string; imageUrl: string },
    asset: any
) => {
    const formData = new FormData();
    const plantInfo = JSON.stringify({
        name: plant.name,
        description: plant.description,
        imageUrl: plant.imageUrl,
        // add other plant fields here if needed
    });

    formData.append('plantinfo', plantInfo);

    // @ts-ignore
    formData.append('photo', {
        uri: asset.uri,
        name: asset.fileName,
        type: asset.mimeType,
    });

    try {
        const apiClient = await createApiClient(token);
        const url = "/UploadFile/PlantPic";
        console.debug("Attempted URL: ", apiClient.defaults.baseURL + url);
        const response = await apiClient.post(url, formData, {
            headers: {
                "Content-Type": "multipart/form-data",
            },
        });
        console.log("Plant addition successful:", response.data);
        return response.data;
    } catch (error: any) {
        if (error.response) {
            // Request made and server responded
            console.error("Error response data:", error.response.data);
            console.error("Error response status:", error.response.status);
            console.error("Error response headers:", error.response.headers);
        } else if (error.request) {
            // Request made but no response received
            console.error("No response received:", error.request);
        } else {
            // Something happened in setting up the request that triggered an Error
            console.error("Error in setting up request:", error.message);
        }
        throw error;
    }
};

