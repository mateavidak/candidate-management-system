import { getApiBaseUrl } from './config'
import type { ApiErrorBody } from './types'

export class ApiRequestError extends Error {
  readonly status: number
  readonly body?: ApiErrorBody

  constructor(message: string, status: number, body?: ApiErrorBody) {
    super(message)
    this.name = 'ApiRequestError'
    this.status = status
    this.body = body
  }
}

async function parseJsonSafe(res: Response): Promise<unknown> {
  const text = await res.text()
  if (!text) return undefined
  try {
    return JSON.parse(text) as unknown
  } catch {
    return undefined
  }
}

export async function apiRequest<T>(
  path: string,
  init?: RequestInit
): Promise<T> {
  const url = `${getApiBaseUrl()}${path.startsWith('/') ? path : `/${path}`}`
  const res = await fetch(url, {
    ...init,
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
      ...init?.headers,
    },
  })

  if (res.status === 204) return undefined as T

  const data = await parseJsonSafe(res)

  if (!res.ok) {
    const body = (typeof data === 'object' && data !== null ? data : {}) as ApiErrorBody
    const msg =
      typeof body.message === 'string'
        ? body.message
        : `${res.status} ${res.statusText}`
    throw new ApiRequestError(msg, res.status, body)
  }

  return data as T
}
