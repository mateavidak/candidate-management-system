import { apiRequest } from './http'
import type { CreateSkillRequest, SkillResponse } from './types'

export async function fetchSkills(): Promise<SkillResponse[]> {
  return apiRequest<SkillResponse[]>('/api/skills')
}

export async function createSkill(body: CreateSkillRequest): Promise<SkillResponse> {
  return apiRequest<SkillResponse>('/api/skills', {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

export async function deleteSkill(id: number): Promise<void> {
  await apiRequest<unknown>(`/api/skills/${id}`, { method: 'DELETE' })
}
