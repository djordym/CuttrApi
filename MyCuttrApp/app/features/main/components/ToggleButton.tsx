// src/components/ToggleButton.tsx

import React from 'react';
import { View, TouchableOpacity, Text, StyleSheet } from 'react-native';
import { COLORS } from '../../../theme/colors';

interface ToggleButtonProps {
  options: [string, string];
  selected: string;
  onToggle: (option: string) => void;
}

export const ToggleButton: React.FC<ToggleButtonProps> = ({
  options,
  selected,
  onToggle,
}) => {
  return (
    <View style={styles.toggleContainer}>
      {options.map((option) => {
        const isActive = selected === option;
        return (
          <TouchableOpacity
            key={option}
            onPress={() => onToggle(option)}
            style={[
              styles.button,
              isActive && styles.activeButton,
            ]}
            accessibilityRole="button"
            accessibilityLabel={option}
          >
            <Text
              style={[
                styles.buttonText,
                isActive && styles.activeButtonText,
              ]}
            >
              {option}
            </Text>
          </TouchableOpacity>
        );
      })}
    </View>
  );
};

export default ToggleButton;

const styles = StyleSheet.create({
  toggleContainer: {
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: COLORS.textLight,
    borderRadius: 20,
    alignSelf: 'center',
    padding: 3,
    marginBottom: 15,
  },
  button: {
    paddingHorizontal: 20,
    paddingVertical: 8,
    borderRadius: 18,
  },
  activeButton: {
    backgroundColor: COLORS.accentGreen,
  },
  buttonText: {
    fontSize: 14,
    fontWeight: '600',
    color: COLORS.textDark,
  },
  activeButtonText: {
    color: COLORS.textLight,
  },
});
