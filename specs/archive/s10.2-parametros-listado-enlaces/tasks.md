# Tareas: S10.2 — Parámetros del listado de enlaces

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»), decisión explícita en esta conversación.
  - Plan: [plan.md](plan.md), aprobado por DLG el 27-X-2026 («Siguiente»), decisión explícita en esta conversación.
  - Tareas: DLG, 27-X-2026 («Vamos»), decisión explícita en esta conversación.
- Implementación:
  - Estado: completada.
  - Inicio: DLG, 27-X-2026 («Inicia la implementación»), instrucción explícita en esta conversación.
  - Finalización: 27-X-2026; `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkListingIntegrationTests|FullyQualifiedName~PrivateListingQueryAnalyzerTests" --nologo` pasó 56/56 y `git diff --check` pasó.
  - Evidencia consolidada: [Evidencia por criterio](#evidencia-por-criterio), T01–T03 y [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación).
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T01 verifica los comportamientos de los parámetros y añade las pruebas unitarias o HTTP necesarias; T02 implementa correcciones limitadas a cualquier fallo demostrado; T03 ejecuta las validaciones focalizadas y sincroniza la trazabilidad; T04 presenta la evidencia para aceptación humana. Los tests preexistentes son una base a inspeccionar, no evidencia de cierre hasta ejecutar las pruebas correspondientes tras aprobar las tareas.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Confirmar y, si falta cobertura, añadir tests del analizador y de integración HTTP para `e` inválido/repetido ignorado, `c` inexistente y ajeno ignorado con parámetros válidos preservados, y `q` en el límite y excedido. | CA01–CA03; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [V01–V03](plan.md#estrategia-de-validación). | Ninguna | Ejecutar los casos focalizados nuevos/seleccionados; los resultados prueban las expectativas de cada criterio y `q` excedido genera el resumen accesible sin búsqueda. | completada | Suite focalizada ejecutada el 27-X-2026: 56/56 tests correctos, incluidos los casos añadidos de `e` inválido repetido, error accesible de `q` excesivo y límite exacto de puntos Unicode. Windows, .NET SDK 10.0.401. |
| T02 | Corregir quirúrgicamente el analizador o su integración en la página solo si T01 demuestra una carencia; no añadir soporte de etiquetas ni alterar reglas ajenas al alcance. | CA01–CA03; [plan.md → «Flujos internos»](plan.md#flujos-internos), [«Garantías técnicas»](plan.md#garantías-técnicas). | T01 | Los tests afectados pasan; `e` no filtra, `c` no accesible se ignora y el error de `q` impide consultar con el valor excesivo. Si no se requiere código productivo, dejar constancia verificable y pasar a validación integrada. | completada | La revisión y los tests no revelaron carencias productivas: el analizador existente ignoraba `e`, descartaba `c` no accesible y rechazaba `q` excedido antes de llamar al servicio. No se requirieron cambios de código productivo; suite combinada 56/56 el 27-X-2026, Windows, .NET SDK 10.0.401. |
| T03 | Ejecutar las pruebas focalizadas combinadas, `git diff --check` y revisar cobertura criterio → test → evidencia y los enlaces documentales. | CA01–CA03; [plan.md → V01–V04](plan.md#estrategia-de-validación). | T01, T02 | Los tests de analizador/listado pasan, diff limpio, enlaces existentes resuelven y los resultados se registran sin atribuir garantías no comprobadas. | completada | 27-X-2026: tests focalizados 56/56; `git diff --check` sin errores; destinos relativos documentales comprobados. Windows, .NET SDK 10.0.401. |
| T04 | Revisar la evidencia con DLG, registrar aceptación o correcciones y sincronizar la trazabilidad de S10.2 en el roadmap e índice sin cerrar la etapa S10. | CA01–CA03; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre). | T03 | Se registra una decisión humana explícita o sus correcciones; roadmap e índice reflejan solo el estado comprobado. | completada | DLG aceptó S10.2 el 27-X-2026 con la referencia «Completado» en esta conversación; aceptación registrada y trazabilidad sincronizada. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T03 | V01 y V04 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Satisfactorio: la petición HTTP con `e=invalid&e=999` no produce error ni altera resultados filtrados por los parámetros aplicables; el analizador ignora `e` en el listado de enlaces. | 27-X-2026; Windows, .NET SDK 10.0.401 | `GetLinks_WithRepeatedAndUnsupportedParameters_UsesOnlyFirstApplicableValues`; `Analyze_WhenParameterIsNotApplicableToListing_IgnoresIt`; ejecución focalizada 56/56. |
| CA02 | T01, T03 | V02 y V04 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Satisfactorio: colecciones inexistentes y ajenas se ignoran; solo se muestran enlaces del usuario identificado y no se revela la colección ni el contenido ajeno. | 27-X-2026; Windows, .NET SDK 10.0.401, SQLite en memoria | `GetLinks_WithForeignOrMissingCollection_IgnoresFilterWithoutDisclosingPrivateData`; `Analyze_WhenCollectionIsEmptyOrNotAccessible_IgnoresItAndPreservesOtherValues`; ejecución focalizada 56/56. |
| CA03 | T01, T03 | V03 y V04 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Satisfactorio: `q` por encima del límite presenta el error en `role="alert"` y no muestra tarjetas; el analizador devuelve error y `SearchText` nulo. El caso de límite exacto acepta el texto. La inspección de `LinksModel.OnGet` confirma que los errores de análisis retornan antes de invocar el servicio de listado. | 27-X-2026; Windows, .NET SDK 10.0.401 | `GetLinks_WithOversizedSearch_ShowsAccessibleErrorWithoutResults`; `Analyze_WhenSearchExceedsUnicodeCodePointLimit_ReportsErrorWithoutSearchValue`; `Analyze_WhenSearchIsAtUnicodeCodePointLimit_AcceptsSearchText`; código en [Links.cshtml.cs](../../../src/Linkubator.Web/Pages/App/Links.cshtml.cs); ejecución focalizada 56/56. |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: aceptado.
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA03 cuentan con evidencia técnica satisfactoria.
- Bloqueos y riesgos restantes: ninguno conocido dentro del alcance S10.2.
- Aceptación humana: DLG, 27-X-2026 («Completado»), referencia explícita en esta conversación.
- Archivado: no solicitado.
- Ubicación archivada: pendiente; la carpeta permanece activa.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes; solo tras una solicitud de archivo.
- Siguiente paso autorizado: continuar con los trabajos vecinos de S10 según el roadmap. S10.2 queda aceptada; esto no cierra la etapa S10 ni autoriza el archivado.
