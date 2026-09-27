# Especificaciones de Linkubator

Este documento contiene las reglas exactas que la implementación debe cumplir: algoritmos, límites, valores, mensajes y comportamientos. Es la única fuente de estos datos; el resto de documentos los enlazan sin repetirlos.

## Textos introducidos por el usuario

- Todo texto que introduce el usuario, salvo la contraseña, se recorta de espacios al principio y al final antes de validarlo.
- Si queda vacío, se trata como no informado: se rechaza en los campos obligatorios y se guarda como `null` en los opcionales.
- La contraseña se usa tal cual, sin recortar.
- Lo que introduce el usuario por encima de su longitud máxima se rechaza.
- Lo que obtiene el scraper se trunca a su longitud máxima, salvo `Image`, que se descarta si no supera la validación de URLs.

## Longitudes máximas

| Propiedad | Máximo |
|---|---|
| `User.Email` | 254 caracteres |
| `User.Name` | 100 caracteres |
| `User.Alias` | 25 caracteres (mínimo 10) |
| Contraseña en claro | 128 caracteres (mínimo 15) |
| `Collection.Name` | 50 caracteres |
| `Collection.Description` | 250 caracteres |
| `Collection.Slug` y `Tag.Slug` | 50 caracteres (mínimo 1) |
| `Tag.Name` | 50 caracteres |
| `Link.UrlOriginal` e `Image` | 2048 caracteres |
| `Link.Title` | 300 caracteres |
| `Link.Description` | 1000 caracteres |

## Generación de alias y slugs

`User.Alias`, `Collection.Slug` y `Tag.Slug` se generan mediante la misma transformación:

1. Convertir el texto a minúsculas.
2. Eliminar las marcas diacríticas de cualquier letra, conservando la letra base: `Generación` se convierte en `generacion` y `pingüino` en `pinguino`.
3. Sustituir `ç` por `c` y `ñ` por `n`: `caça` se convierte en `caca` y `España` en `espana`.
4. Sustituir los espacios por guiones medios.
5. Conservar únicamente letras ASCII (`a`–`z`), números (`0`–`9`) y guiones medios. Las letras de otros alfabetos, los emojis y los símbolos se eliminan.
6. Eliminar cualquier otro carácter.
7. Eliminar guiones medios duplicados.
8. Eliminar guiones medios al principio y al final.

Ejemplo:

```text
Saltó la raña al charço!!! I luego, croo
-> salto-la-rana-al-charco-i-luego-croo
```

Reglas comunes:

- Si el resultado queda vacío, la operación se rechaza.
- Si el resultado queda fuera de su rango de longitud (alias: 10 a 25 caracteres; slugs: 1 a 50), la operación se rechaza. Nunca se trunca.
- Si el resultado colisiona con otro existente (alias en todo el sistema; slug dentro del usuario), al crear o al modificar, la operación se rechaza. No se añaden sufijos ni prefijos para resolver colisiones.

Alias:

- El usuario escribe el alias y, antes de guardarlo, la aplicación le muestra el resultado de la transformación; por ejemplo, «Ana López García» se guardará como `ana-lopez-garcia`.
- Palabras reservadas, comparadas por coincidencia exacta con el alias ya transformado: `linkubator`, `administrator` y `administrador`.
- Si el alias ya lo usa otra cuenta (verificada, o sin verificar con su token vigente), el registro o el cambio de alias se rechaza indicándolo. La respuesta no dice nada sobre el email.

Slugs:

- El slug de colección y el de etiqueta no se muestran al usuario.
- Se regeneran automáticamente cuando cambia el nombre.
- La colección por defecto «Bandeja de entrada» tiene el slug `bandeja-de-entrada`.

## Email

- Se guarda y se compara sin espacios al principio ni al final y en minúsculas: `Ana@Example.com` y `ana@example.com` son el mismo email.
- No se alteran los puntos ni el signo `+` de la parte local.
- Solo se valida su sintaxis mínima: una única `@`, parte local y dominio no vacíos y un dominio con al menos un punto. Si no la cumple, se rechaza.

