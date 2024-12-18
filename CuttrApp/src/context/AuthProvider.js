// src/context/AuthProvider.js
import React, { useReducer, useMemo, useEffect } from 'react';
import AuthContext from './AuthContext';
import * as SecureStore from 'expo-secure-store';
import authReducer from '../reducers/authReducer';
import { login, postSignUp } from '../api/authService.js';

const AuthProvider = ({ children }) => {
  const [state, dispatch] = useReducer(authReducer, {
    isLoading: true,
    isSignout: false,
    userToken: null,
  });

  useEffect(() => {
    const bootstrapAsync = async () => {
      let userToken;
      try {
        userToken = await SecureStore.getItemAsync('userToken');
      } catch (e) {
        console.error("Error restoring token: ", e);
      }
      dispatch({ type: 'RESTORE_TOKEN', token: userToken });
    };

    bootstrapAsync();
  }, []);

  const authContext = useMemo(
    () => ({
      signIn: async (data) => {
        // Simulate authentication process and obtain token
        
          const userToken = await login(data.email, data.password);
          if(userToken){
            try {
              // Securely store the token
              console.log('Storing token:', userToken);
              await SecureStore.setItemAsync('userToken', userToken);
    
              // Then update the application state
              dispatch({ type: 'SIGN_IN', token: userToken });
            } catch (e) {
              console.error("Error storing the user token:", e);
            }
          }
      },
      signOut: () => {
        console.log('Signing out');
        // Remove the token from storage
        SecureStore.deleteItemAsync('userToken');
        dispatch({ type: 'SIGN_OUT' })
      },
      signUp: async (name, email, password) => {
        try{
          await postSignUp(name, email, password);
        } catch (e) {
          console.error("Error signing up:", e);
        }
      },
      state

    }),
    [state]
  );

  return <AuthContext.Provider value={authContext}>{children}</AuthContext.Provider>;
};

export default AuthProvider;
