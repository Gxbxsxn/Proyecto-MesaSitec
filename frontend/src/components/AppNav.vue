<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const router = useRouter()

function salir() {
  auth.cerrarSesion()
  router.push({ name: 'login' })
}
</script>

<template>
  <nav data-testid="app-nav" style="background: white; border-bottom: 1px solid var(--color-borde);">
    <div class="contenedor" style="display: flex; justify-content: space-between; align-items: center; padding: 0.9rem 1.5rem;">
      <router-link :to="{ name: 'solicitudes-listado' }" style="font-weight: 700; color: var(--color-primario); text-decoration: none;">
        MesaSitec
      </router-link>
      <div style="display: flex; align-items: center; gap: 1rem;">
        <span data-testid="nav-usuario-nombre">{{ auth.usuario?.nombre }}</span>
        <span data-testid="nav-usuario-rol" class="badge" style="background: #e0e7ff; color: var(--color-primario);">
          {{ auth.usuario?.rol }}
        </span>
        <button data-testid="btn-logout" class="btn" @click="salir">Salir</button>
      </div>
    </div>
  </nav>
</template>
