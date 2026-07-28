import { http } from '@/api/http'
import type {
  CrearSolicitudRequest,
  EditarSolicitudRequest,
  ListadoSolicitudesParams,
  SolicitudDetalle,
  SolicitudPaginada,
  TransicionRequest,
} from '@/types'

export async function listarSolicitudes(params: ListadoSolicitudesParams): Promise<SolicitudPaginada> {
  const { data } = await http.get<SolicitudPaginada>('/solicitudes', { params })
  return data
}

export async function obtenerSolicitud(id: string): Promise<SolicitudDetalle> {
  const { data } = await http.get<SolicitudDetalle>(`/solicitudes/${id}`)
  return data
}

export async function crearSolicitud(payload: CrearSolicitudRequest): Promise<SolicitudDetalle> {
  const { data } = await http.post<SolicitudDetalle>('/solicitudes', payload)
  return data
}

export async function editarSolicitud(id: string, payload: EditarSolicitudRequest): Promise<SolicitudDetalle> {
  const { data } = await http.put<SolicitudDetalle>(`/solicitudes/${id}`, payload)
  return data
}

export async function ejecutarTransicion(id: string, payload: TransicionRequest): Promise<SolicitudDetalle> {
  const { data } = await http.post<SolicitudDetalle>(`/solicitudes/${id}/transiciones`, payload)
  return data
}
