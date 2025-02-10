import React from 'react';
import {
  View,
  TouchableOpacity,
  Text,
  StyleSheet,
  StyleProp,
  ViewStyle,
  TextStyle,
} from 'react-native';
import { COLORS } from '../../../theme/colors';

/**
 * A single component that can handle both single-selection and multi-selection
 * by switching on the `mode` prop.
 */
export type TagGroupMode = 'single' | 'multiple';

interface TagGroupProps<T> {
  /** The list of all possible tag values. */
  values: T[];

  /** The selection mode of this component (single or multiple). */
  mode: TagGroupMode;

  /** Single-select: the selected tag. */
  selectedValue?: T | null;

  /** Multi-select: the selected tags. */
  selectedValues?: T[];

  /**
   * Single-select callback: 
   *   - If `isRequired` is true, tapping the selected tag again keeps it selected.
   *   - If `isRequired` is false, tapping the selected tag again deselects it (sets it to null).
   */
  onSelectSingle?: (val: T | null) => void;

  /** Multi-select callback: toggles an item in or out of the array of selected values. */
  onToggleMulti?: (val: T) => void;

  /** Whether a single-select is required (cannot unselect once set). Default is false. */
  isRequired?: boolean;

  /** Optional container style override for the tag group wrapper. */
  containerStyle?: StyleProp<ViewStyle>;

  /** Optional style override for individual tags. */
  tagStyle?: StyleProp<ViewStyle>;

  /** Optional style override for selected tags. */
  tagSelectedStyle?: StyleProp<ViewStyle>;

  /** Optional style override for text inside each tag. */
  tagTextStyle?: StyleProp<TextStyle>;

  /** Optional style override for text in a selected tag. */
  tagTextSelectedStyle?: StyleProp<TextStyle>;

  /**
   * Optional function to convert a tag value into its localized label.
   * If provided, this function will be used to display the tag's text.
   */
  getLabel?: (val: T) => string;
}

function TagGroup<T extends string | number>(props: TagGroupProps<T>) {
  const {
    mode,
    values,
    selectedValue,
    selectedValues,
    onSelectSingle,
    onToggleMulti,
    isRequired = false,
    containerStyle,
    tagStyle,
    tagSelectedStyle,
    tagTextStyle,
    tagTextSelectedStyle,
    getLabel,
  } = props;

  /**
   * Returns true if `val` is currently selected in either single- or multi-select mode.
   */
  const isSelected = (val: T) => {
    if (mode === 'single') {
      return val === selectedValue;
    }
    // Multi-select
    return selectedValues?.includes(val);
  };

  /**
   * Handle tag presses differently depending on the mode.
   * - Single-select: either select the new tag or possibly unselect if isRequired is false.
   * - Multi-select: toggle the given tag in the selectedValues array.
   */
  const handlePress = (val: T) => {
    if (mode === 'single') {
      if (!onSelectSingle) return;
      const alreadySelected = isSelected(val);
      if (isRequired) {
        onSelectSingle(val);
      } else {
        onSelectSingle(alreadySelected ? null : val);
      }
    } else {
      // Multi-select
      if (!onToggleMulti) return;
      onToggleMulti(val);
    }
  };

  return (
    <View style={[styles.container, containerStyle]}>
      {values.map((val) => {
        const selected = isSelected(val);
        return (
          <TouchableOpacity
            key={String(val)}
            style={[
              styles.tag,
              tagStyle,
              selected && [styles.tagSelected, tagSelectedStyle],
            ]}
            onPress={() => handlePress(val)}
            accessible
            accessibilityRole="button"
            accessibilityLabel={`Select tag value: ${getLabel ? getLabel(val) : String(val)}`}
          >
            <Text
              style={[
                styles.tagText,
                tagTextStyle,
                selected && [styles.tagTextSelected, tagTextSelectedStyle],
              ]}
            >
              {getLabel ? getLabel(val) : String(val)}
            </Text>
          </TouchableOpacity>
        );
      })}
    </View>
  );
}

export default TagGroup;

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    marginBottom: 6,
  },
  tag: {
    borderWidth: 1,
    borderColor: COLORS.accentGreen,
    borderRadius: 20,
    paddingVertical: 6,
    paddingHorizontal: 12,
    marginRight: 8,
    marginBottom: 8,
  },
  tagSelected: {
    backgroundColor: COLORS.accentGreen,
  },
  tagText: {
    fontSize: 12,
    color: COLORS.accentGreen,
    fontWeight: '600',
  },
  tagTextSelected: {
    color: COLORS.textLight,
    fontWeight: '600',
  },
});
