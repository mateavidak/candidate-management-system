import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { createSkill, deleteSkill, fetchSkills } from '../api/skillsApi'
import type { SkillResponse } from '../api/types'
import { ApiRequestError } from '../api/http'
import './SkillsPage.css'

export function SkillsPage() {
  const [skills, setSkills] = useState<SkillResponse[]>([])
  const [name, setName] = useState('')
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      setSkills(await fetchSkills())
    } catch (e) {
      setError(e instanceof ApiRequestError ? e.message : 'Could not load skills.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  const onCreate = async (e: FormEvent) => {
    e.preventDefault()
    const trimmed = name.trim()
    if (trimmed.length < 2) return
    setBusy(true)
    setError(null)
    try {
      await createSkill({ name: trimmed })
      setName('')
      await load()
    } catch (err) {
      setError(err instanceof ApiRequestError ? err.message : 'Could not create skill.')
    } finally {
      setBusy(false)
    }
  }

  const onDelete = async (id: number, skillName: string) => {
    if (!window.confirm(`Delete skill “${skillName}”?`)) return
    setBusy(true)
    setError(null)
    try {
      await deleteSkill(id)
      await load()
    } catch (err) {
      setError(err instanceof ApiRequestError ? err.message : 'Could not delete skill.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="page skills-page">
      <header className="page__header">
        <div>
          <h1 className="page__title">Skills directory</h1>
          <p className="page__lead">
            Canonical list used when assigning skills to candidates. Names must be unique.
          </p>
        </div>
        <Link to="/candidates" className="btn btn--ghost">
          View candidates
        </Link>
      </header>

      {error && (
        <div className="alert alert--error" role="alert">
          {error}
        </div>
      )}

      <section className="panel skills-page__add">
        <h2 className="panel__title">Add skill</h2>
        <form className="skills-page__form" onSubmit={(e) => void onCreate(e)}>
          <input
            className="input"
            placeholder="e.g. Python programming"
            value={name}
            onChange={(e) => setName(e.target.value)}
            maxLength={200}
            disabled={busy}
          />
          <button type="submit" className="btn btn--primary" disabled={busy || name.trim().length < 2}>
            Add skill
          </button>
        </form>
      </section>

      <section className="panel panel--table">
        {loading ? (
          <div className="skeleton">Loading skills…</div>
        ) : skills.length === 0 ? (
          <div className="empty">
            <p>No skills yet. Add the first one above.</p>
          </div>
        ) : (
          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th className="table__actions">Actions</th>
                </tr>
              </thead>
              <tbody>
                {skills.map((s) => (
                  <tr key={s.id}>
                    <td className="table__strong">{s.name}</td>
                    <td className="table__actions">
                      <button
                        type="button"
                        className="btn btn--sm btn--danger"
                        disabled={busy}
                        onClick={() => void onDelete(s.id, s.name)}
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  )
}
