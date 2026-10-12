# Plan tÃ©cnico: S3.1 â€” Entidades `User`, `Collection` y `Link`

## Estado y aprobaciÃ³n del plan

- Estado: aprobado por DLG el 12-X-2026.
- Aprobaciones:
	- EspecificaciÃ³n: [spec.md](spec.md), aprobada por DLG el 12-X-2026 (Â«CorregidoÂ»).
	- Plan: aprobado por DLG el 12-X-2026 (Â«Consensuados los stoppersÂ»).

## DiseÃ±o de implementaciÃ³n

### Componentes y contratos

Todo el cÃ³digo vive en `Linkubator.Domain`, sin referencias a otros proyectos ni a paquetes nuevos ([architecture.md â†’ Â«DomainÂ»](../../../context/architecture.md#domain)). Las clases se organizan por responsabilidad y con namespace alineado con la ruta; los usings de namespaces internos usan alias.

| Componente | UbicaciÃ³n propuesta | Responsabilidad | Reglas fuente |
| --- | --- | --- | --- |
| `BaseEntity` | `Entities/Bases/` | Clase base con `Id` y `CreatedAt`, heredada por `User`, `Collection` y `Link`. | [architecture.md â†’ Â«DomainÂ»](../../../context/architecture.md#domain), [decisions.md â†’ Â«TecnologÃ­aÂ»](../../../context/decisions.md#tecnologÃ­a) |
| `EmailPolicy` | `Policies/` | Normaliza (recorte y minÃºsculas) y valida un email: obligatorio, longitud mÃ¡xima y sintaxis `addr-spec`. Devuelve el email normalizado o lanza una excepciÃ³n de dominio. | [specifications.md â†’ Â«EmailÂ»](../../../context/specifications.md#email), [Â«Longitudes mÃ¡ximasÂ»](../../../context/specifications.md#longitudes-mÃ¡ximas) |
| ExcepciÃ³n de email invÃ¡lido | `Exceptions/` | ExcepciÃ³n especÃ­fica de dominio para una sintaxis de email no vÃ¡lida, derivada de `DomainException`. | [decisions.md â†’ Â«TecnologÃ­aÂ»](../../../context/decisions.md#tecnologÃ­a) (una excepciÃ³n especÃ­fica por regla) |
| `User` | `Entities/` | Usuario con el registro completado. Se crea con un mÃ©todo de fÃ¡brica estÃ¡tico. | [domain-model.md â†’ Â«UserÂ»](../../../context/domain-model.md#user) |
| `Collection` | `Entities/` | ColecciÃ³n privada con renombrado y cambio de descripciÃ³n. | [domain-model.md â†’ Â«CollectionÂ»](../../../context/domain-model.md#collection) |
| `Link` | `Entities/` | Enlace privado con ediciÃ³n de `Title`, `Description` e `Image`. | [domain-model.md â†’ Â«LinkÂ»](../../../context/domain-model.md#link) |

**Contratos comunes de las entidades**

- Las propiedades tienen setter no pÃºblico y no hay constructor pÃºblico que permita crear una entidad sin validar. La creaciÃ³n y la ediciÃ³n pasan por mÃ©todos que validan todo antes de asignar ([architecture.md â†’ Â«Principios de DiseÃ±oÂ»](../../../context/architecture.md#principios-de-diseÃ±o): encapsulamiento e inmutabilidad).
- Los textos opcionales usan `UserTextPolicy.TrimToNull`; los obligatorios, `UserTextPolicy.ThrowIfNullOrWhiteSpace` tras recortar; las longitudes se comprueban con `UserTextPolicy.IsWithinMaximumLength`, que cuenta puntos de cÃ³digo. Los lÃ­mites se leen de [Â«Longitudes mÃ¡ximasÂ»](../../../context/specifications.md#longitudes-mÃ¡ximas) y no se copian en este plan.
- Las entradas invÃ¡lidas lanzan la excepciÃ³n de dominio que ya usan las polÃ­ticas existentes: `NotValidInputException`, `TextExceedsMaximumLengthException`, `TextBelowMinimumLengthException` y las de URL. Solo se aÃ±ade la excepciÃ³n de email invÃ¡lido.
- `Id` y `CreatedAt` quedan sin asignar al crear la entidad; los asigna la persistencia (ver decisiÃ³n D01).

**`User`**

- MÃ©todo de fÃ¡brica con: email, nombre, candidato a alias, hash de contraseÃ±a y fecha de confirmaciÃ³n del email. Aplica `EmailPolicy`, `AliasPolicy.Generate` y las reglas de texto de `Name`.
- `Password` se recibe como hash ya calculado: solo se exige que estÃ© informado. No se validan su formato ni sus reglas de contraseÃ±a en claro (excluidas por la spec).
- `EmailConfirmedAt` es obligatorio: un valor por defecto se rechaza con `NotValidInputException`.
- `SecurityStamp` se genera al crear el usuario con un valor aleatorio de un generador criptogrÃ¡fico (decisiÃ³n D03).
- `FailedLoginAttempts` es `0`; `LockoutEnd`, `LastEmailSentAt` y `LastExistingAccountNoticeAt` son `null`. No hay mÃ©todos que los cambien.
- No hay mÃ©todos de cambio de `Email`, `Alias`, `Password` ni `SecurityStamp`: su comportamiento pertenece a etapas posteriores.

**`Collection`**

- MÃ©todo de fÃ¡brica con `UserId`, nombre y descripciÃ³n. `Slug` se obtiene de `SlugPolicy.Generate(Name)` con el nombre ya recortado. `IsPublic` es `false`.
- MÃ©todo de renombrado, que regenera `Slug`, y mÃ©todo de cambio de descripciÃ³n.
- No hay mÃ©todo que cambie `IsPublic`, `UserId` ni `CreatedAt`.

**`Link`**

- MÃ©todo de fÃ¡brica con `UserId`, `CollectionId`, `UrlOriginal`, `Title`, `Description` e `Image`.
- `UrlOriginal`: recortar, `LinkUrlPolicy.Adjust`, `UrlValidationPolicy.Validate`; `UrlNormalized` se calcula con `UrlNormalizationPolicy.Normalize` sobre ese resultado.
- `Image`: recortar; si queda vacÃ­a es `null`; si no, `ImageUrlPolicy.Adjust` contra la `UrlOriginal` ajustada y despuÃ©s `UrlValidationPolicy.Validate`.
- `IsPublic` es `false` y `ScrapingStatus` es `NotRequested`.
- MÃ©todo de ediciÃ³n de `Title`, `Description` e `Image`, con las mismas reglas que la creaciÃ³n. Recibe los tres valores y los sustituye todos.
- No hay setter ni mÃ©todo que cambie `UrlOriginal`, `UrlNormalized`, `IsPublic`, `CollectionId` ni `UserId`.

### Flujos internos

1. **CreaciÃ³n.** Recortar cada texto, validar todos los valores, calcular las derivadas (alias, slug, URL ajustada, clave normalizada, imagen ajustada) y asignar las propiedades al final. Si cualquier paso lanza, no se devuelve entidad.
2. **EdiciÃ³n y renombrado.** Calcular los valores nuevos en variables locales y asignarlos solo cuando todos son vÃ¡lidos, de modo que un error deja la entidad como estaba.
3. **Errores.** Las excepciones de las polÃ­ticas se propagan sin envolver. Las entidades no capturan excepciones ni registran logs.

### Persistencia y dependencias

- No hay persistencia, transacciones, integraciones ni paquetes nuevos. Domain no conoce SQLite ni Dapper.
- S5 es la propietaria del mapeo a base de datos y de cÃ³mo materializa las entidades con setters no pÃºblicos (ver riesgo R01).
- S3.2 y S3.3 extienden estas entidades con las invariantes de propiedad y de pÃºblico y privado (ver riesgo R02).
- Las unicidades de `Alias`, `Email`, `Collection.Slug` y `Link.UrlNormalized` se aplican en Application e Infrastructure (S4 y S5).

### GarantÃ­as tÃ©cnicas

- Las entidades solo exponen caminos que validan: no existe un camino pÃºblico para cambiar `UrlOriginal`, `UrlNormalized`, `IsPublic` ni `CollectionId`, ni para dejar `IsPublic` en `true` ([domain-model.md â†’ Â«LinkÂ»](../../../context/domain-model.md#link) y [Â«PÃºblico y privadoÂ»](../../../context/domain-model.md#pÃºblico-y-privado)).
- Una operaciÃ³n rechazada no deja la entidad parcialmente modificada.
- `UserId` y `CollectionId` se reciben tal cual: S3.1 no autoriza nada con ellos ([domain-model.md â†’ Â«Propiedad de los datosÂ»](../../../context/domain-model.md#propiedad-de-los-datos) es de S3.2).
- ContraseÃ±as y tokens no se registran: S3.1 no registra datos y `User` solo conserva el hash recibido ([specifications.md â†’ Â«ContraseÃ±asÂ»](../../../context/specifications.md#contraseÃ±as)).
- Domain sigue sin referenciar otros proyectos, comprobado por los tests de arquitectura de S0.7.

## Decisiones tÃ©cnicas y riesgos

| DecisiÃ³n o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobaciÃ³n | Impacto y resoluciÃ³n necesaria |
| --- | --- | --- | --- |
| D01. `Id` y `CreatedAt` de `BaseEntity` quedan sin asignar al crear la entidad (`Id = 0`, `CreatedAt` sin valor) y los asigna la persistencia, segÃºn [specifications.md â†’ Â«RegistroÂ»](../../../context/specifications.md#registro). Tipos propuestos: entero de 64 bits para `Id` y `DateTime` en UTC para `CreatedAt`. | Confirmado | DecisiÃ³n de DLG, 12-X-2026; [spec.md â†’ P03](spec.md#bloqueos-y-preguntas-pendientes) | Las firmas de creaciÃ³n no reciben `Id` ni `CreatedAt`. |
| D02. Las entidades se crean con mÃ©todos de fÃ¡brica estÃ¡ticos y setters no pÃºblicos, no con setters pÃºblicos. | Confirmado | DecisiÃ³n de DLG, 12-X-2026; [architecture.md â†’ Â«Principios de DiseÃ±oÂ»](../../../context/architecture.md#principios-de-diseÃ±o) | Garantiza CA11 y CA13. Si Dapper no puede materializar alguna propiedad (R01), se harÃ¡ pÃºblico su setter en S5, solo para las propiedades afectadas. |
| D03. `SecurityStamp` se genera en `User` con un generador criptogrÃ¡fico, a partir de 32 bytes aleatorios en una cadena segura para URL. Las fuentes solo exigen un valor aleatorio. | Confirmado | DecisiÃ³n de DLG, 12-X-2026; [domain-model.md â†’ Â«UserÂ»](../../../context/domain-model.md#user) | Formato y longitud no estÃ¡n en las fuentes: son un detalle tÃ©cnico de este plan. |
| D04. La polÃ­tica de email lanza una excepciÃ³n nueva y especÃ­fica para una sintaxis invÃ¡lida; la ausencia de email y la longitud usan las excepciones existentes. | Confirmado | DecisiÃ³n de DLG, 12-X-2026; [decisions.md â†’ Â«TecnologÃ­aÂ»](../../../context/decisions.md#tecnologÃ­a) | AÃ±ade un tipo a `Exceptions/`; los tests de excepciones de S1.2 siguen siendo vÃ¡lidos. |
| D05. ImplementaciÃ³n de la sintaxis `addr-spec`: se valida el subconjunto que define [specifications.md â†’ Â«EmailÂ»](../../../context/specifications.md#email), sin CFWS ni formas obsoletas, con un analizador propio y no con `System.Net.Mail.MailAddress`, que no se usa. | Confirmado | DecisiÃ³n de DLG, 12-X-2026; [decisions.md â†’ Â«Datos, URLs y bÃºsquedaÂ»](../../../context/decisions.md#datos-urls-y-bÃºsqueda) | Los tests de V02 incluyen direcciones con comentarios y formas obsoletas, que deben rechazarse. |
| D06. `UserId` y `CollectionId` recibidos deben ser positivos. Las fuentes no lo dicen: se propone rechazar valores no positivos con `NotValidInputException` como defensa mÃ­nima. | Confirmado | DecisiÃ³n de DLG, 12-X-2026 | Se rechazan con `NotValidInputException` y se cubren con tests en V04 y V05. |
| D07. [architecture.md â†’ Â«DomainÂ»](../../../context/architecture.md#domain) no menciona la polÃ­tica de email entre las transformaciones de Domain ni [mvp0-plan.md](../../../plans/mvp0-plan.md) la incluye en S3.1. | Confirmado | DecisiÃ³n de DLG, 12-X-2026; [spec.md â†’ P01](spec.md#bloqueos-y-preguntas-pendientes) | La menciÃ³n ya estÃ¡ aÃ±adida en [architecture.md â†’ Â«DomainÂ»](../../../context/architecture.md#domain) y en la tarea S3.1 de [mvp0-plan.md](../../../plans/mvp0-plan.md). |
| R01. Los setters no pÃºblicos y la ausencia de constructor pÃºblico podrÃ­an impedir la materializaciÃ³n con Dapper en S5. | Aceptado | DecisiÃ³n de DLG, 12-X-2026 | S3.1 deja un constructor sin parÃ¡metros no pÃºblico y no aÃ±ade nada mÃ¡s para Dapper. S5 comprueba quÃ© propiedades fallan y harÃ¡ pÃºblico solo su setter. Esa excepciÃ³n no debe aplicarse a `UrlOriginal`, `UrlNormalized`, `IsPublic`, `CollectionId` ni `UserId` sin una revisiÃ³n expresa, porque sus invariantes dependen de CA11 y CA13. V06 y esos criterios se revisarÃ­an entonces en la spec o plan de S5. |
| R02. S3.2 y S3.3 pueden necesitar cambiar la firma de creaciÃ³n de `Link` (por ejemplo, recibir la colecciÃ³n para comprobar la propiedad) y aÃ±adir mÃ©todos de visibilidad y de movimiento. | Pendiente | [spec.md â†’ Â«AlcanceÂ»](spec.md#alcance) | Se acepta como retrabajo acotado: S3.1 recibe ids por indicaciÃ³n de la spec. |
| R03. La lÃ­nea base y el filtro de tests deben verificarse en el entorno actual. | Pendiente | Esta propuesta | Detectar el runner al inicio; si el filtro no funciona, ejecutar el proyecto de tests completo. |

## Estrategia de validaciÃ³n

Los tests se ubican reflejando la capa: `tests/Linkubator.Tests/Domain/Policies/` para `EmailPolicy` y `tests/Linkubator.Tests/Domain/Entities/` para las entidades. Los valores esperados se derivan de las fuentes, no de la salida del cÃ³digo bajo prueba.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisiÃ³n | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA12 | Automatizada: lÃ­nea base | SDK .NET 10 y dependencias restauradas | `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Filtro vÃ¡lido y suite actual verde antes de los cambios | Comando, SDK, conteo y cÃ³digo de salida. |
| V02 | CA14 | Automatizada: `EmailPolicy` | V01 | Tests de `EmailPolicy` con espacios y mayÃºsculas, puntos y `+` en la parte local, sin `@`, sintaxis invÃ¡lida, comentarios y formas obsoletas, vacÃ­o y valor por encima del lÃ­mite | Los vÃ¡lidos quedan recortados y en minÃºsculas sin alterar puntos ni `+`; los demÃ¡s lanzan la excepciÃ³n esperada | Nombres de tests, resultado y cÃ³digo de salida. |
| V03 | CA01, CA02, CA03, CA14 | Automatizada: `User` | V02 | Tests de `User`: propiedades creadas, `SecurityStamp` distinto entre dos usuarios, valores iniciales de bloqueo y correo, campos obligatorios, lÃ­mites, alias generado y alias rechazado, email invÃ¡lido | La creaciÃ³n vÃ¡lida expone los valores esperados; cada entrada invÃ¡lida lanza la excepciÃ³n esperada y no hay usuario | Nombres de tests, resultado y cÃ³digo de salida. |
| V04 | CA04, CA05, CA06 | Automatizada: `Collection` | V01 | Tests de `Collection`: creaciÃ³n, recorte, descripciÃ³n vacÃ­a como `null`, lÃ­mites, slug generado, nombre sin slug vÃ¡lido, renombrado vÃ¡lido e invÃ¡lido, cambio de descripciÃ³n | `Slug` se regenera solo con un nombre vÃ¡lido; ante un error la colecciÃ³n conserva `Name` y `Slug`; la colecciÃ³n nace privada | Nombres de tests, resultado y cÃ³digo de salida. |
| V05 | CA07, CA08, CA09, CA10 | Automatizada: `Link` | V01 | Tests de `Link`: URL ajustada y clave normalizada derivadas de la fuente, URL con esquema no permitido, longitud excesiva, sin dominio y con credenciales, opcionales vacÃ­os y con espacios, `Image` relativa, con host `localhost` o IP, estado inicial, ediciÃ³n vÃ¡lida e invÃ¡lida | Los valores vÃ¡lidos coinciden con la fuente; los invÃ¡lidos lanzan la excepciÃ³n esperada; la ediciÃ³n fallida conserva los valores anteriores | Nombres de tests, resultado y cÃ³digo de salida. |
| V06 | CA09, CA11, CA13 | Automatizada: superficie pÃºblica | V03â€“V05 | Test que, por reflexiÃ³n, comprueba que no existe setter pÃºblico en las propiedades de las entidades ni mÃ©todo pÃºblico que cambie `UrlOriginal`, `UrlNormalized`, `IsPublic`, `CollectionId` o `UserId`; incluye una comprobaciÃ³n negativa del propio test contra un tipo de prueba que sÃ­ expone un setter pÃºblico | El test pasa con las entidades y detecta el tipo de prueba con setter pÃºblico | Nombre del test, resultado y cÃ³digo de salida. |
| V07 | CA12 | Automatizada integrada y build | V02â€“V06 | `dotnet test Linkubator.sln --no-restore --verbosity minimal` y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal` | Todos los tests pasan, incluidos los de arquitectura de S0.7, y la compilaciÃ³n no produce advertencias | Comando, SDK, cÃ³digo de salida y resumen. |
| V08 | CA13 y trazabilidad de CA01â€“CA14 | RevisiÃ³n tÃ©cnica | V07 | Revisar `git diff` y la tabla fuente â†’ criterio â†’ tarea â†’ validaciÃ³n | Los cambios se limitan a Domain y a sus tests; no hay camino para dejar `IsPublic = true`, cambiar `IsPublic` o `CollectionId`, ni comprobar la igualdad de `UserId` | Resumen del diff y tabla de cobertura. |

LÃ­mites de las validaciones:

- V06 comprueba la forma de la API pÃºblica y no el comportamiento interno; la comprobaciÃ³n negativa solo demuestra que el test detecta un setter pÃºblico, no que cubra toda infracciÃ³n posible.
- V07 se apoya en los tests de arquitectura de S0.7 para que Domain no incorpore dependencias; no se aÃ±ade una comprobaciÃ³n negativa nueva porque esos tests ya la tienen y S3.1 no altera esa infraestructura.
- Ninguna validaciÃ³n demuestra la aceptaciÃ³n humana ni que S3.2, S3.3 o S3.4 estÃ©n cubiertos.

## Orden de ejecuciÃ³n y cierre

- Tareas: [tasks.md](tasks.md), con dependencias, paralelismo justificado y checks inmediatos. La polÃ­tica de email debe ir antes de `User`; `Collection` y `Link` son independientes entre sÃ­ y de `User`.
- Las decisiones D01 a D07 estÃ¡n confirmadas por DLG y R01 estÃ¡ aceptado.
- S3.1 se cierra cuando CA01â€“CA14 tengan evidencia, no queden bloqueos y DLG acepte el resultado.
- Ante un cambio de regla, detener la tarea afectada y tramitar su aprobaciÃ³n en la fuente propietaria antes de continuar.

