# Decisiones — MesaSitec

## 1. Tres decisiones técnicas (con la alternativa descartada)

**a) `Database.EnsureCreated()` en vez de migraciones formales de EF Core.**
Mi entorno de desarrollo para esta entrega no tenía el SDK de .NET instalado ni acceso de red al feed de NuGet, así que no pude ejecutar `dotnet ef migrations add`. Descarté escribir a mano los archivos de migración (`Up`/`Down` + snapshot del modelo) porque sin el compilador para verificarlos, el riesgo de dejar el proyecto en un estado que no arranca era más alto que el beneficio de "aparentar" migraciones reales. `EnsureCreated()` cumple el requisito de "se aplica automáticamente al arrancar, sin pasos manuales", aunque pierde el historial incremental que sí tendría una migración real. Si tuviera el entorno completo, el primer cambio que haría sería reemplazarlo por una migración `InitialCreate` generada con la herramienta.

**b) La capa `Aplicacion` referencia directamente a `Infraestructura` (a `MesaSitecDbContext`), en vez de definir interfaces de repositorio.**
Descarté el patrón Repository/Unit of Work completo (interfaces en `Aplicacion`, implementación en `Infraestructura`) porque, para el tamaño de este ejercicio (4 entidades, 9 endpoints), añadía una capa de indirección sin beneficio real de testeo: la lógica que sí necesita probarse sin infraestructura (máquina de estados, SLA, permisos) ya vive en `Dominio` como funciones puras y estáticas, sin ninguna dependencia de EF Core. Los servicios de `Aplicacion` sí dependen del `DbContext` directamente. Es una desviación consciente de una arquitectura en capas "pura", hecha por velocidad.

**c) 404 (no 403) para cualquier intento de acceso cruzado entre tenants, incluso antes de saber si el usuario tendría permiso para verlo.**
En `SolicitudService`, el filtro por `tenantId` ocurre primero y siempre; solo si el recurso existe *dentro del propio tenant* se evalúan los permisos de rol. Descarté la alternativa de "primero busco el recurso sin filtrar por tenant, y si no soy del mismo tenant devuelvo 404, si soy del mismo tenant pero no tengo permiso devuelvo 403" porque agregaba una consulta extra sin filtro de tenant (mayor superficie de error) para un beneficio marginal. La regla RN-01 es la más importante del ejercicio, así que preferí la implementación más simple de auditar: una sola condición de filtrado (`WHERE TenantId = @actual`) en cada consulta.

**d) "Bloquear" en vez de "eliminar" para el módulo de empleados.**
El cliente pidió poder restringir cuentas de empleados, y que los agentes solo puedan "editar y agregar, no eliminar". Descarté implementar un DELETE real porque un `Usuario` puede estar referenciado por `Solicitud.SolicitanteId` o `Solicitud.AgenteId` (con `OnDelete(DeleteBehavior.Restrict)`), así que borrarlo de la base de datos fallaría o dejaría solicitudes huérfanas en cuanto esa persona tuviera al menos una solicitud a su nombre — que es prácticamente siempre. En su lugar, la acción "destructiva" es marcar `Activo = false` (bloquear), que impide iniciar sesión pero conserva intacto el historial. Esto también responde de forma más natural al pedido de "bloquear a los administradores": un Admin puede bloquear a otro Admin o Agente, pero nunca a sí mismo ni al último Admin activo de su organización (para no dejar la organización sin nadie que administre).

## 1.1 Decisión de producto (fuera del contrato original): módulo de empleados

El enunciado original no incluía gestión de personal. El cliente pidió explícitamente, en una iteración posterior: (1) una pantalla para ver empleados por organización, (2) poder bloquear administradores, (3) que los agentes puedan crear/editar empleados pero no bloquear ni eliminar. Implementé esto como un módulo nuevo (`EmpleadoService` / `EmpleadosController`, rutas bajo `/api/v1/empleados`), completamente separado del contrato de los 9 endpoints originales, con sus propias reglas en `Dominio/Reglas/ReglasPermisosEmpleados.cs` y sus propias pruebas. Un Solicitante no tiene ningún acceso a este módulo, ni en la API (los controllers exigen rol Admin o Agente) ni en el frontend (la ruta no aparece en el menú y el guard de rutas la bloquea si se navega por URL directamente).

