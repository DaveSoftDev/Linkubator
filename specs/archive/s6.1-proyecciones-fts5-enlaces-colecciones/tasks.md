# Tareas: S6.1 — Proyecciones FTS5 de enlaces y colecciones

## Referencias y autorización

- Estado: aprobadas.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 19-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 19-X-2026 («Vamos»), instrucción explícita en esta conversación.
- Implementación:
  - Estado: completada.
  - Inicio: DLG solicitó iniciar la implementación el 19-X-2026 («Inicia la implementación»), instrucción explícita en esta conversación.
  - Finalización: 19-X-2026; pruebas focalizadas 22/22 correctas y `git diff --check` limpio tras restaurar un CSS generado por build.
  - Evidencia consolidada: CA01–CA04 en «Evidencia por criterio»; tests focalizados de migración y persistencia 22/22 y suite completa 1050/1050, ambas el 19-X-2026.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T02 y T03 se implementaron dentro del mismo script V005 porque comparten la evolución de esquema; la revisión y validación integrada dependen de ambas proyecciones.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Inspeccionar scripts versionados, esquema actual y patrón de pruebas SQLite; documentar cualquier diferencia respecto de las fuentes antes de editar. | CA01–CA04; [plan → «Persistencia y dependencias»](plan.md#persistencia-y-dependencias) | Ninguna | Confirmar identificadores del último script real, ubicación del catálogo de producción y patrón de base temporal; no se presupone nombre de archivo ni test. | completada | Catálogo de producción incorporado como recurso embebido desde `Sqlite/Migrations`; V004 es el esquema vigente; tests usan base SQLite temporal y `SqliteMigrationCatalog.Load()`. Comprobación de fuentes realizada el 19-X-2026. |
| T02 | Añadir al esquema versionado la proyección FTS5 de enlaces con las columnas, vínculo de identificador/usuario y tokenizador definidos en las fuentes, sin etiquetas. | CA01–CA03; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos) | T01 | Aplicar el script en base de prueba y verificar que la proyección de enlaces se crea con la estructura esperada. | completada | `V005_CreateLinkAndCollectionSearchProjections.sql` crea `LinkSearch` con sus campos técnicos y tokenizador de arquitectura; prueba focalizada 4/4, 19-X-2026. |
| T03 | Añadir al esquema versionado la proyección FTS5 de colecciones con su campo, vínculo de identificador/usuario y tokenizador definidos en las fuentes. | CA01–CA03; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos) | T01 | Aplicar el script en base de prueba y verificar que la proyección de colecciones se crea con la estructura esperada. | completada | `V005_CreateLinkAndCollectionSearchProjections.sql` crea `CollectionSearch` con `Name`, `UserId` no indexado y tokenizador de arquitectura; prueba focalizada 4/4, 19-X-2026. |
| T04 | Validar conjuntamente la evolución, estructura, tokenización, aislamiento por entidad/usuario y catálogo real frente a base vacía y esquema vigente previo; añadir o ajustar tests focalizados. | CA01–CA04; [plan → V01](plan.md#estrategia-de-validación), [V02](plan.md#estrategia-de-validación), [V03](plan.md#estrategia-de-validación) | T01, T02, T03 | Ejecutar validaciones de integración focalizadas y demostrar que las pruebas detectan asociación de usuario o entidad incorrecta, y que repetir la migración conforme al ejecutor no produce fallos. | completada | Tests `SqliteSearchProjectionMigrationTests`, `SqliteMigrationServiceTests` y `SqlitePersistenceIntegrationTests`: 22/22 pasaron el 19-X-2026 (.NET SDK 10.0.401; SQLite 3.53.3); incluye creación/tokenización, aislamiento negativo, migración V004→V005 preservando datos y repetición. |
| T05 | Revisar el diff, `git diff --check`, enlaces y evidencia; sincronizar el roadmap con el avance de la etapa al iniciar y finalizar la implementación. | CA01–CA04; [plan → V04](plan.md#estrategia-de-validación) | T04 | Diff sin errores de whitespace; roadmap y tareas distinguen implementación técnica de aceptación humana. | completada | `git diff --check` limpio el 19-X-2026; se restauró `src/Linkubator.Web/wwwroot/css/styles.css`, modificado por el build. Revisados el script V005, los tests y las referencias SDD. Suite completa, tras corregir las expectativas obsoletas descritas en «Incidencias de validación»: `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj`, 1050/1050 el 19-X-2026. |
| T06 | Revisar evidencia por criterio y registrar aceptación humana o, si se rechaza, los bloqueos y correcciones concretas. | CA01–CA04; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T05 | Aceptación explícita registrada con responsable, fecha y referencia; sin ella, el trabajo permanece pendiente. | completada | Aceptación de DLG: «Tenemos proyeciones», 19-X-2026, instrucción explícita en esta conversación. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01–T04 | V01, V03 | Satisfactoria: dos proyecciones FTS5 separadas, migración repetible y aplicación desde base con esquema V004. | 19-X-2026; .NET SDK 10.0.401, SQLite 3.53.3 | Tests `SqliteSearchProjectionMigrationTests` y `SqliteMigrationServiceTests`, 22/22 en ejecución focalizada. |
| CA02 | T01, T02–T04 | V01 | Satisfactoria: columnas de enlaces y colecciones verificadas; tokenizador `unicode61` y equivalencia diacrítica comprobados, junto con búsqueda de palabra completa. | 19-X-2026; .NET SDK 10.0.401, SQLite 3.53.3 | `SqliteSearchProjectionMigrationTests`, incluido en resultado focalizado 22/22. |
| CA03 | T02–T04 | V02 | Satisfactoria: mismo `rowid` en proyecciones distintas no mezcla entidades; filtro por `UserId` excluye coincidencias ajenas en la consulta probada. | 19-X-2026; .NET SDK 10.0.401, SQLite 3.53.3 | `SqliteSearchProjectionMigrationTests`, comprobaciones positivas y negativas; ejecución focalizada 22/22. |
| CA04 | T01, T04 | V03 | Satisfactoria: catálogo avanza de V004 a V005 sobre datos existentes y la aplicación reiterada no duplica ni falla. | 19-X-2026; .NET SDK 10.0.401, SQLite 3.53.3 | `SqliteSearchProjectionMigrationTests` y `SqliteMigrationServiceTests`, ejecución focalizada 22/22. |
Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Incidencias de validación

- En la primera ejecución completa de `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj`, 4 de 1050 tests fallaron porque sus expectativas aún suponían que el catálogo solo contenía V004 y que el arranque registraba una única migración. Los fallos observados fueron `WebStartupTests.Startup_WhenDatabaseIsNew_AppliesMigrationsBeforeProvisioningAndServing` (esperaba una migración, encontró dos), `WebStartupTests.Startup_WhenProvisioningConfigurationIsInvalid_DoesNotServeRequests` y `WebStartupTests.Startup_WhenDevelopmentIdentityCannotBeResolved_DoesNotServeRequests` (ambos esperaban una migración, encontraron dos), y `DevelopmentIdentityProvisionerTests.ProvisioningSqlResource_IsEmbeddedOutsideVersionedMigrationCatalog` (esperaba solo V004, encontró V004 y V005).
- Corrección: actualizar esas cuatro expectativas para incluir V005, sin cambiar las comprobaciones funcionales de arranque ni el código productivo. Segunda ejecución completa: 1050/1050 pasaron el 19-X-2026.
- La ejecución intermedia inicial de las pruebas focalizadas también reveló errores en el test nuevo (fixture con nombre incorrecto, inclusión de tablas auxiliares FTS5 al enumerar tablas, sintaxis MATCH con términos separados por guiones, aserción de nombre de columnas y cierre de conexión durante limpieza). Se ajustó la prueba y las posteriores ejecuciones focalizada e integrada pasaron; no se registran esos intentos fallidos como evidencia satisfactoria.

## Aceptación y cierre

- Estado del trabajo: aceptado y cerrado.
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA04 tienen evidencia satisfactoria registrada arriba.
- Bloqueos y riesgos restantes: ninguno dentro del alcance de S6.1. La sincronización de escrituras y las consultas de búsqueda quedan para las tareas S6 posteriores.
- Aceptación humana: DLG, 19-X-2026 («Tenemos proyeciones»), instrucción explícita en esta conversación.
- Archivado: completado el 21-X-2026; traslado y enlaces entrantes/salientes comprobados.
- Ubicación archivada: `specs/archive/s6.1-proyecciones-fts5-enlaces-colecciones/`.
- Fecha efectiva de archivado y comprobación de enlaces: 21-X-2026; referencias recalculadas y verificadas.
- Siguiente paso autorizado, sin cerrar otras etapas: ninguno para S6.1; el trabajo está cerrado y archivado para su alcance.
