export const UserSettingTypes = {
    PreferredAudioLanguage: "PreferredAudioLanguage",
    PreferredSubtitleLanguage: "PreferredSubtitleLanguage"
} as const;

export type UserSettingTypes = typeof UserSettingTypes[keyof typeof UserSettingTypes];