## 2. Qué hice con ayuda de IA y qué escribí a mano

Usé Claude (Sonnet) para generar la mayor parte del código de este repositorio a partir del enunciado, incluyendo la estructura de proyectos, las entidades, los servicios de aplicación, los controllers, el middleware de errores, las pruebas xUnit, y las vistas de Vue. Yo (actuando como el candidato) dirigí el proceso leyendo cada regla de negocio del enunciado y pidiendo que se implementara explícitamente por número (RN-01 a RN-07), revisé el contrato de la API endpoint por endpoint contra el código generado, y verifiqué manualmente que el frontend compilara (`vue-tsc --noEmit`, `vite build`) en el entorno disponible. No pude hacer la misma verificación de compilación en el backend (ver más abajo, "dónde me atasqué").

## 3. Qué haría distinto con una semana más

- Generaría una migración real de EF Core (`dotnet ef migrations add InitialCreate`) en vez de `EnsureCreated()`.
- Agregaría un `docker-compose.yml` que levante todo con un solo comando, probado de punta a punta.
- Extraería interfaces de repositorio para poder mockear la base de datos en pruebas de los servicios de `Aplicacion` (hoy solo el dominio puro tiene pruebas unitarias; los servicios no tienen pruebas de integración).
- Generaría los tipos de TypeScript desde el `openapi.json` de Swagger en vez de escribirlos a mano, para que un cambio en el contrato del backend rompa el build del frontend automáticamente.
- Añadiría manejo optimista de concurrencia en el correlativo de `Solicitud` (RN-07 lo deja fuera de alcance, pero lo dejaría resuelto si sobrara tiempo).
- Escribiría pruebas end-to-end del frontend contra los `data-testid`, en vez de solo confiar en que existan.

## Nota aparte: el único `any` explícito del frontend

`frontend/src/vite-env.d.ts` declara el módulo `*.vue` usando `DefineComponent<{}, {}, any>`. Es la declaración de tipos estándar que usa la propia plantilla oficial de Vue + TypeScript + Vite para que el compilador entienda los imports `.vue`; no hay forma de tipar ese tercer genérico sin conocer de antemano el tipo de cada componente. `tsc --noEmit` pasa en verde con esta única excepción declarada.

## 4. Dónde me atasqué y cómo lo resolví

El punto más incómodo de esta entrega fue no tener el SDK de .NET ni acceso de red a NuGet en el entorno donde trabajé. Esto significa que **todo el backend fue escrito sin poder ejecutar `dotnet build` ni `dotnet test` ni una sola vez**. Lo resolví de la siguiente manera:

- Escribí el código en piezas pequeñas y muy explícitas (un archivo por responsabilidad), revisando cada using, cada tipo de retorno y cada firma de método a mano, en vez de escribir clases grandes donde un error tipográfico se esconde más fácil.
- Prioricé que la lógica de negocio más importante (máquina de estados, SLA, permisos) viviera en clases estáticas del dominio sin dependencias externas — son las que tienen pruebas unitarias escritas junto al código, y son las más fáciles de razonar sin un compilador.
- El frontend sí lo pude compilar y verificar de verdad (`npm install`, `vue-tsc --noEmit`, `npm run build`, los tres en verde), así que esa parte tiene una garantía real de que funciona, no solo de que "se ve bien".

Si esto se descubre en la entrevista técnica, prefiero decirlo aquí primero: el backend necesita una pasada de `dotnet build` antes de confiar en él al 100%, aunque la lógica de negocio fue diseñada con cuidado y siguiendo el enunciado al detalle.
