<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { listarCategorias } from '@/api/categorias'
import { listarSolicitudes } from '@/api/solicitudes'
import AnilloEstadistica from '@/components/AnilloEstadistica.vue'
import type { Categoria, Estado, Prioridad, SolicitudListadoItem, ProblemaApi } from '@/types'

const router = useRouter()

const items = ref<SolicitudListadoItem[]>([])
const categorias = ref<Categoria[]>([])
const cargando = ref(false)
const error = ref<string | null>(null)

const page = ref(1)
const pageSize = 20
const total = ref(0)
const totalPaginas = ref(0)

// Resumen para el panel de anillos. Se calcula con el mismo endpoint de
// listado (pageSize=1, solo nos interesa el "total" que ya calcula el
// servidor), así el número nunca se inventa en el cliente.
const resumen = ref({ total: 0, vencidas: 0, resueltas: 0 })

async function cargarResumen() {
  try {
    const [totales, vencidas, resueltas] = await Promise.all([
      listarSolicitudes({ page: 1, pageSize: 1 }),
      listarSolicitudes({ page: 1, pageSize: 1, vencidas: true }),
      listarSolicitudes({ page: 1, pageSize: 1, estado: 'Resuelta' }),
    ])
    resumen.value = {
      total: totales.total,
      vencidas: vencidas.total,
      resueltas: resueltas.total,
    }
  } catch {
    // El panel de resumen es informativo; si falla, el listado igual funciona.
  }
}

const filtroEstado = ref<Estado | ''>('')
const filtroPrioridad = ref<Prioridad | ''>('')
const filtroCategoria = ref('')
const filtroVencidas = ref(false)
const filtroBusqueda = ref('')

const estados: Estado[] = ['Nueva', 'Asignada', 'EnProceso', 'Resuelta', 'Cerrada', 'Cancelada']
const prioridades: Prioridad[] = ['Baja', 'Media', 'Alta', 'Critica']

function clasePillEstado(estado: Estado) {
  return `pill pill-${estado.toLowerCase()}`
}

function clasePillPrioridad(prioridad: Prioridad) {
  return `pill pill-${prioridad.toLowerCase()}`
}

async function cargar() {
  cargando.value = true
  error.value = null
  try {
    const respuesta = await listarSolicitudes({
      estado: filtroEstado.value || undefined,
      prioridad: filtroPrioridad.value || undefined,
      categoriaId: filtroCategoria.value || undefined,
      vencidas: filtroVencidas.value || undefined,
      q: filtroBusqueda.value || undefined,
      page: page.value,
      pageSize,
      sort: '-fechaCreacion',
    })
    items.value = respuesta.items
    total.value = respuesta.total
    totalPaginas.value = respuesta.totalPaginas
  } catch (e) {
    const problema = e as ProblemaApi
    error.value = problema.detail || 'No se pudo cargar el listado de solicitudes.'
  } finally {
    cargando.value = false
  }
}

function limpiarFiltros() {
  filtroEstado.value = ''
  filtroPrioridad.value = ''
  filtroCategoria.value = ''
  filtroVencidas.value = false
  filtroBusqueda.value = ''
  page.value = 1
}

function irADetalle(id: string) {
  router.push({ name: 'solicitudes-detalle', params: { id } })
}

function anterior() {
  if (page.value > 1) page.value -= 1
}

function siguiente() {
  if (page.value < totalPaginas.value) page.value += 1
}

let temporizadorBusqueda: ReturnType<typeof setTimeout> | undefined

watch([filtroEstado, filtroPrioridad, filtroCategoria, filtroVencidas], () => {
  page.value = 1
  cargar()
})

watch(filtroBusqueda, () => {
  clearTimeout(temporizadorBusqueda)
  temporizadorBusqueda = setTimeout(() => {
    page.value = 1
    cargar()
  }, 350)
})

watch(page, () => cargar())

onMounted(async () => {
  try {
    categorias.value = await listarCategorias()
  } catch {
    // La lista de categorías es una mejora del filtro; si falla, el listado igual funciona.
  }
  await Promise.all([cargar(), cargarResumen()])
})
</script>

