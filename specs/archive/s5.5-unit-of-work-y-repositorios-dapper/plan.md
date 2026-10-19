# Plan técnico: S5.5 — Unit of Work y repositorios Dapper

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»).
	- Plan: aprobado por DLG el 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- **Unit of Work SQLite:** implementación en Infrastructure de `IUnitOfWork` e `IUnitOfWorkFactory`. La fábrica crea una unidad con conexión común de S5.1; `Begin` inicia la transacción de escritura con `BEGIN IMMEDIATE`; `Commit`, `Rollback` y disposición controlan y liberan los recursos propios de la unidad. Cada repositorio resuelto por esa unidad comparte la misma conexión y transacción.
- **Repositorios:** implementaciones Dapper para `ICollectionRepository`, `ILinkRepository`, `IDashboardRepository` e `IPublicReadRepository`, organizadas por responsabilidad en Infrastructure. Las operaciones enlazan parámetros SQL y materializan entidades/DTOs de Application/Domain; Application no recibe ni referencia tipos de persistencia.
- **Acceso fuera de la unidad:** los contratos que no participan en una unidad de trabajo usan una conexión SQLite scoped abierta por `ISqliteConnectionFactory`; el contenedor comparte esa conexión entre los repositorios scoped y la dispone al cerrar el scope. Las operaciones de escritura coordinadas por un UoW usan los repositorios creados por esa unidad, no la conexión scoped del acceso aislado.
- **Registro:** `AddInfrastructure` registra la fábrica de UoW, una conexión SQLite scoped y los repositorios concretos mediante sus contratos. La prueba de composición verifica la resolución de los contratos y la liberación de la conexión al terminar el scope.
- **SQL y mapeo:** mantener SQL explícito y parametrizado, con predicados de propiedad por `userId` en cada operación privada y con las uniones permitidas por los criterios públicos. Mapear las columnas existentes del esquema S5.3; no alterar el esquema, ni introducir cascadas o triggers.
- **Búsqueda:** para este trabajo, los tests de listados usan `query` nulo o vacío y comprueban el rechazo explícito de términos. No se implementa `LIKE` ni otra búsqueda temporal; S6 añadirá la consulta sobre FTS5. DLG confirmó esta frontera el 19-X-2026.
- **Límites de Application:** no se cambia Application en S5.5. El desfase observado en `LinkService.CreateLink` —lecturas de colección/visibilidad y duplicidad antes de comenzar UoW— se mantiene como riesgo abierto para la garantía concurrente de extremo a extremo; no se declara resuelto por los repositorios. Elevar a trabajo propietario de Application si se pretende probar esa garantía antes de S5.8.

### Flujos internos

1. La fábrica de UoW obtiene una conexión abierta y ya configurada por S5.1/S5.4, y crea los repositorios de la unidad sobre esa conexión.
2. Application inicia la unidad. El UoW ejecuta `BEGIN IMMEDIATE` en la conexión única antes de devolver el control al flujo de escritura.
3. Cada operación de repositorio ejecuta SQL Dapper con la misma transacción cuando la unidad está activa; repositorios de lectura fuera de UoW administran conexión y liberación localmente.
4. Ante éxito, Application llama a `Commit`; ante rechazo o excepción, el consumidor llama a `Rollback` y la disposición final libera los recursos. La disposición de una unidad todavía activa debe proteger contra escrituras parciales.
5. Ninguna operación externa ni espera de usuario/reintento se incorpora al ciclo transaccional, conforme a [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia).

### Persistencia y dependencias

- Usar `Microsoft.Data.Sqlite`, Dapper, `ISqliteConnectionFactory`, migraciones versionadas y esquema ya disponibles en S5.1–S5.4. Las pruebas crearán bases temporales y aplicarán scripts disponibles mediante `ISqliteMigrationService`; no modificarán la base local del proyecto.
- La transacción SQLite se administra por el UoW. Comprobar mediante efecto observable que las escrituras pasan por `BEGIN IMMEDIATE` y por una misma unidad; no inferirlo solo de una llamada a `Begin`.
- Aplicar `COLLATE LINKUBATOR_ES_ES` solo en órdenes alfabéticos explícitos. El orden completo integrado y los desempates se revalidan en S5.8.
- No se requieren cambios de tablas ni migraciones en S5.5. Las proyecciones FTS5 y su sincronización quedan fuera y son dependencia de S6.
- Los cambios de estado de Application que sean necesarios para ubicar comprobaciones concurrentes dentro de UoW no se incorporan en este trabajo; la arquitectura mantiene la exigencia y el límite se documenta para su resolución propietaria.

