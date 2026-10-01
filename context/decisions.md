# Decisiones de Linkubator

Este documento recoge qué se ha decidido y por qué, qué riesgos se aceptan, qué queda pendiente y qué queda fuera del MVP0. No repite las reglas: cada decisión enlaza al documento donde se especifica.

## Confirmadas

### Producto y alcance

- **Linkubator gestiona enlaces, colecciones y etiquetas, preparado para varios usuarios.** En la documentación funcional se denomina «etiqueta»; `Tag` y `LinkTag` se reservan para los nombres técnicos.
- **El MVP0 se ejecuta únicamente en local.** El despliegue y el envío de correo real quedan para más adelante.
- **La interfaz está en castellano.** Por eso la colección por defecto se llama «Bandeja de entrada» y el segmento de las rutas públicas es `colecciones`.
- **Toda la zona privada y de cuenta cuelga de `/app`.** Así la raíz queda libre para las páginas públicas con alias, sin colisiones (ver architecture.md → «Enrutamiento»).
- **La raíz `/` muestra una landing pública.** Un visitante que llega sin alias necesita saber qué es Linkubator y cómo entrar; sin landing, la raíz solo podría redirigir al login, que no explica nada.
- **Las páginas públicas se pueden verificar en local, pero no serán rastreables hasta un despliegue público.**
- **Cada usuario recibe al completar el registro una colección privada «Bandeja de entrada».** Así puede guardar sus primeros enlaces sin tener que crear una colección. Es una colección normal: se puede renombrar.
- **Un usuario conserva siempre al menos una colección.** No se puede borrar la última, esté o no vacía, para que siempre haya dónde guardar un enlace nuevo.
- **Solo se borran colecciones y etiquetas vacías, y el borrado es siempre físico.**
- **Hacer algo público es siempre una acción explícita del usuario.** Colecciones y enlaces nacen privados, y mover un enlace o volver a hacer pública una colección nunca hace públicos enlaces por su cuenta (ver domain-model.md → «Público y privado»).
- **Al cambiar un alias o renombrar una colección, la URL anterior responde `404`.** Las redirecciones `301` quedan fuera del MVP0.
- **Una colección pública sin enlaces públicos no se muestra en público: su página responde `404` y no aparece en la página de usuario ni en la de colecciones.** Servir esa página vacía haría que los buscadores la catalogaran como de bajo contenido (soft 404); y si apareciera en los listados, estos enlazarían a un `404`. Por la misma razón, la página de un usuario sin ninguna colección que se muestre responde `404`, lo que además no revela si tiene colecciones privadas (ver domain-model.md → «Público y privado»).
- **Las colecciones se ordenan por `Slug` en los listados y por `CreatedAt` en la página de usuario.** En un listado completo se busca por nombre, así que el orden alfabético es el útil; la página de usuario muestra «las últimas», y eso solo tiene sentido por fecha (ver specifications.md → «Listados»).
- **Las páginas públicas no muestran el nombre ni el email del usuario; su título es el alias.** El alias ya es público; el resto son datos personales.
- **Barra final solo en las rutas que tienen páginas por debajo.** Distingue las rutas contenedoras (`/{alias}/`) de las finales (`/{alias}/colecciones/{slug}`) y hace predecible la forma canónica de cada URL (ver specifications.md → «Rutas»).
- **En las páginas públicas el `h1` y el `meta title` son el mismo texto, en la página de usuario y en la de colecciones incluyen su alias, y el `meta title` lleva siempre el sufijo ` | Linkubator`.** Que `h1` y `meta title` coincidan refuerza la relevancia de la página para ese texto ante los buscadores; que incluyan el alias evita títulos duplicados entre usuarios y concentra la señal en el nombre público de cada uno (la página de colección usa solo el nombre de la colección, que puede repetirse entre usuarios, y se acepta); el sufijo identifica el servicio en los resultados sin ensuciar el `h1` (ver specifications.md → «Páginas públicas»).
- **La paginación pública usa el parámetro de consulta `pagina`, y cada página interior es su propia canonical.** Un parámetro es lo que los buscadores esperan para paginar y no añade segmentos a las rutas; la primera página no lleva parámetro para que su URL sea la limpia de la tabla de rutas. La política de indexación de las páginas interiores (`rel="prev"`/`rel="next"`, `noindex`) es SEO avanzado y queda fuera del MVP0 (ver specifications.md → «Paginación de las páginas públicas»).
- **El SEO se aborda por fases.** Lo que afecta al dominio, a las rutas y al HTML se define desde el MVP0; el SEO avanzado queda fuera.
- **El scraping forma parte del MVP0, pero se implementa en una fase posterior.** Hasta entonces, el usuario rellena los metadatos a mano (ver specifications.md → «Metadatos y scraping»).
- **El scraper solo reintenta los errores transitorios.** Un `200` sin metadatos no va a tenerlos 5 minutos después, y un `404` no va a convertirse en `200`; reintentar en esos casos solo consumiría recursos. Solo el timeout, el error de red y el `5xx` pueden resolverse solos, y por eso son los únicos que se reintentan (ver specifications.md → «Metadatos y scraping»).
- **Una etiqueta escrita en el formulario del enlace cuyo slug coincide con el de una existente se asocia en lugar de rechazarse.** El usuario no tiene por qué saber si ya la había creado, ni con qué mayúsculas o acentos.
- **«Mostrar contraseña» y «Recordarme» tienen un límite de implementación de 1 hora cada uno.** Si superan ese tiempo, pasan al backlog de MVP1; se decide al cerrar la etapa de roadmap.md → «Interfaz privada - Gestión básica».
- **Se respeta `prefers-reduced-motion`.** Algunas animaciones pueden provocar molestias a personas con trastornos vestibulares.

