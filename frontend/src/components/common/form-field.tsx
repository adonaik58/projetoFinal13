import type { ReactNode } from 'react'

import { Field, FieldDescription, FieldError, FieldLabel } from '@/components/ui/field'

export interface FieldControlProps {
  id: string
  'aria-invalid': boolean
  'aria-describedby': string | undefined
}

/** Label + control + description + error, wired together for screen readers. */
export function FormField({
  id,
  label,
  error,
  description,
  className,
  children,
}: {
  id: string
  label: ReactNode
  error?: { message?: string | undefined } | undefined
  description?: ReactNode
  className?: string
  children: (control: FieldControlProps) => ReactNode
}) {
  const invalid = !!error
  const descriptionId = description ? `${id}-description` : undefined
  const errorId = invalid ? `${id}-error` : undefined
  const describedBy = [descriptionId, errorId].filter(Boolean).join(' ') || undefined

  return (
    <Field data-invalid={invalid} className={className}>
      <FieldLabel htmlFor={id}>{label}</FieldLabel>
      {children({ id, 'aria-invalid': invalid, 'aria-describedby': describedBy })}
      {description && <FieldDescription id={descriptionId}>{description}</FieldDescription>}
      <FieldError id={errorId} errors={[error]} />
    </Field>
  )
}
