# Seguridad y rotacion de secretos

## 1. Rotar la contrasena de PostgreSQL (obligatorio)

La cadena de conexion anterior estuvo versionada en `src/appsettings.json` y en
el historial de git. Aunque el archivo ya no contiene secretos, **el valor
anterior debe considerarse comprometido**.

Pasos:

1. Entrar a Neon -> proyecto -> **Roles** -> `neondb_owner` -> **Reset password**.
2. Guardar la nueva contrasena en el gestor de secretos.
3. Actualizar en Render la variable `ConnectionStrings__DefaultConnection`.
4. Redesplegar y verificar `/health`.

## 2. Rotar la API key

1. Generar una clave larga y aleatoria:

   ```powershell
   [Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
   ```

2. Configurar en Render:
   - `Security__ApiKey` para clientes.
   - `Security__AdminApiKey` para operaciones administrativas.
3. Actualizar el frontend/cliente con la nueva key.
4. Retirar la variable antigua `API_KEY` cuando ya no se use como respaldo.

## 3. Limpiar el historial de git (opcional pero recomendado)

Si el repositorio es publico o compartido con terceros, borrar el secreto del
historial evita que siga siendo visible. Esto reescribe commits y requiere
coordinacion con todo el equipo.

Con `git-filter-repo`:

```bash
# replacements.txt
OLD_PASSWORD==>***REMOVED***
```

```bash
git filter-repo --replace-text replacements.txt
git push --force --all
git push --force --tags
```

Alternativas: BFG Repo-Cleaner o rotar el secreto y asumir que el historial
queda como evidencia. **La rotacion es obligatoria; la limpieza es opcional.**

## 4. Que NO debe volver al repositorio

- Cadenas de conexion con usuario/password.
- API keys reales.
- `appsettings.Production.json` con secretos.
- Archivos `.env`.
- Certificados `.pfx` o llaves privadas.

`.gitignore` ya excluye `.env`, `.env.*`, `appsettings.Production.json`,
`appsettings.Local.json` y `secrets.json`.

## 5. Endpoint SQL eliminado

Se elimino `ConsultaSqlController`. Ejecutaba cualquier `SELECT` enviado por el
cliente con el usuario de la base, lo que permitia leer hashes de contrasenas y
otras tablas, ademas de agotar recursos con consultas costosas.

Si en el futuro se necesita una herramienta administrativa:

- Usar un rol de PostgreSQL de **solo lectura**.
- Forzar `LIMIT` y `statement_timeout`.
- Exigir rol admin y registrar auditoria.
- Nunca exponerla con la misma API key de los clientes.
