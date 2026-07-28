<script setup lang="ts">
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import type { ProblemaApi } from '@/types'

const email = ref('')
const password = ref('')
const error = ref<string | null>(null)
const cargando = ref(false)

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

async function enviar() {
  error.value = null
  cargando.value = true
  try {
    await auth.iniciarSesion(email.value, password.value)
    const destino = (route.query.redirect as string) || '/solicitudes'
    router.push(destino)
  } catch (e) {
    const problema = e as ProblemaApi
    error.value = problema.codigo === 'NO_AUTENTICADO'
      ? 'Email o contraseña incorrectos.'
      : (problema.detail || 'No se pudo iniciar sesión.')
  } finally {
    cargando.value = false
  }
}
</script>

<template>
  <div class="contenedor" style="max-width: 420px; padding-top: 4rem;">
    <div class="tarjeta">
      <h1 style="margin-top: 0; font-size: 1.4rem;">Ingresar a MesaSitec</h1>
      <form @submit.prevent="enviar">
        <div class="campo">
          <label for="email">Email</label>
          <input id="email" data-testid="login-email" v-model="email" type="email" required autocomplete="username" />
        </div>
        <div class="campo">
          <label for="password">Contraseña</label>
          <input id="password" data-testid="login-password" v-model="password" type="password" required autocomplete="current-password" />
        </div>
        <p v-if="error" data-testid="login-error" class="error-campo">{{ error }}</p>
        <button data-testid="login-submit" type="submit" class="btn btn-primario" :disabled="cargando" style="width: 100%;">
          {{ cargando ? 'Ingresando…' : 'Ingresar' }}
        </button>
      </form>
    </div>
  </div>
</template>