### Cuenta

- **El registro pide solo el email; el nombre, el alias y la contraseña se piden al completar el registro desde el enlace del correo.** Un registro sin completar solo contiene un email: no puede retener un alias ajeno ni filtra nada si se abandona. Volver a registrarse con el mismo email reenvía el enlace, lo que elimina el reenvío desde el login y la sustitución de cuentas sin verificar. La respuesta del registro queda genérica sin excepciones, porque el alias ocupado se indica en la página de completar, a la que solo llega quien tiene el correo. Y el formulario largo se rellena después de demostrar la titularidad del email, así que nunca se pierde lo escrito. El registro adopta la misma forma que la recuperación de contraseña: email, enlace y formulario (ver specifications.md → «Registro» y «Completar el registro»).
- **No se puede iniciar sesión sin completar el registro.** Un registro sin completar no tiene contraseña.
- **El alias ocupado se indica al completar el registro y al cambiarlo.** El alias es público, así que decirlo no revela nada privado.
- **Cambiar el email exige la contraseña actual.** Con una sesión robada bastaría cambiar el email y pedir la recuperación para quedarse con la cuenta.
- **El nuevo email se comprueba al pedir el cambio y otra vez al confirmarlo.** Otra cuenta puede haberlo registrado entretanto.
- **El cambio de email se confirma con un POST desde una página de confirmación, no con el GET del enlace.** Los antivirus, los filtros de correo y las previsualizaciones abren los enlaces antes que el usuario: si el GET aplicara el cambio, lo harían sin que nadie lo decidiera y gastarían el token. La sesión solo se reemite si es del dueño del token; de lo contrario, abrir el enlace podría iniciar sesión como otra cuenta o alterar la de otro usuario. No se pide volver a escribir el email: tener el enlace ya prueba el control del buzón, y quien confirma no puede cambiar nada de lo que el dueño ya pidió con su contraseña (ver specifications.md → «Cambio de email»).
- **Pedir un cambio de email hacia un email ocupado responde igual que hacia uno libre, y avisa al dueño de ese email.** Si se indicara que está ocupado, cualquier usuario con sesión podría averiguar qué emails tienen cuenta. Se reutiliza el aviso de cuenta existente del registro, que además alerta al titular del intento. Como la contraseña se verifica antes de comprobar el email, ambas ramas calculan el hash y el tiempo de respuesta no las distingue (ver specifications.md → «Cambio de email»).
- **Cambiar o restablecer la contraseña invalida también los cambios de email pendientes.** Quien restablece su contraseña porque sospecha de un acceso ajeno no debe dejar vivo un cambio de email iniciado por otro.
- **Confirmar un cambio de email invalida los restablecimientos de contraseña pendientes y cierra las demás sesiones.** Es el mismo razonamiento en sentido inverso: quien cambia de email suele hacerlo para dejar atrás un buzón en el que ya no confía, y un token de restablecimiento enviado a ese buzón seguiría siendo válido hasta caducar. Cerrar las demás sesiones expulsa a quien pudiera estar usando una sesión robada, igual que al cambiar la contraseña (ver specifications.md → «Cambio de email»).
- **El límite de correos por minuto solo se aplica a los correos que se pueden pedir; los avisos de seguridad se envían siempre.** Si se limitaran, un restablecimiento hecho justo después de pedirlo se quedaría sin aviso (ver specifications.md → «Correo»).
- **Los correos con token y el aviso de cuenta existente tienen límites independientes.** Con un límite compartido, quien registrara repetidamente el email de otra persona mantendría ocupado el límite de esa cuenta y le impediría pedir su propia recuperación (ver specifications.md → «Correo»).

