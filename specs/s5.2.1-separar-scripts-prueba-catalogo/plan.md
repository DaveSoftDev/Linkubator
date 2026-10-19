# Plan técnico: S5.2.1 — Separar scripts de prueba del catálogo de migraciones

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.
	- Plan: aprobado por DLG el 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- **Catálogo de migraciones de producción.** Mantener `SqliteMigrationCatalog` como lector de recursos incrustados bajo `Linkubator.Infrastructure.Sqlite.Migrations.*`; retirar de esa carpeta los tres SQL de muestra `V001`–`V003`. `V004_CreateUserCollectionLinkSchema.sql` permanece como migración del esquema. No cambiar el filtro ni el contrato público del ejecutor.
- **Pruebas del ejecutor.** Conservar los casos de orden, idempotencia y error en `SqliteMigrationServiceTests`, que ya entregan sus `SqliteMigrationScript` directamente al servicio. Los SQL de muestra no necesitan duplicarse en otra carpeta de assets.
- **Consumidores del catálogo real.** Actualizar la comprobación de versiones en `DevelopmentIdentityProvisionerTests` para reflejar la migración real restante y ajustar la prueba de esquema para comprobar que ninguna tabla `sample_data` existe, en vez de excluirla del resultado.
- **Pruebas integradas.** Mantener el uso de `SqliteMigrationCatalog.Load()` en las bases temporales de identidad y repositorios; estas comprueban el mismo conjunto de migraciones que recibe la aplicación.

### Flujos internos

1. El catálogo lee los recursos SQL versionados de Infrastructure como hasta ahora.
2. El servicio de migraciones recibe solo el esquema de dominio y registra su versión al aplicarlo.
3. Los tests de comportamiento del ejecutor continúan creando colecciones de scripts en memoria; no dependen del catálogo de producción.
4. Los tests que necesitan una base de aplicación usan el catálogo real y comprueban esquema, historial, aprovisionamiento e invariantes integradas sin `sample_data`.
5. Una base con historial previo de `V001`–`V004` sigue arrancando: las versiones ya registradas se ignoran, no se borran filas del historial ni se eliminan tablas de bases existentes.

### Persistencia y dependencias

- Eliminar solamente los tres recursos SQL de muestra de `src/Linkubator.Infrastructure/Sqlite/Migrations/`; `V004` y el historial del servicio no cambian.
- Los tests unitarios no necesitan archivos SQL auxiliares: el constructor de `SqliteMigrationService` ya acepta una secuencia de `SqliteMigrationScript`.
- Infrastructure no modificará ni limpiará archivos de base de datos existentes. El cambio afecta al catálogo leído en futuros arranques y a bases nuevas; una base existente conserva cualquier tabla `sample_data` que ya tenga.
- No ejecutar ni validar aquí el aprovisionamiento manual repetido de S5.10 ni la inspección humana de S5.9.

### Garantías técnicas

