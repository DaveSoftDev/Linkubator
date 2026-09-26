# Modelo de dominio de Linkubator

## Entidades

### User

Representa al propietario de los datos.

Propiedades previstas:

- `Id`
- `Email`
- `Name`
- `Alias`
- `Password`
- `CreatedAt`

Reglas:

- `Email` es obligatorio y único globalmente.
- `Alias` es obligatorio y único globalmente.
- El `Name` es también obligatorio.
- El `Alias` tiene cómo máximo 25 caracteres.
- El `Alias` se genera aplicando las reglas comunes de generación especificadas en decisions.md (Generación de alias y slugs).
- El `Alias` puede cambiarse si el nuevo valor es válido y único globalmente.
- Si el usuario no proporciona un `Alias` que después de su generación tenga una longitud menor que 10 caracteres, la operación se rechaza.
- No se almacenan contraseñas en claro.
- `Password` tiene setter privado; solo se muta a través de un método propio de la entidad (p. ej. `CambiarContraseña`) que verifica la contraseña actual antes de aceptar y hashear la nueva.
- La contraseña en claro debe tener entre 10 y 50 caracteres antes de hashearse; se acepta cualquier carácter válido, sin exigir combinación obligatoria de tipos. Existe una sugerencia del porqué explicado en `Validación de contraseñas comprometidas` de la sección `Fuera de alcance del MVP0` de decisions.md.
- `CreatedAt` es únicamente informativo, para auditoría; no participa en ninguna regla de negocio ni caso de uso.

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

- El `Name` y el `Slug` son obligatorios.
- `Name` es único dentro del usuario, como consecuencia directa de la unicidad de `Slug`.
- Si el `Name` no produce ningún carácter válido, la operación se rechaza.
- `Slug` es único dentro del usuario.
- `Slug` es invisible para el usuario y no puede cambiarse.
- `Slug` se genera automáticamente y de forma determinista a partir de `Name`; aplicando las reglas comunes de generación especificadas en decisions.md (Generación de alias y slugs).
- Si el slug generado colisiona dentro del usuario, la operación se rechaza.
- Una colección puede existir sin tener ningún enlace asociado.
- Una colección privada no es accesible públicamente.
- El borrado de una colección es físico.
- `CreatedAt` es únicamente informativo, para auditoría; no participa en ninguna regla de negocio ni caso de uso.

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
- La URL original se conserva y es inmutable después de crear el enlace.
- `UrlNormalized` es único para el mismo usuario.
- `UrlNormalized` es una clave plana, no una URL válida, y solo sirve para detectar duplicados.
- `UrlNormalized` se calcula aplicando la normalización documentada en decisions.md (Normalización de URLs para duplicados).
- Todo enlace nuevo se crea con `IsPublic = false`.
- `IsPublic` no puede hacerse público en un enlace de una colección privada.
- Solo podrán editarse los metadatos (`Title`, `Description`, `Image`) e `IsPublic`.
- `Title`, `Description` e `Image` son nullable: permanecen sin valor si el scraping no obtiene datos y el usuario no los edita manualmente.
- `ScrapingStatus` estado de la acción de scraping (Pendiente, Procesando, Completado, Fallido).
- `Retries` cuenta los intentos de scraping ya realizados, con un máximo de 3. 
- `NextTry` es la fecha y hora programada del siguiente intento; será `null` cuando los reintentos se hayan agotado.
- Al moverlo a otra colección, `IsPublic` se establecerá en `false`.
- El borrado será físico.

### Tag

Representa técnicamente una etiqueta del usuario.

Propiedades previstas:

- `Id`
- `UserId`
- `Name`
- `Slug`
- `CreatedAt`

Reglas:

- El `Name` y el `Slug` son obligatorios.
- `Name` es único dentro del usuario, como consecuencia directa de la unicidad de `Slug`.
- Si el `Name` no produce ningún carácter válido, la operación se rechaza.
- `Slug` es único dentro del usuario.
- `Slug` es invisible para el usuario y no puede cambiarse.
- `Slug` se genera automáticamente y de forma determinista a partir de `Name`; aplicando las reglas comunes de generación especificadas en decisions.md (Generación de alias y slugs).
- Si el slug generado colisiona dentro del usuario, la operación se rechaza.
- Un tag puede existir sin tener ningún enlace asociado.
- Su relación con los enlaces se gestiona mediante `LinkTag`.
- Solo se podrán eliminar etiquetas vacías (sin enlaces asociados). El borrado es físico; como salvaguarda, si un tag tuviera relaciones `LinkTag`, se eliminarían de forma atómica en la misma transacción.
- `CreatedAt` es únicamente informativo, para auditoría; no participa en ninguna regla de negocio ni caso de uso.

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

Toda consulta privada debe filtrarse por el usuario actual. 

Los identificadores recibidos desde la interfaz no son suficientes para autorizar una operación.

Una operación privada debe verificar conjuntamente la propiedad del recurso y sus relaciones. Por ejemplo, un enlace solo podrá asociarse a una colección, aunque pueda tener varias etiquetas del mismo usuario, nunca de otro usuario.

## Visibilidad efectiva

```text
Collection.IsPublic && Link.IsPublic
```

Una colección privada bloquea siempre la visibilidad de sus enlaces.

Al hacer privada una colección pública, todos los enlaces de esa colección pasan a `IsPublic = false`. Al volver a pública una colección privada, los enlaces permanecen sin publicar; el usuario deberá publicar los enlaces explícitamente.

## Restricciones de unicidad

- `User.Alias`.
- `User.Email`.
- `UserId + Collection.Slug`.
- `UserId + Link.UrlNormalized`.
- `UserId + Tag.Slug`.
- `LinkId + TagId`.

La eliminación de un enlace eliminará también sus relaciones `LinkTag` dentro de la misma transacción y su proyección de búsqueda de forma atómica. Solo se podrán eliminar etiquetas vacías (sin enlaces asociados); como salvaguarda, si un tag tuviera relaciones `LinkTag`, se eliminarían de forma atómica en la misma transacción. Esta eliminación se hará de forma manual desde la aplicación, nunca de forma automática desde la base de datos.

## Estados del scraping

La implementación deberá definir un estado equivalente a:

- Pendiente.
- Procesando.
- Completado.
- Fallido.

El enlace se conserva aunque el scraping termine sin metadatos.

