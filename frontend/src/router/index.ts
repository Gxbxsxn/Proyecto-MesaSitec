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
    {
      path: '/empleados',
      name: 'empleados-listado',
      component: () => import('@/views/EmpleadosListView.vue'),
      meta: { roles: ['Admin', 'Agente'] },
    },
    {
      path: '/empleados/nuevo',
      name: 'empleados-nuevo',
      component: () => import('@/views/EmpleadoFormView.vue'),
      meta: { roles: ['Admin', 'Agente'] },
    },
    {
      path: '/empleados/:id/editar',
      name: 'empleados-editar',
      component: () => import('@/views/EmpleadoFormView.vue'),
      props: true,
      meta: { roles: ['Admin', 'Agente'] },
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

  const rolesPermitidos = to.meta.roles as string[] | undefined
  if (rolesPermitidos && auth.rol && !rolesPermitidos.includes(auth.rol)) {
    return { name: 'solicitudes-listado' }
  }

  return true
})

export default router
