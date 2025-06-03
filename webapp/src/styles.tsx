import {
    BrandVariants,
    GriffelStyle,
    Theme,
    createDarkTheme,
    createLightTheme,
    makeStyles,
    shorthands,
    themeToTokensObject,
    tokens,
} from '@fluentui/react-components';

export const retroCyberBrandRamp: BrandVariants = {
    10: '#0A0A0F', // Deep dark purple-black
    20: '#1A0D2E', // Dark purple
    30: '#2D1B4E', // Deep violet
    40: '#4A2B7C', // Purple
    50: '#6A3D9A', // Bright purple
    60: '#8B4DBA', // Light purple
    70: '#AC60D8', // Neon purple
    80: '#CD74F6', // Bright neon purple
    90: '#E388FF', // Electric purple
    100: '#F09CFF', // Pink-purple
    110: '#FF6EC7', // Hot pink
    120: '#FF40A6', // Magenta
    130: '#FF1285', // Electric magenta
    140: '#FF4A9B', // Bright magenta
    150: '#FF82C4', // Light magenta
    160: '#FFBAED', // Very light magenta
};

export const retroCyberLightTheme: Theme & { colorMeBackground: string } = {
    ...createLightTheme(retroCyberBrandRamp),
    colorMeBackground: '#F0F8FF', // Light cyber blue
};

export const retroCyberDarkTheme: Theme & { colorMeBackground: string } = {
    ...createDarkTheme(retroCyberBrandRamp),
    colorMeBackground: '#1A0D2E', // Deep dark purple
};

export const customTokens = themeToTokensObject(retroCyberDarkTheme);

export const Breakpoints = {
    small: (style: GriffelStyle): Record<string, GriffelStyle> => {
        return { '@media (max-width: 744px)': style };
    },
};

export const ScrollBarStyles: GriffelStyle = {
    overflowY: 'auto',
    '&:hover': {
        '&::-webkit-scrollbar-thumb': {
            backgroundColor: tokens.colorScrollbarOverlay,
            visibility: 'visible',
        },
        '&::-webkit-scrollbar-track': {
            backgroundColor: tokens.colorNeutralBackground1,
            WebkitBoxShadow: 'inset 0 0 5px rgba(0, 0, 0, 0.1)',
            visibility: 'visible',
        },
    },
};

export const SharedStyles: Record<string, GriffelStyle> = {
    scroll: {
        height: '100%',
        ...ScrollBarStyles,
    },
    overflowEllipsis: {
        ...shorthands.overflow('hidden'),
        textOverflow: 'ellipsis',
        whiteSpace: 'nowrap',
    },
    cyberGlow: {
        boxShadow: '0 0 20px rgba(138, 77, 186, 0.5), 0 0 40px rgba(138, 77, 186, 0.3)',
        border: '1px solid rgba(138, 77, 186, 0.6)',
    },
    cyberButton: {
        background: 'linear-gradient(45deg, #4A2B7C, #6A3D9A)',
        border: '2px solid #8B4DBA',
        boxShadow: '0 0 15px rgba(138, 77, 186, 0.4)',
        transition: 'all 0.3s ease',
        '&:hover': {
            boxShadow: '0 0 25px rgba(138, 77, 186, 0.7)',
            transform: 'translateY(-2px)',
        },
    },
    retroFont: {
        fontFamily: '"Courier New", "Roboto Mono", monospace',
        textShadow: '0 0 10px rgba(255, 110, 199, 0.5)',
    },
};

export const useSharedClasses = makeStyles({
    informativeView: {
        display: 'flex',
        flexDirection: 'column',
        ...shorthands.padding('80px'),
        alignItems: 'center',
        ...shorthands.gap(tokens.spacingVerticalXL),
        marginTop: tokens.spacingVerticalXXXL,
    },
});

export const useDialogClasses = makeStyles({
    surface: {
        paddingRight: tokens.spacingVerticalXS,
    },
    content: {
        display: 'flex',
        flexDirection: 'column',
        ...shorthands.overflow('hidden'),
        width: '100%',
    },
    paragraphs: {
        marginTop: tokens.spacingHorizontalS,
    },
    innerContent: {
        height: '100%',
        ...SharedStyles.scroll,
        paddingRight: tokens.spacingVerticalL,
    },
    text: {
        whiteSpace: 'pre-wrap',
        textOverflow: 'wrap',
    },
    footer: {
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'flex-start',
        minWidth: '175px',
    },
});
