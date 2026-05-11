import * as React from 'react'
import type { CreateCandidateRequest } from '../api/types'
import type { CandidateFormValues } from '../types/candidateForm'
import { validateCandidateRequest, type CandidateFieldErrors } from '../validation/candidateRequest'

export type { CandidateFormValues }

type Props = {
  initial: CandidateFormValues
  submitLabel: string
  onSubmit: (values: CreateCandidateRequest) => Promise<void>
  onCancel: () => void
  busy?: boolean
  remoteFieldErrors?: CandidateFieldErrors | null
  onRemoteFieldErrorsConsumed?: () => void
}

export function CandidateForm({
  initial,
  submitLabel,
  onSubmit,
  onCancel,
  busy,
  remoteFieldErrors,
  onRemoteFieldErrorsConsumed,
}: Props) {
  const [values, setValues] = React.useState<CandidateFormValues>(initial)
  const [fieldErrors, setFieldErrors] = React.useState<CandidateFieldErrors>({})
  const emailRef = React.useRef<HTMLInputElement>(null)
  const contactRef = React.useRef<HTMLInputElement>(null)

  React.useEffect(() => {
    setValues(initial)
    setFieldErrors({})
  }, [initial])

  React.useEffect(() => {
    if (!remoteFieldErrors || Object.keys(remoteFieldErrors).length === 0) return
    setFieldErrors((prev) => ({ ...prev, ...remoteFieldErrors }))
    onRemoteFieldErrorsConsumed?.()
  }, [remoteFieldErrors, onRemoteFieldErrorsConsumed])

  const clearFieldError = (key: keyof CandidateFormValues) => {
    setFieldErrors((prev) => {
      if (!prev[key]) return prev
      const next = { ...prev }
      delete next[key]
      return next
    })
  }

  const runContactConstraint = React.useCallback(() => {
    const el = contactRef.current
    if (!el) return
    el.setCustomValidity('')
    const trimmed = values.contactNumber.trim()
    if (trimmed.length === 0) return
    const errs = validateCandidateRequest({
      fullName: values.fullName.trim(),
      dateOfBirth: values.dateOfBirth,
      contactNumber: trimmed,
      email: values.email.trim(),
    })
    if (errs.contactNumber) {
      el.setCustomValidity(errs.contactNumber)
      void el.reportValidity()
    }
  }, [values.contactNumber, values.fullName, values.dateOfBirth, values.email])

  const runEmailConstraint = React.useCallback(() => {
    const el = emailRef.current
    if (!el) return
    el.setCustomValidity('')
    if (!el.checkValidity()) void el.reportValidity()
  }, [])

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    const emailEl = emailRef.current
    const contactEl = contactRef.current

    emailEl?.setCustomValidity('')
    if (emailEl && !emailEl.checkValidity()) {
      void emailEl.reportValidity()
      return
    }

    const trimmedFullName = values.fullName.trim()
    const trimmedContact = values.contactNumber.trim()
    const trimmedEmail = values.email.trim()

    contactEl?.setCustomValidity('')
    const nextErrors = validateCandidateRequest({
      fullName: trimmedFullName,
      dateOfBirth: values.dateOfBirth,
      contactNumber: trimmedContact,
      email: trimmedEmail,
    })
    if (nextErrors.contactNumber && contactEl) {
      contactEl.setCustomValidity(nextErrors.contactNumber)
      void contactEl.reportValidity()
      return
    }

    const { contactNumber: _c, ...rest } = nextErrors
    setFieldErrors(rest)
    if (rest.fullName || rest.dateOfBirth) return

    await onSubmit({
      fullName: trimmedFullName,
      dateOfBirth: values.dateOfBirth,
      contactNumber: trimmedContact,
      email: trimmedEmail,
    })
  }

  const fieldClass = (key: keyof CandidateFormValues) =>
    fieldErrors[key] ? 'field field--invalid' : 'field'

  return (
    <form className="form" noValidate onSubmit={(e) => void handleSubmit(e)}>
      <div className="form__grid">
        <label className={fieldClass('fullName')}>
          <span className="field__label">Full name</span>
          <input
            className="input"
            aria-invalid={fieldErrors.fullName ? true : undefined}
            aria-describedby={fieldErrors.fullName ? 'err-fullName' : undefined}
            maxLength={200}
            value={values.fullName}
            onChange={(e) => {
              clearFieldError('fullName')
              setValues((v) => ({ ...v, fullName: e.target.value }))
            }}
          />
          {fieldErrors.fullName && (
            <span id="err-fullName" className="field__hint field__hint--error" role="alert">
              {fieldErrors.fullName}
            </span>
          )}
        </label>
        <label className={fieldClass('dateOfBirth')}>
          <span className="field__label">Date of birth</span>
          <input
            className="input"
            type="date"
            aria-invalid={fieldErrors.dateOfBirth ? true : undefined}
            aria-describedby={fieldErrors.dateOfBirth ? 'err-dateOfBirth' : undefined}
            value={values.dateOfBirth}
            onChange={(e) => {
              clearFieldError('dateOfBirth')
              setValues((v) => ({ ...v, dateOfBirth: e.target.value }))
            }}
          />
          {fieldErrors.dateOfBirth && (
            <span id="err-dateOfBirth" className="field__hint field__hint--error" role="alert">
              {fieldErrors.dateOfBirth}
            </span>
          )}
        </label>
        <label className={fieldClass('contactNumber')}>
          <span className="field__label">Contact number</span>
          <input
            ref={contactRef}
            className="input"
            type="text"
            inputMode="tel"
            autoComplete="tel"
            aria-invalid={fieldErrors.contactNumber ? true : undefined}
            aria-describedby={fieldErrors.contactNumber ? 'err-contactNumber' : undefined}
            maxLength={50}
            placeholder="+387 61 111 222"
            value={values.contactNumber}
            onChange={(e) => {
              contactRef.current?.setCustomValidity('')
              clearFieldError('contactNumber')
              setValues((v) => ({ ...v, contactNumber: e.target.value }))
            }}
            onBlur={() => runContactConstraint()}
          />
          {fieldErrors.contactNumber && (
            <span id="err-contactNumber" className="field__hint field__hint--error" role="alert">
              {fieldErrors.contactNumber}
            </span>
          )}
        </label>
        <label className={fieldClass('email')}>
          <span className="field__label">Email</span>
          <input
            ref={emailRef}
            className="input"
            type="email"
            autoComplete="email"
            required
            aria-invalid={fieldErrors.email ? true : undefined}
            aria-describedby={fieldErrors.email ? 'err-email' : undefined}
            maxLength={320}
            value={values.email}
            onChange={(e) => {
              emailRef.current?.setCustomValidity('')
              clearFieldError('email')
              setValues((v) => ({ ...v, email: e.target.value }))
            }}
            onBlur={() => runEmailConstraint()}
          />
          {fieldErrors.email && (
            <span id="err-email" className="field__hint field__hint--error" role="alert">
              {fieldErrors.email}
            </span>
          )}
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
