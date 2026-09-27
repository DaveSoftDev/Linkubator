# Modelo de dominio de Linkubator

Este documento describe las entidades, sus propiedades, sus relaciones y las invariantes que el dominio garantiza siempre. Los valores exactos (longitudes, algoritmos, límites, caducidades) están en specifications.md; los detalles de persistencia, en architecture.md.

## User

Representa al propietario de los datos.

Propiedades:

- `Id`
- `Email`
- `Name`
- `Alias`
- `Password`
- `EmailConfirmedAt`
- `SecurityStamp`
- `FailedLoginAttempts`
- `LockoutEnd`
- `LastEmailSentAt`
- `CreatedAt`

Invariantes:

- `Email` es obligatorio y único en todo el sistema, normalizado y validado según specifications.md → Email.
- `Name` es obligatorio y no necesita ser único.
- `Alias` es obligatorio, público, mutable y único en todo el sistema. Se genera según specifications.md → Generación de alias y slugs.
- `Password` guarda solo el hash de la contraseña, nunca la contraseña en claro. El dominio no calcula hashes: recibe el hash ya calculado por el servicio de hashing (ver architecture.md → Capas).
- `Password` solo cambia por tres vías: el registro (contraseña inicial), el cambio de contraseña (tras verificar la actual) y el restablecimiento con un token de recuperación válido.
- `EmailConfirmedAt` es `null` mientras el email no está verificado. Sin email verificado no se puede iniciar sesión. Restablecer la contraseña con un token válido también fija `EmailConfirmedAt` si era `null`.
- Cambiar `Email` no lo sustituye de inmediato: el nuevo valor solo se aplica al verificarlo con un token de cambio de email. Solicitar el cambio exige verificar la contraseña actual, y el nuevo email debe ser único al solicitarlo y de nuevo al confirmarlo.
- `FailedLoginAttempts` cuenta los intentos fallidos consecutivos de verificar la contraseña. Empieza en 0 y es un único contador por cuenta, compartido por todas las operaciones que verifican la contraseña. Al llegar al máximo se fija `LockoutEnd` y el contador vuelve a 0; también vuelve a 0 al acertar. Mientras `LockoutEnd` está en el futuro, no se verifica la contraseña ni se cuentan intentos. Los valores están en specifications.md → Bloqueo por intentos fallidos.
- `LastEmailSentAt` guarda el momento del último correo con límite enviado a la cuenta (ver specifications.md → Correos que envía la aplicación).
- `SecurityStamp` es un valor aleatorio que se genera al crear el usuario y se regenera al cambiar o restablecer la contraseña. Las sesiones emitidas con un sello anterior dejan de ser válidas.
- `CreatedAt` es solo informativo, para auditoría; no participa en ninguna regla.
- Un usuario tiene siempre al menos una colección: al crearlo se crea su colección privada «Bandeja de entrada», y no puede borrar la última que le quede.
- Eliminar un usuario elimina todos sus datos: colecciones, enlaces, etiquetas, relaciones `LinkTag`, su índice de búsqueda y sus tokens.
- Una cuenta sin verificar cuyo token de verificación ha caducado puede ser sustituida por un nuevo registro con el mismo email o el mismo alias; al sustituirla se eliminan todos sus datos.

## Collection

Representa una agrupación de enlaces.

Propiedades:

- `Id`
- `UserId`
- `Name`
- `Slug`
- `Description`
- `IsPublic`
- `CreatedAt`

Invariantes:

- `Name` y `Slug` son obligatorios. `Description` es opcional.
- `Slug` se genera a partir de `Name` según specifications.md → Generación de alias y slugs, y se regenera cada vez que cambia `Name`.
- `Slug` es único dentro del usuario, nunca en todo el sistema. Como consecuencia, `Name` también es único dentro del usuario.
- Si `Name` no produce un slug válido o el slug colisiona dentro del usuario, al crear o al renombrar, la operación se rechaza.
- Una colección puede no tener enlaces.
- Una colección con enlaces no se puede eliminar, ni tampoco la última colección del usuario. El borrado es físico.
- `IsPublic` es `false` al crearla, salvo que el usuario la marque como pública en ese momento (ver «Público y privado»).
- `CreatedAt` es solo informativo, para auditoría; no participa en ninguna regla ni se usa para ordenar.

## Link

Representa un enlace guardado por el usuario.

Propiedades:

- `Id`
- `UserId`
- `CollectionId`
- `UrlOriginal`
- `UrlNormalized`
- `Title`
- `Description`
- `Image`
- `IsPublic`
- `ScrapingStatus`
- `ScrapingAttempts`
- `NextScrapingAt`
- `CreatedAt`

Invariantes:

- Pertenece a un único usuario y a una única colección.
- `UrlOriginal` guarda la URL ajustada y validada según specifications.md → Ajuste y validación. Es inmutable: para cambiar la URL hay que eliminar el enlace y crear otro.
- `UrlNormalized` es único dentro del usuario.
- `UrlNormalized` se calcula a partir de `UrlOriginal` según specifications.md → Normalización para duplicados. Es una clave plana, no una URL válida, y solo sirve para detectar duplicados.
- `Title`, `Description` e `Image` son opcionales. `Image`, si existe, cumple las mismas reglas de ajuste y validación que `UrlOriginal`.
- De sus propiedades, solo se pueden editar `Title`, `Description`, `Image` e `IsPublic`. Además, se pueden cambiar sus etiquetas (`LinkTag`) y moverlo a otra colección (`CollectionId`).
- `IsPublic` es `false` al crearlo, salvo que el usuario lo marque como público en ese momento y su colección sea pública (ver «Público y privado»).
- `ScrapingStatus` es `null` mientras no se ha solicitado scraping (ver «Estados del scraping»).
- `ScrapingAttempts` cuenta los intentos de scraping ya realizados.
- `NextScrapingAt` es la fecha y hora del siguiente intento programado; es `null` cuando no queda ninguno.
- `CreatedAt` solo se usa para ordenar.
- El borrado es físico y elimina también sus relaciones `LinkTag` y su entrada en el índice de búsqueda.

