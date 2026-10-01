# Proceso de generación de alias y slugs

Diagrama derivado de [specifications.md → «Generación de alias y slugs»](../specifications.md#generación-de-alias-y-slugs).

```mermaid
flowchart TD
    start([Texto de entrada])
    kind{¿Qué se genera?}
    aliasInput[Texto de alias escrito por el usuario]
    slugInput[Nombre de colección o etiqueta]
    slugKind{¿Es slug de colección o etiqueta?}
    slugTarget{Tipo de slug que se está validando}
    inbox{¿Es la Bandeja de entrada por defecto?}
    fixedInbox[Asignar slug fijo bandeja-de-entrada]
    lowercase[Convertir a minúsculas]
    diacritics[Eliminar marcas diacríticas conservando la letra base]
    replacements[Sustituir ç por c y ñ por n]
    spaces[Sustituir espacios por guiones medios]
    allowed[Conservar solo letras ASCII a-z, números y guiones medios]
    cleanup[Eliminar otros caracteres, guiones duplicados e iniciales o finales]
    empty{¿Resultado vacío?}
    rejectEmpty[Rechazar]
    isAlias{¿Es un alias?}
    aliasRange{¿Está dentro del rango de longitud del alias?}
    slugRange{¿Está dentro del rango de longitud del slug?}
    rejectRange[Rechazar sin truncar]
    preview[Mostrar vista previa del alias transformado]
    reserved{¿Alias reservado?}
    rejectReserved[Rechazar alias]
    aliasCollision{¿Alias ya usado en el sistema?}
    collectionCollision{¿Slug ya usado por este usuario?}
    tagCollision{¿Slug ya usado por este usuario?}
    tagOrigin{¿Etiqueta creada desde el formulario de un enlace?}
    associate[Asociar la etiqueta existente]
    rejectCollision[Rechazar por colisión]
    saveAlias[Guardar alias]
    saveSlug[Guardar slug]

    start --> kind
    kind -->|Alias| aliasInput --> lowercase
    kind -->|Slug| slugInput --> slugKind
    slugKind -->|Colección| inbox
    slugKind -->|Etiqueta| lowercase
    inbox -->|Sí| fixedInbox --> collectionCollision
    inbox -->|No| lowercase
    lowercase --> diacritics --> replacements --> spaces --> allowed --> cleanup --> empty
    empty -->|Sí| rejectEmpty
    empty -->|No| isAlias
    isAlias -->|Sí| aliasRange
    isAlias -->|No| slugRange
    aliasRange -->|No| rejectRange
    aliasRange -->|Sí| preview --> reserved
    slugRange -->|No| rejectRange
    slugRange -->|Sí| slugTarget
    slugTarget -->|Colección| collectionCollision
    slugTarget -->|Etiqueta| tagCollision
    reserved -->|Sí| rejectReserved
    reserved -->|No| aliasCollision
    aliasCollision -->|Sí| rejectCollision
    aliasCollision -->|No| saveAlias
    collectionCollision -->|No| saveSlug
    collectionCollision -->|Sí| rejectCollision
    tagCollision -->|No| saveSlug
    tagCollision -->|Sí| tagOrigin
    tagOrigin -->|Sí| associate
    tagOrigin -->|No| rejectCollision
```

El alias se transforma antes de mostrar la vista previa y se valida antes de guardarlo. Los slugs se generan a partir del nombre y no se muestran al usuario. La coincidencia de nombre dentro de un usuario se resuelve mediante la unicidad del slug generado. La lista de valores reservados, los rangos y las reglas de colisión tienen como fuente de verdad [specifications.md](../specifications.md).