<script setup lang="ts">
import { ref, watch } from 'vue'
import type { AgenteDisponible } from '@/types'

const props = defineProps<{
  visible: boolean
  accion: string
  requiereAgente: boolean
  requiereMotivo: boolean
  minCaracteresMotivo: number
  agentes: AgenteDisponible[]
  error: string | null
  enviando: boolean
}>()

const emit = defineEmits<{
  confirmar: [payload: { agenteId?: string; motivo?: string }]
  cancelar: []
}>()

const agenteId = ref('')
const motivo = ref('')

watch(
  () => props.visible,
  (visible) => {
    if (visible) {
      agenteId.value = ''
      motivo.value = ''
    }
  },
)

function confirmar() {
  emit('confirmar', {
    agenteId: props.requiereAgente ? agenteId.value : undefined,
    motivo: props.requiereMotivo ? motivo.value : undefined,
  })
}

const titulos: Record<string, string> = {
  asignar: 'Asignar solicitud',
  iniciar: 'Iniciar atención',
  resolver: 'Resolver solicitud',
  cerrar: 'Cerrar solicitud',
  reabrir: 'Reabrir solicitud',
  cancelar: 'Cancelar solicitud',
}
</script>

<template>
  <div v-if="visible" data-testid="modal-accion" class="modal-fondo" @click.self="emit('cancelar')">
    <div class="modal-caja">
      <h2 style="margin-top: 0; font-size: 1.1rem;">{{ titulos[accion] ?? accion }}</h2>

      <div v-if="requiereAgente" class="campo">
        <label>Agente</label>
        <select data-testid="modal-select-agente" v-model="agenteId">
          <option value="" disabled>Selecciona un agente</option>
          <option v-for="a in agentes" :key="a.id" :value="a.id">{{ a.nombre }}</option>
        </select>
      </div>

      <div v-if="requiereMotivo" class="campo">
        <label>Motivo (mínimo {{ minCaracteresMotivo }} caracteres)</label>
        <textarea data-testid="modal-motivo" v-model="motivo" rows="3"></textarea>
      </div>

      <p v-if="error" data-testid="modal-error" class="error-campo">{{ error }}</p>

      <div style="display: flex; justify-content: flex-end; gap: 0.5rem; margin-top: 1rem;">
        <button data-testid="modal-cancelar" class="btn" @click="emit('cancelar')">Cancelar</button>
        <button data-testid="modal-confirmar" class="btn btn-primario" :disabled="enviando" @click="confirmar">
          {{ enviando ? 'Enviando…' : 'Confirmar' }}
        </button>
      </div>
    </div>
  </div>
</template>
