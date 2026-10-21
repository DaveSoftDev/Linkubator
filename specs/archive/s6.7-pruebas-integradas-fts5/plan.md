# Plan técnico: S6.7 — Pruebas integradas de búsqueda FTS5

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.
  - Plan: aprobado por DLG el 20-X-2026 («Aprobar el plan»), decisión explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- Mantener las pruebas en `Linkubator.Tests` como integración de Infrastructure con SQLite, utilizando el fixture temporal `SqliteRepositoryTestDatabase` y llamadas a los repositorios productivos.
- Reutilizar `SqliteLinkRepository` y `SqliteCollectionRepository` para operaciones de escritura y consultas; comprobar las proyecciones correspondientes directamente solo cuando haga falta demostrar persistencia o rollback que la interfaz de búsqueda no exponga.
- Construir los escenarios con entidades sintéticas de dos usuarios y ambas entidades buscables. No añadir endpoints, herramientas de mantenimiento, migraciones ni cambios de contratos de producción.
- Distribuir los casos en los tests de repositorio existentes o en un fixture de integración dedicado si la inspección confirma que mejora la claridad; conservar el patrón de un test por archivo cuando se introduzcan tipos públicos.

### Flujos internos

1. Crear una base temporal con migraciones y usuarios de prueba mediante el fixture existente.
2. Preparar colecciones y enlaces con operaciones de repositorio dentro de las transacciones de prueba requeridas, y consultar tanto las proyecciones como los listados persistentes.
3. Para sincronización integrada, ejecutar las escrituras y luego comprobar la coincidencia por texto antes y después de los cambios; incluir el renombrado de colección sobre sus enlaces y las eliminaciones cubiertas por el criterio.
4. Para atomicidad, preparar valores previos observables y hacer que falle una operación después de iniciar cambios de escritura. Verificar que el mecanismo reproduce el fallo y que, tras el rollback, tanto las entidades como las proyecciones y las consultas mantienen el estado anterior.
5. Para propiedad y separación por entidad, sembrar coincidencias controladas en usuarios y tipos distintos y comprobar los resultados de cada listado. Manipular únicamente en los datos de test el `UserId` de una proyección para ejercer el control negativo de discrepancia con la entidad.
6. Para acentos/NFC, usar datos compuestos y descompuestos en el término de búsqueda y en campos proyectados; comprobar resultados y persistencia intacta de los valores originales.

### Persistencia y dependencias

- Reutilizar migraciones y esquema de S6.1, sincronización de S6.4, normalización de S6.2, constructor de S6.3 y consultas de S6.5; no modificar esquema ni scripts SQL.
- Los tests llaman a las implementaciones reales de repositorios y base SQLite temporal. No simular el almacenamiento de proyecciones ni sustituir la consulta por un mock.
- La inyección de fallo reside en el borde de pruebas/SQLite: abrir una transacción explícita, quitar temporalmente la tabla de proyección para que falle la escritura posterior del repositorio tras modificar la entidad, capturar la excepción y revertir la transacción. Reutilizar y ampliar el patrón ya probado en `SqliteLinkRepositoryTests.Update_WhenProjectionWriteFails_RollsBackLinkAndProjection`; no añadir puntos de fallo, abstracciones ni comportamientos especiales en producción.
- La inspección y la ejecución focalizada confirmaron `dotnet test` con VSTest, xUnit y .NET SDK 10.0.401. El filtro de clase exacto es `FullyQualifiedName~SqliteFts5IntegratedSearchTests`.
- S6.8 y S6.9 no son dependencias de cierre: las validaciones requeridas son automatizadas sobre datos sintéticos y SQLite temporal.

### Garantías técnicas

