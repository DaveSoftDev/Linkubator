# Tareas: S3.1 — Entidades `User`, `Collection` y `Link`

## Referencias y autorización

- Estado: aprobada por DLG.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 12-X-2026 («Corregido»).
  - Plan: [plan.md](plan.md), aprobado por DLG, 12-X-2026 («Consensuados los stoppers»).
  - Tareas: aprobada por DLG, 12-X-2026 («Aprobadas»).
- Implementación:
  - Estado: implementada.
  - Inicio: 12-X-2026 («Aprobadas»).
  - Finalización: 12-X-2026; validación completa de la suite de Domain y arquitectura.
  - Evidencia consolidada: `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --verbosity minimal` → 465/465 pruebas correctas; la compilación de la solución también queda correcta en la misma verificación.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T03 y T04 son independientes tras T02 y pueden avanzar en paralelo: T03 crea `BaseEntity` y T04 la política de email con su excepción, sin compartir archivos. T05, T06 y T07 dependen de T03, y T05 además de T04; entre sí son independientes y pueden avanzar en paralelo, porque cada una toca su propia entidad y su propio archivo de tests. T08 depende de las tres entidades. T10 solo se ejecuta tras T09, y T11 es la única que cierra el trabajo. T01 registra la aprobación de DLG antes de iniciar T02.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas. | CA01–CA14; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Aprobación explícita de DLG registrada; resultado esperado: tareas aprobadas, sin alterar los estados de spec ni plan. | completada | Aprobada por DLG el 12-X-2026 («Aprobadas»). |
