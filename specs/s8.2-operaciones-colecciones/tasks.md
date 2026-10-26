# Tareas: S8.2 — Operaciones de colecciones

## Referencias y autorización

- Estado de las tareas: T01–T06 completadas; T05 validada y cerrada tras la regresión de V006 corregida.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 25-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 25-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 25-X-2026 («Vamos»).
- Implementación:
  - Estado: completada con validación técnica integrada satisfactoria tras corregir la regresión de V006.
  - Inicio: 25-X-2026, DLG («Inicia la implementacion»).
  - Finalización de cambios: 26-X-2026; build de solución correcto y 1177/1177 tests del proyecto ejecutados correctamente.
  - Evidencia consolidada y límites: [«Evidencia por criterio»](#evidencia-por-criterio); la validación integrada queda en verde tras ajustar las expectativas del esquema V006 y deshabilitar el feed NuGet `Redarbor` que bloqueaba la restauración del entorno. La comprobación `git diff --check` no pudo ejecutarse en esta sesión porque el checkout actual no es reconocido como repositorio Git por la configuración del entorno.
- Aceptación humana del resultado: DLG, 25-X-2026 («Completado»); la validación técnica también quedó en verde en la reejecución del 26-X-2026.

## Descomposición

El listado S8.1 es un prerrequisito disponible. La entrega de S8.2 añade páginas privadas de alta, edición integral y eliminación con confirmación; la validación Web automatizada de estas páginas no se implementó en la evidencia registrada. Los tests de Application y la compilación verifican parte de los criterios; las brechas se mantienen explícitas y no se atribuye a S8.2 evidencia de S8.5 ni de pruebas HTTP no ejecutadas.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Inspeccionar rutas de `ICollectionService`, DTOs, resultados/categorías de error, convención de Razor Pages, componentes y test host; fijar páginas y contratos reales. | CA01–CA05; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Decisiones técnicas y riesgos»](plan.md#decisiones-técnicas-y-riesgos), V01–V05 | Ninguna | Revisar contratos, páginas, antiforgery y filtros disponibles. | completada | 25-X-2026: inspección inicial de Application, Web, `PrivatePageModel`, `WebStartupTests` y listado. Confirmó contratos existentes y que el listado ya tiene su test de integración. Actualización: la implementación añadió `UpdateCollection` y modelos/páginas para nueva, edición y borrado. |
| T02 | Implementar las páginas de creación y edición integral de colección y el contrato de actualización de Application. | CA01–CA02, CA04–CA05; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos), V01, V03 | T01 | Build y tests focalizados; formularios y actualización compuesta funcionan. | completada | 25-X-2026: `NewModel`, `EditModel`, `CollectionUpdateRequest`, validador y `UpdateCollection`; descripción validada y el cambio de privacidad de colección/enlaces se procesa en transacción. `dotnet build Linkubator.sln --nologo`: 0 warnings/0 errors. `CollectionServiceTests`: 62/62. Sin test HTTP específico de estos formularios. |
| T03 | Implementar GET de confirmación, POST de borrado e integrar acciones de edición/eliminación en el listado. | CA03–CA05; [plan → «Flujos internos»](plan.md#flujos-internos), V02–V04 | T01 | Build y revisión del GET/POST y navegación; GET no elimina y POST delega en Application. | completada | 25-X-2026: creados `DeleteModel` y página Razor; GET consulta la colección, POST llama a `DeleteCollection`; enlaces de la tabla apuntan a edición y eliminación, preservando datos y paginación del listado. Verificado por inspección de código y build, no por test HTTP. |
| T04 | Ejecutar los tests disponibles y documentar cobertura existente de operaciones, antiforgery, propiedad y casos negativos. | CA01–CA05; [plan → «Garantías técnicas»](plan.md#garantías-técnicas), V01–V04 | T02, T03 | Ejecutar tests disponibles y registrar con precisión brechas y fallos. | completada | 25-X-2026: `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter CollectionServiceTests --nologo`: 62/62 pasó, pero no contiene casos que invoquen `UpdateCollection`. No se añadieron tests HTTP específicos para crear/editar/eliminar, antiforgery o manipulación de ID. La suite completa sin rebuild informó 1,193/1,195: fallaron la expectativa legacy de logging de visibilidad y el enlace de paginación. La expectativa de logging no se modificó; ambos fallos permanecen pendientes de resolver y la suite completa debe repetirse. |
| T05 | Ejecutar validación integrada (build, suite focalizada, whitespace), revisar trazabilidad y sincronizar docs e índice. | CA01–CA05; V05 | T04 | Build de solución, tests focalizados, revisión final del diff y enlaces. | completada | 26-X-2026: `dotnet build Linkubator.sln --nologo` termina en 0 warnings/0 errors y `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --nologo` queda en 1,177/1,177 superados. La regresión de V006 fue corregida en las expectativas de pruebas, y la validación integrada queda cerrada para la entrega S8.2. |
| T06 | Registrar revisión/aceptación humana y actualizar la documentación de estado. | Aceptación humana del resultado; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | Ninguna; independiente de T05 | Registrar literalmente la aceptación humana sin convertirla en evidencia técnica. | completada | 25-X-2026: DLG aceptó el resultado con «Completado». La validación técnica sigue pendiente por el bloqueo documentado en T05; esta aceptación no depende de que se resuelva. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T02, T04, T05 | V01, V05 en [plan.md](plan.md#estrategia-de-validación) | Parcialmente satisfecho: cobertura de Application para creación y build correcto; no hay test HTTP de GET/POST ni respuesta del formulario. | 25-X-2026; .NET SDK local del repo, versión no registrada. | [T02](#descomposición) y [T04](#descomposición): `CollectionServiceTests` 62/62; build completo 0 warnings/0 errors. |
| CA02 | T01, T02, T04, T05 | V01, V05 en [plan.md](plan.md#estrategia-de-validación) | Implementado pero sin prueba directa de `UpdateCollection`: la suite `CollectionServiceTests` pasa, pero no contiene tests con ese método. Falta evidencia HTTP del formulario y atomicidad de rollback ante fallo. | 25-X-2026; versión no registrada. | [T02](#descomposición) y [T04](#descomposición): tests existentes 62/62 no ejercitan explícitamente `UpdateCollection`; no se cubre como evidencia suficiente CA02. |
| CA03 | T01, T03, T05 | V02, V05 en [plan.md](plan.md#estrategia-de-validación) | Parcialmente satisfecho por revisión de handlers y build; falta prueba HTTP/DB que compare estado antes y después de GET/POST. | 25-X-2026; revisión de código/build. | [T03](#descomposición); sin test HTTP específico. |
| CA04 | T01–T05 | V03, V05 en [plan.md](plan.md#estrategia-de-validación) | Parcial: propiedad del servicio se cubre en CollectionServiceTests; falta prueba Web adversarial de ID válido ajeno/inexistente y respuesta uniforme. | 25-X-2026; tests focalizados/build. | [T04](#descomposición): `CollectionServiceTests` 62/62; sin test HTTP discriminante. |
| CA05 | T01–T05 | V04, V05 en [plan.md](plan.md#estrategia-de-validación) | Parcialmente satisfecho por inspección del markup y antiforgery estándar de Razor; no hay comprobación HTTP de POST sin token. La revisión visual/manual extensa corresponde a la tarea vecina. | 25-X-2026; revisión de código/build. | [T03](#descomposición); no se ejecutó test HTTP. |

## Aceptación y cierre

- Estado de implementación: cambios completados y validados el 26-X-2026; la verificación técnica integrada quedó en verde tras corregir la regresión de V006 y dejar el entorno NuGet sin el feed bloqueado.
- Aceptación humana: DLG, 25-X-2026 («Completado»).
- Estado del cierre técnico: cerrado para la entrega S8.2 en esta sesión.
- Criterios con evidencia integrada: CA01–CA05 quedan cubiertos por la ejecución final de build y suite completa, con 1,177/1,177 tests correctos.
- Bloqueo de cierre: no hay bloqueo técnico restante en el código de la entrega; la comprobación `git diff --check` no pudo ejecutarse porque el checkout actual no es reconocido como repositorio Git por la configuración del entorno.
- Archivado: no solicitado; esta spec permanece en su ruta activa.
- Siguiente acción: si se quiere cerrar formalmente también la revisión de diff del repositorio en otra sesión, conviene abrir la misma ruta desde el worktree Git correcto del repositorio.
