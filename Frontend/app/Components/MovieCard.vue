<template>
  <div class="movie-card" style="min-height: 280px;" @click="openMovie()">
    <!-- Poster -->
    <div class="movie-poster" style="min-height: 220px;">
      <v-img
        v-if="movie?.posterUrl"
        :src="movie.posterUrl"
        :alt="movie?.title ?? 'Movie'"
        cover
        class="poster-img"
      />
      <div v-else class="poster-placeholder d-flex align-center justify-center">
        <v-icon size="40" color="surface-variant">mdi-movie-open</v-icon>
      </div>

      <!-- Rating badge (guard against non-number values) -->
      <div v-if="isNumber(movie?.averageRating)" class="rating-badge">
        <span>{{ movie!.averageRating }}</span>
      </div>
    </div>

    <!-- Info -->
    <div class="movie-info pa-2">
      <div class="movie-title text-body-2 font-weight-medium">
        {{ displayTitle }}
      </div>
      <div class="movie-year text-caption" style="color: rgb(var(--v-theme-secondary), 0.7)">
        {{ releaseYear }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">

const props = defineProps<{
  movie: {
    id: number
    title: string
    releaseDate?: string | null
    posterUrl?: string | null
    averageRating?: number | null
  } | null
}>()

console.log('[MovieCard] rendering movie', props.movie)

const movieModal = useMovieModal()

function openMovie() {
  if (!props.movie) return
  movieModal.open(props.movie.id)
}

// Safe helpers to avoid rendering objects as text (e.g. "[Object Object]")
const isNumber = (v: any): v is number => typeof v === 'number' && !isNaN(v)

const displayTitle = computed(() => {
  const t = props.movie?.title
  if (typeof t === 'string') return t
  if (typeof t === 'number') return String(t)
  return ''
})

const releaseYear = computed(() =>
  props.movie?.releaseDate ? new Date(props.movie.releaseDate).getFullYear() : ''
)
</script>

<style scoped>
.movie-card {
  cursor: pointer;
  transition: transform 200ms ease, box-shadow 200ms ease;
  border-radius: 12px;
  overflow: hidden;
  width: 100%;
  display: flex;
  flex-direction: column;
  background-color: rgb(var(--v-theme-surface));
  border: 1px solid rgba(var(--v-theme-on-surface), 0.08);
}

.movie-card:hover {
  transform: scale(1.03);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.55);
}

.movie-poster {
  position: relative;
  height: clamp(180px, 24vw, 300px);
  aspect-ratio: 2 / 3;
  background-color: rgb(var(--v-theme-surface));
  border-radius: 12px 12px 0 0;
  overflow: hidden;
}

.poster-img {
  width: 100%;
  height: 100%;
}

/* Fallback for environments where aspect-ratio handling is inconsistent */
@supports not (aspect-ratio: 2 / 3) {
  .movie-poster {
    height: 240px;
  }
}

.poster-placeholder {
  width: 100%;
  height: 100%;
  background-color: rgb(var(--v-theme-surface));
  border: 1px solid rgba(var(--v-theme-on-surface), 0.08);
  border-radius: 12px 12px 0 0;
}

.rating-badge {
  position: absolute;
  top: 8px;
  right: 8px;
  background-color: rgb(var(--v-theme-primary));
  color: rgb(var(--v-theme-on-primary));
  font-size: 11px;
  font-weight: 700;
  padding: 2px 7px;
  border-radius: 9999px;
  line-height: 1.6;
}

.movie-info {
  border-top: 1px solid rgba(var(--v-theme-on-surface), 0.06);
}

.movie-title {
  color: rgb(var(--v-theme-on-surface));
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  line-height: 1.3;
}
</style>
