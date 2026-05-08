/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Components/**/*.razor",
    "./Components/**/*.cs",
    "./node_modules/flowbite/**/*.js"
  ],
  darkMode: 'class',
  theme: {
    extend: {
      fontFamily: {
        sans: ['"Host Grotesk"', 'sans-serif'],
        serif: ['"Host Grotesk"', 'Georgia', 'serif'],
      },
      colors: {
        brand: {
          50: '#e6ebf5',
          100: '#ccd7eb',
          200: '#99afd6',
          300: '#6687c2',
          400: '#335fad',
          500: '#003799',
          600: '#002c7a',
          700: '#00215c',
          800: '#00163d',
          900: '#000b1f',
          950: '#00155b',
        },
        cyan: {
          100: '#E0F4FF',
          200: '#3DC5FF',
          500: '#3AC0FF',
          600: '#136EB3',
          700: '#093E8A',
        },
        ink: {
          DEFAULT: '#101828',
          soft:    '#1E2939',
          deep:    '#000F5C',
          muted:   '#99A1AF',
          subtle:  '#959595',
          faint:   '#666666',
        },
        surface: {
          DEFAULT: '#FFFFFF',
          subtle:  '#F9FAFB',
          info:    '#F0FAFF',
          card:    '#001C40',
        },
        line: {
          DEFAULT: '#E5E7EB',
          subtle:  '#F3F4F6',
        },
        success: {
          100: 'rgba(34,197,94,0.2)',
          600: '#16A34A',
        },
      },
      fontSize: {
        'display-xl': ['64px', { lineHeight: '72px', letterSpacing: '-0.02em' }],
        'display-lg': ['48px', { lineHeight: '56px', letterSpacing: '-0.02em' }],
        'h2':         ['48px', { lineHeight: '56px', letterSpacing: '-0.02em' }],
        'h3':         ['36px', { lineHeight: '44px' }],
        'h4':         ['24px', { lineHeight: '32px' }],
        'body-lg':    ['18px', { lineHeight: '28px' }],
        'body':       ['16px', { lineHeight: '24px' }],
        'body-sm':    ['14px', { lineHeight: '20px' }],
        'caption':    ['12px', { lineHeight: '16px', letterSpacing: '0.1em' }],
      },
      spacing: {
        'section-y':   '96px',
        'section-x':   '80px',
        'section-gap': '64px',
      },
      borderRadius: {
        'base': '24px',
      },
    },
  },
  plugins: [
    require('flowbite/plugin'),
  ],
};
