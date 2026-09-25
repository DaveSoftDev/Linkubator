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

Reglas:

- `Alias` es obligatorio y único globalmente.
- El `Name`, `Email` y `Password` son obligatorios.
- El alias tiene entre 10 y 25 caracteres.
- El alias se genera aplicando las reglas comunes de generación de identificadores: minúsculas, eliminación de acentos, sustitución de `ç`/`ñ`, espacios a guiones, conservación de letras/números/guiones y eliminación del resto de caracteres.
- El alias puede cambiarse si el nuevo valor es válido y único.
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
- `Slug` se genera automáticamente a partir de `Name`.
- `Slug` se genera aplicando las mismas reglas que `Alias`, con una longitud máxima de 50 caracteres.
- `Slug` es invisible para el usuario y no puede cambiarse.
- Si el slug generado colisiona dentro del usuario, la operación se rechaza.
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
- `UrlNormalized` es una clave plana, no una URL válida, y solo sirve para detectar duplicados.
- `UrlNormalized` se calcula después de validar la URL original y aplicar la normalización documentada.
- No puede existir la misma `UrlNormalized` dos veces para el mismo usuario.
- Todo enlace nuevo se crea con `IsPublic = false`.
- `IsPublic` no puede hacer público un enlace de una colección privada.
- La URL es inmutable después de crear el enlace.
- Solo podrán editarse los metadatos (`Title`, `Description`, `Image`) y `IsPublic`.
- Al moverlo a otra colección, `IsPublic` se establecerá en `false`.
- El borrado será físico.

### Tag

Representa técnicamente una etiqueta del usuario.

Propiedades previstas:

- `Id`
- `UserId`
- `Name`
- `Slug`

Reglas:

- Pertenece a un único usuario.
- `Slug` se genera aplicando las mismas reglas que `Alias` y `Collection.Slug`.
- Si el texto no produce ningún carácter válido, la operación se rechaza.
- `Slug` es único dentro del usuario.
- Su relación con los enlaces se gestiona mediante `LinkTag`.

## Generación común de identificadores públicos

`User.Alias`, `Collection.Slug` y `Tag.Slug` se generan con la misma transformación:

1. Convertir el texto a minúsculas.
2. Eliminar acentos conservando la letra base.
3. Sustituir `ç` por `c` y `ñ` por `n`.
4. Sustituir espacios por guiones medios.
5. Conservar letras, números y guiones medios.
6. Eliminar el resto de caracteres.
7. Eliminar guiones duplicados y guiones en los extremos.

El resultado de `Saltó la raña al charço!!! I luego, croo` será `salto-la-rana-al-charco-i-luego-croo`.

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

Una operación privada debe verificar conjuntamente la propiedad del recurso y sus relaciones. Por ejemplo, un enlace solo podrá asociarse a una colección, aunque pueda tener varias etiquetas del mismo usuario, nunca de otro usuario.

## Visibilidad efectiva

```text
Collection.IsPublic && Link.IsPublic
```

Una colección privada bloquea siempre la visibilidad de sus enlaces.

Cuando una colección privada vuelve a ser pública, a los enlaces no se le cambia la visibilidad automáticamente.

## Restricciones de unicidad

- `User.Alias`.
- `UserId + Collection.Slug`.
- `UserId + Link.UrlNormalized`.
- `UserId + Tag.Slug`.
- `LinkId + TagId`.

La eliminación de un enlace eliminará también sus relaciones `LinkTag` y su proyección de búsqueda de forma atómica. La eliminación de un tag eliminará sus relaciones `LinkTag` dentro de la misma transacción. Esta eliminación se hará de forma manual desde la aplicación, nunca de forma automática desde la base de datos.

## Estados del scraping

La implementación deberá definir un estado equivalente a:

- Pendiente.
- Procesando.
- Completado.
- Fallido.

El enlace se conserva aunque el scraping termine sin metadatos.
