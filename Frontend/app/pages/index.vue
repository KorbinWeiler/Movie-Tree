<template>
  <v-container fluid class="pa-6">

    <!-- Trending Now -->
    <section class="mb-10">
      <div class="section-header d-flex align-center justify-space-between mb-4">
        <span class="section-label">TRENDING NOW</span>
        <v-btn variant="text" color="primary" size="small" class="see-all-btn">See All</v-btn>
      </div>
      <div class="movie-row">
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
      <div class="movie-row ai-picks-row">
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
const movieStore = useMovieStore()
const generateStore = useGenerateStore()
const userStore = useUserStore()
const authStore = useAuthStore()

try {
  await Promise.all([
    movieStore.fetchTrending(),
    generateStore.fetchAiPicks(),
  ])
} catch { /* server offline — pages shows empty sections */ }

if (authStore.isLoggedIn) {
  try { await userStore.fetchWatchLater() } catch { /* ignore */ }
}

const trendingMovies = computed(() => movieStore.trending)
const aiPickMovies = computed(() => generateStore.aiPicks?.movies.map(p => p.movie) ?? [])
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
  gap: 16px;
  overflow-x: auto;
  padding-bottom: 8px;
  scrollbar-width: thin;
  scrollbar-color: rgba(var(--v-theme-on-surface), 0.2) transparent;
  scroll-snap-type: x mandatory;
}

.movie-row::-webkit-scrollbar {
  height: 4px;
}

.movie-row::-webkit-scrollbar-thumb {
  background: rgba(var(--v-theme-on-surface), 0.2);
  border-radius: 4px;
}

.movie-row-item {
  flex-shrink: 0;
  width: 140px;
  scroll-snap-align: start;
}

.ai-picks-row {
  background: rgba(var(--v-theme-primary), 0.04);
  border-radius: 12px;
  padding: 12px;
  border: 1px solid rgba(var(--v-theme-primary), 0.1);
}
</style>