- La aplicación no descubre ni aplica los tres scripts de muestra desde sus recursos incrustados.
- Las pruebas aisladas siguen verificando el orden, la repetición y el fallo de scripts sin añadir scripts de prueba al ensamblado de producción.
- La aplicación del catálogo real a una base vacía deja el esquema `User`, `Collection` y `Link` aprobado, sin crear `sample_data`.
- El cambio no borra ni altera los historiales de migración ni los datos de bases existentes; se comprobará el arranque con un historial previo que contenga las versiones retiradas.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Retirar los archivos `V001`–`V003` del directorio que se incrusta como catálogo de producción | Propuesta | [spec.md → CA01](spec.md#criterios-de-aceptación), [S5.9 → decisión de DLG](../s5.9-revision-manual-migraciones-esquema/plan.md#decisiones-técnicas-y-riesgos) | La carga del catálogo usa actualmente un comodín por carpeta; mover o borrar los tres archivos da el mismo resultado en producción. Se propone retirarlos porque los tests ya definen los scripts de prueba en memoria. |
| No conservar copias SQL físicas de los fixtures | Propuesta | [S5.2 → CA01–CA03](../s5.2-ejecutor-scripts-sql-versionados/spec.md#criterios-de-aceptación) y tests existentes de `SqliteMigrationService` | Evita dos fuentes para fixtures ya expresados en tests; reconsiderar solo si aparece un consumidor real de esos archivos. |
| Las entradas históricas `V001`–`V003` pueden permanecer en `schema_migrations` y `sample_data` puede permanecer en bases existentes | Confirmada por el alcance de S5.2.1 | [spec.md → Alcance](spec.md#alcance), [S5.2 → CA02](../s5.2-ejecutor-scripts-sql-versionados/spec.md#criterios-de-aceptación) | No se reescriben bases ni se ejecutan borrados; se comprueba que el migrador tolera el historial ya aplicado y omite `V004` si también está registrado. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada: catálogo de recursos de producción | Cambio de catálogo y test de recursos disponible | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~DevelopmentIdentityProvisionerTests" --verbosity minimal` | La enumeración del catálogo solo contiene la migración de esquema aprobada y no presenta `sample_data` ni las versiones `V001`–`V003`. | Test, filtro, SDK y resultado observado. |
| V02 | CA02 | Automatizada: orden, idempotencia y error del ejecutor | `SqliteMigrationServiceTests` compilables con scripts en memoria | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~SqliteMigrationServiceTests" --verbosity minimal` | Pasan las comprobaciones de orden, segunda ejecución sin cambios y fallo observable sin registro de la versión fallida ni continuación. | Tests ejecutados, resultado y versiones del entorno. |
| V03 | CA03 | Integrada: esquema real en base vacía | Catálogo real con `V004`; fábrica SQLite de tests | Ejecutar la prueba de esquema de `SqliteMigrationServiceTests` y consultar sus aserciones de tablas, columnas, índices y claves foráneas | La base migrada contiene `User`, `Collection` y `Link`, no contiene `sample_data`, y el historial contiene solo las versiones del catálogo real. | Test, esquema observado y contenido del historial. |
| V04 | CA04 | Integrada: consumidores reales y compatibilidad del historial | Tests de identidad y repositorios disponibles; incluir caso de historial previo `V001`–`V004` en test de migración | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~DevelopmentIdentityProviderTests|FullyQualifiedName~DevelopmentIdentityProvisionerTests|FullyQualifiedName~Sqlite" --verbosity minimal` | Los consumidores reales inician con el catálogo ajustado; un historial anterior no provoca error ni borrado de datos y las pruebas de esquema/aprovisionamiento pasan. | Filtro, tests, salida, historial anterior y estado de la base temporal. |
| V05 | CA01–CA04 | Integrada: regresión de proyecto | Todas las pruebas focalizadas correctas | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --verbosity minimal` y `dotnet build Linkubator.sln --no-restore --verbosity minimal` | Suite y build correctos; ninguna prueba ni consumidor de catálogo depende de los SQL de muestra retirados. | Comandos, SDK, códigos de salida y resúmenes. |

V01 solo verifica la enumeración de recursos; V02 mantiene las garantías del ejecutor con fixtures en memoria. V03 cubre una base nueva. V04 debe incluir expresamente compatibilidad de historial previo y no demuestra la inspección manual que corresponde a S5.9.

## Orden de ejecución y cierre

- [tasks.md](tasks.md) se creó tras la aprobación de este plan y queda pendiente de aprobación por separado. Las tareas cubren retirar los recursos de muestra, actualizar consumidores del catálogo, validar una base nueva y un historial existente, y consolidar evidencia.
- No borrar datos de bases existentes ni cambiar el comportamiento del ejecutor. Si las pruebas revelan que el migrador no tolera versiones previas ausentes del catálogo, detenerse y proponer el ajuste en una spec de seguimiento.
- S5.2.1 podrá cerrarse cuando CA01–CA04 tengan evidencia satisfactoria, la suite/build requeridos pasen, sus tareas estén completadas y DLG acepte explícitamente el resultado en `tasks.md`. Su cierre habilita la aprobación de las tareas de S5.9; no acepta ni cierra S5.9.