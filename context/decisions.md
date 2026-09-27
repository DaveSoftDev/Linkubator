# Decisiones de Linkubator

Este documento recoge qué se ha decidido y por qué, qué riesgos se aceptan, qué queda pendiente y qué queda fuera del MVP0. No repite las reglas: cada decisión enlaza al documento donde se especifica.

## Confirmadas

### Producto y alcance

- **Linkubator gestiona enlaces, colecciones y etiquetas, preparado para varios usuarios.** En la documentación funcional se denomina «etiqueta»; `Tag` y `LinkTag` se reservan para los nombres técnicos.
- **El MVP0 se ejecuta únicamente en local.** El despliegue y el envío de correo real quedan para más adelante.
- **La interfaz está en castellano.** Por eso la colección por defecto se llama «Bandeja de entrada» y el segmento de las rutas públicas es `colecciones`.
- **Toda la zona privada y de cuenta cuelga de `/app`.** Así la raíz queda libre para las páginas públicas con alias, sin colisiones (ver architecture.md → Enrutamiento).
- **La raíz `/` muestra una landing pública.** Explica qué es Linkubator y da acceso al registro y al login.
- **Las páginas públicas se pueden verificar en local, pero no serán rastreables hasta un despliegue público.**
- **Cada usuario recibe al registrarse una colección privada «Bandeja de entrada».** Así puede guardar sus primeros enlaces sin tener que crear una colección. Es una colección normal: se puede renombrar.
- **Un usuario conserva siempre al menos una colección.** No se puede borrar la última, esté o no vacía, para que siempre haya dónde guardar un enlace nuevo.
- **Solo se borran colecciones y etiquetas vacías, y el borrado es siempre físico.**
- **Hacer algo público es siempre una acción explícita del usuario.** Colecciones y enlaces nacen privados, y mover un enlace o volver a hacer pública una colección nunca publica enlaces por su cuenta (ver domain-model.md → Público y privado).
- **Al cambiar un alias o renombrar una colección, la URL anterior responde `404`.** Las redirecciones `301` quedan fuera del MVP0.
- **La página de un usuario sin colecciones públicas responde `404`.** Así no revela si tiene colecciones privadas.
- **Una colección pública sin enlaces públicos responde `404`.** Así los buscadores desindexarán esa página.
- **Las colecciones se ordenan por Slug** en los listados privados del usuario y en la página pública de colecciones (`/{alias}/colecciones/`) (ver specifications.md → Listados y Páginas públicas).
- **Las colecciones se ordenan por CreatedAt** en la página pública del usuario (`/{alias}/`) donde se muestran las últimas colecciones públicas y los últimos enlaces públicos (ver specifications.md → Listados y Páginas públicas).
- **Las páginas públicas no muestran el nombre ni el email del usuario; su título es el alias.** El alias ya es público; el resto son datos personales.
- **Barra final solo en las rutas que tienen páginas por debajo.** Distingue las rutas contenedoras (`/{alias}/`) de las finales (`/{alias}/colecciones/{slug}`) y hace predecible la forma canónica de cada URL (ver specifications.md → Rutas).
- **El SEO se aborda por fases.** Lo que afecta al dominio, a las rutas y al HTML se define desde el MVP0; el SEO avanzado queda fuera.
- **El scraping forma parte del MVP0, pero se implementa en una fase posterior.** Hasta entonces, el usuario rellena los metadatos a mano (ver specifications.md → Metadatos y scraping).
- **Una etiqueta escrita en el formulario del enlace cuyo nombre ya existe se asocia en lugar de rechazarse.** El usuario no tiene por qué saber si ya la había creado.
- **«Mostrar contraseña» y «Recordarme» tienen un límite de implementación de 1 hora cada uno.** Si superan ese tiempo, pasan al backlog de MVP1.
- **Se respeta `prefers-reduced-motion`.** Algunas animaciones pueden provocar molestias a personas con trastornos vestibulares.

### Cuenta y seguridad

- **Las contraseñas siguen la guía NIST SP 800-63B-4 desde el MVP0** (ver specifications.md → Contraseñas).
  - No se exigen reglas de composición porque los usuarios tienden a cumplirlas de forma predecible (una mayúscula al principio y un número o símbolo al final), lo que apenas mejora la seguridad.
  - El bloqueo tras intentos fallidos es más estricto que el máximo de 100 que fija NIST.
  - El usuario es el único responsable de la fortaleza de la contraseña que elija.
