import React from 'react';
import {
  Modal,
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  FlatList,
  Image,
  ScrollView,
} from 'react-native';
import {
  SwipeResponse,
  ConnectionResponse,
  MatchResponse
} from '../../../types/apiTypes';
import MatchCard from '../components/MatchCard';
import { COLORS } from '../../../theme/colors';
import { t } from 'i18next';

interface ItsAMatchModalProps {
  visible: boolean;
  matches: SwipeResponse[];
  currentUserId: number;
  onClose: () => void;
}

const ItsAMatchModal: React.FC<ItsAMatchModalProps> = ({
  visible,
  matches,
  currentUserId,
  onClose,
}) => {
  // If there are no matches, we can render nothing or an empty state
  if (!matches.length) {
    return null;
  }

  // Since we assume all matches belong to a single connection,
  // we can derive the connection from the first match
  const { connection } = matches[0];

  // Determine the "other user" in this connection
  const otherUser =
    connection.user1.userId === currentUserId
      ? connection.user2
      : connection.user1;

  // Extract just the `match` objects
  const matchData = matches.map((swipeResp) => swipeResp.match);

  return (
    <Modal visible={visible} transparent animationType="slide">
      <View style={styles.overlay}>
        <View style={styles.modalContainer}>
          <Text style={styles.title}>{t('new_matches')}</Text>

            {/* Show the other user's info once at the top */}
            <View style={styles.connectionHeader}>
              <Image
                source={{ uri: otherUser.profilePictureUrl }}
                style={styles.connectionImage}
              />
              <Text style={styles.connectionName}>{otherUser.name}</Text>
            </View>

            {/* Render the list of Match objects */}
            <FlatList
              data={matchData}
              keyExtractor={(item) => item.matchId.toString()}
              showsVerticalScrollIndicator={false}
              renderItem={({ item }) => (
                <MatchCard match={item} currentUserId={currentUserId} />
              )}
              contentContainerStyle={styles.matchesList}
            />

          {/* Close button */}
          <TouchableOpacity style={styles.closeButton} onPress={onClose}>
            <Text style={styles.closeButtonText}>Close</Text>
          </TouchableOpacity>
        </View>
      </View>
    </Modal>
  );
};

export default ItsAMatchModal;

const styles = StyleSheet.create({
  overlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.6)',
    justifyContent: 'center',
    padding: 20,
  },
  modalContainer: {
    flex: 1,
    backgroundColor: '#fff',
    borderRadius: 16,
    padding: 20,
    marginVertical: 40,
  },
  title: {
    fontSize: 24,
    fontWeight: '700',
    color: COLORS.accentGreen,
    marginBottom: 16,
    textAlign: 'center',
  },
  contentContainer: {
    paddingBottom: 20,
  },
  connectionHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 10,
    alignSelf: 'center',
  },
  connectionImage: {
    width: 50,
    height: 50,
    borderRadius: 25,
    marginRight: 12,
  },
  connectionName: {
    fontSize: 18,
    fontWeight: '600',
    color: COLORS.textDark,
  },
  matchesList: {
    padding: 10,
  },
  closeButton: {
    backgroundColor: COLORS.accentGreen,
    paddingVertical: 12,
    borderRadius: 8,
    alignItems: 'center',
    marginTop: 10,
  },
  closeButtonText: {
    color: COLORS.textLight,
    fontSize: 16,
    fontWeight: '600',
  },
});
