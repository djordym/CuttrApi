import { PlantStage, PlantCategory, WateringNeed, LightRequirement, Size, IndoorOutdoor, PropagationEase, PetFriendly, Extras } from './enums';

export interface RefreshTokenRequest {
    refreshToken: string;
}

export interface MessageRequest {
    matchId: number;
    messageText: string;
}

export interface PlantCreateRequest {
    plantDetails: PlantRequest;
    image: File;
}

export interface PlantRequest {
    speciesName: string;
    description: string | null;
    plantStage: PlantStage;
    plantCategory: PlantCategory | null;
    wateringNeed: WateringNeed | null;
    lightRequirement: LightRequirement | null;
    size: Size | null;
    indoorOutdoor: IndoorOutdoor | null;
    propagationEase: PropagationEase | null;
    petFriendly: PetFriendly | null;
    extras: Extras[];
}

export interface PlantUpdateRequest {
    speciesName: string;
    careRequirements: string;
    description: string;
    category: string;
}

export interface ReportRequest {
    reportedUserId: number;
    reason: string;
    comments: string;
}

export interface SwipeRequest {
    swiperPlantId: number;
    swipedPlantId: number;
    isLike: boolean;
}

export interface UpdateLocationRequest {
    latitude: number;
    longitude: number;
}

export interface UserLoginRequest {
    email: string;
    password: string;
}

export interface UserPreferencesRequest {
    searchRadius: number;
    preferedPlantStage: PlantStage[];
    preferedPlantCategory: PlantCategory[];
    preferedWateringNeed: WateringNeed[];
    preferedLightRequirement: LightRequirement[];
    preferedSize: Size[];
    preferedIndoorOutdoor: IndoorOutdoor[];
    preferedPropagationEase: PropagationEase[];
    preferedPetFriendly: PetFriendly[];
    preferedExtras: Extras[];
}

export interface UserProfileImageUpdateRequest {
    image: File;
}

export interface UserRegistrationRequest {
    email: string;
    password: string;
    name: string;
}

export interface UserUpdateRequest {
    name: string;
    bio: string;
}

export interface AuthTokenResponse {
    accessToken: string;
    refreshToken: string;
    tokenType: string;
    expiresIn: number;
}

export interface MatchResponse {
    matchId: number;
    plant1: PlantResponse;
    plant2: PlantResponse;
    user1: UserResponse;
    user2: UserResponse;
    isClosed: boolean;    
}

export interface MessageResponse {
    messageId: number;
    matchId: number;
    senderUserId: number;
    messageText: string;
    sentAt: Date;
    isRead: boolean;
}

export interface PlantResponse {
    plantId: number;
    userId: number;
    speciesName: string;
    description: string;
    plantStage: PlantStage;
    plantCategory: PlantCategory;
    wateringNeed: WateringNeed;
    lightRequirement: LightRequirement;
    size?: Size;
    indoorOutdoor?: IndoorOutdoor;
    propagationEase?: PropagationEase;
    petFriendly?: PetFriendly;
    extras?: Extras[];
    imageUrl: string;
}

export interface ReportResponse {
    reportId: number;
    reporterUserId: number;
    reportedUserId: number;
    reason: string;
    comments: string;
    createdAt: Date;
    isResolved: boolean;
}

export interface SwipeResponse {
    isMatch: boolean;
    match: MatchResponse;
}

export interface UserLoginResponse {
    userId: number;
    email: string;
    tokens: AuthTokenResponse;
}

export interface UserPreferencesResponse {
    userId: number;
    searchRadius: number;
    preferedPlantStage: PlantStage[];
    preferedPlantCategory: PlantCategory[];
    preferedWateringNeed: WateringNeed[];
    preferedLightRequirement: LightRequirement[];
    preferedSize: Size[];
    preferedIndoorOutdoor: IndoorOutdoor[];
    preferedPropagationEase: PropagationEase[];
    preferedPetFriendly: PetFriendly[];
    preferedExtras: Extras[];
}

export interface UserResponse {
    userId: number;
    email: string;
    name: string;
    profilePictureUrl: string;
    bio: string;
    locationLatitude?: number;
    locationLongitude?: number;
}
