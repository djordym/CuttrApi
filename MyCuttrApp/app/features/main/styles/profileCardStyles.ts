import { StyleSheet, Platform } from "react-native";
import { COLORS } from "../../../theme/colors";

export const profileCardStyles = StyleSheet.create({
  // Container that wraps the card
  profileCardContainer: {
    borderRadius: 18,
    overflow: "hidden",
    ...Platform.select({
      ios: {
        shadowColor: "#000",
        shadowOpacity: 0.12,
        shadowRadius: 6,
        shadowOffset: { width: 0, height: 3 },
      },
      android: {
        elevation: 4,
      },
    }),
  },

  // Inner container (for gradients, etc.)
  profileCardInner: {
    borderRadius: 18,
  },

  // Top “header” area of the card
  profileTopContainer: {
    backgroundColor: COLORS.primary,
    height: 120,
    position: "relative",
  },
  profileBackgroundImage: {
    height: "100%",
    resizeMode: "cover",
    position: "absolute",
    bottom: 0,
    right: 0,
    left: -200,
  },

  // Profile Picture
  profilePictureContainer: {
    position: "absolute",
    bottom: -75,
    left: 25,
  },
  profilePicture: {
    width: 170,
    height: 170,
    borderRadius: 40,
    borderWidth: 3,
    borderColor: "#fff",
  },
  profilePlaceholder: {
    width: 170,
    height: 170,
    borderRadius: 40,
    backgroundColor: "#eee",
    alignItems: "center",
    justifyContent: "center",
    borderWidth: 3,
    borderColor: "#fff",
  },
  cameraIconWrapper: {
    position: "absolute",
    bottom: 0,
    right: 0,
    backgroundColor: COLORS.accentGreen,
    borderRadius: 16,
    padding: 6,
  },

  // Edit / Save button in the top-right corner
  profileEditButton: {
    position: "absolute",
    top: 16,
    right: 16,
    width: 35,
    height: 35,
    borderRadius: 20,
    backgroundColor: COLORS.accentGreen,
    justifyContent: "center",
    alignItems: "center",
    ...Platform.select({
      ios: {
        shadowColor: "#000",
        shadowOpacity: 0.1,
        shadowRadius: 5,
        shadowOffset: { width: 0, height: 3 },
      },
      android: {
        elevation: 3,
      },
    }),
  },

  // Name & Location area
  profileInfoContainer: {
    right: -190,
  },
  nameContainer: {
    marginLeft: 17,
    marginTop: 5,
  },
  editNameContainer: {
    flex: 1,
    marginRight: 15,
    backgroundColor: COLORS.cardBg1,
    borderRadius: 5,
    ...Platform.select({
      ios: {
        shadowColor: "#000",
        shadowOpacity: 0.1,
        shadowRadius: 5,
        shadowOffset: { width: 0, height: 3 },
      },
      android: {
        elevation: 3,
      },
    }),
  },
  profileNameText: {
    fontSize: 18,
    fontWeight: "700",
    color: COLORS.textDark,
    margin: 3,
  },
  editableTextName: {
    borderRadius: 5,
    borderBottomWidth: 0,
    padding: 3,
    margin: 0,
  },

  profileLocationRow: {
    flexDirection: "row",
    alignItems: "center",
    marginBottom: 8,
    marginHorizontal: 20,
  },
  locationIcon: {
    marginRight: 4,
    top: 1,
  },
  profileLocationText: {
    fontSize: 14,
    color: COLORS.textDark,
    marginTop: 2,    
  },

  // Bio
  bioContainer: {
    margin: 20,
  },
  bioContainerEdit: {
    backgroundColor: COLORS.cardBg1,
    borderRadius: 5,
    ...Platform.select({
      ios: {
        shadowColor: "#000",
        shadowOpacity: 0.1,
        shadowRadius: 5,
        shadowOffset: { width: 0, height: 3 },
      },
      android: {
        elevation: 3,
      },
    }),
  },
  bioText: {
    fontSize: 14,
    color: COLORS.textDark,
    lineHeight: 20,
    margin: 5,
  },
  editableTextBio: {
    // remove typical text input borders
    borderRadius: 5,
    borderBottomWidth: 0,
    padding: 5,
    margin: 0,
  },
  bioPlaceholder: {
    color: "#999",
    fontStyle: "italic",
  },

  // For inline editing

  // Errors
  errorText: {
    color: "#FF6B6B",
    marginBottom: 10,
    textAlign: "center",
    fontWeight: "600",
  },
});

export default profileCardStyles;