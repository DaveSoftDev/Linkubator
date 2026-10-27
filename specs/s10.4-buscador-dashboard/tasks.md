# Tareas: S10.4 — Conectar el buscador del dashboard

## Referencias y autorización

- Estado: aprobadas.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 27-X-2026 («Siguiente»), decisión explícita en esta conversación.
  - Tareas: DLG, 27-X-2026 («Vamos»), decisión explícita en esta conversación.
- Implementación:
  - Estado: completada y aceptada.
  - Inicio: DLG, 27-X-2026 («Inicia la implementación»), instrucción explícita en esta conversación.
  - Finalización: 27-X-2026; pruebas focalizadas 42/42 y `git diff --check` satisfactorio.
  - Aceptación humana del resultado: DLG, 27-X-2026 («Completado»), instrucción explícita en esta conversación.
  - Evidencia consolidada: [T01–T04](#descomposición), [evidencia por criterio](#evidencia-por-criterio), [LinkListingIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkListingIntegrationTests.cs) y [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs).
- La aceptación cierra S10.4, pero no cierra la etapa S10 ni autoriza el archivado.

## Descomposición

T01 confirma la cobertura existente del formulario y precisa si hace falta una prueba focalizada que observe su envío y el flujo de búsqueda destino. T02 implementa únicamente la brecha demostrada y añade la comprobación inmediata. T03 ejecuta las validaciones del plan y revisa trazabilidad y cambios. T04 queda para la revisión y aceptación humana; no depende de trabajos S10 posteriores.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Contrastar el formulario del dashboard y `AppDashboard_RendersSearchAndDatabaseBackedEmptyState` con CA01 y CA03; determinar si la prueba actual demuestra los campos y el destino exigidos y si queda pendiente comprobar la navegación con una consulta. | CA01, CA03; [componentes y contratos](plan.md#componentes-y-contratos), [V01–V02](plan.md#estrategia-de-validación) | Ninguna | Revisar las aserciones y el flujo real, separando lo probado de lo que solo se infiere; resultado: brechas y cobertura existente documentadas antes de cambios. | completada | La inspección confirmó que la prueba ya existente revisa `action="/app/enlaces"` y `name="q"`, pero no el método ni accesibilidad, y tampoco envía una consulta ni observa los resultados del listado. Véase [V01–V02](plan.md#estrategia-de-validación); no se afirma ejecución de tests. |
| T02 | Añadir una prueba de integración Web que compruebe el método GET, el propósito accesible, el envío de `q` desde el dashboard y los resultados al llegar al listado; el formulario ya configuraba el destino y no requirió cambios productivos. | CA01–CA03; [flujos internos](plan.md#flujos-internos), [garantías técnicas](plan.md#garantías-técnicas), [V01–V02](plan.md#estrategia-de-validación) | T01 | Ejecutar las pruebas focalizadas del dashboard y del listado; resultado: el GET lleva la consulta al listado, el destino la procesa por su flujo actual y mantiene el aislamiento por identidad, sin resultados de búsqueda en el dashboard. | completada | Se añadió `GetDashboardSearch_SubmitsQueryToOwnedLinkListing`; las pruebas focalizadas pasaron 42/42. No se modificó código productivo. |
| T03 | Ejecutar las validaciones automatizadas y revisión técnica definidas en V01–V03; registrar resultados, límites y enlaces a evidencia, y sincronizar el roadmap con el estado real de implementación. | CA01–CA03; [V01–V03](plan.md#estrategia-de-validación) | T02 | Ejecutar `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkListingIntegrationTests|FullyQualifiedName~WebStartupTests.AppDashboard_RendersSearchAndDatabaseBackedEmptyState" --no-restore` y `git diff --check`; resultado satisfactorio, con trazabilidad completa y sin atribuir cobertura de S10.5 o S10.6. | completada | 27-X-2026: 42/42 tests focalizados pasaron; `git diff --check` sin errores. El comando emitió una advertencia de normalización LF/CRLF sobre `src/Linkubator.Web/wwwroot/css/styles.css`; no se produjeron errores de diff. |
| T04 | Revisar la evidencia y los criterios y registrar la aceptación humana del resultado. | CA01–CA03; [aceptación y cierre](#aceptación-y-cierre) | T03 | Confirmar que todos los criterios tienen evidencia satisfactoria y registrar la decisión humana explícita. | completada | DLG aceptó S10.4 el 27-X-2026 con la referencia «Completado», explícita en esta conversación. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01–T03 | [V01–V02](plan.md#estrategia-de-validación) | Satisfactorio: el formulario GET del dashboard envía `q` a `/app/enlaces` y una consulta realizada desde ese destino devuelve el resultado coincidente. | 27-X-2026; .NET SDK 10.0.401, SQLite en memoria | `GetDashboardSearch_SubmitsQueryToOwnedLinkListing`; conjunto focalizado 42/42. |
| CA02 | T01–T03 | [V02](plan.md#estrategia-de-validación) | Satisfactorio: el listado muestra el enlace coincidente del usuario identificado y omite el enlace no coincidente y el resultado coincidente de otro usuario. | 27-X-2026; .NET SDK 10.0.401, SQLite en memoria | `GetDashboardSearch_SubmitsQueryToOwnedLinkListing`; conjunto focalizado 42/42. |
| CA03 | T01–T03 | [V01, V03](plan.md#estrategia-de-validación) | Satisfactorio: el formulario renderiza destino, método implícito GET, rol/nombre accesible y el campo `q`, sin `p`; el dashboard no recibe resultados de la consulta. | 27-X-2026; .NET SDK 10.0.401, SQLite en memoria | `GetDashboardSearch_SubmitsQueryToOwnedLinkListing`, `AppDashboard_RendersSearchAndDatabaseBackedEmptyState`; conjunto focalizado 42/42. |

Un resultado no disponible o fallido seguirá pendiente o bloqueado; los resultados esperados no se registran como resultados reales. Las pruebas focalizadas de esta spec no sustituyen las pruebas transversales de S10.5 ni la revisión manual de S10.6.

## Aceptación y cierre

- Estado del trabajo: aceptado y completado.
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA03 cuentan con validación focalizada satisfactoria.
- Bloqueos y riesgos restantes: ninguno conocido dentro del alcance; la validación focalizada no sustituye la suite transversal S10.5 ni la revisión manual S10.6, que son trabajos separados.
- Aceptación humana: DLG, 27-X-2026 («Completado»), instrucción explícita en esta conversación.
- Archivado: no solicitado.
- Ubicación archivada: no aplica; la carpeta permanece activa.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes; solo tras una solicitud de archivado.
- Siguiente paso autorizado: continuar con los trabajos restantes de S10; S10.4 queda completada y no cierra la etapa S10.
