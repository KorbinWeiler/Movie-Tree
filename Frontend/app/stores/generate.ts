import { defineStore } from 'pinia'
import type { MovieSummaryDto } from './movie'

const STORAGE_KEY = 'ai_picks'

interface StoredPicks {
  date: string          // 'YYYY-MM-DD'
  movies: MovieSummaryDto[]
}

function toSafeTitle(raw: any): string {
  const value = raw?.title ?? raw?.Title
  if (typeof value === 'string') return value.trim()
  if (typeof value === 'number') return String(value)
  if (value && typeof value === 'object') {
    const nested = value.title ?? value.name ?? value.text ?? value.originalTitle ?? value.english
    if (typeof nested === 'string') return nested.trim()
  }
  return ''
}

function toSafeNumber(value: unknown): number | null {
  if (typeof value === 'number') return Number.isFinite(value) ? value : null
  if (typeof value === 'string') {
    const parsed = Number(value)
    return Number.isFinite(parsed) ? parsed : null
  }
  return null
}

function normalizeMovie(raw: any): MovieSummaryDto {
  try {
    const titleValue = raw?.title ?? raw?.Title
    const ratingValue = raw?.averageRating ?? raw?.AverageRating
    const badTitle = titleValue != null && typeof titleValue !== 'string' && typeof titleValue !== 'number'
    const badRating = ratingValue != null && typeof ratingValue !== 'number' && typeof ratingValue !== 'string'
    if (badTitle || badRating) {
      console.warn('[generate.normalizeMovie] Unexpected movie payload types', { badTitle, badRating, raw })
    }
  } catch (e) {
    console.warn('[generate.normalizeMovie] failed to inspect payload', e)
  }
  const title = toSafeTitle(raw)
  const averageRating = toSafeNumber(raw?.averageRating ?? raw?.AverageRating)
  const reviewCount = toSafeNumber(raw?.reviewCount ?? raw?.ReviewCount) ?? 0
  return {
    id: raw?.id ?? raw?.Id ?? 0,
    title,
    posterUrl: raw?.posterUrl ?? raw?.PosterUrl ?? null,
    releaseDate: raw?.releaseDate ?? raw?.ReleaseDate ?? null,
    averageRating,
    reviewCount,
    genres: (raw?.genres ?? raw?.Genres ?? []).map((g: any) => ({
      id: g?.id ?? g?.Id ?? 0,
      name: g?.name ?? g?.Name ?? '',
    })),
  }
}

function normalizeMovies(raw: any): MovieSummaryDto[] {
  if (!Array.isArray(raw)) return []
  return raw
    .map(normalizeMovie)
    .filter(m => m.id > 0 && m.title.trim().length > 0)
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
      if (!import.meta.client) {
        await this.fetchFresh()
        return
      }

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

    setPicks(movies: MovieSummaryDto[]) {
      this.picks = movies
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
      } catch (e) { console.error('[generate] refreshCachedPicks failed:', e) }
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
      } catch (e) {
        console.error('[generate] fetchFresh failed:', e)
      } finally {
        this.isGenerating = false
      }
    },
  },
})
