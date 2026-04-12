import sq from './sq.js'
import en from './en.js'

export const translations = { sq, en }

export const t = (lang, key) => translations[lang]?.[key] || key
