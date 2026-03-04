// Restore auth state from localStorage on client-side startup
export default defineNuxtPlugin(() => {
  const auth = useAuthStore()
  auth.init()
  // If we have a token, fetch the current user profile
  if (auth.isLoggedIn && !auth.user) {
    auth.fetchMe()
  }
})
