// Typed wrapper around $fetch that injects the base URL and auth token
export function useApi() {
  const config = useRuntimeConfig()

  async function apiFetch<T>(path: string, options: Parameters<typeof $fetch>[1] = {}): Promise<T> {
    const headers: Record<string, string> = {}
    const base = String(config.public.apiBase || '').replace(/\/$/, '')
    const normalizedPath = path.startsWith('/') ? path : `/${path}`

    // Read token lazily so this works both inside and outside Vue setup context
    let token: string | null = null
    try {
      token = useAuthStore().token
    } catch {
      // Outside setup context (e.g. called from a Pinia action) — fall back to localStorage
      if (import.meta.client) {
        token = localStorage.getItem('auth_token')
      }
    }

    if (token) {
      headers['Authorization'] = `Bearer ${token}`
    }

    return $fetch<T>(`${base}${normalizedPath}`, {
      ...options,
      headers: { ...headers, ...(options.headers as Record<string, string> ?? {}) },
    })
  }

  return { apiFetch }
}
