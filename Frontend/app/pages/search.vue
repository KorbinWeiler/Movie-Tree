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
        <MaybeMovieCard :movie="movie" />
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
import { defineComponent, getCurrentInstance, h, onMounted, resolveComponent, shallowRef } from 'vue'

const movieStore = useMovieStore()
const query = ref('')
const activeGenreId = ref<number | null>(null)

const MaybeMovieCard = defineComponent({
  name: 'MaybeMovieCard',
  inheritAttrs: false,
  props: {
    movie: { type: Object as () => any, required: true },
  },
  setup(props, { attrs }) {
    const importedComp = shallowRef<any>(null)
    const movieModal = useMovieModal()
    const inst = getCurrentInstance()

    onMounted(async () => {
      if (importedComp.value) return
      try {
        const mod = await import('../components/MovieCard.vue')
        importedComp.value = mod.default
      } catch {
        // Keep fallback card rendering.
      }
    })

    const openMovie = () => {
      const id = Number(props.movie?.id)
      if (Number.isFinite(id) && id > 0) movieModal.open(id)
    }

    return () => {
      const registered = Boolean(
        inst?.appContext?.components?.MovieCard || inst?.appContext?.components?.['movie-card']
      )

      if (registered) return h(resolveComponent('MovieCard') as any, { ...attrs, movie: props.movie })
      if (importedComp.value) return h(importedComp.value, { ...attrs, movie: props.movie })

      return h('div', {
        ...attrs,
        class: ['movie-card', attrs.class],
        style: 'width:100%;display:flex;flex-direction:column;border-radius:12px;overflow:hidden;background:rgb(var(--v-theme-surface));border:1px solid rgba(var(--v-theme-on-surface),0.08);cursor:pointer;',
        onClick: openMovie,
      }, [
        props.movie?.posterUrl
          ? h('img', {
            src: props.movie.posterUrl,
            alt: props.movie?.title ?? 'Movie',
            style: 'width:100%;height:220px;object-fit:cover;',
          })
          : h('div', {
            style: 'width:100%;height:220px;display:flex;align-items:center;justify-content:center;background:rgb(var(--v-theme-surface));',
          }, 'No image'),
        h('div', { style: 'padding:8px;' }, [
          h('div', { style: 'font-weight:600;line-height:1.3;' }, String(props.movie?.title ?? '')),
          h('div', { style: 'font-size:12px;color:rgba(var(--v-theme-on-surface),0.6);margin-top:4px;' },
            props.movie?.releaseDate ? String(new Date(props.movie.releaseDate).getFullYear()) : ''),
        ]),
      ])
    }
  },
})

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
