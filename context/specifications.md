# Especificaciones de Linkubator

Este documento contiene las reglas exactas que la implementación debe cumplir: algoritmos, límites, valores, mensajes y comportamientos. Es la única fuente de estos datos; el resto de documentos los enlazan sin repetirlos.

## Idioma de las páginas

Todas las páginas, públicas y privadas, declaran el idioma del documento mediante el atributo `lang="es"` en el elemento raíz `<html>`.

## Textos introducidos por el usuario

- Todo texto que introduce el usuario, salvo la contraseña, se recorta de espacios al principio y al final antes de validarlo.
- Si queda vacío, se trata como no informado: se rechaza en los campos obligatorios y se guarda como `null` en los opcionales.
- La contraseña se usa tal cual, sin recortar.
- Lo que introduce el usuario por encima de su longitud máxima se rechaza.
- Lo que obtiene el scraper se trunca a su longitud máxima entre puntos de código, sin partir parejas sustitutas UTF-16, salvo `Image`, que se descarta si no supera la validación de URLs o no supera la validación.

## Longitudes máximas

Todos los límites de longitud de este documento se cuentan en puntos de código Unicode: una pareja sustituta UTF-16 cuenta como un carácter. No se cuentan unidades UTF-16 ni grafemas. La longitud se mide en el punto de validación definido para cada campo, después del recorte y de las transformaciones que deban aplicarse antes de esa validación.

| Propiedad | Máximo |
|---|---|
| `User.Email` | 250 caracteres |
| `User.Name` | 100 caracteres |
| `User.Alias` | 50 caracteres (mínimo 10). Se calculan después de la transformación |
| Contraseña en claro | 128 caracteres (mínimo 15) |
| `Collection.Name` | 50 caracteres |
| `Collection.Description` | 250 caracteres |
| `Collection.Slug` | 150 caracteres (mínimo 1) |
| `Tag.Name` | 50 caracteres |
| `Tag.Slug` | 150 caracteres (mínimo 1) |
| `Link.UrlOriginal` | 2048 caracteres |
| `Link.Image` | 2048 caracteres |
| `Link.Title` | 300 caracteres |
| `Link.Description` | 1000 caracteres |
| `q` (texto de búsqueda) | 100 caracteres |

## Cuenta

### Identidad de desarrollo

