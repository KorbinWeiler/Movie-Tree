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
        this.trending = await apiFetch<MovieSummaryDto[]>('/movie/temp-trending')
      } catch { /* API unavailable — keep empty array */ }
    },

    async search(q: string, genreId?: number) {
      const { apiFetch } = useApi()
      const params = new URLSearchParams()
      if (q) params.set('q', q)
      if (genreId) params.set('genreId', String(genreId))
      try {
        this.searchResults = await apiFetch<MovieSummaryDto[]>(`/movie?${params}`)
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
      } catch { this.currentMovie = null }
      return this.currentMovie
    },
  },
})
