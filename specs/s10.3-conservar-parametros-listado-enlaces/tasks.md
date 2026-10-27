# Tareas: S10.3 — Conservar parámetros del listado de enlaces

## Referencias y autorización

- Estado: aprobadas y completadas.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 27-X-2026 («Siguiente»).
  - Tareas: DLG, 27-X-2026 («Vamos»), decisión explícita en esta conversación.
- Implementación:
  - Estado: completada.
  - Inicio: DLG, 27-X-2026 («Inicia la implementación»), instrucción explícita en esta conversación.
  - Finalización: 27-X-2026; `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkListingIntegrationTests" --nologo` pasó 40/40 y `git diff --check` no reportó errores.
  - Evidencia consolidada: [Evidencia por criterio](#evidencia-por-criterio), T01–T04 y [LinkListingIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkListingIntegrationTests.cs).
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T01 confirma la brecha observable del plan y la cobertura de pruebas de la página de enlaces. T02 ejecute y, si procede, corrige solo el punto mínimo necesario para que el formulario y la paginación conserven `c` y `q` sin arrastrar `p`. T03 valida el conjunto focalizado y revisa la trazabilidad documental. Son dependientes y no incluyen el buscador del dashboard, la suite transversal ni la revisión manual de trabajos vecinos.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar `GetLinks_WithSearch_PaginatesFilteredResultsAndPreservesCollection` y confirmar los casos que distinguen una búsqueda nueva desde otra página, la ausencia de `p` en el formulario y la conservación de `q`/`c` al paginar. | CA01–CA03; [componentes y contratos](plan.md#componentes-y-contratos), [V01–V02](plan.md#estrategia-de-validación) | Ninguna | Inspección del test y del flujo de GET para verificar que los asserts distinguen página, filtro y texto; no se cambia comportamiento productivo antes del cierre del análisis. | completada | El caso inspeccionado cubre la página filtrada, `q` visible, `c` presente y `p` ausente en el formulario; la validación final confirmó 40/40 en el conjunto focalizado. |
| T02 | Añadir o adaptar pruebas HTTP focalizadas en `LinkListingIntegrationTests` y corregir solo la mínima lógica de navegación y formulario si la comprobación lo requiere. | CA01–CA03; [flujos internos](plan.md#flujos-internos), [V01–V02](plan.md#estrategia-de-validación) | T01 | Ejecutar los casos focalizados del listado con consulta paginada; el formulario conserva colección y búsqueda, omite página y los enlaces de paginación llevan los filtros aplicables. | completada | La renderización y la navegación ya conservan `c` y `q` en `LinksModel.BuildPageUrl` y el formulario de `_SearchForm` no emite `p` cuando se hace una búsqueda nueva; la suite de integración focalizada pasó 40/40. |
| T03 | Validar el conjunto focalizado y revisar que criterios, tests, cambios y enlaces documentales trazan entre sí, actualizando la evidencia de esta tabla y la sección de cierre con resultados reales. | CA01–CA03; [V03](plan.md#estrategia-de-validación) | T02 | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkListingIntegrationTests" --nologo` y `git diff --check`; resultados satisfactorios y límites de cobertura registrados, sin declarar cobertura de S10.4–S10.6. | completada | 27-X-2026: 40/40 tests de listado pasados; `git diff --check` sin errores; la evidencia documental se actualizó. |
| T04 | Revisar la evidencia técnica y registrar la aceptación humana explícita para el cierre S10.3, sincronizando índice y roadmap sin cerrar otras etapas. | Todos los criterios; [aceptación y cierre](#aceptación-y-cierre) | T03 | Se confirma la decisión humana con la referencia «Completado» y la trazabilidad de índice, roadmap y tareas queda alineada. | completada | DLG aceptó oportunamente el trabajo con la referencia «Completado» en esta conversación; los documentos relacionados se actualizan en la misma sesión. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T02, T03 | V01, V03 en [plan técnico](plan.md#estrategia-de-validación) | Satisfactorio: el formulario de búsqueda conserva el filtro de colección aplicable y el texto enviado, y la nueva búsqueda no arrastra la página actual al analizar la URL. | 27-X-2026; Windows, .NET SDK 10.0.401, SQLite en memoria | `GetLinks_WithSearch_PaginatesFilteredResultsAndPreservesCollection`; comando ejecutado: `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkListingIntegrationTests" --nologo` (40/40). |
| CA02 | T01, T02, T03 | V02, V03 en [plan técnico](plan.md#estrategia-de-validación) | Satisfactorio: los enlaces de paginación conservan `c` y `q`, cambian `p` según la página y el HTML resultante mantiene el mismo filtro para la siguiente vista. | 27-X-2026; Windows, .NET SDK 10.0.401, SQLite en memoria | `GetLinks_WithSearch_PaginatesFilteredResultsAndPreservesCollection`; validación focalizada 40/40 y revisión del modelo de paginación en [Links.cshtml.cs](../../src/Linkubator.Web/Pages/App/Links.cshtml.cs). |
| CA03 | T01, T02, T03 | V01, V03 en [plan técnico](plan.md#estrategia-de-validación) | Satisfactorio: el formulario no incluye `p` y la nueva búsqueda reanuda la primera página sin perder el contexto aplicable. | 27-X-2026; Windows, .NET SDK 10.0.401, SQLite en memoria | `GetLinks_WithSearch_PaginatesFilteredResultsAndPreservesCollection` y el formulario compartido en [_SearchForm.cshtml](../../src/Linkubator.Web/Pages/Shared/_SearchForm.cshtml); validación focalizada 40/40. |

La ejecución focalizada acredita el HTML resultante y la navegación generada; no cubre la integración del buscador del dashboard ni la suite transversal S10.5. La validación del trabajo queda limitada al flujo de listado de enlaces y sus criterios vigentes.

## Aceptación y cierre

- Estado del trabajo: aceptado.
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA03 cuentan con evidencia técnica satisfactoria.
- Bloqueos y riesgos restantes: ninguno conocidos dentro del alcance S10.3; la validación se limita al listado privado de enlaces y no declara cobertura de S10.4–S10.6.
- Aceptación humana: DLG, 27-X-2026 («Completado»), referencia explícita en esta conversación.
- Archivado: no solicitado.
- Ubicación archivada: no aplica; la carpeta permanece activa.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes; solo tras solicitud de archivo.
- Siguiente paso autorizado: continuar con el resto de S10 según roadmap. S10.3 queda aceptada y no cierra la etapa S10 ni autoriza el archivado.

