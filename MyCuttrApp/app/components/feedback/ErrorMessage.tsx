import React from 'react';
import { Text, StyleSheet, View } from 'react-native';

interface Props {
  message: string | null;
}

const ErrorMessage: React.FC<Props> = ({ message }) => {
  if (!message) return null;
  return (
    <View style={styles.container}>
      <Text style={styles.text}>{message}</Text>
    </View>
  );
};

export default ErrorMessage;

const styles = StyleSheet.create({
  container: {
    backgroundColor: '#FF6B6B',
    padding: 10,
    marginBottom:10,
    borderRadius:8
  },
  text: {
    color: '#fff',
    fontWeight:'500',
    textAlign:'center'
  }
});
