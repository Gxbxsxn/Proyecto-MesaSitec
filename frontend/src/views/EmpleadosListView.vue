<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { bloquearEmpleado, desbloquearEmpleado, listarEmpleados } from '@/api/empleados'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'
import type { EmpleadoListadoItem, ProblemaApi, Rol } from '@/types'

const router = useRouter()
const auth = useAuthStore()
const toast = useToastStore()

const empleados = ref<EmpleadoListadoItem[]>([])
const cargando = ref(false)
const error = ref<string | null>(null)

const filtroRol = ref<Rol | ''>('')
const filtroBusqueda = ref('')

const esAdmin = ref(auth.rol === 'Admin')
const idEnProceso = ref<string | null>(null)

async function cargar() {
  cargando.value = true
  error.value = null
  try {
    empleados.value = await listarEmpleados(filtroRol.value || undefined, filtroBusqueda.value || undefined)
  } catch (e) {
    const problema = e as ProblemaApi
    error.value = problema.detail || 'No se pudo cargar el listado de empleados.'
  } finally {
    cargando.value = false
  }
}

let temporizador: ReturnType<typeof setTimeout> | undefined
watch(filtroBusqueda, () => {
  clearTimeout(temporizador)
  temporizador = setTimeout(cargar, 350)
})
watch(filtroRol, cargar)

onMounted(cargar)

async function alternarBloqueo(empleado: EmpleadoListadoItem) {
  idEnProceso.value = empleado.id
  try {
    const actualizado = empleado.activo ? await bloquearEmpleado(empleado.id) : await desbloquearEmpleado(empleado.id)
    const indice = empleados.value.findIndex((e) => e.id === empleado.id)
    if (indice !== -1) empleados.value[indice] = actualizado
    toast.mostrar(actualizado.activo ? 'Empleado desbloqueado.' : 'Empleado bloqueado.', 'exito')
  } catch (e) {
    const problema = e as ProblemaApi
    toast.mostrar(problema.detail || 'No se pudo cambiar el estado del empleado.', 'error')
  } finally {
    idEnProceso.value = null
  }
}
</script>

<template>
  <div class="contenedor">
    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.4rem;">
      <div>
        <h1 style="font-size: 1.5rem; margin: 0;">Empleados</h1>
        <p style="margin: 0.2rem 0 0; color: var(--tinta-suave); font-size: 0.85rem;">
          Administradores y agentes de tu organización
        </p>
      </div>
      <button data-testid="btn-nuevo-empleado" class="btn btn-primario" @click="router.push({ name: 'empleados-nuevo' })">
        + Nuevo empleado
      </button>
    </div>

    <div class="tarjeta" style="margin-bottom: 1.2rem;">
      <div class="barra-filtros">
        <div class="campo" style="margin: 0;">
          <label>Rol</label>
          <select data-testid="filtro-empleados-rol" v-model="filtroRol">
            <option value="">Todos</option>
            <option value="Admin">Admin</option>
            <option value="Agente">Agente</option>
            <option value="Solicitante">Solicitante</option>
          </select>
        </div>
        <div class="campo" style="margin: 0; grid-column: span 2;">
          <label>Buscar</label>
          <input data-testid="filtro-empleados-busqueda" v-model="filtroBusqueda" type="text" placeholder="Nombre o email" />
        </div>
      </div>
    </div>

    <div class="tarjeta" style="padding: 0; overflow: hidden;">
      <div v-if="cargando" data-testid="listado-empleados-cargando" style="padding: 2rem; text-align: center;">
        Cargando empleados…
      </div>

      <div v-else-if="error" style="padding: 1.5rem; color: var(--color-peligro);">{{ error }}</div>

      <div v-else-if="empleados.length === 0" data-testid="listado-empleados-vacio" style="padding: 2.5rem; text-align: center; color: var(--tinta-suave);">
        No hay empleados que coincidan con los filtros aplicados.
      </div>

      <table v-else data-testid="tabla-empleados">
        <thead>
          <tr>
            <th>Nombre</th>
            <th>Email</th>
            <th>Rol</th>
            <th>Estado</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="empleado in empleados" :key="empleado.id" data-testid="fila-empleado" :data-email="empleado.email">
            <td data-testid="celda-nombre-empleado" style="font-weight: 500;">{{ empleado.nombre }}</td>
            <td data-testid="celda-email-empleado" style="color: var(--tinta-suave);">{{ empleado.email }}</td>
            <td data-testid="celda-rol-empleado">
              <span :class="`pill pill-${empleado.rol === 'Admin' ? 'critica' : empleado.rol === 'Agente' ? 'enproceso' : 'baja'}`">
                {{ empleado.rol }}
              </span>
            </td>
            <td data-testid="celda-estado-empleado">
              <span class="pill" :class="empleado.activo ? 'pill-resuelta' : 'pill-cancelada'">
                {{ empleado.activo ? 'Activo' : 'Bloqueado' }}
              </span>
            </td>
            <td style="display: flex; gap: 0.4rem; justify-content: flex-end;">
              <button data-testid="btn-editar-empleado" class="btn" @click="router.push({ name: 'empleados-editar', params: { id: empleado.id } })">
                Editar
              </button>
              <button
                v-if="esAdmin && empleado.id !== auth.usuario?.id"
                :data-testid="empleado.activo ? 'btn-bloquear-empleado' : 'btn-desbloquear-empleado'"
                class="btn"
                :class="{ 'btn-peligro': empleado.activo }"
                :disabled="idEnProceso === empleado.id"
                @click="alternarBloqueo(empleado)"
              >
                {{ empleado.activo ? 'Bloquear' : 'Desbloquear' }}
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
