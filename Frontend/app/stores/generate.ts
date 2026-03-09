import { defineStore } from 'pinia'
import type { MovieSummaryDto } from './movie'

const STORAGE_KEY = 'ai_picks'

interface StoredPicks {
  date: string          // 'YYYY-MM-DD'
  movies: MovieSummaryDto[]
}

function todayString() {
  return new Date().toISOString().slice(0, 10)
}

export const useGenerateStore = defineStore('generate', {
  state: () => ({
    picks: [] as MovieSummaryDto[],
    isGenerating: false,
  }),

  actions: {
    // Load from localStorage; fetch fresh if missing or from a previous day
    async loadPicks() {
      if (!import.meta.client) return

      const raw = localStorage.getItem(STORAGE_KEY)
      if (raw) {
        try {
          const stored: StoredPicks = JSON.parse(raw)
          if (stored.date === todayString() && stored.movies.length > 0) {
            this.picks = stored.movies
            return
          }
        } catch { /* corrupted — fall through to fetch */ }
      }

      await this.fetchFresh()
    },

    // Always fetch a new set and persist it
    async fetchFresh() {
      const { apiFetch } = useApi()
      this.isGenerating = true
      try {
        this.picks = await apiFetch<MovieSummaryDto[]>('/generate')
        if (import.meta.client) {
          const stored: StoredPicks = { date: todayString(), movies: this.picks }
          localStorage.setItem(STORAGE_KEY, JSON.stringify(stored))
        }
      } finally {
        this.isGenerating = false
      }
    },
  },
})
