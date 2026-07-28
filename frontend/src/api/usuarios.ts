import { http } from '@/api/http'
import type { AgenteDisponible } from '@/types'

export async function listarAgentesAsignables(): Promise<AgenteDisponible[]> {
  const { data } = await http.get<AgenteDisponible[]>('/usuarios/agentes')
  return data
}
