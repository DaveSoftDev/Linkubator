# Modelo de dominio

## Entidades

### User

Representa al propietario de los datos.

Propiedades previstas:

- `Id`
- `Name`
- `Email`
- `UserAlias`
- `PasswordHash`

Reglas:

- `UserAlias` es obligatorio y único globalmente.
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
- `Slug` es único dentro del usuario.
- No tiene `UpdatedAt`.
- Una colección privada no es accesible públicamente.

### Link

Representa un enlace guardado por el usuario.

Propiedades previstas:

- `Id`
- `UserId`
- `CollectionId`
- `OriginalUrl`
- `NormalizedUrl`
- `IsPublished`
- `Title`
- `Description`
- `OgImage`
- Estado del scraping.
- Número de intentos.
- Fecha del próximo intento.
- `CreatedAt`

Reglas:

- Pertenece a un único usuario y una única colección.
- La URL original se conserva.
- La URL normalizada solo sirve para detectar duplicados.
- No puede existir la misma URL normalizada dos veces para el mismo usuario.
- `IsPublished` no puede hacer público un enlace de una colección privada.
- El borrado será físico.

No se incluirán inicialmente notas, favicon, favoritos, archivado ni `UpdatedAt`.

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

Una operación privada debe verificar conjuntamente la propiedad del recurso y sus relaciones. Por ejemplo, un enlace solo podrá asociarse a una colección y a un tag del mismo usuario.

## Visibilidad efectiva

```text
Collection.IsPublic && Link.IsPublished
```

Una colección privada bloquea siempre la visibilidad de sus enlaces.

Cuando una colección privada vuelve a ser pública, los enlaces no se publican automáticamente.

## Restricciones de unicidad

- `User.UserAlias`.
- `UserId + Collection.Slug`.
- `UserId + Link.NormalizedUrl`.
- `UserId + Tag.Slug`.
- `LinkId + TagId`.

## Estados del scraping

La implementación deberá definir un estado equivalente a:

- Pendiente.
- Procesando.
- Completado.
- Fallido.

El enlace se conserva aunque el scraping termine sin metadatos.
