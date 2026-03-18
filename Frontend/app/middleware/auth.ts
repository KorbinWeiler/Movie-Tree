// Redirect unauthenticated users away from protected pages
export default defineNuxtRouteMiddleware(async () => {
  const auth = useAuthStore()

  // Allow the protected page to SSR a loading state; the client will restore
  // the session from localStorage and only then decide whether to redirect.
  if (import.meta.server) {
    return
  }

  await auth.restoreSession()

  if (!auth.isLoggedIn || !auth.user) {
    return navigateTo('/login')
  }
})
