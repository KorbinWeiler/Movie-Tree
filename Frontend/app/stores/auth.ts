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

function normalizeAuthResponse(raw: any): AuthResponse {
  return {
    token: raw?.token ?? raw?.Token ?? '',
    expiresAt: raw?.expiresAt ?? raw?.ExpiresAt ?? '',
  }
}

function normalizeMeResponse(raw: any): MeResponse {
  return {
    id: raw?.id ?? raw?.Id ?? '',
    userName: raw?.userName ?? raw?.UserName ?? '',
    email: raw?.email ?? raw?.Email ?? '',
    role: raw?.role ?? raw?.Role ?? '',
    profilePictureUrl: raw?.profilePictureUrl ?? raw?.ProfilePictureUrl ?? null,
  }
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: null as string | null,
    expiresAt: null as string | null,
    user: null as MeResponse | null,
  }),

  getters: {
    isLoggedIn: (state) => !!state.token && new Date(state.expiresAt ?? 0) > new Date(),
    isAdmin: (state) => state.user?.role === 'Admin',
  },

  actions: {
    // Persist token to localStorage on client
    init() {
      if (import.meta.client) {
        this.token = localStorage.getItem('auth_token')
        this.expiresAt = localStorage.getItem('auth_expires')
      }
    },

    async login(email: string, password: string) {
      const config = useRuntimeConfig()
      const raw = await $fetch<any>(`${config.public.apiBase}/auth/login`, {
        method: 'POST',
        body: { email, password },
      })
      const res = normalizeAuthResponse(raw)
      this._setToken(res)
      await this.fetchMe()
    },

    async register(username: string, email: string, password: string) {
      const config = useRuntimeConfig()
      const raw = await $fetch<any>(`${config.public.apiBase}/auth/register`, {
        method: 'POST',
        body: { username, email, password },
      })
      const res = normalizeAuthResponse(raw)
      this._setToken(res)
      await this.fetchMe()
    },

    async fetchMe() {
      if (!this.token) return
      try {
        const config = useRuntimeConfig()
        const raw = await $fetch<any>(`${config.public.apiBase}/auth/me`, {
          headers: { Authorization: `Bearer ${this.token}` },
        })
        this.user = normalizeMeResponse(raw)
      } catch (e: any) {
        const status = e?.statusCode ?? e?.response?.status
        if (status === 401) {
          this.logout()
          return
        }

        // Keep token on transient/non-auth failures so navigation is not blocked.
        this.user = null
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
      if (!res.token || !res.expiresAt) {
        throw new Error('Invalid auth response payload.')
      }

      this.token = res.token
      this.expiresAt = res.expiresAt
      if (import.meta.client) {
        localStorage.setItem('auth_token', res.token)
        localStorage.setItem('auth_expires', res.expiresAt)
      }
    },
  },
})
