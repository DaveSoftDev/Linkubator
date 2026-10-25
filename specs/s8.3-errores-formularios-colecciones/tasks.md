# Tareas: S8.3 — Errores de formularios de colecciones

## Referencias y autorización

- Estado: completada y aceptada.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 25-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 25-X-2026 («Siguiente»).
  - Tareas: DLG, 25-X-2026 («Vamos»).
  - Aceptación humana de cierre: DLG, 25-X-2026 («Completado»).
- Implementación:
  - Estado: completada.
  - Inicio: 25-X-2026.
  - Finalización: 25-X-2026, tras validación focalizada y cierre técnico de la evidencia.
  - Evidencia consolidada: [problemas de acceso](../../src/Linkubator.Web/Pages/App/Collections/Delete.cshtml.cs), [foco y errores de formulario](../../src/Linkubator.Web/Pages/App/Collections/New.cshtml), [edición con valores conservados](../../src/Linkubator.Web/Pages/App/Collections/Edit.cshtml.cs), [suite de integración](../../tests/Linkubator.Tests/Integration/CollectionsListingIntegrationTests.cs), [build Web](../../src/Linkubator.Web/Linkubator.Web.csproj) y ejecución de `dotnet test` de la suite focalizada con 12/12 casos correctos.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T01 identifica el contrato real de error y el mecanismo de pruebas Web/DOM antes de implementar. T02 y T03 son incrementos independientes de presentación: uno cubre errores de validación y foco en creación/edición; el otro el mensaje de rechazo en eliminación. T04 integra y valida los criterios que dependen de ambos.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Inspeccionar los resultados y categorías de `ICollectionService`, los campos de error, los componentes de presentación y el test host/herramienta DOM disponibles; documentar el mapeo final y filtros compatibles sin cambiar reglas. | CA01–CA04; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos), V01–V03 | Ninguna | Revisar contratos, Razor, tests Web y paquetes actuales; resultado: mapeo de error por campo/categoría y validación ejecutable identificados, o bloqueo concreto registrado. | completada | Cumplida tras inspección del `ICollectionService`, `PageModel` y test host; se confirmó el mapeo de validación, dominio y recurso no accesible y las rutas de acceso no divulgadas. |
| T02 | Implementar la presentación accesible de errores de validación y errores de dominio en crear/editar: asociación por campo, conservación de entrada, estado visual y foco en el primer campo erróneo; añadir pruebas focalizadas. | CA01–CA03; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos), V01–V02 | T01 | Ejecutar tests focalizados de creación/edición; resultado: errores asociados a controles correctos, valores conservados, foco según orden y motivo de rechazo visible; sin mutaciones ante rechazos. | completada | Implementado en [New.cshtml.cs](../../src/Linkubator.Web/Pages/App/Collections/New.cshtml.cs), [Edit.cshtml.cs](../../src/Linkubator.Web/Pages/App/Collections/Edit.cshtml.cs), [New.cshtml](../../src/Linkubator.Web/Pages/App/Collections/New.cshtml) y [Edit.cshtml](../../src/Linkubator.Web/Pages/App/Collections/Edit.cshtml); pruebas focalizadas y ejecución 12/12. |
| T03 | Implementar o ajustar la presentación del error de regla del dominio en el flujo de eliminación y añadir prueba de rechazo con comprobación del estado. | CA04; [plan → «Flujos internos»](plan.md#flujos-internos), V01 | T01 | Ejecutar prueba focalizada de eliminación; resultado: mensaje explicativo presente en la confirmación y colección persistida sin cambios. | completada | Implementado en [Delete.cshtml.cs](../../src/Linkubator.Web/Pages/App/Collections/Delete.cshtml.cs) y [Delete.cshtml](../../src/Linkubator.Web/Pages/App/Collections/Delete.cshtml); validado por la suite de integración con rechazo de eliminación y sin cambios persistidos. |
| T04 | Validar integrada y revisar trazabilidad, cobertura de criterios, estado persistido, foco, markup accesible y enlaces/documentación del trabajo. | CA01–CA04; [plan → «Garantías técnicas»](plan.md#garantías-técnicas), [«Estrategia de validación»](plan.md#estrategia-de-validación), V01–V03 | T02, T03 | Ejecutar build Web, tests focalizados compatibles con las pruebas implementadas y `git diff --check`; resultado: build y tests verdes, diff limpio y evidencia por criterio registrada sin atribuir cobertura no comprobada. | completada | `dotnet build src\Linkubator.Web\Linkubator.Web.csproj --nologo` => 0 warnings/0 errors. `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter CollectionsListingIntegrationTests --nologo` => 12/12 pruebas correctas. |
| T05 | Actualizar [roadmap.md](../../context/roadmap.md) con la referencia y el estado comprobado de S8.3, y revisar el índice de specs. | CA01–CA04; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T04 | Verificar enlaces, aprobación y estado sin modificar el estado global de S8 ni declarar aceptación no otorgada. | completada | Se sincronizó el índice de specs y el roadmap con el estado real de S8.3. |
| T06 | Revisar evidencia y registrar aceptación humana o, si se rechaza, correcciones y bloqueo; sincronizar el estado de cierre. | CA01–CA04; [spec → «Criterios de aceptación»](spec.md#criterios-de-aceptación) | T04, T05 | Confirmar que toda evidencia satisface los criterios dentro del alcance; aceptar solo con decisión explícita registrada. | completada | DLG aceptó el cierre con la referencia «Completado» el 25-X-2026; la evidencia del trabajo y el estado final quedan registrados. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T02, T04 | V01, V03 en [plan.md](plan.md#estrategia-de-validación) | Completado | 25-X-2026, Web local + `dotnet test` focalizada | [CollectionsListingIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/CollectionsListingIntegrationTests.cs) y [New.cshtml.cs](../../src/Linkubator.Web/Pages/App/Collections/New.cshtml.cs) |
| CA02 | T01, T02, T04 | V02, V03 en [plan.md](plan.md#estrategia-de-validación) | Completado | 25-X-2026, Web local + validación del DOM de formulario | [New.cshtml](../../src/Linkubator.Web/Pages/App/Collections/New.cshtml) y [Edit.cshtml](../../src/Linkubator.Web/Pages/App/Collections/Edit.cshtml) |
| CA03 | T01, T02, T04 | V01, V03 en [plan.md](plan.md#estrategia-de-validación) | Completado | 25-X-2026, tests de integración de creación/edición con rechazo de dominio | [collections tests](../../tests/Linkubator.Tests/Integration/CollectionsListingIntegrationTests.cs) |
| CA04 | T01, T03, T04 | V01, V03 en [plan.md](plan.md#estrategia-de-validación) | Completado | 25-X-2026, flujo de confirmación y validación de estado persistido | [Delete.cshtml.cs](../../src/Linkubator.Web/Pages/App/Collections/Delete.cshtml.cs) y [Delete.cshtml](../../src/Linkubator.Web/Pages/App/Collections/Delete.cshtml) |

La evidencia disponible demuestra cumplimiento del alcance y no se registra ninguna regresión ni resultado no verificado.

## Aceptación y cierre

- Estado del trabajo: completado. No confundirlo con los estados de cada tarea.
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos y riesgos restantes: ninguno confirmado dentro del alcance de S8.3.
- Aceptación humana: DLG, 25-X-2026 («Completado»).
- Archivado: no solicitado.
- Ubicación archivada: no aplica.
- Fecha efectiva de archivado y comprobación de enlaces: no aplica.
- Siguiente paso autorizado: cierre del trabajo y paso a la siguiente etapa del roadmap.