## Contraseñas

Reglas de la guía NIST SP 800-63B-4 (sección 3.1.1.2) para contraseñas usadas como único factor de autenticación:

- Longitud entre 15 y 128 caracteres. Cada code point Unicode cuenta como un carácter.
- Se acepta cualquier carácter Unicode, incluido el espacio. La contraseña se normaliza a NFC antes de contar su longitud, compararla y hashearla.
- No se exigen reglas de composición (mayúsculas, números, símbolos…), ni cambios periódicos, ni pistas, ni preguntas de seguridad.
- La contraseña se verifica completa; nunca se trunca.
- Los campos de contraseña permiten pegar y el autocompletado de los gestores de contraseñas.
- Los campos de contraseña ofrecen un botón accesible para mostrar u ocultar el texto (sujeto al límite de implementación de decisions.md → Producto y alcance).
- Al establecer o cambiar una contraseña, se compara completa, tras normalizarla a NFC y sin distinguir mayúsculas, con la lista de contraseñas prohibidas y con estas palabras de contexto: `linkubator`, el alias, la parte local del email y el nombre del usuario. Si coincide, se rechaza indicando el motivo y orientando para elegir otra.

### Lista inicial de contraseñas prohibidas

Patrones comunes (secuencias, teclado, repeticiones y palabras típicas) de al menos 15 caracteres, que es la longitud mínima. Al implementar podrá ampliarse con una lista pública filtrada a 15 o más caracteres.

```text
123456789012345
1234567890123456
12345678901234567890
000000000000000
111111111111111
123123123123123
qwertyuiopasdfg
qwertyuiopasdfgh
qwertyuiopasdfghjkl
asdfghjklqwertyuiop
1q2w3e4r5t6y7u8i9o0p
qazwsxedcrfvtgbyhn
zaq12wsxcde34rfv
abcdefghijklmnop
abcdefghijklmnopqrstuvwxyz
passwordpassword
password1234567
password123456789
iloveyouiloveyou
administrator123
contraseña12345
contrasena12345
contraseñasegura
micontraseñasegura
tequieromucho123
holamundo123456
linkubator12345
linkubatorlinkubator
```

### Bloqueo por intentos fallidos

- Tras 5 intentos fallidos consecutivos de verificar la contraseña de una cuenta (login, cambio de contraseña, cambio de email o eliminación de cuenta), la cuenta queda bloqueada durante 10 minutos.
- El contador se reinicia al acertar y también al aplicarse el bloqueo, de modo que, pasados los 10 minutos, se vuelve a disponer de 5 intentos.
- Mientras dura el bloqueo, no se verifica la contraseña ni se cuentan intentos, y la respuesta es la misma que con credenciales incorrectas.
- Si el bloqueo se produce en una operación con sesión abierta (cambio de contraseña, cambio de email o eliminación de cuenta), la sesión no se cierra: solo se rechaza la operación.

### Respuestas que no revelan si una cuenta existe

- Los mensajes de error del login son genéricos y no revelan si el email existe.
- Tampoco lo revela el tipo de respuesta: si el email no existe, se calcula igualmente un hash con los mismos parámetros antes de responder.
- Solo cuando la contraseña es correcta y el email no está verificado se indica que hay que verificarlo, con la opción de reenviar el correo.
- El registro y la recuperación de contraseña dan siempre la misma respuesta, exista o no una cuenta con ese email. La única excepción es el alias ocupado (ver «Generación de alias y slugs»), que no dice nada sobre el email.

### Hash de la contraseña

Argon2id con la configuración mínima de OWASP (Password Storage Cheat Sheet):