- **Argon2id desde el primer día, con la configuración mínima de OWASP.** Así se evita tener que migrar hashes más adelante. Los parámetros van dentro del hash (formato PHC), de modo que se pueden endurecer sin invalidar los existentes (ver specifications.md → Hash de la contraseña).
- **No se puede iniciar sesión sin verificar el email.**
- **Las respuestas nunca revelan si un email tiene cuenta, ni por su contenido ni por su tipo de respuesta.** Por eso el login calcula un hash aunque el email no exista y los correos se envían en segundo plano.
- **El alias ocupado sí se indica en el registro.** El alias es público, así que decirlo no revela nada privado, y la respuesta no dice nada sobre el email.
- **Una cuenta sin verificar con el token caducado se puede sustituir por un nuevo registro con el mismo email o alias.** Así nadie puede retener un email o un alias ajeno.
- **Restablecer la contraseña también verifica el email.** Usar el enlace demuestra la titularidad de la dirección.
- **Cambiar el email exige la contraseña actual.** Con una sesión robada bastaría cambiar el email y pedir la recuperación para quedarse con la cuenta.
- **El nuevo email se comprueba al pedir el cambio y otra vez al confirmarlo.** Otra cuenta puede haberlo registrado entretanto.
- **Cambiar o restablecer la contraseña invalida también los cambios de email pendientes.** Quien restablece su contraseña porque sospecha de un acceso ajeno no debe dejar vivo un cambio de email iniciado por otro.
- **En `/app`, un recurso inexistente y uno de otro usuario responden igual (`404`).** Así no se revela que existen datos de otros usuarios (ver specifications.md → Errores y recursos no disponibles).
- **El límite de correos por minuto solo se aplica a los correos que se pueden pedir; los avisos de seguridad se envían siempre.** Si se limitaran, un restablecimiento hecho justo después de pedirlo se quedaría sin aviso (ver specifications.md → Correos que envía la aplicación).
- **Los tokens de correo se guardan como hash SHA-256, no con Argon2.** Al ser aleatorios de 256 bits, no necesitan un algoritmo lento.
- **La duración máxima de la sesión es la que fija NIST para el nivel AAL1** (ver specifications.md → Sesión).
- **Autenticación por cookies de ASP.NET Core, sin ASP.NET Core Identity.** Los almacenes por defecto de Identity dependen de Entity Framework Core, que no se usa.
- **Solo cookies técnicas.** Al estar exentas de consentimiento, no hace falta banner de cookies.
- **HTTPS también en local.** Lo exige la cookie `__Host-`, que lleva `Secure`.
- **`ReturnUrl` solo acepta URLs locales.** Evita las redirecciones abiertas.
- **Cabeceras de seguridad mínimas (`nosniff` y `frame-ancestors 'none'`).** Impiden interpretar respuestas con otro tipo de contenido y que la aplicación se incruste en otra página. Una CSP completa queda fuera del MVP0.
- **Los enlaces a URLs de usuarios llevan `rel="nofollow ugc noopener noreferrer"`.** Son URLs de terceros elegidas por los usuarios: no deben transmitir reputación SEO ni acceso a la página de origen.
- **MailKit para el correo y smtp4dev en local.** Microsoft recomienda MailKit frente a `System.Net.Mail.SmtpClient` para desarrollos nuevos. En MVP1 bastará con cambiar la configuración SMTP a un proveedor real.

### Datos, URLs y búsqueda

