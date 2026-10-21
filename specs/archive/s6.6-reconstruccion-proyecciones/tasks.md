# Tareas: S6.6 — Reconstrucción manual de proyecciones FTS5

## Referencias y autorización

- Estado: aprobadas.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.
  - Plan: [plan.md](plan.md), aprobado por DLG el 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.
  - Tareas: aprobadas por DLG el 20-X-2026 («Vamos»), instrucción explícita en esta conversación.
- Implementación:
  - Estado: completada.
  - Inicio: DLG, 20-X-2026 («Inicia la implementación»), instrucción explícita en esta conversación.
  - Finalización técnica: 20-X-2026; tests focalizados 5/5, build de solución correcto (0 warnings/errors), CLI comprobada con SQLite 3.53.3 y `git diff --check` limpio; referencias en «Evidencia por criterio».
  - Evidencia consolidada: T03–T05 completadas; tests focalizados, CLI temporal, build, revisión de arranque y diff limpio registrados en la matriz de criterios.
- La preparación de tareas no inicia la implementación ni inventa aprobaciones o evidencias.

## Descomposición

T01 inspeccionó la factoría/UoW, el host Web, el fixture SQLite y la herramienta local de S5.10. Se reutilizará el patrón de consola .NET bajo `tools/`, con un servicio explícito en Infrastructure y sin llamada desde el arranque Web. La implementación y las pruebas dependen de esa decisión; la sincronización ordinaria y la prueba manual local de S6.9 permanecen en sus tareas propietarias.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Inspeccionar la infraestructura SQLite/UoW y la estructura de tests para elegir un punto de invocación manual y el fixture de integración existente. Registrar decisión de ubicación y comandos focalizados concretos sin implementar aún el comando. | CA03; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Estrategia de validación»](plan.md#estrategia-de-validación) | Ninguna | Inspección de los proyectos Infrastructure/Web y tests: identificar conexión, transacción, migraciones y fixture SQLite; confirmar una vía de ejecución manual que no se invoque en startup. Resultado: ubicación y filtros de validación documentados. | completada | Revisados SqliteUnitOfWork, SqliteConnectionFactory, SqliteMigrationService, composition root de Web, fixture SqliteRepositoryTestDatabase y tooling `tools/s510-manual-review`; criterio CA03 verificable con inspección sin introducir cambios. 20-X-2026 |
| T02 | Implementar `ISqliteSearchProjectionRebuilder`/`SqliteSearchProjectionRebuilder` y la utilidad de consola manual separada para reconstruir enlaces y colecciones de una base SQLite existente. | CA01, CA02, CA03; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos) | T01 | Construir Infrastructure y la herramienta; comprobar la validación de argumento/base inexistente y revisar que no se añade integración al startup Web. | completada | Contrato y servicio transaccional implementados; CLI ejercitada con base SQLite temporal inicializada (SQLite 3.53.3) y reconstrucción correcta. También se comprobó rechazo de ruta inexistente sin crear un archivo. La suite focalizada ampliada pasó 5/5. 20-X-2026 |
| T03 | Añadir pruebas de integración SQLite que alteren las proyecciones, ejecuten la reconstrucción y comprueben reparación, repetibilidad, separación de usuarios, valores nulos y normalización NFC. | CA01; [plan.md → V01](plan.md#estrategia-de-validación) | T02 | Ejecutar `dotnet test .\tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteSearchProjectionRebuilderTests`; debe reparar ambas proyecciones y preservar los datos de entidad. | completada | `SqliteSearchProjectionRebuilderTests` pasó 5/5 el 20-X-2026. Se cubren reparación de ambas entidades, NFC y conservación del original, repetición idempotente con dos usuarios sin filas obsoletas y campos opcionales nulos. |
| T04 | Añadir prueba negativa de atomicidad con fallo inducido y verificar rollback total de ambas proyecciones. | CA02; [plan.md → V02](plan.md#estrategia-de-validación) | T02 | Ejecutar el mismo filtro focalizado: ante el fallo, las dos proyecciones conservan sus valores previos; el fallo se induce haciendo que falte la tabla fuente de enlaces, sin trigger ni seam productivo. | completada | `Rebuild_WhenLinkProjectionInsertFails_RollsBackBothProjections` pasó dentro de la suite 5/5 el 20-X-2026; el error de ausencia de `Link` sucede tras vaciar y repoblar la proyección de colecciones y se verifica el rollback total. |
| T05 | Revisar el cableado de la operación y ejecutar validación técnica completa del alcance propio; registrar evidencia por criterio y sincronizar el avance del roadmap. | CA01–CA03; [plan.md → V03](plan.md#estrategia-de-validación), [«Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T03, T04 | Ejecutar tests focalizados, build de solución y `git diff --check`; revisar que la CLI no se cablea al startup Web; registrar resultados reales. | completada | `dotnet test .\tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteSearchProjectionRebuilderTests` 5/5; `dotnet build .\Linkubator.sln --no-restore` correcto, 0 warnings/errores; `git diff --check` limpio; búsqueda en Web `Program.cs` confirma ausencia de referencia al rebuilder. 20-X-2026 |
| T06 | Revisar evidencias, registrar aceptación humana del resultado y cerrar S6.6 para su alcance. | CA01–CA03; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T05 | Aceptación explícita de DLG: «Tenemos reconstrucción de proyecciones»; registrar fecha, validar que CA01–CA03 tienen evidencia satisfactoria y actualizar el roadmap. | completada | DLG aceptó y cerró S6.6 para su alcance el 20-X-2026: «Tenemos reconstrucción de proyecciones». |

Las pruebas de integración T03 y T04 pueden desarrollarse en paralelo una vez establecida la interfaz por T02, pero se ejecutan en T05 para consolidar resultados. La comprobación manual con datos locales pertenece a S6.9 y no era prerrequisito para cerrar S6.6.

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T03, T05 | V01; `dotnet test .\tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteSearchProjectionRebuilderTests` y ejecución CLI en base temporal | Satisfactoria: reconstruyó filas de enlaces y colecciones desde sus entidades; cubre repetición, aislamiento por usuario, valores nulos y NFC. Probada invocación manual real. | 20-X-2026; .NET 10 / SQLite 3.53.3 | Suite focalizada 5/5; herramienta CLI ejecutada sobre base temporal inicializada |
| CA02 | T02, T04, T05 | V02; `dotnet test .\tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteSearchProjectionRebuilderTests` | Satisfactoria: fallo al regenerar enlaces revirtió los cambios previos de ambas proyecciones. | 20-X-2026; .NET 10 / SQLite fixture | Suite focalizada `SqliteSearchProjectionRebuilderTests`, 5/5 |
| CA03 | T01, T02, T05 | V03; revisión de `src/Linkubator.Web/Program.cs`, refs por `Select-String` y `git diff --check` | Satisfactoria: ninguna referencia al rebuilder en el arranque Web; herramienta local separada y diff sin errores de whitespace. | 20-X-2026; .NET 10 / workspace | T05: build de solución correcto y revisión de referencias registrada |

## Aceptación y cierre

- Estado del trabajo: aceptado y cerrado para su alcance.
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA03 tienen evidencia técnica satisfactoria.
- Bloqueos y riesgos restantes: ninguno identificado dentro del alcance de S6.6.
- Aceptación humana: DLG, 20-X-2026 («Tenemos reconstrucción de proyecciones»), instrucción explícita en esta conversación.
- Archivado: completado el 21-X-2026; traslado y enlaces entrantes/salientes comprobados.
- Ubicación archivada: `specs/archive/s6.6-reconstruccion-proyecciones/`.
- Fecha efectiva de archivado y comprobación de enlaces: 21-X-2026; referencias recalculadas y verificadas.
- Siguiente paso autorizado, sin cerrar otras etapas: ninguno para S6.6; el trabajo está aceptado, cerrado y archivado. La comprobación manual S6.9 se completó en su etapa propietaria.
