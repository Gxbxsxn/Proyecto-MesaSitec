export type Rol = 'Admin' | 'Agente' | 'Solicitante'

export type Prioridad = 'Baja' | 'Media' | 'Alta' | 'Critica'

export type Estado = 'Nueva' | 'Asignada' | 'EnProceso' | 'Resuelta' | 'Cerrada' | 'Cancelada'

export type Accion = 'asignar' | 'iniciar' | 'resolver' | 'cerrar' | 'reabrir' | 'cancelar'

export interface Usuario {
  id: string
  nombre: string
  email: string
  rol: Rol
  tenantId: string
  tenantNombre: string
}

export interface LoginResponse {
  accessToken: string
  expiraEn: number
  usuario: Usuario
}

export interface Categoria {
  id: string
  nombre: string
  slaHoras: number
}

export interface CategoriaResumen {
  id: string
  nombre: string
}

export interface AgenteResumen {
  id: string
  nombre: string
}

export interface SolicitanteResumen {
  id: string
  nombre: string
}

export interface SolicitudListadoItem {
  id: string
  codigo: string
  titulo: string
  estado: Estado
  prioridad: Prioridad
  categoria: CategoriaResumen
  agente: AgenteResumen | null
  fechaCreacion: string
  fechaLimiteSla: string
  vencida: boolean
}

export interface SolicitudDetalle extends SolicitudListadoItem {
  descripcion: string
  solicitante: SolicitanteResumen
  fechaResolucion: string | null
  motivoResolucion: string | null
  motivoCancelacion: string | null
}

export interface SolicitudPaginada {
  items: SolicitudListadoItem[]
  page: number
  pageSize: number
  total: number
  totalPaginas: number
}

export interface CrearSolicitudRequest {
  titulo: string
  descripcion: string
  categoriaId: string
  prioridad: Prioridad
}

export type EditarSolicitudRequest = CrearSolicitudRequest

export interface TransicionRequest {
  accion: Accion
  agenteId?: string
  motivo?: string
}

export interface ListadoSolicitudesParams {
  estado?: Estado
  prioridad?: Prioridad
  categoriaId?: string
  agenteId?: string
  q?: string
  vencidas?: boolean
  page?: number
  pageSize?: number
  sort?: string
}

export interface AgenteDisponible {
  id: string
  nombre: string
  email: string
}

export interface EmpleadoListadoItem {
  id: string
  nombre: string
  email: string
  rol: Rol
  activo: boolean
}

export interface CrearEmpleadoRequest {
  nombre: string
  email: string
  rol: Rol
  passwordTemporal: string
}

export interface EditarEmpleadoRequest {
  nombre: string
  email: string
  rol?: Rol
}

export interface ProblemaApi {
  type: string
  title: string
  status: number
  detail: string
  codigo: string
  errores?: Record<string, string[]>
}
