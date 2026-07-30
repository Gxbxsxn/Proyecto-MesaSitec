<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useToastStore } from '@/stores/toast'
import type { ProblemaApi } from '@/types'

const RECORDAR_KEY = 'mesasitec_email_recordado'

const email = ref('')
const password = ref('')
const recordar = ref(false)
const error = ref<string | null>(null)
const cargando = ref(false)

const auth = useAuthStore()
const toast = useToastStore()
const router = useRouter()
const route = useRoute()

onMounted(() => {
  const guardado = localStorage.getItem(RECORDAR_KEY)
  if (guardado) {
    email.value = guardado
    recordar.value = true
  }
})

async function enviar() {
  error.value = null
  cargando.value = true
  try {
    await auth.iniciarSesion(email.value, password.value)

    if (recordar.value) {
      localStorage.setItem(RECORDAR_KEY, email.value)
    } else {
      localStorage.removeItem(RECORDAR_KEY)
    }

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

function mostrarAyudaContrasena() {
  toast.mostrar('Pide a un Admin de tu organización que restablezca tu contraseña.', 'exito')
}
</script>

<template>
  <div class="pantalla-login d-flex align-items-center justify-content-center">
    <div class="tarjeta-login card border-0">
      <div class="card-body p-4 p-md-5">
        <h1 class="h4 fw-bold text-white text-center mb-4">Ingresar a MesaSitec</h1>

        <form @submit.prevent="enviar" novalidate>
          <div class="mb-3">
            <div class="input-group input-group-login">
              <span class="input-group-text"><i class="bi bi-person-fill"></i></span>
              <input
                data-testid="login-email"
                v-model="email"
                type="email"
                class="form-control"
                placeholder="Email"
                required
                autocomplete="username"
              />
            </div>
          </div>

          <div class="mb-3">
            <div class="input-group input-group-login">
              <span class="input-group-text"><i class="bi bi-lock-fill"></i></span>
              <input
                data-testid="login-password"
                v-model="password"
                type="password"
                class="form-control"
                placeholder="Contraseña"
                required
                autocomplete="current-password"
              />
            </div>
          </div>

          <div class="form-check mb-3">
            <input
              id="recordar"
              data-testid="login-remember"
              v-model="recordar"
              type="checkbox"
              class="form-check-input"
            />
            <label class="form-check-label text-white-50 small" for="recordar">Recordarme</label>
          </div>

          <p v-if="error" data-testid="login-error" class="text-warning small mb-3">{{ error }}</p>

          <button
            data-testid="login-submit"
            type="submit"
            class="btn btn-login w-100 fw-semibold text-uppercase"
            :disabled="cargando"
          >
            {{ cargando ? 'Ingresando…' : 'Ingresar' }}
          </button>
        </form>
      </div>

      <div class="card-footer border-0 bg-transparent text-center pb-4">
        <span class="text-white-50 small">¿Sin cuenta? </span>
        <span class="text-white small fw-semibold">Pídesela a un administrador de tu organización</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.pantalla-login {
  min-height: 100vh;
  background:
    radial-gradient(circle at 15% 20%, rgba(255, 255, 255, 0.08), transparent 40%),
    radial-gradient(circle at 85% 80%, rgba(255, 255, 255, 0.06), transparent 45%),
    linear-gradient(135deg, #0a3d32 0%, var(--primario) 55%, #123a52 100%);
  padding: 1.5rem;
}

.tarjeta-login {
  width: 100%;
  max-width: 400px;
  background: rgba(12, 30, 26, 0.55);
  backdrop-filter: blur(10px);
  border-radius: 16px;
  box-shadow: 0 25px 60px rgba(0, 0, 0, 0.35);
}

.input-group-login .input-group-text {
  background: rgba(255, 255, 255, 0.08);
  border: 1px solid rgba(255, 255, 255, 0.15);
  border-right: none;
  color: rgba(255, 255, 255, 0.65);
}

.input-group-login .form-control {
  background: rgba(255, 255, 255, 0.08);
  border: 1px solid rgba(255, 255, 255, 0.15);
  border-left: none;
  color: white;
}

.input-group-login .form-control::placeholder {
  color: rgba(255, 255, 255, 0.5);
}

.input-group-login .form-control:focus {
  background: rgba(255, 255, 255, 0.12);
  box-shadow: none;
  border-color: rgba(255, 255, 255, 0.3);
  color: white;
}

.form-check-input:checked {
  background-color: var(--primario);
  border-color: var(--primario);
}

.btn-login {
  background: linear-gradient(135deg, #17a589, var(--primario));
  border: none;
  color: white;
  padding: 0.7rem;
  letter-spacing: 0.04em;
  font-size: 0.85rem;
  border-radius: 999px;
  transition: filter 0.15s ease;
}

.btn-login:hover:not(:disabled) {
  filter: brightness(1.1);
  color: white;
}

.btn-login:disabled {
  opacity: 0.6;
}
</style>
