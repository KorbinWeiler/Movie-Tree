import { defineStore } from 'pinia'

export interface GenreDto {
  id: number
  name: string
}

export interface MovieSummaryDto {
  id: number
  title: string
  posterUrl: string | null
  releaseDate: string | null
  averageRating: number | null
  reviewCount: number
  genres: GenreDto[]
}

export interface MovieDetailDto extends MovieSummaryDto {
  description: string | null
  runtimeMinutes: number | null
  isVisible: boolean
}

function logBadMoviePayload(raw: any, source = 'movie.ts') {
  try {
    const titleValue = raw?.title ?? raw?.Title
    const ratingValue = raw?.averageRating ?? raw?.AverageRating
    const badTitle = titleValue != null && typeof titleValue !== 'string' && typeof titleValue !== 'number'
    const badRating = ratingValue != null && typeof ratingValue !== 'number' && typeof ratingValue !== 'string'
    if (badTitle || badRating) {
      console.warn(`[${source}] Unexpected movie payload types`, { badTitle, badRating, raw })
    }
  } catch (e) {
    console.warn('[movie.ts] Failed to inspect movie payload', e)
  }
}

function toSafeTitle(raw: any): string {
  const value = raw?.title ?? raw?.Title
  if (typeof value === 'string') return value.trim()
  if (typeof value === 'number') return String(value)
  if (value && typeof value === 'object') {
    // Handle occasional translated-title objects from external sources.
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
  logBadMoviePayload(raw, 'movie.normalizeMovie')
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

export const useMovieStore = defineStore('movie', {
  state: () => ({
    trending: [] as MovieSummaryDto[],
    searchResults: [] as MovieSummaryDto[],
    genres: [] as GenreDto[],
    currentMovie: null as MovieDetailDto | null,
    searchQuery: '',
    selectedGenreId: null as number | null,
  }),

  actions: {
    async fetchTrending() {
      const { apiFetch } = useApi()
      const desired = 30
      try {
        // Prefer the real trending endpoint first
        const raw = await apiFetch<any[]>(`/movie/trending?count=${desired}`)
        let movies = normalizeMovies(raw)

        // If the real trending endpoint returned fewer than requested,
        // fill the remainder with random movies from the temp endpoint.
        if (movies.length < desired) {
          try {
            const remaining = desired - movies.length
            const fallbackRaw = await apiFetch<any[]>(`/movie/temp-trending?count=${remaining}`)
            const fallback = normalizeMovies(fallbackRaw).filter(m => !movies.some(x => x.id === m.id))
            movies = movies.concat(fallback)
          } catch (inner) {
            // Ignore fallback errors and use whatever we have from trending
            console.warn('[movie] fetchTrending fallback failed:', inner)
          }
        }

        this.trending = movies
      } catch (e) { console.error('[movie] fetchTrending failed:', e) }
    },

    async search(q: string, genreId?: number) {
      const { apiFetch } = useApi()
      const params = new URLSearchParams({ pageSize: '50' })
      if (q) params.set('q', q)
      if (genreId) params.set('genreId', String(genreId))
      try {
        const raw = await apiFetch<any[]>(`/movie?${params}`)
        this.searchResults = normalizeMovies(raw)
      } catch (e) { console.error('[movie] search failed:', e); this.searchResults = [] }
    },

    async fetchGenres() {
      const { apiFetch } = useApi()
      try {
        this.genres = await apiFetch<GenreDto[]>('/movie/genres')
      } catch (e) { console.error('[movie] fetchGenres failed:', e) }
    },

    async fetchById(id: number) {
      const { apiFetch } = useApi()
      try {
        this.currentMovie = await apiFetch<MovieDetailDto>(`/movie/${id}`)
        // Backfill posterUrl on any card already in search/trending/ai-picks lists
        if (this.currentMovie?.posterUrl) {
          const poster = this.currentMovie.posterUrl
          const sr = this.searchResults.find(m => m.id === id)
          if (sr) sr.posterUrl = poster
          const tr = this.trending.find(m => m.id === id)
          if (tr) tr.posterUrl = poster
          const generateStore = useGenerateStore()
          const gp = generateStore.picks.find(m => m.id === id)
          if (gp) {
            gp.posterUrl = poster
            generateStore.persistPicks()
          }
        }
      } catch { this.currentMovie = null }
      return this.currentMovie
    },

    async setMovieVisibility(id: number, isVisible: boolean) {
      const { apiFetch } = useApi()
      await apiFetch(`/admin/movie/${id}/visibility`, {
        method: 'PATCH',
        body: { isVisible },
      })
      if (!isVisible) {
        this.trending = this.trending.filter(m => m.id !== id)
        this.searchResults = this.searchResults.filter(m => m.id !== id)
      }
      if (this.currentMovie?.id === id) this.currentMovie = null
    },

    async fetchHiddenMovies(q?: string, page = 1) {
      const { apiFetch } = useApi()
      const params = new URLSearchParams({ page: String(page), pageSize: '30' })
      if (q) params.set('q', q)
      return apiFetch<{ total: number; movies: { id: number; title: string; posterUrl: string | null; releaseDate: string | null }[] }>(
        `/admin/hidden-movies?${params}`
      )
    },
  },
})
