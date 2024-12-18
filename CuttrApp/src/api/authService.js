import { createApiClient } from './apiClient';

export const login = async (email, password) => {
    try {
        const apiClient = await createApiClient();
        const url = `/User/token`;
        console.debug("Attempted URL: ", url);

        const response = await apiClient.post(url, { email, password });
        if (response.data) {
            console.log('Login successful:', response.data);
            console.debug('Token:', response.data);
        }
        return response.data;
    } catch (error) {
        console.error('Login error:', error.response?.data || error.message);
        throw error;
    }
};

export const postSignUp = async (name, email, password) => {
    try {
        const apiClient = await createApiClient();
        const url = `/User/signup`;
        console.debug("Attempted URL: ", url);
        console.debug("Name: ", name);
        console.debug("Email: ", email);
        console.debug("Password: ", password);

        const response = await apiClient.post(url, { name, email, password });
        console.log('Signup successful:', response.data);
        return response.data;
    } catch (error) {
        console.error('Signup error:', error.response?.data || error.message);
        throw error;
    }
};

export const validateToken = async (token) => {
    try {
        const apiClient = await createApiClient(token);
        const url = `/User/validateToken`;
        console.debug("Attempted URL: ", url);

        const response = await apiClient.get(url);
        console.log('Token validation successful:', response.data);
        return true;
    } catch (error) {
        console.error('Token validation error:', error.response?.data || error.message);
        return false;
    }
};

