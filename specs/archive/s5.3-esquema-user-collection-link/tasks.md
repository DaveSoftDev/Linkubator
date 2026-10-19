# Tareas: S5.3 — Esquema de `User`, `Collection` y `Link`

## Referencias y autorización

- Estado: aprobado y ejecutado.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 19-X-2026 («Siguiente»).
    - Tareas: aprobadas por DLG el 19-X-2026 («Ready»).
- Implementación:
    - Estado: completada.
    - Inicio: 19-X-2026, con la implantación del esquema y la prueba de estructura sobre base temporal.
    - Finalización: 19-X-2026, comprobada con validación automatizada y compilación de la solución.
    - Evidencia consolidada: [src/Linkubator.Infrastructure/Sqlite/Migrations/V004_CreateUserCollectionLinkSchema.sql](../../../src/Linkubator.Infrastructure/Sqlite/Migrations/V004_CreateUserCollectionLinkSchema.sql), [src/Linkubator.Infrastructure/Properties/AssemblyInfo.cs](../../../src/Linkubator.Infrastructure/Properties/AssemblyInfo.cs) y [tests/Linkubator.Tests/Infrastructure/SqliteMigrationServiceTests.cs](../../../tests/Linkubator.Tests/Infrastructure/SqliteMigrationServiceTests.cs); validación ejecutada con `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteMigrationServiceTests --verbosity minimal` y `dotnet build Linkubator.sln --no-restore --verbosity minimal`.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

