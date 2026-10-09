# Proceso de transformación común a ASCII

Diagrama derivado de [specifications.md → «Transformación común a ASCII»](../specifications.md#transformación-común-a-ascii). Lo usan la generación del alias y la generación de slugs.

```mermaid
flowchart TD
    start([Texto ya recortado])
    nfkd[Normalizar a Unicode NFKD]
    blanks{¿Contiene controles o separadores distintos del espacio normal?}
    rejectBlanks[Rechazar indicando el motivo]
    lowercase[Convertir a minúsculas]
    diacritics[Eliminar marcas diacríticas conservando la letra base]
    table[Aplicar la tabla de conversión de símbolos]
    spaces[Sustituir espacios por guiones medios]
    nonAscii{¿Quedan letras o números que no son ASCII?}
    rejectNonAscii[Rechazar indicando el motivo]
    cleanup[Eliminar otros caracteres]
    hyphens[Eliminar guiones medios duplicados e iniciales o finales]
    result([Texto ASCII])

    start --> nfkd --> blanks
    blanks -->|Sí| rejectBlanks
    blanks -->|No| lowercase --> diacritics --> table --> spaces --> nonAscii
    nonAscii -->|Sí| rejectNonAscii
    nonAscii -->|No| cleanup --> hyphens --> result
```
