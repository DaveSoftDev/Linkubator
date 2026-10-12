# Plan técnico: S3.1 — Entidades `User`, `Collection` y `Link`

## Estado y aprobación del plan

- Estado: aprobado por DLG el 12-X-2026.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 12-X-2026 («Corregido»).
	- Plan: aprobado por DLG el 12-X-2026 («Consensuados los stoppers»).

## Diseño de implementación

### Componentes y contratos

Todo el código vive en `Linkubator.Domain`, sin referencias a otros proyectos ni a paquetes nuevos ([architecture.md → «Domain»](../../context/architecture.md#domain)). Las clases se organizan por responsabilidad y con namespace alineado con la ruta; los usings de namespaces internos usan alias.

| Componente | Ubicación propuesta | Responsabilidad | Reglas fuente |
| --- | --- | --- | --- |
| `BaseEntity` | `Entities/Bases/` | Clase base con `Id` y `CreatedAt`, heredada por `User`, `Collection` y `Link`. | [architecture.md → «Domain»](../../context/architecture.md#domain), [decisions.md → «Tecnología»](../../context/decisions.md#tecnología) |
| `EmailPolicy` | `Policies/` | Normaliza (recorte y minúsculas) y valida un email: obligatorio, longitud máxima y sintaxis `addr-spec`. Devuelve el email normalizado o lanza una excepción de dominio. | [specifications.md → «Email»](../../context/specifications.md#email), [«Longitudes máximas»](../../context/specifications.md#longitudes-máximas) |
| Excepción de email inválido | `Exceptions/` | Excepción específica de dominio para una sintaxis de email no válida, derivada de `DomainException`. | [decisions.md → «Tecnología»](../../context/decisions.md#tecnología) (una excepción específica por regla) |
| `User` | `Entities/` | Usuario con el registro completado. Se crea con un método de fábrica estático. | [domain-model.md → «User»](../../context/domain-model.md#user) |
| `Collection` | `Entities/` | Colección privada con renombrado y cambio de descripción. | [domain-model.md → «Collection»](../../context/domain-model.md#collection) |
| `Link` | `Entities/` | Enlace privado con edición de `Title`, `Description` e `Image`. | [domain-model.md → «Link»](../../context/domain-model.md#link) |

**Contratos comunes de las entidades**

- Las propiedades tienen setter no público y no hay constructor público que permita crear una entidad sin validar. La creación y la edición pasan por métodos que validan todo antes de asignar ([architecture.md → «Principios de Diseño»](../../context/architecture.md#principios-de-diseño): encapsulamiento e inmutabilidad).
- Los textos opcionales usan `UserTextPolicy.TrimToNull`; los obligatorios, `UserTextPolicy.ThrowIfNullOrWhiteSpace` tras recortar; las longitudes se comprueban con `UserTextPolicy.IsWithinMaximumLength`, que cuenta puntos de código. Los límites se leen de [«Longitudes máximas»](../../context/specifications.md#longitudes-máximas) y no se copian en este plan.
- Las entradas inválidas lanzan la excepción de dominio que ya usan las políticas existentes: `NotValidInputException`, `TextExceedsMaximumLengthException`, `TextBelowMinimumLengthException` y las de URL. Solo se añade la excepción de email inválido.
- `Id` y `CreatedAt` quedan sin asignar al crear la entidad; los asigna la persistencia (ver decisión D01).

**`User`**

- Método de fábrica con: email, nombre, candidato a alias, hash de contraseña y fecha de confirmación del email. Aplica `EmailPolicy`, `AliasPolicy.Generate` y las reglas de texto de `Name`.
- `Password` se recibe como hash ya calculado: solo se exige que esté informado. No se validan su formato ni sus reglas de contraseña en claro (excluidas por la spec).
- `EmailConfirmedAt` es obligatorio: un valor por defecto se rechaza con `NotValidInputException`.
- `SecurityStamp` se genera al crear el usuario con un valor aleatorio de un generador criptográfico (decisión D03).
- `FailedLoginAttempts` es `0`; `LockoutEnd`, `LastEmailSentAt` y `LastExistingAccountNoticeAt` son `null`. No hay métodos que los cambien.
- No hay métodos de cambio de `Email`, `Alias`, `Password` ni `SecurityStamp`: su comportamiento pertenece a etapas posteriores.

**`Collection`**

- Método de fábrica con `UserId`, nombre y descripción. `Slug` se obtiene de `SlugPolicy.Generate(Name)` con el nombre ya recortado. `IsPublic` es `false`.
- Método de renombrado, que regenera `Slug`, y método de cambio de descripción.
- No hay método que cambie `IsPublic`, `UserId` ni `CreatedAt`.

**`Link`**

- Método de fábrica con `UserId`, `CollectionId`, `UrlOriginal`, `Title`, `Description` e `Image`.
- `UrlOriginal`: recortar, `LinkUrlPolicy.Adjust`, `UrlValidationPolicy.Validate`; `UrlNormalized` se calcula con `UrlNormalizationPolicy.Normalize` sobre ese resultado.
- `Image`: recortar; si queda vacía es `null`; si no, `ImageUrlPolicy.Adjust` contra la `UrlOriginal` ajustada y después `UrlValidationPolicy.Validate`.
- `IsPublic` es `false` y `ScrapingStatus` es `NotRequested`.
- Método de edición de `Title`, `Description` e `Image`, con las mismas reglas que la creación. Recibe los tres valores y los sustituye todos.
- No hay setter ni método que cambie `UrlOriginal`, `UrlNormalized`, `IsPublic`, `CollectionId` ni `UserId`.

### Flujos internos

1. **Creación.** Recortar cada texto, validar todos los valores, calcular las derivadas (alias, slug, URL ajustada, clave normalizada, imagen ajustada) y asignar las propiedades al final. Si cualquier paso lanza, no se devuelve entidad.
2. **Edición y renombrado.** Calcular los valores nuevos en variables locales y asignarlos solo cuando todos son válidos, de modo que un error deja la entidad como estaba.
3. **Errores.** Las excepciones de las políticas se propagan sin envolver. Las entidades no capturan excepciones ni registran logs.

### Persistencia y dependencias

- No hay persistencia, transacciones, integraciones ni paquetes nuevos. Domain no conoce SQLite ni Dapper.
- S5 es la propietaria del mapeo a base de datos y de cómo materializa las entidades con setters no públicos (ver riesgo R01).
- S3.2 y S3.3 extienden estas entidades con las invariantes de propiedad y de público y privado (ver riesgo R02).
- Las unicidades de `Alias`, `Email`, `Collection.Slug` y `Link.UrlNormalized` se aplican en Application e Infrastructure (S4 y S5).

### Garantías técnicas

- Las entidades solo exponen caminos que validan: no existe un camino público para cambiar `UrlOriginal`, `UrlNormalized`, `IsPublic` ni `CollectionId`, ni para dejar `IsPublic` en `true` ([domain-model.md → «Link»](../../context/domain-model.md#link) y [«Público y privado»](../../context/domain-model.md#público-y-privado)).
- Una operación rechazada no deja la entidad parcialmente modificada.
- `UserId` y `CollectionId` se reciben tal cual: S3.1 no autoriza nada con ellos ([domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) es de S3.2).
- Contraseñas y tokens no se registran: S3.1 no registra datos y `User` solo conserva el hash recibido ([specifications.md → «Contraseñas»](../../context/specifications.md#contraseñas)).
- Domain sigue sin referenciar otros proyectos, comprobado por los tests de arquitectura de S0.7.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| D01. `Id` y `CreatedAt` de `BaseEntity` quedan sin asignar al crear la entidad (`Id = 0`, `CreatedAt` sin valor) y los asigna la persistencia, según [specifications.md → «Registro»](../../context/specifications.md#registro). Tipos propuestos: entero de 64 bits para `Id` y `DateTime` en UTC para `CreatedAt`. | Confirmado | Decisión de DLG, 12-X-2026; [spec.md → P03](spec.md#bloqueos-y-preguntas-pendientes) | Las firmas de creación no reciben `Id` ni `CreatedAt`. |
| D02. Las entidades se crean con métodos de fábrica estáticos y setters no públicos, no con setters públicos. | Confirmado | Decisión de DLG, 12-X-2026; [architecture.md → «Principios de Diseño»](../../context/architecture.md#principios-de-diseño) | Garantiza CA11 y CA13. Si Dapper no puede materializar alguna propiedad (R01), se hará público su setter en S5, solo para las propiedades afectadas. |
| D03. `SecurityStamp` se genera en `User` con un generador criptográfico, a partir de 32 bytes aleatorios en una cadena segura para URL. Las fuentes solo exigen un valor aleatorio. | Confirmado | Decisión de DLG, 12-X-2026; [domain-model.md → «User»](../../context/domain-model.md#user) | Formato y longitud no están en las fuentes: son un detalle técnico de este plan. |
| D04. La política de email lanza una excepción nueva y específica para una sintaxis inválida; la ausencia de email y la longitud usan las excepciones existentes. | Confirmado | Decisión de DLG, 12-X-2026; [decisions.md → «Tecnología»](../../context/decisions.md#tecnología) | Añade un tipo a `Exceptions/`; los tests de excepciones de S1.2 siguen siendo válidos. |
| D05. Implementación de la sintaxis `addr-spec`: se valida el subconjunto que define [specifications.md → «Email»](../../context/specifications.md#email), sin CFWS ni formas obsoletas, con un analizador propio y no con `System.Net.Mail.MailAddress`, que no se usa. | Confirmado | Decisión de DLG, 12-X-2026; [decisions.md → «Datos, URLs y búsqueda»](../../context/decisions.md#datos-urls-y-búsqueda) | Los tests de V02 incluyen direcciones con comentarios y formas obsoletas, que deben rechazarse. |
| D06. `UserId` y `CollectionId` recibidos deben ser positivos. Las fuentes no lo dicen: se propone rechazar valores no positivos con `NotValidInputException` como defensa mínima. | Confirmado | Decisión de DLG, 12-X-2026 | Se rechazan con `NotValidInputException` y se cubren con tests en V04 y V05. |
| D07. [architecture.md → «Domain»](../../context/architecture.md#domain) no menciona la política de email entre las transformaciones de Domain ni [mvp0-plan.md](../../plans/mvp0-plan.md) la incluye en S3.1. | Confirmado | Decisión de DLG, 12-X-2026; [spec.md → P01](spec.md#bloqueos-y-preguntas-pendientes) | La mención ya está añadida en [architecture.md → «Domain»](../../context/architecture.md#domain) y en la tarea S3.1 de [mvp0-plan.md](../../plans/mvp0-plan.md). |
| R01. Los setters no públicos y la ausencia de constructor público podrían impedir la materialización con Dapper en S5. | Aceptado | Decisión de DLG, 12-X-2026 | S3.1 deja un constructor sin parámetros no público y no añade nada más para Dapper. S5 comprueba qué propiedades fallan y hará público solo su setter. Esa excepción no debe aplicarse a `UrlOriginal`, `UrlNormalized`, `IsPublic`, `CollectionId` ni `UserId` sin una revisión expresa, porque sus invariantes dependen de CA11 y CA13. V06 y esos criterios se revisarían entonces en la spec o plan de S5. |
| R02. S3.2 y S3.3 pueden necesitar cambiar la firma de creación de `Link` (por ejemplo, recibir la colección para comprobar la propiedad) y añadir métodos de visibilidad y de movimiento. | Pendiente | [spec.md → «Alcance»](spec.md#alcance) | Se acepta como retrabajo acotado: S3.1 recibe ids por indicación de la spec. |
| R03. La línea base y el filtro de tests deben verificarse en el entorno actual. | Pendiente | Esta propuesta | Detectar el runner al inicio; si el filtro no funciona, ejecutar el proyecto de tests completo. |

## Estrategia de validación

Los tests se ubican reflejando la capa: `tests/Linkubator.Tests/Domain/Policies/` para `EmailPolicy` y `tests/Linkubator.Tests/Domain/Entities/` para las entidades. Los valores esperados se derivan de las fuentes, no de la salida del código bajo prueba.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA12 | Automatizada: línea base | SDK .NET 10 y dependencias restauradas | `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Filtro válido y suite actual verde antes de los cambios | Comando, SDK, conteo y código de salida. |
| V02 | CA14 | Automatizada: `EmailPolicy` | V01 | Tests de `EmailPolicy` con espacios y mayúsculas, puntos y `+` en la parte local, sin `@`, sintaxis inválida, comentarios y formas obsoletas, vacío y valor por encima del límite | Los válidos quedan recortados y en minúsculas sin alterar puntos ni `+`; los demás lanzan la excepción esperada | Nombres de tests, resultado y código de salida. |
| V03 | CA01, CA02, CA03, CA14 | Automatizada: `User` | V02 | Tests de `User`: propiedades creadas, `SecurityStamp` distinto entre dos usuarios, valores iniciales de bloqueo y correo, campos obligatorios, límites, alias generado y alias rechazado, email inválido | La creación válida expone los valores esperados; cada entrada inválida lanza la excepción esperada y no hay usuario | Nombres de tests, resultado y código de salida. |
| V04 | CA04, CA05, CA06 | Automatizada: `Collection` | V01 | Tests de `Collection`: creación, recorte, descripción vacía como `null`, límites, slug generado, nombre sin slug válido, renombrado válido e inválido, cambio de descripción | `Slug` se regenera solo con un nombre válido; ante un error la colección conserva `Name` y `Slug`; la colección nace privada | Nombres de tests, resultado y código de salida. |
| V05 | CA07, CA08, CA09, CA10 | Automatizada: `Link` | V01 | Tests de `Link`: URL ajustada y clave normalizada derivadas de la fuente, URL con esquema no permitido, longitud excesiva, sin dominio y con credenciales, opcionales vacíos y con espacios, `Image` relativa, con host `localhost` o IP, estado inicial, edición válida e inválida | Los valores válidos coinciden con la fuente; los inválidos lanzan la excepción esperada; la edición fallida conserva los valores anteriores | Nombres de tests, resultado y código de salida. |
| V06 | CA09, CA11, CA13 | Automatizada: superficie pública | V03–V05 | Test que, por reflexión, comprueba que no existe setter público en las propiedades de las entidades ni método público que cambie `UrlOriginal`, `UrlNormalized`, `IsPublic`, `CollectionId` o `UserId`; incluye una comprobación negativa del propio test contra un tipo de prueba que sí expone un setter público | El test pasa con las entidades y detecta el tipo de prueba con setter público | Nombre del test, resultado y código de salida. |
| V07 | CA12 | Automatizada integrada y build | V02–V06 | `dotnet test Linkubator.sln --no-restore --verbosity minimal` y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal` | Todos los tests pasan, incluidos los de arquitectura de S0.7, y la compilación no produce advertencias | Comando, SDK, código de salida y resumen. |
| V08 | CA13 y trazabilidad de CA01–CA14 | Revisión técnica | V07 | Revisar `git diff` y la tabla fuente → criterio → tarea → validación | Los cambios se limitan a Domain y a sus tests; no hay camino para dejar `IsPublic = true`, cambiar `IsPublic` o `CollectionId`, ni comprobar la igualdad de `UserId` | Resumen del diff y tabla de cobertura. |

Límites de las validaciones:

- V06 comprueba la forma de la API pública y no el comportamiento interno; la comprobación negativa solo demuestra que el test detecta un setter público, no que cubra toda infracción posible.
- V07 se apoya en los tests de arquitectura de S0.7 para que Domain no incorpore dependencias; no se añade una comprobación negativa nueva porque esos tests ya la tienen y S3.1 no altera esa infraestructura.
- Ninguna validación demuestra la aceptación humana ni que S3.2, S3.3 o S3.4 estén cubiertos.

## Orden de ejecución y cierre

- Tareas: [tasks.md](tasks.md), con dependencias, paralelismo justificado y checks inmediatos. La política de email debe ir antes de `User`; `Collection` y `Link` son independientes entre sí y de `User`.
- Las decisiones D01 a D07 están confirmadas por DLG y R01 está aceptado.
- S3.1 se cierra cuando CA01–CA14 tengan evidencia, no queden bloqueos y DLG acepte el resultado.
- Ante un cambio de regla, detener la tarea afectada y tramitar su aprobación en la fuente propietaria antes de continuar.
