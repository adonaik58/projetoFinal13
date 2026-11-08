import { createFileRoute } from '@tanstack/react-router'

import { auditSearchSchema } from '@/features/audit/search'
import { AuditPage } from '@/features/audit/pages/audit-page'
import { requirePermission } from '@/features/auth/guards'

export const Route = createFileRoute('/_authenticated/_shell/management/audit')({
  validateSearch: auditSearchSchema,
  beforeLoad: requirePermission('audit.view'),
  staticData: { title: 'Auditoria', section: 'Gestão' },
  component: AuditPage,
})
