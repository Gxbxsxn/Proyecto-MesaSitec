<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { crearEmpleado, editarEmpleado, listarEmpleados } from '@/api/empleados'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'
import type { ProblemaApi, Rol } from '@/types'

const props = defineProps<{ id?: string }>()

const router = useRouter()
const auth = useAuthStore()
const toast = useToastStore()

const esEdicion = computed(() => Boolean(props.id))
const esAdmin = computed(() => auth.rol === 'Admin')

const nombre = ref('')
const email = ref('')
const rol = ref<Rol>('Solicitante')
const passwordTemporal = ref('')

const cargando = ref(true)
const enviando = ref(false)
const errorGeneral = ref<string | null>(null)
const errores = ref<Record<string, string>>({})

// Un Agente solo puede dar de alta Solicitantes (RN-08); un Admin puede elegir cualquier rol.
const rolesDisponibles = computed<Rol[]>(() => (esAdmin.value ? ['Admin', 'Agente', 'Solicitante'] : ['Solicitante']))

onMounted(async () => {
  try {
    if (props.id) {
      // No hay un GET /empleados/{id} individual; reutilizamos el listado y filtramos.
      // Es una decisión pragmática: evita duplicar un endpoint solo para precargar el formulario.
      const todos = await listarEmpleados()
      const encontrado = todos.find((e) => e.id === props.id)
      if (!encontrado) {
        errorGeneral.value = 'No se encontró ese empleado.'
      } else {
        nombre.value = encontrado.nombre
        email.value = encontrado.email
        rol.value = encontrado.rol
      }
    } else if (!esAdmin.value) {
      rol.value = 'Solicitante'
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

  if (nombre.value.trim().length < 3 || nombre.value.trim().length > 120)
    nuevosErrores.nombre = 'El nombre debe tener entre 3 y 120 caracteres.'
  if (!email.value.includes('@'))
    nuevosErrores.email = 'El email no es válido.'
  if (!esEdicion.value && passwordTemporal.value.length < 8)
    nuevosErrores.passwordTemporal = 'La contraseña temporal debe tener al menos 8 caracteres.'

  errores.value = nuevosErrores
  return Object.keys(nuevosErrores).length === 0
}

async function enviar() {
  errorGeneral.value = null
  if (!validar()) return

  enviando.value = true
  try {
    if (props.id) {
      await editarEmpleado(props.id, {
        nombre: nombre.value.trim(),
        email: email.value.trim(),
        rol: esAdmin.value ? rol.value : undefined,
      })
      toast.mostrar('Empleado actualizado.', 'exito')
    } else {
      await crearEmpleado({
        nombre: nombre.value.trim(),
        email: email.value.trim(),
        rol: rol.value,
        passwordTemporal: passwordTemporal.value,
      })
      toast.mostrar('Empleado creado.', 'exito')
    }
    router.push({ name: 'empleados-listado' })
  } catch (e) {
    const problema = e as ProblemaApi
    if (problema.errores) {
      const mapeados: Record<string, string> = {}
      for (const [campo, mensajes] of Object.entries(problema.errores)) {
        mapeados[campo] = mensajes[0] ?? 'Valor inválido.'
      }
      errores.value = mapeados
    } else {
      errorGeneral.value = problema.detail || 'No se pudo guardar el empleado.'
    }
  } finally {
    enviando.value = false
  }
}
</script>

<template>
  <div class="contenedor" style="max-width: 560px;">
    <div class="tarjeta">
      <h1 style="margin-top: 0; font-size: 1.3rem;">{{ esEdicion ? 'Editar empleado' : 'Nuevo empleado' }}</h1>

      <div v-if="cargando" style="padding: 1rem 0;">Cargando…</div>

      <form v-else @submit.prevent="enviar">
        <p v-if="errorGeneral" style="color: var(--color-peligro);">{{ errorGeneral }}</p>

        <div class="campo">
          <label for="empleado-nombre">Nombre</label>
          <input id="empleado-nombre" data-testid="form-empleado-nombre" v-model="nombre" type="text" maxlength="120" />
          <span v-if="errores.nombre" data-testid="error-empleado-nombre" class="error-campo">{{ errores.nombre }}</span>
        </div>

        <div class="campo">
          <label for="empleado-email">Email</label>
          <input id="empleado-email" data-testid="form-empleado-email" v-model="email" type="email" />
          <span v-if="errores.email" data-testid="error-empleado-email" class="error-campo">{{ errores.email }}</span>
        </div>

        <div class="campo">
          <label for="empleado-rol">Rol</label>
          <select id="empleado-rol" data-testid="form-empleado-rol" v-model="rol" :disabled="!esAdmin">
            <option v-for="r in rolesDisponibles" :key="r" :value="r">{{ r }}</option>
          </select>
          <span v-if="!esAdmin" style="font-size: 0.78rem; color: var(--tinta-suave);">
            Solo un Admin puede elegir o cambiar el rol.
          </span>
        </div>

        <div v-if="!esEdicion" class="campo">
          <label for="empleado-password">Contraseña temporal</label>
          <input id="empleado-password" data-testid="form-empleado-password" v-model="passwordTemporal" type="text" minlength="8" />
          <span v-if="errores.passwordTemporal" data-testid="error-empleado-password" class="error-campo">{{ errores.passwordTemporal }}</span>
        </div>

        <div style="display: flex; gap: 0.5rem; justify-content: flex-end;">
          <button type="button" class="btn" @click="router.back()">Cancelar</button>
          <button type="submit" data-testid="form-empleado-submit" class="btn btn-primario" :disabled="enviando">
            {{ enviando ? 'Guardando…' : 'Guardar' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