- Memoria: 19 MiB (19456 KiB). Iteraciones: 2. Paralelismo: 1.
- Sal aleatoria de 16 bytes por contraseña, generada con un generador criptográficamente seguro.
- Hash de 32 bytes.
- `User.Password` se guarda en formato PHC, con el algoritmo y los parámetros incluidos: `$argon2id$v=19$m=19456,t=2,p=1$<sal>$<hash>`.
- La comparación del hash calculado con el guardado se hace en tiempo constante.
- Si los parámetros cambian en el futuro, el hash se recalcula con los nuevos en el siguiente login correcto, y los hashes antiguos se siguen verificando con los parámetros que llevan dentro.

## Correo y tokens

### Tokens

- Los enlaces de verificación de email, restablecimiento de contraseña y cambio de email usan un token aleatorio de 256 bits, de un solo uso.
- Solo se guarda el hash SHA-256 del token; el token en claro solo viaja en el enlace del correo.
- Al emitir un token, se invalidan los tokens anteriores sin usar del mismo usuario y propósito.
- Al cambiar o restablecer la contraseña, se invalidan también los tokens de restablecimiento y de cambio de email pendientes.
- Los tokens nunca se registran en logs.

| Propósito | Caducidad |
|---|---|
| Verificación de email | 24 horas |
| Restablecimiento de contraseña | 15 minutos |
| Cambio de email | 2 horas |

### Correos que envía la aplicación

| Correo | Destinatario | Cuándo | Límite por minuto |
|---|---|---|---|
| Verificación de email | Email del registro | Al registrarse y al pedir el reenvío | Sí |
| Aviso de cuenta existente, con un enlace a la página de recuperación (sin token) | Email del registro | Al registrarse con un email que ya tiene cuenta | Sí |
| Enlace de restablecimiento de contraseña | Email de la cuenta | Al pedir la recuperación | Sí |
| Verificación del nuevo email | Nuevo email | Al pedir el cambio de email | Sí |
| Aviso de email cambiado | Email anterior | Al confirmar el cambio de email | No |
| Aviso de contraseña cambiada | Email de la cuenta | Al cambiar o restablecer la contraseña | No |
| Confirmación de cuenta eliminada | Email de la cuenta | Al eliminar la cuenta | No |

- Se envía como máximo 1 correo por minuto y cuenta en los correos con límite. Las peticiones que superen ese límite reciben la misma respuesta genérica sin enviar nada.
- El momento del último envío con límite se guarda en `User.LastEmailSentAt`.
- Los avisos de seguridad (los correos sin límite) se envían siempre y no consultan ni actualizan `User.LastEmailSentAt`.
- Los correos se envían después de confirmar la transacción y fuera de la petición (ver architecture.md → Correo).

## Cuenta

### Registro

- El usuario introduce nombre, email, alias y contraseña.
- Se crea el usuario sin verificar, junto con su colección privada «Bandeja de entrada», y se envía el correo de verificación.
- No se puede iniciar sesión hasta verificar el email (invariante de domain-model.md → User).
- La respuesta es siempre la misma, exista o no ya una cuenta con ese email.
- Si ya existe una cuenta verificada con ese email, o una sin verificar con su token todavía vigente, no se crea nada y se envía a esa dirección el aviso de cuenta existente.
- Si existe una cuenta sin verificar con el mismo email, y su token de verificación ha caducado, el nuevo registro la sustituye y borra físicamente sus datos.

### Verificación e inicio de sesión

- El email se verifica abriendo el enlace del correo, mientras el token sea válido.
- El usuario puede pedir el reenvío del correo de verificación desde el login, cuando introduce la contraseña correcta de una cuenta sin verificar.
- El login pide email y contraseña, y ofrece la opción «Recordarme» (ver «Sesión»).

### Recuperación de contraseña

- El usuario introduce su email y recibe siempre la misma respuesta genérica, exista o no la cuenta.
- Si existe, se le envía un enlace para introducir la nueva contraseña.
- Al completarse: se invalida el token, se cierran todas las sesiones abiertas, se reinicia el bloqueo por intentos fallidos y se envía el aviso de contraseña cambiada.
- Si la cuenta no estaba verificada, completar el restablecimiento también verifica el email (invariante de domain-model.md → User).

