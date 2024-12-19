import React from 'react';
import { TextInput, View, StyleSheet } from 'react-native';

interface Props {
  value: string;
  onChangeText: (text: string) => void;
  placeholder?: string;
  secureTextEntry?: boolean;
}

const TextInputField: React.FC<Props> = ({ value, onChangeText, placeholder, secureTextEntry }) => {
  return (
    <View style={styles.container}>
      <TextInput 
        style={styles.input}
        value={value}
        onChangeText={onChangeText}
        placeholder={placeholder}
        secureTextEntry={secureTextEntry}
        placeholderTextColor="#999"
      />
    </View>
  );
};

export default TextInputField;

const styles = StyleSheet.create({
  container: {
    marginVertical: 10,
    borderWidth:1,
    borderColor:'#ccc',
    borderRadius:8,
  },
  input: {
    padding:12,
    fontSize:16,
  }
});
