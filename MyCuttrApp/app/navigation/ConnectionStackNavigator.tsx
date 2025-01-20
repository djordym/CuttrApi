import React from "react";
import { createNativeStackNavigator } from "@react-navigation/native-stack";
import ConnectionsScreen from "../features/main/screens/ConnectionsScreen";
import ChatScreen from "../features/main/screens/ChatScreen";

export type ConnectionStackParamList = {
    Connections: undefined;
    Chat: undefined;
    };

const Stack = createNativeStackNavigator<ConnectionStackParamList>();

const ConnectionStackNavigator = () => {
    return (
        <Stack.Navigator screenOptions={{ headerShown: false }}>
            <Stack.Screen name="Connections" component={ConnectionsScreen} />
            <Stack.Screen name="Chat" component={ChatScreen} />
        </Stack.Navigator>
    );
}

export default ConnectionStackNavigator;