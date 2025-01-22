// File: src/styles/headerStyles.ts
import { StyleSheet } from 'react-native';
import { COLORS } from '../../../theme/colors';

export const headerStyles = StyleSheet.create({
  headerGradient: {
    paddingHorizontal: 20,
    paddingVertical: 15,
    marginBottom: 10,
    elevation: 10,
    backgroundColor: COLORS.background,
    // ... any other shared header style props
  },
  headerRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    // ...
  },
  headerRowChat: {
    flexDirection: 'row',
    alignItems: 'center',
    // ...
    },
  headerTitle: {
    fontSize: 24,
    fontWeight: '700',
    color: COLORS.textLight,
    // ...
  },
  headerActionButton: {
    padding: 6,
  },
  headerBackButton: {
    marginRight: 10,
    borderColor: COLORS.textLight,
    paddingRight: 10,
    borderRightWidth: 1,
},
  headerRowSticky: {
    position: 'sticky',
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginHorizontal: 20,
    marginBottom: 10,
  }
});
