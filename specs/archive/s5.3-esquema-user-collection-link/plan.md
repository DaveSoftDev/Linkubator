# Plan técnico: S5.3 — Esquema de `User`, `Collection` y `Link`

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»).
	- Plan: aprobado por DLG el 19-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

- **Script de esquema.** Añadir un script SQL versionado a Infrastructure, descubierto por el catálogo existente de S5.2 y con una versión posterior a las que ya están ocupadas. No modificar el ejecutor ni sus scripts de prueba como parte de S5.3.
- **Tablas de dominio incluidas.** El script crea exclusivamente `User`, `Collection` y `Link`, con columnas para sus propiedades persistidas según [domain-model.md → «User»](../../../context/domain-model.md#user), [«Collection»](../../../context/domain-model.md#collection) y [«Link»](../../../context/domain-model.md#link). Las propiedades base `Id` y `CreatedAt` se incluyen donde correspondan.
- **Mapeo físico propuesto.** Usar nombres de columna coincidentes con las propiedades de dominio y `INTEGER PRIMARY KEY` para los identificadores, conforme a la arquitectura. Los campos opcionales y los de `User` mientras el registro está sin completar aceptan `NULL`; los demás campos obligatorios se declaran `NOT NULL`. Contrastar cada columna y nulabilidad con el modelo y el materializador/repositorio previsto, sin cambiar las reglas funcionales.
- **Unicidad.** Definir `UNIQUE` para `User.Email`, `User.Alias`, `(UserId, Collection.Slug)` y `(UserId, Link.UrlNormalized)`. SQLite permite varios `NULL` en el índice único de `Alias`, necesario para los registros sin completar. No asignar colación propia a columnas o índices: S5.4 registra una colación usada en consultas, no para unicidad.
- **Relaciones.** `Collection.UserId` y `Link.UserId` referencian `User.Id`. Añadir `UNIQUE (UserId, Id)` a `Collection` como clave de soporte y una clave foránea compuesta `(Link.UserId, Link.CollectionId)` hacia `(Collection.UserId, Collection.Id)`. Ninguna relación usa cascadas. No añadir triggers.
- **Índices secundarios propuestos.** Definir índices de acceso para usuario, colección, estado público y fecha, partiendo de las consultas y criterios de [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia). Revisar duplicación con índices ya creados por claves primarias, foráneas y unicidad; justificar cada índice y sus columnas en esta propuesta para evitar redundancias. La composición final es una decisión técnica del plan que debe revisar DLG.
- **Registro y ejecución.** Reutilizar `ISqliteMigrationService` y `ISqliteConnectionFactory`; no añadir una ruta de ejecución automática en Web. El servicio explícito de S5.2 aplica el script.
- **Pruebas propias de S5.3.** Añadir pruebas sobre una base SQLite temporal que ejecuten las migraciones y consulten el catálogo de esquema (`sqlite_master`, `PRAGMA table_info`, `PRAGMA index_list`/`index_info` y `PRAGMA foreign_key_list`). Estas pruebas comprueban DDL y estructura para CA01–CA05, sin sustituir la suite de integración de restricciones y carreras asignada a S5.8.

### Flujos internos

1. Los tests crean una base temporal y resuelven el ejecutor con la fábrica de conexiones existente.
2. El ejecutor aplica en orden las migraciones disponibles, incluida la nueva versión de esquema.
3. Las comprobaciones consultan las tablas, columnas, nulabilidad, índices, claves foráneas y triggers declarados en SQLite.
4. La prueba de límites consulta que no existan tablas para `Tag`, `LinkTag` o `UserToken`, ni proyecciones FTS5 creadas por este script.

### Persistencia y dependencias

- Reutilizar la fábrica de S5.1, incluidos sus PRAGMA, y el ejecutor/versionado de S5.2; no duplicar inicialización SQLite ni registro de historial.
- Ejecutar el esquema como una migración versionada independiente. No incluir la preparación local de desarrollo de S5.6 ni datos de usuario; el esquema debe poder crearse sobre una base nueva sin depender del archivo local.
- Las fechas siguen la convención UTC ISO 8601 de la arquitectura. El formato concreto de enlace de valores de fecha, los valores de `ScrapingStatus` y la correspondencia de booleanos con SQLite deben quedar compatibles con el materializador y repositorios, y confirmarse antes de implementación; no alteran la definición de las entidades.
- S5.8 conserva la responsabilidad de probar imposición efectiva de unicidades y propiedad, carreras y orden alfabético. Esas pruebas no son requisito para cerrar S5.3.

### Garantías técnicas

- Un `Link` no puede referenciar una `Collection` de otro usuario por la clave foránea compuesta; se mantiene además la integridad de cada relación con `User` ([domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos) y [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia)).
- Las restricciones de unicidad se corresponden con las propiedades propietarias de [domain-model.md → «Restricciones de unicidad»](../../../context/domain-model.md#restricciones-de-unicidad), sin añadir comparaciones de unicidad ajenas a esas reglas.
- El DDL no delega cambios o eliminaciones relacionadas a cascadas ni triggers ([architecture.md → «Persistencia»](../../../context/architecture.md#persistencia)).
- La revisión estructural de esta spec no pretende demostrar concurrencia ni el comportamiento de los repositorios; esos límites se conservan en S5.8 y S5.9.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Tablas con columnas alineadas con las propiedades y claves `INTEGER PRIMARY KEY` | Propuesta | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), [domain-model.md](../../../context/domain-model.md) | Simplifica mapeo; revisar nulabilidad y tipos SQLite en esta propuesta antes de aprobar el plan. |
| Índices únicos de email, alias, slug de colección por usuario y URL normalizada de enlace por usuario; `UNIQUE (Collection.UserId, Collection.Id)` para la FK compuesta | Confirmada por las fuentes | [domain-model.md → «Restricciones de unicidad»](../../../context/domain-model.md#restricciones-de-unicidad), [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) | Materializa garantías de dominio y soporte relacional. |
| Índices secundarios por usuario, colección, estado público y fecha | Confirmada; composición revisada y aplicada | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), DLG aprobó el plan el 19-X-2026 | Los tests inspeccionan `index_list` e `index_xinfo`; el índice duplicado de `(UserId, Id)` se excluyó porque la clave única ya lo cubre. |
| Sin cascadas ni triggers | Confirmada por arquitectura y verificada | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) | Las pruebas inspeccionan acciones FK y el catálogo SQLite; la aplicación conserva las actualizaciones y eliminaciones relacionadas explícitas. |
| Representación persistida de `ScrapingStatus`, booleanos y fechas | Confirmada técnicamente | [domain-model.md → «Link»](../../../context/domain-model.md#link), [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), [tasks.md → T02](tasks.md) | `ScrapingStatus` y booleanos usan `INTEGER`; booleanos llevan `CHECK (0,1)`, `ScrapingStatus` usa `DEFAULT 0` y las fechas se guardan como `TEXT` ISO-8601 UTC. Los tests verifican el esquema y `CreatedAt` generado. |
| Versionado sin colisión con recursos ya presentes | Confirmada | [S5.2 → Componentes y contratos](../s5.2-ejecutor-scripts-sql-versionados/plan.md#componentes-y-contratos), [tasks.md → T02](tasks.md) | `V004` era la siguiente versión libre; no se añadió `V005`, porque no existen bases creadas que requieran corrección retroactiva. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada: tablas, columnas, tipos y nulabilidad del esquema | Migración S5.3 y tests sobre SQLite temporal | Test focalizado que aplica el catálogo y consulta `sqlite_master` y `PRAGMA table_info` para las tres tablas | Las tablas y columnas requeridas están presentes; PK, nulabilidad y tipos estructurales concuerdan con el diseño aprobado | Nombre de test, SDK, tablas/columnas observadas y resultado. |
| V02 | CA02 | Automatizada/técnica: definición de unicidad | V01 pasa | Test/revisión de `PRAGMA index_list` y `PRAGMA index_info`, comprobando además que el índice de alias admite `NULL` mediante metadata DDL o comportamiento aislado | Se identifican las claves únicas con el ámbito correcto, incluyendo los valores nulos de alias | Índices y columnas inspeccionados, método y resultado. |
| V03 | CA03 | Automatizada: estructura de claves foráneas | V01 pasa y la conexión de fábrica tiene claves foráneas habilitadas | Test que inspecciona `PRAGMA foreign_key_list` y el índice único objetivo de `Collection` | Se observan las referencias a `User` y la FK compuesta `Link` → `Collection` con las parejas de columnas y soporte esperados | Filas observadas de PRAGMA, conexión usada y resultado. |
| V04 | CA04 | Técnica/automatizada: índices secundarios | Migración S5.3 y diseño de consultas disponible | Test de catálogo de índices y revisión contra la composición aprobada en este plan | Cada índice propuesto existe y cubre el acceso que justifica; no se cuentan como índices secundarios los generados por PK/UNIQUE | Nombres, columnas, justificación y resultado de revisión. |
| V05 | CA05 | Automatizada: ausencia de cascadas y triggers | Migración aplicada | Test que inspecciona SQL de tablas/índices en `sqlite_master` y el catálogo de triggers | Las FK no usan `ON UPDATE/DELETE CASCADE` y el esquema no registra triggers | DDL/metadata revisados y resultado. |
| V06 | CA01–CA05 | Integrada: tests focalizados y build | Tests y cambios del alcance completos; paquetes restaurados | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteSchema --verbosity minimal` y `dotnet build Linkubator.sln --no-restore --verbosity minimal` | Tests focalizados y compilación correctos | Comandos, SDK, códigos de salida y resumen. |
| V07 | CA01–CA05 | Técnica: límites y dependencias | Migración integrada | Revisar que se reutilizan catálogo/fábrica, que Web no ejecuta migraciones al arrancar y que este DDL no crea tablas de otras etapas | No se duplican mecanismo de migración o configuración y no se incorpora alcance de S5.4–S5.7 o S6 | Archivos revisados, límites confirmados y desviaciones. |

V01–V05 verifican el esquema declarativo y su metadata; no sustituyen los tests de imposición efectiva, carreras y repositorios de S5.8 ni la revisión manual de la base final de S5.9. V07 es una revisión de límites y no prueba por sí misma comportamiento en ejecución.

## Orden de ejecución y cierre

- La especificación, el plan y las tareas fueron aprobados por DLG el 19-X-2026 («Siguiente» para spec y plan; «Ready» para tareas). La implementación y validación se completaron; DLG aceptó y cerró el alcance el 19-X-2026 («Tenemos schema»). El estado y la evidencia están registrados en [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre).
- Las pruebas deben ejecutarse sobre una base temporal y poder cerrar CA01–CA05 sin esperar S5.4–S5.10.
- Las tareas aprobadas incluyen la revisión de los formatos de persistencia y la elección del siguiente número de migración antes de implementar.
- S5.3 podrá cerrarse cuando todos sus criterios tengan evidencia satisfactoria, las tareas propias estén completadas y DLG acepte explícitamente el resultado en `tasks.md`. Los tests de integración ampliados y la aplicación/revisión manual del conjunto completo de scripts permanecen en S5.8 y S5.9.
- Si el diseño descubre una discrepancia entre esquema e invariantes, detener el trabajo afectado y tramitar el cambio en su documento propietario antes de modificar las reglas o implementar una alternativa.
