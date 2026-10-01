# Arquitectura prevista para Linkubator

Este documento describe cómo se construirá Linkubator: plataforma, capas, persistencia, seguridad técnica y procesos en segundo plano. Las reglas de negocio están en domain-model.md y specifications.md; las razones de cada elección técnica, en decisions.md → «Tecnología».

## Plataforma

- .NET 10 y C#.
- ASP.NET Core Razor Pages con renderizado del lado servidor.
- Tailwind CSS.
- SQLite con FTS5, mediante `Microsoft.Data.Sqlite` y Dapper con SQL explícito.
- Argon2id con `Konscious.Security.Cryptography.Argon2` para el hash de las contraseñas.
- MailKit para el envío de correo por SMTP, y smtp4dev como servidor SMTP local de desarrollo.
- Serilog como proveedor de logs estructurados, detrás de `ILogger`.
- Autenticación con el manejador de cookies de ASP.NET Core, sin ASP.NET Core Identity.
- Clean Architecture, SOLID, DRY y YAGNI, con los patrones Repository, Unit of Work y Result.

> Diagrama de contenedores: [Aplicación y dependencias](diagrams/container-diagram.md).
> Diagrama de contexto: [Contexto](diagrams/context-diagram.md).

El MVP0 se ejecuta únicamente en local. La estrategia de despliegue queda fuera del MVP0.

## Capas

> Diagrama de capas: [Capas y dependencias](diagrams/architecture-layers.md).

### Domain

- Entidades, value objects, invariantes, reglas de negocio, errores de dominio y enumeradores.
- La generación de `User.Alias`, `Collection.Slug` y `Tag.Slug` es una regla compartida del dominio (ver specifications.md → «Generación de alias y slugs»), y la de `Link.UrlNormalized` también vive aquí (ver specifications.md → «Normalización para duplicados»): ambas son transformaciones de texto sin dependencias de infraestructura.
- No conoce SQLite, Dapper, HTTP, Razor Pages ni detalles de infraestructura.

### Application

- Casos de uso, DTOs, validadores y patrón `Result`.
- Interfaces de repositorios y de Unit of Work.
- Abstracciones del usuario identificado, del scraper, del hashing de contraseñas (`IPasswordHasher`) y del envío de correo (`IEmailSender`).

### Infrastructure

- Repositorios con Dapper y SQL explícito, conexiones SQLite, transacciones y Unit of Work ligero.
- Scripts SQL versionados, índices y restricciones.
- Índice de búsqueda con SQLite FTS5.
- `IPasswordHasher` con Argon2id. `Konscious.Security.Cryptography.Argon2` solo calcula el hash: la generación de la sal, el formato PHC y la comparación en tiempo constante los hace esta implementación (valores en specifications.md → «Hash de la contraseña»).
- `IPasswordHasher` limita el número de hashes que se calculan a la vez; los demás esperan en cola. Cada hash consume mucha memoria (ver specifications.md → «Hash de la contraseña»), y sin límite muchas peticiones simultáneas agotarían la del proceso. El valor del límite se fija al implementar. Un límite por IP queda fuera del MVP0.
- `IEmailSender` con MailKit (ver «Correo»).
- Scraper HTTP y trabajos persistidos, o un mecanismo equivalente, para el scraping (ver «Scraping»).

### Web

- Razor Pages, Tailwind, panel de gestión y páginas públicas.
- Enrutamiento, autenticación, autorización y seguridad web (ver «Autenticación» y «Seguridad web»).
- Resolución del usuario identificado.
- Presentación de errores y estados accesibles.

### Casos de uso iniciales

Cuenta:

- Registrarse con el email y completar el registro desde el enlace del correo.
- Iniciar y cerrar sesión.
- Editar la configuración, cambiar la contraseña y cambiar el email.
- Solicitar la recuperación de contraseña y restablecerla.
- Eliminar la cuenta.

Gestión:

- Colecciones: Crear, editar, eliminar y hacerlas públicas o privadas.
- Etiquetas: crear, editar y eliminar.
- Enlaces: crear, editar, eliminar, hacerlos públicos o privados y moverlos entre colecciones.
- Asociar y desasociar etiquetas, incluida la creación de una etiqueta desde el formulario del enlace.
- Listar las colecciones y las etiquetas del usuario.
- Buscar enlaces y filtrarlos por colección y/o etiqueta.
- Procesar metadatos (en la etapa de scraping).

Público (anónimo y de solo lectura):

- Consultar la landing, la página de un usuario, la página de colecciones y la de una colección pública.

El comportamiento de cada caso de uso está en specifications.md y sus invariantes, en domain-model.md.

> Diagrama de casos de uso del sistema: [Casos de uso de Linkubator](diagrams/use-case-diagram.md).

