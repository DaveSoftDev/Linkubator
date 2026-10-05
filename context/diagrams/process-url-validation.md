# Proceso de validación de URL

Diagrama derivado de [specifications.md → «Validación»](../specifications.md#validación). Se aplica al resultado del ajuste de URL o de `Image`; los criterios exactos están en esa sección.

```mermaid
flowchart TD
    start([URL ajustada])
    absolute{¿Es una URI absoluta válida?}
    scheme{¿El esquema está permitido?}
    host{¿El host cumple un formato permitido?}
    credentials{¿Contiene credenciales?}
    port{¿El puerto es válido o no está especificado?}
    reject[Rechazar la URL]
    valid([Validación superada])

    start --> absolute
    absolute -->|No| reject
    absolute -->|Sí| scheme
    scheme -->|No| reject
    scheme -->|Sí| host
    host -->|No| reject
    host -->|Sí| credentials
    credentials -->|Sí| reject
    credentials -->|No| port
    port -->|No| reject
    port -->|Sí| valid
```