### Seguridad

- **Las contraseñas siguen la guía NIST SP 800-63B-4 desde el MVP0** (ver specifications.md → «Contraseñas»).
  - No se exigen reglas de composición porque los usuarios tienden a cumplirlas de forma predecible (una mayúscula al principio y un número o símbolo al final), lo que apenas mejora la seguridad.
  - El bloqueo tras intentos fallidos es más estricto que el máximo de 100 que fija NIST.
  - El usuario es el único responsable de la fortaleza de la contraseña que elija.
- **Argon2id desde el primer día, con la configuración mínima de OWASP.** Así se evita tener que migrar hashes más adelante. Los parámetros van dentro del hash (formato PHC), de modo que se pueden endurecer sin invalidar los existentes (ver specifications.md → «Hash de la contraseña»).
- **Las respuestas nunca revelan si un email tiene cuenta, ni por su contenido ni por su tipo de respuesta.** Por eso los correos se envían en segundo plano.
- **El login calcula el hash Argon2id aunque el email no exista, el registro esté sin completar o la cuenta esté bloqueada.** Argon2id es lento a propósito y es la operación dominante del login: si solo se calculara cuando hay una contraseña guardada y la cuenta no está bloqueada, el tiempo de respuesta distinguiría las ramas aunque el mensaje fuera el mismo. En un login bloqueado se descarta el resultado y el coste es el de un email inexistente, que ya se puede provocar, así que no abre una vía nueva de agotamiento de recursos. Con sesión abierta no se iguala: el usuario ya está autenticado como esa cuenta y el tiempo no revela nada. El registro, la recuperación y la petición de cambio de email no reciben una contraseña sin verificar, así que no tienen nada que igualar; sus diferencias entre ramas (algunas escrituras en SQLite local) son de un orden inferior y no se igualan (ver specifications.md → «Respuestas que no revelan si una cuenta existe»).
- **En `/app`, un recurso inexistente y uno de otro usuario responden igual (`404`).** Así no se revela que existen datos de otros usuarios (ver specifications.md → «Errores controlados»).
- **Los tokens de correo se guardan como hash SHA-256, no con Argon2.** Al ser aleatorios de 256 bits, no necesitan un algoritmo lento.
- **La duración máxima de la sesión es la que fija NIST para el nivel AAL1, y es absoluta, no deslizante.** NIST exige reautenticarse pasado ese plazo con independencia de la actividad. El manejador de cookies de ASP.NET Core desliza la caducidad por defecto, de modo que un usuario activo no volvería a autenticarse jamás; por eso se desactiva. Al volver a emitir la sesión (cambio de contraseña o de email) se conserva la caducidad original, para que esas operaciones no sirvan para alargar la sesión. «Recordarme» no amplía el plazo: solo evita que la sesión muera al cerrar el navegador. Se asume que esto contradice la expectativa habitual de un «Recordarme» sin fecha de fin; la forma correcta de ofrecerlo sería un token de larga duración separado de la sesión, que queda fuera del MVP0 (ver specifications.md → «Sesión»).
- **Solo cookies técnicas.** Al estar exentas de consentimiento, no hace falta banner de cookies.
- **HTTPS también en local.** Lo exige la cookie `__Host-`, que lleva `Secure`.
- **Los enlaces de los correos usan un origen público configurado, no la cabecera `Host`.** Esa cabecera la controla quien hace la petición: con ella, un atacante podría pedir la recuperación de la contraseña de otra persona con un `Host` propio y recibir el token cuando la víctima pulse el enlace de un correo legítimo. El mismo origen se usa en el canonical, y `AllowedHosts` rechaza los hosts ajenos (ver architecture.md → «Correo» y «Seguridad web»).
- **Los tokens de los enlaces nunca se registran, ni siquiera dentro de la URL.** Un token en un log equivale a una contraseña de un solo uso: quien lea el log podría usarlo mientras sea válido (ver specifications.md → «Registro de eventos»).
- **`ReturnUrl` solo acepta URLs locales.** Evita las redirecciones abiertas.
- **Cabeceras de seguridad mínimas (`nosniff`, `frame-ancestors 'none'` y `Referrer-Policy`).** Impiden interpretar respuestas con otro tipo de contenido y que la aplicación se incruste en otra página. `Referrer-Policy: strict-origin-when-cross-origin` evita que la URL de una página pública (con alias y slug) llegue a los servidores de las imágenes externas que muestra; solo reciben el origen. Una CSP completa queda fuera del MVP0.
- **Los enlaces a URLs de usuarios llevan `rel="nofollow ugc noopener noreferrer"`.** Son URLs de terceros elegidas por los usuarios: no deben transmitir reputación SEO ni acceso a la página de origen.

