import { defineStore } from 'pinia'
import type { MovieSummaryDto } from './movie'

export interface PickedMovieDto {
  position: number
  movie: MovieSummaryDto
}

export interface GeneratedPickDto {
  aiPickListId: number
  movies: PickedMovieDto[]
}

export type GenerationMode = 'AllHistory' | 'Selected' | 'Genre' | 'FullAI'

export const useGenerateStore = defineStore('generate', {
  state: () => ({
    aiPicks: null as GeneratedPickDto | null,         // home page global picks
    lastGenerated: null as GeneratedPickDto | null,   // user's last generated list
    isGenerating: false,
  }),

  actions: {
    async fetchAiPicks() {
      const { apiFetch } = useApi()
      try {
        this.aiPicks = await apiFetch<GeneratedPickDto>('/generate/ai-picks')
      } catch { /* keep null */ }
    },

    async generate(mode: GenerationMode, movieIds?: number[], genreId?: number) {
      const { apiFetch } = useApi()
      this.isGenerating = true
      try {
        this.lastGenerated = await apiFetch<GeneratedPickDto>('/generate', {
          method: 'POST',
          body: { mode, movieIds: movieIds ?? null, genreId: genreId ?? null },
        })
      } finally {
        this.isGenerating = false
      }
      return this.lastGenerated
    },
  },
})
