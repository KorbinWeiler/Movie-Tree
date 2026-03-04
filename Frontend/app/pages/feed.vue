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
        v-for="review in visibleReviews"
        :key="review.id"
        :review="review"
      />
    </div>

  </v-container>
</template>

<script setup lang="ts">
const feedStore = useFeedStore()
const activeTab = ref<'public' | 'friends'>('public')

const tabs = [
  { label: 'Public', value: 'public' },
  { label: 'Friends', value: 'friends' },
] as const

try { await feedStore.fetchPublic(true) } catch { /* ignore */ }

watch(activeTab, async (tab) => {
  if (tab === 'friends' && feedStore.friendsFeed.length === 0) {
    try { await feedStore.fetchFriends(true) } catch { /* ignore */ }
  }
})

const visibleReviews = computed(() =>
  activeTab.value === 'public' ? feedStore.publicFeed : feedStore.friendsFeed
)
</script>

<style scoped>
.tab-btn {
  text-transform: none;
  font-weight: 500;
}
</style>
