import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import '@mdi/font/css/materialdesignicons.css'
import 'vuetify/styles'

export default defineNuxtPlugin((nuxtApp) => {
  const vuetify = createVuetify({
    components,
    directives,
    icons: {
      defaultSet: 'mdi',
    },
    theme: {
      defaultTheme: 'dark',
      themes: {
        dark: {
          dark: true,
          colors: {
            primary: '#FFD147',
            'primary-hover': '#FFE070',
            'primary-pressed': '#E6B800',
            background: '#0F1117',
            surface: '#1A1D25',
            error: '#E05656',
            success: '#4DB87A',
            warning: '#F79B3E',
            info: '#5B8FD4',
          },
        },
      },
    },
  })

  nuxtApp.vueApp.use(vuetify)
})
