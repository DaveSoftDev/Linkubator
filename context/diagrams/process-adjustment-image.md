# Proceso de ajuste de URL para Image

Diagrama derivado de [specifications.md → «Ajuste de `Image`»](../specifications.md#ajuste-de-image). Muestra el ajuste; la validación posterior se detalla en [specifications.md → «Validación»](../specifications.md#validación).

```mermaid
flowchart TD
    start([URL de `Image` introducida])
    trim[Recortar espacios del principio y del final]
    empty{¿Quedó vacía?}
    emptyResult[Continuar sin `Image`]
    controls{¿Contiene controles o saltos de línea?}
    reject[Rechazar ajuste]
    formError[Para entrada manual, mostrar error y no guardar]
    scraperDiscard[El scraper descarta la imagen y continúa sin ella]
    protocolRelative{¿Empieza por //?}
    normalizeProtocolRelative[Anteponer `https:`]
    hasScheme{¿Tiene esquema URI explícito según la regla común?}
    allowedScheme{¿Es un esquema admitido con autoridad válida?}
    hostPort{¿Tiene forma host:puerto según la regla común?}
    addProtocol[Anteponer `https://`]
    linkURL[Resolver referencia relativa contra `Link.UrlOriginal`]
    uriResolution[Aplicar resolución URI estándar a ruta, consulta o fragmento]
    isHttps{¿El esquema es `https`?}
    isHttp{¿El esquema es `http`?}
    changeScheme[Cambiar el esquema a `https`]
    forbiddenHost{¿El host es `localhost` o una IP?}
    tooLong{¿Supera la longitud máxima?}
    adjusted([URL de `Image` ajustada, lista para validación])
    validate[Continuar a validación]

    start --> trim --> empty
    empty -->|Sí| emptyResult
    emptyResult --> formSave[Formulario permite guardar enlace sin imagen]
    emptyResult --> scraperContinue[Scraping continúa sin imagen]
    empty -->|No| controls
    controls -->|Sí| reject
    reject --> formError
    reject --> scraperDiscard
    controls -->|No| protocolRelative
    protocolRelative -->|Sí| normalizeProtocolRelative --> forbiddenHost
    protocolRelative -->|No| hasScheme
    hasScheme -->|Sí| allowedScheme
    allowedScheme -->|No| reject
    allowedScheme -->|Sí| isHttps
    isHttps -->|Sí| forbiddenHost
    isHttps -->|No| isHttp
    isHttp -->|Sí| changeScheme --> forbiddenHost
    isHttp -->|No| reject
    hasScheme -->|No| hostPort
    hostPort -->|Sí| addProtocol --> forbiddenHost
    hostPort -->|No: tratar como referencia relativa| linkURL --> uriResolution --> isHttps
    forbiddenHost -->|Sí| reject
    forbiddenHost -->|No| tooLong
    tooLong -->|Sí| reject
    tooLong -->|No| adjusted --> validate
```
