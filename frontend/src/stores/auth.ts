import { defineStore } from 'pinia'
import { login as apiLogin } from '@/api/auth'
import { TOKEN_KEY, USUARIO_KEY } from '@/api/http'
import type { Usuario } from '@/types'

interface AuthState {
  usuario: Usuario | null
  token: string | null
}

function leerUsuarioGuardado(): Usuario | null {
  const crudo = localStorage.getItem(USUARIO_KEY)
  if (!crudo) return null
  try {
    return JSON.parse(crudo) as Usuario
  } catch {
    return null
  }
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    usuario: leerUsuarioGuardado(),
    token: localStorage.getItem(TOKEN_KEY),
  }),

  getters: {
    estaAutenticado: (state) => Boolean(state.token && state.usuario),
    rol: (state) => state.usuario?.rol ?? null,
  },

  actions: {
    async iniciarSesion(email: string, password: string) {
      const respuesta = await apiLogin(email, password)
      this.token = respuesta.accessToken
      this.usuario = respuesta.usuario
      localStorage.setItem(TOKEN_KEY, respuesta.accessToken)
      localStorage.setItem(USUARIO_KEY, JSON.stringify(respuesta.usuario))
    },

    cerrarSesion() {
      this.token = null
      this.usuario = null
      localStorage.removeItem(TOKEN_KEY)
      localStorage.removeItem(USUARIO_KEY)
    },
  },
})
