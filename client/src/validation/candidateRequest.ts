import type { CandidateFormValues } from '../types/candidateForm'

export const CONTACT_PHONE_REGEX = /^\+?[0-9][0-9\s().\-]{5,49}$/

export type CandidateFieldKey = keyof CandidateFormValues

export type CandidateFieldErrors = Partial<Record<CandidateFieldKey, string>>

const aspNetToFormKey: Record<string, CandidateFieldKey> = {
  FullName: 'fullName',
  DateOfBirth: 'dateOfBirth',
  ContactNumber: 'contactNumber',
  Email: 'email',
}

function todayIsoDateUtc(): string {
  const d = new Date()
  const y = d.getUTCFullYear()
  const m = String(d.getUTCMonth() + 1).padStart(2, '0')
  const day = String(d.getUTCDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
}

export function validateCandidateRequest(values: CandidateFormValues): CandidateFieldErrors {
  const errors: CandidateFieldErrors = {}
  const fullName = values.fullName.trim()
  const contact = values.contactNumber.trim()

  if (fullName.length < 2) {
    errors.fullName = 'Full name must be at least 2 characters (not only spaces).'
  }

  if (!values.dateOfBirth) {
    errors.dateOfBirth = 'Date of birth is required.'
  } else if (values.dateOfBirth > todayIsoDateUtc()) {
    errors.dateOfBirth = 'Date of birth cannot be in the future.'
  }

  if (!contact) {
    errors.contactNumber = 'Contact number must be at least 6 characters.'
  } else if (contact.length < 6) {
    errors.contactNumber = 'Contact number must be at least 6 characters.'
  } else if (!CONTACT_PHONE_REGEX.test(contact)) {
    errors.contactNumber = 'Contact number has an invalid format.'
  }

  return errors
}

export function mapAspNetValidationErrors(
  errors: Record<string, string[] | undefined> | undefined
): CandidateFieldErrors {
  if (!errors || typeof errors !== 'object') return {}
  const out: CandidateFieldErrors = {}
  for (const [aspKey, messages] of Object.entries(errors)) {
    const formKey = aspNetToFormKey[aspKey]
    if (!formKey || !messages?.length) continue
    out[formKey] = messages[0]!
  }
  return out
}

export function validationErrorsSummary(errors: Record<string, string[] | undefined> | undefined): string {
  if (!errors || typeof errors !== 'object') return ''
  const lines = Object.entries(errors).flatMap(([key, msgs]) =>
    (Array.isArray(msgs) ? msgs : []).map((m) => `${humanizeFieldName(key)}: ${m}`)
  )
  return lines.join(' ')
}

function humanizeFieldName(aspName: string): string {
  const map: Record<string, string> = {
    FullName: 'Full name',
    DateOfBirth: 'Date of birth',
    ContactNumber: 'Contact number',
    Email: 'Email',
  }
  return map[aspName] ?? aspName
}
