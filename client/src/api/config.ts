export function getApiBaseUrl(): string {
  const base = import.meta.env.VITE_API_BASE_URL
  if (base) return base.replace(/\/$/, '')
  return 'http://localhost:5263'
}
