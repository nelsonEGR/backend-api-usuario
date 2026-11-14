# API Usuario

API REST en ASP.NET Core 8 para la gestion de usuarios, ubicacion y contrasenas.
Esta preparada para ejecutarse en local y desplegarse en **Render** mediante Docker,
sin secretos dentro del repositorio.

---

## 1. Stack

| Componente | Tecnologia |
|---|---|
| Runtime | .NET 8 / ASP.NET Core |
| Base de datos | PostgreSQL (Neon) |
| ORM | EF Core 9 + Npgsql |
| Hash de contrasenas | BCrypt.Net |
| Documentacion | Swagger / OpenAPI |
| Tests | xUnit + EF Core InMemory |
| Contenedor | Docker multi-stage, usuario sin privilegios |

## 2. Estructura

```
backend-api-usuario/
├── src/
│   ├── Configuration/      Opciones tipadas y resolucion de la cadena de conexion
│   ├── Common/             Excepciones de negocio y modelos compartidos
│   ├── Controllers/        Endpoints HTTP (delgados)
│   ├── Data/               DbContext, configuraciones EF y migraciones
│   ├── Dtos/               Contratos de entrada/salida con validaciones
│   ├── Extensions/         Registro de servicios y configuracion del host
│   ├── HealthChecks/       Verificacion de conexion y del esquema de PostgreSQL
│   ├── Logging/            Mensajes de arranque (LoggerMessage)
│   ├── Mappings/           Conversion entidad -> DTO
│   ├── Middleware/         Manejo global de errores y cabeceras de seguridad
│   ├── Models/             Entidades del dominio
│   ├── Security/           Autenticacion por API key y hashing
│   ├── Services/           Reglas de negocio
│   ├── Program.cs
│   └── Dockerfile
├── tests/
│   └── ApiUsuario.Tests/   Pruebas unitarias de servicios y validaciones
├── .env.example
├── .github/workflows/ci.yml
└── Directory.Build.props
```

Flujo de una peticion:

```
HTTP -> ForwardedHeaders -> ExceptionHandler -> SecurityHeaders
     -> CORS -> RateLimiter -> Authentication (API key) -> Authorization
     -> Controller -> Service -> EF Core -> PostgreSQL
```

## 3. Requisitos

- .NET SDK 8.0 o superior.
- PostgreSQL 14+ (local o Neon).
- Docker (opcional, para probar la imagen).

## 4. Configuracion por variables de entorno

**No hay credenciales en `appsettings.json`.** Toda la configuracion sensible se
inyecta por variables de entorno, igual en local y en produccion.

| Variable | Obligatoria | Descripcion |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Si* | Cadena de conexion Npgsql. |
| `DATABASE_URL` | Alternativa | URI `postgres://usuario:password@host/db`; se convierte automaticamente. |
| `Security__ApiKey` (o `API_KEY`) | Si en produccion | API key usada como `clientSecret` del login generico y como `X-API-KEY` en cada peticion. |
| `Security__AdminApiKey` (o `ADMIN_API_KEY`) | Recomendada | API key administrativa (rol `admin`). |
| `Security__RequireApiKey` | No | En Development, `false` desactiva token y API key. En produccion siempre se exige. |
| `Security__UseHsts` | No | Activa HSTS. Por defecto `true` en Production. |
| `Jwt__Key` (o `JWT_KEY`) | Si en produccion | Clave de firma del JWT. Minimo 32 caracteres. |
| `Jwt__Issuer` | No | Emisor del token (default `api-usuario`). |
| `Jwt__Audience` | No | Audiencia del token (default `api-usuario-clients`). |
| `Jwt__ExpiresMinutes` | No | Validez del `access_token` en minutos (5 a 1440, default 5). |
| `Cors__AllowedOrigins__0` | Recomendada | Origen permitido, por ejemplo `https://mi-frontend.com`. |
| `Cors__AllowAnyOrigin` | No | Mantiene compatibilidad con `*`. Recomendado `false` en produccion. |
| `Swagger__Enabled` | No | Por defecto solo `true` en Development. |
| `RateLimiting__PermitLimit` | No | Peticiones por IP y ventana (default 120). |
| `RateLimiting__WindowSeconds` | No | Ventana en segundos (default 60). |
| `RateLimiting__SensitivePermitLimit` | No | Limite para endpoints de contrasenas (default 10). |
| `ASPNETCORE_ENVIRONMENT` | No | `Production` en Render. La imagen Docker ya lo define. |
| `PORT` | Render | Render lo inyecta automaticamente; Kestrel lo usa. |