### Datos, URLs y búsqueda

- **Solo se aceptan URLs `http` y `https`.** Descarta esquemas peligrosos como `javascript:` o `data:`. En `UrlOriginal` se aceptan `localhost` y direcciones IP; que el scraper pueda o no descargarlas es otra cuestión, resuelta por la protección SSRF.
- **`Image` solo admite `https` y hosts públicos (ni `localhost` ni direcciones IP).** `UrlOriginal` solo se pide cuando alguien hace clic; `Image` la descarga automáticamente el navegador de cada visitante de una página pública. Una imagen en `localhost` o en una IP privada haría que cada visitante lanzara peticiones contra su propia red local, y una imagen `http` dentro de una página `https` es contenido mixto, que los navegadores bloquean o marcan con advertencias (ver specifications.md → «Reglas adicionales para `Image`»).
- **`Image` acepta URLs relativas al protocolo (`//host/ruta`) y les antepone `https:`; `UrlOriginal` las rechaza.** Durante la migración de la web de `http` a `https` fue habitual publicar los recursos sin esquema para que heredaran el de la página y evitar contenido mixto, y muchos `og:image` siguen así; descartarlos perdería imágenes válidas. En `Image` no hay ambigüedad, porque solo se admite `https`. En `UrlOriginal` ambos esquemas son válidos y no se puede saber cuál quería el usuario, así que se le pide que lo indique.
- **La URL que guarda el usuario no se transforma más allá del ajuste, y nunca se sustituye por la de una redirección.** Es la que se usa para navegar.
- **Los duplicados se detectan con una clave plana (`UrlNormalized`), no con una URL reconstruible.** Se aceptan conscientemente colisiones derivadas de eliminar separadores. El fragmento y los parámetros se identifican sobre la URL analizada, antes de decodificar, para que un carácter estructural codificado (`%23`, `%26`) no cambie la estructura de la URL. Cada componente se decodifica una sola vez y con la regla que le corresponde: en la ruta `+` es literal y en la consulta es un espacio; decodificar dos veces haría que la clave dependiera de en qué parte de la URL está el texto. El host se toma en punycode (`IdnHost`) para que un mismo dominio internacionalizado escrito en Unicode o en punycode produzca la misma clave, y se conservan las letras y los números de cualquier alfabeto: la clave es interna y no necesita la restricción a ASCII de los slugs, y con solo ASCII todas las rutas en otros alfabetos colisionarían entre sí (ver specifications.md → «Normalización para duplicados»).
- **No se siguen redirecciones para detectar duplicados.** Haría falta una petición HTTP en el momento de guardar, y el duplicado se comprueba dentro de la transacción de escritura, donde no se hacen llamadas externas.
- **Un duplicado se rechaza indicando en qué colección está el enlace existente.** El enlace es del propio usuario, así que decirle dónde está no revela nada y le ahorra buscarlo.
- **Lo que introduce el usuario por encima del máximo se rechaza; lo que obtiene el scraper se trunca.** El usuario puede corregir su texto; el de una página externa no.
- **Los textos se recortan de espacios, salvo la contraseña.** En una contraseña, los espacios son caracteres válidos.
- **Del email solo se valida la sintaxis mínima.** La validación real es el correo de verificación; una validación sintáctica estricta rechazaría direcciones válidas sin aportar seguridad.
- **Alias y slugs solo usan ASCII, y un resultado fuera de rango se rechaza en lugar de truncarse.** Forman parte de URLs públicas: ASCII evita codificaciones y dobles formas de la misma dirección, y truncar produciría un alias o un slug que el usuario no ha visto ni elegido.
- **El alias tiene un mínimo de 10 caracteres.** Además de evitar alias triviales, garantiza que ningún alias pueda coincidir con el segmento `app` ni con otras rutas cortas de la aplicación (ver architecture.md → «Enrutamiento»).
- **Palabras reservadas para el alias.** Evitan suplantar al servicio (lista en specifications.md → «Generación de alias y slugs»).
- **Identificadores `INTEGER PRIMARY KEY`.** Es el `rowid` que usa FTS5 y da sentido al desempate por `Id`.
- **Las invariantes entre tablas se garantizan en la transacción de escritura, y la de enlace–colección además con una clave foránea compuesta.** SQLite serializa los escritores, así que comprobar la regla dentro de `BEGIN IMMEDIATE` basta para que dos peticiones concurrentes (hacer privada una colección mientras se hace público uno de sus enlaces) no dejen un enlace público en una colección privada. La clave foránea compuesta añade una garantía física para la propiedad enlace–colección sin coste (solo un índice único sobre `Collection (UserId, Id)`). No se hace lo mismo con las etiquetas porque exigiría añadir `UserId` a `LinkTag`, y no se usan triggers para no repartir la lógica entre la aplicación y la base de datos (ver architecture.md → «Persistencia»).
- **Fechas en UTC.** Evita ambigüedades con cambios de hora y zonas, y el orden por fecha es el mismo en cualquier lugar.
- **El índice FTS5 se reconstruye con un comando manual.** En el MVP0 local basta con poder regenerarlo a mano si se desincroniza; una reparación automática añadiría complejidad sin necesidad (ver architecture.md → «FTS5»).
- **La búsqueda no distingue acentos y no aplica stemming.** Sin distinguir acentos, el usuario encuentra lo que escribe aunque no acentúe; el stemming en castellano no viene con FTS5 y su ausencia se acepta como límite (ver architecture.md → «FTS5»).