### Cambio de email

- Requiere reintroducir la contraseña actual, y el nuevo email debe estar libre al pedir el cambio y al confirmarlo (invariantes de domain-model.md → User).
- El nuevo email solo sustituye al anterior tras verificarlo; entonces se envía el aviso de email cambiado a la dirección anterior.

### Cambio de contraseña

- Se hace desde la configuración, mediante un botón que lleva a otra página con tres campos enmascarados: contraseña actual, nueva y confirmación de la nueva.
- Al completarse, la sesión actual sigue abierta, se cierran las demás y se envía el aviso de contraseña cambiada.

### Configuración de usuario

- El usuario puede editar su nombre, su email (ver «Cambio de email»), su alias y su contraseña (ver «Cambio de contraseña»).

### Eliminación de cuenta

- Se hace desde la configuración, reintroduciendo la contraseña.
- Borra físicamente el usuario y todos sus datos: colecciones, enlaces, etiquetas, sus relaciones, su índice de búsqueda y sus tokens.
- Después se cierra la sesión y se envía la confirmación de cuenta eliminada.

## Sesión

- Una sesión dura como máximo 30 días desde el login.
- «Recordarme»: si no se marca, la sesión termina al cerrar el navegador; si se marca, se mantiene hasta ese máximo de 30 días. Está sujeto al límite de implementación de decisions.md → Producto y alcance; si pasa a MVP1, todas las sesiones terminarán al cerrar el navegador.
- Cambiar la contraseña mantiene la sesión actual y cierra todas las demás. Restablecerla y eliminar la cuenta cierran todas.
- La zona privada muestra en todas sus páginas un botón para cerrar sesión.
- Solo se usan cookies técnicas (autenticación y antiforgery), exentas de consentimiento; no es necesario tener un banner de cookies.

## URLs de los enlaces

### Ajuste y validación

Antes de crear un enlace, la URL introducida pasa por un ajuste y una validación. Si falla cualquier paso, la operación se rechaza. Las mismas reglas se aplican a `Image` cuando el usuario la edita manualmente y a la imagen que obtenga el scraper: si `og:image` es una ruta relativa, se resuelve contra la URL de la página; si después no supera la validación, se descarta.

Ajuste, en este orden:

1. Recortar los espacios del principio y del final.
2. Rechazar si queda vacía, si supera 2048 caracteres o si contiene caracteres de control o saltos de línea.
3. Rechazar si empieza por `//` (URL relativa al protocolo).
4. Si empieza por un esquema explícito seguido de `://`, solo se aceptan `http` y `https`, sin distinguir mayúsculas. Cualquier otro esquema (`ftp`, `file`…) se rechaza.
5. Si empieza por un prefijo del tipo `texto:` que no va seguido de `//` ni de un número de puerto, se rechaza. Así se descartan `javascript:`, `data:`, `mailto:`, `vbscript:` y similares.
6. Si no tiene esquema, se añade: `http://` cuando el host es `localhost` y `https://` en cualquier otro caso.

Validación, sobre el resultado del ajuste:

1. Debe ser una URI absoluta válida para `System.Uri`.
2. El esquema debe ser `http` o `https`.
3. El host debe ser `localhost`, una dirección IPv4, una dirección IPv6 entre corchetes o un dominio con al menos un punto y un dominio de primer nivel de 2 o más letras (o en formato `xn--`). Se aceptan dominios internacionalizados.
4. No puede contener credenciales (`usuario:clave@`).
5. El puerto, si existe, debe ser válido.

`Link.UrlOriginal` guarda el resultado del ajuste, sin ninguna otra transformación. Como tras el ajuste el esquema siempre existe, el tratamiento de puertos de la normalización es determinista.

