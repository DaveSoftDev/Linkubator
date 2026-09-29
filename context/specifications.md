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
- Si el alias ya lo usa otra cuenta, completar el registro o cambiar el alias se rechaza indicándolo. Solo las cuentas con el registro completado tienen alias.

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
- El contador empieza en 0 y se reinicia al acertar, al aplicarse el bloqueo (de modo que, pasados los 10 minutos, se vuelve a disponer de 5 intentos) y al restablecer la contraseña con un token válido, que además anula el bloqueo.
- Mientras dura el bloqueo, no se verifica la contraseña ni se cuentan intentos, y la respuesta es la misma que con credenciales incorrectas.
- Si el bloqueo se produce en una operación con sesión abierta (cambio de contraseña, cambio de email o eliminación de cuenta), la sesión no se cierra: solo se rechaza la operación.

### Respuestas que no revelan si una cuenta existe

- Los mensajes de error del login son genéricos y no revelan si el email existe ni si su registro está sin completar.
- Tampoco lo revela el tiempo de respuesta: el login calcula el hash con los mismos parámetros aunque el email no exista o el registro esté sin completar y no haya contraseña guardada.
- El registro, la recuperación de contraseña y la petición de cambio de email dan siempre la misma respuesta, exista o no una cuenta con ese email y esté o no completada. Ninguna de las tres recibe una contraseña sin verificar, así que no hay hash que igualar.

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

- Los enlaces de completar registro, restablecimiento de contraseña y cambio de email usan un token aleatorio de 256 bits, de un solo uso.
- Solo se guarda el hash SHA-256 del token; el token en claro solo viaja en el enlace del correo.
- Al emitir un token, se invalidan los tokens anteriores sin usar del mismo usuario y propósito.
- Al cambiar o restablecer la contraseña, se invalidan también los tokens de restablecimiento y de cambio de email pendientes.
- Al confirmar un cambio de email, se invalidan también los tokens de restablecimiento de contraseña pendientes.
- Los tokens nunca se registran en logs.

| Propósito | Caducidad |
|---|---|
| Completar registro | 24 horas |
| Restablecimiento de contraseña | 15 minutos |
| Cambio de email | 2 horas |

### Correos que envía la aplicación

| Correo | Destinatario | Cuándo | Límite por minuto |
|---|---|---|---|
| Completar registro | Email del registro | Al registrarse con un email sin cuenta o con un registro sin completar, y al pedir la recuperación de un registro sin completar | Sí |
| Aviso de cuenta existente, con un enlace a la página de recuperación (sin token) | Email del registro o nuevo email solicitado | Al registrarse con un email que ya tiene una cuenta completada, o al pedir un cambio de email hacia uno que ya tiene cuenta | Sí |
| Enlace de restablecimiento de contraseña | Email de la cuenta | Al pedir la recuperación de una cuenta completada | Sí |
| Verificación del nuevo email | Nuevo email | Al pedir el cambio de email | Sí |
| Aviso de email cambiado | Email anterior | Al confirmar el cambio de email | No |
| Aviso de contraseña cambiada | Email de la cuenta | Al cambiar o restablecer la contraseña | No |
| Confirmación de cuenta eliminada | Email de la cuenta | Al eliminar la cuenta | No |

- Se envía como máximo 1 correo por minuto y cuenta en los correos con límite. Las peticiones que superen ese límite reciben la misma respuesta genérica sin enviar nada, y tampoco emiten ningún token: el token anterior, si lo hay, sigue vigente y el enlace que el usuario ya tiene en su buzón sigue funcionando.
- El aviso de cuenta existente se limita por la cuenta que ya tiene ese email, no por la que registra o pide el cambio.
- El momento del último envío con límite se guarda en `User.LastEmailSentAt`.
- Los avisos de seguridad (los correos sin límite) se envían siempre y no consultan ni actualizan `User.LastEmailSentAt`.
- Cómo y cuándo se envían los correos está en architecture.md → Correo.

## Cuenta

### Registro

