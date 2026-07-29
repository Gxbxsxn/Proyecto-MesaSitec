<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(
  defineProps<{
    valor: number
    total: number
    color: string
    tamano?: number
  }>(),
  { tamano: 64 },
)

const radio = computed(() => props.tamano / 2 - 6)
const circunferencia = computed(() => 2 * Math.PI * radio.value)
const proporcion = computed(() => (props.total > 0 ? Math.min(props.valor / props.total, 1) : 0))
const desplazamiento = computed(() => circunferencia.value * (1 - proporcion.value))
</script>

<template>
  <svg :width="tamano" :height="tamano" :viewBox="`0 0 ${tamano} ${tamano}`" role="img" aria-hidden="true">
    <circle class="anillo-track" :cx="tamano / 2" :cy="tamano / 2" :r="radio" stroke-width="6" />
    <circle
      :cx="tamano / 2"
      :cy="tamano / 2"
      :r="radio"
      fill="none"
      stroke-width="6"
      stroke-linecap="round"
      :stroke="color"
      :stroke-dasharray="circunferencia"
      :stroke-dashoffset="desplazamiento"
      :transform="`rotate(-90 ${tamano / 2} ${tamano / 2})`"
      style="transition: stroke-dashoffset 0.4s ease;"
    />
  </svg>
</template>