\* Si no se configura ninguna de las dos, la aplicacion falla al iniciar con un
mensaje claro. Es intencional: evita arrancar sin base de datos.

### Login generico y doble validacion

1. `POST /api/auth/token` con `clientId` (opcional) y `clientSecret` (la API key).
   Devuelve `accessToken`, `tokenType`, `expiresIn` y `expiresAt`.
2. En el resto de rutas protegidas envie **ambos** headers:

   ```
   Authorization: Bearer <accessToken>
   X-API-KEY: <api key o admin key>
   ```

Roles:

- `Security__ApiKey`: rol `client`. Acceso a los endpoints normales.
- `Security__AdminApiKey`: rol `admin`. Necesaria para asignar y resetear contrasenas.
- Si `Security__AdminApiKey` no existe, la key principal conserva permisos de
  administracion para no romper el despliegue anterior. En produccion configure
  ambas y registre la advertencia que aparece en los logs.

El `access_token` expira en 5 minutos por defecto (`Jwt__ExpiresMinutes`). Al
expirar, vuelva a ejecutar el login generico: no hay refresh token todavia.

En **Development** la seguridad esta desactivada por defecto
(`Security:RequireApiKey=false`): no se exige ni `access_token` ni `X-API-KEY`.
Para probar la doble validacion en local, configure
`Security:RequireApiKey=true`.

## 5. Ejecucion local

```powershell
# 1. Restaurar herramientas y dependencias
dotnet tool restore
dotnet restore

# 2. Configurar secretos locales (no se versionan)
cd src
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=api_usuario;Username=postgres;Password=TU_PASSWORD;SslMode=Prefer"
dotnet user-secrets set "Security:ApiKey" "dev-api-key"
dotnet user-secrets set "Security:AdminApiKey" "dev-admin-key"
dotnet user-secrets set "Jwt:Key" "dev-jwt-signing-key-con-mas-de-32-caracteres"

# 3. Ejecutar
dotnet run
```

Swagger: `http://localhost:5291/swagger`

En **Development** la seguridad queda desactivada por defecto: no se exige
`access_token` ni `X-API-KEY`, igual que antes. Para probar la doble validacion
en local configure `Security:RequireApiKey=true` (por user-secrets o `Security__RequireApiKey=true`).
En produccion la doble validacion siempre esta activa.

### Alternativa con variables de entorno

```powershell
$env:ConnectionStrings__DefaultConnection="Host=localhost;Database=api_usuario;Username=postgres;Password=TU_PASSWORD"
$env:Security__ApiKey="dev-api-key"
$env:Security__AdminApiKey="dev-admin-key"
$env:Jwt__Key="dev-jwt-signing-key-con-mas-de-32-caracteres"
dotnet run --project src/api.csproj
```

## 6. Endpoints

Todas las rutas protegidas exigen doble validacion: `Authorization: Bearer` y
`X-API-KEY`. Solo `/`, `/health`, `/api/auth/token` y Swagger son anonimos.

| Metodo | Ruta | Descripcion |
|---|---|---|
| POST | `/api/auth/token` | Login generico: recibe `clientId` (opcional) y `clientSecret`, devuelve el `accessToken`. |
| GET | `/` | Verificacion simple de que el proceso responde. |
| GET | `/health` | Estado del proceso, conexion a PostgreSQL y esquema (tablas/columnas). Usar en Render. |
| GET | `/api/usuarios` | Lista todos los usuarios (comportamiento historico). |
| GET | `/api/usuarios?id=1` | Consulta un usuario por ID (equivale a `/api/usuarios/1`). |
| GET | `/api/usuarios?page=1&pageSize=50` | Igual, pero paginado. Parametros opcionales. |
| GET | `/api/usuarios/{id}` | Consulta un usuario. |
| POST | `/api/usuarios` | Crea un usuario (nace inactivo). |
| PUT | `/api/usuarios/{id}` | Actualiza datos del usuario. |
| DELETE | `/api/usuarios/{id}` | Elimina un usuario inactivo. |
| PATCH | `/api/usuarios/{id}/activar` | Activa la cuenta. |
| PATCH | `/api/usuarios/{id}/desactivar` | Desactiva la cuenta. |
| POST | `/api/UsuarioDetalles/asignar-password` | Asigna contrasena inicial (admin). |
| PATCH | `/api/UsuarioDetalles/cambiar-password` | Cambia contrasena con la actual. |
| PATCH | `/api/UsuarioDetalles/reset-password` | Resetea contrasena (admin). |
| PUT | `/api/UsuarioDetalles/{id}/ubicacion` | Actualiza ciudad/departamento. |
| GET | `/api/departamentos?id=1` | Lista o consulta departamentos. |
| GET | `/api/ciudades?id_ciudad=1&id_departamento=1` | Lista o filtra ciudades. |

