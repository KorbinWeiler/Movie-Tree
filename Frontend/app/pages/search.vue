<template>
  <v-container fluid class="pa-6">

    <!-- Genre Filter Chips -->
    <div class="d-flex flex-wrap ga-2 justify-center mb-5">
      <v-chip
        v-for="genre in genres"
        :key="genre"
        :variant="activeGenre === genre ? 'flat' : 'outlined'"
        :color="activeGenre === genre ? 'primary' : 'default'"
        size="small"
        rounded="sm"
        class="genre-chip"
        :style="activeGenre === genre ? 'color: rgb(var(--v-theme-on-primary))' : ''"
        @click="toggleGenre(genre)"
      >
        {{ genre }}
      </v-chip>
    </div>

    <!-- Search Bar -->
    <div class="d-flex justify-center mb-8">
      <v-text-field
        v-model="query"
        prepend-inner-icon="mdi-magnify"
        placeholder="Search movies..."
        variant="outlined"
        density="compact"
        rounded="pill"
        hide-details
        clearable
        style="max-width: 480px; width: 100%;"
        bg-color="surface"
      />
    </div>

    <!-- Movie Grid -->
    <v-row v-if="filteredMovies.length" dense>
      <v-col
        v-for="movie in filteredMovies"
        :key="movie.id"
        cols="6"
        sm="4"
        md="3"
        lg="2"
      >
        <MovieCard :movie="movie" />
      </v-col>
    </v-row>

    <!-- Empty state -->
    <div v-else class="d-flex flex-column align-center justify-center py-16 text-center">
      <v-icon size="64" style="color: rgba(var(--v-theme-on-surface), 0.2)" class="mb-4">
        mdi-movie-search-outline
      </v-icon>
      <h3 class="text-h6 font-weight-semibold mb-2">No movies found</h3>
      <p class="text-body-2" style="color: rgba(var(--v-theme-on-surface), 0.5)">
        Try a different title or genre filter.
      </p>
    </div>

  </v-container>
</template>

<script setup lang="ts">
const query = ref('')
const activeGenre = ref<string | null>(null)

const genres = ['Family', 'Horror', 'Fantasy', 'Action', 'Romance', 'Comedy', 'Science Fiction', 'Drama', 'Thriller', 'Animation']

const toggleGenre = (genre: string) => {
  activeGenre.value = activeGenre.value === genre ? null : genre
}

const allMovies = [
  { id: 1, title: 'The Shining', year: 1980, rating: 8.4, genres: ['Horror'] },
  { id: 2, title: 'Get Out', year: 2017, rating: 7.7, genres: ['Horror', 'Thriller'] },
  { id: 3, title: 'Alien', year: 1979, rating: 8.5, genres: ['Science Fiction', 'Horror'] },
  { id: 4, title: 'Toy Story', year: 1995, rating: 8.3, genres: ['Animation', 'Family', 'Comedy'] },
  { id: 5, title: 'The Princess Bride', year: 1987, rating: 8.0, genres: ['Fantasy', 'Romance', 'Comedy'] },
  { id: 6, title: 'Die Hard', year: 1988, rating: 8.2, genres: ['Action'] },
  { id: 7, title: 'Mad Max: Fury Road', year: 2015, rating: 8.1, genres: ['Action', 'Science Fiction'] },
  { id: 8, title: 'Eternal Sunshine', year: 2004, rating: 8.3, genres: ['Romance', 'Drama', 'Science Fiction'] },
  { id: 9, title: 'Spirited Away', year: 2001, rating: 8.6, genres: ['Animation', 'Fantasy', 'Family'] },
  { id: 10, title: 'The Big Lebowski', year: 1998, rating: 8.1, genres: ['Comedy'] },
  { id: 11, title: 'Inception', year: 2010, rating: 8.8, genres: ['Action', 'Science Fiction', 'Thriller'] },
  { id: 12, title: "Schindler's List", year: 1993, rating: 9.0, genres: ['Drama', 'Thriller'] },
  { id: 13, title: 'Amélie', year: 2001, rating: 8.3, genres: ['Romance', 'Comedy', 'Drama'] },
  { id: 14, title: 'Pan\'s Labyrinth', year: 2006, rating: 8.2, genres: ['Fantasy', 'Drama', 'Thriller'] },
  { id: 15, title: 'Groundhog Day', year: 1993, rating: 8.0, genres: ['Comedy', 'Fantasy', 'Romance'] },
  { id: 16, title: 'The Thing', year: 1982, rating: 8.2, genres: ['Horror', 'Science Fiction'] },
  { id: 17, title: 'Princess Mononoke', year: 1997, rating: 8.3, genres: ['Fantasy', 'Animation'] },
  { id: 18, title: 'Speed Racer', year: 2008, rating: 6.5, genres: ['Action', 'Family'] },
]

const filteredMovies = computed(() => {
  return allMovies.filter((m) => {
    const matchesGenre = !activeGenre.value || m.genres.includes(activeGenre.value)
    const matchesQuery = !query.value || m.title.toLowerCase().includes(query.value.toLowerCase())
    return matchesGenre && matchesQuery
  })
})
</script>

<style scoped>
.genre-chip {
  text-transform: none;
  cursor: pointer;
  font-weight: 500;
}
</style>
