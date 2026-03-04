import { defineStore } from 'pinia'
import type { ReviewDto } from './user'

export const useFeedStore = defineStore('feed', {
  state: () => ({
    publicFeed: [] as ReviewDto[],
    friendsFeed: [] as ReviewDto[],
    publicPage: 1,
    friendsPage: 1,
    publicHasMore: true,
    friendsHasMore: true,
  }),

  actions: {
    async fetchPublic(reset = false) {
      if (reset) {
        this.publicFeed = []
        this.publicPage = 1
        this.publicHasMore = true
      }
      if (!this.publicHasMore) return

      const { apiFetch } = useApi()
      try {
        const results = await apiFetch<ReviewDto[]>(`/feed/public?page=${this.publicPage}&pageSize=20`)
        this.publicFeed.push(...results)
        this.publicPage++
        if (results.length < 20) this.publicHasMore = false
      } catch { this.publicHasMore = false }
    },

    async fetchFriends(reset = false) {
      if (reset) {
        this.friendsFeed = []
        this.friendsPage = 1
        this.friendsHasMore = true
      }
      if (!this.friendsHasMore) return

      const { apiFetch } = useApi()
      try {
        const results = await apiFetch<ReviewDto[]>(`/feed/friends?page=${this.friendsPage}&pageSize=20`)
        this.friendsFeed.push(...results)
        this.friendsPage++
        if (results.length < 20) this.friendsHasMore = false
      } catch { this.friendsHasMore = false }
    },
  },
})
