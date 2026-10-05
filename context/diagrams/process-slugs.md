# Proceso de generación de slugs

Diagrama derivado de [specifications.md → «Generación de slugs»](../specifications.md#generación-de-slugs).

```mermaid
flowchart TD
    start([Nombre de colección o etiqueta])
    lowercase[Convertir a minúsculas]
    diacritics[Eliminar marcas diacríticas conservando la letra base]
    table[Aplicar la tabla de conversión de símbolos]
    spaces[Sustituir espacios por guiones medios]
    nonAscii{¿Quedan letras o números que no son ASCII?}
    rejectNonAscii[Rechazar indicando el motivo]
    cleanup[Eliminar otros caracteres, guiones duplicados e iniciales o finales]
    empty{¿Resultado vacío?}
    rejectEmpty[Rechazar]
    range{¿Está dentro del rango de longitud del slug?}
    rejectRange[Rechazar sin truncar]
    slugTarget{Tipo de slug que se está validando}
    collectionCollision{¿Slug ya usado por este usuario?}
    tagCollision{¿Slug ya usado por este usuario?}
    tagOrigin{¿Etiqueta creada desde el formulario de un enlace?}
    associate[Asociar la etiqueta existente e indicar cuál]
    rejectCollision[Rechazar por colisión]
    saveSlug[Guardar slug]

    start --> lowercase
    lowercase --> diacritics --> table --> spaces --> nonAscii
    nonAscii -->|Sí| rejectNonAscii
    nonAscii -->|No| cleanup --> empty
    empty -->|Sí| rejectEmpty
    empty -->|No| range
    range -->|No| rejectRange
    range -->|Sí| slugTarget
    slugTarget -->|Colección| collectionCollision
    slugTarget -->|Etiqueta| tagCollision
    collectionCollision -->|No| saveSlug
    collectionCollision -->|Sí| rejectCollision
    tagCollision -->|No| saveSlug
    tagCollision -->|Sí| tagOrigin
    tagOrigin -->|Sí| associate
    tagOrigin -->|No| rejectCollision
```
