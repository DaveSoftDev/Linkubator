# Proceso de generación de slugs

Diagrama derivado de [specifications.md → «Generación de slugs»](../specifications.md#generación-de-slugs). La transformación se detalla en [Transformación común a ASCII](process-ascii-transformation.md).

```mermaid
flowchart TD
    start([Nombre de colección o etiqueta])
    transform[Aplicar la transformación común a ASCII]
    rejectTransform[Rechazar indicando el motivo]
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

    start --> transform
    transform -->|Rechazado| rejectTransform
    transform -->|Texto ASCII| empty
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
