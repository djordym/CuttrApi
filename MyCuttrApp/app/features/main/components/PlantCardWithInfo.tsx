// FullSizePlantCard.tsx
import React from 'react';
import { View, Image, StyleSheet, Platform } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { COLORS } from '../../../theme/colors';
import { PlantOverlay } from '../components/PlantOverlay';
import { PlantResponse } from '../../../types/apiTypes';

interface PlantCardWithInfoProps {
    plant: PlantResponse;
    compact?: boolean;
}

export const PlantCardWithInfo: React.FC<PlantCardWithInfoProps> = ({ plant, compact = false }) => {
    // Combine any tags for the overlay
    const allTags = [
        plant.plantStage,
        plant.plantCategory,
        plant.wateringNeed,
        plant.lightRequirement,
        plant.size,
        plant.indoorOutdoor,
        plant.propagationEase,
        plant.petFriendly,
        ...(plant.extras ?? []),
    ].filter(Boolean);

    return (
        <View style={styles.plantCardFull}>
            <View style={styles.fullImageContainer}>
                {plant.imageUrl ? (
                    <Image
                        source={{ uri: plant.imageUrl }}
                        style={styles.fullImage}
                        resizeMode="contain"
                    />
                ) : (
                    <View style={styles.plantPlaceholder}>
                        <Ionicons name="leaf" size={60} color={COLORS.primary} />
                    </View>
                )}

                {/* Overlay with species, description, tags, etc. */}
                <View style={styles.fullImageOverlay}>
                    <PlantOverlay
                        speciesName={plant.speciesName}
                        description={plant.description}
                        tags={allTags}
                        compact={compact}
                    />
                </View>
            </View>
        </View>
    );
};

const styles = StyleSheet.create({
    plantCardFull: {
        marginBottom: 15,
        flex: 1,
        borderRadius: 8,
        overflow: 'hidden',
        ...Platform.select({
            ios: {
                shadowColor: '#000',
                shadowOpacity: 0.12,
                shadowRadius: 5,
                shadowOffset: { width: 0, height: 3 },
            },
            android: {
                elevation: 3,
            },
        }),
    },
    fullImageContainer: {
        width: '100%',
        position: 'relative',
    },
    fullImage: {
        width: '100%',
        aspectRatio: 3 / 4,
    },
    plantPlaceholder: {
        width: '100%',
        height: undefined,
        aspectRatio: 3 / 4,
        backgroundColor: '#eee',
        justifyContent: 'center',
        alignItems: 'center',
    },
    fullImageOverlay: {
        ...StyleSheet.absoluteFillObject,
        justifyContent: 'flex-end',
    },
});
