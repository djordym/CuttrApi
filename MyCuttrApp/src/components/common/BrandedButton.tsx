import React from 'react';
import { TouchableOpacity, Text, StyleSheet } from 'react-native';

interface Props {
  title: string;
  onPress: () => void;
  disabled?: boolean;
}

const BrandedButton: React.FC<Props> = ({ title, onPress, disabled }) => {
  return (
    <TouchableOpacity onPress={onPress} style={[styles.button, disabled && {opacity:0.5}]} disabled={disabled}>
      <Text style={styles.buttonText}>{title}</Text>
    </TouchableOpacity>
  );
};

export default BrandedButton;

const styles = StyleSheet.create({
  button: {
    backgroundColor: '#1EAE98',
    padding: 16,
    borderRadius: 8,
    alignItems:'center',
    marginVertical:10
  },
  buttonText: {
    color:'#fff',
    fontSize:16,
    fontWeight:'600'
  }
});
