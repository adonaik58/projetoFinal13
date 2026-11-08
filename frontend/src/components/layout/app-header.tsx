import { Link, useMatches } from '@tanstack/react-router'
import { Fragment, useEffect } from 'react'

import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from '@/components/ui/breadcrumb'
import { Separator } from '@/components/ui/separator'
import { SidebarTrigger } from '@/components/ui/sidebar'

import { ThemeToggle } from './theme-toggle'

export function AppHeader() {
  const matches = useMatches()
  const current = [...matches].reverse().find((match) => match.staticData.title)
  const title = current?.staticData.title
  const section = current?.staticData.section

  useEffect(() => {
    document.title = title ? `${title} · Xyami.Park` : 'Xyami.Park'
  }, [title])

  return (
    <header className="sticky top-0 z-20 flex h-14 shrink-0 items-center gap-2 border-b bg-background/80 px-4 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <SidebarTrigger className="-ml-1" aria-label="Alternar menu lateral" />
      <Separator orientation="vertical" className="mr-2 data-[orientation=vertical]:h-4" />
      <Breadcrumb>
        <BreadcrumbList>
          <BreadcrumbItem className="hidden md:block">
            <BreadcrumbLink asChild>
              <Link to="/">Xyami.Park</Link>
            </BreadcrumbLink>
          </BreadcrumbItem>
          {section && (
            <Fragment>
              <BreadcrumbSeparator className="hidden md:block" />
              <BreadcrumbItem className="hidden md:block">{section}</BreadcrumbItem>
            </Fragment>
          )}
          {title && (
            <Fragment>
              <BreadcrumbSeparator className="hidden md:block" />
              <BreadcrumbItem>
                <BreadcrumbPage>{title}</BreadcrumbPage>
              </BreadcrumbItem>
            </Fragment>
          )}
        </BreadcrumbList>
      </Breadcrumb>
      <div className="ml-auto flex items-center gap-1">
        <ThemeToggle />
      </div>
    </header>
  )
}