Aceptar un enlace a `localhost` o a una IP no implica que el scraper pueda descargarlo (ver architecture.md → Scraping).

| Entrada | Resultado |
|---|---|
| `example.com/articulo` | `https://example.com/articulo` |
| `https://Example.com/a` con espacios alrededor | `https://Example.com/a` |
| `localhost:5000/admin` | `http://localhost:5000/admin` |
| `example.com:8080/x` | `https://example.com:8080/x` |
| `http://192.168.1.1` | Aceptada tal cual |
| `http://[2001:db8::1]/` | Aceptada tal cual |
| `javascript:alert(1)` | Rechazada: esquema no permitido |
| `mailto:ana@example.com` | Rechazada: esquema no permitido |
| `ftp://example.com` | Rechazada: esquema no permitido |
| `//example.com` | Rechazada: relativa al protocolo |
| `https://intranet` | Rechazada: sin dominio |
| `intranet` | Rechazada: sin dominio |
| `https://ana:clave@example.com` | Rechazada: contiene credenciales |

### Normalización para duplicados

`Link.UrlNormalized` se genera en dos fases. Primero se trabaja sobre la estructura de la URL; después, sobre el texto resultante.

Fase 1, sobre la URL analizada con `System.Uri`:

1. Partir de la URL ya ajustada y validada.
2. Descartar el fragmento.
3. Tratar el puerto: eliminarlo si es `80` u `8080` en HTTP, o `443` en HTTPS; conservar cualquier otro puerto.
4. Descartar el esquema `http` o `https`, ya que ambos se consideran equivalentes.
5. Separar los parámetros de la consulta, con el nombre y el valor de cada uno ya decodificados.
6. Eliminar los parámetros de marketing conocidos (lista siguiente).
7. Convertir a minúsculas el nombre y el valor de los parámetros restantes.
8. Ordenar los parámetros restantes por nombre y valor mediante comparación ordinal (por código de carácter, sin reglas de idioma).
9. Conservar los parámetros repetidos.
10. Unir en una sola cadena el host, el puerto si se conserva, la ruta y los parámetros ordenados.

Fase 2, sobre la cadena resultante:

11. Aplicar `UrlDecode` para obtener el valor real de secuencias como `%20` o `+`.
12. Convertir todo a minúsculas.
13. Eliminar los acentos conservando la letra base.
14. Sustituir `ç` por `c`.
15. Sustituir `ñ` por `n`.
16. Convertir los espacios en guiones medios.
17. Conservar únicamente letras, números y guiones medios.
18. Eliminar cualquier otro carácter, incluidos `.`, `:`, `/`, `?`, `&` y `=`.
19. Eliminar guiones medios duplicados.
20. Eliminar guiones medios al principio y al final.

Como el fragmento y los parámetros se identifican antes de decodificar, un `%23` o un `%26` codificados nunca cortan la URL ni separan parámetros: se decodifican en la fase 2 y desaparecen al aplanar.

Parámetros de marketing eliminados, sin distinguir mayúsculas: `utm_source`, `utm_medium`, `utm_campaign`, `utm_term`, `utm_content`, `gclid`, `wbraid`, `gbraid`, `fbclid`, `msclkid`, `ttclid`, `epik`, `gad_source`, `gad_campaignid`, `srsltid`, `gcs`, `gcd`, `at_medium`, `at_campaign`, `at_platform`, `at_creation` y `at_term`.

Ejemplos:

```text
HTTPS://Example.COM/Generación de datos/?utm_source=google&id=10
-> examplecomgeneracion-de-datosid10
```

```text
http://example.com:4587/Artículo número 2?b=2&a=1
-> examplecom4587articulo-numero-2a1b2
```

También se aceptan colisiones como esta:

```text
example.com:4587/articulo-numero-2/?a=1&b=2
example.com:4587/articulo-numero-2/a/?$=1&b=2
-> examplecom4587articulo-numero-2a1b2
```

