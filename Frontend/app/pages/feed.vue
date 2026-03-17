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

    <v-alert
      v-if="loadError"
      type="warning"
      variant="tonal"
      density="comfortable"
      class="mb-4"
    >
      {{ loadError }}
    </v-alert>

    <div v-if="isLoading" class="d-flex flex-column ga-4">
      <v-skeleton-loader
        v-for="item in 3"
        :key="item"
        type="article"
        class="feed-skeleton"
      />
    </div>

    <v-sheet
      v-else-if="visibleReviews.length === 0"
      rounded="lg"
      border
      class="pa-6 text-medium-emphasis"
    >
      {{ emptyMessage }}
    </v-sheet>

    <div v-else class="d-flex flex-column ga-4">
      <ReviewCard
        v-for="review in visibleReviews"
        :key="review.id"
        :review="review"
      />
    </div>

  </v-container>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import ReviewCard from '~/components/ReviewCard.vue'

const feedStore = useFeedStore()
const authStore = useAuthStore()
const activeTab = ref<'public' | 'friends'>('public')
const isLoading = ref(false)
const loadError = ref('')

const tabs = [
  { label: 'Public', value: 'public' },
  { label: 'Friends', value: 'friends' },
] as const

const loadTab = async (tab: 'public' | 'friends', reset = false) => {
  isLoading.value = true
  loadError.value = ''

  try {
    if (tab === 'public') {
      await feedStore.fetchPublic(reset)
      return
    }

    await feedStore.fetchFriends(reset)
  } catch {
    loadError.value = tab === 'friends'
      ? 'Unable to load the friends feed right now.'
      : 'Unable to load the public feed right now.'
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  void loadTab('public', true)
})

watch(activeTab, async (tab) => {
  if (tab === 'public' && feedStore.publicFeed.length === 0) {
    await loadTab('public', true)
    return
  }

  if (tab === 'friends' && feedStore.friendsFeed.length === 0) {
    await loadTab('friends', true)
  }
})

const visibleReviews = computed(() =>
  activeTab.value === 'public' ? feedStore.publicFeed : feedStore.friendsFeed
)

const emptyMessage = computed(() => {
  if (activeTab.value === 'friends') {
    return authStore.isLoggedIn
      ? 'No reviews from your friends yet.'
      : 'Sign in to see reviews from friends.'
  }

  return 'No public reviews found.'
})
</script>

<style scoped>
.tab-btn {
  text-transform: none;
  font-weight: 500;
}

.feed-skeleton {
  border-radius: 16px;
}
</style>
