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
        <MaybeMovieCard :movie="movie" />
      </v-col>
    </v-row>

  </v-container>
</template>

<script setup lang="ts">
import { defineComponent, getCurrentInstance, h, resolveComponent } from 'vue'

definePageMeta({ middleware: 'auth' })

const userStore = useUserStore()
const activeTab = ref<'watched' | 'watchlater'>('watched')

const MaybeMovieCard = defineComponent({
  name: 'MaybeMovieCard',
  inheritAttrs: false,
  props: {
    movie: { type: Object as () => any, required: true },
  },
  setup(props, { attrs }) {
    const movieModal = useMovieModal()
    const inst = getCurrentInstance()

    const openMovie = () => {
      const id = Number(props.movie?.id)
      if (Number.isFinite(id) && id > 0) movieModal.open(id)
    }

    return () => {
      const registered = Boolean(
        inst?.appContext?.components?.MovieCard || inst?.appContext?.components?.['movie-card']
      )

      if (registered) return h(resolveComponent('MovieCard') as any, { ...attrs, movie: props.movie })
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
