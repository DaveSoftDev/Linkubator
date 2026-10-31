# Auditoría individual del grupo 8: persistencia y transacciones

## Alcance y resultado

**Estado actual:** revisión individual automatizada cerrada: selección relacionada **61/61** y suite global **1.827/1.827** superadas. El diagnóstico inicial siguiente se conserva como historial.

Diagnóstico inicial del 2026-10-30, sin cambios en tests ni producción en esa fase. Se revisaron migraciones, configuración de conexiones, unidad de trabajo y restricciones/atomicidad SQLite por caso. El inventario inicial contenía **34 casos de 32 métodos: 24 vigentes y 10 a reforzar**. No se identificaron duplicados demostrados.

La ejecución focalizada por clases relacionadas pasó **47/47**, sin omitidos. Trece casos de esa selección ya están asignados a los grupos 1, 2 y 5; se conservan allí. En particular, la creación pública bloqueada por cambio de privacidad se conserva: además del estado final comprobado en las pruebas ordenadas del grupo 2, comprueba específicamente la ausencia de la URL rechazada.

```powershell
dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-restore --filter 'FullyQualifiedName~SqliteUnitOfWorkTests|FullyQualifiedName~SqliteMigrationServiceTests|FullyQualifiedName~SqliteConnectionFactoryTests|FullyQualifiedName~SqlitePersistenceIntegrationTests' -v minimal
```

La ejecución focalizada valida el comportamiento actual, no demuestra por sí sola ausencia de huecos. No se ha repetido la suite global en esta fase de diagnóstico; la última ejecución del grupo 7 quedó con dos fallos ajenos a ese lote.

## Trazabilidad

