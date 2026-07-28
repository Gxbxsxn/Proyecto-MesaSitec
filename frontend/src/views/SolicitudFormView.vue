<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { listarCategorias } from '@/api/categorias'
import { crearSolicitud, editarSolicitud, obtenerSolicitud } from '@/api/solicitudes'
import { useToastStore } from '@/stores/toast'
import type { Categoria, Prioridad, ProblemaApi } from '@/types'

const props = defineProps<{ id?: string }>()

const router = useRouter()
const toast = useToastStore()

const esEdicion = computed(() => Boolean(props.id))

const titulo = ref('')
const descripcion = ref('')
const categoriaId = ref('')
const prioridad = ref<Prioridad | ''>('')

const categorias = ref<Categoria[]>([])
const cargando = ref(true)
const enviando = ref(false)
const errorGeneral = ref<string | null>(null)

const errores = ref<Record<string, string>>({})

const prioridades: Prioridad[] = ['Baja', 'Media', 'Alta', 'Critica']

onMounted(async () => {
  try {
    categorias.value = await listarCategorias()
    if (props.id) {
      const solicitud = await obtenerSolicitud(props.id)
      titulo.value = solicitud.titulo
      descripcion.value = solicitud.descripcion
      categoriaId.value = solicitud.categoria.id
      prioridad.value = solicitud.prioridad
    }
  } catch (e) {
    const problema = e as ProblemaApi
    errorGeneral.value = problema.detail || 'No se pudo cargar el formulario.'
  } finally {
    cargando.value = false
  }
})

function validar(): boolean {
  const nuevosErrores: Record<string, string> = {}

  if (titulo.value.trim().length < 5 || titulo.value.trim().length > 120) {
    nuevosErrores.titulo = 'El título debe tener entre 5 y 120 caracteres.'
  }
  if (descripcion.value.trim().length < 10 || descripcion.value.trim().length > 4000) {
    nuevosErrores.descripcion = 'La descripción debe tener entre 10 y 4000 caracteres.'
  }
  if (!categoriaId.value) {
    nuevosErrores.categoria = 'Selecciona una categoría.'
  }

  errores.value = nuevosErrores
  return Object.keys(nuevosErrores).length === 0
}

async function enviar() {
  errorGeneral.value = null
  if (!prioridad.value || !validar()) return

  enviando.value = true
  try {
    const payload = {
      titulo: titulo.value.trim(),
      descripcion: descripcion.value.trim(),
      categoriaId: categoriaId.value,
      prioridad: prioridad.value,
    }

    const resultado = props.id
      ? await editarSolicitud(props.id, payload)
      : await crearSolicitud(payload)

    toast.mostrar(esEdicion.value ? 'Solicitud actualizada.' : 'Solicitud creada.', 'exito')
    router.push({ name: 'solicitudes-detalle', params: { id: resultado.id } })
  } catch (e) {
    const problema = e as ProblemaApi
    if (problema.errores) {
      const mapeados: Record<string, string> = {}
      for (const [campo, mensajes] of Object.entries(problema.errores)) {
        mapeados[campo] = mensajes[0] ?? 'Valor inválido.'
      }
      errores.value = mapeados
    } else {
      errorGeneral.value = problema.detail || 'No se pudo guardar la solicitud.'
    }
  } finally {
    enviando.value = false
  }
}
</script>

<template>
  <div class="contenedor" style="max-width: 640px;">
    <div class="tarjeta">
      <h1 style="margin-top: 0; font-size: 1.3rem;">{{ esEdicion ? 'Editar solicitud' : 'Nueva solicitud' }}</h1>

      <div v-if="cargando" style="padding: 1rem 0;">Cargando…</div>

      <form v-else @submit.prevent="enviar">
        <p v-if="errorGeneral" style="color: var(--color-peligro);">{{ errorGeneral }}</p>

        <div class="campo">
          <label for="titulo">Título</label>
          <input id="titulo" data-testid="form-titulo" v-model="titulo" type="text" maxlength="120" />
          <span v-if="errores.titulo" data-testid="error-titulo" class="error-campo">{{ errores.titulo }}</span>
        </div>

        <div class="campo">
          <label for="descripcion">Descripción</label>
          <textarea id="descripcion" data-testid="form-descripcion" v-model="descripcion" rows="5" maxlength="4000"></textarea>
          <span v-if="errores.descripcion" data-testid="error-descripcion" class="error-campo">{{ errores.descripcion }}</span>
        </div>

        <div class="campo">
          <label for="categoria">Categoría</label>
          <select id="categoria" data-testid="form-categoria" v-model="categoriaId">
            <option value="" disabled>Selecciona una categoría</option>
            <option v-for="c in categorias" :key="c.id" :value="c.id">{{ c.nombre }} ({{ c.slaHoras }}h)</option>
          </select>
          <span v-if="errores.categoria" data-testid="error-categoria" class="error-campo">{{ errores.categoria }}</span>
        </div>

        <div class="campo">
          <label for="prioridad">Prioridad</label>
          <select id="prioridad" data-testid="form-prioridad" v-model="prioridad">
            <option value="" disabled>Selecciona una prioridad</option>
            <option v-for="p in prioridades" :key="p" :value="p">{{ p }}</option>
          </select>
        </div>

        <div style="display: flex; gap: 0.5rem; justify-content: flex-end;">
          <button type="button" data-testid="form-cancelar" class="btn" @click="router.back()">Cancelar</button>
          <button type="submit" data-testid="form-submit" class="btn btn-primario" :disabled="enviando">
            {{ enviando ? 'Guardando…' : 'Guardar' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