### Tecnología

- **Razor Pages con renderizado en servidor.** Las páginas públicas deben llegar a los buscadores con HTML semántico ya renderizado.
- **SQLite con FTS5 para la búsqueda.**
  - Frente a `LIKE`: índice invertido en lugar de recorrer todas las filas, ranking por relevancia (BM25) y consultas booleanas, de frase y de prefijo.
  - Frente a MySQL o PostgreSQL: no hay servidor que arrancar ni mantener, ni conexiones de red, y la base de datos y el índice viven en un único archivo, así que la copia de seguridad es copiar ese archivo.
  - Límites aceptados: sin stemming por defecto, un único escritor a la vez y sin búsqueda difusa ni vectorial. Si alguno se volviera necesario, habría que pasar a PostgreSQL.
- **Dapper en lugar de Entity Framework Core.**
  - Es un micro-ORM sobre ADO.NET: el SQL se escribe explícitamente y se controla por completo, sin change tracking ni traducción de LINQ.
  - En un proyecto de pocas tablas, EF Core aporta sobre todo funciones que no se usarían.
  - Su curva de aprendizaje es mínima: quien sabe SQL sabe usar Dapper, sin conceptos como `DbContext` o el ciclo de vida del tracking.
  - Consume menos: sin `DbContext`, `IQueryable` ni identity map en memoria, hay menos objetos y menos trabajo para el recolector de basura.
  - Se renuncia a las migraciones automáticas (bastan scripts SQL versionados), al change tracking, a las propiedades de navegación y a LINQ sobre la base de datos.