- El usuario introduce solo su email.
- La respuesta es siempre la misma, exista o no una cuenta con ese email y esté o no completada.
- Si no existe ninguna cuenta con ese email, se crea un registro sin completar, que solo contiene el email, y se envía el correo de completar registro.
- Si existe un registro sin completar con ese email, se emite un token nuevo y se vuelve a enviar el correo de completar registro. Volver a registrarse es la forma de pedir el reenvío.
- Si existe una cuenta completada con ese email, no se crea nada y se envía a esa dirección el aviso de cuenta existente.
- Un registro sin completar no tiene nombre, alias, contraseña ni colecciones, y no puede iniciar sesión (invariantes de domain-model.md → User).

### Completar el registro

- El enlace del correo abre un formulario con nombre, alias y contraseña, mientras el token sea válido.
- El alias se muestra con su vista previa (ver «Generación de alias y slugs») y la contraseña sigue las reglas de «Contraseñas».
- Si el alias está ocupado o algún dato no es válido, se indica junto al campo y el token sigue siendo válido hasta que se complete o caduque.
- Al completarse, en una sola operación: se fijan el nombre, el alias, la contraseña y `EmailConfirmedAt`, se crea la colección privada «Bandeja de entrada» y se consume el token (ver architecture.md → Persistencia).
- Después el usuario inicia sesión con normalidad.

### Inicio de sesión

- El login pide email y contraseña, y ofrece la opción «Recordarme» (ver «Sesión»).
- Un registro sin completar responde igual que un email inexistente y no acumula intentos fallidos (ver «Respuestas que no revelan si una cuenta existe»).

### Recuperación de contraseña

- El usuario introduce su email y recibe siempre la misma respuesta genérica, exista o no la cuenta.
- Si existe una cuenta completada, se le envía un enlace para introducir la nueva contraseña. Si existe un registro sin completar, se le envía el correo de completar registro.
- Al completarse: se invalida el token, se cierran todas las sesiones abiertas, se reinicia el bloqueo por intentos fallidos y se envía el aviso de contraseña cambiada.

### Cambio de email

- Requiere reintroducir la contraseña actual (invariantes de domain-model.md → User).
- La respuesta al pedir el cambio es siempre la misma, esté libre u ocupado el nuevo email: se indica que, si la dirección está disponible, se ha enviado un enlace de verificación.
- Si el nuevo email está libre, se emite el token de cambio de email y se envía la verificación del nuevo email.
- Si el nuevo email ya lo tiene otra cuenta, no se emite ningún token y se envía a esa dirección el aviso de cuenta existente.
- Al abrir el enlace de confirmación, si entretanto otra cuenta ha ocupado el nuevo email, la confirmación falla con la misma respuesta que un token no válido o caducado.
- El nuevo email solo sustituye al anterior tras verificarlo; entonces se invalidan los tokens de restablecimiento de contraseña pendientes, se cierran las demás sesiones (la sesión desde la que se confirma, si la hay, sigue abierta; ver «Sesión») y se envía el aviso de email cambiado a la dirección anterior.

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

- Una sesión caduca a los 30 días del login, con independencia de la actividad y de si se marcó «Recordarme». La actividad no prolonga la sesión; solo un nuevo login inicia un plazo nuevo.
- «Recordarme» solo decide si la sesión sobrevive al cierre del navegador: si no se marca, termina al cerrarlo; si se marca, se mantiene hasta caducar. Está sujeto al límite de implementación de decisions.md → Producto y alcance; si pasa a MVP1, todas las sesiones terminarán al cerrar el navegador.
- Cuando la sesión se vuelve a emitir (al cambiar la contraseña o al confirmar un cambio de email), conserva la fecha de caducidad y el carácter (con o sin «Recordarme») de la sesión original.
- Al caducar la sesión, la siguiente petición a `/app` lleva al login.
- Cambiar la contraseña y confirmar un cambio de email mantienen la sesión actual, si la hay, y cierran todas las demás. Restablecer la contraseña y eliminar la cuenta cierran todas.
- La zona privada muestra en todas sus páginas un botón para cerrar sesión.
- Solo se usan cookies técnicas (autenticación y antiforgery); no hay banner de cookies (motivo en decisions.md → Cuenta y seguridad).

