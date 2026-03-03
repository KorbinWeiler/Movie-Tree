<template>
  <v-container class="pa-6" style="max-width: 720px">

    <div class="d-flex align-center justify-space-between mb-6">
      <h1 class="text-h5 font-weight-bold">Feed</h1>
    </div>

    <v-divider class="mb-2" />

    <div class="d-flex ga-1 mb-6 mt-4">
      <v-btn
        v-for="tab in tabs"
        :key="tab.value"
        :variant="activeTab === tab.value ? 'flat' : 'text'"
        :color="activeTab === tab.value ? 'primary' : 'default'"
        size="small"
        rounded="pill"
        @click="activeTab = tab.value"
        class="tab-btn"
        :style="activeTab === tab.value ? 'color: rgb(var(--v-theme-on-primary))' : ''"
      >
        {{ tab.label }}
      </v-btn>
    </div>

    <div class="d-flex flex-column ga-4">
      <ReviewCard
        v-for="review in filteredReviews"
        :key="review.id"
        :review="review"
      />
    </div>

  </v-container>
</template>

<script setup lang="ts">
const activeTab = ref<'public' | 'friends'>('public')

const tabs = [
  { label: 'Public', value: 'public' },
  { label: 'Friends', value: 'friends' },
]

const allReviews = [
  {
    id: 1,
    username: 'cinephile92',
    date: 'Mar 2, 2026',
    score: 9,
    tab: 'public',
    movie: { title: 'Rashomon', year: 1950, genre: 'Drama', director: 'Akira Kurosawa' },
  },
  {
    id: 2,
    username: 'movie_buff',
    date: 'Mar 1, 2026',
    score: 7,
    tab: 'public',
    text: 'A hauntingly beautiful film. The long takes and minimalist dialogue make every scene feel deliberate and meaningful.',
    movie: { title: 'Stalker', year: 1979, genre: 'Sci-Fi Drama', director: 'Andrei Tarkovsky' },
  },
  {
    id: 3,
    username: 'film_friend',
    date: 'Mar 1, 2026',
    score: 8,
    tab: 'friends',
    text: 'One of the most important war films ever made. Absolutely brutal and necessary.',
    movie: { title: 'Come and See', year: 1985, genre: 'War Drama', director: 'Elem Klimov' },
  },
  {
    id: 4,
    username: 'reelreviewer',
    date: 'Feb 28, 2026',
    score: 6,
    tab: 'public',
    movie: { title: 'Breathless', year: 1960, genre: 'Crime Drama', director: 'Jean-Luc Godard' },
  },
  {
    id: 5,
    username: 'jane_watches',
    date: 'Feb 27, 2026',
    score: 10,
    tab: 'friends',
    text: 'Perfect film. Changed the way I think about cinema.',
    movie: { title: 'Tokyo Story', year: 1953, genre: 'Drama', director: 'Yasujirō Ozu' },
  },
]

const filteredReviews = computed(() =>
  allReviews.filter((r) => r.tab === activeTab.value)
)
</script>

<style scoped>
.tab-btn {
  text-transform: none;
  font-weight: 500;
}
</style>
