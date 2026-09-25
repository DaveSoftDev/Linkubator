# Modelo de dominio de Linkubator

## Entidades

### User

Representa al propietario de los datos.

Propiedades previstas:

- `Id`
- `Name`
- `Email`
- `Alias`
- `Password`
- `CreatedAt`
- `LastLoginAt`

Reglas:

- `Alias` es obligatorio y único globalmente.
- El `Name`, `Email` y `Password` son obligatorios.
- El alias tiene entre 10 y 25 caracteres.
- El alias usa minúsculas, números y guiones.
- El alias no puede cambiarse.
- No se almacenan contraseñas en claro.

### Collection

Representa una agrupación de enlaces.

Propiedades previstas:

- `Id`
- `UserId`
- `Name`
- `Slug`
- `Description`
- `IsPublic`
- `CreatedAt`

Reglas:

- Pertenece a un único usuario.
- El `Name` y el `Slug` es obligatorio.
- `Slug` es único dentro del usuario.
- `Slug` sigue las reglas del alias de usuario, con una longitud máxima de 50 caracteres.
- `Slug` puede cambiarse si el nuevo valor es válido y único dentro del usuario.
- Una colección privada no es accesible públicamente.

Si contiene uno o más enlaces, no podrá eliminarse. Solo podrá eliminarse físicamente cuando no tenga enlaces.

### Link

Representa un enlace guardado por el usuario.

Propiedades previstas:

- `Id`
- `UserId`
- `CollectionId`
- `UrlOriginal`
- `UrlNormalized`
- `Title`
- `IsPublic`
- `Description`
- `Image`
- `ScrapingStatus`
- `Retries`
- `NextTry`
- `CreatedAt`

Reglas:

- Pertenece a un único usuario y una única colección.
- La URL original se conserva.
- La URL normalizada solo sirve para detectar duplicados.
- No puede existir la misma URL normalizada dos veces para el mismo usuario.
- `IsPublic` no puede hacer público un enlace de una colección privada.
- La URL es inmutable después de crear el enlace.
- Solo podrán editarse los metadatos (`Title`, `Description`, `Image`) y `IsPublic`.
- Al moverlo a otra colección, `IsPublic` se establecerá en `false`.
- El borrado será físico.

### Tag

Representa una etiqueta del usuario.

Propiedades previstas:

- `Id`
- `UserId`
- `Name`
- `Slug`

Reglas:

- Pertenece a un único usuario.
- Su relación con los enlaces se gestiona mediante `LinkTag`.

### LinkTag

Relación muchos a muchos entre enlaces y tags.

Propiedades:

- `LinkId`
- `TagId`

Regla:

- La pareja `LinkId + TagId` es única.
- Las altas, cambios y borrados de esta relación se realizarán dentro de la misma unidad transaccional que la operación principal.

## Relaciones

```text
User 1 ---- N Collection
User 1 ---- N Link
User 1 ---- N Tag
Collection 1 ---- N Link
Link N ---- N Tag
```

## Reglas de propiedad

Toda consulta privada debe filtrarse por el usuario actual. Los identificadores recibidos desde la interfaz no son suficientes para autorizar una operación.

Una operación privada debe verificar conjuntamente la propiedad del recurso y sus relaciones. Por ejemplo, un enlace solo podrá asociarse a una colección, anque pueda tener varios un tags del mismo usuario.

## Visibilidad efectiva

```text
Collection.IsPublic && Link.IsPublic
```

Una colección privada bloquea siempre la visibilidad de sus enlaces.

Cuando una colección privada vuelve a ser pública, a los enlaces no se le cambia la visibilidad automáticamente.

## Restricciones de unicidad

- `User.UserAlias`.
- `User.UserId + Collection.Slug`.
- `User.UserId + Link.NormalizedUrl`.
- `User.UserId + Tag.Slug`.
- `Link.LinkId + Tag.TagId`.

La eliminación de un enlace eliminará también sus relaciones `LinkTag` y su proyección de búsqueda de forma atómica. La eliminación de un tag eliminará sus relaciones `LinkTag` dentro de la misma transacción. Esta eliminación se hará de forma manual desde la aplicación, nunca de forma automática desde la base de datos.

## Estados del scraping

La implementación deberá definir un estado equivalente a:

- Pendiente.
- Procesando.
- Completado.
- Fallido.

El enlace se conserva aunque el scraping termine sin metadatos.
