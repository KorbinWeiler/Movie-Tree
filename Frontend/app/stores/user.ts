import { defineStore } from 'pinia'
import type { MovieSummaryDto } from './movie'
import { useFeedStore } from './feed'

export interface ReviewDto {
  id: number
  userId: string
  userName: string
  userAvatar: string | null
  movieId: number
  movieTitle: string
  moviePoster: string | null
  rating: number
  reviewText: string | null
  visibility: 'Public' | 'Friends' | 'Private'
  createdAt: string
  updatedAt: string | null
}

export interface WatchLaterDto {
  id: number
  movie: MovieSummaryDto
  addedAt: string
}

export const useUserStore = defineStore('user', {
  state: () => ({
    reviews: [] as ReviewDto[],
    watchLater: [] as WatchLaterDto[],
  }),

  actions: {
    async fetchMyReviews() {
      const { apiFetch } = useApi()
      const auth = useAuthStore()
      if (!auth.user) return
      try {
        this.reviews = await apiFetch<ReviewDto[]>(`/review/user/${auth.user.id}`)
      } catch { /* keep existing state */ }
    },

    async fetchWatchLater() {
      const { apiFetch } = useApi()
      try {
        this.watchLater = await apiFetch<WatchLaterDto[]>('/watchlater')
      } catch { /* keep existing state */ }
    },

    async addToWatchLater(movieId: number) {
      const { apiFetch } = useApi()
      await apiFetch(`/watchlater/${movieId}`, { method: 'POST' })
      await this.fetchWatchLater()
    },

    async removeFromWatchLater(movieId: number) {
      const { apiFetch } = useApi()
      await apiFetch(`/watchlater/${movieId}`, { method: 'DELETE' })
      this.watchLater = this.watchLater.filter(w => w.movie.id !== movieId)
    },

    async createReview(movieId: number, rating: number, reviewText: string | null, visibility: string) {
      const { apiFetch } = useApi()
      const review = await apiFetch<ReviewDto>('/review', {
        method: 'POST',
        body: { movieId, rating, reviewText, visibility },
      })
      this.reviews.unshift(review)

      const feedStore = useFeedStore()
      if (review.visibility === 'Public') {
        feedStore.publicFeed.unshift(review)
      }
      if (review.visibility === 'Public' || review.visibility === 'Friends') {
        feedStore.friendsFeed.unshift(review)
      }

      return review
    },

    async deleteReview(reviewId: number) {
      const { apiFetch } = useApi()
      await apiFetch(`/review/${reviewId}`, { method: 'DELETE' })
      this.reviews = this.reviews.filter(r => r.id !== reviewId)
    },
  },
})
