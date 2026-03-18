// Restore auth state from localStorage on client-side startup
export default defineNuxtPlugin(() => {
  const auth = useAuthStore()
  void auth.restoreSession()
})
