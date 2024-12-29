import React, { useState } from 'react';
import { Modal, View, Text, TextInput, StyleSheet, TouchableOpacity, ActivityIndicator } from 'react-native';
import { useTranslation } from 'react-i18next';
import { userService } from '../../../api/userService';
import { UserUpdateRequest } from '../../../types/apiTypes';

interface EditProfileModalProps {
  visible: boolean;
  initialName: string;
  initialBio: string;
  onClose: () => void;
  onUpdated: () => void; // callback after successful update
}

export const EditProfileModal: React.FC<EditProfileModalProps> = ({ visible, initialName, initialBio, onClose, onUpdated }) => {
  const { t } = useTranslation();
  const [name, setName] = useState(initialName);
  const [bio, setBio] = useState(initialBio);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleConfirm = async () => {
    setLoading(true);
    setError(null);

    const payload: UserUpdateRequest = { name: name, bio: bio };
    try {
      await userService.updateMe(payload);
      onUpdated();
      onClose();
    } catch {
      setError(t('edit_profile_error_message'));
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    onClose();
  };

  return (
    <Modal visible={visible} animationType="slide" transparent>
      <View style={styles.overlay}>
        <View style={styles.modalContainer}>
          <Text style={styles.title}>{t('edit_profile_title')}</Text>
          {error && <Text style={styles.errorText}>{error}</Text>}

          <Text style={styles.label}>{t('edit_profile_name_label')}:</Text>
          <TextInput
            style={styles.input}
            value={name}
            onChangeText={setName}
            accessibilityLabel={t('edit_profile_name_label')}
          />

          <Text style={styles.label}>{t('edit_profile_bio_label')}:</Text>
          <TextInput
            style={[styles.input, { height:80 }]}
            value={bio}
            onChangeText={setBio}
            multiline
            accessibilityLabel={t('edit_profile_bio_label')}
          />

          {loading && <ActivityIndicator size="small" color="#1EAE98" style={{marginVertical:10}}/>}

          <View style={styles.actions}>
            <TouchableOpacity onPress={handleCancel} style={styles.cancelButton} accessibilityRole="button">
              <Text style={styles.cancelButtonText}>{t('edit_profile_cancel_button')}</Text>
            </TouchableOpacity>
            <TouchableOpacity onPress={handleConfirm} style={styles.confirmButton} accessibilityRole="button">
              <Text style={styles.confirmButtonText}>{t('edit_profile_confirm_button')}</Text>
            </TouchableOpacity>
          </View>
        </View>
      </View>
    </Modal>
  );
};


const styles = StyleSheet.create({
  overlay:{
    flex:1,
    backgroundColor:'rgba(0,0,0,0.5)',
    justifyContent:'center',
    alignItems:'center'
  },
  modalContainer:{
    width:'90%',
    backgroundColor:'#fff',
    borderRadius:16,
    padding:20
  },
  title:{
    fontSize:18,
    fontWeight:'700',
    color:'#333',
    marginBottom:10
  },
  errorText:{
    color:'#FF6B6B',
    marginBottom:10
  },
  label:{
    fontSize:14,
    fontWeight:'600',
    color:'#333',
    marginBottom:4
  },
  input:{
    borderWidth:1,
    borderColor:'#ccc',
    borderRadius:8,
    padding:10,
    marginBottom:10,
    fontSize:14
  },
  actions:{
    flexDirection:'row',
    justifyContent:'flex-end',
    marginTop:10
  },
  cancelButton:{
    marginRight:10
  },
  cancelButtonText:{
    fontSize:16,
    color:'#333'
  },
  confirmButton:{
    backgroundColor:'#1EAE98',
    paddingHorizontal:15,
    paddingVertical:10,
    borderRadius:8
  },
  confirmButtonText:{
    fontSize:16,
    color:'#fff',
    fontWeight:'600'
  }
});
