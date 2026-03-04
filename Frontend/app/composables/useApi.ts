// Typed wrapper around $fetch that injects the base URL and auth token
export function useApi() {
  const config = useRuntimeConfig()
  const authStore = useAuthStore()

  async function apiFetch<T>(path: string, options: Parameters<typeof $fetch>[1] = {}): Promise<T> {
    const headers: Record<string, string> = {}
    if (authStore.token) {
      headers['Authorization'] = `Bearer ${authStore.token}`
    }
    return $fetch<T>(`${config.public.apiBase}${path}`, {
      ...options,
      headers: { ...headers, ...(options.headers as Record<string, string> ?? {}) },
    })
  }

  return { apiFetch }
}
