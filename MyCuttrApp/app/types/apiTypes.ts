import { PlantStage, PlantCategory, WateringNeed, LightRequirement, Size, IndoorOutdoor, PropagationEase, PetFriendly, Extras } from './enums';


export interface RefreshTokenRequest {
    RefreshToken: string;
}

export interface MessageRequest {
    MatchId: number;
    MessageText: string;
}

export interface PlantCreateRequest {
    PlantDetails: PlantRequest;
    Image: File;
}

export interface PlantRequest {
    SpeciesName: string;
    Description: string;
    PlantStage: PlantStage;
    PlantCategory: PlantCategory;
    WateringNeed: WateringNeed;
    LightRequirement: LightRequirement;
    Size?: Size;
    IndoorOutdoor?: IndoorOutdoor;
    PropagationEase?: PropagationEase;
    PetFriendly?: PetFriendly;
    Extras?: Extras[];
}

export interface PlantUpdateRequest {
    SpeciesName: string;
    CareRequirements: string;
    Description: string;
    Category: string;
}

export interface ReportRequest {
    ReportedUserId: number;
    Reason: string;
    Comments: string;
}

export interface SwipeRequest {
    SwiperPlantId: number;
    SwipedPlantId: number;
    IsLike: boolean;
}

export interface UpdateLocationRequest {
    Latitude: number;
    Longitude: number;
}

export interface UserLoginRequest {
    Email: string;
    Password: string;
}

export interface UserPreferencesRequest {
    SearchRadius: number;
    PreferedPlantStage: PlantStage[];
    PreferedPlantCategory: PlantCategory[];
    PreferedWateringNeed: WateringNeed[];
    PreferedLightRequirement: LightRequirement[];
    PreferedSize: Size[];
    PreferedIndoorOutdoor: IndoorOutdoor[];
    PreferedPropagationEase: PropagationEase[];
    PreferedPetFriendly: PetFriendly[];
    PreferedExtras: Extras[];
}

export interface UserProfileImageUpdateRequest {
    Image: File;
}

export interface UserRegistrationRequest {
    Email: string;
    Password: string;
    Name: string;
}

export interface UserUpdateRequest {
    Name: string;
    Bio: string;
}

export interface AuthTokenResponse {
    AccessToken: string;
    RefreshToken: string;
    TokenType: string;
    ExpiresIn: number;
}

export interface MatchResponse {
    MatchId: number;
    Plant1: PlantResponse;
    Plant2: PlantResponse;
    User1: UserResponse;
    User2: UserResponse;
}

export interface MessageResponse {
    MessageId: number;
    MatchId: number;
    SenderUserId: number;
    MessageText: string;
    SentAt: Date;
    IsRead: boolean;
}

export interface PlantResponse {
    PlantId: number;
    UserId: number;
    SpeciesName: string;
    Description: string;
    PlantStage: PlantStage;
    PlantCategory: PlantCategory;
    WateringNeed: WateringNeed;
    LightRequirement: LightRequirement;
    Size?: Size;
    IndoorOutdoor?: IndoorOutdoor;
    PropagationEase?: PropagationEase;
    PetFriendly?: PetFriendly;
    Extras?: Extras[];
    ImageUrl: string;
}

export interface ReportResponse {
    ReportId: number;
    ReporterUserId: number;
    ReportedUserId: number;
    Reason: string;
    Comments: string;
    CreatedAt: Date;
    IsResolved: boolean;
}

export interface SwipeResponse {
    IsMatch: boolean;
    Match: MatchResponse;
}

export interface UserLoginResponse {
    UserId: number;
    Email: string;
    Tokens: AuthTokenResponse;
}

export interface UserPreferencesResponse {
    UserId: number;
    SearchRadius: number;
    PreferedPlantStage: PlantStage[];
    PreferedPlantCategory: PlantCategory[];
    PreferedWateringNeed: WateringNeed[];
    PreferedLightRequirement: LightRequirement[];
    PreferedSize: Size[];
    PreferedIndoorOutdoor: IndoorOutdoor[];
    PreferedPropagationEase: PropagationEase[];
    PreferedPetFriendly: PetFriendly[];
    PreferedExtras: Extras[];
}

export interface UserResponse {
    UserId: number;
    Email: string;
    Name: string;
    ProfilePictureUrl: string;
    Bio: string;
    LocationLatitude?: number;
    LocationLongitude?: number;
}