S5.3 reutiliza la fábrica de S5.1 y el catálogo/ejecutor de S5.2, ambos implementados y aceptados. La prueba propia comprueba la estructura declarada sobre una base temporal; no sustituye a S5.8, que probará restricciones en operaciones, propiedad efectiva, carreras y orden alfabético. La correspondencia de tipos de `ScrapingStatus`, booleanos y fechas se debe confirmar técnicamente antes de fijarla en el DDL; si la confirmación revela una regla de producto no cubierta, bloquear y escalar antes de continuar.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas, confirmando cobertura criterio → validación → tarea, alcance y dependencias. | CA01–CA05; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Verificar que S5.4–S5.10 y otras tablas quedan fuera del cambio; resultado esperado: aprobación explícita de tareas, separada de spec y plan. | completada | DLG aprobó las tareas con «Ready», 19-X-2026. |
| T02 | Inspeccionar el modelo de entidades, las convenciones de SQLite y el catálogo de migraciones; documentar el mapeo técnico compatible para fechas, booleanos y `ScrapingStatus`, y seleccionar el siguiente número de migración libre. | CA01; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [Persistencia y dependencias](plan.md#persistencia-y-dependencias) | T01 | Contrastar `BaseEntity`, propiedades y enum con las fuentes y recursos actuales; resultado esperado: cada tipo/columna tiene un mapeo documentado, sin cambios de reglas funcionales ni colisión de versión. Si se descubre una ambigüedad funcional, bloquear la tarea. | completada | Mapeo confirmado: todas las fechas usan `TEXT` ISO-8601 UTC, booleanos `INTEGER` con `CHECK (0,1)`, `ScrapingStatus` usa `INTEGER` con `DEFAULT 0` y `V004` era la siguiente versión libre, sin colisión con S5.2. |
| T03 | Añadir el script SQL versionado con las tablas `User`, `Collection` y `Link`, columnas y nulabilidad compatibles con las entidades, unicidades y relaciones aprobadas. | CA01–CA03; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [Garantías técnicas](plan.md#garantías-técnicas) | T02 | Aplicar la migración con el servicio de S5.2 sobre una base temporal; resultado esperado: las tres tablas existen y el catálogo SQLite declara columnas, claves y unicidades previstas, sin tablas de entidades futuras. | completada | La migración [V004_CreateUserCollectionLinkSchema.sql](../../../src/Linkubator.Infrastructure/Sqlite/Migrations/V004_CreateUserCollectionLinkSchema.sql) crea `User`, `Collection` y `Link` con `UNIQUE` y `FOREIGN KEY` en una base temporal. |
| T04 | Añadir índices secundarios justificados para los accesos previstos, evitando duplicados de índices de PK, FK y unicidad. | CA04; [plan → Componentes y contratos](plan.md#componentes-y-contratos) | T03 | Inspeccionar `PRAGMA index_list` e `index_xinfo` en la base temporal y contrastar con la composición aprobada; resultado esperado: los índices previstos existen, cubren columnas y dirección correctas y no duplican índices únicos. | completada | La prueba comprueba las columnas y el orden de cada índice secundario y la lista completa de índices únicos. `idx_collection_user_id (UserId, Id)` se quitó directamente de `V004`: es redundante con `UNIQUE (UserId, Id)` y no aparece en el ER. No se conserva `V005`, ya que todavía no hay bases creadas que hayan aplicado la migración. |
| T05 | Añadir tests xUnit focalizados de estructura del esquema y sus límites mediante SQLite temporal. | CA01–CA05; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V01–V05 | T03, T04 | Ejecutar filtro `FullyQualifiedName~SqliteMigrationServiceTests`; resultado esperado: se comprueban todas las columnas/tipos/nulabilidad/PK, índices y dirección, FK simples y compuesta, acciones referenciales, valores por defecto, ausencia de cascadas/triggers y alias nulo único. | completada | [SqliteMigrationServiceTests.cs](../../../tests/Linkubator.Tests/Infrastructure/SqliteMigrationServiceTests.cs) inspecciona `table_info`, `index_list`, `index_xinfo`, `foreign_key_list` y el DDL; además inserta dos usuarios con `Alias NULL` y verifica un `CreatedAt` UTC ISO-8601 generado por SQLite. |
| T06 | Ejecutar validación integrada y revisar que el esquema reutiliza las dependencias existentes sin activar migraciones al arranque. | CA01–CA05; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V06–V07 | T05 | Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteMigrationServiceTests --verbosity minimal` y `dotnet build Linkubator.sln --no-restore --verbosity minimal`; revisar composición de Infrastructure/Web. Resultado esperado: pruebas focalizadas y build correctos, sin ejecución automática desde Web. | completada | Reejecutado el 19-X-2026 con .NET SDK 10.0.12 tras quitar el índice redundante directamente de `V004`: 4/4 pruebas focalizadas correctas; build completo y `git diff --check` correctos. |
| T07 | Revisar evidencia por criterio, sincronizar el roadmap y solicitar aceptación humana del resultado. | CA01–CA05; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T06 | Confirmar evidencia satisfactoria de CA01–CA05, registrar límites S5.8/S5.9 y actualizar referencias de estado; resultado esperado: aceptación explícita de DLG o bloqueo documentado, sin declarar finalizada la etapa S5. | completada | DLG aceptó y cerró S5.3 con «Tenemos schema», 19-X-2026. Evidencia técnica y roadmap sincronizados; S5.8/S5.9 permanecen como validaciones vecinas fuera del cierre S5.3. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02–T03, T05–T06 | V01, V06 | Cumplido | 19-X-2026; .NET SDK 10.0.12 | La prueba compara todas las columnas, sus tipos SQLite, nulabilidad y clave primaria con el esquema esperado; comprueba además defaults y que el valor generado por SQLite para `CreatedAt` es UTC ISO-8601. Test focalizado 4/4 y build correctos |
| CA02 | T03, T05–T06 | V02, V06 | Cumplido | 19-X-2026; .NET SDK 10.0.12 | Se inspeccionan todos los índices únicos por columnas y orden; se comprueban `Email`, `Alias`, `(UserId, Slug)`, `(UserId, Id)` y `(UserId, UrlNormalized)`. Dos filas con `Alias NULL` coexisten |
| CA03 | T03, T05–T06 | V03, V06 | Cumplido | 19-X-2026; .NET SDK 10.0.12 | `foreign_key_list` confirma las relaciones a `User` y que los dos componentes de la FK de `Link` comparten el mismo id de relación y destino compuesto; `Collection (UserId, Id)` está respaldado por unicidad |
| CA04 | T04–T06 | V04, V06 | Cumplido | 19-X-2026; .NET SDK 10.0.12 | `index_list`/`index_xinfo` confirman nombres, columnas, orden y dirección (`CreatedAt DESC`) de cada índice secundario previsto; no existe índice adicional para `(UserId, Id)` porque ya lo cubre la clave única |
| CA05 | T03, T05–T06 | V05–V06 | Cumplido | 19-X-2026; .NET SDK 10.0.12 | Las acciones de todas las FK se comparan exactamente (`ON DELETE RESTRICT`, `ON UPDATE NO ACTION` por omisión), el DDL no contiene `CASCADE` y el catálogo no registra triggers |


Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: cerrado y aceptado por DLG el 19-X-2026 («Tenemos schema»).
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA05 tienen evidencia satisfactoria por la ejecución del 19-X-2026.
- Bloqueos y riesgos restantes: no hay bloqueos funcionales de esquema en el alcance S5.3; S5.8 y S5.9 quedan como validaciones vecinas y no bloquean este cierre.
- Aceptación humana del resultado: DLG, 19-X-2026 («Tenemos schema»).
- Archivado: completado el 19-X-2026; traslado y enlaces entrantes/salientes comprobados.
- Ubicación archivada: `specs/archive/s5.3-esquema-user-collection-link/`.
- Fecha efectiva de archivado y comprobación de enlaces: 19-X-2026; referencias recalculadas y verificadas.
- Siguiente paso autorizado: continuar con el siguiente bloque del roadmap. S5.3 queda cerrada para su alcance; no se ha solicitado archivado.
