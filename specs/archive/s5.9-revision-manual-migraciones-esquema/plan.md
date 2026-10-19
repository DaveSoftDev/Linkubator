# Plan técnico: S5.9 — Revisión manual de migraciones y esquema SQLite

## Estado y aprobación del plan

- Estado: aprobado, incluida la revisión de V01/V02.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.
	- Plan original: aprobado por DLG el 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.
	- Revisión de V01/V02: aprobada por DLG el 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- **Composición del arranque.** `Linkubator.Web` invoca `ISqliteMigrationService.ApplyPendingMigrations()` al iniciar, antes de ejecutar el aprovisionamiento de identidad. S5.9 usará ese flujo disponible; no añade contratos ni cambia el arranque.
- **Catálogo de scripts.** Infrastructure incrusta los archivos `Sqlite/Migrations/*.sql`; `SqliteMigrationCatalog` los ordena por versión y `SqliteMigrationService` registra las versiones en `schema_migrations`. La revisión abarcará el catálogo completo que efectivamente consume el servicio.
- **Base de inspección.** Propuesta: crear una base vacía en una carpeta temporal única y redirigir `ConnectionStrings__DefaultConnection` a ella durante el arranque. La aplicación también aprovisionará allí la identidad de desarrollo; el archivo de base habitual no se abrirá ni modificará.
- **Herramienta SQLite.** Propuesta de revisión: reutilizar `Microsoft.Data.Sqlite` desde los tests existentes para consultar una base temporal aislada; no añadir paquetes ni herramientas externas. La lectura del SQL versionado se hará en el editor y se contrastará manualmente con el esquema comprobado por esos tests.
- **Límite de revisión.** La inspección manual comprueba el SQL versionado y el esquema que deja aplicado. No demuestra la activación de PRAGMA por conexión ni las carreras de escritura, cubiertas por S5.1 y S5.8 respectivamente.

### Flujos internos

1. Ejecutar la prueba de integración de arranque Web, que crea una base temporal aislada y comprueba aplicación de migraciones, historial y aprovisionamiento.
2. Ejecutar las pruebas de esquema con el catálogo real, que consultan los metadatos SQLite mediante `Microsoft.Data.Sqlite` y validan tablas, columnas, índices, claves foráneas, restricciones y ausencia de objetos no aprobados.
3. Revisar manualmente `V004_CreateUserCollectionLinkSchema.sql` en el editor y contrastar sus definiciones con los resultados de las pruebas y las fuentes propietarias.
4. Registrar comandos, pruebas, SDK, salida observada, archivos SQL revisados y hallazgos. No cerrar CA02 ni CA03 mientras haya discrepancias sin resolver.

### Persistencia y dependencias

- Reutilizar el mecanismo de arranque y la fábrica SQLite existentes; no añadir migraciones, paquetes ni código de infraestructura en este trabajo.
- La ruta temporal se configura solo para el proceso de revisión. La base persistente `data/database/Linkubator.db` y los archivos versionados permanecen intactos.
- El arranque Web ejecuta también el aprovisionamiento de identidad después de las migraciones. Ese efecto queda limitado a la base temporal; no se usa esta ejecución para aceptar la repetibilidad del SQL local, que corresponde a S5.10.
- Las definiciones de colación registradas en las conexiones .NET no están disponibles en una conexión SQLite externa; la herramienta se usa para inspeccionar esquema, no para aceptar el orden alfabético. La evidencia de comportamiento integrado de listados permanece en S5.8.

### Garantías técnicas

