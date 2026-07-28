<script setup lang="ts">
import { useToastStore } from '@/stores/toast'

const toast = useToastStore()
</script>

<template>
  <div
    v-if="toast.mensaje"
    data-testid="toast-mensaje"
    class="toast"
    :style="{ background: toast.tipo === 'error' ? 'var(--color-peligro)' : 'var(--color-exito)' }"
  >
    {{ toast.mensaje }}
  </div>
</template>
