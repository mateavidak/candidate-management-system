import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { createCandidate } from '../api/candidatesApi'
import type { CreateCandidateRequest } from '../api/types'
import { ApiRequestError } from '../api/http'
import { CandidateForm } from '../components/CandidateForm'

export function CandidateNewPage() {
  const navigate = useNavigate()
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const onSubmit = async (body: CreateCandidateRequest) => {
    setBusy(true)
    setError(null)
    try {
      const created = await createCandidate(body)
      navigate(`/candidates/${created.id}`)
    } catch (e) {
      setError(e instanceof ApiRequestError ? e.message : 'Could not create candidate.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="page">
      <header className="page__header">
        <div>
          <h1 className="page__title">New candidate</h1>
          <p className="page__lead">Add a person and assign skills from their profile after saving.</p>
        </div>
        <Link to="/candidates" className="btn btn--ghost">
          Back to list
        </Link>
      </header>

      {error && (
        <div className="alert alert--error" role="alert">
          {error}
        </div>
      )}

      <section className="panel">
        <CandidateForm
          initial={{
            fullName: '',
            dateOfBirth: '1995-01-01',
            contactNumber: '',
            email: '',
          }}
          submitLabel="Create candidate"
          onSubmit={onSubmit}
          onCancel={() => navigate('/candidates')}
          busy={busy}
        />
      </section>
    </div>
  )
}
