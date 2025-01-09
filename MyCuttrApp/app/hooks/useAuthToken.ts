//This file is up for deletion

// import { useEffect, useState } from 'react';
// import { storage } from '../utils/storage';

// export const useAuthToken = () => {
//   const [initializing, setInitializing] = useState(true);
//   const [accessToken, setAccessToken] = useState<string | null>(null);
//   const [refreshToken, setRefreshToken] = useState<string | null>(null);

//   useEffect(() => {
//     const loadTokens = async () => {
//       const at = await storage.getAccessToken();
//       const rt = await storage.getRefreshToken();
//       setAccessToken(at);
//       setRefreshToken(rt);
//       setInitializing(false);
//     };
//     loadTokens();
//   }, []);

//   return { initializing, accessToken, refreshToken };
// };
