import { StyleSheet, Platform } from 'react-native';
import { COLORS } from '../../../theme/colors';

export const authStyles = StyleSheet.create({
  gradientBackground: {
    flex: 1,
  },
  scrollContent: {
    flexGrow: 1,
    justifyContent: 'center',
  },
  keyboardAvoid: {
    flex: 1,
    justifyContent: 'center',
    paddingHorizontal: 24,
  },
  logoContainer: {
    alignItems: 'center',
    marginBottom: 30,
  },
  logo: {
    width: 200,
    height: 200,
    resizeMode: 'contain',
    borderRadius: 10,
  },
  cardContainer: {
    backgroundColor: '#fff',
    borderRadius: 12,
    padding: 20,
    marginHorizontal: 4,
    ...Platform.select({
      ios: {
        shadowColor: '#000',
        shadowOpacity: 0.1,
        shadowRadius: 5,
        shadowOffset: { width: 0, height: 3 },
      },
      android: {
        elevation: 3,
      },
    }),
  },
  title: {
    fontSize: 24,
    fontWeight: '700',
    color: COLORS.textDark,
    textAlign: 'center',
  },
  // Common navigation link styling ("Register" or "Login")

  navLinkContainer: {
    marginTop: 20,
    alignSelf: 'center',
  },
  navLinkText: {
    color: COLORS.accentGreen,
    fontSize: 14,
    fontWeight: '600',
  },
  confirmButton: {
    backgroundColor: COLORS.accentGreen,
    borderRadius: 8,
    padding: 13,
    alignItems: 'center',
    justifyContent: 'center',
    marginTop: 20,
  },
  confirmButtonText: {
    fontSize: 14,
    color: COLORS.textLight,
    fontWeight: '600',
    textAlign: 'center',
  },
});

export default authStyles;