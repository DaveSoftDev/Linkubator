# Tareas: S5.9 — Revisión manual de migraciones y esquema SQLite

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 19-X-2026 («Siguiente»).
    - Tareas originales: aprobadas por DLG el 19-X-2026 («Aprobado»), instrucción explícita en esta conversación.
    - Revisión de V01/V02 del plan: aprobada por DLG el 19-X-2026 («Siguiente»).
    - Tareas actualizadas: re-aprobadas por DLG el 19-X-2026 («Aprobado»), instrucción explícita en esta conversación.
- Implementación:
    - Estado: completada.
    - Inicio: 19-X-2026, tras la aprobación de tareas por DLG («Aprobado»).
    - Finalización: 19-X-2026; V01 pasó 1/1, V02 pasó 5/5 y la revisión manual de V004 no encontró discrepancias.
    - Evidencia consolidada: T04–T05; historial de una migración antes del aprovisionamiento, esquema comprobado por Microsoft.Data.Sqlite y revisión de V004.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

DLG decidió excluir `V001`–`V003` del catálogo aplicado en producción («Excluir», 19-X-2026). La decisión se implementó y aceptó en [S5.2.1](../s5.2.1-separar-scripts-prueba-catalogo/tasks.md#aceptación-y-cierre) («Completada», 19-X-2026); el catálogo ya solo incluye V004. DLG aprobó la revisión V01/V02 del plan («Siguiente») y re-aprobó las tareas actualizadas («Aprobado»), 19-X-2026. Se reanuda T04 con el procedimiento revisado; la base temporal previa se conserva, pero V01 creará su propia base aislada.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Registrar la decisión explícita de DLG de excluir `V001`–`V003` del catálogo aplicado en producción y comprobar el estado del catálogo actual. | CA01–CA02; [plan → Decisiones técnicas y riesgos](plan.md#decisiones-técnicas-y-riesgos) | Decisión de DLG | Contrastar la decisión con el catálogo incrustado y registrar el prerrequisito que impide iniciar la revisión; resultado esperado: decisión inequívoca y discrepancia técnica identificada. | completada | DLG eligió «Excluir» el 19-X-2026; S5.2.1 confirmó catálogo solo con V004 y aceptación «Completada» el 19-X-2026. |
| T02 | Corregir el catálogo aplicado en producción para excluir los scripts de prueba mediante un trabajo SDD aprobado, sin alterar silenciosamente el alcance de S5.9. | CA01–CA02; [plan → Decisiones técnicas y riesgos](plan.md#decisiones-técnicas-y-riesgos) | T01; trabajo SDD de corrección completado y aceptado | Comprobar que el catálogo de producción solo incorpora las migraciones aprobadas y que las pruebas del ejecutor siguen cubriendo orden/idempotencia/fallo con scripts aislados; resultado esperado: prerrequisito disponible sin migraciones `sample_data`. | completada | Prerrequisito satisfecho por [S5.2.1](../s5.2.1-separar-scripts-prueba-catalogo/tasks.md#aceptación-y-cierre), implementada, validada 1046/1046 y aceptada por DLG («Completada», 19-X-2026). |
| T03 | Revisar y aprobar estas tareas cuando T02 esté completada, confirmando cobertura, dependencias, checks y límites de alcance. | CA01–CA04; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T02 | Revisar trazabilidad criterio → validación → tarea y registrar aprobación explícita; resultado esperado: autorización separada de la aprobación de spec y plan. | completada | DLG aprobó las tareas con «Aprobado», 19-X-2026. |
| T04 | Ejecutar el test de arranque Web sobre una base SQLite temporal vacía y registrar las migraciones aplicadas. | CA01; [plan → Flujos internos](plan.md#flujos-internos), V01 | T03, T08 | Ejecutar el test del plan; resultado esperado: el host de pruebas migra la base vacía antes de provisionar y el historial muestra las versiones esperadas. | completada | 19-X-2026: `Startup_WhenDatabaseIsNew_AppliesMigrationsBeforeProvisioningAndServing` pasó 1/1; base de `WebApplicationFactory` aislada, una migración registrada antes del aprovisionamiento y servicio. |
| T05 | Inspeccionar el esquema mediante los tests existentes con `Microsoft.Data.Sqlite` y revisar manualmente `V004_CreateUserCollectionLinkSchema.sql`. | CA02–CA03; [plan → Garantías técnicas](plan.md#garantías-técnicas), V02 | T04 | Ejecutar `SqliteMigrationServiceTests` y contrastar sus consultas de metadatos con el SQL y las fuentes; resultado esperado: esquema conforme o discrepancias registradas y bloqueantes. | completada | 19-X-2026: `SqliteMigrationServiceTests` pasó 5/5. Revisión manual de V004 contrastada con aserciones de columnas, índices, unicidades, claves foráneas, CHECK, defaults y ausencia de cascadas/triggers; sin discrepancias observadas. |
| T06 | Consolidar evidencia por criterio y actualizar los enlaces de estado del roadmap y del índice. | CA01–CA04; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V01–V02 | T05 | Comprobar que cada criterio tiene resultado real, fecha, herramienta y evidencia; resultado esperado: evidencia completa o bloqueos explícitos, sin atribuir pruebas de S5.1/S5.8 a esta revisión. | completada | 19-X-2026: CA01–CA04 enlazados a V01–V03; roadmap e índice sincronizados con la aceptación «Completada» de DLG. |
| T07 | Solicitar y registrar la aceptación humana del resultado de S5.9. | CA04; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre), V03 | T06 | Revisar evidencias y registrar decisión explícita de DLG; resultado esperado: aceptación de S5.9 o bloqueo y correcciones necesarias. | completada | DLG aceptó y cerró S5.9 con «Completada», 19-X-2026, instrucción explícita en esta conversación. |
| T08 | Revisar y aprobar las tareas actualizadas según la revisión aprobada de V01/V02. | CA01–CA04; [plan → Estrategia de validación](plan.md#estrategia-de-validación) | Revisión de V01/V02 aprobada en el plan | Confirmar que V01/V02, dependencias, checks y alcance reflejan el plan actualizado; resultado esperado: aprobación explícita de las tareas revisadas. | completada | DLG re-aprobó las tareas con «Aprobado», 19-X-2026. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01–T04, T06 | V01, [plan → Estrategia de validación](plan.md#estrategia-de-validación) | Cumplido | 19-X-2026, .NET 10 | `Startup_WhenDatabaseIsNew_AppliesMigrationsBeforeProvisioningAndServing` 1/1; el historial registra una migración antes del aprovisionamiento |
| CA02 | T01–T02, T05–T06 | V02, [plan → Estrategia de validación](plan.md#estrategia-de-validación) | Cumplido | 19-X-2026, .NET 10 | `SqliteMigrationServiceTests` 5/5; V004 y restricciones/índices contrastados sin discrepancias |
| CA03 | T05–T06 | V02, [plan → Estrategia de validación](plan.md#estrategia-de-validación) | Cumplido | 19-X-2026, .NET 10 | `SqliteMigrationServiceTests` 5/5; clave foránea compuesta y ausencia de cascadas/triggers verificadas |
| CA04 | T06–T07; verificación suplementaria | V03 y revisión suplementaria | Cumplido. Se completó una revisión reproducible en una base aislada identificada; el catálogo incrustado aplicó V004, se inspeccionó el esquema y no se observaron discrepancias con el SQL revisado. | 19-X-2026; .NET SDK 10.0.401, .NET runtime 10.0.12, Microsoft.Data.Sqlite 10.0.0.0, SQLite 3.53.3 | Base temporal `s59-review-20261019-003.db` (eliminada tras inspección). Se registraron historial `V004`, tablas `User`, `Collection`, `Link`, índices, columnas, claves foráneas `RESTRICT`, ausencia de triggers/vistas e `integrity_check=ok`. El SQL revisado fue `V004_CreateUserCollectionLinkSchema.sql`; no se hallaron discrepancias. Esta comprobación suplementa la revisión aceptada previamente y no altera su fecha ni referencia. |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: cerrado y aceptado por DLG («Completada», 19-X-2026).
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA04 tienen evidencia satisfactoria, incluida la comprobación suplementaria de CA04.
- Bloqueos y riesgos restantes: ninguno conocido para el alcance S5.9. La comprobación suplementaria de CA04 está identificada como tal y no se atribuye a la revisión manual original.
- Aceptación humana: aceptada por DLG con «Completada», 19-X-2026, instrucción explícita en esta conversación.
- Archivado: completado el 19-X-2026; traslado y enlaces entrantes/salientes comprobados.
- Ubicación archivada: `specs/archive/s5.9-revision-manual-migraciones-esquema/`.
- Fecha efectiva de archivado y comprobación de enlaces: 19-X-2026; referencias recalculadas y verificadas.
- Siguiente paso autorizado, sin cerrar otras etapas: trasladar la carpeta S5.9 a `specs/archive/` y comprobar destinos y enlaces. La aceptación humana previa de DLG («Completada», 19-X-2026) se conserva sin cambios.