## URLs de los enlaces

### Ajuste y validación

Antes de crear un enlace, la URL introducida pasa por un ajuste y una validación. Si falla cualquier paso, la operación se rechaza. `Image` sigue estas mismas reglas con las diferencias de «Reglas adicionales para `Image`».

Ajuste, en este orden:

1. Recortar los espacios del principio y del final.
2. Rechazar si queda vacía o si contiene caracteres de control o saltos de línea.
3. Rechazar si empieza por `//` (URL relativa al protocolo).
4. Si empieza por un esquema explícito seguido de `://`, solo se aceptan `http` y `https`, sin distinguir mayúsculas. Cualquier otro esquema (`ftp`, `file`…) se rechaza.
5. Si empieza por un prefijo del tipo `texto:` que no va seguido de `//` ni de un número de puerto, se rechaza. Así se descartan `javascript:`, `data:`, `mailto:`, `vbscript:` y similares.
6. Si no tiene esquema, se añade: `http://` cuando el host es `localhost` y `https://` en cualquier otro caso.
7. Rechazar si el resultado supera 2048 caracteres. La longitud se mide sobre la URL ya ajustada, con el esquema incluido, que es lo que se guarda.

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
| `//example.com` | Rechazada: relativa al protocolo (solo en `UrlOriginal`; ver «Reglas adicionales para `Image`») |
| `https://intranet` | Rechazada: sin dominio |
| `intranet` | Rechazada: sin dominio |
| `https://ana:clave@example.com` | Rechazada: contiene credenciales |

### Reglas adicionales para `Image`

`Image` se muestra como imagen en las páginas públicas y el navegador de cada visitante la descarga automáticamente. Por eso, tanto si la introduce el usuario como si la obtiene el scraper, pasa por el ajuste y la validación anteriores con estas diferencias:

Ajuste:

- Si empieza por `//` (URL relativa al protocolo), no se rechaza: se le antepone `https:`.
- Si no tiene esquema, se añade siempre `https://`, también cuando el host es `localhost`.

Validación:

- El esquema debe ser `https`. Un `http://` explícito no supera la validación.
- El host debe ser un dominio según el punto 3 de la validación general. No se aceptan `localhost` ni direcciones IP.
- Esta comprobación valida el formato del host; no comprueba a qué dirección IP resuelve ni el destino de las redirecciones. El riesgo de que el navegador de un visitante intente acceder a una dirección privada se acepta en el MVP0 (ver decisions.md → Riesgos aceptados).

Si la introduce el usuario y no supera las reglas, la operación se rechaza. Si la obtiene el scraper y no las supera, se descarta y el enlace se guarda sin imagen.

Cuando la obtiene el scraper de `og:image`:

- Si empieza por `//`, se le antepone `https:` sin resolverla contra la página, sea cual sea el esquema de la página.
- Si es una referencia relativa de cualquier otro tipo (por ejemplo `/img/a.jpg` o `img/a.jpg`), se resuelve contra la URL de la página y después se aplican las reglas. Si la página se sirvió por `http`, el resultado es `http` y se descarta.

| Entrada | Resultado |
|---|---|
| `//cdn.example.com/img/img-01.jpg` | `https://cdn.example.com/img/img-01.jpg` |
| `cdn.example.com/img.jpg`, introducida por el usuario | `https://cdn.example.com/img.jpg` |
| `https://cdn.example.com/img.jpg` | Aceptada tal cual |
| `http://cdn.example.com/img.jpg` | Rechazada o descartada: solo `https` |
| `localhost:5000/img.jpg` | Rechazada o descartada: host no público |
| `https://192.168.1.1/img.jpg` | Rechazada o descartada: host no público |
| `/img/a.jpg`, obtenida por el scraper en `https://example.com/post` | `https://example.com/img/a.jpg` |
| `/img/a.jpg`, obtenida por el scraper en `http://example.com/post` | Descartada: solo `https` |

