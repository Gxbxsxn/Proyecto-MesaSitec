<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { obtenerSolicitud, ejecutarTransicion } from '@/api/solicitudes'
import { listarAgentesAsignables } from '@/api/usuarios'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'
import ModalAccion from '@/components/ModalAccion.vue'
import type { Accion, AgenteDisponible, Estado, ProblemaApi, SolicitudDetalle } from '@/types'

const props = defineProps<{ id: string }>()

const router = useRouter()
const auth = useAuthStore()
const toast = useToastStore()

const solicitud = ref<SolicitudDetalle | null>(null)
const cargando = ref(true)
const errorCarga = ref<string | null>(null)
const agentes = ref<AgenteDisponible[]>([])

// RN-02: transiciones permitidas desde cada estado.
const transicionesPorEstado: Record<Estado, Accion[]> = {
  Nueva: ['asignar', 'cancelar'],
  Asignada: ['iniciar', 'asignar', 'cancelar'],
  EnProceso: ['resolver', 'asignar', 'cancelar'],
  Resuelta: ['cerrar', 'reabrir'],
  Cerrada: [],
  Cancelada: [],
}

async function cargar() {
  cargando.value = true
  errorCarga.value = null
  try {
    solicitud.value = await obtenerSolicitud(props.id)
    if (auth.rol === 'Admin' || auth.rol === 'Agente') {
      agentes.value = await listarAgentesAsignables()
    }
  } catch (e) {
    const problema = e as ProblemaApi
    errorCarga.value = problema.codigo === 'RECURSO_NO_ENCONTRADO'
      ? 'Esta solicitud no existe o no pertenece a tu organización.'
      : (problema.detail || 'No se pudo cargar la solicitud.')
  } finally {
    cargando.value = false
  }
}

onMounted(cargar)

const esPropia = computed(() => solicitud.value && auth.usuario && solicitud.value.solicitante.id === auth.usuario.id)

const puedeEditar = computed(() => {
  if (!solicitud.value) return false
  if (auth.rol === 'Admin' || auth.rol === 'Agente') return true
  return auth.rol === 'Solicitante' && esPropia.value === true && solicitud.value.estado === 'Nueva'
})

// RN-03 aplicado sobre las transiciones que el estado actual permite (sección 7.5).
const accionesVisibles = computed(() => {
  if (!solicitud.value) return []
  const posibles = transicionesPorEstado[solicitud.value.estado]

  return posibles.filter((accion) => {
    if (accion === 'cerrar') {
      return auth.rol === 'Admin' || auth.rol === 'Agente' || (auth.rol === 'Solicitante' && esPropia.value)
    }
    if (accion === 'cancelar') {
      return auth.rol === 'Admin'
    }
    // asignar / iniciar / resolver / reabrir
    return auth.rol === 'Admin' || auth.rol === 'Agente'
  })
})

// ---- Modal de acción ----
const modalVisible = ref(false)
const accionActual = ref<Accion>('asignar')
const errorModal = ref<string | null>(null)
const enviandoModal = ref(false)

function abrirModal(accion: Accion) {
  accionActual.value = accion
  errorModal.value = null
  modalVisible.value = true
}

const requiereAgente = computed(() => accionActual.value === 'asignar')
const requiereMotivo = computed(() => accionActual.value === 'resolver' || accionActual.value === 'cancelar')
const minCaracteresMotivo = computed(() => (accionActual.value === 'resolver' ? 20 : 10))

async function confirmarAccion(payload: { agenteId?: string; motivo?: string }) {
  if (!solicitud.value) return
  errorModal.value = null
  enviandoModal.value = true
  try {
    solicitud.value = await ejecutarTransicion(solicitud.value.id, {
      accion: accionActual.value,
      agenteId: payload.agenteId,
      motivo: payload.motivo,
    })
    modalVisible.value = false
    toast.mostrar('Acción aplicada correctamente.', 'exito')
  } catch (e) {
    const problema = e as ProblemaApi
    errorModal.value = problema.detail || 'No se pudo aplicar la acción.'
  } finally {
    enviandoModal.value = false
  }
}
</script>

