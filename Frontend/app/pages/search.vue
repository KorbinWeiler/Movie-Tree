<template>
  <v-container fluid class="pa-6">

    <!-- Genre Filter Chips -->
    <div class="d-flex flex-wrap ga-2 justify-center mb-5">
      <v-chip
        v-for="genre in genres"
        :key="genre.id"
        :variant="activeGenreId === genre.id ? 'flat' : 'outlined'"
        :color="activeGenreId === genre.id ? 'primary' : 'default'"
        size="small"
        rounded="sm"
        class="genre-chip"
        :style="activeGenreId === genre.id ? 'color: rgb(var(--v-theme-on-primary))' : ''"
        @click="toggleGenre(genre.id)"
      >
        {{ genre.name }}
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
    <v-row v-if="results.length" dense>
      <v-col
        v-for="movie in results"
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
import { MovieCard } from '#components'

const movieStore = useMovieStore()
const query = ref('')
const activeGenreId = ref<number | null>(null)

try { await movieStore.fetchGenres() } catch { /* ignore */ }
try { await movieStore.search('') } catch { /* ignore */ }

const toggleGenre = (genreId: number) => {
  activeGenreId.value = activeGenreId.value === genreId ? null : genreId
  doSearch()
}

const doSearch = useDebounceFn(async () => {
  await movieStore.search(query.value, activeGenreId.value ?? undefined)
}, 300)

watch(query, doSearch)

const genres = computed(() => movieStore.genres)
const results = computed(() => movieStore.searchResults)
</script>

<style scoped>
.genre-chip {
  text-transform: none;
  cursor: pointer;
  font-weight: 500;
}
</style>
