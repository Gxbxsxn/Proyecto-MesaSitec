# MesaSitec

Mesa de servicio multi-tenant construida para la prueba técnica de Sitecpro.

- **Backend:** .NET 8, EF Core, SQLite, JWT (HS256), Swagger.
- **Frontend:** Vue 3 (`<script setup>`), TypeScript estricto, Vite, Vue Router, Pinia.

## Requisitos previos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) o superior
- [Node.js 20+](https://nodejs.org/) y npm
- No se necesita instalar ninguna base de datos: SQLite se crea como un archivo local.

## Cómo levantar el proyecto (≤ 4 comandos, < 5 minutos)

Desde la raíz del repositorio:

```bash
cp .env.example .env

# 1) Backend — restaura, migra/siembra y arranca en http://localhost:5080
cd backend/src/Api && dotnet run

# 2) Frontend — en otra terminal, desde la raíz del repo
cd frontend && npm install && npm run dev
```

- API disponible en `http://localhost:5080` — `GET /api/v1/health` responde sin token, y Swagger está en `http://localhost:5080/swagger`.
- Frontend disponible en `http://localhost:5173`.
- La base de datos SQLite (`mesasitec.db`) se crea y se siembra sola la primera vez que arranca la API. No hay pasos manuales.

Las variables de entorno del backend (`JWT_SECRET`, `CONNECTION_STRING`, `FRONTEND_ORIGIN`, `SEED_FECHA_BASE`) se leen del entorno del proceso; si usas un archivo `.env`, cárgalo con tu shell o herramienta preferida (por ejemplo `export $(cat .env | xargs)` en bash) antes de `dotnet run`, o simplemente exporta `JWT_SECRET` a mano, que es la única obligatoria para que la app arranque.

## Credenciales de prueba

Contraseña de todos los usuarios semilla: **`Sitec.2026`**

| Email | Organización | Rol |
|---|---|---|
| `admin@norte.test` | Cooperativa Norte | Admin |
| `agente1@norte.test` | Cooperativa Norte | Agente |
| `agente2@norte.test` | Cooperativa Norte | Agente |
| `user1@norte.test` | Cooperativa Norte | Solicitante |
| `user2@norte.test` | Cooperativa Norte | Solicitante |
| `admin@sur.test` | Bufete Sur | Admin |
| `user1@sur.test` | Bufete Sur | Solicitante |

Para reproducir la prueba de aislamiento (RN-01): inicia sesión con `user1@sur.test` e intenta abrir por URL el `id` de una solicitud de Cooperativa Norte → debe responder `404`.

## Correr las pruebas del backend

```bash
cd backend && dotnet test
```

13 pruebas xUnit (mínimo pedido: 8), cubriendo la máquina de estados (RN-02), el cálculo del SLA (RN-04) y las reglas de permisos por rol (RN-03).

## Verificar el tipado estricto del frontend

```bash
cd frontend && npm run type-check
```

## Qué está implementado

- Los 9 endpoints del contrato (sección 6.2), con el formato de error `problem+json` y los códigos exactos pedidos.
- **RN-01** aislamiento multi-tenant (404, no 403, en cualquier acceso cruzado).
- **RN-02** máquina de estados completa, implementada a mano en el dominio.
- **RN-03** permisos por rol para listar, ver, editar y ejecutar transiciones.
- **RN-04** cálculo de SLA en servidor, con recálculo al cambiar categoría/prioridad.
- **RN-05** validación de agente al asignar.
- **RN-06** motivo obligatorio al resolver/cancelar, con los mínimos de caracteres pedidos.
- **RN-07** código correlativo por organización y año.
- Las 4 vistas del frontend (`/login`, `/solicitudes`, `/solicitudes/nueva`, `/solicitudes/:id`, `/solicitudes/:id/editar`), con filtrado/orden/paginación resueltos en el servidor y todos los `data-testid` de la sección 7.4.
- Los botones de acción no permitidos no se renderizan (no solo se deshabilitan), según la sección 7.5.
- Datos semilla deterministas basados en `SEED_FECHA_BASE`.

## Qué NO está implementado (declarado con honestidad)

- **Migraciones formales de EF Core.** Al arrancar, la app usa `Database.EnsureCreated()` en lugar de una migración generada con `dotnet ef migrations add`. No tuve acceso a la herramienta `dotnet-ef` en mi entorno de desarrollo para esta entrega (ver `DECISIONES.md`). El esquema se crea igual de forma automática y sin pasos manuales, pero no hay historial de migraciones incrementales.
- **`docker-compose.yml`** no incluido. Preferí invertir el tiempo restante en dejar sólido el backend, el frontend y las pruebas antes que en un empaquetado Docker no verificado.
- **Generación de DTOs desde OpenAPI** no implementada; los tipos de TypeScript están escritos a mano en `frontend/src/types/index.ts`, mapeando 1:1 el contrato de la sección 6.
- **Un endpoint adicional no contractual**: `GET /api/v1/usuarios/agentes`, usado únicamente para poblar el selector de agente en el modal de "asignar". El enunciado no incluye ningún endpoint para listar usuarios, y sin él la acción "asignar" no se puede completar desde la interfaz. Está protegido con `[Authorize(Roles = "Admin,Agente")]` y filtrado por tenant. Los 9 endpoints del contrato original no fueron modificados.
- No implementé un mecanismo de bloqueo/reintento ante correlativos simultáneos (explícitamente fuera de alcance, según RN-07).
- **Importante — no pude compilar el backend en mi entorno de desarrollo** (sin SDK de .NET ni acceso de red a NuGet). El código fue escrito con cuidado siguiendo las convenciones de C#/EF Core/ASP.NET Core, pero no fue verificado con `dotnet build`/`dotnet test` antes de esta entrega. Es el primer paso que haría con acceso completo al entorno. El frontend sí fue compilado y verificado (`vue-tsc --noEmit` y `vite build` pasan en verde).

## Estructura del repositorio

```
/
├─ README.md
├─ DECISIONES.md
├─ .env.example
├─ backend/
│  ├─ MesaSitec.sln
│  ├─ src/{Api,Aplicacion,Dominio,Infraestructura}
│  └─ tests/
└─ frontend/
   └─ src/{api,components,views,stores,types,router}
```
