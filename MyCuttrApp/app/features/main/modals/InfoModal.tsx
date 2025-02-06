// File: app/features/main/components/InfoModal.tsx
import React from 'react';
import { Modal, View, TouchableOpacity, StyleSheet, Dimensions } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { COLORS } from '../../../theme/colors'; // adjust the import path as needed
import { LinearGradient } from 'expo-linear-gradient';

const { width } = Dimensions.get('window');

export interface InfoModalProps {
  visible: boolean;
  onClose: () => void;
  children: React.ReactNode;
}

const InfoModal: React.FC<InfoModalProps> = ({ visible, onClose, children }) => {
  return (
    <Modal visible={visible} transparent animationType="fade">
      <View style={styles.infoOverlayContainer}>
        <LinearGradient style={styles.infoModalContainer} colors={[COLORS.primary, COLORS.secondary]}>
          <TouchableOpacity style={styles.closeButton} onPress={onClose}>
            <Ionicons name="close" size={24} color="#fff" />
          </TouchableOpacity>
          {children}
        </LinearGradient>
      </View>
    </Modal>
  );
};

export default InfoModal;

const styles = StyleSheet.create({
  infoOverlayContainer: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.6)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  infoModalContainer: {
    width: width * 0.9,
    borderRadius: 12,
    overflow: 'hidden',
    padding: 10,
    position: 'relative',
  },
  closeButton: {
    position: 'absolute',
    top: 15,
    right: 15,
    backgroundColor: 'rgba(0,0,0,0.5)',
    borderRadius: 20,
    padding: 4,
    zIndex: 10,
  },
});