### Normalización para duplicados

`Link.UrlNormalized` se genera en dos fases. Primero se trabaja sobre la estructura de la URL; después, sobre el texto resultante.

Fase 1, sobre la URL analizada con `System.Uri`:

1. Partir de la URL ya ajustada y validada.
2. Descartar el fragmento.
3. Tratar el puerto: eliminarlo si es `80` en HTTP o `443` en HTTPS, que son los puertos por defecto; conservar cualquier otro puerto.
4. Descartar el esquema `http` o `https`, ya que ambos se consideran equivalentes.
5. Tomar el host tal y como lo entrega `System.Uri`.
6. Decodificar la ruta una sola vez con la decodificación de URI (`Uri.UnescapeDataString`), en la que `+` es un carácter literal.
7. Separar los parámetros de la consulta y decodificar una sola vez el nombre y el valor de cada uno con la decodificación de formularios, en la que `+` equivale a un espacio.
8. Eliminar los parámetros de marketing conocidos (lista siguiente).
9. Convertir a minúsculas el nombre y el valor de los parámetros restantes.
10. Ordenar los parámetros restantes por nombre y valor mediante comparación ordinal (por código de carácter, sin reglas de idioma).
11. Conservar los parámetros repetidos.
12. Unir en una sola cadena el host, el puerto si se conserva, la ruta y los parámetros ordenados.

Fase 2, sobre la cadena resultante, sin volver a decodificar nada:

13. Convertir todo a minúsculas.
14. Eliminar los acentos conservando la letra base.
15. Sustituir `ç` por `c`.
16. Sustituir `ñ` por `n`.
17. Convertir los espacios en guiones medios.
18. Conservar únicamente letras, números y guiones medios.
19. Eliminar cualquier otro carácter, incluidos `.`, `:`, `/`, `?`, `&` y `=`.
20. Eliminar guiones medios duplicados.
21. Eliminar guiones medios al principio y al final.

Cada componente se decodifica exactamente una vez, con la regla que le corresponde. Como el fragmento y los parámetros se separan sobre la URL codificada y después nada se reinterpreta, un `%23` o un `%26` codificados nunca cortan la URL ni separan parámetros: se convierten en texto y desaparecen al aplanar. Un texto codificado dos veces (`%2520`) se decodifica una sola vez y conserva su `%20` como texto, que también desaparece al aplanar.

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

```text
http://example.com:8080/x
https://example.com:8080/x
-> examplecom8080x
```

```text
http://example.com:80/x
https://example.com:443/x
https://example.com/x
-> examplecomx
```

```text
https://example.com/c++/tutorial?q=c++
-> examplecomctutorialqc
```

En la ruta, `+` es literal y se elimina al aplanar; en la consulta, `+` es un espacio y se convierte en guión. Como el guión final se elimina, la clave resulta igual, pero `?q=c++ 2` produce `qc-2` y `/c++2` produce `c2`.