### Garantías técnicas

- Todos los repositorios obtenidos desde una instancia de UoW comparten una sola conexión/transacción, y no pueden confirmar por separado.
- La escritura solo se considera confirmada tras `Commit`; `Rollback` o disposición sin confirmar deshacen la unidad y liberan sus recursos.
- Cada operación privada filtra por identidad recibida y las actualizaciones/borrados combinan el identificador del recurso con el `userId`, sin confiar en la unicidad aislada del identificador.
- Las lecturas públicas filtran por las condiciones de publicación exigidas por los contratos y criterios; los DTOs devueltos no incluyen datos privados.
- El SQL de listado no hace trabajo de búsqueda textual en S5.5; términos de búsqueda, FTS5 y su mantenimiento no se presentan como entregados.
- Las pruebas de S5.5 son focalizadas en los componentes. No demuestran las carreras concretas de negocio ni el orden completo de todos los listados asignados a S5.8.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Usar `BEGIN IMMEDIATE` como inicio de cada unidad de escritura. | Confirmada | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), spec CA02 | Impone exclusión de escritor antes de las comprobaciones/escrituras ejecutadas en la unidad. |
| Reutilizar la fábrica de conexiones para abrir y configurar las conexiones UoW y registrar los repositorios desde `AddInfrastructure`. | Confirmada en la aprobación del plan | S5.1, S5.4 y spec CA01–CA03; DLG aprobó el plan con «Siguiente» el 19-X-2026 | Evita duplicar PRAGMAs/colación y mantiene un punto de configuración. |
| Los repositorios scoped fuera del UoW comparten una conexión scoped administrada por DI; los repositorios resueltos desde el UoW comparten la conexión de esa unidad. | Confirmada en la implementación y validada por test de composición | Contratos de S4.2, spec CA01–CA03 y prueba `SqliteInfrastructureRegistrationTests` | Aísla el ciclo de vida del acceso scoped y verifica disposición al terminar el scope. |
| `LinkService.CreateLink` inicia UoW después de comprobaciones que la arquitectura sitúa dentro de la transacción. | Riesgo abierto | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), [S4.7 → CA01](../s4.7-casos-uso-enlaces/spec.md#criterios-de-aceptación), spec «Bloqueos y preguntas pendientes» | S5.5 no garantiza concurrencia de creación mientras Application haga esas lecturas fuera del UoW. Resolver con la fuente/trabajo propietario y asignar la prueba de carrera a S5.8; no ampliar esta implementación sin aprobación. |
| Número y disposición física de repositorios/auxiliares SQL, y retorno de `Commit`/`Rollback` ante estados inválidos. | Propuesta técnica | Contratos actuales y arquitectura; concretar al aprobar el plan | Implementar lo mínimo compatible con contratos, probado en estados válidos y no válidos. Detenerse si requiere cambiar contratos o reglas. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada y arquitectura: resolución DI y dirección de referencias | Registro de servicios e implementaciones | Test de `AddInfrastructure` que resuelve UoW y los cuatro contratos de repositorio; ejecutar `ProjectReferenceMatrixTests` | Todas las interfaces requeridas se resuelven; Application no depende de Infrastructure, SQLite ni Dapper | Test focalizado, test de arquitectura, build y archivos de registro. |
| V02 | CA02 | Automatizada: inicio de transacción y conexión compartida | SQLite temporal migrada; implementaciones UoW/repositorios | Test de Infrastructure que comienza UoW, opera con dos repositorios y observa los efectos de la transacción antes/después de commit o rollback; confirmar la semántica `BEGIN IMMEDIATE` mediante comportamiento de bloqueo en prueba acotada, sin esperas prolongadas | La unidad obtiene bloqueo de escritura al inicio; la escritura de ambos repositorios se confirma/revierte como una unidad | Test, mecanismo de observación, resultado y configuración/runtime. |
| V03 | CA03 | Automatizada: commit, rollback y disposición | V02 | Tests con base temporal; provocar un fallo controlado entre dos escrituras, comprobar rollback, y comprobar disposición de una unidad activa sin commit | No queda persistencia parcial y una conexión nueva observa el estado confirmado o revertido esperado; recursos se pueden volver a abrir sin bloqueo residual | Tests, escenario de fallo, lecturas posteriores y estado de recursos. |
| V04 | CA04 | Automatizada: aislamiento de repositorios privados | S5.3 aplicado en base temporal | Tests por contrato para cada operación privada relevante con más de un usuario, `userId` distinto y recursos no accesibles; comprobar lecturas y que UPDATE/DELETE no cambien filas ajenas | No se filtran ni modifican filas ajenas; las operaciones cumplen su contrato cuando no hay acceso | Tests por método/caso, estado antes y después. |
| V05 | CA05–CA06 | Automatizada: escritura, mapeo y lecturas públicas | S5.3 aplicado; fixtures con usuarios, colecciones y enlaces públicos/privados | Tests de repositorios Dapper para insert/get/update/delete, mapeo de propiedades y consultas de dashboard/páginas públicas, incluidos estados vacíos y páginas interiores según los criterios aplicables | Datos se materializan correctamente y las lecturas no exponen contenido privado; cambios de escritura se reflejan en SQLite | Tests focalizados, fixture/migraciones, resultado por contrato. |
| V06 | CA07 | Automatizada: colación en las consultas de repositorio | S5.4; datos de listado sin búsqueda textual | Tests focalizados de listados alfabéticos con pares representativos y desempates según el criterio propietario; inspección SQL si el test no distingue una colación ausente | Las consultas declaran la colación registrada; el orden esperado de esta etapa se observa. No se usa como evidencia del conjunto completo S5.8 | Test y valores de orden; resultado de revisión. |
| V07 | CA08 | Automatizada y revisión técnica: frontera de búsqueda | Contratos de listado de Application y alcance de S6 | Tests/revisión que prueban `query` nulo/vacío y una entrada con términos en el comportamiento provisional aprobado; inspeccionar ausencia de `MATCH` FTS5 y de búsqueda sustituta | Las variantes vacías siguen el contrato sin términos; no se afirma búsqueda de términos antes de S6 y la entrada con términos sigue la decisión aprobada | Test de frontera, SQL revisado y decisión registrada. |
| V08 | CA01–CA08 | Integrada: regresión focalizada | Implementación y pruebas anteriores | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteUnitOfWork --verbosity minimal`; `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~DapperRepository --verbosity minimal`; `dotnet build Linkubator.sln --no-restore --verbosity minimal`; `git diff --check` | Tests focalizados, build y chequeo del diff correctos | Comandos, SDK, códigos de salida, resúmenes y enlace a resultados de V01–V07. |
| V09 | CA01–CA08 | Técnica: revisión del límite del cambio | Diff completo de la implementación | Revisar que no se modificaron reglas/contratos/esquema ni se incluyeron FTS5, SQL local, proveedor de identidad o Web; revisar parametrización y transacciones breves | El cambio queda dentro del alcance S5.5 y las salvedades quedan registradas para S5.8/S6 | Archivos inspeccionados, hallazgos y desviaciones. |

Los nombres de tests y filtros de V08 son propuestas de nomenclatura; ajustar al nombre efectivo de las clases cuando existan. V02 debe distinguir el inicio de escritura `BEGIN IMMEDIATE` de un inicio diferido: un test que solo compruebe atomicidad final no demuestra el modo de inicio. V04 demuestra el aislamiento de los repositorios, no que Application siempre pase la identidad correcta. V05 no cubre las carreras de negocio ni la aceptación de reglas de Application. V06 no sustituye la prueba integrada de orden completo de S5.8. V07 no demuestra búsqueda FTS5, que corresponde a S6.

## Orden de ejecución y cierre

- `tasks.md` se creó tras la aprobación del plan y las tareas fueron aprobadas por DLG («Ready») antes de iniciar la implementación. La decisión de DLG reserva la búsqueda textual a S6; S5.5 no implementa búsqueda provisional y rechaza explícitamente consultas con términos.
- Cada criterio cuenta con tarea y evidencia propia; el cierre de S5.5 no depende de S5.8 ni S6.
- Si la revisión de contratos, los tests o el diseño detectan que hay que cambiar Application, contratos, fuentes funcionales o el esquema, detener el trabajo afectado y proponer el cambio en el documento propietario antes de implementar.
- El cierre de S5.5 requiere evidencia satisfactoria de sus propios criterios y aceptación humana registrada en `tasks.md`; las pruebas integradas de carreras de S5.8 y la búsqueda de S6 son seguimientos separados.
