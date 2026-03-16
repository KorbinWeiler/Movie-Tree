import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

describe('auth store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    ;(globalThis as any).useRuntimeConfig = () => ({
      public: { apiBase: 'https://localhost:7133/api' },
    })
  })

  it('login stores normalized token and loads normalized profile', async () => {
    ;(globalThis as any).$fetch = vi.fn(async (url: string) => {
      if (url.endsWith('/auth/login')) {
        return { Token: 'abc', ExpiresAt: '2099-01-01T00:00:00Z' }
      }
      if (url.endsWith('/auth/me')) {
        return {
          Id: 'u1',
          UserName: 'alice',
          Email: 'alice@example.com',
          Role: 'Admin',
          ProfilePictureUrl: null,
        }
      }

      throw new Error(`Unexpected URL: ${url}`)
    })

    const { useAuthStore } = await import('../app/stores/auth')
    const store = useAuthStore()

    await store.login('alice@example.com', 'secret')

    expect(store.token).toBe('abc')
    expect(store.expiresAt).toBe('2099-01-01T00:00:00Z')
    expect(store.user?.userName).toBe('alice')
    expect(store.isAdmin).toBe(true)
  })

  it('fetchMe logs out on unauthorized response', async () => {
    ;(globalThis as any).$fetch = vi.fn(async () => {
      throw { statusCode: 401 }
    })

    const { useAuthStore } = await import('../app/stores/auth')
    const store = useAuthStore()

    store.token = 'abc'
    store.expiresAt = '2099-01-01T00:00:00Z'
    store.user = {
      id: 'u1',
      userName: 'alice',
      email: 'alice@example.com',
      role: 'User',
      profilePictureUrl: null,
    }

    await store.fetchMe()

    expect(store.token).toBeNull()
    expect(store.expiresAt).toBeNull()
    expect(store.user).toBeNull()
  })

  it('_setToken throws for invalid payload', async () => {
    const { useAuthStore } = await import('../app/stores/auth')
    const store = useAuthStore()

    expect(() => store._setToken({ token: '', expiresAt: '' } as any)).toThrow(
      'Invalid auth response payload.',
    )
  })
})
