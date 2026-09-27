# Arquitectura prevista para Linkubator

Este documento describe cómo se construirá Linkubator: plataforma, capas, persistencia, seguridad técnica y procesos en segundo plano. Las reglas de negocio están en domain-model.md y specifications.md; las razones de cada elección técnica, en decisions.md → Tecnología.

## Plataforma

- .NET 10 y C#.
- ASP.NET Core Razor Pages con renderizado del lado servidor.
- Tailwind CSS.
- SQLite con FTS5, mediante `Microsoft.Data.Sqlite` y Dapper con SQL explícito.
- Argon2id con `Konscious.Security.Cryptography.Argon2` para el hash de las contraseñas.
- MailKit para el envío de correo por SMTP, y smtp4dev como servidor SMTP local de desarrollo.
- Autenticación con el manejador de cookies de ASP.NET Core, sin ASP.NET Core Identity.
- Clean Architecture, SOLID, DRY y YAGNI, con los patrones Repository, Unit of Work y Result.

El MVP0 se ejecuta únicamente en local. La estrategia de despliegue queda fuera del MVP0.

## Capas

### Domain

- Entidades, value objects, invariantes, reglas de negocio, errores de dominio y enumeradores.
- La generación de `User.Alias`, `Collection.Slug` y `Tag.Slug` es una regla compartida del dominio, no lógica duplicada en cada caso de uso.
- No conoce SQLite, Dapper, HTTP, Razor Pages ni detalles de infraestructura.

### Application

- Casos de uso, DTOs, validadores y patrón `Result`.
- Interfaces de repositorios y de Unit of Work.
- Abstracciones del usuario identificado, del scraper, del hashing de contraseñas (`IPasswordHasher`) y del envío de correo (`IEmailSender`).
- La generación de `Link.UrlNormalized` es una regla técnica independiente: produce una clave plana para detectar duplicados, no una URL reconstruible.

### Infrastructure

- Repositorios con Dapper y SQL explícito, conexiones SQLite, transacciones y Unit of Work ligero.
- Scripts SQL versionados, índices y restricciones.
- Índice de búsqueda con SQLite FTS5.
- `IPasswordHasher` con Argon2id. `Konscious.Security.Cryptography.Argon2` solo calcula el hash: la generación de la sal, el formato PHC y la comparación en tiempo constante los hace esta implementación (valores en specifications.md → Hash de la contraseña).
- `IEmailSender` con MailKit (ver «Correo»).
- Scraper HTTP y trabajos persistidos, o un mecanismo equivalente, para el scraping (ver «Scraping»).

### Web

- Razor Pages, Tailwind, panel de gestión y páginas públicas.
- Enrutamiento, autenticación, autorización y seguridad web (ver «Autenticación y seguridad web»).
- Resolución del usuario identificado.
- Presentación de errores y estados accesibles.

### Casos de uso iniciales

Cuenta:

- Registrarse, verificar el email y reenviar la verificación.
- Iniciar y cerrar sesión.
- Editar la configuración, cambiar la contraseña y cambiar el email.
- Solicitar la recuperación de contraseña y restablecerla.
- Eliminar la cuenta.

Gestión:

- Colecciones: Crear, editar, eliminar y hacerlas públicas o privadas.
- Etiquetas: crear, editar y eliminar.
- Enlaces: crear, editar, eliminar, hacerlos públicos o privados y moverlos entre colecciones (Pública a Pública, Pública a Privada y Privada a Pública).
- Asociar y desasociar etiquetas, incluida la creación de una etiqueta desde el formulario del enlace.
- Listar las colecciones y las etiquetas del usuario.
- Buscar enlaces y filtrarlos por colección y/o etiqueta.
- Procesar metadatos (en la etapa de scraping).

Público (anónimo y de solo lectura):

- Consultar la landing, la página de un usuario y la de una colección pública.

El comportamiento de cada caso de uso está en specifications.md y sus invariantes, en domain-model.md.

## Autenticación y seguridad web

- La aplicación se sirve por HTTPS también en local, con el certificado de desarrollo de .NET, y las peticiones HTTP se redirigen a HTTPS.
- La cookie de autenticación usa el prefijo `__Host-`, `Path=/`, `HttpOnly`, `Secure` y `SameSite=Lax`, y solo contiene un ticket cifrado sin datos personales en claro.
- En cada petición autenticada se comprueba el `SecurityStamp` del ticket contra el del usuario; si no coincide, la sesión deja de ser válida. Al cambiar la contraseña, se vuelve a emitir la cookie de la sesión actual con el nuevo sello.
- Las páginas de `/app` se agrupan en la carpeta `Pages/App` y se protegen de una vez con la convención de autorización por carpeta (`AuthorizeFolder("/App")`). Las páginas de cuenta que deben ser anónimas se marcan explícitamente (`AllowAnonymousToPage`).
- Tras el login, `ReturnUrl` solo se acepta si es una URL local de la aplicación; si no, se redirige al panel (`/app`).
- Todos los formularios que modifican datos usan POST con validación antiforgery, que Razor Pages aplica por defecto. Cerrar sesión también es un POST con antiforgery.
- Todas las respuestas llevan las cabeceras `X-Content-Type-Options: nosniff` y `Content-Security-Policy: frame-ancestors 'none'`.

## Correo

- Los correos se envían con MailKit por SMTP. En local los captura smtp4dev, que los muestra en una interfaz web sin entregarlos a nadie.
- Los correos se encolan en memoria después de confirmar la transacción, y los envía un `BackgroundService` fuera de la petición (motivo en decisions.md → Cuenta y seguridad).
- Si la aplicación se detiene con correos en cola, se pierden (riesgo aceptado, ver decisions.md → Riesgos aceptados).

