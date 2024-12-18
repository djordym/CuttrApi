export interface UserProfile {
    userId: number;
    userName: string;
    description: string;
    quote: string;
    profilePhotoPath: string;
    ownedPlants: Plant[];
  }
  
export interface Plant {
    plantId: number;
    name: string;
    description: string;
    imageUrl: string;
    userId: number;
    exchangesAsInitiating: PlantExchange[];
    exchangesAsResponding: PlantExchange[];
  }
  
export interface PlantExchange {
    plantExchangeResponseId: number;
    initiatingPlantId: number;
    respondingPlantId: number;
    initiatingUserApproval: boolean | null;
    respondingUserApproval: boolean | null;
  }

export interface Match { //adding users is not strictly necessary but i did it like this in backend and to lazy to change everything + better for readability
  matchId: number;
  user1: UserProfile;
  user2: UserProfile;
  plant1: Plant;
  plant2: Plant;
}
  