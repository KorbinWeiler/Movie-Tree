import vuetify, { transformAssetUrls } from 'vite-plugin-vuetify'

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },
  css: ['~/assets/main.css'],
  build: {
    transpile: ['vuetify'],
  },
  vite: {
    plugins: [vuetify({ autoImport: true }) as any],
    vue: {
      template: {
        transformAssetUrls,
      },
    },
  },
})
