import { useQueryClient } from '@tanstack/react-query'
import { useEffect, useSyncExternalStore } from 'react'

import { dashboardKeys } from '@/features/dashboard/api'
import { spaceKeys } from '@/features/spaces/api'
import { ticketKeys } from '@/features/tickets/api'
import { userKeys } from '@/features/users/api'
import { connectRealtime, realtimeStatus, type RealtimeStatus } from '@/lib/realtime'
import { session } from '@/lib/session'

const isAuthenticated = () => session.getState().status === 'authenticated'

/** Keeps the open views in sync with server-side changes pushed over the realtime hub. */
export function useRealtimeSync() {
  const queryClient = useQueryClient()
  const authenticated = useSyncExternalStore(session.subscribe, isAuthenticated, isAuthenticated)

  useEffect(() => {
    if (!authenticated) return

    const refreshParking = () => {
      for (const queryKey of [spaceKeys.all, ticketKeys.all, dashboardKeys.all]) {
        void queryClient.invalidateQueries({ queryKey })
      }
    }
    const refreshUsers = () => void queryClient.invalidateQueries({ queryKey: userKeys.all })

    return connectRealtime({
      onChanged: (topic) => {
        if (topic === 'users') refreshUsers()
        else refreshParking()
      },
      onResync: () => {
        refreshParking()
        refreshUsers()
      },
    })
  }, [authenticated, queryClient])
}

export function useRealtimeStatus(): RealtimeStatus {
  return useSyncExternalStore(realtimeStatus.subscribe, realtimeStatus.get, realtimeStatus.get)
}