| Fuente | Aspectos revisados |
|---|---|
| [architecture.md → «Persistencia»](../context/architecture.md#persistencia) | Conexiones, restricciones, transacciones, colación, migraciones y atomicidad de proyecciones. |
| [domain-model.md → «Restricciones de unicidad»](../context/domain-model.md#restricciones-de-unicidad) | Límites por usuario y unicidad de datos. |
| [domain-model.md → «Propiedad de los datos»](../context/domain-model.md#propiedad-de-los-datos) | Clave compuesta y propiedad, con tests de negocio asignados al grupo 2. |
| [roadmap.md → «Checklist de calidad del MVP0»](../context/roadmap.md#checklist-de-calidad-del-mvp0) | Evidencia de infraestructura y persistencia aplicable; no se atribuyen pruebas de capacidades futuras. |

## Refuerzos propuestos

| ID | Prioridad | Hallazgo | Acción propuesta |
|---|---|---|---|
| A1 | Baja | `CompareSpanish_CoversNullEqualityAndOrderingBranches` invoca por reflexión un método privado. Es frágil ante refactors y prueba ramas internas en lugar del contrato SQL. Los tests públicos ya comprueban ordenación y equivalencia en conexiones. | Sustituirlo por pruebas de resultados observables de la colación mediante SQL, conservando cualquier caso funcional no cubierto; no exigir que SQLite invoque el comparador para `NULL`. |
| A2 | Media | `ApplyPendingMigrations_WhenLegacyVersionsAreRecorded_PreservesHistoryAndSampleData` primero aplica todo el catálogo actual y después añade registros históricos. Comprueba que se preservan registros y tabla ajenos al reaplicar, no una migración desde un esquema legado. | Renombrar a la garantía real y reforzar los valores históricos; contrastar con las pruebas de migración de proyecciones existentes sin afirmar que se cubre un upgrade que no se ha preparado. |
| A3 | Media | `UnitOfWorkFactory_Get_WhenContextIsLinkubator_ReturnsTransactionalRepositorySet` comprueba ausencia de colección antes y cantidad de ambas entidades después, pero no que la segunda entidad y las dos proyecciones sean invisibles antes de commit ni sus datos exactos después. | Comparar entidades y proyecciones con una conexión observadora antes y después del commit; preservar el control de las dos interfaces. |
| A4 | Media | `UnitOfWork_DisposeAndDisposeAsync_WhenRepeated_AreSafe` inicia transacción sin escrituras y solo verifica ausencia de excepción. No demuestra que el `Dispose` síncrono revierta escrituras y libere el bloqueo. | Insertar entidad y proyección antes del primer `Dispose`, verificar rollback y que otra conexión puede escribir, manteniendo la repetición de ambos métodos. |
| A5 | Baja | `UnitOfWork_CommitAndRollback_WhenNoTransactionActive_AreNoOps` ejecuta ambos métodos y después consulta desde otra conexión; no comprueba que la misma unidad siga siendo utilizable. | Comprobar una transacción posterior con escritura y commit en la misma unidad, sin añadir un comportamiento no previsto por su API. |
| A6 | Media | El rollback de edición por fallo en un enlace comprueba datos de colección y solo parte de la proyección de uno de los enlaces; un cambio parcial en el otro o en otras columnas de proyección podría pasar. | Tomar snapshots precisos de ambos enlaces y proyecciones, además de colección, tras comprobar la causa del fallo inyectado. |
| A7 | Media | Los cuatro rechazos de unicidad solo comprueban código SQLite genérico 19 y un recuento filtrado. Una restricción distinta o cambios colaterales podrían simular parte del resultado. | Comprobar código extendido de unicidad y snapshots de filas relevantes, con controles de escritura válidos y sin duplicar los tests de aplicación de otros grupos. |

## Huecos propuestos

| ID | Prioridad | Escenario | Acción propuesta |
|---|---|---|---|
| H1 | Media | Hay rollback explícito y dispose asíncrono, pero no se contrasta la liberación del bloqueo y reutilización tras rollback/commit desde una conexión competidora. | Prueba determinista con segunda conexión, tiempo de espera cero y controles antes/después de finalizar; sin sleeps. |
| H2 | Media | La migración fallida se comprueba al primer intento; falta comprobar que un nuevo ejecutor con script corregido reanuda las pendientes sin repetir la migración confirmada. | Inyectar fallo tras una escritura, verificar rollback e historial y ejecutar catálogo corregido con marcadores que detecten reejecución o continuación indebida. |
| H3 | Media | No hay tests directos de catálogo inyectado vacío, versión malformada y SQL vacío. El constructor actualmente ordena antes de validar; entradas como versión nula podrían producir una excepción distinta. | Especificar primero el contrato técnico esperado del constructor y añadir matriz de entradas inválidas sin convertir excepciones accidentales en regla de producto. |
| H4 | Baja | Faltan pruebas aisladas de cancelación ya solicitada de `Begin`, `Commit` y `Rollback`, y de su efecto sobre escrituras no confirmadas. | Verificar cancelación sin pérdida ni confirmación accidental y limpieza posterior, siempre que se confirme el contrato de `CancellationToken` del límite técnico. |
| H5 | Media | Los tests de unicidad negativa no prueban directamente en SQLite el contraste entre clave repetida para el mismo usuario y la misma clave para otro usuario, conservando ambas filas. | Para slug de colección y URL normalizada, incluir control positivo entre usuarios y rechazo para el mismo propietario; evitar confundirlo con la unicidad global de email y alias. |

## Correcciones aplicadas

| Hallazgo | Resolución |
|---|---|
| A1 | Sustituida la reflexión por comparaciones SQL de igualdad y orden con la colación registrada. |
| A2 | Renombrado el escenario de historial desconocido y comprobadas las marcas históricas exactas. |
| A3 | Entidades y proyecciones invisibles antes del commit y datos concretos tras él. |
| A4–A5 | Dispose síncrono con escrituras y prueba de rollback; reutilización de la misma unidad tras commit/rollback sin transacción. |
| A6 | Fallo inyectado identificado y snapshots de todos los enlaces y sus proyecciones. |
| A7 / H5 | Código SQLite extendido de unicidad y snapshots; controles entre usuarios para slug y URL normalizada. |
| H1 | Competidor bloqueado durante la transacción y escritura permitida tras commit o rollback. |
| H2 | Reanudación tras corregir migración fallida, con datos e historial exactos. |
| H3 | Validación del catálogo vacío, versión inválida y SQL vacío. Se corrige en producción el orden de validación: una versión nula se rechaza antes de ordenarla, con el mismo error controlado que el resto de versiones inválidas. |
| H4 | Cancelación previa de Begin y de Commit/Rollback con observación de aislamiento, bloqueo y rollback posterior. Se prueba el contrato técnico existente, no se introduce una regla de producto. |

El [inventario actualizado](test-audit-group-8.csv) contiene **48 casos de 39 métodos**: 24 conservados, 10 reforzados y 14 nuevos. Sus identificadores se han cotejado con el TRX definitivo; 13 casos relacionados siguen asignados a grupos anteriores. La selección relacionada superó **61/61** con código final recompilado, sin omitidos.

Las dos primeras ejecuciones globales obtuvieron **1.825/1.827**, sin omitidos, por los mismos dos fallos externos ya observados en el grupo 7: validadores personalizados aceptaban una solicitud nula y los servicios intentaban crear un resultado fallido sin errores. Tras corregir ambos servicios, sus pruebas pasaron **124/124** y la suite global recompilada pasó **1.827/1.827**, sin omitidos. **Revisión individual automatizada cerrada.**