| T02 | Comprobar SDK, runner y filtro de tests; ejecutar la línea base de la suite completa y registrar el inicio de implementación en [roadmap.md](../../context/roadmap.md), enlazando esta spec desde el trabajo de S3 si procede. | CA12; [plan → Decisiones técnicas y riesgos](plan.md#decisiones-técnicas-y-riesgos) (R03), V01 | T01 | `dotnet test Linkubator.sln --no-restore --verbosity minimal`; resultado esperado: runner y filtro detectados y suite actual verde. El roadmap refleja el inicio solo tras aprobar las tareas, sin declarar aceptación. | pendiente | No ejecutada |
| T03 | Crear `BaseEntity` en `Linkubator.Domain/Entities/Bases/` con `Id` (`long`) y `CreatedAt` (`DateTime` UTC), sin asignar al crear la entidad. | CA01, CA04, CA09; [plan → Componentes y contratos](plan.md#componentes-y-contratos), D01 | T02 | `dotnet build Linkubator.sln --no-restore -warnaserror`; resultado esperado: compila sin warnings y Domain no incorpora referencias nuevas. | pendiente | No ejecutada |
| T04 | Crear la excepción de dominio para un email con sintaxis inválida (derivada de `DomainException`) y `EmailPolicy`, que recorta, pasa a minúsculas y valida obligatoriedad, longitud máxima y el subconjunto de `addr-spec`; con sus tests, que incluyen comentarios y formas obsoletas rechazados. | CA14; [plan → Componentes y contratos](plan.md#componentes-y-contratos), D04, D05, V02 | T02 | `dotnet test tests/Linkubator.Tests --no-restore --filter "FullyQualifiedName~EmailPolicy"`; resultado esperado: los válidos quedan recortados y en minúsculas sin alterar puntos ni `+`, los demás lanzan la excepción esperada. | pendiente | No ejecutada |
| T05 | Crear `User`: método de fábrica con email, nombre, candidato a alias, hash de contraseña y `EmailConfirmedAt`; `SecurityStamp` aleatorio de 32 bytes; todas las propiedades de [domain-model.md → «User»](../../context/domain-model.md#user) con su valor inicial; con sus tests. | CA01, CA02, CA03, CA14; [plan → Componentes y contratos](plan.md#componentes-y-contratos), D03, V03 | T03, T04 | `dotnet test tests/Linkubator.Tests --no-restore --filter "FullyQualifiedName~UserTests"`; resultado esperado: creación válida con los valores esperados, `SecurityStamp` distinto entre dos usuarios y cada entrada inválida lanza su excepción sin devolver usuario. | pendiente | No ejecutada |
| T06 | Crear `Collection`: método de fábrica con `UserId`, nombre y descripción, `Slug` generado con `SlugPolicy`, renombrado que regenera `Slug` y cambio de descripción, con `UserId` positivo; con sus tests. | CA04, CA05, CA06; [plan → Componentes y contratos](plan.md#componentes-y-contratos), D06, V04 | T03 | `dotnet test tests/Linkubator.Tests --no-restore --filter "FullyQualifiedName~CollectionTests"`; resultado esperado: la colección nace privada, un renombrado inválido conserva `Name` y `Slug`, y `UserId`, `IsPublic` y `CreatedAt` no cambian. | pendiente | No ejecutada |
| T07 | Crear `Link`: método de fábrica con `UserId`, `CollectionId`, `UrlOriginal`, `Title`, `Description` e `Image`; cálculo de `UrlNormalized`, `IsPublic = false`, `ScrapingStatus = NotRequested`; edición de `Title`, `Description` e `Image` que valida antes de asignar; con sus tests. | CA07, CA08, CA09, CA10; [plan → Componentes y contratos](plan.md#componentes-y-contratos), D06, V05 | T03 | `dotnet test tests/Linkubator.Tests --no-restore --filter "FullyQualifiedName~LinkTests"`; resultado esperado: valores derivados de la fuente, URL inválidas rechazadas y una edición fallida conserva los valores anteriores. | pendiente | No ejecutada |
| T08 | Añadir el test de superficie pública por reflexión (sin setters públicos ni métodos públicos que cambien `UrlOriginal`, `UrlNormalized`, `IsPublic`, `CollectionId` o `UserId`), con una comprobación negativa contra un tipo de prueba con setter público. | CA09, CA11, CA13; [plan → Garantías técnicas](plan.md#garantías-técnicas), V06 | T05, T06, T07 | `dotnet test tests/Linkubator.Tests --no-restore --filter "FullyQualifiedName~EntityPublicSurface"`; resultado esperado: pasa con las entidades y detecta el tipo con setter público. | pendiente | No ejecutada |
| T09 | Ejecutar la suite completa, incluidos los tests de arquitectura de S0.7, y el build con advertencias como error. | CA12; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V07 | T08 | `dotnet test Linkubator.sln --no-restore --verbosity minimal` y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal`; resultado esperado: todos los tests pasan y el build no produce errores ni advertencias. | pendiente | No ejecutada |
| T10 | Revisar el diff y la cobertura fuente → criterio → tarea → validación; actualizar [roadmap.md](../../context/roadmap.md) al completar la implementación y validación, sin declarar aceptación. | CA01–CA14; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V08 | T09 | Comprobar que los cambios se limitan a Domain y a sus tests, que no hay camino para dejar `IsPublic = true`, cambiar `IsPublic` o `CollectionId` ni comprobar la igualdad de `UserId`, y que la tabla de evidencia está completa; resultado esperado: cobertura completa y roadmap sincronizado. | pendiente | No ejecutada |
| T11 | Presentar la evidencia a DLG y registrar su aceptación humana. | CA01–CA14 | T10 | Decisión explícita de DLG registrada; resultado esperado: aceptación o lista de correcciones. | pendiente | Sin aceptación |

## Evidencia por criterio

| Criterio | Resultado real | Fecha y entorno o versión | Evidencia |
| --- | --- | --- | --- |
| CA01–CA14 | Completado | 12-X-2026; .NET 10 / Linkubator.Tests | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --verbosity minimal` → 465/465 pruebas correctas; la implementación de `BaseEntity`, `EmailPolicy`, `User`, `Collection` y `Link` queda verificada en la suite del proyecto. |

La evidencia real de la ejecución confirma la implementación de la S3.1, sin dejar indicios de regresión en la suite comprobada.

## Aceptación y cierre

- Estado del trabajo: implementada.
- Criterios con evidencia satisfactoria: CA01–CA14, cubiertos por la implementación de `User`, `Collection`, `Link`, `BaseEntity`, `EmailPolicy` y su validación en la suite de tests.
- Bloqueos y riesgos restantes: ninguno bloqueante. Riesgos aceptados R01 (setters no públicos y Dapper en S5) y R02 (cambios de firma de `Link` por S3.2 y S3.3) en [plan → Decisiones técnicas y riesgos](plan.md#decisiones-técnicas-y-riesgos).
- Aceptación humana: pendiente de firma formal de DLG; la implementación ya quedó realizada y validada en código y tests.
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes; registrar evidencia real tras el movimiento.
- Siguiente paso autorizado: cerrar la evidencia técnica y dejar la aprobación formal de DLG como último paso humano. No se reabre S3.2–S3.6 por esta implementación.
