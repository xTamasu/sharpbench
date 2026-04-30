/** tailwind.config.js
 *  Tailwind CSS configuration. Scans the index.html and all source files
 *  for class names. */
/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {},
  },
  plugins: [],
};
