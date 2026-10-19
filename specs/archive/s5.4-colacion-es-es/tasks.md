# Tareas: S5.4 — Registro de la colación `es-ES` en las conexiones SQLite

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 19-X-2026 («Siguiente»); nombre SQL `LINKUBATOR_ES_ES` confirmado por DLG el 19-X-2026.
    - Tareas: aprobadas por DLG el 19-X-2026 («Ready»).
- Implementación:
    - Estado: completada.
    - Inicio: 19-X-2026, con la instrucción de DLG «implementa».
    - Finalización: 19-X-2026; tests focalizados 5/5, build de solución correcto y `git diff --check` limpio.
    - Evidencia consolidada: [SqliteConnectionFactory.cs](../../../src/Linkubator.Infrastructure/Sqlite/SqliteConnectionFactory.cs), [SqliteConnectionFactoryTests.cs](../../../tests/Linkubator.Tests/Infrastructure/SqliteConnectionFactoryTests.cs) y validación integrada registrada en T05.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

La S5.4 reutiliza la fábrica de conexiones de S5.1. La implementación y sus pruebas son secuenciales: primero se fija y registra el comparador en cada conexión; después se comprueba el alcance y la comparación cultural; finalmente se ejecuta la validación integrada y se revisan los límites. El trabajo no implementa las consultas de repositorio ni espera a las pruebas integradas de orden de S5.8.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas, confirmando cobertura de criterios, validaciones y alcance. | CA01–CA03; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Verificar dependencias, criterios y límites respecto de S5.5/S5.8; resultado esperado: aprobación explícita de tareas, separada de spec y plan. | completada | DLG aprobó las tareas con «Siguiente», 19-X-2026. |
| T02 | Registrar en la fábrica de conexiones el comparador `es-ES` con las opciones aprobadas y el nombre SQL `LINKUBATOR_ES_ES`. | CA01–CA02; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [Flujos internos](plan.md#flujos-internos) | T01 | Abrir conexión por la fábrica y ejecutar una comparación SQL con `COLLATE LINKUBATOR_ES_ES`; resultado esperado: SQLite resuelve la colación y completa la comparación. | completada | `SqliteConnectionFactory.CreateOpenConnection()` registra la colación antes de entregar cada conexión; la consulta explícita devuelve el resultado esperado. |
| T03 | Añadir pruebas focalizadas para disponibilidad por conexión y comportamiento de comparación cultural. | CA01–CA02; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V01–V02 | T02 | Ejecutar tests de colación sobre dos conexiones independientes; resultado esperado: ambas resuelven la colación y comparan conforme a las opciones aprobadas, sin exigir que `ñ` se distinga de `n`. | completada | Las pruebas verifican equivalencia de «Árbol»/«arbol» y el orden respecto de «barranco»; la colación queda disponible en ambas conexiones independientes. |
| T04 | Comprobar el alcance explícito de la colación y que no se introducen cambios de esquema, índices o unicidad. | CA03; [plan → Garantías técnicas](plan.md#garantías-técnicas), [Estrategia de validación](plan.md#estrategia-de-validación), V03 | T02, T03 | Ejecutar la comparación con y sin `COLLATE` y revisar el diff de persistencia; resultado esperado: el callback se usa solo al declarar la colación y no hay DDL ni consultas de repositorios modificados. | completada | La prueba confirma que la comparación explícita ignora diacríticos y la predeterminada de SQLite no; el diff de implementación no modifica DDL, índices ni consultas de repositorio. |
| T05 | Ejecutar validación integrada y revisar los límites de responsabilidad. | CA01–CA03; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V04–V05 | T03, T04 | Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteConnectionFactoryTests --verbosity minimal`, `dotnet build Linkubator.sln --no-restore --verbosity minimal` y `git diff --check`; resultado esperado: tests y build correctos, diff limpio y cambios limitados al registro y sus tests. | completada | Ejecutado el 19-X-2026 con .NET SDK 10.0.12: tests focalizados 5/5, build de solución correcto y `git diff --check` correcto. El diff productivo se limita a la fábrica; se añadieron pruebas en la clase focalizada. |
| T06 | Revisar evidencia por criterio, sincronizar el roadmap y solicitar aceptación del resultado. | CA01–CA03; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T05 | Confirmar evidencia satisfactoria para CA01–CA03, actualizar el estado real de implementación y roadmap; resultado esperado: aceptación explícita de DLG o bloqueo documentado, sin declarar cerrada la etapa S5. | completada | DLG aceptó el resultado con «Colación añadida», 19-X-2026. Roadmap, spec, plan y este cierre quedaron sincronizados; S5 completa permanece abierta. |

La tarea T06 incluye la actualización documental del estado de S5.4 en el roadmap; no altera el estado de otros trabajos ni anticipa el cierre de la etapa de persistencia.

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02–T03, T05 | V01, V04 | Cumplido | 19-X-2026; .NET SDK 10.0.12 | El test `CreateOpenConnection_RegistersSpanishCollationForSqliteSorting` ejecuta consultas explícitas con `COLLATE LINKUBATOR_ES_ES`; filtro `SqliteConnectionFactoryTests` 5/5. |
| CA02 | T02–T03, T05 | V02, V04 | Cumplido | 19-X-2026; .NET SDK 10.0.12 | `CreateOpenConnection_RegistersSpanishCollationOnIndependentConnections` confirma la disponibilidad y equivalencia acentual en dos conexiones independientes; filtro focalizado 5/5. |
| CA03 | T04–T05 | V03–V05 | Cumplido | 19-X-2026; .NET SDK 10.0.12 | `CreateOpenConnection_CollationOnlyAffectsExpressionsThatDeclareIt` contrasta comparación explícita y predeterminada; inspección del diff confirma ausencia de cambios de DDL, índices, unicidad y consultas de repositorio. Build y `git diff --check` correctos. |

## Aceptación y cierre

- Estado del trabajo: aceptado y cerrado para el alcance de S5.4.
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA03 tienen evidencia satisfactoria.
- Bloqueos y riesgos restantes: ninguno para el alcance S5.4. El orden integrado de los listados corresponde a S5.8 y no bloquea este cierre.
- Aceptación humana: DLG, 19-X-2026 («Colación añadida»).
- Archivado: completado el 19-X-2026; traslado y enlaces entrantes/salientes comprobados.
- Ubicación archivada: `specs/archive/s5.4-colacion-es-es/`.
- Fecha efectiva de archivado y comprobación de enlaces: 19-X-2026; referencias recalculadas y verificadas.
- Siguiente paso autorizado: continuar con el siguiente trabajo del roadmap; S5.4 queda cerrada para su alcance. El archivado no se ha solicitado.
