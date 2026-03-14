<template>
  <v-container fluid class="pa-6">

    <!-- Trending Now -->
    <section class="mb-10">
      <div class="section-header d-flex align-center justify-space-between mb-4">
        <span class="section-label">TRENDING NOW</span>
        <v-btn variant="text" color="primary" size="small" class="see-all-btn">See All</v-btn>
      </div>
      <div v-if="isTrendingLoading" class="movie-row">
        <div v-for="n in 6" :key="`trending-skeleton-${n}`" class="movie-row-item">
          <v-skeleton-loader class="movie-skeleton" type="image, article" />
        </div>
      </div>
      <div v-else class="movie-row">
        <MovieCard
          v-for="movie in trendingMovies"
          :key="movie.id"
          :movie="movie"
          class="movie-row-item"
        />
      </div>
    </section>

    <!-- AI Picks -->
    <section class="mb-10">
      <div class="section-header d-flex align-center justify-space-between mb-4">
        <span class="section-label">AI PICKS</span>
        <v-btn variant="text" color="primary" size="small" class="see-all-btn">See All</v-btn>
      </div>
      <div v-if="isAiPicksLoading" class="movie-row ai-picks-row">
        <div v-for="n in 6" :key="`ai-skeleton-${n}`" class="movie-row-item">
          <v-skeleton-loader class="movie-skeleton" type="image, article" />
        </div>
      </div>
      <div v-else class="movie-row ai-picks-row">
        <MovieCard
          v-for="movie in aiPickMovies"
          :key="movie.id"
          :movie="movie"
          class="movie-row-item"
        />
      </div>
    </section>

    <!-- Watch Later (when logged in) -->
    <section v-if="authStore.isLoggedIn && watchLaterMovies.length" class="mb-10">
      <div class="section-header d-flex align-center justify-space-between mb-4">
        <span class="section-label">WATCH LATER</span>
        <v-btn variant="text" color="primary" size="small" class="see-all-btn">See All</v-btn>
      </div>
      <div class="movie-row">
        <MovieCard
          v-for="movie in watchLaterMovies"
          :key="movie.id"
          :movie="movie"
          class="movie-row-item"
        />
      </div>
    </section>

  </v-container>
</template>

<script setup lang="ts">
import MovieCard from '@/components/MovieCard.vue'

const movieStore = useMovieStore()
const generateStore = useGenerateStore()
const userStore = useUserStore()
const authStore = useAuthStore()
const isTrendingLoading = ref(true)
const isAiPicksLoading = ref(true)

const loadHomeSections = async () => {
  await Promise.allSettled([
    (async () => {
      try {
        await movieStore.fetchTrending()
      } finally {
        isTrendingLoading.value = false
      }
    })(),
    (async () => {
      try {
        await generateStore.loadPicks()
      } finally {
        isAiPicksLoading.value = false
      }
    })(),
  ])
}

if (import.meta.client) {
  void loadHomeSections()
} else {
  await loadHomeSections()
}

if (authStore.isLoggedIn) {
  try { await userStore.fetchWatchLater() } catch { /* ignore */ }
}

const trendingMovies = computed(() => movieStore.trending)
const aiPickMovies = computed(() => generateStore.picks)
const watchLaterMovies = computed(() => userStore.watchLater.map(w => w.movie))
</script>

<style scoped>
.section-label {
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.06em;
  color: rgba(var(--v-theme-on-surface), 0.5);
}

.see-all-btn {
  text-transform: none;
  font-size: 13px;
}

.movie-row {
  display: flex;
  justify-content: center;
  gap: 16px;
  overflow-x: auto;
  padding-bottom: 8px;
  padding: 12px;
  scrollbar-width: thin;
  scrollbar-color: rgba(var(--v-theme-on-surface), 0.2) transparent;
  /* scroll-snap-type: x mandatory; */
}

.movie-row::-webkit-scrollbar {
  height: 4px;
}

.movie-row::-webkit-scrollbar-thumb {
  background: rgba(var(--v-theme-on-surface), 0.2);
  border-radius: 4px;
}

.movie-row-item {
  aspect-ratio: 2/3;
  flex: 0 0 clamp(130px, 12vw, 190px) !important;
  width: clamp(130px, 12vw, 190px) !important;
  min-width: 130px !important;
  max-width: 190px !important;
  scroll-snap-align: start;
}

.movie-skeleton {
  width: 100%;
  height: 100%;
  border-radius: 12px;
}

.ai-picks-row {
  background: rgba(var(--v-theme-primary), 0.04);
  border-radius: 12px;
  padding: 12px;
  border: 1px solid rgba(var(--v-theme-primary), 0.1);
}
</style>