## Autenticación

- La cookie de autenticación usa el prefijo `__Host-`, `Path=/`, `HttpOnly`, `Secure` y `SameSite=Lax`, y solo contiene un ticket cifrado sin datos personales en claro.
- En cada petición autenticada se comprueba el `SecurityStamp` del ticket contra el del usuario; si no coincide, la sesión deja de ser válida. Al cambiar la contraseña y al confirmar un cambio de email con sesión abierta, se vuelve a emitir la cookie de la sesión actual con el nuevo sello, copiando del ticket anterior su fecha de caducidad y su persistencia. En la confirmación del cambio de email solo se reemite si la sesión pertenece al dueño del token; si no, no se crea ni se modifica ninguna sesión.
- El ticket de autenticación lleva una caducidad absoluta calculada en el login y el deslizamiento de la caducidad (`SlidingExpiration`) está desactivado. La persistencia de la cookie (`Expires`) se decide en cada login según «Recordarme»; el ticket caduca igual en ambos casos (valores en specifications.md → «Sesión»).
- Las páginas de `/app` se agrupan en la carpeta `Pages/App` y se protegen de una vez con la convención de autorización por carpeta (`AuthorizeFolder("/App")`). Las páginas de cuenta que deben ser anónimas se marcan explícitamente (`AllowAnonymousToPage`).

## Seguridad web

- La aplicación se sirve por HTTPS también en local, con el certificado de desarrollo de .NET, y las peticiones HTTP se redirigen a HTTPS.
- Tras el login, `ReturnUrl` solo se acepta si es una URL local de la aplicación; si no, se redirige al panel (`/app`).
- Todos los formularios que modifican datos usan POST con validación antiforgery, que Razor Pages aplica por defecto. Cerrar sesión también es un POST con antiforgery. Ningún GET modifica datos, tampoco el de los enlaces de los correos: abrirlos solo muestra una página.
- Todas las respuestas llevan las cabeceras `X-Content-Type-Options: nosniff`, `Content-Security-Policy: frame-ancestors 'none'` y `Referrer-Policy: strict-origin-when-cross-origin`.
- Solo se aceptan peticiones cuyo `Host` sea el de la aplicación (`AllowedHosts`); las demás se rechazan.

## Correo

- Los correos se envían con MailKit por SMTP. En local los captura smtp4dev, que los muestra en una interfaz web sin entregarlos a nadie.
- Los correos se encolan en memoria después de confirmar la transacción, y los envía un `BackgroundService` fuera de la petición (motivo en decisions.md → «Seguridad»).
- Si la aplicación se detiene con correos en cola, se pierden (riesgo aceptado, ver decisions.md → «Riesgos aceptados»).
- Los enlaces de los correos y las URL canonical se construyen con el origen público configurado de la aplicación, nunca con la cabecera `Host` de la petición (motivo en decisions.md → «Seguridad»).

## Enrutamiento

- Todo lo que no es público cuelga de `/app`: registro, completar registro, login, recuperación y restablecimiento de contraseña (páginas anónimas); panel de gestión; configuración, cambio de contraseña y eliminación de cuenta.
- La raíz queda reservada a las rutas públicas de specifications.md → «Páginas públicas» y a los recursos estáticos.
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
- Las restricciones únicas de SQLite son la garantía final frente a duplicados y colisiones, y la clave foránea compuesta entre `Link` y `Collection` es la garantía final de que un enlace nunca está en una colección de otro usuario.
- Las reglas que no garantiza una restricción de la base de datos se comprueban dentro de la misma transacción de escritura que la operación, iniciada con `BEGIN IMMEDIATE`, para evitar condiciones de carrera entre peticiones concurrentes (dos pestañas o dos navegadores del mismo usuario). Como SQLite solo admite un escritor a la vez, la segunda transacción no empieza hasta que la primera termina y lee siempre el estado ya confirmado. Se comprueban así:
  - No borrar la última colección del usuario.
  - La validez de un token (`UserToken`) y su consumo, al completar el registro, restablecer la contraseña o confirmar un cambio de email: el token se vuelve a validar dentro de la transacción, y si ya no es válido se revierte sin aplicar nada. La comprobación previa a la transacción solo evita trabajo inútil (por ejemplo, calcular un hash).
  - Los límites de correo (`User.LastEmailSentAt` y `User.LastExistingAccountNoticeAt`) y el contador de intentos fallidos (`User.FailedLoginAttempts`).
  - La propiedad de las relaciones (domain-model.md → «Propiedad de los datos»): al crear o mover un enlace, que la colección sea del usuario; al asociar etiquetas, que cada etiqueta sea del usuario.
  - Las invariantes de domain-model.md → «Público y privado»: al hacer público un enlace, que su colección sea pública en ese momento; al hacer privada una colección, sus enlaces pasan a privados en la misma transacción.