```text
https://example.com/a%2520b
-> examplecoma20b
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

Correspondencia de metadatos, tomando el primero que exista de cada lista:

| Campo | Se toma de, en este orden |
|---|---|
| `Title` | `<meta name="title">` → `<title>` → `<meta property="og:title">` |
| `Description` | `<meta name="description">` → `<meta property="og:description">` |
| `Image` | `<meta property="og:image">`, validada según «Reglas adicionales para `Image`» |

Hasta la etapa de scraping:

- Crear un enlace no lanza ningún scraping: el usuario rellena `Title`, `Description` e `Image` o los deja vacíos.
- El enlace queda con `ScrapingStatus = null` (scraping no solicitado), `ScrapingAttempts = 0` y `NextScrapingAt = null`.

Flujo provisional, cuando exista el scraping (los pendientes están en decisions.md → Pendientes):

- Al crear un enlace, `ScrapingStatus` pasa a `Pending`.
- El primer intento se lanza cuando el usuario abandona el campo de la URL que acaba de introducir.
- Cada intento pone `ScrapingStatus` en `Processing` y suma una unidad a `ScrapingAttempts`.
- Cada intento solo rellena los campos vacíos; nunca sobrescribe valores ya informados, ya sea por un intento anterior o por edición manual del usuario.
- El resultado de un intento decide el estado:
  - Respuesta `200` con HTML: se extraen los metadatos que haya y el enlace queda `Completed`, tenga los tres campos, alguno o ninguno. No se reintenta.
  - Timeout, error de red o respuesta `5xx`: se considera transitorio. Se programa un nuevo intento 5 minutos después (`NextScrapingAt`), hasta 3 intentos en total. Si se agotan, el enlace queda `Failed`.
  - Respuesta `4xx`, contenido que no es HTML, respuesta que supera el tamaño máximo o destino bloqueado por la protección SSRF: se considera definitivo. El enlace queda `Failed` sin más intentos.
- El enlace se conserva siempre, con los metadatos que se hayan podido obtener.

## Búsqueda y filtros

- La búsqueda y los filtros solo actúan sobre los enlaces del usuario identificado.
- La búsqueda textual cubre la URL original, el título, la descripción, el nombre de la colección y los nombres de las etiquetas.
- No distingue acentos: «canción» encuentra «cancion».
- Se puede filtrar por una colección y/o una etiqueta; los filtros se combinan con el texto de búsqueda y se aplican todos a la vez.
- El orden y la paginación están en «Listados».

## Listados

| Listado | Orden | Paginado |
|---|---|---|
| Resultados de búsqueda | Relevancia (solo si hay texto), `Link.CreatedAt` descendente y `Link.Id` descendente como desempates | Sí |
| Enlaces (privados y de una colección pública) | `Link.CreatedAt` descendente y `Link.Id` descendente como desempate | Sí |
| Colecciones del usuario (privado) | `Slug` ascendente | No |
| Etiquetas del usuario (privado) | `Slug` ascendente | No |
| Colecciones que se muestran en público (página de colecciones) | `Slug` ascendente | No |
| 5 últimas colecciones que se muestran en público (página de usuario) | `CreatedAt` descendente y `Id` descendente como desempate | No |
| 5 últimos enlaces públicos de colecciones que se muestran en público (página de usuario) | `CreatedAt` descendente y `Id` descendente como desempate | No |

- Los listados paginados muestran 15 elementos por página. Los demás muestran todos sus elementos.
- Una página fuera de rango responde `404`.

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
- Todas se renderizan en servidor, con encabezados semánticos, un único `h1`, `meta title`, `meta description` y URL canonical. El `meta title` se forma con el mismo texto que el `h1`, seguido del sufijo ` | Linkubator`; el `h1` no lleva ese sufijo (motivo en decisions.md → Producto y alcance). Los textos concretos que faltan están en decisions.md → Pendientes → Páginas públicas.

### Paginación de las páginas públicas

- Solo se pagina la página de colección (ver «Listados»).
- La página se indica con el parámetro de consulta `pagina`: `/{alias}/colecciones/{slug}?pagina=2`.
- La primera página es la URL sin parámetro. `?pagina=1` se sirve con el mismo contenido y su canonical apunta a la URL sin parámetro.
- Las páginas 2 y siguientes tienen como canonical su propia URL con el parámetro.
- Un valor de `pagina` que no sea un entero mayor que 0, o que esté fuera de rango, responde `404`.
- `rel="prev"`, `rel="next"` y `noindex` en las páginas interiores quedan fuera del MVP0 (ver decisions.md → Fuera de alcance del MVP0).

### Landing

- Explica qué es Linkubator y da acceso al registro y al login.

### Página de usuario

- Muestra las 5 últimas colecciones que se muestran en público (domain-model.md → Público y privado) con su nombre, su descripción y un enlace a cada una. Si tiene menos de 5, se muestran las que tenga. Si el usuario no tiene ninguna colección que se muestre en público, la página responde `404` (ver decisions.md → Producto y alcance).
- Muestra los 5 últimos enlaces públicos de esas colecciones, ordenados por `CreatedAt` descendente. Si tiene menos de 5, se muestran los que tenga; siempre hay al menos uno, porque una colección que se muestra tiene al menos un enlace público. Cada enlace muestra su título (o la URL si no tiene), su descripción y su imagen si las tiene, y enlaza a `UrlOriginal` con `rel="nofollow ugc noopener noreferrer"`. Un enlace privado o un enlace de una colección privada nunca aparece aquí.
- El `h1` es el alias y el `meta title` es el alias seguido de ` | Linkubator`. La `meta description` está pendiente de definir. No muestra el nombre ni el email del usuario.
- Responde `404` si el alias no existe (incluido un alias antiguo tras un cambio), sin revelar si tiene colecciones privadas.

### Página de colecciones

- Muestra todas las colecciones del usuario que se muestran en público (domain-model.md → Público y privado) con su nombre, su descripción y un enlace a cada una.
- El `h1` y el `meta title` incluyen el alias; el `meta title` añade ` | Linkubator`. Sus textos exactos y la `meta description` están pendientes de definir. No muestra ningún dato privado del usuario.
- Responde `404` si el alias no existe (incluido un alias antiguo tras un cambio) o si el usuario no tiene ninguna colección que se muestre en público, sin revelar si tiene colecciones privadas.

### Página de colección

- El `h1` es el nombre de la colección y el `meta title` es ese nombre seguido de ` | Linkubator`. La `meta description` es su descripción. El texto de respaldo cuando la colección no tiene descripción está pendiente de definir.
- Muestra solo los enlaces públicos. Cada uno muestra su título (o la URL si no tiene), su descripción y su imagen si las tiene, y enlaza a `UrlOriginal` con `rel="nofollow ugc noopener noreferrer"`.
- Si la colección es pública pero no tiene enlaces públicos, no se muestra en público y responde `404` (ver decisions.md → Producto y alcance).
- Una colección privada, una inexistente y un slug antiguo tras renombrarla responden igual: `404`.

## Errores y recursos no disponibles

- Todas las páginas, públicas y privadas, declaran `lang="es"`.
- Un error de validación vuelve a mostrar el mismo formulario, con los datos introducidos (salvo las contraseñas) y el mensaje junto a cada campo afectado (ver requirements.md → Accesibilidad).
- Una operación rechazada por una regla del dominio (por ejemplo, borrar una colección con enlaces) vuelve a mostrar la página con un mensaje que explica el motivo.
- En `/app`, un recurso inexistente y un recurso de otro usuario responden con el mismo código HTTP `404` y la misma respuesta genérica. La respuesta no indica si el recurso existe ni si pertenece a otra cuenta.
- Las respuestas de las páginas públicas están en «Páginas públicas».

## Registro de eventos (logs)

- Se registran desde el inicio: operaciones relevantes, errores de persistencia, fallos y reintentos del scraper, validaciones, bloqueos por SSRF, cambios entre público y privado y eventos de autenticación.
- Eventos de autenticación que se registran siempre: logins fallidos, bloqueos, restablecimientos de contraseña, cambios de email y eliminaciones de cuenta. Se identifican por `UserId`, sin email en claro.
- Nunca se registran secretos, contraseñas, tokens, hashes innecesarios, credenciales contenidas en URLs ni el texto de las búsquedas.

## Estados del scraping

`ScrapingStatus` de `Link` toma uno de estos valores:

- `null`: no se ha solicitado scraping, como en los enlaces creados antes de la etapa de scraping.
- `Pending`: el scraping está solicitado y aún no se ha ejecutado ningún intento.
- `Processing`: hay un intento en curso.
- `Completed`: se descargó la página y se extrajeron los metadatos que tenía, fueran los que fueran.
- `Failed`: no se pudo descargar la página, por un error definitivo o por agotar los reintentos de un error transitorio.
