<template>
  <v-dialog v-model="isOpen" max-width="760" :max-height="'90vh'">
    <!-- Loading skeleton -->
    <v-card v-if="isOpen && !movie" color="surface" rounded="xl" class="movie-modal">
      <div class="modal-body d-flex">
        <div class="modal-poster flex-shrink-0">
          <v-skeleton-loader type="image" height="100%" />
        </div>
        <div class="flex-grow-1 pa-6">
          <v-skeleton-loader type="heading, subtitle, text@4" />
        </div>
      </div>
    </v-card>

    <!-- Movie details -->
    <v-card v-else-if="movie" color="surface" rounded="xl" class="movie-modal">
      <v-btn icon variant="text" size="small" class="modal-close" @click="close()">
        <v-icon>mdi-close</v-icon>
      </v-btn>

      <div class="modal-body d-flex flex-column flex-sm-row">
        <!-- Poster -->
        <div class="modal-poster flex-shrink-0">
          <v-img
            v-if="movie.posterUrl"
            :src="movie.posterUrl"
            :alt="movie.title"
            cover
            class="poster-img"
          />
          <div v-else class="poster-placeholder d-flex align-center justify-center">
            <v-icon size="48" color="surface-variant">mdi-movie-open</v-icon>
          </div>
        </div>

        <!-- Info panel -->
        <div class="modal-info pa-5 pa-sm-6 d-flex flex-column flex-grow-1 overflow-y-auto">
          <!-- Title + rating -->
          <div class="d-flex align-start justify-space-between mb-1 ga-3">
            <h2 class="text-h5 font-weight-bold" style="line-height: 1.25">{{ movie.title }}</h2>
            <div v-if="movie.averageRating" class="rating-pill flex-shrink-0 d-flex align-center ga-1">
              <v-icon size="13" color="primary">mdi-star</v-icon>
              <span class="text-body-2 font-weight-bold">{{ movie.averageRating.toFixed(1) }}</span>
              <span class="text-caption" style="color: rgba(var(--v-theme-on-surface), 0.45)">/10</span>
            </div>
          </div>

          <!-- Year / runtime -->
          <p class="text-body-2 mb-3" style="color: rgba(var(--v-theme-on-surface), 0.5)">
            {{ releaseYear }}<template v-if="runtimeText"> &bull; {{ runtimeText }}</template>
          </p>

          <!-- Genres -->
          <div v-if="movie.genres?.length" class="d-flex flex-wrap ga-2 mb-4">
            <v-chip
              v-for="genre in movie.genres"
              :key="genre.id"
              size="x-small"
              variant="outlined"
              color="primary"
            >{{ genre.name }}</v-chip>
          </div>

          <!-- Description -->
          <p
            v-if="movie.description"
            class="text-body-2 mb-0 flex-grow-1"
            style="color: rgba(var(--v-theme-on-surface), 0.75); line-height: 1.65"
          >{{ movie.description }}</p>
          <div v-else class="flex-grow-1" />

          <v-divider class="my-5" />

          <!-- Unauthenticated prompt -->
          <p v-if="!auth.isLoggedIn" class="text-body-2 mb-0" style="color: rgba(var(--v-theme-on-surface), 0.55)">
            <NuxtLink to="/login" class="text-primary" @click="close()">Sign in</NuxtLink>
            to add to Watch Later or write a review.
          </p>

          <!-- Authenticated actions -->
          <div v-else class="d-flex flex-column ga-3">
            <!-- Watch Later -->
            <v-btn
              :variant="isInWatchLater ? 'flat' : 'outlined'"
              :color="isInWatchLater ? 'primary' : 'default'"
              :loading="watchLaterLoading"
              rounded="pill"
              size="small"
              style="align-self: flex-start; text-transform: none"
              @click="toggleWatchLater"
            >
              <v-icon start size="16">
                {{ isInWatchLater ? 'mdi-bookmark-check' : 'mdi-bookmark-plus-outline' }}
              </v-icon>
              {{ isInWatchLater ? 'In Watch Later' : 'Add to Watch Later' }}
            </v-btn>

            <!-- Write a review -->
            <div>
              <v-btn
                v-if="!showReviewForm"
                variant="outlined"
                color="primary"
                rounded="pill"
                size="small"
                style="text-transform: none"
                @click="showReviewForm = true"
              >
                <v-icon start size="16">mdi-pencil-outline</v-icon>
                Write a Review
              </v-btn>

              <!-- Review form -->
              <div v-else class="review-form pa-4">
                <p class="text-body-2 font-weight-medium mb-2">Rating</p>
                <div class="d-flex align-center ga-2 mb-4">
                  <v-slider
                    v-model="reviewRating"
                    :min="1"
                    :max="10"
                    :step="1"
                    thumb-label="always"
                    color="primary"
                    class="flex-grow-1"
                    hide-details
                  />
                  <span class="font-weight-bold" style="font-size: 18px; min-width: 44px; text-align: right">
                    {{ reviewRating }}<span class="text-caption font-weight-regular" style="color: rgba(var(--v-theme-on-surface), 0.5)">/10</span>
                  </span>
                </div>

                <v-textarea
                  v-model="reviewText"
                  placeholder="Share your thoughts... (optional)"
                  variant="outlined"
                  density="compact"
                  rows="3"
                  auto-grow
                  hide-details
                  class="mb-3"
                  bg-color="surface"
                />

                <div class="d-flex align-center flex-wrap ga-2 mb-4">
                  <span class="text-caption" style="color: rgba(var(--v-theme-on-surface), 0.5)">Visibility</span>
                  <v-btn-toggle
                    v-model="reviewVisibility"
                    mandatory
                    density="compact"
                    variant="outlined"
                    color="primary"
                  >
                    <v-btn value="Public" size="x-small">Public</v-btn>
                    <v-btn value="Friends" size="x-small">Friends</v-btn>
                    <v-btn value="Private" size="x-small">Private</v-btn>
                  </v-btn-toggle>
                </div>

                <div class="d-flex ga-2">
                  <v-btn
                    color="primary"
                    variant="flat"
                    size="small"
                    rounded="pill"
                    :loading="reviewLoading"
                    style="text-transform: none; color: rgb(var(--v-theme-on-primary))"
                    @click="submitReview"
                  >Submit</v-btn>
                  <v-btn
                    variant="text"
                    size="small"
                    rounded="pill"
                    style="text-transform: none"
                    @click="showReviewForm = false"
                  >Cancel</v-btn>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import type { MovieDetailDto } from '~/stores/movie'

