import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { fetchCandidates, searchCandidates } from '../api/candidatesApi'
import { fetchSkills } from '../api/skillsApi'
import type { CandidateResponse, SkillResponse } from '../api/types'
import { ApiRequestError } from '../api/http'
import './CandidatesListPage.css'

export function CandidatesListPage() {
  const [candidates, setCandidates] = useState<CandidateResponse[]>([])
  const [skills, setSkills] = useState<SkillResponse[]>([])
  const [nameFilter, setNameFilter] = useState('')
  const [selectedSkillIds, setSelectedSkillIds] = useState<number[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const loadSkills = useCallback(async () => {
    try {
      setSkills(await fetchSkills())
    } catch (e) {
      console.error(e)
    }
  }, [])

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const hasSearch = nameFilter.trim().length > 0 || selectedSkillIds.length > 0
      const data = hasSearch
        ? await searchCandidates({
            name: nameFilter.trim() || undefined,
            skillIds: selectedSkillIds.length ? selectedSkillIds : undefined,
          })
        : await fetchCandidates()
      setCandidates(data)
    } catch (e) {
      const msg =
        e instanceof ApiRequestError
          ? e.message
          : 'Could not load candidates. Is the API running?'
      setError(msg)
    } finally {
      setLoading(false)
    }
  }, [nameFilter, selectedSkillIds])

  useEffect(() => {
    void loadSkills()
  }, [loadSkills])

  useEffect(() => {
    void load()
  }, [load])

  const toggleSkill = (id: number) => {
    setSelectedSkillIds((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]
    )
  }

  return (
    <div className="page">
      <header className="page__header">
        <div>
          <h1 className="page__title">Candidates</h1>
          <p className="page__lead">
            Search by name and filter by skills. Open a profile to edit details or
            manage skills.
          </p>
        </div>
        <Link to="/candidates/new" className="btn btn--primary">
          Add candidate
        </Link>
      </header>

      <section className="panel">
        <h2 className="panel__title">Search & filters</h2>
        <div className="filters">
          <label className="field">
            <span className="field__label">Name contains</span>
            <input
              className="input"
              value={nameFilter}
              onChange={(e) => setNameFilter(e.target.value)}
              placeholder="e.g. Petrović"
            />
          </label>
          <div className="field field--grow">
            <span className="field__label">Must have skills (all selected)</span>
            <div className="chip-row" role="group" aria-label="Skills filter">
              {skills.length === 0 ? (
                <span className="muted">No skills in directory yet.</span>
              ) : (
                skills.map((s) => (
                  <button
                    key={s.id}
                    type="button"
                    className={`chip ${selectedSkillIds.includes(s.id) ? 'chip--on' : ''}`}
                    onClick={() => toggleSkill(s.id)}
                  >
                    {s.name}
                  </button>
                ))
              )}
            </div>
          </div>
          <div className="filters__actions">
            <button type="button" className="btn btn--ghost" onClick={() => void load()}>
              Refresh
            </button>
            <button
              type="button"
              className="btn btn--secondary"
              onClick={() => {
                setNameFilter('')
                setSelectedSkillIds([])
              }}
            >
              Clear filters
            </button>
          </div>
        </div>
      </section>

      {error && (
        <div className="alert alert--error" role="alert">
          {error}
        </div>
      )}

      <section className="panel panel--table">
        {loading ? (
          <div className="skeleton">Loading candidates…</div>
        ) : candidates.length === 0 ? (
          <div className="empty">
            <p>No candidates match your filters.</p>
            <Link to="/candidates/new" className="btn btn--primary">
              Add the first candidate
            </Link>
          </div>
        ) : (
          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Phone</th>
                  <th>Date of birth</th>
                  <th>Skills</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {candidates.map((c) => (
                  <tr key={c.id}>
                    <td className="table__strong">{c.fullName}</td>
                    <td>{c.email}</td>
                    <td className="muted">{c.contactNumber}</td>
                    <td>{c.dateOfBirth}</td>
                    <td>
                      <div className="tags">
                        {c.skills.slice(0, 4).map((s) => (
                          <span key={s.id} className="tag">
                            {s.name}
                          </span>
                        ))}
                        {c.skills.length > 4 && (
                          <span className="tag tag--more">+{c.skills.length - 4}</span>
                        )}
                      </div>
                    </td>
                    <td className="table__actions">
                      <Link to={`/candidates/${c.id}`} className="btn btn--sm btn--ghost">
                        Open
                      </Link>
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