## Enrutamiento

- Todo lo que no es público cuelga de `/app`: registro, verificación de email, login, recuperación y restablecimiento de contraseña (páginas anónimas); panel de gestión; configuración, cambio de contraseña y eliminación de cuenta.
- La raíz queda reservada a las rutas públicas de specifications.md → Páginas públicas y a los recursos estáticos.
- Las rutas literales tienen prioridad sobre las parametrizadas, y `app` no puede ser un alias porque es más corto que la longitud mínima de un alias. Como los alias tampoco tienen puntos, no pueden coincidir con ninguna ruta de la aplicación ni con un recurso estático.
- Las variantes de una URL pública (barra final, mayúsculas) se sirven sin redirecciones; la etiqueta canonical indica la forma canónica.

## Persistencia

El Unit of Work es ligero y gestiona una conexión y una transacción explícita. Las operaciones que afectan a varias tablas se ejecutan de forma atómica.

Ciclo de acceso a datos:

1. Abrir la conexión.
2. Iniciar la transacción solo cuando sea necesaria.
3. Ejecutar las operaciones mínimas de la unidad de trabajo.
4. Confirmar o revertir inmediatamente.
5. Liberar la transacción, la conexión y los recursos asociados.

Reglas obligatorias:

- Las transacciones son lo más cortas posible.
- Dentro de una transacción no se espera al usuario, no se hacen llamadas HTTP, scraping ni envíos de correo, y no se esperan reintentos ni intervalos de tiempo.
- Las lecturas no mantienen transacciones abiertas innecesariamente.
- Las consultas paginadas no cargan en memoria más datos de los necesarios.
- Las restricciones únicas de SQLite son la garantía final frente a duplicados y colisiones.
- Las reglas que no garantizan una restricción de la base de datos se comprueban dentro de la misma transacción de escritura que la operación, iniciada con `BEGIN IMMEDIATE`, para evitar condiciones de carrera. Por ejemplo: no borrar la última colección del usuario, el límite de correos (`User.LastEmailSentAt`) y el contador de intentos fallidos (`User.FailedLoginAttempts`).
- Los borrados de datos relacionados los hace la aplicación de forma explícita, nunca la base de datos de forma automática (sin `ON DELETE CASCADE`).

Operaciones que van en una sola transacción:

- El registro de un usuario y la creación de su «Bandeja de entrada».
- La sustitución de una cuenta sin verificar y el borrado de sus datos.
- La eliminación de una cuenta y de todos sus datos.
- Cualquier escritura o borrado de enlaces, colecciones o etiquetas, junto con sus relaciones `LinkTag` y la proyección FTS5 afectada.

Configuración y convenciones:

- SQLite se configura con WAL, `busy_timeout` y claves foráneas activadas. Los reintentos por bloqueo son limitados y solo se aplican a errores transitorios identificados.
- Los identificadores son `INTEGER PRIMARY KEY` (el `rowid` de SQLite), salvo en `LinkTag`, cuya clave es la pareja `LinkId + TagId`.
- Todas las fechas se guardan en UTC y en formato ISO 8601.
- Las migraciones se resuelven mediante scripts SQL versionados o una herramienta ligera equivalente, sin EF Core.

Restricciones e índices previstos:

- Las restricciones de unicidad de domain-model.md → Restricciones de unicidad.
- Índices por usuario, colección, estado público y fecha.
- Índice de `UserToken` por usuario y propósito.

## FTS5

- El índice de búsqueda es una proyección desnormalizada con la URL original, el título, la descripción, el nombre de la colección y los nombres de las etiquetas, más el `UserId` de cada entrada.
- La consulta combina siempre `MATCH` con el filtro obligatorio del usuario identificado; FTS5 nunca puede devolver resultados de otro usuario.
- FTS5 se reserva para el texto libre. Los filtros por colección, etiqueta, estado público y fecha usan SQL e índices convencionales.
- Tokenizador `unicode61` con `remove_diacritics 2` y sin stemming.
- Los términos de consulta se escapan.
- El orden por relevancia usa `ORDER BY rank` ascendente: en FTS5, `rank` (`bm25()`) es más bajo cuanto mejor es la coincidencia, así que `ORDER BY rank DESC` devolvería primero los menos relevantes.
- La paginación es estable, con los desempates de specifications.md → Búsqueda y filtros.
- La reconstrucción del índice es un comando de mantenimiento que se ejecuta a mano: vacía la proyección y la regenera desde las tablas, en una transacción. En el MVP0 no se lanza de forma automática.

## Scraping

El scraping forma parte del MVP0, pero se implementará en una fase posterior. Su diseño técnico no está cerrado hasta resolver los pendientes de decisions.md → Pendientes. El flujo funcional provisional está en specifications.md → Metadatos y scraping.

- Se ejecuta fuera de la petición principal, mediante un `BackgroundService` y trabajos persistidos o un mecanismo equivalente.
- Medidas obligatorias:
  - Protección SSRF: se validan los destinos antes y durante las redirecciones, y se bloquean los destinos loopback, privados y link-local.
  - Solo protocolos `http` y `https`.
  - Timeout.
  - Límite de bytes descargados.
  - Tipos de contenido permitidos.
  - No sustituir nunca la URL original por la de una redirección.
  - Sanitizar los metadatos extraídos.
  - Validar `og:image` según specifications.md → Ajuste y validación.

## Logs

- Los logs son agnósticos de la plataforma de observabilidad, y no se integra ninguna en el MVP0.
- Qué se registra y qué nunca se registra está en specifications.md → Registro de eventos.
