import axios, { type AxiosError } from 'axios'
import type { ProblemaApi } from '@/types'

export const TOKEN_KEY = 'mesasitec_token'
export const USUARIO_KEY = 'mesasitec_usuario'

const baseURL = (import.meta.env.VITE_API_URL as string | undefined) ?? 'http://localhost:5080/api/v1'

export const http = axios.create({ baseURL })

http.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_KEY)
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

http.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ProblemaApi>) => {
    if (error.response?.status === 401) {
      localStorage.removeItem(TOKEN_KEY)
      localStorage.removeItem(USUARIO_KEY)
      if (window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
    }
    return Promise.reject(normalizarError(error))
  },
)

export function normalizarError(error: AxiosError<ProblemaApi>): ProblemaApi {
  if (error.response?.data) {
    return error.response.data
  }
  return {
    type: 'https://mesasitec.local/errores/error-red',
    title: 'Error de red',
    status: 0,
    detail: 'No se pudo contactar al servidor. Verifica tu conexión.',
    codigo: 'ERROR_RED',
  }
}
