# Decisiones de Linkubator

Este documento recoge qué se ha decidido y por qué, qué riesgos se aceptan, qué queda pendiente, que se llevara a cabo en el MVP0 y qué queda fuera del MVP0. No repite las reglas: cada decisión enlaza al documento donde se especifica.

## Contexto del MVP0

- El MVP0 se ejecuta únicamente en local y no incluye autenticación real ni envío de correo. 
- Para llevar a cabo este MVP0, a la zona privada se le inyecta una identidad de desarrollo que representa a un usuario válido (ver [«Identidad de desarrollo»](#identidad-de-desarrollo)); el detalle técnico está en [architecture.md → «Autenticación»](architecture.md#autenticación).
- Linkubator está preparado para multiples usuarios, y cada uno solo ve y modifica sus propios datos.
- La interfaz está en castellano y funciona en los navegadores actuales.
- Las páginas públicas y sus fundamentos SEO se pueden verificar.
- La landing de la página pública, utiliza el botón de login para entrar en la zona privada con la identidad de desarrollo.

## Confirmadas

### Producto y alcance

- **El MVP0 se ejecuta únicamente en local y usa una identidad de desarrollo inyectada desde código que representa a un usuario válido.** La zona privada y la propiedad por usuario se implementan desde el MVP0; el registro, el inicio de sesión y el resto de la autenticación real se incorporan en el MVP1, para que los casos de uso estén preparados para sustituir esa identidad por la autenticada.
- **El usuario de desarrollo del MVP0 se provisiona por separado y se localiza por su email.** El email es único y estable; usarlo evita acoplar el aprovisionamiento a un `Id` generado o a un alias mutable. Su hash inicial procede de una contraseña aleatoria descartada, para no guardar un secreto de acceso en el repositorio. Los pasos técnicos están en [architecture.md → «Autenticación»](architecture.md#autenticación).
- **Linkubator MVP0 gestiona solamente enlaces y colecciones, preparado para varios usuarios.**
- **La generación del alias forma parte intencionadamente del MVP0, aunque todavía no haya registro ni cambio de alias.** Así el dominio incorpora desde el inicio todas las validaciones del alias que aplicará el sistema, y no solo las necesarias para la identidad de desarrollo.
- **La interfaz está en castellano.** Por eso la colección por defecto se llama «Bandeja de entrada» y el segmento de las rutas públicas es `colecciones`.
- **La raíz `/` muestra una landing pública.** Un visitante que llega sin alias necesita saber qué es Linkubator. Es además el punto de entrada a la gestión mediante el botón de login, porque no hay login real; con la autenticación del MVP1 ese login y registro deben operar de forma normal (ver [specifications.md → «Landing»](specifications.md#landing)).
- **En el MVP0, el enlace de registro de la landing no inicia el registro.** Conserva `href="#"` y no navega a un formulario ni envía una solicitud, porque el registro real pertenece a una etapa posterior; el comportamiento general de la landing está en [specifications.md → «Landing»](specifications.md#landing) y su comprobación, en [roadmap.md → «Área pública y fundamentos SEO»](roadmap.md#área-pública-y-fundamentos-seo).
- **Las páginas públicas se pueden verificar en local, pero no serán rastreables hasta un despliegue público.**
- **Al cambiar un alias o renombrar una colección, la URL anterior responde `404`.** Las redirecciones `301` quedan fuera del MVP0.
- **La gestión de etiquetas forma parte del MVP1.** El MVP0 se limita a colecciones y enlaces.
- **La autenticación real y la gestión de cuentas forman parte del MVP1.** El despliegue y el envío de correo a buzones reales quedan para más adelante.
- **La gestión privada se presentará mediante diálogos en el MVP1.** Se mantienen las páginas canónicas y sus rutas para que cada formulario siga teniendo una dirección directa; los diálogos son una presentación de esos mismos formularios, no flujos distintos. Así se puede actuar desde el contexto de gestión sin perder el acceso directo a cada formulario (ver [specifications.md → «Páginas privadas»](specifications.md#páginas-privadas) y [roadmap.md → «Gestión privada mediante diálogos»](roadmap.md#gestión-privada-mediante-diálogos)).
- **El scraping automático de metadatos forma parte del MVP1, no del MVP0.**

### Cuenta

Las reglas de esta sección describen la gestión de cuentas del MVP1. En el MVP0, la zona privada recibe desde código la identidad de desarrollo indicada en [«Producto y alcance»](#producto-y-alcance); no hay registro, login ni sesiones reales.

- **Los tokens distinguen la consumición de la invalidación.** `UsedAt` registra que la operación del token se completó; `InvalidatedAt` registra que otra operación lo revocó antes de consumirlo. Mantener ambos estados separados evita atribuir una operación completada cuando solo se revocó el token. Las reglas están en [domain-model.md → «UserToken»](domain-model.md#usertoken) y [specifications.md → «Tokens»](specifications.md#tokens).
- **No se puede iniciar sesión sin completar el registro.** Un registro sin completar no tiene contraseña. La identidad de desarrollo ya simula que el usuario ha completado el registro (ver [Identidad de desarrollo](#identidad-de-desarrollo)).
- **Los registros de inicio de sesión se identifican mediante HMAC del email normalizado, no mediante `UserId`.** Así se pueden correlacionar intentos asociados a una misma dirección aunque no exista una cuenta, sin registrar el email en claro ni distinguir en los logs si la cuenta existe. El HMAC sigue permitiendo enlazar los intentos entre sí; la regla exacta está en [specifications.md → «Registro de eventos»](specifications.md#registro-de-eventos).
- **La sesión tiene caducidad absoluta y «Recordarme» no amplía su duración.** Se sigue el plazo de reautenticación de NIST para el nivel AAL1; una sesión que se prolongara con la actividad podría evitar esa reautenticación. Ofrecer una sesión recordada de mayor duración requeriría una credencial separada, fuera del alcance del MVP0. Las reglas están en [specifications.md → «Sesión»](specifications.md#sesión).
- **El límite de correos por minuto solo se aplica a los correos que se pueden pedir; los avisos de seguridad se envían siempre.** Si se limitaran, un restablecimiento hecho justo después de pedirlo se quedaría sin aviso (ver [specifications.md → «Correo»](specifications.md#correo)).
- **Los correos con token y el aviso de cuenta existente tienen límites independientes.** Con un límite compartido, quien registrara repetidamente el email de otra persona mantendría ocupado el límite de esa cuenta y le impediría pedir su propia recuperación (ver [specifications.md → «Correo»](specifications.md#correo)).

### Seguridad

- **Se prioriza una salida accesible ante errores de acceso a recursos privados.** Un aviso uniforme evita revelar datos de terceros y permite recuperar la navegación sin confundir errores de parámetros con páginas inexistentes. El comportamiento está en [specifications.md → «Rutas privadas»](specifications.md#rutas-privadas).
- **Argon2id desde la implementación de autenticación del MVP1, con la configuración mínima de OWASP.** Así se evita tener que migrar hashes más adelante. Los parámetros van dentro del hash (formato PHC), de modo que se pueden endurecer sin invalidar los existentes (ver [specifications.md → «Hash de la contraseña»](specifications.md#hash-de-la-contraseña)).
- **Los tokens de correo se almacenan con SHA-256, no con un hash lento como Argon2.** Como son aleatorios y tienen suficiente entropía, un hash lento no aporta protección práctica adicional. El detalle está en [specifications.md → «Tokens»](specifications.md#tokens).
- **En el MVP1 solo se usan cookies técnicas.** Al estar exentas de consentimiento, no hace falta banner de cookies.
- **El control de concurrencia de hashes protege la memoria del proceso.** Cada cálculo consume mucha memoria; permitir hashes simultáneos sin límite podría agotarla. El mecanismo está descrito en [architecture.md → «Infrastructure»](architecture.md#infrastructure).
- **La cola de correo es en memoria y se procesa fuera de la petición y de la transacción.** Así no se espera al servidor SMTP dentro de una operación de base de datos ni se retrasa la respuesta del usuario por el envío. Se acepta perder los mensajes pendientes si se detiene la aplicación, dado el alcance local; el usuario puede volver a pedirlos.
- **Los enlaces de correo y las URL canonical usan el origen público configurado, no la cabecera `Host`.** El origen configurado evita construir enlaces con un host controlable por quien envía la petición.

### Datos, URLs y búsqueda

- **El alias y los slugs comparten una única transformación a ASCII.** El mismo texto produce el mismo resultado sea cual sea su uso, y el dominio mantiene una sola implementación. Solo difieren en las validaciones posteriores (rangos, palabras reservadas y colisiones), detalladas en [specifications.md → «Transformación común a ASCII»](specifications.md#transformación-común-a-ascii), [«Generación del alias»](specifications.md#generación-del-alias) y [«Generación de slugs»](specifications.md#generación-de-slugs).
- **La transformación rechaza lo que cambiaría el sentido del texto y elimina lo que no tiene equivalente.** Se rechazan los controles, los separadores distintos del espacio normal y las letras o números no ASCII, para no producir un valor parcial que colisione sin explicación. Se eliminan en silencio los emojis, la puntuación y los símbolos fuera de la tabla, que no tienen equivalente ASCII.
- **La transformación normaliza con NFKD antes de pasar a minúsculas.** Así las letras y números de ancho completo, los superíndices y los espacios de compatibilidad se reducen a ASCII. Se aceptan sus efectos: `™` produce `tm`, `½` produce `1-2` y `℃` produce `c`.
- **Se deja margen para el crecimiento de los slugs.** La tabla convierte símbolos en palabras (`C++` pasa de tres caracteres a una expresión más larga), y en SQLite el texto ocupa lo que se usa y no lo declarado. Los límites y el tratamiento de los valores que los superan están en [specifications.md → «Longitudes máximas»](specifications.md#longitudes-máximas).
- **Las proyecciones FTS5 se reconstruyen con un comando manual.** En el MVP0 local basta con poder regenerarlas a mano si se desincronizan; una reparación automática añadiría complejidad sin necesidad (ver [architecture.md → «FTS5»](architecture.md#fts5)).

### Tecnología

- **SQLite con FTS5 para la búsqueda.**
  - Frente a `LIKE`: índice invertido en lugar de recorrer todas las filas, ranking por relevancia (BM25) y consultas booleanas, de frase y de prefijo.
  - Frente a *MySQL* o *PostgreSQL*: no hay servidor que arrancar ni mantener, ni conexiones de red, y la base de datos y el índice viven en un único archivo, así que la copia de seguridad es copiar ese archivo, una vez esté la base de datos sin uso.
  - Límites aceptados: sin stemming por defecto, un único escritor a la vez y sin búsqueda difusa ni vectorial. Si alguno se volviera necesario, habría que pasar a *PostgreSQL*.
- **Dapper en lugar de Entity Framework Core.**
  - Es un micro-ORM sobre ADO.NET: el SQL se escribe explícitamente y se controla por completo, sin change tracking ni traducción de LINQ.
  - En un proyecto de pocas tablas, *EF Core* aporta sobre todo funciones que no se usarían.
  - Su curva de aprendizaje es mínima: quien sabe SQL sabe usar Dapper, sin conceptos como `DbContext` o el ciclo de vida del tracking.
  - Consume menos: sin `DbContext`, `IQueryable` ni identity map en memoria, hay menos objetos y menos trabajo para el recolector de basura.
  - Se renuncia a las migraciones automáticas (bastan scripts SQL versionados), al change tracking, a las propiedades de navegación y a LINQ sobre la base de datos.
- **Una colación propia registrada desde .NET para el orden alfabético.** SQLite solo trae `BINARY`, `NOCASE` (limitada a la A-Z) y `RTRIM`, y ninguna ordena como el castellano. Frente a guardar una columna con una clave de ordenación precalculada, no duplica datos ni obliga a recalcularla si cambian las reglas de idioma. Se aplica en las consultas y no en columnas ni índices para que la base se pueda seguir abriendo y modificando con herramientas externas, como la CLI de `sqlite3`, que no conocen la colación. Se acepta que el orden dependa de los datos de cultura del runtime y que, en el MVP0, la colación pueda equiparar `ñ` con `n` sin lógica especial para separarlas (ver [architecture.md → «Persistencia»](architecture.md#persistencia) y [specifications.md → «Listados»](specifications.md#listados)).
- **Serilog como proveedor de logs detrás de `ILogger`.** Aporta eventos estructurados y salidas locales por consola y archivo rotativo sin acoplar los casos de uso ni el dominio a un proveedor. Mantiene abierta una integración futura con una plataforma de observabilidad, que sigue fuera del MVP0 (ver [architecture.md → «Logs»](architecture.md#logs)).
- **Autenticación por cookies de ASP.NET Core, sin ASP.NET Core Identity, en el MVP1.** Los almacenes por defecto de Identity dependen de Entity Framework Core, que no se usa.
- **MailKit para el correo y smtp4dev en local, ambos en el MVP1.** Microsoft recomienda MailKit frente a `System.Net.Mail.SmtpClient` para desarrollos nuevos. En MVP1 bastará con cambiar la configuración SMTP a un proveedor real.
- **`Konscious.Security.Cryptography.Argon2`, MailKit y Serilog son excepciones justificadas a la filosofía de mínimas dependencias.** Son librerías acotadas y de propósito específico, no abstracciones pesadas como EF Core.
- **Implementar Identidad de desarrollo para la gestión privada.** Para este MVP implementaremos un sistema de identidad de desarrollo para que toda la aplicación esté preparada a trabajar con identidad y que al implementar la gestión de cuenta en el MVP1 sea mucho más fácil (ver [«Identidad de desarrollo»](#identidad-de-desarrollo)).
- **Usar una excepción base de dominio y una excepción específica por regla.** Así los consumidores pueden manejar cualquier error de dominio en conjunto o capturar por separado cada regla; el contrato técnico está en [S1.2 → Componentes y contratos](../specs/s1.2-errores-y-enumeradores/plan.md#componentes-y-contratos). Decisión aprobada por DLG el 09-X-2026.

## Riesgos aceptados

- **Bloqueos provocados por terceros.** Quien conozca el email de una cuenta puede provocar bloqueos temporales.
- **Saturación del límite de correo por terceros.** Cuando se incorpore la autenticación en el MVP1, quien conozca el email de una cuenta podría pedir su recuperación una y otra vez: la víctima recibiría correos que no ha pedido y cada token nuevo invalidaría el anterior. Se acepta este riesgo para el alcance local; un límite por IP o una verificación anti-bots se valorará antes de la exposición pública.
- **Sin límite de peticiones por IP en el login, el registro y la recuperación de contraseña.** Como el login calcula siempre el hash, un volumen alto de peticiones puede saturar la cola de hashes simultáneos (ver [architecture.md → «Arquitectura»](architecture.md#arquitectura)) y retrasar los logins legítimos. Se acepta para el alcance local; el límite por IP se valorará antes de la exposición pública (ver [«Fuera de alcance del MVP0»](#fuera-de-alcance-del-mvp0)).
- **Reutilización de alias liberados.** Un alias liberado (por cambio de alias o eliminación de cuenta) puede ocuparlo otro usuario, que heredaría las URLs públicas que circulaban del anterior. En local, sin buscadores ni enlaces compartidos, el impacto es nulo.
- **Acumulación de registros sin completar.** Al incorporar el registro en el MVP1, cualquiera podrá registrar emails ajenos. Cada registro solo contendrá el email, no bloqueará a su dueño (volver a registrarse reenviará el enlace) y no se limpiará inicialmente.
- **Pérdida de correos en cola.** Si la aplicación se detiene con correos pendientes de enviar, se pierden y el usuario tendrá que volver a pedirlos.
- **Clave plana para detectar duplicados de URL.** Se usa `UrlNormalized`, no una URL reconstruible: el algoritmo descarta separadores y normaliza componentes para detectar equivalencias, aceptando que URLs distintas colisionen. Se decodifica cada componente una sola vez y con su regla correspondiente (en la ruta `+` es literal y en la consulta representa un espacio), para que la clave no dependa de la ubicación del texto. El host se expresa en punycode mediante `IdnHost`, unificando dominios internacionales escritos en Unicode o punycode; la ruta conserva letras y números de cualquier alfabeto porque la clave es interna y limitarla a ASCII haría colisionar rutas de otros alfabetos. Como riesgo aceptado, las colisiones también pueden afectar palabras diferenciadas solo por una marca diacrítica eliminada en todos los alfabetos (por ejemplo, las kanas `か` y `が`). Si hay colisión, se informa del enlace existente y el usuario puede comparar las URL originales. La regla exacta está en [specifications.md → «Normalización para duplicados»](specifications.md#normalización-para-duplicados) y [«Duplicados»](specifications.md#duplicados).
- **Conversión a ASCII del alias y de los slugs.** Dos nombres pueden producir el mismo slug por acentos (`Papá` y `papa`) o por símbolos que no están en la tabla, y los nombres o alias escritos solo en otros alfabetos no se pueden crear. Se conoce la problemática de convertir texto no ASCII (alfabetos, normalización Unicode, caracteres que se confunden con los latinos en URLs públicas y comportamiento de `%2F` y `+` en rutas) y se acepta en el MVP0, que no será público; se revisará en el MVP1.
- **Los servidores de imágenes externas ven a los visitantes.** Al mostrar `Image` directamente desde su origen, ese servidor recibe la IP y el navegador de cada visitante de la página pública. Para solicitudes a destinos HTTPS de otro origen, `Referrer-Policy` oculta la ruta y la consulta de la página, pero envía el origen de Linkubator y no oculta la visita. Servir las imágenes desde Linkubator queda fuera del MVP0.
- **La validación de `Image` no garantiza que el destino sea público.** Se comprueba que el host tenga formato de dominio, pero no se valida su resolución DNS ni el destino de las redirecciones. Un dominio podría resolver o redirigir a una dirección privada y hacer que el navegador de un visitante intente acceder a su red local; se acepta este riesgo en el MVP0 (ver [specifications.md → «Ajuste de `Image`»](specifications.md#ajuste-de-image)).

### Identidad de desarrollo

- La identidad de desarrollo permite validar la propiedad por usuario y los casos de uso privados antes de incorporar autenticación real. Su provisión y resolución técnica están en [architecture.md → «Autenticación»](architecture.md#autenticación).
- El aprovisionamiento usa el email único y estable en lugar del `Id` generado o del alias mutable. El hash inicial procede de una contraseña aleatoria descartada, para no incluir una credencial de desarrollo en el repositorio.

## Pendientes

### Pendientes de scraping

El scraping está confirmado para MVP1. Quedan pendientes decisiones de diseño y de ejecución en segundo plano, que deben resolverse antes de implementarlo.

1. Timeout exacto del scraper.
2. Redirecciones permitidas durante el scraping.
3. Tamaño máximo de respuesta del scraper.
4. Tipos de contenido aceptados por el scraper.
5. Relación entre el intento lanzado al presionar el botón al lado del campo URL, cuando el enlace aún no está guardado, y el enlace persistido (`ScrapingAttempts`, `NextScrapingAt`), incluido qué ocurre si el usuario guarda antes de que termine.
6. Si los enlaces creados antes de activar el scraping (`ScrapingStatus = NotRequested`) se procesarán después.
7. Contra qué URL se resuelve una `og:image` relativa cuando la página ha redirigido: la URL original del enlace o la final de la que se obtuvo el documento.
8. Cómo se recupera un enlace que se queda en `Processing` si la aplicación se detiene durante un intento.
9. Alcance de la protección SSRF más allá de las redirecciones: que la dirección comprobada sea la misma a la que se conecta (resolución DNS) y el tratamiento de las direcciones IPv6 y de las IPv4 mapeadas en IPv6.
10. Qué ocurre si el enlace se borra o el usuario edita sus metadatos mientras hay un intento en curso.

### Pendiente de limpieza de tokens

1. Definir cuándo y cómo eliminar los registros `UserToken` usados, caducados o invalidados, y cuánto tiempo conservarlos. La limpieza de los registros de cuentas sin completar está prevista fuera del MVP0.

### Pendiente de rutas de cuenta

Antes de implementar [Autenticación y gestión de cuenta](roadmap.md#autenticación-y-gestión-de-cuenta), definir en [specifications.md → «Páginas privadas»](specifications.md#páginas-privadas) las rutas y métodos HTTP de registro, compleción del registro, inicio y cierre de sesión, recuperación y restablecimiento de contraseña, configuración de usuario, cambio de email y contraseña, y solicitud y confirmación de eliminación de cuenta. También se debe precisar para cada ruta si requiere sesión autenticada o permite acceso anónimo, y cómo se transporta el token cuando corresponda. Sin esas definiciones no se podrá implementar la sección de cuenta.

## Fuera de alcance del MVP0

- Scraping automático de metadatos (ver decisión confirmada en [«Producto y alcance»](#producto-y-alcance)).
- Consulta online de contraseñas comprometidas. **NIST SP 800-63B-4** exige comparar la contraseña con una lista de contraseñas prohibidas. En el MVP0 se usará la lista local. Como complemento futuro al MVP1 se sugiere *Pwned Passwords*, de Have I Been Pwned (ver [documentación de Pwned Passwords](https://haveibeenpwned.com/API/v3#PwnedPasswords)), mediante su endpoint de k-anonimato (`https://api.pwnedpasswords.com/range/{5 primeros caracteres del hash SHA-1}`), gratuito y sin enviar la contraseña en claro. Tiene sentido cuando haya exposición pública real.
- Envío de correo mediante un proveedor real.
- Redirecciones `301` de alias y de slug de colección.
- Reserva temporal de los alias liberados.
- Limpieza de los registros sin completar.
- Importación de enlaces (por ejemplo, desde los marcadores del navegador).
- Exportación de enlaces.
- Búsqueda por prefijo y operadores de búsqueda (frases exactas, `OR`, `NOT`, `NEAR`, `*`).
- Extensión de navegador.
- Analítica avanzada.
- Plataforma externa de observabilidad (Grafana, Kibana o similares).
- SEO avanzado: Open Graph, Twitter Cards, `robots.txt`, `sitemap.xml`, datos estructurados, política de indexación de las páginas interiores de la paginación (`rel="prev"`, `rel="next"`, `noindex`) y auditoría de títulos, descripciones, canonical y rastreabilidad.
- Content-Security-Policy completa; en el MVP0 solo se usa `frame-ancestors`.
- Proxy o caché de imágenes en el servidor, para servir `Image` desde Linkubator y que los servidores externos no vean a los visitantes.
- Límite de peticiones por IP en el login, el registro y la recuperación de contraseña. No forma parte del alcance comprometido; el MVP1 sí limita los hashes simultáneos (ver [architecture.md → «Arquitectura»](architecture.md#arquitectura)).
- Slugs y alias con caracteres no ASCII, y una clave propia para las etiquetas, que no aparece en ninguna URL (ver el riesgo «Conversión a ASCII de los slugs» en [«Riesgos aceptados»](#riesgos-aceptados)).
- Estrategia de despliegue.
