import { http } from '@/api/http'
import type { CrearEmpleadoRequest, EditarEmpleadoRequest, EmpleadoListadoItem, Rol } from '@/types'

export async function listarEmpleados(rol?: Rol, q?: string): Promise<EmpleadoListadoItem[]> {
  const { data } = await http.get<EmpleadoListadoItem[]>('/empleados', { params: { rol, q } })
  return data
}

export async function crearEmpleado(payload: CrearEmpleadoRequest): Promise<EmpleadoListadoItem> {
  const { data } = await http.post<EmpleadoListadoItem>('/empleados', payload)
  return data
}

export async function editarEmpleado(id: string, payload: EditarEmpleadoRequest): Promise<EmpleadoListadoItem> {
  const { data } = await http.put<EmpleadoListadoItem>(`/empleados/${id}`, payload)
  return data
}

export async function bloquearEmpleado(id: string): Promise<EmpleadoListadoItem> {
  const { data } = await http.post<EmpleadoListadoItem>(`/empleados/${id}/bloquear`)
  return data
}

export async function desbloquearEmpleado(id: string): Promise<EmpleadoListadoItem> {
  const { data } = await http.post<EmpleadoListadoItem>(`/empleados/${id}/desbloquear`)
  return data
}
