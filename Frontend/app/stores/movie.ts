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
      try {
        const raw = await apiFetch<any[]>('/movie/temp-trending')
        this.trending = normalizeMovies(raw)
      } catch { /* API unavailable — keep empty array */ }
    },

    async search(q: string, genreId?: number) {
      const { apiFetch } = useApi()
      const params = new URLSearchParams({ pageSize: '50' })
      if (q) params.set('q', q)
      if (genreId) params.set('genreId', String(genreId))
      try {
        const raw = await apiFetch<any[]>(`/movie?${params}`)
        this.searchResults = normalizeMovies(raw)
      } catch { this.searchResults = [] }
    },

    async fetchGenres() {
      const { apiFetch } = useApi()
      try {
        this.genres = await apiFetch<GenreDto[]>('/movie/genres')
      } catch { /* keep empty */ }
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
