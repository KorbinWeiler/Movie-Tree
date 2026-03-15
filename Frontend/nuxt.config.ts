import vuetify, { transformAssetUrls } from 'vite-plugin-vuetify'

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },
  modules: ['@pinia/nuxt', '@vueuse/nuxt'],
  // Ensure Nuxt auto-registers components from the `app/components` directory
  components: [
    {
      path: '~/app/components',
      pathPrefix: false,
      extensions: ['.vue'],
    },
  ],
  css: ['~/assets/main.css'],
  runtimeConfig: {
    public: {
        apiBase: (globalThis as any).process?.env?.NUXT_PUBLIC_API_BASE ?? 'https://localhost:7133/api',
    },
  },
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
