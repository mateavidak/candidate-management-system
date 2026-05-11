export interface SkillResponse {
  id: number
  name: string
}

export interface CandidateResponse {
  id: number
  fullName: string
  dateOfBirth: string
  contactNumber: string
  email: string
  skills: SkillResponse[]
}

export interface CreateCandidateRequest {
  fullName: string
  dateOfBirth: string
  contactNumber: string
  email: string
}

export type UpdateCandidateRequest = CreateCandidateRequest

export interface CreateSkillRequest {
  name: string
}

export interface ApiErrorBody {
  code?: string
  message?: string
  title?: string
  errors?: Record<string, string[]>
}
