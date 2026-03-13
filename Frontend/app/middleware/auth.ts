// Redirect unauthenticated users away from protected pages
export default defineNuxtRouteMiddleware(() => {
  const auth = useAuthStore()
  if (!auth.isLoggedIn) {
    return navigateTo('/login')
  }
})