- Hasta que LoginV0 incorpore autenticación real, cada arranque de la aplicación prepara la identidad de desarrollo en todos los entornos, después de aplicar las migraciones y antes de atender peticiones privadas.
- La identidad de desarrollo comparte el acceso privado entre todas las personas con acceso a la aplicación hasta LoginV0; el riesgo aceptado por DLG consta en [decisions.md → «Identidad de desarrollo»](decisions.md#identidad-de-desarrollo).
- El perfil inicial de desarrollo tiene el email `user@linkubator.test`, el nombre `Demo User` y el alias `my-demo-user`. El perfil mostrado corresponde a los datos actuales de la cuenta.
- Si no existe una cuenta con el email inicial, se crea completa con el perfil inicial, `EmailConfirmedAt` informado y un hash Argon2id inicial de una contraseña aleatoria descartada.
- Si la cuenta con el email inicial está sin completar, se completa con el perfil inicial, se informa `EmailConfirmedAt` y se establece el hash inicial. Si ya está completada, sus datos no se modifican.
- Si la cuenta no tiene colecciones, se crea la colección privada «Bandeja de entrada». Si ya tiene una o más colecciones, se conservan sin cambios, independientemente de su nombre o privacidad.
- La creación o compleción de la cuenta y, cuando corresponda, la creación de su colección inicial se aplican como una única operación atómica.
- Repetir el aprovisionamiento no duplica ni reinicia la cuenta o sus colecciones existentes.
- El marcador `EmailConfirmedAt` de esta identidad es técnico y no demuestra la titularidad real del email.

### Registro

- A partir del email introducido por el usuario:
  - Si no existe ninguna cuenta con ese email, se crea un registro sin completar: aún no tiene perfil completo (`Name`, `Alias` y `Password`); sí contiene el email y el `SecurityStamp`, y puede tener campos operativos como `LastEmailSentAt`. La base de datos asigna `Id` y `CreatedAt`. Se envía el correo de completar registro.
  - Si existe un registro sin completar con ese email, se emite un token nuevo y se vuelve a enviar el correo de completar registro. Volver a registrarse es la forma de pedir el reenvío.
  - Si existe una cuenta completada con ese email, no se crea nada y se envía a esa dirección el correo de aviso de cuenta existente.
- Un registro sin completar no tiene nombre, alias, contraseña ni colecciones, y no puede iniciar sesión (invariantes de [domain-model.md → «User»](domain-model.md#user)).

### Completar el registro

- El enlace del correo abre un formulario con nombre, alias y contraseña, mientras el token sea válido.
- El alias se muestra con su vista previa (ver [«Generación del alias»](#generación-del-alias)) y la contraseña sigue las reglas de [«Contraseñas»](#contraseñas).
- Si el alias está ocupado o algún dato no es válido, se indica junto al campo y el token sigue siendo válido hasta que se complete o caduque.
- Al mostrarse el formulario no se consume el token. El cambio se aplica al pulsar el botón (POST con antiforgery), tras volver a validar el token. Si la validación falla, se muestra la respuesta común de [«Tokens»](#tokens).
- Al completarse, en una sola operación: se fijan el nombre, el alias, la contraseña y `EmailConfirmedAt` con la fecha UTC actual, se crea la colección privada «Bandeja de entrada» para que el usuario ya pueda crear enlaces y asignarlos a colecciones. Además se consume el token (ver [architecture.md → «Persistencia»](architecture.md#persistencia)).
- Después el usuario puede iniciar sesión con normalidad.

> Diagrama de la secuencia: [Registro de usuario](diagrams/account-onboarding-sequence.md).

### Inicio de sesión

- El login pide email y contraseña, y ofrece la opción «Recordarme» (ver [«Sesión»](#sesión)).
- Un registro sin completar responde igual que un email inexistente y no acumula intentos fallidos (ver [«Respuestas que no revelan si una cuenta existe»](#respuestas-que-no-revelan-si-una-cuenta-existe)).
- Cuando un usuario sin sesión iniciada solicita una página privada, se conserva su destino en el parámetro de URL (`ru`) al mostrar el login. Tras iniciar sesión correctamente, `ru` solo se acepta como ruta local absoluta cuya ruta, después de decodificar una sola vez los escapes válidos y normalizar los segmentos `.` y `..`, sea `/app` o comience por `/app/`; la comparación del segmento `app` no distingue mayúsculas. Se conserva la cadena de consulta si existe.
- Se rechaza `ru` si falta, no es una ruta local absoluta, contiene un esquema o autoridad (incluidas las referencias que comienzan por `//`), contiene barras invertidas o caracteres de control, tiene escapes inválidos o su ruta normalizada queda fuera de `/app`. No se vuelve a decodificar el valor. En todos esos casos, tras iniciar sesión correctamente se redirige al dashboard en `/app`.

### Verificación concurrente de contraseña

- Antes de verificar una contraseña presentada, normalizarla a Unicode NFC. No recortarla; usar el valor normalizado para verificar el hash.
- Antes de verificar una contraseña, se leen juntos el hash de `User.Password` y el `User.SecurityStamp` vigente. El cálculo de Argon2id se realiza fuera de cualquier transacción.
- Antes de aplicar el resultado de la verificación, se comprueba que `SecurityStamp` siga siendo el leído junto al hash y que la cuenta no esté bloqueada. Si el sello cambió o hay un bloqueo activo, se rechaza la operación sin aplicar cambios ni contar un intento fallido contra el estado nuevo de la cuenta.
- En el login, antes de emitir la cookie se vuelve a comprobar el sello y el estado de bloqueo; la cookie lleva el sello comprobado. Si la cuenta cambió después de esa comprobación, la validación de sesión por `SecurityStamp` invalida la cookie.
- En operaciones autenticadas que piden la contraseña actual, la comprobación del sello y del bloqueo se hace dentro de la misma transacción que aplicaría la operación. Si la comprobación falla, se revierte y no se ejecuta la operación.

### Recuperación de contraseña

- El usuario introduce su email desde el formulario de «Recuperar contraseña» y recibe siempre la misma respuesta genérica, exista o no la cuenta.
- La cuenta del usuario:
  - Existe y está completada: se le envía un enlace para introducir la nueva contraseña. 
  - Existe y está sin completar: se le envía el correo de completar registro.
- Al mostrarse el formulario no se consume el token. El cambio se aplica al pulsar el botón (POST con antiforgery), tras volver a validar el token. Si la validación falla, se muestra la respuesta común de [«Tokens»](#tokens).
- Al completarse: se consume el token presentado y se aplican las invalidaciones de tokens pendientes definidas en [«Tokens»](#tokens); además, se cierran todas las sesiones abiertas, se reinicia el bloqueo por intentos fallidos y se envía el aviso de contraseña cambiada.

> Diagrama de la secuencia: [Recuperación y restablecimiento de contraseña](diagrams/account-password-reset-sequence.md).

### Cambio de email

- Requiere reintroducir la contraseña actual (invariantes de [domain-model.md → «User»](domain-model.md#user)).
- La solicitud recibe la misma respuesta genérica tanto si el nuevo email está libre como si pertenece a otra cuenta. Si es el email actual de la cuenta, no se emite ningún token y se informa al usuario de que no puede introducir su mismo email.
  - Si el nuevo email está libre, se emite el token de cambio de email y se envía el correo de verificación al nuevo email.
  - Si el nuevo email ya lo tiene otra cuenta, no se emite ningún token y se envía a esa dirección el aviso de cuenta existente.
- Al abrir el enlace de verificación se muestra una página con el nuevo email y un botón para confirmar el cambio; abrir el enlace no modifica nada. El cambio se aplica al pulsar el botón (POST con antiforgery), tras volver a validar el token. Si la validación falla, se muestra la respuesta común de [«Tokens»](#tokens).
- Al confirmar, si entretanto otra cuenta ha ocupado el nuevo email, se muestra la respuesta común de [«Tokens»](#tokens).
- En la confirmación correcta se sustituye `User.Email` y se asigna la fecha UTC actual a `User.EmailConfirmedAt` en la misma operación atómica, conforme a [domain-model.md → «User»](domain-model.md#user). Solicitar el cambio o abrir su enlace no modifica ninguno de esos valores; si la confirmación se rechaza o la operación se revierte, ambos conservan sus valores anteriores.
- El nuevo email solo sustituye al anterior tras verificarlo; entonces se actualiza `SecurityStamp`, se invalidan los tokens pendientes según [«Tokens»](#tokens), se cierran las demás sesiones (la sesión desde la que se confirma sigue abierta solo si pertenece al dueño del token, el `UserId ` del usuario autenticado es el mismo `UserId` del email que se actualiza; si no hay sesión o es de otra cuenta, no se crea ni se modifica ninguna; ver [«Sesión»](#sesión)) y se envía el aviso de email cambiado a la dirección anterior.

> Diagrama de la secuencia: [Cambio de email](diagrams/account-email-change-sequence.md).

### Cambio de contraseña

- Se hace desde la configuración, mediante un botón que lleva a otra página con tres campos enmascarados: contraseña actual, nueva y confirmación de la nueva.
- Al completarse, la sesión actual sigue abierta, se cierran las demás y se envía el aviso de contraseña cambiada.

### Configuración de usuario

- El usuario puede editar su nombre, su email (ver [«Cambio de email»](#cambio-de-email)), su alias y su contraseña (ver [«Cambio de contraseña»](#cambio-de-contraseña)).

### Eliminación de cuenta

- Se solicita desde la configuración y requiere reintroducir la contraseña actual.
- Tras verificar la contraseña, se envía al email actual un enlace con un token de un solo uso para confirmar la eliminación. Hasta que se confirme, la cuenta y todos sus datos permanecen intactos. Si se alcanza el límite de envío de correos con token, no se emite token ni se envía el correo.
- Abrir el enlace mediante GET solo muestra una página de confirmación; no elimina la cuenta ni consume el token. La eliminación solo se ejecuta al confirmar explícitamente mediante POST con antiforgery.
- Al confirmar con un token válido, se borra físicamente el usuario y todos sus datos: colecciones, enlaces, etiquetas, sus relaciones, su índice de búsqueda y sus tokens. Después se cierra la sesión y se envía el aviso de cuenta eliminada al email que tenía la cuenta.

> Diagrama de la secuencia: [Eliminación de cuenta](diagrams/account-deletion-sequence.md).

## Generación de alias y slugs

### Transformación común a ASCII

`User.Alias`, `Collection.Slug` y `Tag.Slug` se generan con la misma transformación. Recibe el texto ya recortado según [«Textos introducidos por el usuario»](#textos-introducidos-por-el-usuario) y no lo recorta. No distingue para qué se usa el resultado: cada uno añade después sus propias validaciones (ver [«Generación del alias»](#generación-del-alias) y [«Generación de slugs»](#generación-de-slugs)).

1. Normalizar el texto a Unicode NFKD. Convierte en su equivalente ASCII las letras y los números de ancho completo (`Ａ` pasa a `A`), los superíndices y subíndices (`x²` pasa a `x2`), `℃` y `℉` (`°C` y `°F`), `™` (`TM`) y los espacios que no son el espacio normal, como el de no separación y el ideográfico (espacio normal). Las fracciones como `½` pasan a `1⁄2`, con la barra de fracción `⁄`, que la [«Tabla de conversión de símbolos»](#tabla-de-conversión-de-símbolos) convierte después en un guion medio: `½` acaba como `1-2`.
2. Rechazar el texto si contiene un carácter de control (como el tabulador o un salto de línea) o un separador distinto del espacio normal. No se eliminan: se rechaza indicando el motivo.
3. Convertir el texto a minúsculas.
4. Eliminar las marcas diacríticas de cualquier letra, conservando la letra base: `Generación` se convierte en `generacion`, `pingüino` en `pinguino`, `caça` en `caca` y `España` en `espana`.
5. Aplicar la [«Tabla de conversión de símbolos»](#tabla-de-conversión-de-símbolos).
6. Sustituir los espacios por guiones medios.
7. Rechazar el texto si todavía contiene alguna letra o algún número que no sea ASCII. No se elimina: se rechaza indicando el motivo.
8. Eliminar cualquier otro carácter que no sea una letra ASCII (`a`–`z`), un número (`0`–`9`) o un guión medio: los emojis, los signos de puntuación y los símbolos que no estén en la tabla.
9. Eliminar guiones medios duplicados: `--` pasa a `-`.
10. Eliminar guiones medios al principio y al final.

> Diagrama del proceso: [Transformación común a ASCII](diagrams/process-ascii-transformation.md).

Ejemplos:

```text
pingüino-2024              -> pinguino-2024
ana--lopez                 -> ana-lopez
Ana_López!                 -> ana-lopez
Ana López García           -> ana-lopez-garcia
Ana😀López                 -> analopez
Ａna１                      -> ana1
Área x²                    -> area-x2
Media ½ hora               -> media-1-2-hora
Temp 20℃                   -> temp-20c
Ana<tabulador>López        -> Rechazado: carácter de control
日本語-ana                  -> Rechazado: letras no ASCII
```

### Generación del alias

El alias es el resultado de la [«Transformación común a ASCII»](#transformación-común-a-ascii), sin cambios.

> Diagrama del proceso: [Generación del alias](diagrams/process-alias.md).

Reglas del alias:

- Antes de guardarlo, la aplicación le muestra al usuario el resultado de la transformación; por ejemplo, si pega «Ana López García» se guardará como `ana-lopez-garcia`.
- Si el resultado queda vacío, o fuera del rango de longitud de «Longitudes máximas», la operación se rechaza. Nunca se trunca.
- Palabras reservadas, comparadas por coincidencia exacta con el alias ya transformado: `linkubator`, `administrator` y `administrador`.
- Si el alias ya lo usa otra cuenta, completar el registro o cambiar el alias se rechaza indicándolo. No se añaden sufijos ni prefijos para resolver la colisión. Solo las cuentas con el registro completado tienen alias.

### Generación de slugs

`Collection.Slug` y `Tag.Slug` se generan a partir del nombre con la [«Transformación común a ASCII»](#transformación-común-a-ascii), la misma que usa el alias. La transformación es idéntica para colecciones y para etiquetas.

> Diagrama del proceso: [Generación de slugs](diagrams/process-slugs.md).

Ejemplo:

```text
Q & A  ->  q-and-a
Saltó la raña al charço!!! I luego, croo  ->  salto-la-rana-al-charco-i-luego-croo
```

Reglas de los slugs:

- El slug de colección y el de etiqueta no se muestran al usuario.
- Se regeneran automáticamente cuando cambia el nombre.
- La equivalencia de nombres para las restricciones de unicidad se determina exclusivamente comparando los slugs generados por esta transformación: dos nombres son equivalentes si y solo si generan el mismo slug. No se aplica una comparación adicional de los nombres originales. Por ejemplo, «Papá» y «papa» generan el mismo slug y colisionan; los símbolos solo causan una colisión si, tras aplicar la transformación, ambos nombres generan exactamente el mismo slug.
- La colección por defecto «Bandeja de entrada» tiene el slug `bandeja-de-entrada`.
- Si el resultado queda vacío, o fuera del rango de longitud de «Longitudes máximas», la operación se rechaza. Nunca se trunca.
- Si el slug generado colisiona con el de otro elemento del mismo tipo y del mismo usuario, al crear o al modificar, la operación se rechaza indicando el nombre del elemento con el que colisiona. Las colecciones se comparan únicamente con otras colecciones y las etiquetas únicamente con otras etiquetas: una colección y una etiqueta del mismo usuario pueden compartir slug. No se añaden sufijos ni prefijos para resolver colisiones.
- Excepción: al crear una etiqueta desde el formulario de un enlace, si su slug coincide con el de una existente, se asocia esa y se indica al usuario cuál se ha asociado.

### Tabla de conversión de símbolos

La tabla se aplica al texto ya normalizado y en minúsculas, por lo que solo contiene minúsculas. Los símbolos se sustituyen por su palabra, separada por guiones medios del resto del texto. Los separadores se sustituyen por un guión medio. Las letras latinas de los bloques Latin-1 y Latin Extended-A que no se reducen a ASCII con la normalización ni al eliminar las marcas diacríticas se sustituyen por su equivalente en ASCII.

| Entrada | Resultado |
|---|---|
| `+` | `plus` |
| `#` | `sharp` |
| `&` | `and` |
| `@` | `at` |
| `%` | `percent` |
| `.` `/` `_` `⁄` | `-` |
| `$` | `dollar` |
| `€` | `euro` |
| `£` | `pound` |
| `ß` | `ss` |
| `æ` | `ae` |
| `œ` | `oe` |
| `ø` | `o` |
| `đ` | `d` |
| `ł` | `l` |
| `ð` | `d` |
| `þ` | `th` |
| `ħ` | `h` |
| `ı` | `i` |
| `ĸ` | `k` |
| `ŋ` | `ng` |
| `ŧ` | `t` |

`µ` y `ŉ` no están en la tabla: la normalización los convierte en letras no ASCII y se rechazan.

| Texto | Resultado |
|---|---|
| `C#` | `c-sharp` |
| `C++` | `c-plus-plus` |
| `Q&A` | `q-and-a` |
| `AT&T` | `at-and-t` |
| `Windows 1.0` | `windows-1-0` |
| `Node.js` | `node-js` |
| `TCP/IP` | `tcp-ip` |
| `Straße` | `strasse` |
| `Papá` | `papa` |
| `Þór` | `thor` |
| `Ŋ` | `ng` |
| `日本語` | Rechazado: letras no ASCII |
| `日本語 Tokyo` | Rechazado: letras no ASCII |

## Email

- Se guarda y se compara sin espacios al principio ni al final y en minúsculas: `Ana@Example.com` y `ana@example.com` son el mismo email.
- No se alteran los puntos ni el signo `+` de la parte local.
- La sintaxis de la dirección se valida contra un subconjunto de la gramática `addr-spec` de [RFC 5322, sección 3.4.1](https://www.rfc-editor.org/rfc/rfc5322#section-3.4.1): `local-part@domain`. El subconjunto admitido es el que define la expresión regular siguiente. Se valida la dirección completa, no una coincidencia parcial. El límite de longitud de la aplicación sigue aplicándose por separado.
- Esta expresión regular anclada define la forma admitida: `dot-atom` o `quoted-string` sin FWS en la parte local y `dot-atom` o `domain-literal` en el dominio:
  ```text
  \A(?:[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*|"(?:[\x21\x23-\x5B\x5D-\x7E]|\\[\x09\x20-\x7E])*")@(?:[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\[[\x21-\x5A\x5E-\x7E]*\])\z
  ```
- Se rechazan las direcciones que usan CFWS de la sección 3.2.2 (comentarios y espacios plegables) o las formas obsoletas de la sección 4, aunque RFC 5322 las considere válidas (motivo en [decisions.md → «Datos, URLs y búsqueda»](decisions.md#datos-urls-y-búsqueda)).
- La normalización a minúsculas es una regla de identidad de la aplicación, no una regla de sintaxis de RFC 5322.

## Contraseñas

Reglas de la guía [**NIST SP 800-63B-4**](https://csrc.nist.gov/pubs/sp/800/63/b/4/final) (sección 3.1.1.2) para contraseñas usadas como único factor de autenticación:

- Longitud entre 15 y 128 caracteres, según la unidad definida en [«Longitudes máximas»](#longitudes-máximas).
- Se acepta cualquier carácter Unicode, incluido el espacio. La contraseña y su confirmación se normalizan por separado a NFC. La contraseña normalizada es la que se cuenta, se compara con la lista de contraseñas prohibidas y se hashea.
- No se exigen reglas de composición (mayúsculas, números, símbolos…), ni cambios periódicos.
- La contraseña se verifica completa; nunca se trunca.
- Los campos de contraseña permiten pegar y el autocompletado de los gestores de contraseñas.
- Los campos de contraseña ofrecen, a su derecha, un botón con icono que muestra u oculta el texto:
  - Con el texto enmascarado, el icono es un párpado cerrado o un ojo tachado y el nombre accesible del botón es «Mostrar contraseña».
  - Con el texto visible, el icono es un ojo abierto y el nombre accesible es «Ocultar contraseña».
- Al establecer o cambiar una contraseña, se compara la contraseña con su confirmación después de normalizar ambas a NFC. La comparación es exacta y sensible a mayúsculas. Si no coinciden, se muestra un mensaje al usuario y no se continúa.
- Al establecer o cambiar una contraseña, se compara completa, tras normalizarla a NFC, con cada entrada de la lista de contraseñas prohibidas mediante comparación ordinal sin distinguir mayúsculas (`OrdinalIgnoreCase`), independiente de la cultura. Si coincide, se rechaza indicando el motivo y orientando para elegir otra.

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
- Mientras dura el bloqueo, no se cuentan intentos y la respuesta es la misma que con credenciales incorrectas, aunque la contraseña sea correcta.
- En el login bloqueado se calcula igualmente el hash con los mismos parámetros y se descarta el resultado (ver [«Respuestas que no revelan si una cuenta existe»](#respuestas-que-no-revelan-si-una-cuenta-existe)). Las operaciones con sesión abierta no lo calculan.
- Si el bloqueo se produce en una operación con sesión abierta (cambio de contraseña, cambio de email o eliminación de cuenta), la sesión no se cierra: solo se rechaza la operación.

### Respuestas que no revelan si una cuenta existe

- Los mensajes de error del login son genéricos y no revelan si el email existe ni si su registro está sin completar.
- Tampoco lo revela el tiempo de respuesta: el login calcula el hash con los mismos parámetros aunque el email no exista, el registro esté sin completar y no haya contraseña guardada, o la cuenta esté bloqueada.
- El registro y la recuperación de contraseña dan siempre la misma respuesta, exista o no una cuenta con ese email y esté o no completada. La petición de cambio de email no distingue entre una dirección libre y una perteneciente a otra cuenta; la excepción para el email actual está en [«Cambio de email»](#cambio-de-email).

### Hash de la contraseña

Argon2id con la configuración mínima de OWASP (Password Storage Cheat Sheet):

- Memoria: 19 MiB (19456 KiB). Iteraciones: 2. Paralelismo: 1.
- Sal aleatoria de 16 bytes por contraseña, generada con un generador criptográficamente seguro.
- Hash de 32 bytes.
- `User.Password` se guarda en formato PHC, con el algoritmo y los parámetros incluidos: `$argon2id$v=19$m=19456,t=2,p=1$<sal>$<hash>`.
- La comparación del hash calculado con el guardado se hace en tiempo constante.
- Como caso excepcional de mantenimiento criptográfico, si los parámetros de hashing cambian en el futuro, en el siguiente login correcto se verifica la contraseña introducida con los parámetros del hash almacenado y se calcula un nuevo hash de esa misma contraseña, con una nueva sal aleatoria y los nuevos parámetros, que sustituye al anterior en `User.Password`. Los hashes antiguos se siguen verificando con los parámetros que llevan dentro hasta su renovación. Esta operación actualiza únicamente la representación criptográfica almacenada: no cambia la contraseña del usuario ni constituye una vía adicional para establecerla o restablecerla durante el ciclo de vida de la cuenta.

## Tokens

- Los enlaces de completar registro, restablecimiento de contraseña, cambio de email y confirmación de eliminación de cuenta usan un token aleatorio de 256 bits, de un solo uso.
- En completar registro, recuperación de contraseña, cambio de email y eliminación de cuenta, tanto al abrir el enlace o la página (GET) como al confirmar la operación (POST), si el token no existe, no es válido, ha caducado, ya se ha usado o se ha invalidado, se muestra «No se ha podido completar la operación. Vuelve a solicitarla.» y no se completa la operación. El GET no consume el token; el POST vuelve a validarlo antes de aplicar el cambio. Si al confirmar un cambio de email otra cuenta ya ocupa el nuevo email, se muestra la misma respuesta.
- Solo se guarda el hash SHA-256 del token; el token en claro solo viaja en el enlace del correo.
- Cada operación solo acepta tokens cuyo `UserId` corresponda a la cuenta afectada y cuyo `Purpose` corresponda exactamente a esa operación. Completar el registro requiere que la cuenta siga sin completar; recuperación de contraseña, cambio de email y eliminación de cuenta requieren una cuenta completada.
- Un token solo tiene efecto una vez: si varias peticiones presentan el mismo token a la vez, solo una lo consigue y las demás reciben la respuesta de «No se ha podido completar la operación. Vuelve a solicitarla.».
- Al completar la operación del token, se informa `UsedAt`. Si otra operación lo invalida antes de consumirlo, se informa `InvalidatedAt`; invalidar no cuenta como consumir. La validez y la exclusión entre ambos estados están definidas en [domain-model.md → «UserToken»](domain-model.md#usertoken).
- Al emitir un token, se invalidan los tokens anteriores sin usar del mismo usuario y propósito, informando `InvalidatedAt`.
- Al cambiar o restablecer la contraseña, se invalidan también los tokens pendientes de restablecimiento, cambio de email y eliminación de cuenta, informando `InvalidatedAt`.
- Al confirmar un cambio de email, se invalidan los tokens pendientes de restablecimiento de contraseña y de eliminación de cuenta, informando `InvalidatedAt`.
- El registro en logs de los tokens se rige por [«Registro de eventos»](#registro-de-eventos).

| Propósito | Caducidad |
|---|---|
| Completar registro | 24 horas |
| Restablecimiento de contraseña | 15 minutos |
| Cambio de email | 2 horas |
| Confirmación de eliminación de cuenta | 2 horas |

## Correo

| Correo | Destinatario | Cuándo | Límite por minuto |
|---|---|---|---|
| Completar registro | Email del registro | Al registrarse con un email sin cuenta o con un registro sin completar, y al pedir la recuperación de un registro sin completar | Sí |
| Aviso de cuenta existente, con un enlace a la página de recuperación (sin token) | Email del registro o nuevo email solicitado | Al registrarse con un email que ya tiene una cuenta completada, o al pedir un cambio de email hacia uno que ya tiene cuenta | Sí |
| Enlace de restablecimiento de contraseña | Email de la cuenta | Al pedir la recuperación de una cuenta completada | Sí |
| Verificación del nuevo email | Nuevo email | Al pedir el cambio de email | Sí |
| Enlace de confirmación de eliminación de cuenta | Email de la cuenta | Al solicitar la eliminación tras verificar la contraseña | Sí |
| Aviso de email cambiado | Email anterior | Al confirmar el cambio de email | No |
| Aviso de contraseña cambiada | Email de la cuenta | Al cambiar o restablecer la contraseña | No |
| Aviso de cuenta eliminada | Email de la cuenta | Tras confirmar y eliminar la cuenta | No |

- Se acepta como máximo 1 solicitud de correo por minuto y cuenta en cada uno de los dos límites. Son independientes entre sí: el de los correos con token, el del aviso de cuenta existente. Las peticiones que superen ese límite reciben la misma respuesta genérica sin enviar nada, y tampoco emiten ningún token: el token anterior, si lo hay, sigue vigente y el enlace que el usuario ya tiene en su buzón sigue funcionando.
- El aviso de cuenta existente se limita por la cuenta que ya tiene ese email, no por la que registra o pide el cambio.
- Al aceptar una solicitud que respeta su límite, se asigna la fecha UTC actual a `User.LastEmailSentAt` o `User.LastExistingAccountNoticeAt`, según corresponda, y se guarda junto con los cambios de la operación. El límite queda consumido al confirmar esa operación en base de datos, antes de encolar o enviar el correo; no espera al envío SMTP ni a la entrega. Si la operación se revierte, la marca también se revierte. Tras confirmarla, un fallo de encolado o envío no revierte ni modifica la marca ni libera el límite; una nueva solicitud debe esperar a que venza el intervalo. Una petición descartada por el límite no actualiza la marca.
- Los avisos de seguridad (los correos sin límite) se envían siempre y no consultan ni actualizan ninguno de los dos.
- Cómo y cuándo se envían los correos está en [architecture.md → «Correo»](architecture.md#correo).

## Sesión

- Una sesión caduca a los 30 días del login, con independencia de la actividad y de si se marcó «Recordarme». La actividad no prolonga la sesión; solo un nuevo login inicia un plazo nuevo.
- «Recordarme» solo decide si la sesión sobrevive al cierre del navegador: si no se marca, termina al cerrarlo; si se marca, se mantiene hasta caducar.
- Cuando la sesión se vuelve a emitir (al cambiar la contraseña o al confirmar un cambio de email), conserva la fecha de caducidad y el carácter (con o sin «Recordarme») de la sesión original.
- Al caducar la sesión, la siguiente petición a `/app` lleva al login.
- Cambiar la contraseña y confirmar un cambio de email mantienen la sesión actual, si la hay y es del usuario afectado, y cierran todas las demás. Restablecer la contraseña y eliminar la cuenta cierran todas.
- La zona privada muestra en todas sus páginas un botón para cerrar sesión.
- Solo se usan cookies técnicas (autenticación y antiforgery); no hay banner de cookies (motivo en [decisions.md → «Seguridad»](decisions.md#seguridad)).

## URLs de los enlaces

Antes de crear un enlace, la URL introducida pasa por un ajuste y una validación.

La URL que guarda el usuario no se transforma más allá del ajuste, y nunca se sustituye por la de una redirección. Es la que se usa para navegar.

Los duplicados se detectan mediante la clave plana `UrlNormalized`, definida en [«Normalización para duplicados»](#normalización-para-duplicados). Los motivos de esta decisión y las colisiones aceptadas se documentan en [decisions.md → «Riesgos aceptados»](decisions.md#riesgos-aceptados).

### Detección de esquema URI

Esta regla se aplica al ajuste de `UrlOriginal` y `Image`:

1. Antes de detectar un esquema, reconocer una entrada de host y puerto cuando comience por un host válido según [«Validación»](#validación), seguido de `:` y un puerto formado solo por dígitos. Después del puerto debe terminar la entrada o comenzar `/`, `?` o `#`. Para IPv6 entre corchetes, el separador del puerto es el `:` que aparece después de `]`. Esta forma no se interpreta como esquema.
2. En cualquier otro caso, hay un esquema URI explícito si, antes del primer `/`, `?` o `#`, aparece un prefijo que cumple `^[A-Za-z][A-Za-z0-9+.-]*:`.
3. Un esquema explícito `http` o `https` requiere `://` seguido de una autoridad válida. Un esquema explícito con otra sintaxis de autoridad se rechaza.

### Ajuste de URL

El formulario conserva la URL introducida para mostrar los errores. El ajuste produce una URL ajustada o string vacío `""`.

1. Recortar los espacios del principio y del final.
2. Si queda vacía o contiene caracteres de control o saltos de línea, rechazar. También se rechaza cualquier espacio que haya quedado en el interior, incluido el espacio normal y los espacios Unicode, porque el analizador de URI los reescribiría después en silencio.
3. Si empieza por `//`, añadir `https:`.
4. Si tiene un esquema URI explícito, según [«Detección de esquema URI»](#detección-de-esquema-uri), solo se aceptan `http` y `https`, sin distinguir mayúsculas. Cualquier otro esquema (`ftp`, `file`, `text`, `javascript`, `data`, `mailto`, `vbscript` y similares) se rechaza.
5. Si no tiene esquema, añadir `http://` cuando el host es `localhost` y `https://` en cualquier otro caso.
6. Si el resultado supera 2048 caracteres, rechazar el ajuste. La longitud se mide sobre la URL ajustada, con el esquema incluido.

> Diagrama del proceso: [Ajuste de URL](diagrams/process-adjustment-url.md).

- Si el ajuste se rechaza, la URL ajustada queda en `""` y no continúa a [validación](#validación). El formulario muestra un error y no guarda el enlace.
- Si el ajuste produce una URL válida, se envía a [validación](#validación); si esta falla, se muestra un error y no se guarda el enlace.

| Entrada | Resultado |
|---|---|
| `https://ejemplo.com/página` | Aceptada tal cual |
| `//otro-ejemplo.com/otra-pagina` | `https://otro-ejemplo.com/otra-pagina` |
| `localhost:5000/pagina-interna` | `http://localhost:5000/pagina-interna` |
| `example.com:8080/x` | `https://example.com:8080/x` |
| `javascript:alert(1)` | Rechazada: esquema no permitido |
| `mailto:ana@example.com` | Rechazada: esquema no permitido |
| `ftp:example.com` | Rechazada: esquema no permitido |
| `http:example.com` | Rechazada: esquema HTTP sin `://` y autoridad válida |
| `https://192.168.1.1/pagina` | Aceptada tal cual |

### Ajuste de `Image`

Si el usuario deja `Image` vacía (`""`), el formulario permite guardar el enlace sin imagen.

El formulario conserva la URL de imagen introducida para mostrar los errores. El ajuste produce una URL ajustada o `""`. Las mismas reglas se aplican a `Image` introducida por el usuario y a la obtenida por el scraper.

`Image` se muestra como imagen en las páginas públicas y el navegador de cada visitante la descarga automáticamente. Por eso, tanto si la introduce el usuario como si la obtiene el scraper, pasa también por un ajuste:

1. Recortar los espacios del principio y del final.
2. Si queda vacía, tratarla como no informada y no continuar a validación.
3. Si contiene caracteres de control o saltos de línea, rechazar el ajuste. También se rechaza cualquier espacio que haya quedado en el interior, incluido el espacio normal y los espacios Unicode, porque el analizador de URI los reescribiría después en silencio.
4. Si empieza por `//`, añadir `https:`.
5. Si tiene un esquema URI explícito, según [«Detección de esquema URI»](#detección-de-esquema-uri), cambiar `http` a `https`; aceptar `https` sin distinguir mayúsculas y rechazar cualquier otro esquema (`ftp`, `file`, `text`, `javascript`, `data`, `mailto`, `vbscript` y similares).
6. Si tiene la forma host y puerto reconocida en [«Detección de esquema URI»](#detección-de-esquema-uri), tratarla como autoridad y añadir `https://`.
7. Cualquier otra entrada sin esquema se trata como referencia relativa, aunque su primer segmento parezca un nombre de dominio. Para indicar una autoridad sin esquema se debe usar el prefijo `//`. Resolver las referencias relativas mediante las reglas estándar de resolución URI contra `Link.UrlOriginal`, ya ajustada y validada:
  - Una ruta relativa se combina con el directorio de la ruta base; `..` sube un directorio y una ruta que empieza por `/` se resuelve desde la raíz del host.
  - Una referencia con ruta y consulta conserva la consulta indicada. Una referencia formada solo por consulta conserva la ruta base y sustituye su consulta.
  - Una referencia formada solo por fragmento conserva la ruta y la consulta base y sustituye el fragmento.
  - Si la URL resultante usa `http`, cambiar su esquema a `https`.
8. Si el host resultante es `localhost` o una IP, rechazar el ajuste.
9. Si el resultado supera 2048 caracteres, rechazar el ajuste. La longitud se mide sobre la URL ajustada, con el esquema incluido.

> Diagrama del proceso: [Ajuste de Imagen](diagrams/process-adjustment-image.md).

- Si el ajuste se rechaza, la URL ajustada queda en `""` y no continúa a [validación](#validación). Si la URL de `Image` la ha introducido el usuario, el formulario muestra un error y no guarda el enlace; si ha sido el scraper, no modifica `Image`.
- Si el ajuste produce una URL, se envía a [validación](#validación); si la URL de `Image` la ha introducido el usuario y falla la validación, se muestra un error y no se guarda el enlace; si ha sido el scraper, no se modifica `Image` y el enlace se guarda sin imagen.

Para el caso sin redirecciones, la base de resolución es `Link.UrlOriginal`, con independencia del origen de `Image`. La base cuando la página del scraper ha redirigido sigue pendiente en [decisions.md → «Pendientes de scraping»](decisions.md#pendientes-de-scraping).

| Entrada | Resultado |
|---|---|
| `https://cdn.example.com/img.jpg` | Aceptada tal cual |
| `http://cdn.example.com/img.jpg` | `https://cdn.example.com/img.jpg` |
| `//cdn.example.com/img/img-01.jpg` | `https://cdn.example.com/img/img-01.jpg` |
| `cdn.example.com/img.jpg`, en `https://example.com/a/b/post` | `https://example.com/a/b/cdn.example.com/img.jpg` |
| `//cdn.example.com/img.jpg` | `https://cdn.example.com/img.jpg` |
| `localhost:5000/img.jpg` | Rechazada |
| `https://192.168.1.1/img.jpg` | Rechazada |
| `/img/a.jpg`, en `https://example.com/post` | `https://example.com/img/a.jpg` |
| `img.jpg`, en `https://example.com/a/b/post` | `https://example.com/a/b/img.jpg` |
| `../assets/cover.webp?width=800&format=webp`, en `https://example.com/a/b/post` | `https://example.com/a/assets/cover.webp?width=800&format=webp` |
| `images/cover.jpg?width=1200`, en `https://example.com/a/b/post` | `https://example.com/a/b/images/cover.jpg?width=1200` |
| `?width=1200`, en `https://example.com/a/b/post?old=1` | `https://example.com/a/b/post?width=1200` |
| `/img/a.jpg`, en `https://192.168.1.1/post` | Rechazada |

### Validación

Independientemente de si es una URL o una `Image` el proceso de validación es el mismo:

1. Debe ser una URI absoluta válida para `System.Uri`.
2. El esquema debe ser `http` o `https`.
3. El host debe ser `localhost`, una dirección IPv4, una dirección IPv6 entre corchetes o un dominio con al menos un punto y un dominio de primer nivel de 2 o más letras (o en formato `xn--`). Se aceptan dominios internacionalizados.
4. No puede contener credenciales (`usuario:clave@`).
5. El puerto, si existe, debe ser válido.

> Diagrama del proceso: [Validación de URL](diagrams/process-url-validation.md).

`Link.UrlOriginal` o `Link.Image` guardan el resultado del ajuste, sin ninguna otra transformación. Como tras el ajuste el esquema siempre existe, el tratamiento de puertos de la normalización es determinista.

El riesgo de que el navegador de un visitante intente acceder a una dirección privada se acepta (ver [decisions.md → «Riesgos aceptados»](decisions.md#riesgos-aceptados)), aunque ello no implica que el scraper pueda descargarlo (ver [architecture.md → «Scraping»](architecture.md#scraping)).

| Entrada | Resultado |
|---|---|
| `example.com/articulo` | `https://example.com/articulo` |
| ` https://Example.com/a ` | `https://Example.com/a` |
| `localhost:5000/admin` | `http://localhost:5000/admin` |
| `example.com:8080/x` | `https://example.com:8080/x` |
| `http://192.168.1.1` | Aceptada tal cual |
| `http://[2001:db8::1]/` | Aceptada tal cual |
| `javascript:alert(1)` | Rechazada: esquema no permitido |
| `mailto:ana@example.com` | Rechazada: esquema no permitido |
| `ftp://example.com` | Rechazada: esquema no permitido |
| `https://intranet` | Rechazada: sin dominio |
| `intranet` | Rechazada: sin dominio |
| `https://ana:clave@example.com` | Rechazada: contiene credenciales |

### Normalización para duplicados

`Link.UrlNormalized` se genera en dos fases. Primero se trabaja sobre la estructura de la URL; después, sobre el texto resultante.

Fase 1, sobre la URL analizada con `System.Uri`:

1. Partir de la URL ya ajustada y validada.
2. Descartar el fragmento.
3. Tratar el puerto: eliminarlo si es `80` en HTTP o `443` en HTTPS, que son los puertos por defecto; conservar cualquier otro puerto.
4. Descartar el esquema `http` o `https`, ya que ambos se consideran equivalentes.
5. Tomar el host con `Uri.IdnHost`, que devuelve los dominios internacionalizados en punycode (`xn--`).
6. Decodificar la ruta una sola vez con la decodificación de URI (`Uri.UnescapeDataString`), en la que `+` es un carácter literal.
7. Separar los parámetros de la consulta y decodificar una sola vez el nombre y el valor de cada uno con la decodificación de formularios, en la que `+` equivale a un espacio.
8. Eliminar los parámetros de marketing conocidos (lista siguiente).
9. Convertir a minúsculas el nombre y el valor de los parámetros restantes, sin reglas de idioma (cultura invariante).
10. Ordenar los parámetros restantes por nombre y valor mediante comparación ordinal (por código de carácter, sin reglas de idioma).
11. Conservar los parámetros repetidos.
12. Unir en una sola cadena el host, el puerto si se conserva, la ruta y los parámetros ordenados.

Fase 2, sobre la cadena resultante, sin volver a decodificar nada:

13. Convertir a minúsculas la URL.
14. Eliminar los acentos y demás marcas diacríticas conservando la letra base.
15. Sustituir `ç` por `c`.
16. Sustituir `ñ` por `n`.
17. Convertir los espacios en guiones medios.
18. Conservar únicamente letras y números de cualquier alfabeto (categorías Unicode Letter y Number) y guiones medios.
19. Eliminar cualquier otro carácter, incluidos `.`, `:`, `/`, `?`, `&` y `=`.
20. Eliminar guiones medios duplicados (`--`).
21. Eliminar guiones medios al principio y al final.

> Diagrama del proceso: [Normalización de URL para detectar duplicados](diagrams/process-url-normalization.md).

Cada componente se decodifica exactamente una vez, con la regla que le corresponde. Como el fragmento y los parámetros se separan sobre la URL codificada y después nada se reinterpreta, un `%23` o un `%26` codificados nunca cortan la URL ni separan parámetros: se convierten en texto y desaparecen al aplanar. Un texto codificado dos veces (`%2520`) se decodifica una sola vez y conserva `%20` como texto: al aplanar se elimina el signo `%`, pero se conservan los dígitos `20`; no se vuelve a interpretar como un espacio.

Parámetros de marketing eliminados, sin distinguir mayúsculas: `utm_source`, `utm_medium`, `utm_campaign`, `utm_term`, `utm_content`, `gclsrc`, `gclid`, `wbraid`, `gbraid`, `fbclid`, `msclkid`, `ttclid`, `epik`, `gad_source`, `gad_campaignid`, `srsltid`, `gcs`, `gcd`, `at_medium`, `at_campaign`, `at_platform`, `at_creation` y `at_term`.

Ejemplos:

```text
HTTPS://Example.COM/Generación%20de%20datos/?utm_source=google&id=10
-> examplecomgeneracion-de-datosid10
```

```text
http://example.com:4587/Artículo%20número%202?b=2&a=1
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

En la ruta, `+` es literal y se elimina al aplanar; en la consulta, `+` es un espacio y se convierte en guión. Como el guión final se elimina, la clave resulta igual, pero `?q=c++%202` produce `qc-2` y `/c++2` produce `c2`.

```text
https://example.com/a%2520b
-> examplecoma20b
```

Un dominio internacionalizado produce la misma clave escrito en Unicode o en punycode, y las rutas en otros alfabetos se conservan:

```text
https://münchen.de/日本語
https://xn--mnchen-3ya.de/日本語
-> xn-mnchen-3yade日本語
```

```text
https://example.com/日本語
-> examplecom日本語
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

## Scraping

Correspondencia de metadatos, tomando el primero que exista de cada lista:

| Campo | Se toma de, en este orden |
|---|---|
| `Title` | `<meta name="title">` → `<title>` → `<meta property="og:title">` |
| `Description` | `<meta name="description">` → `<meta property="og:description">` |
| `Image` | `<meta property="og:image">`, ajustada según [«Ajuste de `Image`»](#ajuste-de-image) y validada según [«Validación»](#validación) |

Flujo previsto para el scraping, sujeto a los pendientes de [decisions.md → «Pendientes de scraping»](decisions.md#pendientes-de-scraping):

- Al crear un enlace, `ScrapingStatus` pasa a `NotRequested`.
- Cuando el usuario lance el scraping manualmente, `ScrapingStatus` pasa a `Pending`.
- Cada intento pone `ScrapingStatus` en `Processing`.
- Cada intento solo rellena los campos vacíos; nunca sobrescribe valores ya informados, ya sea por un intento anterior o por edición manual del usuario.
- El resultado de un intento decide el estado:
  - Respuesta `200` con HTML: se extraen los metadatos que haya y el enlace queda `Completed`, tenga los tres campos, alguno o ninguno. No se reintenta.
  - Timeout, error de red o respuesta `5xx`: se considera `Failed`. El usuario podrá lanzarlo manualmente.
  - Respuesta `4xx`, contenido que no es HTML, respuesta que supera el tamaño máximo o destino bloqueado por la protección SSRF. El enlace queda `Failed`. El usuario podrá lanzarlo manualmente.
- El enlace se conserva siempre, con los metadatos que se hayan podido obtener.
- El usuario puede volver a lanzar el scraping sin borrar previamente los metadatos existentes. Cada nuevo intento solo rellena los campos vacíos y conserva los ya informados. Para volver a obtener el contenido de un campo concreto, el usuario debe vaciar ese campo antes de lanzar el scraping; no necesita vaciar los demás. Si todos los campos están informados, el nuevo intento no modifica sus valores.

## Estados del scraping

`ScrapingStatus` de `Link` toma uno de estos valores:

- `NotRequested`: no se ha solicitado scraping.
- `Pending`: hay una solicitud de scraping pendiente de ejecución.
- `Processing`: hay un intento en curso.
- `Completed`: se descargó la página y se extrajeron los metadatos que tenía, fueran los que fueran.
- `Failed`: el intento de scraping no pudo completarse por un error o por incumplir las condiciones de descarga o procesamiento.

## Búsqueda y filtros

La búsqueda es el parámetro `q` de cada listado privado: un listado con `q` sigue siendo ese mismo listado, filtrado. Las páginas públicas no tienen búsqueda. Los parámetros que acepta cada listado están en [«Rutas privadas»](#rutas-privadas).

| Listado | Resultado | Campos que cubre `q` | Otros filtros |
|---|---|---|---|
| Enlaces | Solo enlaces del usuario identificado | URL original, título, descripción, nombre de la colección y nombres de sus etiquetas | Colección y etiqueta |
| Colecciones | Solo colecciones del usuario identificado | `Collection.Name` y `Collection.Description` | Ninguno |
| Etiquetas | Solo etiquetas del usuario identificado | Solo `Tag.Name` | Ninguno |

Cada listado devuelve solo su propia entidad y nunca mezcla tipos de resultado. El orden y la paginación están en [«Listados»](#listados).

Reglas de `q`, iguales en todos los listados:

- La búsqueda sin distinguir acentos y otras marcas diacríticas se garantiza para los caracteres latinos cubiertos por el tokenizador definido en [architecture.md → «FTS5»](architecture.md#fts5): «canción» encuentra «cancion». Se admiten términos de cualquier alfabeto, pero no se garantiza la equivalencia con y sin diacríticos fuera de esa cobertura; por ejemplo, no se exige que el griego `καφές` coincida con `καφες`.
- Antes de separar el texto en términos, se normaliza a Unicode NFC. El mismo preprocesamiento se aplica al texto de los campos al incorporarlo a las proyecciones FTS5; solo afecta a la búsqueda y no modifica los valores originales. Así, `canción` y `cancio\u0301n` se buscan como la misma palabra.
- No distingue mayúsculas y minúsculas dentro de la cobertura Unicode del tokenizador definido en [architecture.md → «FTS5»](architecture.md#fts5); no se garantiza esa equivalencia para caracteres fuera de su cobertura.
- El texto se separa en secuencias de letras o números, de cualquier alfabeto. Cada secuencia es un término; los demás caracteres (espacios, comillas, `*`, guiones y cualquier otro símbolo) solo separan términos y nunca actúan como operadores. `AND`, `OR`, `NOT` y `NEAR` son términos como cualquier otro.
- Todos los términos son obligatorios: un elemento solo aparece si los contiene todos. Se pasan escapados a la búsqueda de texto.
- Cada término coincide con el comienzo de una palabra, incluida la palabra completa: «canc» encuentra «canción», pero «cion» no encuentra «canción».
- Un `q` vacío, formado solo por espacios o que no deja ningún término equivale a no buscar por texto: el listado se muestra sin filtrar por texto y solo se aplican sus otros filtros, si los tiene.

Reglas de los filtros del listado de enlaces:

- El filtro de colección devuelve solo los enlaces de esa colección. 
- El filtro de etiqueta devuelve solo los enlaces que tienen esa etiqueta.
- `q` y los filtros disponibles se combinan y se aplican todos a la vez.
- Los listados de colecciones y de etiquetas no tienen más filtro que `q`.

## Listados

| Listado | Orden | Paginado |
|---|---|---|
| Enlaces recientes del dashboard | `Link.CreatedAt` descendente e `Link.Id` descendente como desempate | No; muestra hasta 12 enlaces |
| Enlaces privados (zona privada), con o sin `q` | Con texto de búsqueda: relevancia, después `Link.CreatedAt` descendente y `Link.Id` descendente como desempates. Sin texto de búsqueda: `Link.CreatedAt` descendente y `Link.Id` descendente como desempate | Sí |
| Colecciones del usuario (privado), con o sin `q` | `Name` ascendente e `Id` ascendente como desempate | Sí |
| Etiquetas del usuario (privado), con o sin `q` | `Name` ascendente e `Id` ascendente como desempate | Sí |
| Colecciones que se muestran en público (página de colecciones) | `Name` ascendente e `Id` ascendente como desempate | No |
| Enlaces públicos de una colección (página de colección) | Texto a mostrar (`Title` o si no tiene título `UrlOriginal` ). Orden a aplicar (`Title` o si no tiene título `UrlOriginal`) ascendente e `Id` ascendente como desempate | Sí |
| Últimos enlaces públicos de todas las colecciones que se muestran en público (página de usuario; cantidad en [«Página de usuario»](#página-de-usuario)) | `CreatedAt` descendente e `Id` descendente como desempate | No |
| 5 últimas colecciones que se muestran en público (página de usuario) | `CreatedAt` descendente e `Id` descendente como desempate | No |

- Los órdenes alfabéticos de `Name` y del texto mostrado se comparan según el orden lingüístico del castellano, sin distinguir mayúsculas ni marcas diacríticas. No se exige que la `ñ` conserve un peso alfabético propio: la colación puede tratarla como equivalente a la `n`. Si dos valores son equivalentes con esa comparación, se aplica el desempate por `Id` indicado en la tabla.
- `q` y los demás filtros solo reducen el conjunto: la relevancia solo ordena el listado de enlaces cuando hay texto de búsqueda, y los listados de colecciones y de etiquetas conservan su orden alfabético con o sin `q`.
- Los listados paginados muestran 16 elementos por página. Los bloques de enlaces recientes del dashboard y de la página de usuario y el de colecciones recientes de la página de usuario muestran como máximo la cantidad indicada en sus reglas; los demás listados no paginados muestran todos sus elementos.
- La paginación se aplica sobre el resultado ya filtrado y ordenado: la página 1 es la primera de ese resultado y el número de páginas depende de él.
- El tratamiento de páginas fuera de rango se define en [«Rutas privadas»](#rutas-privadas) y [«Paginación de las páginas públicas»](#paginación-de-las-páginas-públicas).

## Páginas privadas

### Rutas privadas

| Página o acción | Ruta | Método |
|---|---|---|
| Entrada del área privada y dashboard | `/app` | GET y POST; GET muestra el dashboard y POST procesa la creación rápida |
| Listado y búsqueda de enlaces | `/app/enlaces` | GET |
| Listado y búsqueda de colecciones | `/app/colecciones` | GET |
| Listado y búsqueda de etiquetas | `/app/etiquetas` | GET |
| Crear enlace | `/app/enlaces/nuevo` | GET y POST |
| Editar enlace | `/app/enlaces/{id}/editar` | GET y POST |
| Mover enlace | `/app/enlaces/{id}/mover` | GET y POST |
| Eliminar enlace | `/app/enlaces/{id}/eliminar` | GET y POST |
| Crear colección | `/app/colecciones/nueva` | GET y POST |
| Editar colección (nombre, descripción y visibilidad) | `/app/colecciones/{id}/editar` | GET y POST |
| Eliminar colección | `/app/colecciones/{id}/eliminar` | GET y POST |
| Crear etiqueta | `/app/etiquetas/nueva` | GET y POST |
| Editar etiqueta | `/app/etiquetas/{id}/editar` | GET y POST |
| Eliminar etiqueta | `/app/etiquetas/{id}/eliminar` | GET y POST |

- El dashboard se sirve directamente en `/app` y muestra el formulario «Crear enlace rápido», el buscador de enlaces y el bloque «Enlaces recientes». Este bloque contiene los enlaces más recientes del usuario identificado, públicos y privados y de todas sus colecciones, con la cantidad y el orden definidos en [«Listados»](#listados), sin búsqueda, filtros ni paginación propios. El buscador envía por GET a `/app/enlaces` con `q`: sus resultados se muestran en ese listado, no en el dashboard.
- Desde cada enlace reciente se puede abrir la URL original en una pestaña nueva o acceder a los formularios canónicos de edición, movimiento y eliminación. No se editan ni eliminan enlaces mediante el GET del dashboard; las operaciones se ejecutan en sus formularios y conservan el origen según las reglas de `ru` de esta sección. El bloque reutiliza la presentación «Tarjetas / Lista» y los distintivos de privacidad de los listados privados.
- Si no hay enlaces, el bloque muestra «Todavía no tienes enlaces guardados.» y «Añade un enlace para empezar a organizar tus recursos.», sin selector de presentación. La creación rápida y el buscador siguen disponibles. El menú lateral mantiene el acceso a la creación completa de enlaces y a la gestión de colecciones.
- La opción «Avanzado...» del menú de creación rápida abre `/app/enlaces/nuevo`, trasladando al campo URL el texto introducido mediante el parámetro `url`, sin crear todavía el enlace. La creación completa requiere elegir una colección propia y aplica sus reglas habituales.
- El menú lateral del área privada ofrece «+ Crear enlace» debajo de la cabecera y antes del buscador, separado de este mediante espacio y una línea divisoria sutil; abre el formulario de creación de enlace. El buscador queda seguido de la navegación. Cuando el menú está contraído, muestra solo el icono «+», con tooltip y nombre accesible «Crear enlace».
- El estado contraído o expandido del menú lateral se conserva al navegar entre páginas del área privada. La apertura temporal del menú móvil no se restaura en la siguiente página.
- «Crear colección» en «Mis colecciones» se presenta como una herramienta neutra, con texto sin negrita, junto al título «Colecciones».
- El listado privado de enlaces ofrece «+ Crear enlace» junto al selector «Tarjetas / Lista», también cuando está vacío y todavía no muestra selector. Se presenta como una herramienta neutra, con la misma altura que el selector y texto sin negrita, no como un botón principal. En pantallas estrechas, ambos controles permanecen juntos. En el listado general abre el formulario sin colección preseleccionada y el usuario debe escogerla. Dentro de una colección, tanto ese botón como «+ Crear enlace» del menú lateral abren el formulario con la colección actual preseleccionada. El formulario solo acepta como preselección una colección del usuario identificado; una referencia inválida, inexistente o ajena no preselecciona ninguna ni revela datos. El usuario puede cambiar la colección antes de crear el enlace. Crear o cancelar devuelve al listado de origen conservando búsqueda y página; la preselección no hace público el enlace.
- Cada colección del menú lateral abre `/app/enlaces` con su identificador en el filtro `c`; el listado muestra sus enlaces y marca solo la colección seleccionada en el menú, sin activar los enlaces de navegación general. «Mis colecciones» se marca al acceder al listado de colecciones. Los enlaces reutilizan la presentación y las acciones del dashboard. El orden y la paginación siguen [«Listados»](#listados), y la búsqueda conserva el filtro según las reglas de esta sección.
- Las tarjetas del dashboard y de los listados privados de enlaces muestran un distintivo de privacidad junto al nombre de la colección y antes de las acciones: globo y texto «Público» en mint para los enlaces públicos, o candado y texto «Privado» en gris para los privados. Los tooltips respectivos son «Este enlace es público» y «Este enlace es privado». El estado de la colección no determina el distintivo del enlace.
- La vista Tarjetas del dashboard y de los listados privados de enlaces muestra tres tarjetas por fila en escritorio hasta 1920 píxeles CSS de ancho del viewport, cuatro desde 1921 y cinco desde 2560. En pantallas menores conserva la adaptación a una o dos tarjetas por fila. Estos cambios no alteran la vista Lista.
- En las tarjetas privadas de enlaces sin imagen, el dibujo abstracto se sustituye por el símbolo decorativo de Linkubator, sin enlace propio, conservando el fondo y el título. Las imágenes informadas no cambian. La vista Lista continúa sin imágenes ni favicons.
- El dashboard y los listados privados de enlaces, generales o filtrados por colección, ofrecen el selector «Tarjetas / Lista» cuando contienen enlaces, en la misma fila que el título del bloque cuando hay espacio suficiente y alineado a la derecha también en pantallas estrechas. Cambia la presentación sin recargar ni alterar los enlaces, su orden, los filtros o la página actual. La vista inicial es Tarjetas; la elección es compartida entre estas páginas, se conserva durante la sesión de navegación y se restaura al navegar, paginar o regresar a cualquiera de ellas. La lista centra el bloque de filas dentro de su contenedor y muestra un enlace por fila, sin imagen ni descripción: título enlazado a la URL original en una pestaña nueva, colección y dominio a la izquierda; editar, mover, eliminar y abrir a continuación del bloque de texto, próximos a él y alineados entre filas, con el distintivo de privacidad debajo de las herramientas y alineado al borde derecho de la fila. En pantallas estrechas, las herramientas pasan debajo de los datos, alineadas a la izquierda, y el distintivo permanece debajo de ellas a la derecha, sin desplazamiento horizontal.
- Los paneles desplegables de selección de colección y el menú de creación rápida del dashboard comparten un fondo verde sólido que no deja entrever el contenido situado detrás.
- «Añadir» en el formulario de creación rápida del dashboard crea un enlace con la URL introducida, privado y sin título, descripción ni imagen. Se asigna a la colección propia cuyo slug sea el de [«Generación de slugs»](#generación-de-slugs) para «Bandeja de entrada»; si no existe, se usa la primera colección propia en el orden de [«Listados»](#listados). Si no hay colecciones disponibles, no se crea el enlace y se muestra «No tienes colecciones disponibles. Crea una colección para añadir enlaces.». La creación aplica las mismas reglas de URL y duplicados que el formulario completo. Tras crear, se vuelve al dashboard; ante un error se conserva la URL y se muestra el error de forma accesible.
- El identificador de ruta `id` y los parámetros de listado `p`, `c` y `e` son enteros positivos escritos solo con dígitos. Los ceros iniciales se aceptan: `003` se interpreta como `3`.
- En los listados, cada filtro `c` o `e` vacío se ignora independientemente de los demás filtros; `p` ausente o vacío indica la primera página. Se conservan los demás filtros válidos.
- Un valor de `p` inválido se ignora y se muestra la primera página sin error. Si es válido pero supera la última página del resultado filtrado, se muestra la última página sin error; si no hay resultados, se muestra el estado vacío de la primera página. Se conservan la búsqueda y los filtros válidos. La regla de las páginas públicas está en [«Paginación de las páginas públicas»](#paginación-de-las-páginas-públicas).
- Un valor de `c` o `e` inválido o fuera de rango muestra un mensaje accesible indicando que los valores no son correctos.
- Un filtro `c` o `e` que identifica una colección o etiqueta inexistente o ajena al usuario se ignora; se conservan los demás filtros válidos.
- En las rutas que identifican un recurso mediante `id`, un valor que no cumpla el formato de identificador válido produce un error de validación y no consulta ni modifica un recurso. Si el identificador tiene formato válido pero el recurso no existe o pertenece a otro usuario, ambos casos reciben el mismo resultado de recurso no accesible; no se muestran datos ni se revela cuál de esas causas ocurrió. En GET se muestra el mensaje genérico «No se ha podido acceder al elemento solicitado.» con un enlace al listado correspondiente; en POST se redirige a ese listado con el mismo aviso accesible.
- El GET de edición de un enlace o colección carga sus datos desde su caso de uso de aplicación mediante el `id` de ruta; la consulta se limita al usuario identificado. Si no existe o pertenece a otro usuario, aplica la respuesta GET de recurso no accesible indicada arriba.
- Al eliminar una colección, un identificador inválido produce un error de validación y no modifica datos. Si el identificador tiene formato válido pero la colección no existe o pertenece a otro usuario, la operación no modifica datos ni revela cuál de esas dos condiciones se da; muestra «No se ha podido eliminar la colección». En GET se muestra el mensaje con un enlace al listado de colecciones; en POST se redirige a ese listado con el mismo aviso accesible.
- Al eliminar un enlace, un identificador inválido produce un error de validación y no modifica datos. Si el identificador tiene formato válido pero el enlace no existe o pertenece a otro usuario, la operación no modifica datos ni revela cuál de esas dos condiciones se da; muestra «No se ha podido eliminar el enlace». En GET se muestra el mensaje con un enlace al listado de enlaces; en POST se redirige a ese listado con el mismo aviso accesible.
- Al crear, editar o mover un enlace, un identificador de colección o etiqueta inválido, inexistente o ajeno muestra un error de validación en el formulario enviado, sin aplicar cambios.
- El parámetro de URL `q` es de tipo texto. Un valor vacío, también provocará que se desprecie el parámetro, retornando la misma página desde la que se ha enviado, sin aplicar ningún tipo de búsqueda pero aplicando el resto de filtros si existieran. Si tras recortar sus espacios extremos, supera su [longitud máxima](#longitudes-máximas), mostrará también un mensaje de error.
- Cada listado acepta solo estos parámetros, cuya semántica está en [«Búsqueda y filtros»](#búsqueda-y-filtros):
  - Enlaces: `q`, `c`, `e` y `p`.
  - Colecciones y etiquetas: solo `q` y `p`.
- Un parámetro repetido hace que se aplique el primer valor y se desprecien los valores sucesivos.
- Un parámetro desconocido, no aplicable a ese listado o de una capacidad todavía no disponible se ignora por completo: no se valida ni se consulta, y no provoca ningún error.
- Los listados privados usan `p`; sus enlaces a la primera página no llevan el parámetro. `?p=1` se sirve con el mismo contenido. La página 1 de un listado sin resultados muestra el estado vacío. Los enlaces de paginación conservan `q` y los filtros aplicables de ese listado.
- Cada listado con búsqueda ofrece un formulario que envía por GET a su propia ruta con `q`, conserva los filtros aplicables de ese listado y descarta `p`, de modo que la búsqueda empieza en la primera página.
- Los selectores de colección de los formularios de creación, edición y movimiento de enlaces y el filtro `c` incluyen todas las colecciones del usuario, ordenadas según [«Listados»](#listados) y sin paginar. En edición se selecciona inicialmente la colección actual. La selección de etiquetas de los formularios de creación y edición y el filtro `e` incluyen todas las etiquetas del usuario con el mismo criterio de orden y sin paginar; el formulario independiente de movimiento no permite cambiar las etiquetas.
- En los formularios de creación y edición de un enlace, la asignación de etiquetas permite seleccionar etiquetas existentes del usuario o crear una desde el propio formulario, conforme a [«Generación de slugs»](#generación-de-slugs). En edición se muestran inicialmente las etiquetas ya asignadas; al guardar, las seleccionadas determinan el conjunto final de asociaciones: se conservan las que siguen seleccionadas, se añaden las nuevas y se eliminan las retiradas. Una selección vacía deja el enlace sin etiquetas. Una etiqueta resuelta por varios caminos, incluida la coincidencia de slug con una existente, se asocia una sola vez. Los efectos de quitar una asociación se definen en [domain-model.md → «LinkTag»](domain-model.md#linktag).
- Los cambios de asociaciones se confirman junto con el resto de la creación o edición del enlace, las etiquetas nuevas que corresponda crear y las proyecciones de búsqueda afectadas, según [architecture.md → «Persistencia»](architecture.md#persistencia). Si se rechaza la operación, no se aplican cambios parciales ni quedan etiquetas creadas por ese envío. Cancelar o abrir el formulario no modifica etiquetas ni asociaciones. El formulario independiente de movimiento conserva las asociaciones existentes.
- Al crear o editar un enlace, la opción para hacerlo público aparece inicialmente desmarcada, incluso al editar un enlace guardado como público. Solo se habilita al seleccionar una colección pública; con una colección privada o sin selección, queda deshabilitada y desmarcada, y explica el motivo. Al guardar, marcada establece el estado público y desmarcada el privado; por tanto, para conservar público un enlace que se está editando hay que volver a marcarla. Cambiar a una colección privada o quitar la selección desmarca la opción. Una petición manipulada que intente hacerlo público al crearlo en una colección privada o al editarlo sin cambiar su colección privada se rechaza según [«Errores controlados»](#errores-controlados). Si cambia la colección, prevalece la regla de movimiento indicada a continuación.
- Si se cambia la colección desde el formulario de edición, se aplica la regla de movimiento de [domain-model.md → «Público y privado»](domain-model.md#público-y-privado): el enlace pasa a ser privado, tanto si el destino es público como privado, aunque se envíe una petición manipulada para hacerlo público. La solicitud simultánea de publicación se ignora y no provoca rechazo por privacidad; esto no omite las demás validaciones de la edición. Para hacerlo público en la nueva colección se requiere una edición posterior y explícita, y que esa colección sea pública. Mientras el destino sea distinto de la colección actual, el control «Hacer público» queda deshabilitado y desmarcado y muestra «Al cambiar de colección, el enlace pasará a ser privado». La página independiente para mover enlaces se conserva.
- El formulario de edición de una colección permite actualizar conjuntamente su nombre, descripción y estado público o privado. El efecto de la transición sobre sus enlaces se rige por [domain-model.md → «Público y privado»](domain-model.md#público-y-privado), y su persistencia por [architecture.md → «Persistencia»](architecture.md#persistencia). El formulario envía un único estado destino explícito. La ruta y método de edición se indican en la tabla anterior.
- El formulario de edición de un enlace permite actualizar el estado público o privado junto con los demás campos editables. Si el valor recibido en el checkbox no se puede interpretar como booleano, se rechaza la edición completa, no se modifica ningún campo y se vuelve a mostrar el formulario con el error de validación correspondiente.
- En los formularios de creación y edición de colecciones, si el valor recibido en el checkbox no se puede interpretar como booleano, se rechaza la operación completa, no se crea la colección ni se modifica ningún campo de la colección o sus enlaces, y se vuelve a mostrar el formulario conservando el nombre y la descripción enviados. El error asociado al checkbox es «El estado público o privado no es válido.».
- Por defecto, una acción POST realizada correctamente redirige a la lista correspondiente: los enlaces a `/app/enlaces`, las colecciones a `/app/colecciones` y las etiquetas a `/app/etiquetas`, sin conservar filtros ni página. Los formularios de editar, mover y eliminar enlaces conservan su origen mediante `ru`: aceptan como destino el dashboard o el listado de enlaces de la propia aplicación, con su búsqueda, filtros y página. Al completar la acción o cancelar, vuelven a ese destino. Un origen ausente, externo o distinto de esas páginas usa el listado de enlaces como destino. Un error de validación vuelve a mostrar su formulario y conserva el origen; un rechazo de privacidad redirige con un aviso accesible, respetando el origen en los formularios que lo admiten.
- «+ Crear enlace» del menú lateral transmite el origen al formulario mediante `ru`. La creación admite como origen el dashboard, el listado de enlaces o el listado de colecciones de la propia aplicación, con su búsqueda, filtros y página. Crear o cancelar vuelve al origen; el acceso directo, un destino externo o una página no admitida vuelve al listado de enlaces. Los errores de validación conservan el origen y los valores del formulario; un rechazo de privacidad vuelve al origen con el aviso accesible correspondiente. Tras crear se muestra una confirmación accesible «Enlace creado.» y la acción «Ver enlace», que abre la URL guardada en una pestaña nueva. La confirmación se muestra una sola vez.
- Las rutas GET de crear, editar, mover y eliminar son las páginas canónicas de sus formularios. La página de eliminar pide confirmación antes de enviar su POST. La interfaz puede presentar esos mismos formularios en diálogos, sin cambiar las rutas, los métodos ni el comportamiento. Los errores de validación y de reglas del dominio se muestran junto al formulario correspondiente; los errores de validación siguen asociados a sus campos.
- Una ruta con un método distinto del indicado en la tabla responde el mismo `404` genérico, incluso si el recurso existe y pertenece al usuario.
- Éstas páginas llevarán la meta etiqueta `<meta name="robots" content="noindex, nofollow" />`.

## Páginas públicas

Para cada imagen de un enlace público, el atributo `alt` contiene el título del enlace si está informado; si no, contiene `UrlOriginal`. El valor es texto, no marcado HTML.

Si un enlace no tiene imagen, su tarjeta pública muestra el símbolo de Linkubator centrado como recurso decorativo, sin texto alternativo ni cambios en los datos del enlace.

Las páginas públicas no muestran avatar de usuario ni los distintivos «Enlace público» y «Colección pública». La línea de origen de cada enlace muestra solo su dominio, sin esquema, puerto, ruta, consulta ni fragmento; esto no modifica las reglas de título ni de destino de cada página.

Las páginas de usuario, colecciones y colección presentan el banner antes de las pestañas de navegación `Perfil` y `Colecciones`; esta última se ofrece cuando existen colecciones que se muestran en público. Los banners comparten una altura base en cada tamaño de pantalla y pueden crecer si el texto lo requiere, sin recortarlo. La navegación no repite el alias ni muestra «Espacio público», y los banners no incluyen enlaces de vuelta que dupliquen las pestañas.

Al mostrar el alias como texto en las páginas públicas, sus metadatos o el enlace de perfil de la barra lateral privada, cada guion se sustituye por un espacio. El alias almacenado, los destinos de los enlaces y las URLs canónicas conservan los guiones; las plantillas de texto de estas páginas usan esta presentación de `{alias}`.

Las cabeceras públicas de la landing, privacidad, usuario, colecciones, colección y página no encontrada comparten los botones `Login` y `Registro`, con el diseño de la landing. Las acciones de acceso al área privada se etiquetan `Login`. Mientras el registro no esté habilitado, sus enlaces conservan el destino provisional `#` y muestran el tooltip `Registro no disponible`.

Los pies del área pública y privada muestran `Copyright 2026.` inmediatamente antes del enlace `Privacidad`, en la misma línea y debajo de su texto descriptivo.

### Rutas públicas

| Página | Ruta |
|---|---|
| Landing | `/` |
| Página de usuario | `/{alias}/` |
| Página de colecciones | `/{alias}/colecciones/` |
| Página de colección | `/{alias}/colecciones/{slug}` |

- Solo estas páginas son indexables. Un enlace nunca tiene página propia.
- Barra final: una ruta que tiene páginas por debajo termina en `/`, y una ruta final no. Por eso `/{alias}/` termina en `/` (debajo tiene la página de colecciones y las páginas de cada colección) y `/{alias}/colecciones/{slug}` no (un enlace nunca tiene página propia).
- Si una página es accesible con varias URLs equivalentes (sin barra final, con mayúsculas), se sirven todas con redirecciones a la url en minúsculas. La etiqueta canonical apunta a la forma canónica en minúsculas de la tabla.
- Todas se renderizan en servidor, con encabezados semánticos.

Las páginas públicas no muestran el nombre ni el email del usuario.

### Paginación de las páginas públicas

- Solo se pagina la página de colección (ver [«Listados»](#listados)).
- La página se indica con el parámetro de consulta `p`: `/{alias}/colecciones/{slug}?p=2`.
- `p` acepta un entero positivo escrito solo con dígitos; los ceros iniciales se aceptan. Si falta, está vacío o su valor no es válido, se muestra la primera página sin error. Si su valor es válido pero supera la última página, se muestra la última página sin error. Los parámetros desconocidos se ignoran. Si `p` aparece varias veces, se usa el primer valor. Esta regla aplica a la paginación de la página pública de colección.
- La primera página es la URL sin parámetro. `?p=1`, cualquier otra representación de 1 con ceros iniciales y cualquier valor inválido de `p` se sirven con el mismo contenido; su canonical apunta a la URL sin parámetro.
- Las páginas 2 y siguientes tienen como canonical la URL con el número de página en decimal y sin ceros iniciales. Por ejemplo, `?p=02` sirve la página 2 y su canonical apunta a `?p=2`.

### Landing

- Explica qué es Linkubator.
- Incluye un botón de login para poder entrar en la zona privada y otro de registro para poderse registrar como usuario de Linkubator.
- El `meta title` es `Linkubator, donde gestionar tus enlaces favoritos`.
- La `meta description` es `Linkubator es un servicio web que te permite gestionar tus enlaces favoritos en colecciones y etiquetas. Para que nunca más pierdas los enlaces importantes`.
- El `h1` es `Linkubator, para gestionar tus enlaces favoritos y no perderlos`
- La `canonical` es el propio dominio.

### Página de usuario

- Muestra los 6 últimos enlaces públicos de todas las colecciones que se muestran en público. Si tiene menos de 6, se muestran los que tenga. Cada enlace muestra su título (o la URL si no tiene), su descripción y su imagen si las tiene, y enlaza a `UrlOriginal` con `rel="nofollow ugc noopener noreferrer"` y `target="_blank"` para que se abran los enlaces en otra página y no se pierda la página de Linkubator. Un enlace privado o un enlace de una colección privada nunca aparece aquí. Si el usuario no tiene ningún enlace público, se mostrará el siguiente texto `{alias} todavía no ha creado enlaces`.
- El orden de los últimos enlaces públicos está especificado en [«Listados»](#listados).
- Después muestra las 5 últimas colecciones que se muestran en público ([domain-model.md → «Público y privado»](domain-model.md#público-y-privado)) ordenadas según el criterio especificado en [«Listados»](#listados); con su nombre, su descripción y un enlace a cada una. Si tiene menos de 5, se muestran las que tenga. Si el usuario no tiene ninguna colección que se muestre en público, se mostrará el siguiente texto `{alias} todavía no ha creado colecciones`.
- Si el usuario tiene colecciones públicas con enlaces públicos, se incluirá un enlace a `/{alias}/colecciones/` para consultar todas las colecciones públicas del usuario.
- La página responde `404` si el alias no existe (incluido un alias antiguo tras un cambio).
- El `meta title` es `Los enlaces favoritos de {alias} en Linkubator`. 
- La `meta description` es `Los enlaces favoritos más importantes de {alias} en Linkubator, para gestionarlos y no perderlos nunca. Regístrate en Linkubator para tener tu página`.
- El `h1` es `Enlaces favoritos de {alias}`.
- En esta página además tendremos dos elementos `h2`:
  - `Los últimos enlaces de {alias}` que será el encabezado de la sección de enlaces.
  - `Las últimas colecciones de {alias}` que será el encabezado de la sección de colecciones.
- La `canonical` es `/{alias}/`.

### Página de colecciones

- Muestra todas las colecciones del usuario que se muestran en público ([domain-model.md → «Público y privado»](domain-model.md#público-y-privado)) con su nombre, su descripción y un enlace a cada una.
- El orden de estas colecciones es el establecido en [«Listados»](#listados).
- La página responde `404` si el alias no existe (incluido un alias antiguo tras un cambio). Si el usuario no tiene ninguna colección que se muestre en público, se mostrará el texto `{alias} todavía no ha creado colecciones`.
- El `meta title` es `Las colecciones de {alias} en Linkubator`. 
- La `meta description` es `Las colecciones de enlaces favoritos de {alias} en Linkubator, para gestionarlos y no perderlos nunca. Regístrate en Linkubator para tener tu página`.
- El `h1` es `Colecciones de {alias}`.
- La `canonical` es `/{alias}/colecciones/`.

### Página de colección

- Muestra solo los enlaces públicos. Cada uno muestra su título (o la URL si no tiene), su descripción y su imagen si las tiene, y enlaza a `UrlOriginal` con `rel="nofollow ugc noopener noreferrer"` y `target="_blank"`.
- El orden de presentación de los enlaces está definido en [«Listados»](#listados).
- Si la colección es pública pero no tiene enlaces públicos, no se muestra en público y responde `404` (ver [decisions.md → «Producto y alcance»](decisions.md#producto-y-alcance)).
- Una colección privada, una inexistente y una petición con el slug anterior cuando el renombrado ha generado uno distinto responden igual: `404`. Si el nuevo nombre genera el mismo slug, la URL se conserva y el renombrado no cambia su disponibilidad pública. Por ejemplo, renombrar «Café» como «Cafe» conserva el slug `cafe` y la misma dirección.
- El `meta title` es `Los enlaces de {alias} para {Collection.Name} en Linkubator`.
- La `meta description` es `Los enlaces favoritos de {alias} en Linkubator, para gestionarlos y no perderlos nunca. Regístrate en Linkubator para tener tu página`.
- El `h1` es `Enlaces en {Collection.Name} de {alias}`.
- La canonical se determina según [«Paginación de las páginas públicas»](#paginación-de-las-páginas-públicas).

### Información de privacidad

- La página informativa de privacidad se sirve en `/Privacy`, enlazada desde los pies de la landing, las páginas públicas de usuario y colecciones, y el área privada.
- Su contenido genérico se identifica como orientativo y pendiente de revisión, sin presentar datos de contacto ni condiciones legales no confirmadas como definitivos.
- Esta página informativa no se añade a las rutas indexables: utiliza `noindex, follow`.

## Errores controlados

- La página pública de recurso no encontrado se sirve en `/404` y se presenta también al solicitar una ruta o recurso público inexistente o no accesible, sin redirigir al navegador. Mantiene el estado HTTP `404`, utiliza `noindex, follow` y ofrece volver al inicio. Su contenido es genérico: no distingue recursos privados, inexistentes o que ya no están disponibles.

- Un error de validación vuelve a mostrar el mismo formulario, con los datos introducidos (salvo las contraseñas) y el mensaje junto a cada campo afectado (ver [requirements.md → «Accesibilidad»](requirements.md#accesibilidad)).
- Si la URL de un enlace o su imagen tiene un esquema no permitido, se muestra «Introduce una dirección que empiece por https:// o http://.» en el resumen de errores y junto al campo afectado; el identificador técnico no se muestra al usuario.
- Si se intenta hacer público un enlace al crearlo en una colección privada o al editarlo sin cambiar su colección privada, se rechaza la operación completa, no se crea ni modifica el enlace y se muestra el mensaje «No se puede hacer público un enlace de una colección privada». Si la edición cambia la colección, se aplica la regla de movimiento de [«Rutas privadas»](#rutas-privadas), sin rechazo por la solicitud simultánea de publicación.
- Una operación rechazada por una regla del dominio (por ejemplo, borrar una colección con enlaces) vuelve a mostrar la página con un mensaje que explica el motivo.
- En `/app`, las respuestas que ocultan la existencia y propiedad de recursos se definen en [«Páginas privadas»](#páginas-privadas).

## Registro de eventos

- Se registran las operaciones relevantes y errores de persistencia, validaciones.
- Los registros de inicio de sesión, tanto exitosos como fallidos, incluidos los bloqueos producidos durante el login, se identifican exclusivamente mediante HMAC-SHA-256 del email introducido, normalizado según [«Email»](#email). No incluyen `UserId` ni el email en claro, exista o no una cuenta asociada.
- Los demás eventos de autenticación (bloqueos de otras operaciones, restablecimientos de contraseña, cambios de email y eliminaciones de cuenta) y los del scraper (fallos, reintentos y bloqueos por SSRF) se identifican por `UserId`.
- Nunca se registran en claro las contraseñas de las cuentas de Linkubator, incluidas las introducidas para verificarlas, establecerlas, cambiarlas o confirmarlas, ni los tokens que Linkubator emite para sus operaciones de cuenta. Tampoco se registran los secretos y credenciales que utiliza Linkubator para operar, ni hashes innecesarios.
- Esta exclusión se aplica también a los mensajes, propiedades estructuradas, excepciones y registros de peticiones. Las peticiones que transportan un token de las operaciones de cuenta de Linkubator no registran el token ni su URL completa, tanto si el token aparece en la ruta como en la consulta.
- Los contenidos introducidos por el usuario para gestionar enlaces, colecciones y etiquetas, y los textos de búsqueda, pueden incluirse en los eventos cuando aporten información para el diagnóstico. Esto incluye nombres, descripciones, títulos y URLs de enlace o imagen, aunque su texto contenga credenciales o tokens de servicios externos, o se parezca a un secreto. La clasificación depende del uso del campo, no de la apariencia de su valor; no se exige detectar secretos en ese contenido. Esta autorización no obliga a registrar todos los campos ni todas las búsquedas.
- La autorización para registrar una URL introducida no modifica las reglas de ajuste y validación de [«Ajuste de URL»](#ajuste-de-url), [«Ajuste de `Image`»](#ajuste-de-image) y [«Validación»](#validación).
