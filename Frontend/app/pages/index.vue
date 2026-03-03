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
    <section class="mb-10">
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
const trendingMovies = [
  { id: 1, title: 'The Grand Illusion', year: 1937, rating: 8.1 },
  { id: 2, title: 'Rashomon', year: 1950, rating: 8.2 },
  { id: 3, title: 'Bicycle Thieves', year: 1948, rating: 8.3 },
  { id: 4, title: 'Breathless', year: 1960, rating: 7.8 },
  { id: 5, title: 'The 400 Blows', year: 1959, rating: 8.0 },
  { id: 6, title: 'Wild Strawberries', year: 1957, rating: 8.1 },
  { id: 7, title: 'Cleo from 5 to 7', year: 1962, rating: 7.6 },
]

const aiPickMovies = [
  { id: 8, title: 'Stalker', year: 1979, rating: 8.1 },
  { id: 9, title: 'Come and See', year: 1985, rating: 8.3 },
  { id: 10, title: 'Andrei Rublev', year: 1966, rating: 8.1 },
  { id: 11, title: 'The Mirror', year: 1975, rating: 8.0 },
  { id: 12, title: 'Wings of Desire', year: 1987, rating: 7.8 },
  { id: 13, title: 'Au Revoir les Enfants', year: 1987, rating: 7.9 },
  { id: 14, title: 'Europa Europa', year: 1990, rating: 7.6 },
]

const watchLaterMovies = [
  { id: 15, title: 'Tokyo Story', year: 1953, rating: 8.2 },
  { id: 16, title: 'Umberto D.', year: 1952, rating: 8.0 },
  { id: 17, title: 'The Seventh Seal', year: 1957, rating: 8.0 },
  { id: 18, title: 'La Strada', year: 1954, rating: 8.0 },
  { id: 19, title: 'Nights of Cabiria', year: 1957, rating: 8.0 },
  { id: 20, title: "Shoot the Piano Player", year: 1960, rating: 7.5 },
  { id: 21, title: 'The Rules of the Game', year: 1939, rating: 7.9 },
]
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