- **Serilog como proveedor de logs detrás de `ILogger`.** Aporta eventos estructurados y salidas locales por consola y archivo rotativo sin acoplar los casos de uso ni el dominio a un proveedor. Mantiene abierta una integración futura con una plataforma de observabilidad, que sigue fuera del MVP0 (ver architecture.md → «Logs»).
- **Autenticación por cookies de ASP.NET Core, sin ASP.NET Core Identity.** Los almacenes por defecto de Identity dependen de Entity Framework Core, que no se usa.
- **MailKit para el correo y smtp4dev en local.** Microsoft recomienda MailKit frente a `System.Net.Mail.SmtpClient` para desarrollos nuevos. En MVP1 bastará con cambiar la configuración SMTP a un proveedor real.
- **`Konscious.Security.Cryptography.Argon2`, MailKit y Serilog son excepciones justificadas a la filosofía de mínimas dependencias.** Son librerías acotadas y de propósito específico, no abstracciones pesadas como EF Core.
- **Clean Architecture, SOLID, DRY y YAGNI, con los patrones Repository, Unit of Work y Result.**

## Riesgos aceptados

- **Bloqueos provocados por terceros.** Quien conozca el email de una cuenta puede provocar bloqueos temporales.
- **Saturación del límite de correo por terceros.** Quien conozca el email de una cuenta puede pedir su recuperación una y otra vez: la víctima recibe correos que no ha pedido y cada token nuevo invalida el anterior, de modo que su enlace deja de valer en cuanto llega otro. Se acepta en el MVP0 local; un límite por IP o una verificación anti-bots lo resolvería cuando haya exposición pública.
- **Reutilización de alias liberados.** Un alias liberado (por cambio de alias o eliminación de cuenta) puede ocuparlo otro usuario, que heredaría las URLs públicas que circulaban del anterior. En local, sin buscadores ni enlaces compartidos, el impacto es nulo.
- **Acumulación de registros sin completar.** Cualquiera puede registrar emails ajenos. Cada uno crea un registro que solo contiene el email, no bloquea a su dueño (volver a registrarse reenvía el enlace) y no se limpia en el MVP0.
- **Pérdida de correos en cola.** Si la aplicación se detiene con correos pendientes de enviar, se pierden y el usuario tendrá que volver a pedirlos.
- **Colisiones de `UrlNormalized`.** Dos URLs distintas pueden producir la misma clave al eliminar separadores; el usuario verá el enlace existente y podrá comparar sus URLs originales. También pueden colisionar palabras que solo se diferencian por una marca diacrítica, que se elimina en todos los alfabetos (por ejemplo, las kanas `か` y `が`).
- **Los servidores de imágenes externas ven a los visitantes.** Al mostrar `Image` directamente desde su origen, ese servidor recibe la IP y el navegador de cada visitante de la página pública. `Referrer-Policy` le oculta la URL de la página, pero no la visita. Servir las imágenes desde Linkubator queda fuera del MVP0.
- **La validación de `Image` no garantiza que el destino sea público.** En el MVP0 se comprueba que el host tenga formato de dominio y se excluyen `localhost` y las direcciones IP, pero no se valida su resolución DNS ni el destino de las redirecciones. Un dominio podría resolver o redirigir a una dirección privada y hacer que el navegador de un visitante intente acceder a su red local; se acepta este riesgo en el MVP0 (ver specifications.md → «Reglas adicionales para `Image`»).

