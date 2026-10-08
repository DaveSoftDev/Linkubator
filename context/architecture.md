# Arquitectura prevista para Linkubator

Este documento describe cómo se construirá Linkubator: tecnologías, arquitectura, persistencia, seguridad técnica y procesos en segundo plano. Las reglas de negocio están en [domain-model.md](domain-model.md) y [specifications.md](specifications.md); las razones de cada elección técnica, en [decisions.md → «Tecnología»](decisions.md#tecnología).

## Tecnologías

- .NET 10 y C#.
- ASP.NET Core Razor Pages con renderizado del lado servidor.
- Tailwind CSS.
- SQLite con FTS5, mediante `Microsoft.Data.Sqlite` y Dapper con SQL explícito.
- Argon2id con `Konscious.Security.Cryptography.Argon2` para el hash de las contraseñas.
- MailKit para el envío de correo por SMTP, y smtp4dev como servidor SMTP local de desarrollo.
- Serilog como proveedor de logs estructurados, detrás de `ILogger`.
- Autenticación con el manejador de cookies de ASP.NET Core, sin ASP.NET Core Identity.

> Diagrama de contenedores: [Aplicación y dependencias](diagrams/container-diagram.md).

> Diagrama de contexto: [Contexto](diagrams/context-diagram.md).

## Arquitectura

**Clean Architecture** con las siguientes capas:

> Diagrama de capas: [Capas y dependencias](diagrams/architecture-layers.md).

### Domain

- Entidades del dominio (heredan de `BaseEntity`).
- DTOs de entrada y salida.
- POCOs / ViewModels.
- Enumeraciones.
- Excepciones de dominio.
- Independiente de frameworks externos.
- Transformaciones de texto sin dependencias de infraestructura.
  - La generación de `User.Alias` (ver [specifications.md → «Generación del alias»](specifications.md#generación-del-alias)).
  - La de `Collection.Slug` y `Tag.Slug` (ver [«Generación de slugs»](specifications.md#generación-de-slugs)) son reglas distintas del dominio.
  - La de `Link.UrlOriginal` (ver [specifications.md → «Ajuste de URL»](specifications.md#ajuste-de-url) y [specifications.md → «Validación»](specifications.md#validación)).
  - La de `Link.UrlNormalized` (ver [specifications.md → «Normalización para duplicados»](specifications.md#normalización-para-duplicados)).
  - La de `Link.Image` (ver [specifications.md → «Ajuste de `Image`»](specifications.md#ajuste-de-image) y [specifications.md → «Validación»](specifications.md#validación)).
- No conoce SQLite, Dapper, HTTP, Razor Pages ni detalles de infraestructura.

### Application

- Casos de uso, DTOs, validadores.
- Servicios de aplicación — casos de uso orquestados.
- Service Layer: `I*Service` → `*Service`.
- Contratos, servicios.
- Interfaces de repositorios y de Unit of Work.
- Constantes, Options, Records.
- Patrón `Result`.
- Abstracciones del usuario identificado. 
- Abstracciones del scraper.
- Abstracciones de hashing de contraseñas (`IPasswordHasher`) y de envío de correo (`IEmailSender`).

### Infrastructure

- Repositorios con Dapper y SQL explícito.
- Conexiones SQLite.
- Transacciones.
- Unit of Work ligero.
- Scripts SQL versionados, índices y restricciones.
- Índice de búsqueda con SQLite FTS5, con una proyección propia por entidad buscable: enlaces, colecciones y los nombres de las etiquetas.
- Implementación de `IPasswordHasher` con Argon2id y de `IEmailSender` con MailKit. `Konscious.Security.Cryptography.Argon2` solo calcula el hash: la generación de la sal, el formato PHC y la comparación en tiempo constante los hace esta implementación (valores en [specifications.md → «Hash de la contraseña»](specifications.md#hash-de-la-contraseña)).
- `IPasswordHasher` limitará el número de hashes que se calculan a la vez; los demás esperan en cola. El valor del límite se fija al implementar (motivo en [decisions.md → «Seguridad»](decisions.md#seguridad)). Un límite por IP queda fuera del alcance definido.
- Scraper HTTP y trabajos persistidos, o un mecanismo equivalente, para el scraping (ver [specifications.md → «Scraping»](specifications.md#scraping)).

### Web

- Razor Pages.
- TailwindCSS
- Panel de gestión.
- Páginas públicas.
- Enrutamiento y seguridad web; la autenticación por cookies y la autorización de cuenta.
- Resolución del usuario identificado desde una sesión autenticada. Los casos de uso no reciben un identificador de usuario del cliente.
- Presentación de errores y estados accesibles.
- Los valores usados en atributos HTML, incluidos `Title` y `UrlOriginal` al formar `alt`, se codifican contextualmente con Razor. No se omite esa codificación, no se preescapan los valores y no se aplica codificación de URL al texto de `alt`.

### Dependencias entre proyectos

Domain, Application, Infrastructure y Web forman el grafo productivo. Sus referencias se limitan a esta matriz:

| Proyecto | Puede referenciar |
| --- | --- |
| Domain | Ninguno de los demás proyectos de la solución |
| Application | Domain |
| Infrastructure | Application y Domain |
| Web | Application e Infrastructure |

Web solo referencia Infrastructure en el composition root para registrar sus implementaciones al arrancar. El flujo de negocio permanece en Web → Application → Domain.

Tests puede referenciar los cuatro proyectos de producción para validarlos. Ningún proyecto de producción referencia Tests, que queda fuera del grafo productivo.

### Casos de uso iniciales

Application implementa los casos de uso funcionales definidos en [requirements.md](requirements.md). Web los invoca mediante sus contratos, sin incorporar reglas de negocio a la presentación.

El comportamiento exacto está en [specifications.md](specifications.md) y las invariantes, en [domain-model.md](domain-model.md). El cálculo del hash de contraseña corresponde a `IPasswordHasher` en Infrastructure; Domain recibe el hash ya calculado.

> Diagrama de casos de uso del sistema: [Casos de uso de Linkubator](diagrams/use-case-diagram.md).

### Principios de Diseño

- **SOLID**: Aplicación estricta de los cinco principios
  - Single Responsibility
  - Open/Closed
  - Liskov Substitution
  - Interface Segregation
  - Dependency Inversion

- **KISS** (Keep It Simple, Stupid): Preferir soluciones simples y claras
- **DRY** (Don't Repeat Yourself): Evitar la duplicación de código
- **YAGNI** (You Aren't Gonna Need It): No implementar funcionalidades hasta que sean realmente necesarias
- **Separation of Concerns**: Mantener las responsabilidades de cada clase y módulo claramente definidas
- **Encapsulamiento**: Proteger el estado interno de las clases y exponer solo lo necesario a través de interfaces y métodos públicos
- **Inmutabilidad**: Preferir objetos inmutables cuando sea posible para reducir efectos secundarios y mejorar la predictibilidad del código

## Autenticación

- Para verificar una contraseña se leen juntos `User.Password` y `User.SecurityStamp`, y Argon2id se calcula fuera de la transacción. Antes de aplicar cualquier resultado, las operaciones de escritura comprueban dentro de su transacción `BEGIN IMMEDIATE` que el sello siga vigente y que la cuenta no esté bloqueada; si no, revierten sin ejecutar la operación ni incrementar intentos sobre un estado nuevo. El login vuelve a leer el sello y el bloqueo antes de emitir la cookie, que incluye el sello comprobado. Las reglas de comportamiento están en [specifications.md → «Verificación concurrente de contraseña»](specifications.md#verificación-concurrente-de-contraseña).
- El usuario de desarrollo se provisiona como cuenta completada y con su colección privada base mediante un archivo SQL local especial, separado de los scripts versionados, ejecutable tras ellos y repetible sin duplicar datos. Usa un email de un dominio reservado para pruebas; su contraseña inicial se almacena como hash de una contraseña aleatoria descartada.
- El email de desarrollo se indica en configuración local. El proveedor de identidad, registrado solo en desarrollo, busca esa cuenta al arrancar y falla si no existe o no está completada. En cada petición a `/app` entrega el `UserId` de esa cuenta, sin aceptar que la petición lo elija. Fuera del entorno de desarrollo, la identidad se resuelve desde la sesión autenticada.
- La cookie de autenticación usa el prefijo `__Host-`, `Path=/`, `HttpOnly`, `Secure` y `SameSite=Lax`, y solo contiene un ticket cifrado sin datos personales en claro.
- En cada petición autenticada se comprueba el `SecurityStamp` del ticket contra el del usuario; si no coincide, la sesión deja de ser válida. Al cambiar la contraseña y al confirmar un cambio de email con sesión abierta, se vuelve a emitir la cookie de la sesión actual con el nuevo sello, copiando del ticket anterior su fecha de caducidad y su persistencia. En la confirmación del cambio de email solo se reemite si la sesión pertenece al dueño del token; si no, no se crea ni se modifica ninguna sesión.
- El ticket de autenticación lleva una caducidad absoluta calculada en el login y el deslizamiento de la caducidad (`SlidingExpiration`) está desactivado. La persistencia de la cookie (`Expires`) se decide en cada login según «Recordarme»; el ticket caduca igual en ambos casos (valores en [specifications.md → «Sesión»](specifications.md#sesión)).
- Las páginas de `/app` se agrupan en la carpeta `Pages/App` y se protegen de una vez con la convención de autorización por carpeta (`AuthorizeFolder("/App")`). Las páginas de cuenta que deben ser anónimas se marcan explícitamente (`AllowAnonymousToPage`).
- La preparación de la contraseña que se usa para probar el login es una operación explícita, separada del arranque. La aplicación obtiene la contraseña de configuración local, calcula su hash con `IPasswordHasher` y se lo entrega como parámetro al script SQL local, que actualiza `User.Password` buscando la cuenta por su email. El script no contiene la contraseña en claro ni un hash fijo. Si se vuelve a ejecutar, actualiza el hash según la configuración local vigente.

## Seguridad web

- La aplicación se sirve por HTTPS también en local, con el certificado de desarrollo de .NET, y las peticiones HTTP se redirigen a HTTPS.
- La validación de `ru` y su redirección de respaldo están en [specifications.md → «Inicio de sesión»](specifications.md#inicio-de-sesión).
- Todos los formularios que modifican datos usan POST con validación antiforgery, que Razor Pages aplica por defecto. Cerrar sesión también es un POST con antiforgery. Ningún GET modifica datos, tampoco el de los enlaces de los correos: abrirlos solo muestra una página.
- Todas las respuestas llevan las cabeceras `X-Content-Type-Options: nosniff`, `Content-Security-Policy: frame-ancestors 'none'` y `Referrer-Policy: strict-origin-when-cross-origin`. Al navegar desde una página de Linkubator a un destino externo HTTPS, esta política omite la ruta y la consulta, pero envía el origen de Linkubator.
- Solo se aceptan peticiones cuyo `Host` sea el de la aplicación (`AllowedHosts`); las demás se rechazan.

## Correo

- Los correos se envían con MailKit por SMTP. En local los captura smtp4dev, que los muestra en una interfaz web sin entregarlos a nadie.
- Los correos se encolan en memoria después de confirmar la transacción, y los envía un `BackgroundService` fuera de la petición (motivo en [decisions.md → «Seguridad»](decisions.md#seguridad)).
- Si la aplicación se detiene con correos en cola, se pierden (riesgo aceptado, ver [decisions.md → «Riesgos aceptados»](decisions.md#riesgos-aceptados)).
- Los enlaces de los correos se construyen con el origen público configurado de la aplicación, nunca con la cabecera `Host` de la petición (motivo en [decisions.md → «Seguridad»](decisions.md#seguridad)).

## Enrutamiento

- Todo lo que no es público cuelga de `/app`: el dashboard y la gestión, registro, completar registro, login, recuperación y restablecimiento de contraseña (páginas anónimas), configuración, cambio de contraseña y eliminación de cuenta.
- `/app` redirige al dashboard. Las rutas privadas y sus parámetros están en [specifications.md → «Páginas privadas»](specifications.md#páginas-privadas).
- Cada formulario privado tiene una página Razor canónica. En la gestión de enlaces y colecciones, los diálogos son la presentación de esos mismos formularios y no una ruta ni un caso de uso distinto (ver [specifications.md → «Páginas privadas»](specifications.md#páginas-privadas)).
- La raíz queda reservada a las rutas públicas de [specifications.md → «Páginas públicas»](specifications.md#páginas-públicas) y a los recursos estáticos.
- Las URL canonical usan el origen público configurado de la aplicación, nunca la cabecera `Host` de la petición (motivo en [decisions.md → «Seguridad»](decisions.md#seguridad)). En local, se configura el origen correspondiente al entorno de desarrollo.
- Las rutas literales tienen prioridad sobre las parametrizadas, y `app` no puede ser un alias porque es más corto que la longitud mínima de un alias. Como los alias tampoco tienen puntos, no pueden coincidir con ninguna ruta de la aplicación ni con un recurso estático.
- Las variantes de una URL pública (sin barra final, mayúsculas) se sirven con redirecciones a la URL pública en minúsculas; la etiqueta canonical indica la forma canónica.

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
  - La validez y los cambios de estado de los tokens (`UserToken`): las operaciones que los emiten, consumen o invalidan actualizan su estado dentro de la misma transacción. Al completar el registro, restablecer la contraseña, confirmar un cambio de email o confirmar la eliminación de cuenta, el token presentado se vuelve a validar dentro de la transacción y, si ya no es válido, se revierte sin aplicar nada. La comprobación previa a la transacción solo evita trabajo inútil (por ejemplo, calcular un hash).
  - Los límites de correo (`User.LastEmailSentAt` y `User.LastExistingAccountNoticeAt`) y el contador de intentos fallidos (`User.FailedLoginAttempts`).
  - La propiedad de las relaciones ([domain-model.md → «Propiedad de los datos»](domain-model.md#propiedad-de-los-datos)): al crear o mover un enlace, que la colección sea del usuario; al asociar etiquetas, que cada etiqueta sea del usuario.
  - Las invariantes de [domain-model.md → «Público y privado»](domain-model.md#público-y-privado): al hacer público un enlace, que su colección sea pública en ese momento; al hacer privada una colección, sus enlaces pasan a privados en la misma transacción.
- Las actualizaciones de datos relacionados los hace la aplicación de forma explícita, nunca la base de datos de forma automática (sin `ON UPDATE CASCADE`). Tampoco se usan triggers: la lógica vive en la aplicación.
- Los borrados de datos relacionados los hace la aplicación de forma explícita, nunca la base de datos de forma automática (sin `ON DELETE CASCADE`). Tampoco se usan triggers: la lógica vive en la aplicación.

Operaciones que van en una sola transacción:

- Completar el registro: fijar nombre, alias, contraseña y `EmailConfirmedAt`, crear la «Bandeja de entrada» y consumir el token.
- El restablecimiento de contraseña, la confirmación del cambio de email, la confirmación de eliminación de cuenta, con sus efectos sobre los tokens, el bloqueo y el `SecurityStamp`.
- La eliminación de una cuenta y de todos sus datos.
- Cualquier escritura o borrado de enlaces, colecciones y etiquetas, junto con las proyecciones FTS5 afectadas. Las operaciones de etiquetas incluyen sus relaciones `LinkTag` y actualizan las proyecciones FTS5 afectadas, y guardar el resultado de un intento de scraping (metadatos, estado e intentos) actualiza también la proyección de enlaces en la misma transacción; la descarga se hace antes, fuera de ella.

Configuración y convenciones:

- SQLite se configura con WAL, `busy_timeout` y claves foráneas activadas. Los reintentos por bloqueo son limitados y solo se aplican a errores transitorios identificados.
- Los identificadores son `INTEGER PRIMARY KEY` (el `rowid` de SQLite), salvo en `LinkTag`, cuya clave es la pareja `LinkId + TagId`.
- `User.Name`, `User.Alias` y `User.Password` admiten `NULL` mientras el registro está sin completar. El índice único de `Alias` admite varios `NULL`, como hace SQLite por defecto.
- Todas las fechas se guardan en UTC y en formato ISO 8601.
- El orden alfabético de [specifications.md → «Listados»](specifications.md#listados) usa una colación propia registrada con `SqliteConnection.CreateCollation` en cada conexión que abre la aplicación. Compara con la cultura `es-ES` sin distinguir mayúsculas ni marcas diacríticas (`CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace`).
  - Se aplica con `COLLATE` en el `ORDER BY` de las consultas alfabéticas, no en la definición de las columnas ni en los índices.
  - La aplicación no activa `InvariantGlobalization`, porque la colación necesita los datos de cultura del runtime.
  - No se usa para la unicidad ni para la búsqueda: esas reglas tienen sus propios mecanismos (slugs, `UrlNormalized` y FTS5).
- Las migraciones se resuelven mediante scripts SQL versionados o una herramienta ligera equivalente.

Restricciones e índices previstos:

- Las restricciones de unicidad de [domain-model.md → «Restricciones de unicidad»](domain-model.md#restricciones-de-unicidad).
- Clave foránea compuesta `Link (UserId, CollectionId)` → `Collection (UserId, Id)`, apoyada en un índice único `Collection (UserId, Id)`.
- Índices por usuario, colección, estado público y fecha.
- Índice de `UserToken` por usuario y propósito.

## FTS5

- El índice de búsqueda son proyecciones FTS5 desnormalizadas, una por entidad buscable: enlaces, colecciones y etiquetas. No hay una tabla única con tipos de entrada, de modo que los identificadores de entidades distintas no colisionan y la relevancia `bm25` nunca se mezcla entre entidades.
- En cada proyección el `rowid` es el `Id` de su entidad. Además se guarda el `UserId`, que sirve para filtrar y no forma parte del texto buscable.
  - Proyección de enlaces: la URL original, el título, la descripción, el nombre de la colección y los nombres de sus etiquetas.
  - Proyección de colecciones: su `Name`.
  - Proyección de etiquetas: su `Name`.
  - Renombrar una colección o una etiqueta, actualiza su propia proyección y las filas de la proyección de enlaces que la contienen, en la misma transacción.
- Cada consulta de texto usa solo la proyección de la entidad del listado y combina siempre `MATCH` con el `UserId` del usuario identificado; la relación con la tabla de la entidad también comprueba que esta pertenece a ese usuario. Así no se puede devolver un resultado de otro usuario ni de otra entidad.
- FTS5 se reserva para el texto libre. Los filtros del listado de enlaces (colección y etiqueta) usan SQL e índices convencionales.
- Antes de tokenizar, se aplica a `q` y al texto que se incorpora a las proyecciones la normalización Unicode NFC definida en [specifications.md → «Búsqueda y filtros»](specifications.md#búsqueda-y-filtros). Los valores originales de las entidades no se modifican.
- Tokenizador `unicode61` con `remove_diacritics 2` y categorías `L* N*` iguala «canción» y «cancion», trata la `ñ` como `n` igual que los slugs, y tokeniza solo letras y números, conforme a [specifications.md → «Búsqueda y filtros»](specifications.md#búsqueda-y-filtros). FTS5 no incluye stemming para castellano.
- La consulta se construye con los términos que define [specifications.md → «Búsqueda y filtros»](specifications.md#búsqueda-y-filtros). Cada término se escribe entre comillas dobles, para que FTS5 no interprete como operador palabras como `AND`, `OR`, `NOT` o `NEAR`, y los términos se combinan con el AND implícito de FTS5. No se usa la búsqueda por prefijo (`*`). Si no hay términos, no se consulta FTS5.
- La relevancia solo se calcula en la proyección de enlaces, con `ORDER BY rank` ascendente: en FTS5, `rank` (`bm25()`) es más bajo cuanto mejor es la coincidencia, así que `ORDER BY rank DESC` devolvería primero los menos relevantes. Las consultas sobre las proyecciones de colecciones y de etiquetas solo obtienen los `Id` coincidentes y no usan `rank`; el orden de cada listado es el de [specifications.md → «Listados»](specifications.md#listados) (la colación está en [«Persistencia»](#persistencia)).
- La reconstrucción del índice es un comando de mantenimiento que se ejecuta a mano: vacía todas las proyecciones (las de enlaces, colecciones y las de etiquetas) y las regenera desde las tablas, en una transacción.

## Scraping

Su diseño técnico no está cerrado hasta resolver los pendientes de [decisions.md → «Pendientes de scraping»](decisions.md#pendientes-de-scraping). El flujo previsto está en [specifications.md → «Scraping»](specifications.md#scraping).

- Se ejecuta fuera de la petición principal, mediante un `BackgroundService` y trabajos persistidos o un mecanismo equivalente.
- Medidas obligatorias:
  - Protección SSRF: se validan los destinos antes y durante las redirecciones, y se bloquean los destinos loopback, privados y link-local.
  - Solo protocolos `http` y `https`.
  - Timeout.
  - Límite de bytes descargados.
  - Tipos de contenido permitidos.
  - No sustituir nunca la URL original por la de una redirección.
  - Sanitizar los metadatos extraídos.
  - Ajustar y validar `og:image` según [specifications.md → «Ajuste de `Image`»](specifications.md#ajuste-de-image) y [«Validación»](specifications.md#validación).

## Logs

- Serilog implementa el registro estructurado detrás de `ILogger`; Domain no conoce el registro y las demás capas dependen de la abstracción.
- Se escriben logs por consola y en un archivo rotativo. El nivel mínimo y las salidas se pueden configurar sin cambiar los casos de uso.
- Qué se registra y qué nunca se registra está en [specifications.md → «Registro de eventos»](specifications.md#registro-de-eventos).
- El registro de peticiones del framework también cumple esa regla: no incluye la cadena de consulta de las URL que llevan un token ni la de los listados privados, que puede contener `q`.