La paginacion es opcional: `page` y `pageSize` solo se aplican si se envian en la
URL. Sin ellos, `GET /api/usuarios` devuelve todos los registros como antes.
Cuando se paginan, la respuesta sigue siendo un arreglo y se agrega el header
`X-Total-Count` con el total de registros.

Con `?id=` la respuesta es un **objeto** (o 404 si no existe); sin `?id` es un
**arreglo**, por lo que los clientes actuales que consumen
`GET /api/usuarios` no se ven afectados.

> En Swagger solo se muestra el campo opcional `id`. Los parametros `page` y
> `pageSize` se mantienen ocultos para no saturar el endpoint; para probar la
> paginacion use la URL `?page=1&pageSize=50` en Postman o Bruno.

Los errores usan `application/problem+json` e incluyen la propiedad `mensaje`
por compatibilidad con el frontend anterior.

## 7. Base de datos y migraciones

El proyecto ya incluye la migracion inicial en `src/Data/Migrations`.

**Importante:** la aplicacion **no** aplica migraciones automaticamente al
arrancar. Esto evita sorpresas en produccion.

### Base de datos nueva

```powershell
dotnet tool restore
dotnet ef database update --project src/api.csproj --startup-project src/api.csproj
```

> El comando usa `ConnectionStrings__DefaultConnection` (o `DATABASE_URL`) del
> entorno, o el secreto local configurado con `dotnet user-secrets`.

### Base de datos existente (la de Render)

La migracion inicial crearia tablas que ya existen. Para marcar la base como
"ya migrada" (baseline), ejecute una sola vez:

```sql
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261114123653_InitialCreate', '9.0.4');
```

Luego, si desea aplicar el indice unico de email, revise duplicados primero:

```sql
SELECT lower(email), count(*)
FROM usuario
GROUP BY lower(email)
HAVING count(*) > 1;

CREATE UNIQUE INDEX IF NOT EXISTS ux_usuario_email ON usuario (email);
```

El codigo de la aplicacion funciona con el esquema actual aunque no se cree el
indice; la validacion de duplicados tambien se hace en la capa de servicio.

### Crear nuevas migraciones

```powershell
dotnet tool restore
dotnet ef migrations add NombreDelCambio --project src/api.csproj --startup-project src/api.csproj --output-dir Data/Migrations
```

## 8. Despliegue en Render

La imagen Docker esta pensada para que el **contexto de build sea la raiz del
repositorio**:

- Dockerfile Path: `src/Dockerfile`
- Docker Context Directory: `.` (raiz del repositorio)
- Health Check Path: `/health`

Si el servicio de Render **no** usa Docker, configure:

- Build Command: `dotnet publish src/api.csproj -c Release -o out`
- Start Command: `dotnet out/api.dll`
- Health Check Path: `/health`

### Paso a paso

1. **Rotar la contrasena de Neon.** La contrasena anterior estuvo en
   `appsettings.json` y en el historial de git. Cambiela en el panel de Neon.
2. En Render, abra **Environment -> Environment Variables** y agregue:

   ```
   ConnectionStrings__DefaultConnection=Host=...;Database=neondb;Username=neondb_owner;Password=NUEVA_PASSWORD;SslMode=Require
   Security__ApiKey=GENERAR_UNA_CLAVE_LARGA
   Security__AdminApiKey=GENERAR_OTRA_CLAVE_LARGA
   Jwt__Key=GENERAR_UNA_CLAVE_DE_AL_MENOS_32_CARACTERES
   Jwt__ExpiresMinutes=5
   Cors__AllowedOrigins__0=https://tu-frontend.com
   Cors__AllowAnyOrigin=false
   Swagger__Enabled=false
   ASPNETCORE_ENVIRONMENT=Production
   ```

   Si ya tenia `API_KEY` configurada, puede conservarla: la aplicacion la usa
   como respaldo de `Security__ApiKey`.

