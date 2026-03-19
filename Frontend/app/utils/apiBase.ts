export function normalizeApiBase(rawBase: string | null | undefined) {
  const base = String(rawBase ?? '').trim().replace(/\/+$/, '')

  if (!base) {
    return '/api'
  }

  if (base.toLowerCase().endsWith('/api')) {
    return base
  }

  return `${base}/api`
}