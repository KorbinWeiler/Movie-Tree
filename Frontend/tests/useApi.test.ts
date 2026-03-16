import { describe, expect, it, vi } from 'vitest'

describe('useApi', () => {
  it('injects base URL, normalizes path and adds bearer token header', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true })

    ;(globalThis as any).useRuntimeConfig = () => ({
      public: { apiBase: 'https://localhost:7133/api/' },
    })
    ;(globalThis as any).useAuthStore = () => ({ token: 'token-123' })
    ;(globalThis as any).$fetch = fetchMock

    const { useApi } = await import('../app/composables/useApi')
    await useApi().apiFetch('movie/trending', {
      method: 'GET',
      headers: { 'X-Test': '1' },
    })

    expect(fetchMock).toHaveBeenCalledTimes(1)
    expect(fetchMock).toHaveBeenCalledWith(
      'https://localhost:7133/api/movie/trending',
      expect.objectContaining({
        method: 'GET',
        headers: expect.objectContaining({
          Authorization: 'Bearer token-123',
          'X-Test': '1',
        }),
      }),
    )
  })

  it('does not add authorization when token cannot be resolved', async () => {
    const fetchMock = vi.fn().mockResolvedValue({ ok: true })

    ;(globalThis as any).useRuntimeConfig = () => ({
      public: { apiBase: 'https://localhost:7133/api' },
    })
    ;(globalThis as any).useAuthStore = () => {
      throw new Error('outside setup context')
    }
    ;(globalThis as any).$fetch = fetchMock

    const { useApi } = await import('../app/composables/useApi')
    await useApi().apiFetch('/movie')

    const [, options] = fetchMock.mock.calls[0]
    expect(options.headers).not.toHaveProperty('Authorization')
  })
})
