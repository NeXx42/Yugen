export const Languages = {
    eng: "English",
    spa: "Spanish",
    fra: "French",
    deu: "German",
    ita: "Italian",
    por: "Portuguese",
    nld: "Dutch",
    swe: "Swedish",
    nor: "Norwegian",
    dan: "Danish",
    fin: "Finnish",
    pol: "Polish",
    rus: "Russian",
    tur: "Turkish",
    ara: "Arabic",
    zho: "Chinese",
    jpn: "Japanese",
    kor: "Korean"
} as const;

export type LanguageCode = keyof typeof Languages;
export const LanguageCodes = Object.keys(Languages) as LanguageCode[];