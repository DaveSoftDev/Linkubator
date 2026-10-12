# Ficha derivada: Collection

> Estado: vista de revisión derivada dentro de S3.5. No es una fuente de reglas ni sustituye la aprobación final de S3.6.

## Fuentes consultadas

- [domain-model.md → «Collection»](../../../context/domain-model.md#collection)
- [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado)
- [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos)
- [requirements.md → «Colecciones»](../../../context/requirements.md#colecciones)
- [requirements.md → «Público y privado»](../../../context/requirements.md#público-y-privado)
- [specifications.md → «Generación de slugs»](../../../context/specifications.md#generación-de-slugs)
- [specifications.md → «Listados»](../../../context/specifications.md#listados)
- [specifications.md → «Páginas públicas»](../../../context/specifications.md#páginas-públicas)
- [domain-model.md → «Restricciones de unicidad»](../../../context/domain-model.md#restricciones-de-unicidad)

## Propiedades y relaciones

`Collection` representa una agrupación de enlaces del usuario y, en la documentación funcional, se llama «colección». Su dominio se describe en [domain-model.md → «Collection»](../../../context/domain-model.md#collection) con estas propiedades: `Id`, `UserId`, `Name`, `Slug`, `Description`, `IsPublic` y `CreatedAt`.

- `Name` y `Slug` son obligatorios; `Description` es opcional.
- `Slug` se genera a partir de `Name` según [specifications.md → «Generación de slugs»](../../../context/specifications.md#generación-de-slugs), y se regenera al cambiar el nombre.
- `Slug` es único dentro del usuario; la restricción real no es global sino por `UserId + Collection.Slug`.
- La colección pertenece a un usuario y se usa como contenedor de enlaces; la relación está declarada en [domain-model.md → «Relaciones»](../../../context/domain-model.md#relaciones).

Las propiedades y reglas fuente están en [domain-model.md#L69-L94](../../../context/domain-model.md#L69); la unicidad y generación del slug se especifican en [domain-model.md#L179-L187](../../../context/domain-model.md#L179) y [specifications.md#L163-L231](../../../context/specifications.md#L163).

## Reglas de unicidad y validación

- No puede haber dos colecciones del mismo usuario con nombres equivalentes, aunque solo difieran en mayúsculas, acentos o símbolos. La unicidad efectiva la define el slug.
- Si `Name` no produce un slug válido o ya existe en el usuario, la operación se rechaza.
- La colección puede no tener enlaces.
- No se permite eliminar una colección con enlaces activos.
- La última colección del usuario tampoco puede eliminarse; la operación se rechaza.
- El borrado es físico; el dominio prohíbe eliminar una colección con enlaces y no especifica borrar contenido asociado como consecuencia de eliminarla.

Las reglas de ciclo de vida constan en [domain-model.md#L79-L94](../../../context/domain-model.md#L79). Los requisitos describen creación, edición, búsqueda y eliminación en [requirements.md#L32-L49](../../../context/requirements.md#L32).

## Páginas públicas y visibilidad

La visibilidad de la colección se modela con `IsPublic` y se describe en [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado):

- Una colección aparece en páginas públicas solo si es pública y tiene al menos un enlace público.
- Una colección privada, o una pública sin enlaces públicos, no aparece en ninguna página pública.
- Una colección vacía puede ser pública pero no aparecerá en páginas públicas hasta que tenga enlaces públicos.
- Cuando una colección se vuelve privada, todos sus enlaces pasan a ser privados.
- Cuando una colección se vuelve pública, sus enlaces permanecen privados hasta que el usuario los marca de forma individual.

La documentación de producto recoge estas posibilidades en [requirements.md → «Colecciones»](../../../context/requirements.md#colecciones) y [requirements.md → «Público y privado»](../../../context/requirements.md#público-y-privado).

La regla fuente de privacidad está en [domain-model.md#L193-L218](../../../context/domain-model.md#L193); la visibilidad en las páginas públicas se describe en [requirements.md#L41-L49](../../../context/requirements.md#L41) y [requirements.md#L81-L83](../../../context/requirements.md#L81).

## Relación con enlaces y propiedad

- La colección es contenedora de enlaces, y cada enlace pertenece a una única colección.
- La política de propiedad se aplica con `UserId`: un enlace nunca puede estar en una colección de otro usuario, y la propiedad del recurso debe verificarse siempre, según [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos).
- Cuando se mueve un enlace a otra colección, el enlace pasa a ser privado aunque la colección destino sea pública; la regla se enuncia en [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado).

La creación de la «Bandeja de entrada» al completar el registro y su carácter de colección normal quedan en [requirements.md#L11-L14](../../../context/requirements.md#L11); su relación con el usuario también está en [domain-model.md#L36-L40](../../../context/domain-model.md#L36). La búsqueda de colecciones se recoge en [requirements.md#L85-L95](../../../context/requirements.md#L85), y la búsqueda/listados exactos en [specifications.md#L622-L669](../../../context/specifications.md#L622). El cambio de slug y la dirección pública anterior se describen en [requirements.md#L45-L49](../../../context/requirements.md#L45) y [specifications.md#L769-L789](../../../context/specifications.md#L769).

## Observaciones finales

- **Contradicción documental detectada:** `requirements.md` limita la equivalencia de nombres a mayúsculas y acentos ([requirements.md#L41-L43](../../../context/requirements.md#L41)), mientras `domain-model.md` incluye también símbolos ([domain-model.md#L76-L78](../../../context/domain-model.md#L76)); la conversión de símbolos y colisiones por slug se especifica en [specifications.md#L163-L231](../../../context/specifications.md#L163). No se resuelve aquí; requiere armonizar el documento propietario correspondiente.
- La expresión «nombres equivalentes» depende de la normalización del slug, así que el hallazgo anterior debe resolverse antes de tratar ambas descripciones como idénticas.
- La operación de publicación/privacidad y la regla de nombre equivalente son las más sensibles de revisión, porque implican comparaciones por texto normalizado y por actos de visibilidad.
- La comprobación final con el código y con la aceptación de invariantes corresponde a S3.6, según [spec.md → «Alcance»](spec.md#alcance).
