import { http } from '@/api/http'
import type { Categoria } from '@/types'

export async function listarCategorias(): Promise<Categoria[]> {
  const { data } = await http.get<Categoria[]>('/categorias')
  return data
}
