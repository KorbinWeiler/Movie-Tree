<template>
  <v-container fluid class="pa-6">

    <div class="d-flex align-center justify-space-between mb-6">
      <h1 class="text-h5 font-weight-bold">Your Movies</h1>
    </div>

    <!-- Tabs: Watched / Watch Later -->
    <div class="d-flex ga-1 mb-6">
      <v-btn
        v-for="tab in tabs"
        :key="tab.value"
        :variant="activeTab === tab.value ? 'flat' : 'text'"
        :color="activeTab === tab.value ? 'primary' : 'default'"
        size="small"
        rounded="pill"
        :style="activeTab === tab.value ? 'color: rgb(var(--v-theme-on-primary))' : ''"
        style="text-transform: none; font-weight: 500;"
        @click="activeTab = tab.value"
      >
        {{ tab.label }}
      </v-btn>
    </div>

    <v-row dense>
      <v-col
        v-for="movie in visibleMovies"
        :key="movie.id"
        cols="6"
        sm="4"
        md="3"
        lg="2"
      >
        <MovieCard :movie="movie" />
      </v-col>
    </v-row>

  </v-container>
</template>

<script setup lang="ts">
const activeTab = ref<'watched' | 'watchlater'>('watched')

const tabs = [
  { label: 'Watched', value: 'watched' },
  { label: 'Watch Later', value: 'watchlater' },
]

const watchedMovies = [
  { id: 1, title: 'Rashomon', year: 1950, rating: 8.2 },
  { id: 2, title: 'Tokyo Story', year: 1953, rating: 8.2 },
  { id: 3, title: 'Come and See', year: 1985, rating: 8.3 },
  { id: 4, title: 'Stalker', year: 1979, rating: 8.1 },
  { id: 5, title: 'Breathless', year: 1960, rating: 7.8 },
  { id: 6, title: 'The 400 Blows', year: 1959, rating: 8.0 },
]

const watchLaterMovies = [
  { id: 7, title: 'The Seventh Seal', year: 1957, rating: 8.0 },
  { id: 8, title: 'La Strada', year: 1954, rating: 8.0 },
  { id: 9, title: 'Wild Strawberries', year: 1957, rating: 8.1 },
]

const visibleMovies = computed(() =>
  activeTab.value === 'watched' ? watchedMovies : watchLaterMovies
)
</script>
