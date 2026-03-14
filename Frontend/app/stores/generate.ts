import { defineStore } from 'pinia'
import type { MovieSummaryDto } from './movie'

const STORAGE_KEY = 'ai_picks'

interface StoredPicks {
  date: string          // 'YYYY-MM-DD'
  movies: MovieSummaryDto[]
}

function normalizeMovie(raw: any): MovieSummaryDto {
  return {
    id: raw?.id ?? raw?.Id ?? 0,
    title: raw?.title ?? raw?.Title ?? '',
    posterUrl: raw?.posterUrl ?? raw?.PosterUrl ?? null,
    releaseDate: raw?.releaseDate ?? raw?.ReleaseDate ?? null,
    averageRating: raw?.averageRating ?? raw?.AverageRating ?? null,
    reviewCount: raw?.reviewCount ?? raw?.ReviewCount ?? 0,
    genres: (raw?.genres ?? raw?.Genres ?? []).map((g: any) => ({
      id: g?.id ?? g?.Id ?? 0,
      name: g?.name ?? g?.Name ?? '',
    })),
  }
}

function normalizeMovies(raw: any): MovieSummaryDto[] {
  if (!Array.isArray(raw)) return []
  return raw.map(normalizeMovie).filter(m => m.id > 0 && m.title.length > 0)
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
    // Load from localStorage; fetch fresh if missing or from a previous day.
    // When a valid cache exists, re-fetch the same movie IDs so ratings/details stay current.
    async loadPicks() {
      if (!import.meta.client) return

      const raw = localStorage.getItem(STORAGE_KEY)
      if (raw) {
        try {
          const stored: StoredPicks = JSON.parse(raw)
          if (stored.date === todayString() && stored.movies.length > 0) {
            // Show cached data immediately, then refresh in the background
            this.picks = stored.movies
            this.refreshCachedPicks(stored.movies.map(m => m.id))
            return
          }
        } catch { /* corrupted — fall through to fetch */ }
      }

      await this.fetchFresh()
    },

    // Re-fetch fresh MovieSummaryDto data for the given IDs and update the cache
    async refreshCachedPicks(ids: number[]) {
      if (!ids.length) return
      const { apiFetch } = useApi()
      try {
        const raw = await apiFetch<any[]>(`/movie/batch?ids=${ids.join(',')}`)
        const fresh = normalizeMovies(raw)
        if (fresh.length > 0) {
          this.picks = fresh
          this.persistPicks()
        }
      } catch { /* keep showing cached data */ }
    },

    // Persist the current in-memory picks to localStorage
    persistPicks() {
      if (!import.meta.client || !this.picks.length) return
      const stored: StoredPicks = { date: todayString(), movies: this.picks }
      localStorage.setItem(STORAGE_KEY, JSON.stringify(stored))
    },

    // Always fetch a new set and persist it
    async fetchFresh() {
      const { apiFetch } = useApi()
      this.isGenerating = true
      try {
        const raw = await apiFetch<any[]>('/generate')
        this.picks = normalizeMovies(raw)
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
