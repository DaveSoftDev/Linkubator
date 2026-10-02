# Proceso de generación del alias

Diagrama derivado de [specifications.md → «Generación del alias»](../specifications.md#generación-del-alias).

```mermaid
flowchart TD
    start([Texto escrito por el usuario])
    input[El campo solo admite letras, números y guiones medios]
    lowercase[Convertir a minúsculas]
    diacritics[Eliminar marcas diacríticas conservando la letra base]
    spaces[Sustituir espacios por guiones medios]
    allowed[Eliminar todo lo que no sea una letra ASCII, un número o un guión medio]
    hyphens[Eliminar guiones medios duplicados e iniciales o finales]
    empty{¿Resultado vacío?}
    rejectEmpty[Rechazar]
    range{¿Está dentro del rango de longitud del alias?}
    rejectRange[Rechazar sin truncar]
    preview[Mostrar vista previa del alias transformado]
    reserved{¿Alias reservado?}
    rejectReserved[Rechazar alias]
    collision{¿Alias ya usado en el sistema?}
    rejectCollision[Rechazar por colisión]
    saveAlias[Guardar alias]

    start --> input --> lowercase --> diacritics --> spaces --> allowed --> hyphens --> empty
    empty -->|Sí| rejectEmpty
    empty -->|No| range
    range -->|No| rejectRange
    range -->|Sí| preview --> reserved
    reserved -->|Sí| rejectReserved
    reserved -->|No| collision
    collision -->|Sí| rejectCollision
    collision -->|No| saveAlias
```

El alias se transforma antes de mostrar la vista previa y se valida antes de guardarlo. No hay tablas de conversión: lo que no es una letra ASCII, un número o un guión medio se elimina. La lista de valores reservados, el rango y la regla de colisión tienen como fuente de verdad [specifications.md](../specifications.md).