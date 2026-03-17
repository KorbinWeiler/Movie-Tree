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

      <div v-if="generationType === 'selected'" class="selection-summary text-body-2">
        <span v-if="reviewedMovieOptions.length">
          {{ selectedMovieIds.length }} of {{ reviewedMovieOptions.length }} reviewed movies selected.
        </span>
        <span v-else>
          Review some movies first to use this mode.
        </span>
      </div>

      <v-btn
        color="primary"
        rounded="pill"
        :loading="generateStore.isGenerating"
        :disabled="generationType === 'selected' && reviewedMovieOptions.length === 0"
        class="generate-btn"
        @click="generateNow"
      >
        Generate
      </v-btn>
    </div>

    <v-row v-if="generateStore.picks.length" dense class="results-row">
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

    <v-row v-else-if="generateStore.isGenerating" dense class="results-row">
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

    <v-dialog v-model="isSelectDialogOpen" max-width="560">
      <v-card rounded="xl">
        <v-card-title class="text-h6 font-weight-bold">Select Reviewed Movies</v-card-title>
        <v-card-text>
          <div v-if="reviewedMovieOptions.length" class="d-flex flex-column ga-2">
            <v-list lines="two" class="rounded-lg border-thin">
              <v-list-item
                v-for="movie in reviewedMovieOptions"
                :key="movie.id"
                @click="toggleSelectedMovie(movie.id)"
              >
                <template #prepend>
                  <v-checkbox-btn
                    :model-value="selectedMovieIds.includes(movie.id)"
                    @update:model-value="toggleSelectedMovie(movie.id)"
                  />
                </template>

                <v-list-item-title>{{ movie.title }}</v-list-item-title>
                <v-list-item-subtitle>
                  {{ movie.reviewCount }} review{{ movie.reviewCount === 1 ? '' : 's' }}
                </v-list-item-subtitle>
              </v-list-item>
            </v-list>
          </div>
          <div v-else class="text-body-2" style="color: rgba(var(--v-theme-on-surface), 0.6)">
            You do not have any reviewed movies yet.
          </div>
        </v-card-text>
        <v-card-actions class="px-6 pb-6 pt-0 d-flex justify-end ga-2">
          <v-btn variant="text" rounded="pill" @click="isSelectDialogOpen = false">Cancel</v-btn>
          <v-btn
            color="primary"
            rounded="pill"
            :disabled="selectedMovieIds.length === 0"
            :loading="generateStore.isGenerating"
            @click="generateSelected"
          >
            Generate
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup lang="ts">
import MovieCard from '~/components/MovieCard.vue'

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

interface ReviewedMovieOption {
  id: number
  title: string
  posterUrl: string | null
  reviewCount: number
}

type GenerationType = 'all' | 'selected' | 'ai'

const RECOMMENDATION_COUNT = 30

const generateStore = useGenerateStore()
const userStore = useUserStore()
const { apiFetch } = useApi()

generateStore.setPicks([])

const generationType = ref<GenerationType>('all')
const generationOptions = [
  { label: 'All Movies', value: 'all' },
  { label: 'Select Movies', value: 'selected' },
  { label: 'AI Generated', value: 'ai' },
]

const selectedMovieIds = ref<number[]>([])
const isSelectDialogOpen = ref(false)

function toNumber(value: unknown): number {
  if (typeof value === 'number') return value
  if (typeof value === 'string') {
    const parsed = Number(value)
    return Number.isFinite(parsed) ? parsed : 0
  }
  return 0
}

function normalizeMovie(raw: any): MovieSummaryDto {
  return {
    id: toNumber(raw?.id ?? raw?.Id),
    title: String(raw?.title ?? raw?.Title ?? '').trim(),
    posterUrl: raw?.posterUrl ?? raw?.PosterUrl ?? null,
    releaseDate: raw?.releaseDate ?? raw?.ReleaseDate ?? null,
    averageRating: raw?.averageRating ?? raw?.AverageRating ?? null,
    reviewCount: toNumber(raw?.reviewCount ?? raw?.ReviewCount),
    genres: (raw?.genres ?? raw?.Genres ?? []).map((g: any) => ({
      id: toNumber(g?.id ?? g?.Id),
      name: String(g?.name ?? g?.Name ?? '').trim(),
    })),
  }
}

function normalizeMovies(raw: any): MovieSummaryDto[] {
  if (!Array.isArray(raw)) return []
  return raw.map(normalizeMovie).filter(movie => movie.id > 0 && movie.title.length > 0)
}

const reviewedMovieOptions = computed<ReviewedMovieOption[]>(() => {
  const byMovieId = new Map<number, ReviewedMovieOption>()

  for (const review of userStore.reviews) {
    if (!byMovieId.has(review.movieId)) {
      byMovieId.set(review.movieId, {
        id: review.movieId,
        title: review.movieTitle,
        posterUrl: review.moviePoster,
        reviewCount: 0,
      })
    }

    const current = byMovieId.get(review.movieId)
    if (current) current.reviewCount += 1
  }

  return Array.from(byMovieId.values()).sort((left, right) => left.title.localeCompare(right.title))
})

try {
  await userStore.fetchMyReviews()
} catch {
  // Ignore page bootstrap review fetch errors.
}

watch(generationType, value => {
  if (value !== 'selected') {
    isSelectDialogOpen.value = false
  }
})

function toggleSelectedMovie(movieId: number) {
  if (selectedMovieIds.value.includes(movieId)) {
    selectedMovieIds.value = selectedMovieIds.value.filter(id => id !== movieId)
    return
  }

  selectedMovieIds.value = [...selectedMovieIds.value, movieId]
}

async function loadRecommendations(url: string, body: Record<string, unknown>) {
  generateStore.isGenerating = true
  try {
    const raw = await apiFetch<any[]>(url, {
      method: 'POST',
      body,
    })
    generateStore.setPicks(normalizeMovies(raw))
    generateStore.persistPicks()
  } finally {
    generateStore.isGenerating = false
  }
}

async function generateSelected() {
  if (selectedMovieIds.value.length === 0) return

  isSelectDialogOpen.value = false
  await loadRecommendations('/generate/recommend', {
    movieIds: selectedMovieIds.value,
    count: RECOMMENDATION_COUNT,
  })
}

async function generateNow() {
  if (generationType.value === 'selected') {
    isSelectDialogOpen.value = true
    return
  }

  if (generationType.value === 'all') {
    await loadRecommendations('/generate/recommend/all', { count: RECOMMENDATION_COUNT })
    return
  }

  await loadRecommendations('/generate/recommend/ai', { count: RECOMMENDATION_COUNT })
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

.selection-summary {
  color: rgba(var(--v-theme-on-surface), 0.6);
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

.results-row {
  margin-top: 16px;
}
</style>
