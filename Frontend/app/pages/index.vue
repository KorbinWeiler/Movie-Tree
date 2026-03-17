<template>
  <v-container fluid class="pa-6">
    <MovieCarouselSection
      title="TRENDING NOW"
      :movies="trendingMovies"
      :loading="isTrendingLoading"
      empty-message="No trending movies right now."
    />

    <MovieCarouselSection
      title="AI PICKS"
      :movies="aiPickMovies"
      :loading="isAiPicksLoading"
      :highlighted="true"
      empty-message="No AI picks available right now."
    />

    <MovieCarouselSection
      v-if="authStore.isLoggedIn && watchLaterMovies.length"
      title="WATCH LATER"
      :movies="watchLaterMovies"
      empty-message="Nothing in watch later yet."
    />

  </v-container>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import MovieCarouselSection from '~/components/MovieCarouselSection.vue'

const movieStore = useMovieStore()
const generateStore = useGenerateStore()
const userStore = useUserStore()
const authStore = useAuthStore()

// Start at false so SSR-rendered HTML (which also ends at false after await)
// matches client initial state — eliminates hydration mismatch that was
// replacing server-rendered MovieCards with skeletons before data reloaded.
const isTrendingLoading = ref(false)
const isAiPicksLoading = ref(false)

const loadHomeSections = async () => {
  isTrendingLoading.value = true
  isAiPicksLoading.value = true
  await Promise.allSettled([
    movieStore.fetchTrending().finally(() => { isTrendingLoading.value = false }),
    generateStore.loadPicks().finally(() => { isAiPicksLoading.value = false }),
  ])
}

// SSR: fetch data so the server can render populated cards.
// isTrendingLoading ends as false by the time setup resolves.
if (!import.meta.client) {
  await loadHomeSections()
  if (authStore.isLoggedIn) {
    try { await userStore.fetchWatchLater() } catch { /* ignore */ }
  }
}

// Client: load after mount to guarantee no SSR hydration conflict.
onMounted(() => {
  void loadHomeSections()
  if (authStore.isLoggedIn) {
    userStore.fetchWatchLater().catch(() => {})
  }
})

const trendingMovies = computed(() => movieStore.trending)
const aiPickMovies = computed(() => generateStore.picks)
const watchLaterMovies = computed(() => userStore.watchLater.map(w => w.movie))
</script>

<style scoped>
</style>