## Tag

Representa técnicamente una etiqueta del usuario. En la documentación funcional se llama «etiqueta».

Propiedades:

- `Id`
- `UserId`
- `Name`
- `Slug`
- `CreatedAt`

Invariantes:

- `Name` y `Slug` son obligatorios.
- `Slug` se genera a partir de `Name` según specifications.md → Generación de alias y slugs, y se regenera cada vez que cambia `Name`.
- `Slug` es único dentro del usuario, nunca en todo el sistema. Como consecuencia, `Name` también es único dentro del usuario.
- Si `Name` no produce un slug válido o el slug colisiona dentro del usuario, al crear o al renombrar, la operación se rechaza. Excepción: al crear una etiqueta desde el formulario de un enlace, si su slug coincide con el de una etiqueta existente, se asocia esa etiqueta en lugar de rechazar la operación.
- Una etiqueta puede no tener enlaces.
- Solo se puede eliminar una etiqueta sin enlaces asociados. El borrado es físico.
- `CreatedAt` es solo informativo, para auditoría; no participa en ninguna regla.

## LinkTag

Relación muchos a muchos entre enlaces y etiquetas.

Propiedades:

- `LinkId`
- `TagId`

Invariantes:

- La pareja `LinkId + TagId` es única y es su clave: `LinkTag` no tiene `Id` propio.

## UserToken

Representa un token de un solo uso enviado por correo.

Propiedades:

- `Id`
- `UserId`
- `Purpose`
- `TokenHash`
- `NewEmail`
- `ExpiresAt`
- `UsedAt`
- `CreatedAt`

Invariantes:

- `Purpose` indica el uso del token: verificación de email, restablecimiento de contraseña o cambio de email.
- Solo se guarda el hash del token (`TokenHash`), que es único.
- Un token es válido si no ha caducado (`ExpiresAt`) y no se ha usado (`UsedAt` es `null`).
- `NewEmail` solo se informa en los tokens de cambio de email, y es el único sitio donde se guarda el email pendiente: `User.Email` no cambia hasta confirmarlo.
- Al emitir un token, se invalidan los anteriores sin usar del mismo usuario y propósito. Al cambiar o restablecer la contraseña, se invalidan también los tokens de restablecimiento y de cambio de email pendientes.
- El formato y las caducidades están en specifications.md → Correo y tokens → Tokens.

## Relaciones

```text
User 1 ---- 1..N Collection
User 1 ---- N Link
User 1 ---- N Tag
User 1 ---- N UserToken
Collection 1 ---- N Link
Link N ---- N Tag (mediante LinkTag)
```

## Propiedad de los datos

- Toda lectura y escritura privada se limita al usuario identificado.
- Los identificadores recibidos desde la interfaz no bastan para autorizar una operación: se comprueba siempre la propiedad del recurso y la de sus relaciones.
- Se cumple siempre que `Link.UserId` es igual al `UserId` de su colección y al de cada etiqueta asociada mediante `LinkTag`. Un enlace nunca puede estar en una colección ni tener una etiqueta de otro usuario.

## Público y privado

`IsPublic` significa lo mismo en colecciones y en enlaces: el elemento se muestra en la parte pública, dentro de su contenedor. Solo hay dos estados: público (`IsPublic = true`) o privado (`IsPublic = false`).

- Una colección pública aparece en la página pública del usuario y tiene su propia página pública. Una colección privada no aparece en ninguna página pública.
- Un enlace público aparece en la página pública de su colección. Un enlace privado no aparece.
- Un enlace se muestra en la parte pública solo si se cumple `Collection.IsPublic && Link.IsPublic`.

Invariantes:

- Toda colección y todo enlace nuevos son privados, salvo que el usuario los marque como públicos al crearlos.
- Un enlace de una colección privada nunca puede ser público.
- Al hacer privada una colección, todos sus enlaces pasan a ser privados.
- Al hacer pública una colección, sus enlaces siguen privados; el usuario tiene que hacerlos públicos uno a uno.
- Al mover un enlace a otra colección, pasa a ser privado, sea la nueva colección pública o privada.

## Restricciones de unicidad

- `User.Alias`.
- `User.Email`.
- `UserId + Collection.Slug`.
- `UserId + Link.UrlNormalized`.
- `UserId + Tag.Slug`.
- `LinkId + TagId`.
- `UserToken.TokenHash`.

## Estados del scraping

`ScrapingStatus` toma uno de estos valores:

- `null`: no se ha solicitado scraping, como en los enlaces creados antes de la etapa de scraping.
- Pendiente.
- Procesando.
- Completado.
- Fallido.
- ReintentosCompletados.

El enlace se conserva aunque el scraping termine sin metadatos.
