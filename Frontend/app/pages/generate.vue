<template>
  <v-container fluid class="pa-6">
    <div class="text-center mb-6">
      <h2 class="text-h5 font-weight-bold mb-2">Your Recommendations</h2>
      <p class="text-body-2 page-subtitle">Choose a generation type, then click Generate.</p>
    </div>

    <div class="controls-wrap mb-8">
      <v-select
        v-model="generationType"
        :items="generationOptions"
        item-title="label"
        item-value="value"
        label="Generation Type"
        density="compact"
        variant="outlined"
        hide-details
      />

      <v-autocomplete
        v-if="generationType === 'selected'"
        v-model="selectedMovieIds"
        v-model:search="movieSearch"
        :items="movieOptions"
        item-title="title"
        item-value="id"
        chips
        closable-chips
        multiple
        clearable
        label="Select Movies"
        density="compact"
        variant="outlined"
        hide-details
        :loading="isSearchingMovies"
      />

      <v-btn
        color="primary"
        rounded="pill"
        :loading="generateStore.isGenerating"
        :disabled="generationType === 'selected' && selectedMovieIds.length === 0"
        class="generate-btn"
        @click="generateNow"
      >
        Generate
      </v-btn>
    </div>

    <v-row v-if="generateStore.picks.length" dense>
      <v-col
        v-for="(movie, index) in generateStore.picks"
        :key="movie.id"
        cols="6"
        sm="4"
        md="3"
        lg="2"
      >
        <div class="position-relative">
          <div class="result-number">{{ index + 1 }}</div>
          <MovieCard :movie="movie" />
        </div>
      </v-col>
    </v-row>

    <v-row v-else-if="generateStore.isGenerating" dense>
      <v-col v-for="n in 9" :key="n" cols="6" sm="4" md="3" lg="2">
        <v-skeleton-loader type="card" rounded="lg" />
      </v-col>
    </v-row>

    <div v-else class="d-flex flex-column align-center justify-center py-16 text-center">
      <v-icon size="72" style="color: rgba(var(--v-theme-on-surface), 0.15)" class="mb-4">
        mdi-creation-outline
      </v-icon>
      <h3 class="text-h6 font-weight-semibold mb-2">No recommendations yet</h3>
      <p class="text-body-2" style="color: rgba(var(--v-theme-on-surface), 0.5); max-width: 340px;">
        Pick a mode and click Generate.
      </p>
    </div>
  </v-container>
</template>

<script setup lang="ts">
import { MovieCard } from '@/components'

definePageMeta({ middleware: 'auth' })

interface GenreDto {
  id: number
  name: string
}

interface MovieSummaryDto {
  id: number
  title: string
  posterUrl: string | null
  releaseDate: string | null
  averageRating: number | null
  reviewCount: number
  genres: GenreDto[]
}

type GenerationType = 'all' | 'selected' | 'ai'

const generateStore = useGenerateStore()
const { apiFetch } = useApi()

// Start empty on page load; user must explicitly click Generate.
generateStore.setPicks([])

const generationType = ref<GenerationType>('all')
const generationOptions = [
  { label: 'All Movies', value: 'all' },
  { label: 'Select Movies', value: 'selected' },
  { label: 'AI Generated', value: 'ai' },
]

const selectedMovieIds = ref<number[]>([])
const movieSearch = ref('')
const movieOptions = ref<MovieSummaryDto[]>([])
const isSearchingMovies = ref(false)

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

let searchTimer: ReturnType<typeof setTimeout> | null = null

watch(movieSearch, (value) => {
  if (searchTimer) clearTimeout(searchTimer)
  searchTimer = setTimeout(async () => {
    const q = value?.trim() ?? ''
    if (q.length < 2) {
      movieOptions.value = []
      return
    }

    isSearchingMovies.value = true
    try {
      const raw = await apiFetch<any[]>(`/movie?q=${encodeURIComponent(q)}&pageSize=30`)
      movieOptions.value = normalizeMovies(raw)
    } catch {
      movieOptions.value = []
    } finally {
      isSearchingMovies.value = false
    }
  }, 300)
})

async function generateNow() {
  if (generationType.value === 'all' || generationType.value === 'ai') {
    generateStore.isGenerating = true
    try {
      const raw = await apiFetch<any[]>('/generate')
      generateStore.setPicks(normalizeMovies(raw))
    } finally {
      generateStore.isGenerating = false
    }
    return
  }

  if (selectedMovieIds.value.length === 0) return

  generateStore.isGenerating = true
  try {
    const raw = await apiFetch<any[]>(`/movie/batch?ids=${selectedMovieIds.value.join(',')}`)
    generateStore.setPicks(normalizeMovies(raw))
  } finally {
    generateStore.isGenerating = false
  }
}
</script>

<style scoped>
.page-subtitle {
  color: rgba(var(--v-theme-on-surface), 0.55);
}

.controls-wrap {
  max-width: 560px;
  margin: 0 auto;
  display: grid;
  gap: 12px;
}

.generate-btn {
  text-transform: none;
  font-weight: 600;
  color: rgb(var(--v-theme-on-primary));
}

.result-number {
  position: absolute;
  top: 6px;
  left: 6px;
  z-index: 1;
  background: rgba(var(--v-theme-primary), 0.85);
  color: rgb(var(--v-theme-on-primary));
  width: 22px;
  height: 22px;
  border-radius: 50%;
  font-size: 11px;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>
