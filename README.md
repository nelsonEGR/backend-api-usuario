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
│   ├── HealthChecks/       Verificacion de disponibilidad de PostgreSQL
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
| `Security__ApiKey` (o `API_KEY`) | Si en produccion | API key que envian los clientes en `X-API-KEY`. |
| `Security__AdminApiKey` (o `ADMIN_API_KEY`) | Recomendada | API key para endpoints administrativos (`X-API-KEY`). |
| `Security__RequireApiKey` | No | `true`/`false`. Por defecto `true` en todos los entornos. |
| `Security__UseHsts` | No | Activa HSTS. Por defecto `true` en Production. |
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

### Roles de API key

- `Security__ApiKey`: rol `client`. Acceso a los endpoints normales.
- `Security__AdminApiKey`: rol `admin`. Necesaria para asignar y resetear contrasenas.
- Si `Security__AdminApiKey` no existe, la key principal conserva permisos de
  administracion para no romper el despliegue anterior. En produccion configure
  ambas y registre la advertencia que aparece en los logs.

Los headers son:

```
X-API-KEY: <api key o admin key>
```

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

# 3. Ejecutar
dotnet run
```

Swagger: `http://localhost:5291/swagger`

En todos los entornos la API key es obligatoria por defecto. En local configure
`Security:ApiKey` con user-secrets. Si necesita desactivarla temporalmente para
probar, ejecute con `Security__RequireApiKey=false` (nunca en produccion).

### Alternativa con variables de entorno

```powershell
$env:ConnectionStrings__DefaultConnection="Host=localhost;Database=api_usuario;Username=postgres;Password=TU_PASSWORD"
$env:Security__ApiKey="dev-api-key"
$env:Security__AdminApiKey="dev-admin-key"
dotnet run --project src/api.csproj
```

## 6. Endpoints

Todas las rutas requieren `X-API-KEY` salvo `/`, `/health` y Swagger.

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/` | Verificacion simple de que el proceso responde. |
| GET | `/health` | Estado del proceso y de PostgreSQL. Usar en Render. |
| GET | `/api/usuarios?page=1&pageSize=50` | Lista usuarios. Sin paginacion devuelve todos. |
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

La paginacion agrega el header `X-Total-Count` y mantiene la respuesta como
arreglo para no romper clientes existentes.

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
   Cors__AllowedOrigins__0=https://tu-frontend.com
   Cors__AllowAnyOrigin=false
   Swagger__Enabled=false
   ASPNETCORE_ENVIRONMENT=Production
   ```

   Si ya tenia `API_KEY` configurada, puede conservarla: la aplicacion la usa
   como respaldo de `Security__ApiKey`.

3. Guarde y haga **Manual Deploy -> Clear build cache & deploy**.
4. Verifique:
   - `https://TU-SERVICIO.onrender.com/health` debe responder `Healthy`.
   - `https://TU-SERVICIO.onrender.com/` debe responder `"API corriendo"`.
   - Un endpoint protegido sin header debe responder `401`.
   - Con `X-API-KEY` correcta debe responder `200`.

### Notas de compatibilidad

- Las rutas, los payloads y los nombres de campos JSON se mantuvieron.
- Los endpoints de asignar/resetear contrasena ahora exigen rol admin. Si no
  configura `Security__AdminApiKey`, la key principal sigue siendo admin.
- Los codigos de error ahora son correctos (`404` cuando no existe, `409` cuando
  hay conflicto) y el cuerpo incluye `mensaje` para el frontend.
- Swagger queda apagado en produccion por defecto. Active
  `Swagger__Enabled=true` solo si lo necesita.

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
