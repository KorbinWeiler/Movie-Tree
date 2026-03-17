<template>
  <section :class="['movie-section', highlighted ? 'movie-section--highlighted' : '']">
    <div class="section-header mb-4">
      <span class="section-label">{{ title }}</span>
    </div>

    <div class="carousel-shell">
      <div v-if="showNavigation" class="carousel-nav carousel-nav--left">
        <v-btn
          icon
          variant="text"
          size="small"
          class="nav-btn"
          @click="goPrevious"
        >
          <v-icon>mdi-chevron-left</v-icon>
        </v-btn>
      </div>

      <div ref="viewportRef" class="movie-viewport">
        <div v-if="loading" class="movie-page" :style="pageStyle">
          <div
            v-for="item in skeletonCount"
            :key="item"
            class="movie-slot"
          >
            <v-skeleton-loader class="movie-skeleton" type="image, article" />
          </div>
        </div>

        <div v-else-if="visibleMovies.length" class="movie-page" :style="pageStyle">
          <MovieCard
            v-for="movie in visibleMovies"
            :key="movie.id"
            :movie="movie"
            class="movie-slot"
          />
        </div>

        <v-sheet
          v-else
          rounded="lg"
          border
          class="pa-6 text-medium-emphasis"
        >
          {{ emptyMessage }}
        </v-sheet>
      </div>

      <div v-if="showNavigation" class="carousel-nav carousel-nav--right">
        <v-btn
          icon
          variant="text"
          size="small"
          class="nav-btn"
          @click="goNext"
        >
          <v-icon>mdi-chevron-right</v-icon>
        </v-btn>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { useElementSize } from '@vueuse/core'
import MovieCard from '~/components/MovieCard.vue'

interface CarouselMovie {
  id: number
  title: string
  posterUrl?: string | null
  releaseDate?: string | null
  averageRating?: number | null
}

const props = withDefaults(defineProps<{
  title: string
  movies: CarouselMovie[]
  loading?: boolean
  highlighted?: boolean
  emptyMessage?: string
}>(), {
  loading: false,
  highlighted: false,
  emptyMessage: 'No movies to show right now.',
})

const defaultCardsPerPage = 4
const gap = 16
const desktopCardWidth = 150
const mobileCardWidth = 120

const viewportRef = ref<HTMLElement | null>(null)
const currentPage = ref(0)

const { width } = useElementSize(viewportRef)

const cardsPerPage = computed(() => {
  const viewportWidth = width.value
  if (!viewportWidth) return defaultCardsPerPage

  const preferredCardWidth = viewportWidth <= 600 ? mobileCardWidth : desktopCardWidth
  return Math.max(1, Math.floor((viewportWidth + gap) / (preferredCardWidth + gap)))
})

const totalPages = computed(() =>
  Math.max(1, Math.ceil(props.movies.length / cardsPerPage.value))
)

const visibleMovies = computed(() => {
  if (!props.movies.length) return []

  const start = currentPage.value * cardsPerPage.value
  return props.movies.slice(start, start + cardsPerPage.value)
})

const skeletonCount = computed(() => cardsPerPage.value)

const pageStyle = computed(() => ({
  gridTemplateColumns: `repeat(${cardsPerPage.value}, minmax(0, 1fr))`,
}))

const showNavigation = computed(() =>
  !props.loading && props.movies.length > cardsPerPage.value
)

const goNext = () => {
  currentPage.value = (currentPage.value + 1) % totalPages.value
}

const goPrevious = () => {
  currentPage.value = (currentPage.value - 1 + totalPages.value) % totalPages.value
}

watch([() => props.movies.length, cardsPerPage], () => {
  if (currentPage.value >= totalPages.value) {
    currentPage.value = 0
  }
})
</script>

<style scoped>
.movie-section {
  margin-bottom: 40px;
}

.movie-section--highlighted {
  background: rgba(var(--v-theme-primary), 0.04);
  border: 1px solid rgba(var(--v-theme-primary), 0.1);
  border-radius: 12px;
  padding: 12px;
}

.section-label {
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.06em;
  color: rgba(var(--v-theme-on-surface), 0.5);
}

.movie-viewport {
  overflow: hidden;
  flex: 1;
}

.carousel-shell {
  display: flex;
  align-items: center;
  gap: 12px;
}

.carousel-nav {
  display: flex;
  align-items: center;
  justify-content: center;
  flex: 0 0 auto;
}

.movie-page {
  display: grid;
  gap: 16px;
  align-items: start;
}

.movie-slot {
  min-width: 0;
  width: 100%;
}

.movie-skeleton {
  width: 100%;
  height: 100%;
  border-radius: 12px;
}

.nav-btn {
  text-transform: none;
  color: rgba(var(--v-theme-on-surface), 0.72);
}

@media (max-width: 600px) {
  .carousel-shell {
    gap: 8px;
  }

  .movie-page {
    gap: 12px;
  }

  .nav-btn {
    width: 32px;
    height: 32px;
  }
}
</style>