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
import MovieCard from '../components/MovieCard.vue'

definePageMeta({ middleware: 'auth' })

const userStore = useUserStore()
const activeTab = ref<'watched' | 'watchlater'>('watched')

const tabs = [
  { label: 'Watched', value: 'watched' },
  { label: 'Watch Later', value: 'watchlater' },
] as const

try {
  await Promise.all([
    userStore.fetchMyReviews(),
    userStore.fetchWatchLater(),
  ])
} catch { /* ignore */ }

const visibleMovies = computed(() =>
  activeTab.value === 'watched'
    ? userStore.reviews.map(r => ({
        id: r.movieId,
        title: r.movieTitle,
        posterUrl: r.moviePoster,
        averageRating: r.rating,
        releaseDate: null,
        reviewCount: 0,
        genres: [],
      }))
    : userStore.watchLater.map(w => w.movie)
)
</script>