- Las pruebas de sincronización consultan el resultado a través de FTS5 después de las escrituras de dominio, por lo que ejercitan la ruta conjunta de proyección y consulta, además de inspeccionar persistencia cuando sea necesario.
- La prueba de atomicidad verifica una condición negativa concreta: si la transacción confirmase solo parte de la escritura, al menos una aserción de entidad, proyección o resultado de búsqueda debe fallar. Esta prueba valida el comportamiento de este camino de persistencia, no todas las transacciones del sistema.
- Los escenarios de propiedad evalúan usuarios distintos y también la discrepancia entre `UserId` indexado y entidad asociada, en cada listado incluido.
- Los escenarios Unicode usan solo valores sintéticos; las aserciones distinguen el texto normalizado de la proyección del valor original de la entidad.
- Los tests no demuestran comportamiento de interfaz Web ni la ejecución manual de reconstrucción asignada a S6.9.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Limitar el cambio a tests de integración de persistencia; no modificar código productivo ni reglas de búsqueda. | confirmado | [spec.md → «Alcance»](spec.md#alcance), [S6.4](../s6.4-sincronizacion-proyecciones-fts5/spec.md), [S6.5](../s6.5-consultas-busqueda-fts5/spec.md) | Las brechas descubiertas en producción se registran para el propietario correspondiente; cualquier cambio funcional requeriría actualizar y aprobar la fuente antes de implementarlo. |
| Usar fixture y SQLite reales para ejercitar conjuntamente operaciones, proyecciones y búsquedas. | confirmado | `SqliteRepositoryTestDatabase` y pruebas existentes de repositorios; criterios CA01–CA05 | Evita que mocks oculten fallos de integración. |
| Inducir un fallo de escritura de proyección dentro de la transacción explícita de test, para comprobar el rollback ordinario de entidad y proyección y añadir la comprobación de búsqueda posterior. | confirmado | [SqliteLinkRepositoryTests.cs](../../../tests/Linkubator.Tests/Infrastructure/SqliteLinkRepositoryTests.cs), prueba `Update_WhenProjectionWriteFails_RollsBackLinkAndProjection`; T01 | Reutiliza un patrón existente sin seams productivos. La prueba integrada añadirá el resultado de búsqueda posterior; S6.6 no es el mecanismo de fallo requerido por CA02. |
| Repartir casos entre tests existentes o crear un fixture/archivo de integración. | propuesta | Convenciones actuales de tests SQLite; confirmar en T01 | Mantener casos legibles, focalizados y sin tipos públicos agrupados indebidamente. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada, integración SQLite de escrituras de enlace/colección y búsqueda posterior: insertar, editar y eliminar enlaces, insertar/renombrar/eliminar colecciones, incluida la proyección de nombre de colección en enlaces. | Fixture SQLite, repositorios de S6.4 y consultas S6.5. | `dotnet test .\tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~SqliteFts5IntegratedSearchTests"`. | Tras cada operación confirmada, las búsquedas y proyecciones pertinentes reflejan el estado actual; no queda resultado obsoleto de una entidad eliminada ni nombre anterior tras el renombrado. | Resultado focalizado, nombres de escenarios, SDK y versión SQLite pertinentes. |
| V02 | CA02 | Automatizada, integración negativa: inducir fallo tras iniciar una escritura compuesta y verificar rollback de datos y proyecciones. | Transacción explícita de test y mecanismo ya usado por `Update_WhenProjectionWriteFails_RollsBackLinkAndProjection`. | Mismo comando focalizado de tests de Infrastructure, filtrado por los tests de repositorios. | La excepción esperada se produce tras un cambio inicial y, después, los valores entidad/proyección/búsqueda son idénticos al estado previo; una escritura parcial haría fallar el test. | Test ejecutado, punto y causa de fallo, valores de control y evidencia antes/después. |
| V03 | CA03, CA04 | Automatizada, SQLite: consultas de ambos listados con coincidencias de usuarios distintos y coincidencias cruzadas entre enlace y colección; alterar en el fixture el `UserId` de una proyección de enlace sin cambiar el propietario de la entidad. | Repositorios de consulta S6.5 y fixture de dos usuarios. | `dotnet test .\tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~SqliteFts5IntegratedSearchTests"`. | Cada listado devuelve solo su entidad y usuario; la entidad ajena no se obtiene ni cuando su fila de proyección presenta un `UserId` adulterado. | Resultado del test y datos sintéticos necesarios para entender los controles negativos. |
| V04 | CA05 | Automatizada, SQLite: campos con acentos en forma descompuesta consultados con forma precompuesta, verificando la proyección y el valor de entidad. | Normalizador S6.2 y proyecciones/consultas integradas S6.4–S6.5. | `dotnet test .\tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~SqliteFts5IntegratedSearchTests"`. | Las formas equivalentes recuperan las coincidencias definidas por la fuente; los valores almacenados en entidades conservan su secuencia original. | Resultado, casos de cadenas sintéticas y comprobaciones de proyección/entidad. |
| V05 | CA01–CA05 | Revisión de cobertura y regresión del alcance propio. | V01–V04. | `dotnet test .\tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~SqliteFts5IntegratedSearchTests"`; `git diff --check`. | Los tests focalizados pasan, todos los criterios tienen evidencia y no hay cambios productivos o documentales funcionales ajenos al alcance. | Salida real de pruebas y diff check, fecha y SDK; criterio/caso de test asociado. |

El comando de V01/V03–V05 combina filtros OR compatibles con VSTest; T01 confirmó la sintaxis y el proyecto de tests. No usar la suite completa salvo que la validación focalizada revele regresiones o dependencias no cubiertas.

## Orden de ejecución y cierre

- Las tareas están aprobadas por DLG el 20-X-2026 («Aprobar y autorizar implementación»); su descomposición, comprobaciones y evidencia constan en [tasks.md → «Descomposición»](tasks.md#descomposición).
- CA01–CA05 tienen evidencia satisfactoria, las tareas incluidas están completas y DLG aceptó S6.7 para su alcance con «Fase terminada» el 20-X-2026; S6.8 y S6.9 no condicionan este cierre.
- Si los tests revelan una discrepancia entre producción y las fuentes, detener el cambio de tests que presuponga un contrato no implementado y comunicar el hallazgo al propietario de la regla/producción. No alterar código de producción bajo esta spec.
