# Tareas: S3a.1 — Refactor de las políticas de URL

## Referencias y autorización

- Estado: tareas aprobadas y ejecución completada. DLG aprobó las tareas el 13-X-2026 con referencia «Listo».
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 13-X-2026, referencia «Cambios validados».
    - Plan: [plan.md](plan.md), aprobado por DLG el 13-X-2026, referencia «Adelante».
    - Tareas: aprobadas por DLG el 13-X-2026, referencia «Listo».
- Implementación:
    - Estado: completada.
    - Inicio: 13-X-2026, tras línea base verde de 582 pruebas.
    - Finalización: implementación 14-X-2026; validación integrada CA06 repetida el 15-X-2026: suite completa 727/727 y build con `-warnaserror` sin errores ni warnings.
    - Evidencia consolidada: V01–V06 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación); el TRX de CA06 se conserva en [evidence/ca06-2026-10-15.trx](evidence/ca06-2026-10-15.trx).
- La autorización directa de implementación del 13-X-2026 precedió a la aprobación formal de estos artefactos; las aprobaciones registradas arriba no alteran la evidencia histórica de ejecución.

## Descomposición

Tareas secuenciales salvo T08, que quedó resuelta dentro de T04 y T05, y T11, cuyos tres movimientos son independientes entre sí. Cada incremento mantuvo la suite de políticas en verde.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Línea base de la suite completa | CA01; [plan → V01](plan.md#estrategia-de-validación) | Ninguna | `dotnet test Linkubator.sln --no-restore`; 582 pruebas correctas | completada | 13-X-2026; 582/582, 0 fallos |
| T02 | Borrar código muerto: `HasSchemeSeparator`, `TryReadHostAndPortForm`, `TryExtractValidHost` y `ExtractHost`; reconvertir sus tests a `TryExtractHost` y añadir la teoría de `IsValidAuthority` | CA01, CA05; [plan → Componentes y contratos](plan.md#componentes-y-contratos) | T01 | Filtro `Domain.Policies`; 495 pruebas correctas | completada | 13-X-2026; 495/495 |
| T03 | Unificar la validación de puerto en `UriAuthorityPolicy.IsValidPort` con regla estricta | CA01, CA02; [plan → Componentes y contratos](plan.md#componentes-y-contratos) | T02 | Filtro `Domain.Policies`; 495 pruebas correctas | completada | 13-X-2026; 495/495 |
| T04 | Absorber `UrlHostPolicy` en `UriAuthorityPolicy`, borrar la clase y renombrar `UrlHostPolicyTests.cs` a `UriAuthorityPolicyTests.cs` | CA02, CA05; [plan → Componentes y contratos](plan.md#componentes-y-contratos) | T03 | Filtro `Domain.Policies`; 495 pruebas correctas | completada | 13-X-2026; 495/495; `git rm` de `UrlHostPolicy.cs` |
| T05 | Eliminar `UriSchemePolicy` y `UriSchemeDetection`; reescribir la detección en `LinkUrlPolicy` e `ImageUrlPolicy` sobre `UriAuthorityPolicy` | CA02, CA03; [plan → Flujos internos](plan.md#flujos-internos) | T04 | Filtro `Domain.Policies`; 495 pruebas correctas | completada | 13-X-2026; 495/495; `git rm` de `UriSchemePolicy.cs` |
| T06 | Reescribir `DetectClassifiesHostAndExplicitSchemeInputs` como `AuthorityClassificationMatchesHostAndPortAndExplicitScheme` con los mismos casos y expectativas | CA01, CA02 | T05 | La teoría pasa con las 13 entradas previas | completada | 13-X-2026; teoría verde |
| T07 | Extraer `UrlInputPolicy.Prepare` y usarla en `LinkUrlPolicy` (con longitud) e `ImageUrlPolicy` (sin longitud); borrar `ValidateAndTrim` y `ContainsControls` duplicados | CA04; [plan → Componentes y contratos](plan.md#componentes-y-contratos) | T05 | Filtro `Domain.Policies`; 495 pruebas correctas | completada | 13-X-2026; 495/495 |
| T08 | Unificar esquemas permitidos en `UriAuthorityPolicy.IsAllowedScheme`, usado por los dos ajustes y la validación | CA03 | T04, T05 | Revisión: una sola definición en `src` | completada | 13-X-2026; búsqueda sin copias restantes salvo la regla de upgrade http→https de `ImageUrlPolicy` |
| T09 | Añadir tests de espacios interiores (enlace e imagen), bordes de puerto y `UrlInputPolicyTests`; el caso de sustituto suelto se fija con `[Fact]` porque xUnit altera el valor en `[InlineData]` | CA04, CA05; [plan → Decisiones técnicas y riesgos](plan.md#decisiones-técnicas-y-riesgos) | T07 | Filtro `Domain.Policies`; 523 pruebas correctas | completada | 13-X-2026; 523/523 (+35 casos nuevos, −11 de `TryExtractValidHost`) |
| T10 | Redactar estos artefactos, registrar el trabajo en el índice y ejecutar la validación integrada | CA01, CA06; [plan → Estrategia de validación](plan.md#estrategia-de-validación) | T09 | Suite 606/606 y build `-warnaserror` sin errores ni warnings | completada | 13-X-2026; .NET 10.0.12 |
| T11 | Dividir `UriAuthorityPolicy` en fachada pública y colaboradores internos (`UriAuthoritySyntax`, `UriHostFormat`, `UriPortFormat`): movimiento de código con cuerpos literales, sin cambio de contrato ni de comportamiento | CA01, CA02, CA05 | T10 | Línea base 630/630; filtro `Domain.Policies` (535) tras cada movimiento; suite completa al final | completada | 13-X-2026; 535/535 tras cada movimiento; test estructural `AuthorityCollaboratorsAreNotVisibleOutsideDomain`; suite 631/631. Deriva doc↔código: `TryAnalyze`→`Analyze` en el plan y la referencia a `UrlHostPolicy` de [decisions.md → «Riesgos aceptados»](../../../context/decisions.md#riesgos-aceptados) reapuntada a `UriAuthorityPolicy`; sin cambios de regla |
| T12 | Adecuar la nomenclatura de los rechazos del ajuste de URL a su causa: host presente con formato no admitido, puerto no válido y autoridad ausente pasan a nombrarse cada uno con su excepción, mediante un ayudante privado en `LinkUrlPolicy` | CA01, CA05 | T11 | `AdjustNamesEachRejectionAfterItsCause` y `SpecificationExamplesAreRejectedByAdjustmentOrValidation` en verde; comprobación negativa rompiendo la taxonomía | completada | 13-X-2026; filtro `Domain.Policies` 542/542; suite 639/639 |
| T13 | Ajustes menores de la revisión de `src/` dentro del alcance: comentario en `UrlInputPolicy.Prepare` sobre el uso de la normalización como detector y test de regresión `NormalizeDoesNotLeakAProviderExceptionForALoneSurrogate` | CA01, CA05 | T11 | Test nuevo en verde; suite completa | completada | 13-X-2026; filtro `Domain.Policies` 542/542 |
| T14 | Alinear con la regla de [architecture.md → «Domain»](../../../context/architecture.md#domain) las dos excepciones genéricas que quedaban: `NotValidInputException` para la base en blanco de `ImageUrlPolicy` y `UrlNotAbsoluteException` para la entrada no absoluta de `UrlNormalizationPolicy` | CA01, CA05 | T13 | Búsqueda de `throw new` de excepciones genéricas en Domain: ninguna; pruebas en verde | completada | 13-X-2026; filtro `Domain.Policies` 542/542; suite 639/639; actualizadas `NormalizeDoesNotRevalidateAnAlreadyAdjustedUrl` y `ResolveRelativeRejectsABaseUrlThatIsNullOrWhitespace` |
| T15 | Revisión y aceptación humana del resultado (DoD de S3a) | Todos | T14 | Decisión registrada por DLG | completada | Aceptada por DLG el 15-X-2026 |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01–T07, T10–T14 | V01, V02 | El resultado cumple las reglas aprobadas; diferencias respecto al código previo: regla de puerto solo dígitos en T03, nomenclatura de excepción para rechazos en T12 y excepciones de dominio alineadas en T14 | 13-X-2026; .NET 10.0.12 | Salida de `dotnet test Linkubator.sln --no-restore --verbosity minimal`; casos de puerto cubiertos en T09 y cambios de excepción detallados en T12 y T14 |
| CA02 | T04–T06, T11 | V04 | Cero referencias a `UriSchemePolicy` y `UrlHostPolicy` en `src` y `tests`; consumidores públicos de producción entran por la fachada, y `UriAuthoritySyntax` usa los formatos internos de host y puerto | 13-X-2026 | Búsqueda en el repositorio y revisión del árbol de llamadas |
| CA03 | T08 | V04 | Una sola definición de la lista de esquemas admitidos | 13-X-2026 | `UriAuthorityPolicy.cs`, método `IsAllowedScheme` |
| CA04 | T07, T09 | V05 | `UrlInputPolicyTests` y los tests de espacios interiores pasan | 13-X-2026 | 523/523 en el filtro `Domain.Policies` |
| CA05 | T02, T09, T11 | V05, V06 | Ningún miembro público sin consumidor productivo ni test; los colaboradores internos quedan fijados como no públicos | 13-X-2026 | Revisión del árbol de llamadas y test `AuthorityCollaboratorsAreNotVisibleOutsideDomain` |
| CA06 | T10, T11 | V01, V03 | Suite completa: 727/727 correctas, 0 fallidas, 0 omitidas. Build completo con `-warnaserror`: correcto, sin errores ni warnings | 15-X-2026; .NET 10.0.12 | [TRX de la ejecución](evidence/ca06-2026-10-15.trx); salida de terminal de `dotnet test Linkubator.sln --no-restore --verbosity minimal` y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal`. Esta ejecución completa posterior es la evidencia vigente de CA06; no se usa como resultado vigente el TRX previo de `tests/Linkubator.Tests/TestResults/test_results.trx`. |

## Aceptación y cierre

- Estado del trabajo: aceptado; implementación completada.
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos y riesgos restantes: ninguno comprobado para la implementación, CA06 o aceptación. La decisión sobre el commit del WIP previo y de este refactor no bloquea el cierre documental.
- Aceptación humana: DLG, 15-X-2026; referencia: [mvp0-plan.md → «S3a: Dominio IV: refactor (M)»](../../../plans/mvp0-plan.md#s3a-dominio-iv-refactor-m), donde la Definition of Done consta completada; esta aceptación también sirve como referencia de la orden de archivado recibida.
- Archivado: completado.
- Ubicación archivada: `specs/archive/s3a-refactor-de-dominio/`.
- Fecha efectiva de archivado y comprobación de enlaces: 15-X-2026; traslado verificado y referencias actualizadas.
- Siguiente paso autorizado, sin cerrar otras etapas: ninguno para S3a.1; el archivado no declara cerrado ningún trabajo vecino.
