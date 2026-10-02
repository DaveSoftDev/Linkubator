# Modelo de dominio de Linkubator

Este documento describe las entidades, sus propiedades, sus relaciones y las invariantes que el dominio garantiza siempre. Los valores exactos (longitudes, algoritmos, límites, caducidades) están en [specifications.md](specifications.md); los detalles de persistencia, en [architecture.md](architecture.md).

El modelo describe el producto por fases. `Tag` y `LinkTag` forman parte del MVP1 y no se implementan en el MVP0.

> Diagrama de clases: [Clases de Linkubator](diagrams/class-diagram.md).

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
- `LastExistingAccountNoticeAt`
- `CreatedAt`

Invariantes:

- `Email` es obligatorio y único en todo el sistema, normalizado y validado según [specifications.md → «Email»](specifications.md#email).
- `EmailConfirmedAt` es `null` mientras el registro está sin completar. Un registro sin completar solo contiene el email: `Name`, `Alias` y `Password` son `null`, no tiene colecciones y no puede iniciar sesión. Al completar el registro se fijan los cuatro valores a la vez y, desde entonces, `Name`, `Alias` y `Password` son obligatorios.
- Volver a registrarse con el email de un registro sin completar no crea otra cuenta: reenvía el correo de completar registro (ver [specifications.md → «Registro»](specifications.md#registro)).
- `Name` no necesita ser único.
- `Alias` es público, mutable y único en todo el sistema. Se genera según [specifications.md → «Generación del alias»](specifications.md#generación-del-alias).
- `Password` guarda solo el hash de la contraseña, nunca la contraseña en claro. El dominio no calcula hashes: recibe el hash ya calculado por el servicio de hashing (ver [architecture.md → «Capas»](architecture.md#capas)).
- `Password` solo cambia por tres vías: completar el registro (contraseña inicial), el cambio de contraseña (tras verificar la actual) y el restablecimiento con un token de recuperación válido.
- Cambiar `Email` no lo sustituye de inmediato: el nuevo valor solo se aplica al verificarlo con un token de cambio de email. Solicitar el cambio exige verificar la contraseña actual. Solo se emite el token si el nuevo email está libre al solicitarlo, y solo se aplica si sigue libre al confirmarlo (ver [specifications.md → «Cambio de email»](specifications.md#cambio-de-email)).
- `FailedLoginAttempts` cuenta los intentos fallidos consecutivos de verificar la contraseña. Es un único contador por cuenta, compartido por todas las operaciones que verifican la contraseña. Al llegar al máximo se fija `LockoutEnd`; mientras `LockoutEnd` está en el futuro, ninguna contraseña se da por válida. Cuándo se reinicia el contador y los valores están en [specifications.md → «Bloqueo por intentos fallidos»](specifications.md#bloqueo-por-intentos-fallidos).
- `LastEmailSentAt` guarda el momento del último correo con token enviado a la cuenta, y `LastExistingAccountNoticeAt` el del último aviso de cuenta existente. Cada uno alimenta su propio límite (ver [specifications.md → «Correo»](specifications.md#correo)).
- `SecurityStamp` es un valor aleatorio que se genera al crear el usuario y se regenera al cambiar o restablecer la contraseña y al confirmar un cambio de email. Las sesiones emitidas con un sello anterior dejan de ser válidas.
- `CreatedAt` es solo informativo, para auditoría; no participa en ninguna regla.
- Un usuario con el registro completado tiene siempre al menos una colección: al completar el registro se crea su colección privada «Bandeja de entrada», y no puede borrar la última que le quede.
- Eliminar un usuario elimina todos sus datos: colecciones, enlaces, etiquetas, relaciones `LinkTag`, su índice de búsqueda y sus tokens.

## Collection

Representa una agrupación de enlaces. En la documentación funcional se llama «colección».

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
- `Slug` se genera a partir de `Name` según [specifications.md → «Generación de slugs»](specifications.md#generación-de-slugs), y se regenera cada vez que cambia `Name`.
- `Slug` es único dentro del usuario, nunca en todo el sistema. `Name` no tiene restricción propia: como dos nombres que producen el mismo slug no pueden coexistir, la unicidad real es la del slug y alcanza a nombres que solo se diferencian en mayúsculas, acentos o símbolos.
- Si `Name` no produce un slug válido o el slug colisiona dentro del usuario, al crear o al renombrar, la operación se rechaza.
- Una colección puede no tener enlaces.
- Una colección con enlaces no se puede eliminar, ni tampoco la última colección del usuario. El borrado es físico.
- `IsPublic` es `false` al crearla, salvo que el usuario la marque como pública en ese momento (ver «Público y privado»).
- `CreatedAt` solo se usa para ordenar en la página de usuario (ver [specifications.md → «Listados»](specifications.md#listados)); no participa en los listados privados.

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
- `UrlOriginal` guarda la URL ajustada y validada según [specifications.md → «Ajuste y validación»](specifications.md#ajuste-y-validación). Es inmutable: para cambiar la URL hay que eliminar el enlace y crear otro.
- `UrlNormalized` es único dentro del usuario.
- `UrlNormalized` se calcula a partir de `UrlOriginal` según [specifications.md → «Normalización para duplicados»](specifications.md#normalización-para-duplicados). Es una clave plana, no una URL válida, y solo sirve para detectar duplicados.
- `Title`, `Description` e `Image` son opcionales. `Image`, si existe, cumple las reglas de ajuste y validación de `UrlOriginal` y las de [specifications.md → «Reglas adicionales para `Image`»](specifications.md#reglas-adicionales-para-image).
- De sus propiedades, solo se pueden editar `Title`, `Description`, `Image` e `IsPublic` y moverlo a otra colección (`CollectionId`).
- `IsPublic` es `false` al crearlo, salvo que el usuario lo marque como público en ese momento y su colección sea pública. Una petición que intente crear como público un enlace de una colección privada se rechaza (ver «Público y privado»).
- `ScrapingStatus` es `null` mientras no se ha solicitado scraping (ver [specifications.md → «Estados del scraping»](specifications.md#estados-del-scraping)).
- `ScrapingAttempts` cuenta los intentos de scraping ya realizados.
- `NextScrapingAt` es la fecha y hora del siguiente intento programado; es `null` cuando no queda ninguno.
- `Link.CreatedAt` solo se usa para ordenar (ver [specifications.md → «Listados»](specifications.md#listados)).
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
- `Slug` se genera a partir de `Name` según [specifications.md → «Generación de slugs»](specifications.md#generación-de-slugs), y se regenera cada vez que cambia `Name`.
- `Slug` es único dentro del usuario, nunca en todo el sistema. `Name` no tiene restricción propia: como dos nombres que producen el mismo slug no pueden coexistir, la unicidad real es la del slug y alcanza a nombres que solo se diferencian en mayúsculas, acentos o símbolos.
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
- `Token`
- `NewEmail`
- `ExpiresAt`
- `UsedAt`
- `CreatedAt`

Invariantes:

- `Purpose` indica el uso del token: completar el registro, restablecer la contraseña o cambiar el email.
- Solo se guarda el hash del token (`Token`), que es único.
- Un token es válido si no ha caducado (`ExpiresAt`) y no se ha usado (`UsedAt` es `null`).
- `NewEmail` solo se informa en los tokens de cambio de email, y es el único sitio donde se guarda el email pendiente: `User.Email` no cambia hasta confirmarlo.
- Un token deja de ser válido cuando se usa, cuando caduca o cuando lo invalida otra operación de la cuenta (ver [specifications.md → «Tokens»](specifications.md#tokens)).
- El formato y las caducidades están en [specifications.md → «Tokens»](specifications.md#tokens).

## Relaciones

```text
User 1 ---- 0..N Collection   (1..N una vez completado el registro)
User 1 ---- 0..N Link
User 1 ---- 0..N Tag
User 1 ---- 0..N UserToken
Collection 1 ---- 0..N Link
Link 0..N ---- 0..N Tag (mediante LinkTag)
```

## Propiedad de los datos

- Toda lectura y escritura privada se limita al usuario identificado.
- Los identificadores recibidos desde la interfaz no bastan para autorizar una operación: se comprueba siempre la propiedad del recurso y la de sus relaciones.
- Se cumple siempre que `Link.UserId` es igual al `UserId` de su colección y al de cada etiqueta asociada mediante `LinkTag`. Un enlace nunca puede estar en una colección ni tener una etiqueta de otro usuario.

## Restricciones de unicidad

- `User.Alias`.
- `User.Email`.
- `UserId + Collection.Slug`.
- `UserId + Link.UrlNormalized`.
- `UserId + Tag.Slug`.
- `LinkId + TagId`.
- `UserToken.Token`.

## Diagrama entidad relación

> Diagrama ER: [Diagrama Entidad-Relación de Linkubator](diagrams/er-diagram.md).

## Público y privado

`IsPublic` significa lo mismo en colecciones y en enlaces: el elemento se muestra en la parte pública, dentro de su contenedor. Solo hay dos estados: público (`IsPublic = true`) o privado (`IsPublic = false`).

- Una colección se muestra en las páginas públicas solo si es pública y tiene al menos un enlace público. Una colección que se muestra tiene su propia página pública y aparece en la página de colecciones del usuario. Una colección privada, o una pública sin enlaces públicos, no aparece en ninguna página pública.
- Un enlace público aparece en la página pública de su colección. Un enlace privado no aparece.
- Un enlace se muestra en la parte pública solo si se cumple `Collection.IsPublic && Link.IsPublic`.

Invariantes:

- Toda colección y todo enlace nuevos son privados, salvo que el usuario los marque como públicos al crearlos.
- Un enlace de una colección privada nunca puede ser público.
- Al hacer privada una colección, todos sus enlaces pasan a ser privados.
- Al hacer pública una colección, sus enlaces siguen privados; el usuario tiene que hacerlos públicos uno a uno.
- Al mover un enlace a otra colección, pasa a ser privado, sea la nueva colección pública o privada.

> Diagrama del proceso: [Transiciones público y privado](diagrams/process-public-private.md).
