# Roadmap del MVP0 de Linkubator

Este documento indica en qué orden se construye el MVP0 y cuándo se da por terminada cada etapa. No repite reglas: cada objetivo remite a los documentos donde se especifica.

## Hecho

Todavía no hay ninguna etapa cerrada.

## Now

### Documentación (en revisión)

Objetivo: tener una definición coherente del MVP0 antes de implementar.

Entregables:

- requirements.md, domain-model.md, specifications.md, architecture.md y decisions.md.
- Este roadmap.

Criterio de finalización:

- No quedan reglas ambiguas que afecten al modelo de datos ni a los casos de uso principales.
- Cada regla vive en un único documento y los demás la enlazan.
- La revisión completa está cerrada sin contradicciones entre documentos.
- La documentación del MVP0 está aceptada. Hasta entonces no empieza la implementación (ver AGENTS.md).

Los pendientes de decisions.md → «Pendientes» (scraping y páginas públicas) no bloquean esta etapa: se resuelven en sus etapas respectivas.

## Next

### Dominio y aplicación

Objetivos:

- Implementar las entidades y sus invariantes (domain-model.md).
- Implementar la generación común de alias y slugs y la normalización de URLs (specifications.md).
- Definir los casos de uso, los contratos `Result`, el contexto de usuario autenticado y las interfaces de repositorios y de Unit of Work (architecture.md → «Capas»).

Criterio de finalización:

- Los casos de uso principales tienen contratos y criterios de aceptación claros.

### Persistencia

Objetivos:

- Preparar SQLite, incorporar Dapper y crear los scripts SQL versionados.
- Añadir las restricciones únicas y los índices.
- Implementar el Unit of Work, las transacciones, `LinkTag` y el índice FTS5 (architecture.md → «Persistencia» y «FTS5»).

Criterio de finalización:

- Las operaciones de enlaces, colecciones y etiquetas son atómicas.
- La búsqueda respeta siempre el usuario identificado.
- FTS5 se mantiene sincronizado con altas, cambios y borrados.
- Las transacciones son breves y no contienen operaciones externas.
- WAL, `busy_timeout` y claves foráneas están configurados.

### Interfaz privada - Gestión básica

Objetivos:

- Cuenta completa: registro con solo el email y compleción desde el enlace del correo, login con «Recordarme» y bloqueo, cierre de sesión, recuperación de contraseña, configuración de usuario y eliminación de cuenta (specifications.md → «Cuenta» y «Sesión»).
- Envío de correo en segundo plano con MailKit y smtp4dev (architecture.md → «Correo»).
- Seguridad web: HTTPS, cookie, `ReturnUrl`, antiforgery y cabeceras (architecture.md → «Autenticación» y «Seguridad web»).
- Gestión de colecciones, enlaces y etiquetas, incluida la creación de etiquetas desde el formulario del enlace. Los enlaces se crean sin scraping.
- Búsqueda y filtros (specifications.md → «Búsqueda y filtros»).
- Estados vacíos y errores accesibles (requirements.md → «Accesibilidad»).

Criterio de finalización:

- El usuario autenticado puede gestionar colecciones, enlaces y etiquetas, y buscar y filtrar sus enlaces.
- Los formularios de creación muestran la casilla de público desmarcada.
- Todas las páginas privadas y de cuenta cuelgan de `/app` y las privadas exigen sesión.
- Ningún registro sin completar puede iniciar sesión.

## Later

### Scraping del MVP0

Objetivos:

- Resolver los pendientes de decisions.md → «Pendientes de scraping».
- Implementar el flujo de specifications.md → «Metadatos y scraping».
- Implementar las medidas obligatorias de architecture.md → «Scraping», incluida la protección SSRF.

Criterio de finalización:

- El enlace se conserva aunque no haya metadatos.
- Los reintentos no bloquean las operaciones de base de datos.
- El flujo provisional queda revisado y documentado.

### Área pública y fundamentos SEO

Objetivos:

- Definir los textos de decisions.md → «Pendientes de páginas públicas».
- Crear la landing, la página de usuario (con las últimas colecciones y los últimos enlaces que se muestran en público), la página de colecciones y la página de colección según specifications.md → «Páginas públicas».

Criterio de finalización:

- No quedan textos pendientes en las páginas públicas.
- Las colecciones y los enlaces privados nunca aparecen en las páginas públicas.
- El HTML público se renderiza en servidor y es legible por los buscadores.

## Checklist de calidad del MVP0

Cada punto remite a la regla que se comprueba.

Dominio y datos:

- Tests de dominio de todas las invariantes de domain-model.md.
- Tests de propiedad entre usuarios, incluida la igualdad de `UserId` entre enlace, colección y etiquetas (domain-model.md → «Propiedad de los datos»).
- Tests de las reglas de público y privado y de sus transiciones (domain-model.md → «Público y privado»).
- Tests de que siempre queda al menos una colección y de que la «Bandeja de entrada» se crea en la misma transacción que completa el registro.
- Tests de transacciones y de condiciones de carrera con `BEGIN IMMEDIATE` (architecture.md → «Persistencia»): hacer privada una colección mientras se hace público uno de sus enlaces, mover un enlace a una colección ajena con un `CollectionId` manipulado (rechazado por la aplicación y por la clave foránea compuesta) y asociar una etiqueta ajena.

Especificaciones:

- Tests de recorte de textos y de longitudes máximas: rechazo de lo introducido y truncado de lo obtenido por el scraper.
- Tests de generación de alias y slugs: transformación, rangos, palabras reservadas, colisiones, regeneración al renombrar y alias ocupado.
- Tests de normalización y validación mínima del email.
- Tests de contraseñas: longitud, NFC, lista de prohibidas y palabras de contexto.
- Tests del bloqueo: activación, reinicio del contador y respuesta idéntica durante el bloqueo, incluso con la contraseña correcta, y que el login bloqueado calcula el hash y descarta el resultado.
- Tests de respuestas que no revelan cuentas, incluido el hash calculado en el login cuando el email no existe, el registro está sin completar o la cuenta está bloqueada.
- Tests del hash: sal distinta por contraseña, formato PHC y verificación de hashes con parámetros anteriores.
- Tests de tokens: caducidad, un solo uso, almacenamiento solo como hash e invalidación de los anteriores, incluida la de los tokens de restablecimiento al confirmar un cambio de email.
- Tests de los límites de correo, de que al superarlos no se emite ningún token, de que el del aviso de cuenta existente no consume el de los correos con token y de que los avisos de seguridad se envían siempre.
- Tests de registro (email nuevo, registro sin completar que reenvía el correo, cuenta completada que recibe el aviso), de completar el registro (alias ocupado, contraseña rechazada, token que sigue válido hasta completarlo), recuperación (incluida la de un registro sin completar), cambio de email (con contraseña, doble comprobación, respuesta idéntica con email ocupado, aviso de cuenta existente al titular y fallo genérico al confirmar si el email se ocupó entretanto), cambio de contraseña y eliminación de cuenta.
- Tests de sesión: caducidad absoluta a pesar de la actividad, «Recordarme» en ambos modos, conservación de la caducidad al volver a emitir la sesión y efectos del `SecurityStamp` al cambiar o restablecer la contraseña y al confirmar un cambio de email, con la sesión del dueño del token, con la de otra cuenta y sin sesión. El GET del enlace de cambio de email no modifica datos.
- Tests de ajuste y validación de URLs con todos los ejemplos de specifications.md, incluidas las reglas adicionales de `Image` (solo `https`, hosts públicos, `//` convertido a `https:`) y `og:image` con rutas relativas en páginas `http` y `https`.
- Tests de `UrlNormalized` con todos los ejemplos de specifications.md, con caracteres estructurales codificados (`%23`, `%26`), texto codificado dos veces (`%2520`), `+` en la ruta y en la consulta, y de detección de duplicados.
- Tests de etiquetas creadas desde el formulario del enlace, incluida la asociación de una existente.
- Tests de búsqueda: usuario identificado, sin distinguir acentos, filtros combinados y resultado más relevante primero.
- Tests de listados: orden, tamaño de página, `404` fuera de rango y, en la página de colección, el parámetro `pagina`, la canonical de `?pagina=1` hacia la URL sin parámetro y la canonical propia de las páginas interiores.
- Tests de páginas públicas: contenido, `404` de alias, slugs antiguos, usuario sin colecciones que se muestren en público y colección sin enlaces públicos, ausencia de colecciones públicas vacías en la página de usuario y en la de colecciones, `rel` de los enlaces externos, canonical y barra final, últimas colecciones y últimos enlaces que se muestran en público, ausencia de enlaces privados y de colecciones privadas.
- Tests de errores: formularios con errores de validación y `404` idéntico para recursos inexistentes y de otro usuario en `/app`.
- Test del comando de reconstrucción del índice FTS5.
- Tests del flujo de scraping: `200` con y sin metadatos, error transitorio con reintentos hasta `Failed`, error definitivo sin reintentos, y la protección SSRF.

Seguridad web:

- Tests de autorización de la zona `/app` y de `ReturnUrl`.
- Tests de redirección a HTTPS, cabeceras de seguridad, antiforgery, rechazo de un `Host` ajeno y enlaces de correo construidos con el origen configurado aunque la petición lleve otro `Host`.
- Tests de los atributos de la cookie de autenticación (`__Host-`, `Path`, `HttpOnly`, `Secure`, `SameSite`) y de que el ticket no contiene datos personales en claro (architecture.md → «Autenticación»).

Accesibilidad y operación:

- Tests de accesibilidad según requirements.md → «Accesibilidad»: teclado, orden de foco y errores anunciados; este test está pensado para que lo haga una persona.
- Pruebas en navegadores actuales; también para que una persona pruebe en varios navegadores actuales.
- Logs disponibles por consola y en archivo rotativo (architecture.md → «Logs»).
- Tests de registro de eventos: se registran los eventos que exige specifications.md → «Registro de eventos», los de autenticación se identifican por `UserId` sin email en claro, y los logs no contienen los datos que esa sección prohíbe registrar.

## Backlog de MVP1 (borrador, no comprometido)

Candidatos sin fecha ni compromiso de entrega. La justificación de cada uno está en decisions.md → «Fuera de alcance del MVP0».

- Redirecciones `301` de alias y de slug de colección.
- Reserva temporal de los alias liberados.
- Consulta online de contraseñas comprometidas (Pwned Passwords).
- Proveedor real de correo.
- «Mostrar contraseña» y «Recordarme», si superan su límite de implementación en el MVP0 (ver decisions.md → «Producto y alcance»).
- «Recordarme» persistente sin fecha de fin, con un token de larga duración separado de la sesión.
- Limpieza de los registros sin completar.
- SEO avanzado.
- Content-Security-Policy completa.
- Proxy o caché de imágenes en el servidor.