- **Solo se aceptan URLs `http` y `https`.** Descarta esquemas peligrosos como `javascript:` o `data:`. Se aceptan `localhost` y direcciones IP; que el scraper pueda o no descargarlas es otra cuestión, resuelta por la protección SSRF.
- **La URL que guarda el usuario no se transforma más allá del ajuste, y nunca se sustituye por la de una redirección.** Es la que se usa para navegar.
- **Los duplicados se detectan con una clave plana (`UrlNormalized`), no con una URL reconstruible.** Se aceptan conscientemente colisiones derivadas de eliminar separadores. El fragmento y los parámetros se identifican sobre la URL analizada, antes de decodificar, para que un carácter estructural codificado (`%23`, `%26`) no cambie la estructura de la URL; el resto del texto se decodifica después y se aplana (ver specifications.md → Normalización para duplicados).
- **No se siguen redirecciones para detectar duplicados.**
- **Un duplicado se rechaza indicando en qué colección está el enlace existente.**
- **Lo que introduce el usuario por encima del máximo se rechaza; lo que obtiene el scraper se trunca.** El usuario puede corregir su texto; el de una página externa no.
- **Los textos se recortan de espacios, salvo la contraseña.** En una contraseña, los espacios son caracteres válidos.
- **Del email solo se valida la sintaxis mínima.** La validación real es el correo de verificación; una validación sintáctica estricta rechazaría direcciones válidas sin aportar seguridad.
- **Alias y slugs solo usan ASCII, y un resultado fuera de rango se rechaza en lugar de truncarse.**
- **Palabras reservadas para el alias.** Evitan suplantar al servicio (lista en specifications.md → Generación de alias y slugs).
- **Identificadores `INTEGER PRIMARY KEY`.** Es el `rowid` que usa FTS5 y da sentido al desempate por `Id`.
- **Fechas en UTC.**
- **El índice FTS5 se reconstruye con un comando manual.** En el MVP0 local basta con poder regenerarlo a mano si se desincroniza; una reparación automática añadiría complejidad sin necesidad (ver architecture.md → FTS5).
- **La búsqueda no distingue acentos y no aplica stemming.** (ver architecture.md → FTS5). 

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
- **`Konscious.Security.Cryptography.Argon2` y MailKit son excepciones justificadas a la filosofía de mínimas dependencias.** Son librerías pequeñas y de propósito único, no abstracciones pesadas como EF Core.
- **Clean Architecture, SOLID, DRY y YAGNI, con los patrones Repository, Unit of Work y Result.**

## Riesgos aceptados

- **Bloqueos provocados por terceros.** Quien conozca el email de una cuenta puede provocar bloqueos temporales.
- **Reutilización de alias liberados.** Un alias liberado (por cambio de alias, eliminación de cuenta o sustitución de una cuenta sin verificar) puede ocuparlo otro usuario, que heredaría las URLs públicas que circulaban del anterior. En local, sin buscadores ni enlaces compartidos, el impacto es nulo.
- **Pérdida de correos en cola.** Si la aplicación se detiene con correos pendientes de enviar, se pierden y el usuario tendrá que volver a pedirlos.
- **Colisiones de `UrlNormalized`.** Dos URLs distintas pueden producir la misma clave al eliminar separadores; el usuario verá el enlace existente y podrá comparar sus URLs originales.

## Pendientes

### Scraping
Diseño detallado del scraping y de su ejecución en segundo plano. No es bloqueante hasta que se aborde la etapa de scraping.

1. Timeout exacto del scraper.
2. Redirecciones permitidas durante el scraping.
3. Tamaño máximo de respuesta del scraper.
4. Tipos de contenido aceptados por el scraper.
5. Relación entre el intento lanzado al abandonar el campo URL, cuando el enlace aún no está guardado, y el enlace persistido (`ScrapingAttempts`, `NextScrapingAt`), incluido qué ocurre si el usuario guarda antes de que termine.
6. Si los enlaces creados antes de la etapa de scraping (`ScrapingStatus = null`) se procesarán después.

## Fuera de alcance del MVP0

- **Consulta online de contraseñas comprometidas.** NIST SP 800-63B-4 exige comparar la contraseña con una lista de contraseñas prohibidas, y en el MVP0 se cumple con la lista local. Como complemento futuro se sugiere *Pwned Passwords*, de Have I Been Pwned (https://haveibeenpwned.com/API/v3#PwnedPasswords), mediante su endpoint de k-anonimato (`https://api.pwnedpasswords.com/range/{5 primeros caracteres del hash SHA-1}`), gratuito y sin enviar la contraseña en claro. Tiene sentido cuando haya exposición pública real.
- Envío de correo mediante un proveedor real.
- Redirecciones `301` de alias y de slug de colección.
- Reserva temporal de los alias liberados.
- Importación de enlaces (por ejemplo, desde los marcadores del navegador).
- Exportación de enlaces.
- Extensión de navegador.
- Colaboración entre usuarios.
- Sincronización con servicios externos.
- Analítica avanzada.
- Plataforma externa de observabilidad (Grafana, Kibana o similares).
- SEO avanzado: Open Graph, Twitter Cards, `robots.txt`, `sitemap.xml`, datos estructurados, política definitiva de paginación indexable y auditoría de títulos, descripciones, canonical y rastreabilidad.
- Content-Security-Policy completa; en el MVP0 solo se usa `frame-ancestors`.
- Estrategia de despliegue.
