# Tareas: S6.5 — Consultas de búsqueda FTS5

## Referencias y autorización

- Estado: aprobadas.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.
  - Plan: [plan.md](plan.md), aprobado por DLG el 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.
  - Tareas: aprobadas por DLG el 20-X-2026 («Vamos»), instrucción explícita en esta conversación.
- Implementación:
  - Estado: completada y aceptada; aceptación «Consultas y búsquedas».
  - Inicio: DLG solicitó iniciar la implementación el 20-X-2026 («Inicia la implementación»), instrucción explícita en esta conversación.
  - Finalización: 20-X-2026; tests focalizados 61/61, suite completa 1081/1081 con .NET SDK 10.0.401 y `git diff --check` limpio. Sin cambios de esquema; SQL y trazabilidad revisados.
  - Evidencia consolidada: T01–T05 y validaciones V01–V04; detalle en «Evidencia por criterio».
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T01 determina la ubicación exacta de las pruebas y comandos focalizados existentes. Tras ella, T02 (consulta de enlaces) y T03 (consulta de colecciones) pueden avanzar en paralelo: usan repositorios y proyecciones diferentes. T04 verifica conjuntamente los criterios, incluido el acuerdo entre recuento y página; T05 actualiza la trazabilidad y prepara la revisión. No se incluye aquí la matriz general de S6.7 ni el trabajo de S6.6.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Inspeccionar las consultas y contratos actuales de enlaces y colecciones, el registro de la colación, los fixtures SQLite y filtros de tests. Determinar los escenarios faltantes y confirmar el comando focalizado disponible. | CA01–CA06; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Persistencia y dependencias»](plan.md#persistencia-y-dependencias), V01–V04 | Ninguna | Identificar métodos de página y recuento, filtros existentes, DTOs, esquema de proyecciones, factoría con colación y convenciones para sembrar datos deterministas; registrar brechas por criterio sin editar producción. | completada | Inspección 20-X-2026: las firmas de página/recuento existen en `ILinkRepository` e `ICollectionRepository`; ambos repositorios rechazaban búsquedas con términos mediante `ThrowIfSearchRequested`. El fixture `SqliteRepositoryTestDatabase` aplica migraciones, crea dos usuarios, siembra entidades y proyecciones normalizadas; la factoría registra `LINKUBATOR_ES_ES`. Tests focalizados: `SqliteLinkRepositoryTests` y `SqliteCollectionRepositoryTests`. Se confirmó el reemplazo de casos previos «ThrowsBeforeS6» por búsquedas reales. Inspección y filtros confirmados el 20-X-2026. |
| T02 | Integrar búsqueda por texto en la consulta paginada y el recuento de enlaces, usando la proyección propia, usuario, filtro de colección, orden aplicable y el caso sin términos. Añadir tests focalizados de resultados y controles negativos. | CA01–CA04; [plan → «Flujos internos»](plan.md#flujos-internos), V01–V02 | T01 | Los tests SQLite cubren coincidencias propias y ajenas, aislamiento, filtro, orden y desempates, resultados con texto y sin términos, página y recuento con filtros equivalentes. | completada | Se integró MATCH parametrizado a `LinkSearch`, filtro `UserId` en proyección y `Link`, filtro de colección, orden de relevancia y desempates; sin términos se conserva listado por fecha/Id y se evita MATCH. Tests ampliados de búsqueda, aislamiento, filtro y recuentos con página; 18 tests de enlaces pasan en conjunto el 20-X-2026. |
| T03 | Integrar búsqueda por texto en la consulta paginada y el recuento de colecciones, usando únicamente su proyección, el usuario, el orden alfabético, la paginación y el caso sin términos. Añadir tests focalizados de resultados y controles negativos. | CA05–CA06; [plan → «Flujos internos»](plan.md#flujos-internos), V03 | T01 | Los tests SQLite cubren coincidencias por nombre, aislamiento de usuario, orden alfabético y paginación con texto y sin términos. | completada | Se integró MATCH parametrizado a `CollectionSearch`, filtro `UserId` en índice y `Collection`, nombre como texto buscable y colación alfabética preservada; sin términos se consulta el listado sin MATCH. Tests de coincidencias, aislamiento, orden y recuentos con página. |
| T04 | Ejecutar los tests focalizados de ambos repositorios y revisar que página y recuento usan el mismo conjunto filtrado; revisar los SQL finales, la parametrización y el alcance de los cambios. | CA01–CA06; [plan → «Garantías técnicas»](plan.md#garantías-técnicas), V01–V04 | T02, T03 | Tests focalizados pasan; `git diff --check` está limpio; cada criterio tiene prueba diferenciadora; no hay cambios de esquema ni entradas interpoladas como SQL. | completada | Revisados SQL de página y conteo: MATCH solo contra proyección correspondiente, parámetros Dapper, `UserId` en proyección y entidad, filtro de colección, orden/relevancia y límites SQL. Sin cambios de esquema, migraciones o triggers. `git diff --check` limpio. Tests focalizados de repositorios más builder y normalizador: 61/61; suite completa `Linkubator.Tests`: 1081/1081, .NET SDK 10.0.401, 20-X-2026. Incluye controles negativos para divergencia de `UserId`, consultas por tipo, acentos/NFC, términos múltiples, filtros, orden/desempates, entradas sin términos y paginación. |
| T05 | Sincronizar la evidencia obtenida en estas tareas, confirmar enlaces y estado real de implementación en la navegación pertinente, y dejar registrada la aceptación humana final «Consultas y búsquedas». | CA01–CA06; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T04 | Evidencias enlazadas a validaciones realizadas; roadmap e índice reflejan el estado real. | completada | Evidencia real consolidada por criterio; enlaces relativos comprobados. El roadmap e índice enlazan la implementación completada y su aceptación «Consultas y búsquedas». |
| T06 | Registrar la aceptación humana final de S6.5. | CA01–CA06; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T05 | La decisión y su referencia se conservan explícitamente en el registro de aceptación; las pruebas por sí solas no cierran el trabajo. | completada | DLG aceptó S6.5 con «Consultas y búsquedas» el 20-X-2026, confirmado explícitamente en esta conversación. Los criterios CA01–CA06 cuentan con evidencia satisfactoria; implementación y validación completas. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T02, T04 | V01, V04 ([plan](plan.md#estrategia-de-validación)) | Verificado: las coincidencias se obtienen de `LinkSearch` y el listado devuelve solo enlaces. | 20-X-2026, .NET SDK 10.0.401; focalizada 61/61 y suite 1081/1081 | `GetPageByUser_WhenQueryMatchesLinks_ReturnsOnlyOwnedMatchesInRankOrder`; tests de repositorio de enlaces. |
| CA02 | T01, T02, T04 | V01, V04 ([plan](plan.md#estrategia-de-validación)) | Verificado: se excluyen coincidencias de otro usuario y el control con `UserId` adulterado en la proyección no devuelve la entidad ajena. | 20-X-2026, .NET SDK 10.0.401; focalizada 61/61 y suite 1081/1081 | `GetPageByUser_WhenProjectionUserDoesNotMatchLinkOwner_ExcludesTheResult`; SQL revisado. |
| CA03 | T01, T02, T04 | V02, V04 ([plan](plan.md#estrategia-de-validación)) | Verificado: relevancia, desempates de fecha/Id, términos múltiples y acentos, filtro y paginación se comprueban en SQLite; el recuento aplica coincidencias y filtro. | 20-X-2026, .NET SDK 10.0.401; focalizada 61/61 y suite 1081/1081 | Tests de orden, NFC, filtro y páginas/conteos en `SqliteLinkRepositoryTests`. |
| CA04 | T01, T02, T04 | V02, V04 ([plan](plan.md#estrategia-de-validación)) | Verificado: entrada sin términos omite búsqueda textual y mantiene filtro/orden no textual; el método de recuento usa el mismo ramal sin MATCH. | 20-X-2026, .NET SDK 10.0.401; focalizada 61/61 y suite 1081/1081 | `GetPageByUser_WhenQueryHasNoTerms_UsesCollectionFilterAndUnsearchedOrder`; SQL de conteo revisado. |
| CA05 | T01, T03, T04 | V03, V04 ([plan](plan.md#estrategia-de-validación)) | Verificado: la consulta usa solo `CollectionSearch`, aísla por usuario, conserva orden alfabético y no coincide con texto de enlaces. | 20-X-2026, .NET SDK 10.0.401; focalizada 61/61 y suite 1081/1081 | Tests de búsqueda, orden y `GetPageByUser_WhenLinkCountMatchesSearchText_DoesNotMatchCollection`; test negativo de `UserId`. |
| CA06 | T01, T03, T04 | V03, V04 ([plan](plan.md#estrategia-de-validación)) | Verificado: sin términos conserva el listado alfabético; una búsqueda multipágina se pagina y su recuento devuelve el número esperado de páginas. | 20-X-2026, .NET SDK 10.0.401; focalizada 61/61 y suite 1081/1081 | Tests de orden sin términos, página segunda y conteo de 16 coincidencias en `SqliteCollectionRepositoryTests`. |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: aceptado.
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA06 tienen evidencia técnica satisfactoria.
- Bloqueos y riesgos restantes: ninguno conocido dentro del alcance. S6.6 y S6.7 permanecen como trabajos vecinos y no son condiciones de cierre de esta unidad.
- Aceptación humana: DLG aceptó S6.5 el 20-X-2026 con «Consultas y búsquedas», confirmado explícitamente en esta conversación.
- Archivado: completado el 21-X-2026; traslado y enlaces entrantes/salientes comprobados.
- Ubicación archivada: `specs/archive/s6.5-consultas-busqueda-fts5/`.
- Fecha efectiva de archivado y comprobación de enlaces: 21-X-2026; referencias recalculadas y verificadas.
- Siguiente paso autorizado, sin cerrar otras etapas: ninguno para S6.5; el trabajo está aceptado, cerrado y archivado.
