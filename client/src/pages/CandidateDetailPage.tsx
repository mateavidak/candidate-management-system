import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import {
  addCandidateSkill,
  deleteCandidate,
  fetchCandidate,
  removeCandidateSkill,
  updateCandidate,
} from '../api/candidatesApi'
import { fetchSkills } from '../api/skillsApi'
import type { CandidateResponse, CreateCandidateRequest, SkillResponse } from '../api/types'
import { ApiRequestError } from '../api/http'
import { CandidateForm, type CandidateFormValues } from '../components/CandidateForm'

export function CandidateDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const candidateId = Number(id)

  const [candidate, setCandidate] = useState<CandidateResponse | null>(null)
  const [allSkills, setAllSkills] = useState<SkillResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [addSkillId, setAddSkillId] = useState<number | ''>('')

  const load = useCallback(async () => {
    if (!Number.isFinite(candidateId)) {
      setLoading(false)
      return
    }
    setLoading(true)
    setError(null)
    try {
      const [c, s] = await Promise.all([fetchCandidate(candidateId), fetchSkills()])
      setCandidate(c)
      setAllSkills(s)
    } catch (e) {
      if (e instanceof ApiRequestError && e.status === 404) {
        setCandidate(null)
      } else {
        setError(e instanceof ApiRequestError ? e.message : 'Could not load candidate.')
      }
    } finally {
      setLoading(false)
    }
  }, [candidateId])

  useEffect(() => {
    void load()
  }, [load])

  useEffect(() => {
    if (!success) return
    const t = window.setTimeout(() => setSuccess(null), 2800)
    return () => window.clearTimeout(t)
  }, [success])

  const initialForm: CandidateFormValues | null = useMemo(() => {
    if (!candidate) return null
    return {
      fullName: candidate.fullName,
      dateOfBirth: candidate.dateOfBirth,
      contactNumber: candidate.contactNumber,
      email: candidate.email,
    }
  }, [candidate])

  const availableSkills = useMemo(() => {
    if (!candidate) return []
    const have = new Set(candidate.skills.map((s) => s.id))
    return allSkills.filter((s) => !have.has(s.id))
  }, [candidate, allSkills])

  const onUpdate = async (body: CreateCandidateRequest) => {
    if (!Number.isFinite(candidateId)) return
    setBusy(true)
    setError(null)
    setSuccess(null)
    try {
      const updated = await updateCandidate(candidateId, body)
      setCandidate(updated)
      setSuccess('Profile updated.')
    } catch (e) {
      setError(e instanceof ApiRequestError ? e.message : 'Update failed.')
    } finally {
      setBusy(false)
    }
  }

  const onDelete = async () => {
    if (!Number.isFinite(candidateId)) return
    if (!window.confirm('Delete this candidate permanently?')) return
    setBusy(true)
    setError(null)
    try {
      await deleteCandidate(candidateId)
      navigate('/candidates')
    } catch (e) {
      setError(e instanceof ApiRequestError ? e.message : 'Delete failed.')
    } finally {
      setBusy(false)
    }
  }

  const onAddSkill = async () => {
    if (!candidate || addSkillId === '') return
    setBusy(true)
    setError(null)
    setSuccess(null)
    try {
      const updated = await addCandidateSkill(candidate.id, addSkillId)
      setCandidate(updated)
      setAddSkillId('')
      setSuccess('Skill added.')
    } catch (e) {
      setError(e instanceof ApiRequestError ? e.message : 'Could not add skill.')
    } finally {
      setBusy(false)
    }
  }

  const onRemoveSkill = async (skillId: number) => {
    if (!candidate) return
    setBusy(true)
    setError(null)
    setSuccess(null)
    try {
      const updated = await removeCandidateSkill(candidate.id, skillId)
      setCandidate(updated)
      setSuccess('Skill removed.')
    } catch (e) {
      setError(e instanceof ApiRequestError ? e.message : 'Could not remove skill.')
    } finally {
      setBusy(false)
    }
  }

  if (loading) {
    return (
      <div className="page">
        <div className="skeleton">Loading profile…</div>
      </div>
    )
  }

  if (!candidate || !initialForm) {
    return (
      <div className="page">
        <h1 className="page__title">Not found</h1>
        <p className="page__lead muted">This candidate does not exist.</p>
        <Link to="/candidates" className="btn btn--primary">
          Back to list
        </Link>
      </div>
    )
  }

  return (
    <div className="page">
      <header className="page__header">
        <div>
          <h1 className="page__title">{candidate.fullName}</h1>
          <p className="page__lead muted">ID {candidate.id} · update profile and manage skills.</p>
        </div>
        <div className="page__header-actions">
          <Link to="/candidates" className="btn btn--ghost">
            Back to list
          </Link>
          <button type="button" className="btn btn--danger" onClick={() => void onDelete()} disabled={busy}>
            Delete
          </button>
        </div>
      </header>

      {error && (
        <div className="alert alert--error" role="alert">
          {error}
        </div>
      )}
      {success && (
        <div className="alert alert--success" role="status">
          {success}
        </div>
      )}

      <section className="panel">
        <h2 className="panel__title">Profile</h2>
        <CandidateForm
          key={`${candidate.id}-${candidate.fullName}-${candidate.email}-${candidate.skills.map((s) => s.id).join(',')}`}
          initial={initialForm}
          submitLabel="Save changes"
          onSubmit={onUpdate}
          onCancel={() => navigate('/candidates')}
          busy={busy}
        />
      </section>

      <section className="panel">
        <h2 className="panel__title">Skills</h2>
        <div className="skills-block">
          <div className="tags" style={{ marginBottom: '1rem' }}>
            {candidate.skills.length === 0 ? (
              <span className="muted">No skills linked yet.</span>
            ) : (
              candidate.skills.map((s) => (
                <span key={s.id} className="tag tag--pill">
                  {s.name}
                  <button
                    type="button"
                    className="tag__remove"
                    aria-label={`Remove ${s.name}`}
                    onClick={() => void onRemoveSkill(s.id)}
                    disabled={busy}
                  >
                    ×
                  </button>
                </span>
              ))
            )}
          </div>
          <div className="add-skill-row">
            <select
              className="select"
              value={addSkillId === '' ? '' : String(addSkillId)}
              onChange={(e) =>
                setAddSkillId(e.target.value === '' ? '' : Number(e.target.value))
              }
              aria-label="Skill to add"
            >
              <option value="">Select skill to add…</option>
              {availableSkills.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.name}
                </option>
              ))}
            </select>
            <button
              type="button"
              className="btn btn--secondary"
              disabled={busy || addSkillId === '' || availableSkills.length === 0}
              onClick={() => void onAddSkill()}
            >
              Add skill
            </button>
          </div>
        </div>
      </section>
    </div>
  )
}