<template>
  <div class="contenedor">
    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.4rem;">
      <div>
        <h1 style="font-size: 1.5rem; margin: 0;">Solicitudes</h1>
        <p style="margin: 0.2rem 0 0; color: var(--tinta-suave); font-size: 0.85rem;">Vista general de tu organización</p>
      </div>
      <button
        data-testid="btn-nueva-solicitud"
        class="btn btn-primario"
        @click="router.push({ name: 'solicitudes-nueva' })"
      >
        + Nueva solicitud
      </button>
    </div>

    <div class="panel-resumen">
      <div class="tarjeta tarjeta-anillo">
        <AnilloEstadistica :valor="resumen.total" :total="resumen.total || 1" color="#0f6e5c" />
        <div class="tarjeta-anillo__texto">
          <span class="tarjeta-anillo__etiqueta">Solicitudes</span>
          <span class="tarjeta-anillo__numero">{{ resumen.total }}</span>
          <span class="tarjeta-anillo__detalle">en tu organización</span>
        </div>
      </div>
      <div class="tarjeta tarjeta-anillo">
        <AnilloEstadistica :valor="resumen.vencidas" :total="resumen.total || 1" color="#c1442f" />
        <div class="tarjeta-anillo__texto">
          <span class="tarjeta-anillo__etiqueta">Vencidas</span>
          <span class="tarjeta-anillo__numero">{{ resumen.vencidas }}</span>
          <span class="tarjeta-anillo__detalle">fuera de su SLA</span>
        </div>
      </div>
      <div class="tarjeta tarjeta-anillo">
        <AnilloEstadistica :valor="resumen.resueltas" :total="resumen.total || 1" color="#2c4aa8" />
        <div class="tarjeta-anillo__texto">
          <span class="tarjeta-anillo__etiqueta">Resueltas</span>
          <span class="tarjeta-anillo__numero">{{ resumen.resueltas }}</span>
          <span class="tarjeta-anillo__detalle">esperando cierre o ya cerradas</span>
        </div>
      </div>
    </div>

    <div class="tarjeta" style="margin-bottom: 1.2rem;">
      <div class="barra-filtros">
        <div class="campo" style="margin: 0;">
          <label>Estado</label>
          <select data-testid="filtro-estado" v-model="filtroEstado">
            <option value="">Todos</option>
            <option v-for="e in estados" :key="e" :value="e">{{ e }}</option>
          </select>
        </div>
        <div class="campo" style="margin: 0;">
          <label>Prioridad</label>
          <select data-testid="filtro-prioridad" v-model="filtroPrioridad">
            <option value="">Todas</option>
            <option v-for="p in prioridades" :key="p" :value="p">{{ p }}</option>
          </select>
        </div>
        <div class="campo" style="margin: 0;">
          <label>Categoría</label>
          <select data-testid="filtro-categoria" v-model="filtroCategoria">
            <option value="">Todas</option>
            <option v-for="c in categorias" :key="c.id" :value="c.id">{{ c.nombre }}</option>
          </select>
        </div>
        <div class="campo" style="margin: 0; grid-column: span 2;">
          <label>Buscar</label>
          <input data-testid="filtro-busqueda" v-model="filtroBusqueda" type="text" placeholder="Título, código o descripción" />
        </div>
        <label style="display: flex; align-items: center; gap: 0.4rem; margin: 0 0 0.6rem;">
          <input data-testid="filtro-vencidas" v-model="filtroVencidas" type="checkbox" style="width: auto;" />
          <span style="font-size: 0.85rem;">Solo vencidas</span>
        </label>
        <button data-testid="btn-limpiar-filtros" class="btn" style="height: fit-content;" @click="limpiarFiltros">Limpiar filtros</button>
      </div>
    </div>

    <div class="tarjeta" style="padding: 0; overflow: hidden;">
      <div v-if="cargando" data-testid="listado-cargando" style="padding: 2rem; text-align: center;">
        Cargando solicitudes…
      </div>

      <div v-else-if="error" style="padding: 1.5rem; color: var(--color-peligro);">
        {{ error }}
      </div>

      <div v-else-if="items.length === 0" data-testid="listado-vacio" style="padding: 2.5rem; text-align: center; color: var(--tinta-suave);">
        No hay solicitudes que coincidan con los filtros aplicados.
      </div>

      <template v-else>
        <table data-testid="tabla-solicitudes">
          <thead>
            <tr>
              <th>Código</th>
              <th>Título</th>
              <th>Estado</th>
              <th>Prioridad</th>
              <th>SLA</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="item in items"
              :key="item.id"
              data-testid="fila-solicitud"
              :data-codigo="item.codigo"
              style="cursor: pointer;"
              @click="irADetalle(item.id)"
            >
              <td data-testid="celda-codigo" style="font-variant-numeric: tabular-nums; color: var(--tinta-suave);">{{ item.codigo }}</td>
              <td style="font-weight: 500;">{{ item.titulo }}</td>
              <td data-testid="celda-estado"><span :class="clasePillEstado(item.estado)">{{ item.estado }}</span></td>
              <td data-testid="celda-prioridad"><span :class="clasePillPrioridad(item.prioridad)">{{ item.prioridad }}</span></td>
              <td data-testid="celda-sla">
                {{ new Date(item.fechaLimiteSla).toLocaleString('es-GT') }}
                <span v-if="item.vencida" data-testid="badge-vencida" class="badge badge-vencida">Vencida</span>
              </td>
            </tr>
          </tbody>
        </table>

        <div style="display: flex; justify-content: space-between; align-items: center; padding: 1rem 1.2rem;">
          <span data-testid="paginacion-info" style="font-size: 0.85rem; color: var(--tinta-suave);">
            Página {{ page }} de {{ Math.max(totalPaginas, 1) }} — {{ total }} resultados
          </span>
          <div style="display: flex; gap: 0.5rem;">
            <button data-testid="paginacion-anterior" class="btn" :disabled="page <= 1" @click="anterior">Anterior</button>
            <button data-testid="paginacion-siguiente" class="btn" :disabled="page >= totalPaginas" @click="siguiente">Siguiente</button>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>
