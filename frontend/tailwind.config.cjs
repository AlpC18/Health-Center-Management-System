/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: 'class',
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        health: {
          bg: 'var(--health-bg)',
          surface: 'var(--health-surface)',
          hover: 'var(--health-hover)',
          brand: 'var(--health-brand)',
          primary: 'var(--health-primary)',
          secondary: 'var(--health-secondary)',
          border: 'var(--health-border)',
          accent: 'var(--health-accent)',
        }
      }
    },
  },
  plugins: [],
}