const { isOpen, movieId, close } = useMovieModal()
const movieStore = useMovieStore()
const userStore = useUserStore()
const auth = useAuthStore()

const movie = ref<MovieDetailDto | null>(null)
const showReviewForm = ref(false)
const reviewRating = ref(7)
const reviewText = ref('')
const reviewVisibility = ref<'Public' | 'Friends' | 'Private'>('Public')
const watchLaterLoading = ref(false)
const reviewLoading = ref(false)

const isInWatchLater = computed(() =>
  userStore.watchLater.some(w => w.movie.id === movieId.value)
)

const releaseYear = computed(() =>
  movie.value?.releaseDate ? new Date(movie.value.releaseDate).getFullYear() : ''
)

const runtimeText = computed(() => {
  const mins = movie.value?.runtimeMinutes
  if (!mins) return ''
  return `${Math.floor(mins / 60)}h ${mins % 60}m`
})

watch(isOpen, async (open) => {
  if (!open) return
  movie.value = null
  showReviewForm.value = false
  reviewRating.value = 7
  reviewText.value = ''
  reviewVisibility.value = 'Public'
  if (movieId.value !== null) {
    movie.value = await movieStore.fetchById(movieId.value)
  }
  if (auth.isLoggedIn) {
    try { await userStore.fetchWatchLater() } catch { /* ignore */ }
  }
})

const toggleWatchLater = async () => {
  if (!movie.value) return
  watchLaterLoading.value = true
  try {
    if (isInWatchLater.value) {
      await userStore.removeFromWatchLater(movie.value.id)
    } else {
      await userStore.addToWatchLater(movie.value.id)
    }
  } finally {
    watchLaterLoading.value = false
  }
}

const submitReview = async () => {
  if (!movie.value) return
  reviewLoading.value = true
  try {
    await userStore.createReview(movie.value.id, reviewRating.value, reviewText.value || null, reviewVisibility.value)
    showReviewForm.value = false
  } finally {
    reviewLoading.value = false
  }
}
</script>

<style scoped>
.movie-modal {
  overflow: hidden;
}

.modal-close {
  position: absolute;
  top: 10px;
  right: 10px;
  z-index: 2;
}

.modal-body {
  min-height: 360px;
}

.modal-poster {
  width: 210px;
  min-width: 210px;
  background: rgb(var(--v-theme-surface));
}

.poster-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.poster-placeholder {
  width: 100%;
  height: 100%;
  min-height: 300px;
  background: rgba(var(--v-theme-on-surface), 0.05);
}

.modal-info {
  max-height: calc(90vh - 64px);
}

.rating-pill {
  background: rgba(var(--v-theme-primary), 0.12);
  padding: 3px 9px;
  border-radius: 9999px;
}

.review-form {
  background: rgba(var(--v-theme-on-surface), 0.04);
  border: 1px solid rgba(var(--v-theme-on-surface), 0.08);
  border-radius: 12px;
}

@media (max-width: 599px) {
  .modal-poster {
    width: 100%;
    min-width: unset;
    height: 220px;
  }

  .poster-img {
    object-position: center 20%;
  }
}
</style>
