/**
 * Opens the printable ticket in a new tab. Returns false when the browser blocked the pop-up,
 * so the caller can offer a manual "Imprimir" action instead.
 */
export function openTicketPrint(ticketId: number): boolean {
  const printWindow = window.open(`/tickets/${String(ticketId)}/print`, '_blank')
  if (!printWindow) return false
  printWindow.opener = null
  return true
}
