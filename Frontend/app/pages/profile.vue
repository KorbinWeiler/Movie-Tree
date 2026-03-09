<template>
  <v-container class="pa-6" style="max-width: 720px">

    <!-- Header -->
    <div class="d-flex align-center ga-5 mb-8">
      <v-avatar size="72" color="primary">
        <v-img v-if="authStore.user?.profilePictureUrl" :src="authStore.user.profilePictureUrl" :alt="authStore.user.userName" />
        <span v-else class="text-h5 font-weight-bold" style="color: rgb(var(--v-theme-on-primary))">
          {{ initial }}
        </span>
      </v-avatar>
      <div>
        <h1 class="text-h5 font-weight-bold mb-0">{{ authStore.user?.userName }}</h1>
        <p class="text-body-2 mb-0" style="color: rgba(var(--v-theme-on-surface), 0.5)">
          {{ authStore.user?.email }}
        </p>
      </div>
    </div>

    <!-- Stats -->
    <div class="d-flex ga-4 mb-8 flex-wrap">
      <v-sheet
        v-for="stat in stats"
        :key="stat.label"
        color="surface"
        border
        rounded="lg"
        class="stat-card pa-4 d-flex flex-column align-center"
      >
        <span class="text-h5 font-weight-bold">{{ stat.value }}</span>
        <span class="text-caption" style="color: rgba(var(--v-theme-on-surface), 0.5)">{{ stat.label }}</span>
      </v-sheet>
    </div>

    <!-- Recent Reviews -->
    <div v-if="userStore.reviews.length">
      <div class="section-label mb-3">RECENT REVIEWS</div>
      <div class="d-flex flex-column ga-3">
        <ReviewCard
          v-for="review in recentReviews"
          :key="review.id"
          :review="review"
        />
      </div>
      <div v-if="userStore.reviews.length > 3" class="d-flex justify-center mt-4">
        <v-btn
          variant="outlined"
          rounded="pill"
          size="small"
          to="/your-movies"
          style="text-transform: none;"
        >
          See all reviews
        </v-btn>
      </div>
    </div>

    <!-- Empty state -->
    <div v-else class="d-flex flex-column align-center justify-center py-12 text-center">
      <v-icon size="56" class="mb-3" style="color: rgba(var(--v-theme-on-surface), 0.18)">mdi-movie-open-outline</v-icon>
      <p class="text-body-2" style="color: rgba(var(--v-theme-on-surface), 0.45)">No reviews yet.</p>
    </div>

  </v-container>
</template>

<script setup lang="ts">
definePageMeta({ middleware: 'auth' })

const authStore = useAuthStore()
const userStore = useUserStore()

try {
  await Promise.all([
    userStore.fetchMyReviews(),
    userStore.fetchWatchLater(),
  ])
} catch { /* ignore */ }

const initial = computed(() =>
  authStore.user?.userName?.charAt(0).toUpperCase() ?? '?'
)

const stats = computed(() => [
  { label: 'Reviews', value: userStore.reviews.length },
  { label: 'Watch Later', value: userStore.watchLater.length },
  {
    label: 'Avg Rating',
    value: userStore.reviews.length
      ? (userStore.reviews.reduce((sum, r) => sum + r.rating, 0) / userStore.reviews.length).toFixed(1)
      : '—',
  },
])

const recentReviews = computed(() => userStore.reviews.slice(0, 3))
</script>

<style scoped>
.section-label {
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.07em;
  color: rgba(var(--v-theme-on-surface), 0.45);
}

.stat-card {
  min-width: 100px;
  flex: 1;
  border-color: rgba(var(--v-theme-on-surface), 0.08) !important;
}
</style>
