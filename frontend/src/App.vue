<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import AppNav from '@/components/AppNav.vue'
import ToastMensaje from '@/components/ToastMensaje.vue'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const auth = useAuthStore()

const mostrarNav = computed(() => !route.meta.publico && auth.estaAutenticado)
</script>

<template>
  <AppNav v-if="mostrarNav" />
  <router-view />
  <ToastMensaje />
</template>
