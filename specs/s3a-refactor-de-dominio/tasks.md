# Tareas: S3a.1 — Refactor de las políticas de URL

## Referencias y autorización

- Estado: aprobado (las tareas se ejecutaron por autorización directa de DLG, 14-X-2026, sobre el plan de `.kilo/plans/1791934991905-s3a-refactor-policies-url.md`).
- Aprobaciones:
    - Especificación: [spec.md](spec.md), pendiente de aprobación humana formal.
    - Plan: [plan.md](plan.md), pendiente de aprobación humana formal.
    - Tareas: ejecutadas con la autorización indicada arriba.
- Implementación:
    - Estado: completada.
    - Inicio: 14-X-2026, tras línea base verde de 582 pruebas.
    - Finalización: 14-X-2026; suite completa 639/639 (línea base del WIP 630/630 más los tests de T11, T12 y T13) y build con `-warnaserror` sin errores ni warnings.
    - Evidencia consolidada: V01–V06 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación).
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

Tareas secuenciales salvo T08, que quedó resuelta dentro de T04 y T05, y T11, cuyos tres movimientos son independientes entre sí. Cada incremento mantuvo la suite de políticas en verde.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Línea base de la suite completa | CA01; [plan → V01](plan.md#estrategia-de-validación) | Ninguna | `dotnet test Linkubator.sln --no-restore`; 582 pruebas correctas | completada | 14-X-2026; 582/582, 0 fallos |
| T02 | Borrar código muerto: `HasSchemeSeparator`, `TryReadHostAndPortForm`, `TryExtractValidHost` y `ExtractHost`; reconvertir sus tests a `TryExtractHost` y añadir la teoría de `IsValidAuthority` | CA01, CA05; [plan → Componentes y contratos](plan.md#componentes-y-contratos) | T01 | Filtro `Domain.Policies`; 495 pruebas correctas | completada | 14-X-2026; 495/495 |
| T03 | Unificar la validación de puerto en `UriAuthorityPolicy.IsValidPort` con regla estricta | CA01, CA02; [plan → Componentes y contratos](plan.md#componentes-y-contratos) | T02 | Filtro `Domain.Policies`; 495 pruebas correctas | completada | 14-X-2026; 495/495 |
| T04 | Absorber `UrlHostPolicy` en `UriAuthorityPolicy`, borrar la clase y renombrar `UrlHostPolicyTests.cs` a `UriAuthorityPolicyTests.cs` | CA02, CA05; [plan → Componentes y contratos](plan.md#componentes-y-contratos) | T03 | Filtro `Domain.Policies`; 495 pruebas correctas | completada | 14-X-2026; 495/495; `git rm` de `UrlHostPolicy.cs` |
| T05 | Eliminar `UriSchemePolicy` y `UriSchemeDetection`; reescribir la detección en `LinkUrlPolicy` e `ImageUrlPolicy` sobre `UriAuthorityPolicy` | CA02, CA03; [plan → Flujos internos](plan.md#flujos-internos) | T04 | Filtro `Domain.Policies`; 495 pruebas correctas | completada | 14-X-2026; 495/495; `git rm` de `UriSchemePolicy.cs` |
| T06 | Reescribir `DetectClassifiesHostAndExplicitSchemeInputs` como `AuthorityClassificationMatchesHostAndPortAndExplicitScheme` con los mismos casos y expectativas | CA01, CA02 | T05 | La teoría pasa con las 13 entradas previas | completada | 14-X-2026; teoría verde |
| T07 | Extraer `UrlInputPolicy.Prepare` y usarla en `LinkUrlPolicy` (con longitud) e `ImageUrlPolicy` (sin longitud); borrar `ValidateAndTrim` y `ContainsControls` duplicados | CA04; [plan → Componentes y contratos](plan.md#componentes-y-contratos) | T05 | Filtro `Domain.Policies`; 495 pruebas correctas | completada | 14-X-2026; 495/495 |
| T08 | Unificar esquemas permitidos en `UriAuthorityPolicy.IsAllowedScheme`, usado por los dos ajustes y la validación | CA03 | T04, T05 | Revisión: una sola definición en `src` | completada | 14-X-2026; búsqueda sin copias restantes salvo la regla de upgrade http→https de `ImageUrlPolicy` |
| T09 | Añadir tests de espacios interiores (enlace e imagen), bordes de puerto y `UrlInputPolicyTests`; el caso de sustituto suelto se fija con `[Fact]` porque xUnit altera el valor en `[InlineData]` | CA04, CA05; [plan → Decisiones técnicas y riesgos](plan.md#decisiones-técnicas-y-riesgos) | T07 | Filtro `Domain.Policies`; 523 pruebas correctas | completada | 14-X-2026; 523/523 (+35 casos nuevos, −11 de `TryExtractValidHost`) |
| T10 | Redactar estos artefactos, registrar el trabajo en el índice y ejecutar la validación integrada | CA01, CA06; [plan → Estrategia de validación](plan.md#estrategia-de-validación) | T09 | Suite 606/606 y build `-warnaserror` sin errores ni warnings | completada | 14-X-2026; .NET 10.0.12 |
| T11 | Dividir `UriAuthorityPolicy` en fachada pública y colaboradores internos (`UriAuthoritySyntax`, `UriHostFormat`, `UriPortFormat`): movimiento de código con cuerpos literales, sin cambio de contrato ni de comportamiento | CA01, CA02, CA05 | T10 | Línea base 630/630; filtro `Domain.Policies` (535) tras cada movimiento; suite completa al final | completada | 14-X-2026; 535/535 tras cada movimiento; test estructural `AuthorityCollaboratorsAreNotVisibleOutsideDomain`; suite 631/631. Deriva doc↔código: `TryAnalyze`→`Analyze` en el plan y la referencia a `UrlHostPolicy` de [decisions.md → «Riesgos aceptados»](../../context/decisions.md#riesgos-aceptados) reapuntada a `UriAuthorityPolicy`; sin cambios de regla |
| T12 | Adecuar la nomenclatura de los rechazos del ajuste de URL a su causa: host presente con formato no admitido, puerto no válido y autoridad ausente pasan a nombrarse cada uno con su excepción, mediante un ayudante privado en `LinkUrlPolicy` | CA01, CA05 | T11 | `AdjustNamesEachRejectionAfterItsCause` y `SpecificationExamplesAreRejectedByAdjustmentOrValidation` en verde; comprobación negativa rompiendo la taxonomía | completada | 14-X-2026; filtro `Domain.Policies` 542/542; suite 639/639 |
| T13 | Ajustes menores de la revisión de `src/` dentro del alcance: comentario en `UrlInputPolicy.Prepare` sobre el uso de la normalización como detector y test de regresión `NormalizeDoesNotLeakAProviderExceptionForALoneSurrogate` | CA01, CA05 | T11 | Test nuevo en verde; suite completa | completada | 14-X-2026; filtro `Domain.Policies` 542/542 |
| T14 | Alinear con la regla de [architecture.md → «Domain»](../../context/architecture.md#domain) las dos excepciones genéricas que quedaban: `NotValidInputException` para la base en blanco de `ImageUrlPolicy` y `UrlNotAbsoluteException` para la entrada no absoluta de `UrlNormalizationPolicy` | CA01, CA05 | T13 | Búsqueda de `throw new` de excepciones genéricas en Domain: ninguna; pruebas en verde | completada | 14-X-2026; filtro `Domain.Policies` 542/542; suite 639/639; actualizadas `NormalizeDoesNotRevalidateAnAlreadyAdjustedUrl` y `ResolveRelativeRejectsABaseUrlThatIsNullOrWhitespace` |
| T15 | Revisión y aceptación humana del resultado (DoD de S3a.1) | Todos | T14 | Decisión registrada por DLG | pendiente | Sin aceptación |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01–T07, T10–T14 | V01, V02 | Suite completa 639/639 correctas; sin cambios de excepción por entrada, salvo la adecuación de nomenclaturas de T12 y la alineación de T14, que solo cambian el código devuelto en entradas ya rechazadas | 14-X-2026; .NET 10.0.12 | Salida de `dotnet test Linkubator.sln --no-restore --verbosity minimal` |
| CA02 | T04–T06, T11 | V04 | Cero referencias a `UriSchemePolicy` y `UrlHostPolicy` en `src` y `tests`; los colaboradores de T11 solo tienen consumidores dentro de `src/Linkubator.Domain/Policies` y los tres consumidores de producción siguen entrando por la fachada | 14-X-2026 | Búsqueda en el repositorio |
| CA03 | T08 | V04 | Una sola definición de la lista de esquemas admitidos | 14-X-2026 | `UriAuthorityPolicy.cs`, método `IsAllowedScheme` |
| CA04 | T07, T09 | V05 | `UrlInputPolicyTests` y los tests de espacios interiores pasan | 14-X-2026 | 523/523 en el filtro `Domain.Policies` |
| CA05 | T02, T09, T11 | V05, V06 | Ningún miembro público sin consumidor productivo ni test; los colaboradores internos quedan fijados como no públicos | 14-X-2026 | Revisión del árbol de llamadas y test `AuthorityCollaboratorsAreNotVisibleOutsideDomain` |
| CA06 | T10, T11 | V01, V03 | Build sin warnings con `-warnaserror` y suite completa verde | 14-X-2026 | Resumen del build y de la suite |

## Aceptación y cierre

- Estado del trabajo: implementación completada; aceptación humana pendiente.
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos y riesgos restantes: ninguno comprobado. Queda la decisión humana sobre el commit del WIP previo y de este refactor.
- Aceptación humana: pendiente; responsable, fecha y referencia cuando exista.
- Archivado: pendiente / no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso autorizado, sin cerrar otras etapas: revisión humana de S3a.1 y, si procede, S3a.2 (entidades).
