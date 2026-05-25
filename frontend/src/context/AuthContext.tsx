import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { loginUser, logoutUser, refreshAccessToken, setApiAccessToken } from '../services/api'
import type { AuthSession, AuthUser, LoginRequest } from '../types/auth'

type AuthContextValue = {
  user: AuthUser | null
  accessToken: string | null
  isAuthenticated: boolean
  login: (credentials: LoginRequest) => Promise<AuthUser>
  logout: () => Promise<void>
  hasRole: (role: string) => boolean
  hasPermission: (permission: string) => boolean
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)
const storageKey = 'estateiq.auth.session'
const refreshSkewMs = 60_000

function readStoredSession() {
  try {
    const rawSession = window.localStorage.getItem(storageKey)
    if (!rawSession) {
      return null
    }

    const session = JSON.parse(rawSession) as Partial<AuthSession>
    if (!session.accessToken || !session.user) {
      return null
    }

    if (!Array.isArray(session.user.roles) || !Array.isArray(session.user.permissions)) {
      return null
    }

    return session as AuthSession
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthSession | null>(() => readStoredSession())

  useEffect(() => {
    setApiAccessToken(session?.accessToken ?? null)

    if (session) {
      window.localStorage.setItem(storageKey, JSON.stringify(session))
    } else {
      window.localStorage.removeItem(storageKey)
    }
  }, [session])

  useEffect(() => {
    if (!session?.refreshToken || !session.expiresAt) {
      return
    }

    let cancelled = false
    const expiresAtMs = new Date(session.expiresAt).getTime()
    const refreshDelayMs = Math.max(expiresAtMs - Date.now() - refreshSkewMs, 0)

    const timeoutId = window.setTimeout(() => {
      refreshAccessToken(session.refreshToken)
        .then((response) => {
          if (cancelled) {
            return
          }

          setSession((current) => current
            ? {
                ...current,
                accessToken: response.accessToken,
                expiresAt: response.expiresAt,
              }
            : current)
        })
        .catch(() => {
          if (!cancelled) {
            setSession(null)
          }
        })
    }, refreshDelayMs)

    return () => {
      cancelled = true
      window.clearTimeout(timeoutId)
    }
  }, [session?.accessToken, session?.expiresAt, session?.refreshToken])

  const value = useMemo<AuthContextValue>(
    () => ({
      user: session?.user ?? null,
      accessToken: session?.accessToken ?? null,
      isAuthenticated: Boolean(session?.accessToken),
      async login(credentials) {
        const response = await loginUser(credentials)
        setSession({
          accessToken: response.accessToken,
          expiresAt: response.expiresAt,
          refreshToken: response.refreshToken,
          refreshTokenExpiresAt: response.refreshTokenExpiresAt,
          user: response.user,
        })
        return response.user
      },
      async logout() {
        const refreshToken = session?.refreshToken
        setSession(null)

        try {
          await logoutUser(refreshToken)
        } catch {
          // Local logout must succeed even if the server session is already gone.
        }
      },
      hasRole(role) {
        return session?.user.roles?.includes(role) ?? false
      },
      hasPermission(permission) {
        return session?.user.permissions?.includes(permission) ?? false
      },
    }),
    [session],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider')
  }

  return context
}