### Duplicados

- Si ya existe un enlace del mismo usuario con la misma `UrlNormalized`, la creación se rechaza indicando en qué colección está el enlace existente.
- No se siguen redirecciones para detectar duplicados.
- La URL original de cada enlace sigue disponible para ver las diferencias entre enlaces que hayan producido la misma clave.

## Metadatos y scraping

Correspondencia de metadatos:

- `Title`: el meta title de la página o, si no lo tiene, su etiqueta HTML `title`.
- `Description`: el meta description.
- `Image`: la meta property `og:image`, validada según «Ajuste y validación».

Hasta la etapa de scraping:

- Crear un enlace no lanza ningún scraping: el usuario rellena `Title`, `Description` e `Image` o los deja vacíos.
- El enlace queda con `ScrapingStatus = null` (scraping no solicitado), `ScrapingAttempts = 0` y `NextScrapingAt = null`.

Flujo provisional, cuando exista el scraping (los pendientes están en decisions.md → Pendientes):

- Cuando el scraping esté implementado, al crear un enlace, se actualizará `ScrapingStatus` a **Pendiente**.
- Se ejecutarán 3 intentos como máximo por enlace, con un intervalo de 5 minutos.
- Cada intento añade una unidad a `ScrapingAttempts`.
- El primer intento se lanza cuando el usuario abandona el campo de la URL que acaba de introducir.
- En cada intento se actualiza `ScrapingStatus` a **Procesando**.
- Si tras un intento queda algún campo vacío, se programa un segundo intento silencioso 5 minutos después y, si siguen quedando campos vacíos, un tercer y último intento 5 minutos después del segundo.
- Cada intento solo rellena los campos vacíos; nunca sobrescribe valores ya informados, ya sea por un intento anterior o por edición manual del usuario.
- El scraping se considera completado cuando `Title`, `Description` e `Image` están todos informados, y entonces no se reintenta. Entonces se actualiza `ScrapingStatus` a **Completado**.
- El enlace se conserva aunque el scraping termine sin metadatos.
- Si al lanzar el proceso de scraping éste falla, se actualizará `ScrapingStatus` a **Fallido**.
- Si se agotan los 3 intentos sin obtener todos los metadatos (y sin que el proceso fallara), se actualizará `ScrapingStatus` a **ReintentosCompletados**. El enlace se conserva con los metadatos que se hayan podido obtener.

## Búsqueda y filtros

- La búsqueda y los filtros solo actúan sobre los enlaces del usuario identificado.
- La búsqueda textual cubre la URL original, el título, la descripción, el nombre de la colección y los nombres de las etiquetas.
- No distingue acentos: «canción» encuentra «cancion».
- Se puede filtrar por una colección y/o una etiqueta; los filtros se combinan con el texto de búsqueda y se aplican todos a la vez.
- Orden de resultados:
  1. Relevancia, del resultado más relevante al menos relevante. Solo aplica cuando hay texto de búsqueda.
  2. `CreatedAt` del enlace, descendente.
  3. `Id` del enlace, descendente.
- Se muestran 15 resultados por página.

## Listados

| Listado | Orden |
|---|---|
| Enlaces (privados y de una colección pública) | `Link.CreatedAt` descendente y `Link.Id` descendente como desempate |
| Colecciones del usuario (privado) | `Slug` ascendente |
| Etiquetas del usuario (privado) | `Slug` ascendente |
| Colecciones de la página pública de usuario | `Slug` ascendente |

- Todos los listados paginados y los resultados de búsqueda muestran 15 elementos por página.
- Una página fuera de rango responde `404`.
- `Link.CreatedAt` solo se usa para ordenar. `Collection.CreatedAt` no se usa para ordenar.

## Páginas públicas

### Rutas

| Página | Ruta |
|---|---|
| Landing | `/` |
| Página de usuario | `/{alias}/` |
| Página de colecciones | `/{alias}/colecciones/` |
| Página de colección | `/{alias}/colecciones/{slug}` |

