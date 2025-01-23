// FullSizePlantCard.tsx

import React, { useState, useCallback } from 'react';
import {
    View,
    Image,
    StyleSheet,
    Platform,
    LayoutChangeEvent,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { COLORS } from '../../../theme/colors';
import { PlantOverlay } from '../components/PlantOverlay';
import { PlantResponse } from '../../../types/apiTypes';
import { LinearGradient } from 'expo-linear-gradient';

interface PlantCardWithInfoProps {
    plant: PlantResponse;
    compact?: boolean;
    /**
     * If true, the black extension height is dynamic (half the overlay).
     * If false, the black extension uses a fixed height.
     */
    isDynamic?: boolean;
    /**
     * When isDynamic=false, this sets the black extension height (default 120).
     */
    fixedExtensionHeight?: number;
}

export const PlantCardWithInfo: React.FC<PlantCardWithInfoProps> = ({
    plant,
    compact = false,
    isDynamic = true,
    fixedExtensionHeight = 120,
}) => {
    const [overlayHeight, setOverlayHeight] = useState(0);

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

    /**
     * Only measure the overlay if dynamic. We store its height to compute extension.
     */
    const handleOverlayLayout = useCallback(
        (event: LayoutChangeEvent) => {
            if (isDynamic && overlayHeight === 0) { // Only set once
                const { height } = event.nativeEvent.layout;
                setOverlayHeight(height);
            }
        },
        [isDynamic, overlayHeight]
    );

    /**
     * If dynamic, black extension height = half the overlay’s measured height.
     * Otherwise, use the fixed extension height.
     */
    const blackExtensionHeight = isDynamic
        ? overlayHeight / 2
        : fixedExtensionHeight;

    return (
        <View style={styles.cardContainer}>
            {/* ---- Plant Image / Placeholder ---- */}
            <View style={styles.imageContainer}>
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
                <LinearGradient colors={['rgba(0,0,0,0)', 'rgba(0,0,0,1)']} style={[styles.lowerGradient, { height: compact ? overlayHeight : overlayHeight * 1.5}]} />
            </View>

            {/* ---- Black Extension (dynamic or fixed) ---- */}
            <View
                style={[
                    styles.blackExtension,
                    { height: blackExtensionHeight },
                ]}
            />
            <View
                style={styles.overlayWrapper}
                onLayout={handleOverlayLayout}
            >
                <PlantOverlay
                    speciesName={plant.speciesName}
                    description={plant.description}
                    tags={allTags}
                    compact={compact}
                />
            </View>
            {/* 
        ---- Overlay Section ----
        The overlay will measure itself (if dynamic),
      */}
        </View>
    );
};

const styles = StyleSheet.create({
    cardContainer: {
        borderRadius: 8,
        overflow: 'hidden',
        backgroundColor: '#fff',
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
    imageContainer: {
        width: '100%',
    },
    fullImage: {
        width: '100%',
        aspectRatio: 3 / 4,
    },
    plantPlaceholder: {
        width: '100%',
        aspectRatio: 3 / 4,
        backgroundColor: '#eee',
        justifyContent: 'center',
        alignItems: 'center',
    },
    lowerGradient: {
        position: 'absolute',
        bottom: 0,
        width: '100%',
    },

    blackExtension: {
        width: '100%',
        backgroundColor: '#000',
    },
    overlayWrapper: {
        position: 'absolute',
        bottom: 0,
        // We measure this layout if isDynamic=true, so we can
        // compute overlayHeight and adjust the extension accordingly.
        // The negative margin is applied dynamically above.
    },
});