## Pendientes

### Pendientes de scraping
Diseño detallado del scraping y de su ejecución en segundo plano. No es bloqueante hasta que se aborde la etapa de scraping.

1. Timeout exacto del scraper.
2. Redirecciones permitidas durante el scraping.
3. Tamaño máximo de respuesta del scraper.
4. Tipos de contenido aceptados por el scraper.
5. Relación entre el intento lanzado al abandonar el campo URL, cuando el enlace aún no está guardado, y el enlace persistido (`ScrapingAttempts`, `NextScrapingAt`), incluido qué ocurre si el usuario guarda antes de que termine.
6. Si los enlaces creados antes de la etapa de scraping (`ScrapingStatus = null`) se procesarán después.
7. Contra qué URL se resuelve una `og:image` relativa cuando la página ha redirigido: la URL original del enlace o la final de la que se obtuvo el documento.
8. Cómo se recupera un enlace que se queda en `Processing` si la aplicación se detiene durante un intento.
9. Alcance de la protección SSRF más allá de las redirecciones: que la dirección comprobada sea la misma a la que se conecta (resolución DNS) y el tratamiento de las direcciones IPv6 y de las IPv4 mapeadas en IPv6.
10. Qué ocurre si el enlace se borra o el usuario edita sus metadatos mientras hay un intento en curso.

### Pendientes de páginas públicas

Textos de las páginas públicas (specifications.md → «Páginas públicas»). No bloquean el modelo ni los casos de uso; se definen antes de la etapa de área pública.

1. `meta description` de la página de usuario.
2. Texto exacto del `h1` y `meta title` de la página de colecciones (incluye el alias) y su `meta description`.
3. `meta description` de respaldo de la página de colección cuando la colección no tiene descripción.
4. `h1`, `meta title` y `meta description` de la landing.

## Fuera de alcance del MVP0

- **Consulta online de contraseñas comprometidas.** NIST SP 800-63B-4 exige comparar la contraseña con una lista de contraseñas prohibidas, y en el MVP0 se cumple con la lista local. Como complemento futuro se sugiere *Pwned Passwords*, de Have I Been Pwned (https://haveibeenpwned.com/API/v3#PwnedPasswords), mediante su endpoint de k-anonimato (`https://api.pwnedpasswords.com/range/{5 primeros caracteres del hash SHA-1}`), gratuito y sin enviar la contraseña en claro. Tiene sentido cuando haya exposición pública real.
- Envío de correo mediante un proveedor real.
- Redirecciones `301` de alias y de slug de colección.
- Reserva temporal de los alias liberados.
- Limpieza de los registros sin completar.
- «Recordarme» persistente sin fecha de fin, mediante un token de larga duración separado de la sesión y revocable.
- Importación de enlaces (por ejemplo, desde los marcadores del navegador).
- Exportación de enlaces.
- Extensión de navegador.
- Colaboración entre usuarios.
- Sincronización con servicios externos.
- Analítica avanzada.
- Plataforma externa de observabilidad (Grafana, Kibana o similares).
- SEO avanzado: Open Graph, Twitter Cards, `robots.txt`, `sitemap.xml`, datos estructurados, política de indexación de las páginas interiores de la paginación (`rel="prev"`, `rel="next"`, `noindex`) y auditoría de títulos, descripciones, canonical y rastreabilidad.
- Content-Security-Policy completa; en el MVP0 solo se usa `frame-ancestors`.
- Proxy o caché de imágenes en el servidor, para servir `Image` desde Linkubator y que los servidores externos no vean a los visitantes.
- Límite de peticiones por IP en el login, el registro y la recuperación de contraseña. En el MVP0 local solo se limitan los hashes simultáneos (ver architecture.md → «Capas»).
- Límites de colecciones, etiquetas y enlaces por usuario, y la paginación de los listados de colecciones y etiquetas.
- Estrategia de despliegue.
