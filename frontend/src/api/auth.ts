import { http } from '@/api/http'
import type { LoginResponse, Usuario } from '@/types'

export async function login(email: string, password: string): Promise<LoginResponse> {
  const { data } = await http.post<LoginResponse>('/auth/login', { email, password })
  return data
}

export async function obtenerPerfil(): Promise<Usuario> {
  const { data } = await http.get<Usuario>('/me')
  return data
}
