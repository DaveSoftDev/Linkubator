# Proceso de ajuste y validación de URL

Diagrama derivado de [specifications.md → «Ajuste y validación»](../specifications.md#ajuste-y-validación). El flujo corresponde a `Link.UrlOriginal`.

```mermaid
flowchart TD
    start([URL introducida])
    trim[Recortar espacios iniciales y finales]
    empty{¿Quedó vacía o contiene controles o saltos de línea?}
    reject[Rechazar la URL]
    protocolRelative{¿Empieza por //?}
    explicitScheme{¿Tiene esquema seguido de ://?}
    allowedScheme{¿El esquema es HTTP o HTTPS?}
    suspiciousPrefix{¿Tiene prefijo texto: que no sea // ni puerto numérico?}
    localhost{¿El host es localhost?}
    addHttp[Anteponer http://]
    addHttps[Anteponer https://]
    tooLong{¿La URL ajustada supera la longitud máxima?}
    absoluteUri{¿Es una URI absoluta válida?}
    validScheme{¿El esquema es HTTP o HTTPS?}
    validHost{¿El host cumple el formato permitido?}
    credentials{¿Contiene credenciales?}
    validPort{¿El puerto, si existe, es válido?}
    accepted([Guardar como Link.UrlOriginal])

    start --> trim --> empty
    empty -->|Sí| reject
    empty -->|No| protocolRelative
    protocolRelative -->|Sí| reject
    protocolRelative -->|No| explicitScheme
    explicitScheme -->|Sí| allowedScheme
    allowedScheme -->|No| reject
    allowedScheme -->|Sí| tooLong
    explicitScheme -->|No| suspiciousPrefix
    suspiciousPrefix -->|Sí| reject
    suspiciousPrefix -->|No| localhost
    localhost -->|Sí| addHttp --> tooLong
    localhost -->|No| addHttps --> tooLong
    tooLong -->|Sí| reject
    tooLong -->|No| absoluteUri
    absoluteUri -->|No| reject
    absoluteUri -->|Sí| validScheme
    validScheme -->|No| reject
    validScheme -->|Sí| validHost
    validHost -->|No| reject
    validHost -->|Sí| credentials
    credentials -->|Sí| reject
    credentials -->|No| validPort
    validPort -->|No| reject
    validPort -->|Sí| accepted
```

El formato de host permitido, la longitud máxima y las diferencias de `Image` (esquema, hosts y URL relativas al protocolo) están en [specifications.md → «Ajuste y validación»](../specifications.md#ajuste-y-validación) y [«Reglas adicionales para `Image`»](../specifications.md#reglas-adicionales-para-image), junto con sus ejemplos.
