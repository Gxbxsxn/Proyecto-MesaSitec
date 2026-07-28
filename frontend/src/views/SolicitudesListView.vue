<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { listarCategorias } from '@/api/categorias'
import { listarSolicitudes } from '@/api/solicitudes'
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

const filtroEstado = ref<Estado | ''>('')
const filtroPrioridad = ref<Prioridad | ''>('')
const filtroCategoria = ref('')
const filtroVencidas = ref(false)
const filtroBusqueda = ref('')

const estados: Estado[] = ['Nueva', 'Asignada', 'EnProceso', 'Resuelta', 'Cerrada', 'Cancelada']
const prioridades: Prioridad[] = ['Baja', 'Media', 'Alta', 'Critica']

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
  await cargar()
})
</script>

<template>
  <div class="contenedor">
    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.2rem;">
      <h1 style="font-size: 1.4rem; margin: 0;">Solicitudes</h1>
      <button
        data-testid="btn-nueva-solicitud"
        class="btn btn-primario"
        @click="router.push({ name: 'solicitudes-nueva' })"
      >
        + Nueva solicitud
      </button>
    </div>

    <div class="tarjeta" style="margin-bottom: 1.2rem;">
      <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); gap: 0.8rem; align-items: end;">
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
        <div class="campo" style="margin: 0;">
          <label>Buscar</label>
          <input data-testid="filtro-busqueda" v-model="filtroBusqueda" type="text" placeholder="Título, código o descripción" />
        </div>
        <label style="display: flex; align-items: center; gap: 0.4rem; margin: 0;">
          <input data-testid="filtro-vencidas" v-model="filtroVencidas" type="checkbox" style="width: auto;" />
          Solo vencidas
        </label>
        <button data-testid="btn-limpiar-filtros" class="btn" @click="limpiarFiltros">Limpiar filtros</button>
      </div>
    </div>

    <div class="tarjeta">
      <div v-if="cargando" data-testid="listado-cargando" style="padding: 2rem; text-align: center;">
        Cargando solicitudes…
      </div>

      <div v-else-if="error" style="padding: 1.5rem; color: var(--color-peligro);">
        {{ error }}
      </div>

      <div v-else-if="items.length === 0" data-testid="listado-vacio" style="padding: 2rem; text-align: center; color: #6b7280;">
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
              <td data-testid="celda-codigo">{{ item.codigo }}</td>
              <td>{{ item.titulo }}</td>
              <td data-testid="celda-estado">{{ item.estado }}</td>
              <td data-testid="celda-prioridad">{{ item.prioridad }}</td>
              <td data-testid="celda-sla">
                {{ new Date(item.fechaLimiteSla).toLocaleString('es-GT') }}
                <span v-if="item.vencida" data-testid="badge-vencida" class="badge badge-vencida">Vencida</span>
              </td>
            </tr>
          </tbody>
        </table>

        <div style="display: flex; justify-content: space-between; align-items: center; padding-top: 1rem;">
          <span data-testid="paginacion-info">
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