- Solo estas páginas son indexables. Un enlace nunca tiene página propia.
- Barra final: una ruta que tiene páginas por debajo termina en `/`, y una ruta final no. Por eso `/{alias}/` termina en `/` (debajo tiene la página de colecciones y las páginas de cada colección) y `/{alias}/colecciones/{slug}` no (un enlace nunca tiene página propia).
- Si una página es accesible con varias URLs equivalentes (con o sin barra final, con mayúsculas), se sirven todas sin redirecciones y la etiqueta canonical apunta a la forma canónica en minúsculas de la tabla.
- Todas se renderizan en servidor, con encabezados semánticos, un único `h1`, `meta title`, `meta description` y URL canonical.

### Landing

- Explica qué es Linkubator y da acceso al registro y al login.

### Página de usuario

- Muestra las 5 últimas colecciones públicas del usuario con su nombre, su descripción y un enlace a cada una. Si tiene menos de 5, se muestran las que sean.
- Muestra los 5 últimos enlaces públicos del usuario, de entre los enlaces de sus colecciones públicas, ordenados por `CreatedAt` descendente. Si tiene menos de 5, se muestran los que sean. Cada enlace muestra su título (o la URL si no tiene), su descripción y su imagen si las tiene, y enlaza a `UrlOriginal` con `rel="nofollow ugc noopener noreferrer"`. Un enlace privado o un enlace de una colección privada nunca aparece aquí.
- El `h1` y el `meta title` son el alias. No muestra el nombre ni el email del usuario.
- Responde `404` si el alias no existe (incluido un alias antiguo tras un cambio) o si el usuario no tiene colecciones públicas, sin revelar si tiene colecciones privadas.

### Página de colecciones

- Muestra todas las colecciones públicas del usuario con su nombre, su descripción y un enlace a cada una.
- El `h1` y el `meta title` son un texto fijo. No muestra el nombre ni el email del usuario.
- Responde `404` si el alias no existe (incluido un alias antiguo tras un cambio) o si el usuario no tiene colecciones públicas, sin revelar si tiene colecciones privadas.

### Página de colección

- El `h1` y el `meta title` son el nombre de la colección, y la `meta description` es su descripción.
- Muestra solo los enlaces públicos. Cada uno muestra su título (o la URL si no tiene), su descripción y su imagen si las tiene, y enlaza a `UrlOriginal` con `rel="nofollow ugc noopener noreferrer"`.
- Si la colección pública no tiene enlaces públicos, responde `404` ya que los buscadores la podrían catalogar como de bajo contenido o soft 404, así que mejor responder directamente con un `404`.
- Una colección privada, una inexistente y un slug antiguo tras renombrarla responden igual: `404`.

## Errores y recursos no disponibles

- Un error de validación vuelve a mostrar el mismo formulario, con los datos introducidos (salvo las contraseñas) y el mensaje junto a cada campo afectado (ver requirements.md → Accesibilidad).
- Una operación rechazada por una regla del dominio (por ejemplo, borrar una colección con enlaces) vuelve a mostrar la página con un mensaje que explica el motivo.
- En `/app`, un recurso inexistente y un recurso de otro usuario responden igual: `404`.
- Las respuestas de las páginas públicas están en «Páginas públicas».

## Registro de eventos (logs)

- Se registran desde el inicio: operaciones relevantes, errores de persistencia, fallos y reintentos del scraper, validaciones, bloqueos por SSRF, cambios entre público y privado y eventos de autenticación.
- Eventos de autenticación que se registran siempre: logins fallidos, bloqueos, restablecimientos de contraseña, cambios de email y eliminaciones de cuenta. Se identifican por `UserId`, sin email en claro.
- Nunca se registran secretos, contraseñas, tokens, hashes innecesarios, credenciales contenidas en URLs ni el texto de las búsquedas.
