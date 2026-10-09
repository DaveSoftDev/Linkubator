# Proceso de generación del alias

Diagrama derivado de [specifications.md → «Generación del alias»](../specifications.md#generación-del-alias). La transformación se detalla en [Transformación común a ASCII](process-ascii-transformation.md).

```mermaid
flowchart TD
    start([Texto escrito por el usuario])
    input[Validar que el texto cumpla los requisitos]
    transform[Aplicar la transformación común a ASCII]
    rejectTransform[Rechazar indicando el motivo]
    empty{¿Resultado vacío?}
    rejectEmpty[Rechazar]
    range{¿Está dentro del rango de longitud del alias?}
    rejectRange[Rechazar sin truncar]
    preview[Mostrar vista previa del alias transformado]
    reserved{¿El alias transformado está reservado?}
    rejectReserved[Rechazar por palabra reservada]
    collision{¿Alias ya usado en el sistema?}
    rejectCollision[Rechazar por colisión]
    saveAlias[Guardar alias]

    start --> input --> transform
    transform -->|Rechazado| rejectTransform
    transform -->|Texto ASCII| empty
    empty -->|Sí| rejectEmpty
    empty -->|No| range
    range -->|No| rejectRange
    range -->|Sí| reserved
    reserved -->|Sí| rejectReserved
    reserved -->|No| preview --> collision
    collision -->|Sí| rejectCollision
    collision -->|No| saveAlias
```
