<template>
  <v-card color="surface" border rounded="lg" class="review-card pa-4">
    <!-- User header -->
    <div class="d-flex align-center mb-4 ga-3">
      <v-avatar size="36" color="primary">
        <v-img v-if="review.userAvatar" :src="review.userAvatar" :alt="review.userName" />
        <span v-else class="text-body-2 font-weight-bold" style="color: rgb(var(--v-theme-on-primary))">
          {{ review.userName.charAt(0).toUpperCase() }}
        </span>
      </v-avatar>
      <div>
        <div class="text-body-2 font-weight-semibold">{{ review.userName }}</div>
        <div class="text-caption" style="color: rgba(var(--v-theme-on-surface), 0.5)">
          {{ formattedDate }}
        </div>
      </div>
    </div>

    <!-- Content row: Poster | Movie Info | Score -->
    <div class="review-content d-flex ga-4">
      <!-- Movie Poster -->
      <div class="review-poster">
        <div class="poster-wrap">
          <v-img
            v-if="review.moviePoster"
            :src="review.moviePoster"
            :alt="review.movieTitle"
            cover
            class="h-100"
          />
          <div v-else class="poster-placeholder d-flex align-center justify-center h-100">
            <v-icon size="28" color="surface-variant">mdi-movie-open</v-icon>
          </div>
        </div>
      </div>

      <!-- Movie Info -->
      <div class="review-movie-info flex-grow-1 d-flex flex-column justify-center">
        <div class="text-h6 font-weight-bold mb-1">{{ review.movieTitle }}</div>

        <div v-if="movieMeta.length" class="review-meta mb-2">
          <span
            v-for="item in movieMeta"
            :key="item"
            class="review-meta-item"
          >
            {{ item }}
          </span>
        </div>

        <p
          v-if="review.movieDescription"
          class="text-body-2 mb-0 review-description"
        >
          {{ review.movieDescription }}
        </p>
      </div>

      <!-- Score -->
      <div class="review-score d-flex align-center justify-center">
        <v-sheet
          rounded="lg"
          class="score-box d-flex flex-column align-center justify-center pa-3"
          :style="{ backgroundColor: scoreColor }"
        >
          <span class="score-number font-weight-bold">{{ review.rating }}</span>
          <span class="score-label text-caption">/10</span>
        </v-sheet>
      </div>
    </div>

    <!-- Review text (optional) -->
    <div
      v-if="review.reviewText"
      class="review-text mt-4 pa-3"
      style="background: rgba(var(--v-theme-on-surface), 0.04); border-radius: 8px;"
    >
      <p class="text-body-2 mb-0" style="color: rgba(var(--v-theme-on-surface), 0.8); line-height: 1.6;">
        {{ review.reviewText }}
      </p>
    </div>
  </v-card>
</template>

<script setup lang="ts">
const props = defineProps<{
  review: {
    id: number
    userName: string
    userAvatar?: string | null
    createdAt: string
    rating: number
    reviewText?: string | null
    movieTitle: string
    moviePoster?: string | null
    movieDescription?: string | null
    movieReleaseDate?: string | null
    movieRuntimeMinutes?: number | null
  }
}>()

const formattedDate = computed(() =>
  new Date(props.review.createdAt).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
)

const formattedReleaseDate = computed(() => {
  if (!props.review.movieReleaseDate) return null

  return new Date(props.review.movieReleaseDate).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
})

const formattedRuntime = computed(() => {
  const runtime = props.review.movieRuntimeMinutes
  if (!runtime) return null

  const hours = Math.floor(runtime / 60)
  const minutes = runtime % 60

  if (hours === 0) return `${minutes}m`
  if (minutes === 0) return `${hours}h`
  return `${hours}h ${minutes}m`
})

const movieMeta = computed(() =>
  [formattedReleaseDate.value, formattedRuntime.value].filter((value): value is string => Boolean(value))
)

const scoreColor = computed(() => {
  const s = props.review.rating
  if (s >= 7) return 'rgba(255, 209, 71, 0.15)'
  if (s >= 4) return 'rgba(247, 155, 62, 0.15)'
  return 'rgba(224, 86, 86, 0.15)'
})
</script>

<style scoped>
.review-card {
  border-color: rgba(var(--v-theme-on-surface), 0.08) !important;
}

.review-poster {
  flex-shrink: 0;
  width: 80px;
}

.review-movie-info {
  min-width: 0;
}

.review-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.review-meta-item {
  color: rgba(var(--v-theme-on-surface), 0.56);
  font-size: 0.78rem;
  font-weight: 600;
  letter-spacing: 0.02em;
}

.review-description {
  color: rgba(var(--v-theme-on-surface), 0.72);
  display: -webkit-box;
  overflow: hidden;
  line-clamp: 3;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 3;
}

.poster-wrap {
  aspect-ratio: 2 / 3;
  border-radius: 8px;
  overflow: hidden;
  background-color: rgba(var(--v-theme-on-surface), 0.06);
}

.poster-placeholder {
  width: 100%;
}

.score-box {
  min-width: 72px;
  min-height: 72px;
}

.score-number {
  font-size: 1.75rem;
  line-height: 1;
  color: rgb(var(--v-theme-on-surface));
}

.score-label {
  color: rgba(var(--v-theme-on-surface), 0.5);
  font-size: 11px;
}
</style>