- La aplicación de migraciones parte de una base aislada y vacía, y utiliza la misma secuencia de arranque que la aplicación.
- El esquema inspeccionado procede del mismo catálogo incrustado que ejecuta Infrastructure; la comparación incluye el registro de versiones y los objetos creados.
- La evidencia no afirmará que WAL, `busy_timeout` o las claves foráneas estén activos en cada conexión por el mero hecho de inspeccionar el archivo; esas garantías se contrastan con las validaciones de S5.1.
- No se aceptarán diferencias de esquema respecto a las fuentes propietarias sin resolverlas y registrar su resolución.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Sustituir CLI/GUI por el proveedor SQLite ya usado en los tests, más revisión manual del SQL versionado | Aprobada por DLG el 19-X-2026 («Siguiente») | [S5.1](../s5.1-configuracion-sqlite-dapper/spec.md), tests existentes `WebStartupTests` y `SqliteMigrationServiceTests`; respuesta de DLG «Cambiar el plan» | Evita incorporar herramientas y paquetes. Cambia el procedimiento de V01/V02; actualizar y volver a aprobar las tareas antes de reanudar S5.9. |
| Ejecutar V01 con el test Web existente y una base temporal de `WebApplicationFactory` | Aprobada por DLG al aprobar la revisión V01/V02 («Siguiente», 19-X-2026) | [V01](#estrategia-de-validación), `WebStartupTests` | Usa la conexión temporal del test y no accede a la base local persistente. |
| No hay CLI `sqlite3` ni GUI SQLite instalada en el entorno | Confirmado por DLG el 19-X-2026 («Ninguna disponible») | Respuesta explícita de DLG en esta conversación | Por ello se aprobó sustituir la inspección externa por los tests existentes con `Microsoft.Data.Sqlite` y revisión manual de `V004`; no instalar herramientas ni paquetes. |
| Excluir `V001`–`V003`, que crean y modifican `sample_data`, del catálogo aplicado en producción | Confirmado por DLG el 19-X-2026 («Excluir»); implementación aceptada en S5.2.1 («Completada», 19-X-2026) | [S5.2.1 → CA01–CA04](../s5.2.1-separar-scripts-prueba-catalogo/spec.md#criterios-de-aceptación), [S5.2.1 → cierre](../s5.2.1-separar-scripts-prueba-catalogo/tasks.md#aceptación-y-cierre) | El catálogo de producción solo incorpora V004; el prerrequisito para iniciar V01 está satisfecho. S5.9 mantiene su alcance de revisión manual y no necesita modificar scripts ni catálogo. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Integrada: arranque y migración sobre base SQLite temporal | SDK .NET y tests disponibles | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~Startup_WhenDatabaseIsNew_AppliesMigrationsBeforeProvisioningAndServing" --verbosity minimal` | El host de pruebas crea una base vacía, aplica las migraciones antes del aprovisionamiento y servicio, y el test comprueba el historial y estado de inicio esperados. | Test, comando, SDK y resultado observado. |
| V02 | CA02–CA03 | Integrada y revisión manual: inspección de esquema con Microsoft.Data.Sqlite y contraste del SQL | Tests de esquema disponibles; V01 satisfactorio | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~SqliteMigrationServiceTests" --verbosity minimal`; revisar `V004_CreateUserCollectionLinkSchema.sql` y contrastar sus definiciones con las aserciones SQLite de columnas, índices, claves foráneas, defaults, restricciones y triggers | Las consultas SQLite de las pruebas confirman el esquema aplicado; la revisión del SQL no encuentra discrepancias con el modelo y arquitectura aprobados, o las documenta y bloquea el cierre. | Tests y comando, SDK, archivo SQL revisado, resultados y discrepancias/resolución. |
| V03 | CA04 | Revisión documental y decisión humana | V01 y V02 con resultados registrados y sin discrepancias abiertas | Revisar la evidencia reunida y decidir expresamente si se acepta el resultado de S5.9 | Evidencia suficiente para que DLG acepte o documente un bloqueo; la revisión por sí sola no registra aceptación. | Referencia explícita de DLG, fecha y resultado, en [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre). |

V01 ejecuta también el aprovisionamiento normal de identidad sobre una base temporal y no demuestra que el SQL local sea repetible. V02 usa el proveedor SQLite en tests; no sustituye la revisión humana del SQL ni prueba por sí sola que WAL, `busy_timeout` y claves foráneas estén activos en cada conexión. V03 es la decisión de aceptación, no una comprobación técnica automática.

## Orden de ejecución y cierre

- `tasks.md` ya existe y su versión original fue aprobada. Como cambió el procedimiento V01/V02, sus tareas actualizadas deben aprobarse explícitamente antes de reanudar T04; no crear un segundo archivo de tareas.
- La decisión de excluir `V001`–`V003` fue implementada y aceptada mediante S5.2.1; el catálogo actual ya solo incorpora V004. La revisión de V01/V02 está aprobada por DLG; solo falta aprobar las tareas revisadas.
- La implementación de S5.9 es una revisión manual; no autoriza cambiar código ni scripts SQL. Los defectos encontrados se registran con su fuente y se tramitan en el trabajo propietario.
- S5.9 podrá cerrarse cuando CA01–CA04 tengan evidencia satisfactoria, no queden discrepancias abiertas, sus tareas estén completadas y DLG acepte expresamente el resultado en `tasks.md`. S5.10 y S6 no condicionan este cierre.
