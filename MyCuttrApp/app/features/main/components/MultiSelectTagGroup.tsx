// components/shared/MultiSelectTagGroup.tsx
import React from 'react';
import { View, TouchableOpacity, Text, StyleSheet } from 'react-native';

interface MultiSelectTagGroupProps<T extends string | number> {
  values: T[];
  selectedValues: T[];
  onToggle: (val: T) => void;
}

function MultiSelectTagGroup<T extends string | number>({
  values,
  selectedValues,
  onToggle,
}: MultiSelectTagGroupProps<T>) {
  return (
    <View style={styles.tagGroupContainer}>
      {values.map((val) => {
        const isSelected = selectedValues.includes(val);
        return (
          <TouchableOpacity
            key={String(val)}
            style={[
              styles.singleTag,
              isSelected && styles.singleTagSelected,
            ]}
            onPress={() => onToggle(val)}
          >
            <Text
              style={[
                styles.singleTagText,
                isSelected && styles.singleTagTextSelected,
              ]}
            >
              {val}
            </Text>
          </TouchableOpacity>
        );
      })}
    </View>
  );
}

export default MultiSelectTagGroup;

const styles = StyleSheet.create({
  tagGroupContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    marginBottom: 6,
  },
  singleTag: {
    borderWidth: 1,
    borderColor: '#1EAE98', // or your "COLORS.primary"
    borderRadius: 20,
    paddingVertical: 6,
    paddingHorizontal: 12,
    marginRight: 8,
    marginBottom: 8,
  },
  singleTagSelected: {
    backgroundColor: '#1EAE98', // or your "COLORS.primary"
  },
  singleTagText: {
    fontSize: 12,
    color: '#1EAE98', // or your "COLORS.primary"
  },
  singleTagTextSelected: {
    color: '#fff',
    fontWeight: '600',
  },
});
