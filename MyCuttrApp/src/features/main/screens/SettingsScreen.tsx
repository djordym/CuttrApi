import React, { useState } from 'react';
import { View, Text, StyleSheet, TouchableOpacity } from 'react-native';
import { useTranslation } from 'react-i18next';
import { storage } from '../../../utils/storage';

const SettingsScreen = () => {
  const { t, i18n } = useTranslation();
  const [currentLang, setCurrentLang] = useState(i18n.language);

  const handleLanguageChange = async (lang: string) => {
    await i18n.changeLanguage(lang);
    await storage.saveLanguage(lang);
    setCurrentLang(lang);
  };

  return (
    <View style={styles.container}>
      <Text style={styles.header}>Settings</Text>
      <Text style={styles.label}>Selected Language: {currentLang}</Text>
      <View style={styles.buttons}>
        <TouchableOpacity onPress={() => handleLanguageChange('en')} style={[styles.button, currentLang === 'en' && styles.selected]}>
          <Text style={styles.buttonText}>English</Text>
        </TouchableOpacity>
        <TouchableOpacity onPress={() => handleLanguageChange('fr')} style={[styles.button, currentLang === 'fr' && styles.selected]}>
          <Text style={styles.buttonText}>Français</Text>
        </TouchableOpacity>
      </View>
    </View>
  );
};

export default SettingsScreen;

const styles = StyleSheet.create({
  container: {
    flex:1,
    backgroundColor:'#fff',
    padding:20
  },
  header: {
    fontSize:24,
    fontWeight:'700',
    marginBottom:20
  },
  label: {
    fontSize:16,
    marginBottom:20
  },
  buttons: {
    flexDirection:'row'
  },
  button: {
    padding:10,
    borderRadius:8,
    borderWidth:1,
    borderColor:'#ccc',
    marginRight:10
  },
  selected: {
    backgroundColor:'#1EAE98',
    borderColor:'#1EAE98'
  },
  buttonText: {
    color:'#000'
  }
});
