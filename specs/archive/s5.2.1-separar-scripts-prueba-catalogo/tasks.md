# Tareas: S5.2.1 — Separar scripts de prueba del catálogo de migraciones

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 19-X-2026 («Siguiente»).
    - Tareas: aprobadas por DLG el 19-X-2026 («Aprobado»), instrucción explícita en esta conversación.
- Implementación:
    - Estado: completada.
    - Inicio: 19-X-2026, al iniciar T02 tras la aprobación de tareas por DLG («Aprobado»).
    - Finalización: 19-X-2026, suite completa 1046/1046 y `dotnet build Linkubator.sln --no-restore --verbosity minimal` correctos.
    - Evidencia consolidada: T02–T05; catálogo V004, esquema sin `sample_data`, compatibilidad con historial previo y suite/build satisfactorios.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

La aplicación se limita a retirar los scripts de muestra del catálogo de producción y preservar las pruebas automatizadas y los datos existentes. No se modifica el comportamiento del ejecutor ni se borra contenido de bases locales.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas, confirmando trazabilidad, compatibilidad del historial existente y límites del cambio. | CA01–CA04; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Revisar criterios, checks y ausencia de borrados de bases existentes; resultado esperado: aprobación explícita de tareas separada de spec y plan. | completada | DLG aprobó las tareas con «Aprobado», 19-X-2026. |
| T02 | Retirar los recursos SQL `V001`–`V003` del catálogo de producción y actualizar las aserciones que enumeran migraciones reales o inspeccionan el esquema resultante. | CA01, CA03 y CA04; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V01 y V03 | T01 | Ejecutar las pruebas focalizadas del catálogo/esquema; resultado esperado: solo aparece la migración real del esquema y `sample_data` no existe en una base vacía. | completada | 19-X-2026: `ProvisioningSqlResource_IsEmbeddedOutsideVersionedMigrationCatalog` 1/1 y `ApplyPendingMigrations_WhenUserCollectionLinkSchemaIsPresent_ExposesExpectedStructure` 1/1; catálogo solo expone V004 y el esquema verifica el conjunto exacto de tablas. |
| T03 | Añadir una prueba de compatibilidad para una base temporal cuyo historial previo contiene `V001`–`V004`, incluyendo una fila de `sample_data`. | CA04; [plan → Garantías técnicas](plan.md#garantías-técnicas), V04 | T01 | Aplicar el catálogo actual a esa base; resultado esperado: el migrador no falla por versiones retiradas ni borra el historial, la tabla o sus datos existentes. | completada | 19-X-2026: `SqliteMigrationServiceTests` pasó 5/5, incluida `ApplyPendingMigrations_WhenLegacyVersionsAreRecorded_PreservesHistoryAndSampleData`; historial V001–V004 y fila legacy conservados. |
| T04 | Ejecutar las pruebas del ejecutor con scripts suministrados directamente al servicio y las pruebas integradas que consumen el catálogo real. | CA01–CA04; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V01–V04 | T02, T03 | Ejecutar los filtros focalizados; resultado esperado: orden, idempotencia, fallo, esquema, identidad y compatibilidad pasan. | completada | 19-X-2026: 72/72 tests en `DevelopmentIdentityProviderTests`, `DevelopmentIdentityProvisionerTests`, `SqliteMigrationServiceTests` y consumidores de `SqliteRepositoryTestDatabase`; incluye orden/idempotencia/fallo, catálogo V004, esquema sin `sample_data` y compatibilidad del historial V001–V004. |
| T05 | Ejecutar la suite completa y el build de solución, y consolidar la evidencia por criterio. | CA01–CA04; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V05 | T04 | Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --verbosity minimal` y `dotnet build Linkubator.sln --no-restore --verbosity minimal`; resultado esperado: suite y build correctos. | completada | 19-X-2026: suite completa 1046/1046 y `dotnet build Linkubator.sln --no-restore --verbosity minimal` correctos tras el ajuste async de T03. |
| T06 | Sincronizar el roadmap y el índice con la implementación y evidencia reales, y solicitar la aceptación humana. | CA01–CA04; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T05 | Verificar que el estado documental refleja únicamente resultados comprobados y solicitar decisión explícita de DLG; resultado esperado: aceptación o bloqueo registrado. | completada | DLG aceptó y cerró S5.2.1 con «Completada», 19-X-2026; roadmap e índice sincronizados. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T04–T05 | V01–V02, [plan → Estrategia de validación](plan.md#estrategia-de-validación) | Cumplido | 19-X-2026, .NET 10 | Catálogo solo con V004; tests focalizados y suite completa 1046/1046 |
| CA02 | T04–T05 | V02, [plan → Estrategia de validación](plan.md#estrategia-de-validación) | Cumplido | 19-X-2026, .NET 10 | Tests del ejecutor con fixtures en memoria y suite completa 1046/1046 |
| CA03 | T02, T04–T05 | V03, [plan → Estrategia de validación](plan.md#estrategia-de-validación) | Cumplido | 19-X-2026, .NET 10 | Test de esquema: conjunto exacto de tablas sin `sample_data`; suite completa 1046/1046 |
| CA04 | T02–T05 | V04–V05, [plan → Estrategia de validación](plan.md#estrategia-de-validación) | Cumplido | 19-X-2026, .NET 10 | Historial V001–V004 y fila de muestra conservados; consumers reales 72/72, suite completa 1046/1046 y build correcto |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: cerrado y aceptado por DLG («Completada», 19-X-2026).
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA04 tienen evidencia satisfactoria.
- Bloqueos y riesgos restantes: ninguno conocido dentro del alcance de S5.2.1.
- Aceptación humana: aceptada por DLG el 19-X-2026 («Completada»), instrucción explícita en esta conversación.
- Archivado: completado el 19-X-2026; traslado y enlaces entrantes/salientes comprobados.
- Ubicación archivada: `specs/archive/s5.2.1-separar-scripts-prueba-catalogo/`.
- Fecha efectiva de archivado y comprobación de enlaces: 19-X-2026; referencias recalculadas y verificadas.
- Siguiente paso autorizado, sin cerrar otras etapas: aprobar las tareas de S5.9 y realizar su revisión manual; el cierre de S5.2.1 no acepta ni cierra S5.9.