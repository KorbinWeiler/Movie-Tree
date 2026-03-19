import { defineStore } from 'pinia'
import { normalizeApiBase } from '../utils/apiBase'

let restoreSessionPromise: Promise<void> | null = null

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
    sessionRestored: false,
    isRestoringSession: false,
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

    async restoreSession(force = false) {
      if (!import.meta.client) return

      if (force) {
        this.sessionRestored = false
        restoreSessionPromise = null
      }

      if (this.sessionRestored && !force) return

      if (restoreSessionPromise) {
        await restoreSessionPromise
        return
      }

      restoreSessionPromise = (async () => {
        this.isRestoringSession = true

        try {
          this.init()

          if (this.isLoggedIn) {
            await this.fetchMe()
          } else {
            this.user = null
          }
        } finally {
          this.sessionRestored = true
          this.isRestoringSession = false
          restoreSessionPromise = null
        }
      })()

      await restoreSessionPromise
    },

    async login(email: string, password: string) {
      const config = useRuntimeConfig()
      const apiBase = normalizeApiBase(config.public.apiBase)
      const raw = await $fetch<any>(`${apiBase}/auth/login`, {
        method: 'POST',
        body: { email, password },
      })
      const res = normalizeAuthResponse(raw)
      this._setToken(res)
      await this.fetchMe()
      this.sessionRestored = true
    },

    async register(username: string, email: string, password: string) {
      const config = useRuntimeConfig()
      const apiBase = normalizeApiBase(config.public.apiBase)
      const raw = await $fetch<any>(`${apiBase}/auth/register`, {
        method: 'POST',
        body: { username, email, password },
      })
      const res = normalizeAuthResponse(raw)
      this._setToken(res)
      await this.fetchMe()
      this.sessionRestored = true
    },

    async fetchMe() {
      if (!this.token) {
        this.user = null
        return
      }

      try {
        const config = useRuntimeConfig()
        const apiBase = normalizeApiBase(config.public.apiBase)
        const raw = await $fetch<any>(`${apiBase}/auth/me`, {
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
      this.sessionRestored = true
      this.isRestoringSession = false
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
