# Proceso de ajuste de URL

Diagrama derivado de [specifications.md → «Ajuste de URL»](../specifications.md#ajuste-de-url). Muestra el ajuste; la validación posterior se detalla en [specifications.md → «Validación»](../specifications.md#validación).

```mermaid
flowchart TD
    start([URL introducida])
    trim[Recortar espacios del principio y del final]
    invalid{¿Quedó vacía o contiene controles o saltos de línea?}
    rejected[Rechazar ajuste y dejar URL ajustada en `""`]
    formError[El formulario muestra error y no guarda el enlace]
    protocolRelative{¿Empieza por //?}
    addProtocolRelative[Anteponer `https:`]
    hostPort{¿Es una entrada reconocida de host:puerto?}
    hasScheme{¿Tiene esquema URI explícito según la regla común?}
    allowedScheme{¿Es un esquema admitido con autoridad válida?}
    unsupported[Rechazar ajuste y dejar URL ajustada en `""`]
    isLocalhost{¿El host es localhost?}
    addProtocolHttp[Anteponer `http`]
    addProtocolHttps[Anteponer `https`]
    tooLong{¿Supera la longitud máxima?}
    adjusted([URL ajustada, lista para validación])
    validate[Continuar a validación]

    start --> trim --> invalid
    invalid -->|Sí| rejected
    invalid -->|No| protocolRelative
    rejected --> formError
    protocolRelative -->|Sí| addProtocolRelative --> tooLong
    protocolRelative -->|No| hostPort
    hostPort -->|Sí| isLocalhost
    hostPort -->|No| hasScheme
    hasScheme -->|Sí| allowedScheme
    hasScheme -->|No| isLocalhost
    isLocalhost -->|Sí| addProtocolHttp --> tooLong
    isLocalhost -->|No| addProtocolHttps --> tooLong
    allowedScheme -->|No| unsupported --> formError
    allowedScheme -->|Sí| tooLong
    tooLong -->|Sí| rejected
    tooLong -->|No| adjusted --> validate
```
