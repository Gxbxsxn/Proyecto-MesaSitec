import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: '/solicitudes' },
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
      meta: { publico: true },
    },
    {
      path: '/solicitudes',
      name: 'solicitudes-listado',
      component: () => import('@/views/SolicitudesListView.vue'),
    },
    {
      path: '/solicitudes/nueva',
      name: 'solicitudes-nueva',
      component: () => import('@/views/SolicitudFormView.vue'),
    },
    {
      path: '/solicitudes/:id',
      name: 'solicitudes-detalle',
      component: () => import('@/views/SolicitudDetalleView.vue'),
      props: true,
    },
    {
      path: '/solicitudes/:id/editar',
      name: 'solicitudes-editar',
      component: () => import('@/views/SolicitudFormView.vue'),
      props: true,
    },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()

  if (!to.meta.publico && !auth.estaAutenticado) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  if (to.name === 'login' && auth.estaAutenticado) {
    return { name: 'solicitudes-listado' }
  }

  return true
})

export default router