3. Guarde y haga **Manual Deploy -> Clear build cache & deploy**.
4. Verifique:
   - `https://TU-SERVICIO.onrender.com/health` debe responder `Healthy`. Si responde
     `Unhealthy`, la conexion falla o la base no tiene las tablas esperadas (por
     ejemplo, una rama vacia de Neon).
   - `https://TU-SERVICIO.onrender.com/` debe responder `"API corriendo"`.
   - Un endpoint protegido sin headers debe responder `401`.
   - `POST /api/auth/token` con el `clientSecret` correcto debe devolver `accessToken`.
   - Con `Authorization: Bearer` y `X-API-KEY` correctos debe responder `200`.

### Problemas comunes al desplegar

**`No se configuro la cadena de conexion`**

Significa que la variable no esta llegando al contenedor. Revise en Render:

1. Que la variable este en **el mismo servicio** que despliega (no en otro servicio
   ni en otro entorno).
2. Que el nombre sea exactamente `ConnectionStrings__DefaultConnection`
   (**dos** guiones bajos, no uno ni dos puntos).
3. Que la variable tenga valor y que el servicio se haya redesplegado despues de
   guardarla (Render lo hace al guardar, pero verifique el commit del nuevo deploy).
4. Que no sea un *Secret File*: debe ser **Environment Variable**.

Tambien se aceptan como respaldo: `DATABASE_URL`, `POSTGRES_URL`,
`POSTGRESQL_URL`, `NEON_DATABASE_URL`, `CONNECTION_STRING` y
`ConnectionStrings_DefaultConnection`.

El mensaje de error incluye los **nombres** (nunca los valores) de las variables
relacionadas que sí existen en el entorno, para detectar errores de tipeo.

**`/health` responde `Unhealthy`**

El health check ahora valida la conexion **y** el esquema: consulta las tablas
`usuario`, `departamentos`, `ciudades` y `usuario_detalle` con `LIMIT 1`. Si la
cadena apunta a una rama vacia de Neon o a otro proyecto, responde `Unhealthy`
en lugar de dejar que los endpoints fallen con `500`.

Revise que `ConnectionStrings__DefaultConnection` apunte al proyecto/rama que
realmente tiene los datos.

**`/health` responde `Healthy` pero un endpoint devuelve `500`**

Puede ser un error de datos o de una columna no contemplada. Revise los logs de
Render: el manejador global registra la excepcion completa (`Npgsql.PostgresException`)
sin exponerla al cliente.

### Notas de compatibilidad

- Las rutas, los payloads y los nombres de campos JSON se mantuvieron.
- Los endpoints de asignar/resetear contrasena ahora exigen rol admin. Si no
  configura `Security__AdminApiKey`, la key principal sigue siendo admin.
- Los codigos de error ahora son correctos (`404` cuando no existe, `409` cuando
  hay conflicto) y el cuerpo incluye `mensaje` para el frontend.
- Swagger queda apagado en produccion por defecto. Active
  `Swagger__Enabled=true` solo si lo necesita.
- Cuando Swagger esta activo, no expone las definiciones de los DTOs: solo
  rutas, metodos y parametros (suficiente para generar las URLs/curl).

## 9. Seguridad

- Sin secretos en el repositorio ni en `appsettings.json`.
- API key obligatoria por configuracion, no por detectar el proveedor de hosting.
- Comparacion de API keys en tiempo constante.
- Endpoints administrativos separados por rol.
- Rate limiting global y especifico para contrasenas.
- CORS configurable por lista blanca.
- Cabeceras de seguridad (`nosniff`, `DENY`, `no-referrer`, etc.).
- Errores internos de PostgreSQL no se exponen al cliente.
- El endpoint de SQL arbitrario fue eliminado.

Lea [`docs/SECURITY.md`](docs/SECURITY.md) para rotar secretos y limpiar el
historial de git.

## 10. Tests y CI

```powershell
dotnet test
```

El workflow `.github/workflows/ci.yml` ejecuta restore, build y tests en cada
push/PR a `main`.

## 11. Escalabilidad: siguientes pasos

Ya quedaron preparadas las bases (capas, DTOs, validacion, errores, health checks,
rate limiting, migraciones y tests). Para crecer:

1. Autenticacion por usuario (JWT/Identity) con roles reales.
2. Versionado de API (`/api/v1`) cuando haya clientes moviles/web estables.
3. Cache distribuida (Redis) para departamentos y ciudades.
4. Soft delete y auditoria de cambios.
5. Logging estructurado (Serilog) + OpenTelemetry.
6. Tests de integracion con Testcontainers y PostgreSQL real.
7. Separar en proyectos (`Api`, `Application`, `Domain`, `Infrastructure`) si el
   dominio sigue creciendo.
8. Read replicas / pooler de Neon cuando aumente la carga de lectura.