<template>
  <div class="contenedor">
    <div v-if="cargando" data-testid="listado-cargando" style="padding: 2rem; text-align: center;">Cargando…</div>

    <div v-else-if="errorCarga" style="padding: 1.5rem; color: var(--color-peligro);">{{ errorCarga }}</div>

    <div v-else-if="solicitud" class="tarjeta">
      <div style="display: flex; justify-content: space-between; align-items: flex-start;">
        <div>
          <h1 data-testid="detalle-codigo" style="margin: 0; font-size: 1.3rem;">{{ solicitud.codigo }}</h1>
          <h2 data-testid="detalle-titulo" style="margin: 0.3rem 0 0; font-size: 1.1rem; font-weight: 500;">{{ solicitud.titulo }}</h2>
        </div>
        <span
          v-if="solicitud.vencida"
          data-testid="detalle-vencida"
          class="badge badge-vencida"
        >
          Vencida
        </span>
      </div>

      <p data-testid="detalle-descripcion" style="white-space: pre-wrap;">{{ solicitud.descripcion }}</p>

      <dl style="display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 0.8rem; margin: 1.2rem 0;">
        <div><dt style="color: var(--tinta-suave); font-size: 0.8rem;">Estado</dt><dd data-testid="detalle-estado" style="margin: 0;"><span :class="`pill pill-${solicitud.estado.toLowerCase()}`">{{ solicitud.estado }}</span></dd></div>
        <div><dt style="color: var(--tinta-suave); font-size: 0.8rem;">Prioridad</dt><dd data-testid="detalle-prioridad" style="margin: 0;"><span :class="`pill pill-${solicitud.prioridad.toLowerCase()}`">{{ solicitud.prioridad }}</span></dd></div>
        <div><dt style="color: var(--tinta-suave); font-size: 0.8rem;">Categoría</dt><dd data-testid="detalle-categoria" style="margin: 0;">{{ solicitud.categoria.nombre }}</dd></div>
        <div><dt style="color: var(--tinta-suave); font-size: 0.8rem;">Agente</dt><dd data-testid="detalle-agente" style="margin: 0;">{{ solicitud.agente?.nombre ?? 'Sin asignar' }}</dd></div>
        <div><dt style="color: var(--tinta-suave); font-size: 0.8rem;">Creada</dt><dd data-testid="detalle-fecha-creacion" style="margin: 0;">{{ new Date(solicitud.fechaCreacion).toLocaleString('es-GT') }}</dd></div>
        <div><dt style="color: var(--tinta-suave); font-size: 0.8rem;">Límite SLA</dt><dd data-testid="detalle-fecha-limite" style="margin: 0;">{{ new Date(solicitud.fechaLimiteSla).toLocaleString('es-GT') }}</dd></div>
      </dl>

      <p v-if="solicitud.motivoResolucion || solicitud.motivoCancelacion" data-testid="detalle-motivo">
        <strong>Motivo:</strong> {{ solicitud.motivoResolucion || solicitud.motivoCancelacion }}
      </p>

      <div style="display: flex; gap: 0.5rem; flex-wrap: wrap; margin-top: 1rem;">
        <button v-if="puedeEditar" data-testid="btn-editar" class="btn" @click="router.push({ name: 'solicitudes-editar', params: { id: solicitud.id } })">
          Editar
        </button>
        <button v-if="accionesVisibles.includes('asignar')" data-testid="btn-accion-asignar" class="btn" @click="abrirModal('asignar')">Asignar</button>
        <button v-if="accionesVisibles.includes('iniciar')" data-testid="btn-accion-iniciar" class="btn" @click="abrirModal('iniciar')">Iniciar</button>
        <button v-if="accionesVisibles.includes('resolver')" data-testid="btn-accion-resolver" class="btn btn-primario" @click="abrirModal('resolver')">Resolver</button>
        <button v-if="accionesVisibles.includes('cerrar')" data-testid="btn-accion-cerrar" class="btn btn-primario" @click="abrirModal('cerrar')">Cerrar</button>
        <button v-if="accionesVisibles.includes('reabrir')" data-testid="btn-accion-reabrir" class="btn" @click="abrirModal('reabrir')">Reabrir</button>
        <button v-if="accionesVisibles.includes('cancelar')" data-testid="btn-accion-cancelar" class="btn btn-peligro" @click="abrirModal('cancelar')">Cancelar</button>
      </div>
    </div>

    <ModalAccion
      :visible="modalVisible"
      :accion="accionActual"
      :requiere-agente="requiereAgente"
      :requiere-motivo="requiereMotivo"
      :min-caracteres-motivo="minCaracteresMotivo"
      :agentes="agentes"
      :error="errorModal"
      :enviando="enviandoModal"
      @confirmar="confirmarAccion"
      @cancelar="modalVisible = false"
    />
  </div>
</template>

