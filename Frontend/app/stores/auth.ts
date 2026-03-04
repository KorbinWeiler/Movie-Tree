import { defineStore } from 'pinia'

interface AuthResponse {
  token: string
  expiresAt: string
}

interface MeResponse {
  id: string
  userName: string
  email: string
  role: string
  profilePictureUrl: string | null
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: null as string | null,
    expiresAt: null as string | null,
    user: null as MeResponse | null,
  }),

  getters: {
    isLoggedIn: (state) => !!state.token && new Date(state.expiresAt ?? 0) > new Date(),
  },

  actions: {
    // Persist token to localStorage on client
    init() {
      if (import.meta.client) {
        this.token = localStorage.getItem('auth_token')
        this.expiresAt = localStorage.getItem('auth_expires')
      }
    },

    async login(username: string, password: string) {
      const config = useRuntimeConfig()
      const res = await $fetch<AuthResponse>(`${config.public.apiBase}/auth/login`, {
        method: 'POST',
        body: { username, password },
      })
      this._setToken(res)
      await this.fetchMe()
    },

    async register(username: string, email: string, password: string) {
      const config = useRuntimeConfig()
      const res = await $fetch<AuthResponse>(`${config.public.apiBase}/auth/register`, {
        method: 'POST',
        body: { username, email, password },
      })
      this._setToken(res)
      await this.fetchMe()
    },

    async fetchMe() {
      if (!this.token) return
      try {
        const config = useRuntimeConfig()
        this.user = await $fetch<MeResponse>(`${config.public.apiBase}/auth/me`, {
          headers: { Authorization: `Bearer ${this.token}` },
        })
      } catch {
        this.logout()
      }
    },

    logout() {
      this.token = null
      this.expiresAt = null
      this.user = null
      if (import.meta.client) {
        localStorage.removeItem('auth_token')
        localStorage.removeItem('auth_expires')
      }
    },

    _setToken(res: AuthResponse) {
      this.token = res.token
      this.expiresAt = res.expiresAt
      if (import.meta.client) {
        localStorage.setItem('auth_token', res.token)
        localStorage.setItem('auth_expires', res.expiresAt)
      }
    },
  },
})
