# Arquitectura prevista para Linkubator

Este documento describe cómo se construirá Linkubator: plataforma, capas, persistencia, seguridad técnica y procesos en segundo plano. Las reglas de negocio están en [domain-model.md](domain-model.md) y [specifications.md](specifications.md); las razones de cada elección técnica, en [decisions.md → «Tecnología»](decisions.md#tecnología).

## Plataforma

- .NET 10 y C#.
- ASP.NET Core Razor Pages con renderizado del lado servidor.
- Tailwind CSS.
- SQLite con FTS5, mediante `Microsoft.Data.Sqlite` y Dapper con SQL explícito.
- Argon2id con `Konscious.Security.Cryptography.Argon2` para el hash de las contraseñas (MVP1).
- MailKit para el envío de correo por SMTP, y smtp4dev como servidor SMTP local de desarrollo (MVP1).
- Serilog como proveedor de logs estructurados, detrás de `ILogger`.
- Autenticación con el manejador de cookies de ASP.NET Core, sin ASP.NET Core Identity (MVP1).
- Clean Architecture, SOLID, DRY y YAGNI, con los patrones Repository, Unit of Work y Result.

> Diagrama de contenedores: [Aplicación y dependencias](diagrams/container-diagram.md).
> Diagrama de contexto: [Contexto](diagrams/context-diagram.md).

El MVP0 se ejecuta únicamente en local. La estrategia de despliegue queda fuera del MVP0.

## Capas

> Diagrama de capas: [Capas y dependencias](diagrams/architecture-layers.md).

### Domain

- Entidades, value objects, invariantes, reglas de negocio, errores de dominio y enumeradores.
- La generación de `User.Alias` (ver [specifications.md → «Generación del alias»](specifications.md#generación-del-alias)) y la de `Collection.Slug` y `Tag.Slug` (este último desde MVP1; ver [«Generación de slugs»](specifications.md#generación-de-slugs)) son reglas distintas del dominio, y la de `Link.UrlNormalized` también vive aquí (ver [specifications.md → «Normalización para duplicados»](specifications.md#normalización-para-duplicados)): todas son transformaciones de texto sin dependencias de infraestructura.
- No conoce SQLite, Dapper, HTTP, Razor Pages ni detalles de infraestructura.

### Application

- Casos de uso, DTOs, validadores y patrón `Result`.
- Interfaces de repositorios y de Unit of Work.
- Abstracciones del usuario identificado. Las abstracciones del scraper, los puertos de hashing de contraseñas (`IPasswordHasher`) y de envío de correo (`IEmailSender`) se incorporan con la autenticación del MVP1.

### Infrastructure

- Repositorios con Dapper y SQL explícito, conexiones SQLite, transacciones y Unit of Work ligero.
- Scripts SQL versionados, índices y restricciones.
- Índice de búsqueda con SQLite FTS5, con una proyección propia por entidad buscable: en el MVP0, enlaces y colecciones; en el MVP1 incorpora también la de etiquetas y los nombres de las etiquetas en la de enlaces.
- Implementación de `IPasswordHasher` con Argon2id y de `IEmailSender` con MailKit en el MVP1. `Konscious.Security.Cryptography.Argon2` solo calcula el hash: la generación de la sal, el formato PHC y la comparación en tiempo constante los hace esta implementación (valores en [specifications.md → «Hash de la contraseña»](specifications.md#hash-de-la-contraseña)).
- `IPasswordHasher` limitará en MVP1 el número de hashes que se calculan a la vez; los demás esperan en cola. Cada hash consume mucha memoria, y sin límite muchas peticiones simultáneas agotarían la del proceso. El valor del límite se fija al implementar. Un límite por IP queda fuera del alcance definido.
- Scraper HTTP y trabajos persistidos, o un mecanismo equivalente, para el scraping (ver «Scraping»).

### Web

- Razor Pages, Tailwind, panel de gestión y páginas públicas.
- Enrutamiento y seguridad web; la autenticación por cookies y la autorización de cuenta se incorporan en el MVP1.
- Resolución del usuario identificado: en el MVP0, un proveedor de desarrollo inyecta desde código la identidad de un usuario válido; en el MVP1, se obtiene de la sesión autenticada. El detalle está en [«Identidad de desarrollo»](#identidad-de-desarrollo). Los casos de uso no reciben un identificador de usuario del cliente.
- Presentación de errores y estados accesibles.

### Casos de uso iniciales

Cuenta (MVP1):

- Registrarse con el email y completar el registro desde el enlace del correo.
- Iniciar y cerrar sesión.
- Editar la configuración, cambiar la contraseña y cambiar el email.
- Solicitar la recuperación de contraseña y restablecerla.
- Eliminar la cuenta.

Gestión privada (MVP0 con identidad inyectada):

- Colecciones: Crear, editar, eliminar y hacerlas públicas o privadas.
- Enlaces: crear, editar, eliminar, hacerlos públicos o privados y moverlos entre colecciones.
- Listar las colecciones del usuario y buscarlas por nombre.
- Buscar enlaces y filtrarlos por colección.
- Procesar metadatos de forma automática (**en MVP1**).

Gestión de etiquetas (MVP1):

- Crear, editar y eliminar etiquetas.
- Asociar y desasociar etiquetas, incluida la creación de una etiqueta desde el formulario del enlace.
- Listar etiquetas y buscarlas por nombre, buscar enlaces por los nombres de sus etiquetas y filtrarlos por etiqueta.

Público (anónimo y de solo lectura):

- Consultar la landing, la página de un usuario, la página de colecciones y la de una colección pública.

El comportamiento de cada caso de uso está en [specifications.md](specifications.md) y sus invariantes, en [domain-model.md](domain-model.md).

> Diagrama de casos de uso del sistema: [Casos de uso de Linkubator](diagrams/use-case-diagram.md).

## Autenticación

La autenticación real se implementa en el MVP1. En el MVP0, la zona privada usa un proveedor de identidad de desarrollo que inyecta desde código la identidad de un usuario válido; no hay login, cookies de autenticación ni sesiones reales (ver [«Identidad de desarrollo»](#identidad-de-desarrollo)).

- La cookie de autenticación usa el prefijo `__Host-`, `Path=/`, `HttpOnly`, `Secure` y `SameSite=Lax`, y solo contiene un ticket cifrado sin datos personales en claro.
- En cada petición autenticada se comprueba el `SecurityStamp` del ticket contra el del usuario; si no coincide, la sesión deja de ser válida. Al cambiar la contraseña y al confirmar un cambio de email con sesión abierta, se vuelve a emitir la cookie de la sesión actual con el nuevo sello, copiando del ticket anterior su fecha de caducidad y su persistencia. En la confirmación del cambio de email solo se reemite si la sesión pertenece al dueño del token; si no, no se crea ni se modifica ninguna sesión.
- El ticket de autenticación lleva una caducidad absoluta calculada en el login y el deslizamiento de la caducidad (`SlidingExpiration`) está desactivado. La persistencia de la cookie (`Expires`) se decide en cada login según «Recordarme»; el ticket caduca igual en ambos casos (valores en [specifications.md → «Sesión»](specifications.md#sesión)).
- Las páginas de `/app` se agrupan en la carpeta `Pages/App` y se protegen de una vez con la convención de autorización por carpeta (`AuthorizeFolder("/App")`). Las páginas de cuenta que deben ser anónimas se marcan explícitamente (`AllowAnonymousToPage`).

### Identidad de desarrollo

Solo en el MVP0. El motivo de cada elección está en [decisions.md → «Producto y alcance»](decisions.md#producto-y-alcance).

- Un archivo SQL local especial provisiona un usuario con el registro completado según [domain-model.md → «User»](domain-model.md#user), con un email de un dominio reservado para pruebas, y su colección privada «Bandeja de entrada».
- Su `Password` es el hash de una contraseña aleatoria que se descarta tras calcularlo, con el formato y los parámetros de [specifications.md → «Hash de la contraseña»](specifications.md#hash-de-la-contraseña). Nadie conoce esa contraseña.
- El archivo está separado de los scripts SQL versionados, se ejecuta después de ellos y se puede repetir sin duplicar datos.
- El email del usuario de desarrollo se indica en la configuración local de la aplicación. No se usa su `Id` ni su `Alias`.
- El proveedor de identidad de desarrollo solo se registra en el entorno de desarrollo. Al arrancar busca el usuario por ese email; si no existe o su registro no está completado, la aplicación no arranca.
- En cada petición a `/app`, el proveedor entrega el `UserId` de ese usuario como usuario identificado. Ningún dato de la petición puede cambiarlo.
- En el MVP1 el proveedor se sustituye por la sesión autenticada. El usuario de desarrollo puede fijar una contraseña mediante la recuperación de contraseña.

> Diagrama de la secuencia: [Identidad de desarrollo](diagrams/dev-identity-sequence.md).

## Seguridad web

- La aplicación se sirve por HTTPS también en local, con el certificado de desarrollo de .NET, y las peticiones HTTP se redirigen a HTTPS.
- La validación de `ReturnUrl` y su redirección de respaldo están en [specifications.md → «Inicio de sesión»](specifications.md#inicio-de-sesión).
- Todos los formularios que modifican datos usan POST con validación antiforgery, que Razor Pages aplica por defecto. En el MVP1, cerrar sesión también es un POST con antiforgery. Ningún GET modifica datos, tampoco el de los enlaces de los correos: abrirlos solo muestra una página.
- Todas las respuestas llevan las cabeceras `X-Content-Type-Options: nosniff`, `Content-Security-Policy: frame-ancestors 'none'` y `Referrer-Policy: strict-origin-when-cross-origin`.
- Solo se aceptan peticiones cuyo `Host` sea el de la aplicación (`AllowedHosts`); las demás se rechazan.

## Correo

El envío de correo se incorpora en el MVP1; en el MVP0 no se implementan los flujos de cuenta que lo requieren.

- Los correos se envían con MailKit por SMTP. En local los captura smtp4dev, que los muestra en una interfaz web sin entregarlos a nadie.
- Los correos se encolan en memoria después de confirmar la transacción, y los envía un `BackgroundService` fuera de la petición (motivo en [decisions.md → «Seguridad»](decisions.md#seguridad)).
- Si la aplicación se detiene con correos en cola, se pierden (riesgo aceptado, ver [decisions.md → «Riesgos aceptados»](decisions.md#riesgos-aceptados)).
- Los enlaces de los correos se construyen con el origen público configurado de la aplicación, nunca con la cabecera `Host` de la petición (motivo en [decisions.md → «Seguridad»](decisions.md#seguridad)).

## Enrutamiento

- Todo lo que no es público cuelga de `/app`: en el MVP0, el dashboard y la gestión; en el MVP1, también registro, completar registro, login, recuperación y restablecimiento de contraseña (páginas anónimas), configuración, cambio de contraseña y eliminación de cuenta.
- `/app` redirige al dashboard. Las rutas privadas del MVP0 y sus parámetros están en [specifications.md → «Páginas privadas»](specifications.md#páginas-privadas).
- Cada formulario privado del MVP0 tiene una página Razor canónica. Los diálogos, si se incorporan, son una presentación opcional de esos mismos formularios y no una ruta ni un caso de uso distinto (ver [specifications.md → «Páginas privadas»](specifications.md#páginas-privadas)).
- La raíz queda reservada a las rutas públicas de [specifications.md → «Páginas públicas»](specifications.md#páginas-públicas) y a los recursos estáticos.
- Las URL canonical usan el origen público configurado de la aplicación también en el MVP0, nunca la cabecera `Host` de la petición (motivo en [decisions.md → «Seguridad»](decisions.md#seguridad)). En local, se configura el origen correspondiente al entorno de desarrollo.
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
  - La propiedad de las relaciones ([domain-model.md → «Propiedad de los datos»](domain-model.md#propiedad-de-los-datos)): en el MVP0, al crear o mover un enlace, que la colección sea del usuario; en el MVP1, al asociar etiquetas, que cada etiqueta sea del usuario.
  - Las invariantes de [domain-model.md → «Público y privado»](domain-model.md#público-y-privado): al hacer público un enlace, que su colección sea pública en ese momento; al hacer privada una colección, sus enlaces pasan a privados en la misma transacción.
- Los borrados de datos relacionados los hace la aplicación de forma explícita, nunca la base de datos de forma automática (sin `ON DELETE CASCADE`). Tampoco se usan triggers: la lógica vive en la aplicación.

Operaciones que van en una sola transacción:

- Completar el registro: fijar nombre, alias, contraseña y `EmailConfirmedAt`, crear la «Bandeja de entrada» y consumir el token.
- El restablecimiento de contraseña y la confirmación del cambio de email, con sus efectos sobre los tokens, el bloqueo y el `SecurityStamp`.
- La eliminación de una cuenta y de todos sus datos.
- En el MVP0, cualquier escritura o borrado de enlaces y colecciones, junto con las proyecciones FTS5 afectadas. En el MVP1, las operaciones de etiquetas incluyen sus relaciones `LinkTag` y actualizan las proyecciones FTS5 afectadas, y guardar el resultado de un intento de scraping (metadatos, estado e intentos) actualiza también la proyección de enlaces en la misma transacción; la descarga se hace antes, fuera de ella.

Configuración y convenciones:

- SQLite se configura con WAL, `busy_timeout` y claves foráneas activadas. Los reintentos por bloqueo son limitados y solo se aplican a errores transitorios identificados.
- Los identificadores son `INTEGER PRIMARY KEY` (el `rowid` de SQLite), salvo en `LinkTag`, cuya clave es la pareja `LinkId + TagId`.
- `User.Name`, `User.Alias` y `User.Password` admiten `NULL` mientras el registro está sin completar. El índice único de `Alias` admite varios `NULL`, como hace SQLite por defecto.
- Todas las fechas se guardan en UTC y en formato ISO 8601.
- El orden alfabético de [specifications.md → «Listados»](specifications.md#listados) usa una colación propia registrada con `SqliteConnection.CreateCollation` en cada conexión que abre la aplicación. Compara con la cultura `es-ES` sin distinguir mayúsculas ni marcas diacríticas (`CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace`).
  - Se aplica con `COLLATE` en el `ORDER BY` de las consultas alfabéticas, no en la definición de las columnas ni en los índices.
  - La aplicación no activa `InvariantGlobalization`, porque la colación necesita los datos de cultura del runtime.
  - No se usa para la unicidad ni para la búsqueda: esas reglas tienen sus propios mecanismos (slugs, `UrlNormalized` y FTS5).
- Las migraciones se resuelven mediante scripts SQL versionados o una herramienta ligera equivalente, sin EF Core.

Restricciones e índices previstos:

- Las restricciones de unicidad de [domain-model.md → «Restricciones de unicidad»](domain-model.md#restricciones-de-unicidad).
- Clave foránea compuesta `Link (UserId, CollectionId)` → `Collection (UserId, Id)`, apoyada en un índice único `Collection (UserId, Id)`.
- Índices por usuario, colección, estado público y fecha.
- Índice de `UserToken` por usuario y propósito.

## FTS5

- El índice de búsqueda son proyecciones FTS5 desnormalizadas, una por entidad buscable: enlaces, colecciones y, desde el MVP1, etiquetas. No hay una tabla única con tipos de entrada, de modo que los identificadores de entidades distintas no colisionan y la relevancia `bm25` nunca se mezcla entre entidades.
- En cada proyección el `rowid` es el `Id` de su entidad. Además se guarda el `UserId`, que sirve para filtrar y no forma parte del texto buscable.
  - Proyección de enlaces: la URL original, el título, la descripción y el nombre de la colección; desde el MVP1 incluye también los nombres de sus etiquetas.
  - Proyección de colecciones: su `Name`.
  - Proyección de etiquetas (MVP1): su `Name`.
  - Renombrar una colección, o una etiqueta en el MVP1, actualiza su propia proyección y las filas de la proyección de enlaces que la contienen, en la misma transacción.
- Cada consulta de texto usa solo la proyección de la entidad del listado y combina siempre `MATCH` con el `UserId` del usuario identificado; la relación con la tabla de la entidad también comprueba que esta pertenece a ese usuario. Así no se puede devolver un resultado de otro usuario ni de otra entidad.
- FTS5 se reserva para el texto libre. Los filtros del listado de enlaces (colección y, desde el MVP1, etiqueta) usan SQL e índices convencionales.
- Tokenizador `unicode61` con `remove_diacritics 2` iguala «canción» y «cancion», y trata la `ñ` como `n`, igual que los slugs. FTS5 no incluye stemming para castellano.
- La consulta se construye con los términos que define [specifications.md → «Búsqueda y filtros»](specifications.md#búsqueda-y-filtros). Cada término se escribe entre comillas dobles, para que FTS5 no interprete como operador palabras como `AND`, `OR`, `NOT` o `NEAR`, y los términos se combinan con el AND implícito de FTS5. No se usa la búsqueda por prefijo (`*`). Si no hay términos, no se consulta FTS5.
- La relevancia solo se calcula en la proyección de enlaces, con `ORDER BY rank` ascendente: en FTS5, `rank` (`bm25()`) es más bajo cuanto mejor es la coincidencia, así que `ORDER BY rank DESC` devolvería primero los menos relevantes. Las consultas sobre las proyecciones de colecciones y de etiquetas solo obtienen los `Id` coincidentes y no usan `rank`; el orden de cada listado es el de [specifications.md → «Listados»](specifications.md#listados) (la colación está en [«Persistencia»](#persistencia)).
- La reconstrucción del índice es un comando de mantenimiento que se ejecuta a mano: vacía todas las proyecciones (las de enlaces y colecciones en el MVP0, y también la de etiquetas en el MVP1) y las regenera desde las tablas, en una transacción. En el MVP0 no se lanza de forma automática.

## Scraping

El scraping automático de metadatos forma parte de MVP1 y no está disponible en MVP0. Su diseño técnico no está cerrado hasta resolver los pendientes de [decisions.md → «Pendientes de scraping»](decisions.md#pendientes-de-scraping). El flujo previsto está en [specifications.md → «Metadatos y scraping»](specifications.md#metadatos-y-scraping).

- Se ejecuta fuera de la petición principal, mediante un `BackgroundService` y trabajos persistidos o un mecanismo equivalente.
- Medidas obligatorias:
  - Protección SSRF: se validan los destinos antes y durante las redirecciones, y se bloquean los destinos loopback, privados y link-local.
  - Solo protocolos `http` y `https`.
  - Timeout.
  - Límite de bytes descargados.
  - Tipos de contenido permitidos.
  - No sustituir nunca la URL original por la de una redirección.
  - Sanitizar los metadatos extraídos.
  - Validar `og:image` según [specifications.md → «Reglas adicionales para `Image`»](specifications.md#reglas-adicionales-para-image).

## Logs

- Serilog implementa el registro estructurado detrás de `ILogger`; Domain no conoce el registro y las demás capas dependen de la abstracción.
- En el MVP0 local se escriben logs por consola y en un archivo rotativo. El nivel mínimo y las salidas se pueden configurar sin cambiar los casos de uso.
- Los logs siguen siendo agnósticos de una plataforma de observabilidad; no se integra ninguna en el MVP0.
- Qué se registra y qué nunca se registra está en [specifications.md → «Registro de eventos»](specifications.md#registro-de-eventos).
- El registro de peticiones del framework también cumple esa regla: no incluye la cadena de consulta de las URL que llevan un token ni la de los listados privados, que puede contener `q`.
