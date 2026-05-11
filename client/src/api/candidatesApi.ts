import { apiRequest } from './http'
import type { CandidateResponse, CreateCandidateRequest, UpdateCandidateRequest } from './types'

export async function fetchCandidates(): Promise<CandidateResponse[]> {
  return apiRequest<CandidateResponse[]>('/api/candidates')
}

export async function searchCandidates(params: {
  name?: string
  skillIds?: number[]
}): Promise<CandidateResponse[]> {
  const qs = new URLSearchParams()
  if (params.name?.trim()) qs.set('name', params.name.trim())
  for (const id of params.skillIds ?? []) {
    qs.append('skillIds', String(id))
  }
  const q = qs.toString()
  return apiRequest<CandidateResponse[]>(
    `/api/candidates/search${q ? `?${q}` : ''}`
  )
}

export async function fetchCandidate(id: number): Promise<CandidateResponse> {
  return apiRequest<CandidateResponse>(`/api/candidates/${id}`)
}

export async function createCandidate(
  body: CreateCandidateRequest
): Promise<CandidateResponse> {
  return apiRequest<CandidateResponse>('/api/candidates', {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

export async function updateCandidate(
  id: number,
  body: UpdateCandidateRequest
): Promise<CandidateResponse> {
  return apiRequest<CandidateResponse>(`/api/candidates/${id}`, {
    method: 'PUT',
    body: JSON.stringify(body),
  })
}

export async function deleteCandidate(id: number): Promise<void> {
  await apiRequest<unknown>(`/api/candidates/${id}`, { method: 'DELETE' })
}

export async function addCandidateSkill(
  candidateId: number,
  skillId: number
): Promise<CandidateResponse> {
  return apiRequest<CandidateResponse>(
    `/api/candidates/${candidateId}/skills/${skillId}`,
    { method: 'POST' }
  )
}

export async function removeCandidateSkill(
  candidateId: number,
  skillId: number
): Promise<CandidateResponse> {
  return apiRequest<CandidateResponse>(
    `/api/candidates/${candidateId}/skills/${skillId}`,
    { method: 'DELETE' }
  )
}
