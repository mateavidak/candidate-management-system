import * as React from 'react'
import type { CreateCandidateRequest } from '../api/types'

export interface CandidateFormValues {
  fullName: string
  dateOfBirth: string
  contactNumber: string
  email: string
}

type Props = {
  initial: CandidateFormValues
  submitLabel: string
  onSubmit: (values: CreateCandidateRequest) => Promise<void>
  onCancel: () => void
  busy?: boolean
}

export function CandidateForm({
  initial,
  submitLabel,
  onSubmit,
  onCancel,
  busy,
}: Props) {
  const [values, setValues] = React.useState<CandidateFormValues>(initial)

  React.useEffect(() => {
    setValues(initial)
  }, [initial])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    await onSubmit({
      fullName: values.fullName.trim(),
      dateOfBirth: values.dateOfBirth,
      contactNumber: values.contactNumber.trim(),
      email: values.email.trim(),
    })
  }

  return (
    <form className="form" onSubmit={(e) => void handleSubmit(e)}>
      <div className="form__grid">
        <label className="field">
          <span className="field__label">Full name</span>
          <input
            className="input"
            required
            maxLength={200}
            value={values.fullName}
            onChange={(e) => setValues((v) => ({ ...v, fullName: e.target.value }))}
          />
        </label>
        <label className="field">
          <span className="field__label">Date of birth</span>
          <input
            className="input"
            type="date"
            required
            value={values.dateOfBirth}
            onChange={(e) => setValues((v) => ({ ...v, dateOfBirth: e.target.value }))}
          />
        </label>
        <label className="field">
          <span className="field__label">Contact number</span>
          <input
            className="input"
            required
            maxLength={50}
            value={values.contactNumber}
            onChange={(e) => setValues((v) => ({ ...v, contactNumber: e.target.value }))}
          />
        </label>
        <label className="field">
          <span className="field__label">Email</span>
          <input
            className="input"
            type="email"
            required
            maxLength={320}
            value={values.email}
            onChange={(e) => setValues((v) => ({ ...v, email: e.target.value }))}
          />
        </label>
      </div>
      <div className="form__actions">
        <button type="button" className="btn btn--secondary" onClick={onCancel} disabled={busy}>
          Cancel
        </button>
        <button type="submit" className="btn btn--primary" disabled={busy}>
          {busy ? 'Saving…' : submitLabel}
        </button>
      </div>
    </form>
  )
}