- Los borrados de datos relacionados los hace la aplicación de forma explícita, nunca la base de datos de forma automática (sin `ON DELETE CASCADE`). Tampoco se usan triggers: la lógica vive en la aplicación.

Operaciones que van en una sola transacción:

- Completar el registro: fijar nombre, alias, contraseña y `EmailConfirmedAt`, crear la «Bandeja de entrada» y consumir el token.
- El restablecimiento de contraseña y la confirmación del cambio de email, con sus efectos sobre los tokens, el bloqueo y el `SecurityStamp`.
- La eliminación de una cuenta y de todos sus datos.
- Cualquier escritura o borrado de enlaces, colecciones o etiquetas, junto con sus relaciones `LinkTag` y la proyección FTS5 afectada.

Configuración y convenciones:

- SQLite se configura con WAL, `busy_timeout` y claves foráneas activadas. Los reintentos por bloqueo son limitados y solo se aplican a errores transitorios identificados.
- Los identificadores son `INTEGER PRIMARY KEY` (el `rowid` de SQLite), salvo en `LinkTag`, cuya clave es la pareja `LinkId + TagId`.
- `User.Name`, `User.Alias` y `User.Password` admiten `NULL` mientras el registro está sin completar. El índice único de `Alias` admite varios `NULL`, como hace SQLite por defecto.
- Todas las fechas se guardan en UTC y en formato ISO 8601.
- Las migraciones se resuelven mediante scripts SQL versionados o una herramienta ligera equivalente, sin EF Core.

Restricciones e índices previstos:

- Las restricciones de unicidad de domain-model.md → «Restricciones de unicidad».
- Clave foránea compuesta `Link (UserId, CollectionId)` → `Collection (UserId, Id)`, apoyada en un índice único `Collection (UserId, Id)`.
- Índices por usuario, colección, estado público y fecha.
- Índice de `UserToken` por usuario y propósito.

## FTS5

- El índice de búsqueda es una proyección desnormalizada con la URL original, el título, la descripción, el nombre de la colección y los nombres de las etiquetas, más el `UserId` de cada entrada.
- La consulta combina siempre `MATCH` con el filtro obligatorio del usuario identificado; FTS5 nunca puede devolver resultados de otro usuario.
- FTS5 se reserva para el texto libre. Los filtros por colección, etiqueta, estado público y fecha usan SQL e índices convencionales.
- Tokenizador `unicode61` con `remove_diacritics 2` iguala «canción» y «cancion», y trata la `ñ` como `n`, igual que los slugs. FTS5 no incluye stemming para castellano.
- Los términos de consulta se escapan.
- El orden por relevancia usa `ORDER BY rank` ascendente: en FTS5, `rank` (`bm25()`) es más bajo cuanto mejor es la coincidencia, así que `ORDER BY rank DESC` devolvería primero los menos relevantes.
- La paginación es estable, con los desempates de specifications.md → «Listados».
- La reconstrucción del índice es un comando de mantenimiento que se ejecuta a mano: vacía la proyección y la regenera desde las tablas, en una transacción. En el MVP0 no se lanza de forma automática.

## Scraping

El scraping forma parte del MVP0, pero se implementará en una fase posterior. Su diseño técnico no está cerrado hasta resolver los pendientes de decisions.md → «Pendientes de scraping». El flujo funcional provisional está en specifications.md → «Metadatos y scraping».

- Se ejecuta fuera de la petición principal, mediante un `BackgroundService` y trabajos persistidos o un mecanismo equivalente.
- Medidas obligatorias:
  - Protección SSRF: se validan los destinos antes y durante las redirecciones, y se bloquean los destinos loopback, privados y link-local.
  - Solo protocolos `http` y `https`.
  - Timeout.
  - Límite de bytes descargados.
  - Tipos de contenido permitidos.
  - No sustituir nunca la URL original por la de una redirección.
  - Sanitizar los metadatos extraídos.
  - Validar `og:image` según specifications.md → «Reglas adicionales para `Image`».

## Logs

- Serilog implementa el registro estructurado detrás de `ILogger`; Domain no conoce el registro y las demás capas dependen de la abstracción.
- En el MVP0 local se escriben logs por consola y en un archivo rotativo. El nivel mínimo y las salidas se pueden configurar sin cambiar los casos de uso.
- Los logs siguen siendo agnósticos de una plataforma de observabilidad; no se integra ninguna en el MVP0.
- Qué se registra y qué nunca se registra está en specifications.md → «Registro de eventos».
- El registro de peticiones del framework también cumple esa regla: no incluye la cadena de consulta de las URL que llevan un token.
