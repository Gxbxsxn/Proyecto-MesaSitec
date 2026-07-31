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
# MesaSitec

Mesa de servicio multi tenant creada para la prueba tecnica de Sitecpro.

1. Backend

   `Backend` usa .NET 8, EF Core, SQLite, JWT con HS256, y Swagger para documentacion.

2. Frontend

   `Frontend` usa Vue 3 con `script setup`, TypeScript estricto, Vite, Vue Router y Pinia.

Requisitos previos

1. Instalar el SDK de .NET 8 o superior
2. Instalar Node.js 20 o superior y npm
3. No es necesario instalar un servidor de base de datos, SQLite se crea como archivo local

Arranque rapido desde la raiz del repositorio

```bash
cp .env.example .env

# iniciar la API en http://localhost:5080
cd backend/src/Api && dotnet run

# abrir el frontend en otra terminal
cd frontend && npm install && npm run dev
```

Notas de arranque

1. La API responde en `http://localhost:5080`.
2. `GET /api/v1/health` no pide token.
3. Swagger esta en `http://localhost:5080/swagger`.
4. El archivo de base de datos `mesasitec.db` se crea la primera vez que arranca la API.

Variables de entorno relevantes

1. `JWT_SECRET` obligatorio para emitir tokens
2. `CONNECTION_STRING` si quieres cambiar la ruta de la base de datos
3. `FRONTEND_ORIGIN` para ajustar origenes admitidos por CORS
4. `SEED_FECHA_BASE` para fijar la fecha base de los datos semilla

Credenciales de prueba

Contraseña comun de usuarios semilla: `Sitec.2026`

Emails de prueba

1. `admin@norte.test` rol Admin en Cooperativa Norte
2. `agente1@norte.test` rol Agente en Cooperativa Norte
3. `agente2@norte.test` rol Agente en Cooperativa Norte
4. `user1@norte.test` rol Solicitante en Cooperativa Norte
5. `user2@norte.test` rol Solicitante en Cooperativa Norte
6. `admin@sur.test` rol Admin en Bufete Sur
7. `user1@sur.test` rol Solicitante en Bufete Sur

Prueba de aislamiento

Inicia sesion con `user1@sur.test` e intenta acceder a una entidad que pertenezca a Cooperativa Norte. La API debe responder 404 para indicar aislamiento por tenant.

Ejecutar pruebas del backend

```bash
cd backend && dotnet test
```

Verificar tipado estricto del frontend

```bash
cd frontend && npm run type-check
```

Modulo de empleados

Rutas principales relacionadas con empleados

1. GET ` /api/v1/empleados ` controla el listado con permisos Admin y Agente
2. POST ` /api/v1/empleados ` crea un empleado con reglas de rol aplicadas
3. PUT ` /api/v1/empleados/{id} ` permite editar campos permitidos
4. POST ` /api/v1/empleados/{id}/bloquear ` accion disponible solo para Admin
5. POST ` /api/v1/empleados/{id}/desbloquear ` accion disponible solo para Admin

Reglas clave

1. Un usuario con rol Solicitante no puede acceder a este modulo
2. Un usuario con rol Agente puede crear y editar empleados segun las reglas de negocio
3. No se usa un borrado fisico para no romper integridad referencial, en su lugar se cambia el valor del campo Activo
4. No es posible bloquear al ultimo Admin de una organizacion

Estado de implementacion

1. Endpoints del contrato implementados con formato de error problem+json
2. Aislamiento por tenant implementado
3. Maquina de estados implementada en el dominio
4. Reglas de permisos por rol implementadas
5. Calculo de SLA en servidor implementado
6. Datos semilla deterministas basados en `SEED_FECHA_BASE`

Arranque con Docker

```bash
docker compose up --build
```

Limitaciones actuales y cambios recientes

1. Migraciones formales de EF Core

  Se han incluido las migraciones iniciales en `backend/src/Infraestructura/Persistencia/Migrations`. La aplicacion aplica las migraciones al arrancar, de modo que no es necesario crear el esquema manualmente.

2. Docker compose

  `docker compose up --build` esta disponible y levanta el backend en `http://localhost:5080` y el frontend en `http://localhost:5173`.

3. Tipos de TypeScript

  Los tipos usados por el frontend estan en `frontend/src/types/api.ts`. La generacion automatica desde OpenAPI no esta integrada como flujo automatizado, pero los tipos necesarios ya estan presentes.

4. Endpoint adicional

  Permanece el endpoint `GET /api/v1/usuarios/agentes` que sirve para poblar selectores de agente en la interfaz. Esta ruta esta protegida y filtrada por tenant.

5. Mecanismo de reintento para correlativos simultaneos

  No se implemento un mecanismo avanzado de bloqueo o reintento en la base de datos para colisiones de correlativos; este comportamiento sigue siendo una limitacion funcional.

6. Compilacion del backend

  El backend puede compilarse y ejecutarse localmente con el SDK .NET 8 y las variables de entorno apropiadas. Si el entorno no tiene acceso a NuGet, la restauracion de paquetes puede fallar.

Estructura resumida del repositorio

```
/
README.md
DECISIONES.md
.env.example
backend/
  MesaSitec.sln
  src/{Api,Aplicacion,Dominio,Infraestructura}
  tests/
frontend/
  src/{api,components,views,stores,types,router}
```


