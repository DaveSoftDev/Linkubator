# Tareas: S5.5 — Unit of Work y repositorios Dapper

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 19-X-2026 («Siguiente»).
    - Tareas: aprobadas por DLG el 19-X-2026 («Ready»), instrucción explícita en esta conversación a las 08:27.
- Implementación:
    - Estado: completada el 19-X-2026.
    - Inicio: 19-X-2026, después de la aprobación de tareas; referencia: «Implementa».
    - Finalización: 19-X-2026; suite SQLite focalizada 60/60, suite completa 1005/1005, build de solución y `git diff --check` correctos; referencia detallada en T07 y la evidencia por criterio.
    - Evidencia consolidada: T01–T07 y CA01–CA08 en este documento.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

La decisión de DLG «Vale, lo dejamos para S6» resuelve la frontera: S5.5 no implementa búsqueda textual y la búsqueda queda para S6. Los repositorios ya rechazan consultas no vacías; ese comportamiento se debe probar, no presentar como búsqueda. DLG aprobó estas tareas con «Ready» antes de iniciar la implementación.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Registrar la frontera de búsqueda para S5.5/S6 y el comportamiento provisional implementado ante `query` no vacío. | CA08; [plan → «Decisiones técnicas y riesgos»](plan.md#decisiones-técnicas-y-riesgos), [spec → «Criterios de aceptación»](spec.md#criterios-de-aceptación) | Decisión de DLG, recibida | Decisión rastreable; no se simula búsqueda y la documentación distingue búsqueda S6 del rechazo actual | completada | DLG: «Vale, lo dejamos para S6. Implementa»; frontera incorporada en spec y plan. |
| T02 | Implementar la fábrica de Unit of Work y el ciclo de conexión/transacción `BEGIN IMMEDIATE`, incluyendo commit, rollback y disposición segura de unidades no confirmadas. | CA02–CA03; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos) y [V02–V03](plan.md#estrategia-de-validación) | S5.1 | Tests de Infrastructure comprueban bloqueo inmediato, commit, rollback, invisibilidad previa a commit, disposición y liberación | completada | `SqliteUnitOfWorkTests` cubre commit, rollback, `BEGIN IMMEDIATE` mediante bloqueo observable, aislamiento antes de commit, disposición con rollback y errores de contexto/repositorio. Suite SQLite focalizada: 60/60. |
| T03 | Implementar repositorios Dapper de colecciones y enlaces para operaciones puntuales y de escritura, resolubles dentro de la unidad y mediante DI, con SQL parametrizado y filtros por `userId`. | CA01, CA04–CA05; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Garantías técnicas»](plan.md#garantías-técnicas), [V01 y V04–V05](plan.md#estrategia-de-validación) | T02; S5.2–S5.3 | Tests de Infrastructure verifican mapeo y CRUD de ambos contratos, identidad/propiedad para lecturas y escrituras privadas; recursos ajenos permanecen sin cambios | completada | `SqliteCollectionRepositoryTests` y `SqliteLinkRepositoryTests` cubren lectura, escritura, aislamiento, listados/paginación y rechazo de términos. Suite SQLite focalizada: 60/60. |
| T04 | Implementar repositorios Dapper de dashboard y lecturas públicas, respetando contratos de salida, privacidad, selección y paginación. | CA01, CA05–CA06; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), [V01 y V05](plan.md#estrategia-de-validación) | T02; S5.2–S5.3 | Tests focalizados cubren datos públicos/privados, estados vacíos y páginas; las lecturas públicas no devuelven contenido privado | completada | `SqliteDashboardRepositoryTests` y `SqlitePublicReadRepositoryTests` cubren resultados vacíos, selección por usuario, conteos, privacidad y paginación. Suite SQLite focalizada: 60/60. |
| T05 | Implementar los listados sin términos, aplicar explícitamente `LINKUBATOR_ES_ES` y rechazar explícitamente los términos mientras la búsqueda no pertenezca a S5.5. | CA06–CA08; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), [V06–V07](plan.md#estrategia-de-validación) | T01, T03–T04; S5.4 | Tests comprueban listado, colación/desempate y rechazo de términos, sin búsqueda sustitutiva | completada | Las pruebas focalizadas cubren orden español, filtro por colección, paginación y rechazo de consultas con términos para listado y conteo. Suite SQLite focalizada: 60/60. |
| T06 | Completar registro DI y pruebas de composición/referencias, confirmando que Application no adquiere dependencias de Infrastructure, SQLite ni Dapper. | CA01; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), [V01](plan.md#estrategia-de-validación) | T02–T04 | Test de registro resuelve UoW y los cuatro contratos, y comprueba liberación de conexión scoped; `ProjectReferenceMatrixTests` pasa | completada | `SqliteInfrastructureRegistrationTests` valida composición y liberación al cerrar scope; `ProjectReferenceMatrixTests` incluido en suite completa. Suite SQLite focalizada: 60/60. |
| T07 | Ejecutar validación integrada y revisión de límites, consolidando la cobertura por criterio y excluyendo S5.8/S6 de la evidencia de cierre. | CA01–CA08; [plan → V08–V09](plan.md#estrategia-de-validación) | T01–T06 | Tests focalizados y `ProjectReferenceMatrixTests`, build de solución y `git diff --check` terminan correctamente; revisión confirma ausencia de entregables fuera de alcance | completada | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~Sqlite --verbosity minimal`: 60/60; suite completa `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --verbosity minimal`: 1005/1005; `dotnet build Linkubator.sln --no-restore --verbosity minimal`: correcto; `git diff --check`: correcto. 19-X-2026, .NET 10.0.12. Revisión confirma cambios de producción limitados a Infrastructure/DI; no se alteran contratos, reglas de producto, esquema ni se incorpora FTS5. |
| T08 | Revisar la evidencia, actualizar el resumen de implementación y registrar la aceptación humana del resultado. | CA01–CA08; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T07 | Evidencia satisfactoria para cada criterio y decisión humana explícita registrada; sin aceptación no se marca aceptada | completada | DLG aceptó S5.5 el 19-X-2026 con «Repositorios listos». |
| T09 | Sincronizar el avance y el enlace de cierre en el roadmap y el índice tras la aceptación; conservar las pruebas posteriores S5.8/S6 como responsabilidades separadas. | CA01–CA08; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre), [roadmap → «Persistencia»](../../context/roadmap.md#persistencia) | T07 y aceptación humana T08 | Roadmap e índice apuntan al estado y evidencia reales; no se declara cerrada la etapa Persistencia por cerrar S5.5 | completada | `context/roadmap.md` y `specs/specs-index.md` sincronizados el 19-X-2026; S5.5 queda aceptada y cerrada solo para su alcance. |

T02–T04 se desarrollaron con el esquema S5.3 disponible. T05 depende de T01 ya resuelta y de T03–T04. T06 valida los registros de composición. T07 consolida sus evidencias; T08/T09 son posteriores a la validación y aceptación, respectivamente.

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T03, T04, T06, T07 | V01, V08, V09 | Evidencia satisfactoria de resolución DI y matriz de referencias | `SqliteInfrastructureRegistrationTests`; `ProjectReferenceMatrixTests`; suite SQLite 60/60 y suite completa 1005/1005, 19-X-2026, .NET 10.0.12 | Verificación técnica de alcance registrada en T07. |
| CA02 | T02, T07 | V02, V08 | Evidencia satisfactoria de UoW compartido y bloqueo inmediato | `SqliteUnitOfWorkTests`; suite SQLite 60/60 y suite completa 1005/1005, 19-X-2026, .NET 10.0.12 | V08 satisfecha; V09 registrada en T07. |
| CA03 | T02, T07 | V03, V08 | Evidencia satisfactoria de commit, rollback y disposición con rollback | `SqliteUnitOfWorkTests`; suite SQLite 60/60 y suite completa 1005/1005, 19-X-2026, .NET 10.0.12 | V08 satisfecha; V09 registrada en T07. |
| CA04 | T03, T07 | V04, V08 | Evidencia satisfactoria de aislamiento de lecturas y escrituras privadas | `SqliteCollectionRepositoryTests` y `SqliteLinkRepositoryTests`; suite SQLite 60/60 y suite completa 1005/1005, 19-X-2026, .NET 10.0.12 | V08 satisfecha; V09 registrada en T07. |
| CA05 | T03, T04, T07 | V05, V08 | Evidencia satisfactoria de CRUD y mapeo representativo de entidades/DTOs | Tests de repositorios de colección/enlace; suite SQLite 60/60 y suite completa 1005/1005, 19-X-2026, .NET 10.0.12 | V08 satisfecha; V09 registrada en T07. |
| CA06 | T04, T05, T07 | V05–V06, V08 | Evidencia satisfactoria de privacidad, selección, paginación y estados vacíos | Tests de dashboard y lecturas públicas; suite SQLite 60/60 y suite completa 1005/1005, 19-X-2026, .NET 10.0.12 | V08 satisfecha; V09 registrada en T07. |
| CA07 | T05, T07 | V06, V08 | Evidencia satisfactoria de orden con colación y desempate por Id | Pruebas de listados; suite SQLite 60/60 y suite completa 1005/1005, 19-X-2026, .NET 10.0.12 | El orden integrado completo sigue asignado a S5.8. |
| CA08 | T01, T05, T07 | V07–V08 | Evidencia satisfactoria: consultas sin términos pasan y términos se rechazan sin búsqueda sustitutiva | Pruebas de listados y conteos; suite SQLite 60/60 y suite completa 1005/1005, 19-X-2026, .NET 10.0.12 | FTS5 sigue fuera de S5.5 y corresponde a S6. |

## Aceptación y cierre

- Estado del trabajo: completado y aceptado por DLG el 19-X-2026 («Repositorios listos»).
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA08 cuentan con evidencia focalizada e integrada registrada.
- Bloqueos y riesgos restantes: no hay bloqueos propios de S5.5. El desfase de `LinkService.CreateLink` respecto a las comprobaciones dentro de `BEGIN IMMEDIATE` sigue siendo riesgo de extremo a extremo según [spec.md → «Bloqueos y preguntas pendientes»](spec.md#bloqueos-y-preguntas-pendientes), no evidencia que S5.5 pueda atribuirse.
- Aceptación humana: DLG, 19-X-2026 («Repositorios listos»).
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso: ninguno dentro del alcance de S5.5. El riesgo de `LinkService.CreateLink` queda como seguimiento independiente y no reabre este cierre.
