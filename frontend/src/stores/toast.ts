import { defineStore } from 'pinia'

interface ToastState {
  mensaje: string | null
  tipo: 'exito' | 'error'
  contador: number
}

export const useToastStore = defineStore('toast', {
  state: (): ToastState => ({
    mensaje: null,
    tipo: 'exito',
    contador: 0,
  }),

  actions: {
    mostrar(mensaje: string, tipo: 'exito' | 'error' = 'exito') {
      this.mensaje = mensaje
      this.tipo = tipo
      this.contador += 1
      const idActual = this.contador
      setTimeout(() => {
        if (this.contador === idActual) this.mensaje = null
      }, 4000)
    },
  },
})
