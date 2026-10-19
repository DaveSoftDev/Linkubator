# Tareas: S5.1 — Configuración de SQLite y Dapper

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 19-X-2026 («Siguiente»).
    - Tareas: aprobadas por DLG el 19-X-2026 («Siguiente»); se confirma `ConnectionStrings:DefaultConnection`.
- Implementación:
    - Estado: completada.
    - Inicio: DLG, 19-X-2026 («implementa»).
    - Finalización: DLG, 19-X-2026 («Tenemos BBDD»); tests focalizados 2/2 y build de solución correctos.
    - Evidencia consolidada: T02–T06 y CA01–CA03, detallados abajo. El archivo `data/database/Linkubator.db` aún no existe; la configuración lo creará al abrir una conexión.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

La clave `ConnectionStrings:DefaultConnection` fue aprobada con estas tareas y se configurará desde el entorno local/de ejecución, sin guardar una ruta de base de datos específica en el repositorio. El timeout de 5000 ms quedó confirmado con la aprobación del plan. T04 configura WAL a nivel del archivo y las opciones de timeout y claves foráneas en cada conexión. T05 valida la independencia de dos conexiones al mismo archivo temporal; no depende del esquema ni de S5.8.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas, confirmando cobertura y el contrato `ConnectionStrings:DefaultConnection`. | CA01–CA03; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Comprobar que cada criterio tiene tareas y validación, que el alcance no incluye S5.2–S5.8 y que la clave propuesta es aceptada; resultado esperado: aprobación explícita de tareas, separada de spec y plan. | completada | DLG aprueba las tareas y confirma la clave con «Siguiente», 19-X-2026. |
| T02 | Añadir `Microsoft.Data.Sqlite` y Dapper como dependencias de Infrastructure, con versiones explícitas compatibles con .NET 10. | CA01; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V01 | T01 | Restaurar y compilar Infrastructure; revisar referencias de proyecto; resultado esperado: ambos paquetes están en Infrastructure y Application/Domain no dependen de ellos. | completada | `Linkubator.Infrastructure.csproj` referencia Dapper 2.1.66 y Microsoft.Data.Sqlite 10.0.0; SQLitePCLRaw.lib.e_sqlite3 3.53.3 evita la versión vulnerable transitiva. Build correcto, 19-X-2026. |
| T03 | Añadir lectura y validación de `ConnectionStrings:DefaultConnection`, y registrar en Infrastructure la fábrica de conexiones mediante `AddInfrastructure`. | CA01; [plan → Componentes y contratos](plan.md#componentes-y-contratos) y [Flujos internos](plan.md#flujos-internos), V01 | T02 | Resolver la fábrica desde el contenedor de servicios con una configuración temporal válida y crear una conexión abierta; resultado esperado: la configuración es obligatoria y el registro común crea la conexión dirigida a la base de datos configurada. | completada | Revisión de `AddInfrastructure(IConfiguration)`: valida `ConnectionStrings:DefaultConnection` y registra la fábrica; Web configura `Data Source=data/database/Linkubator.db`. Build correcto, 19-X-2026. El test focalizado valida el comportamiento de la fábrica directamente. |
| T04 | Inicializar WAL para el archivo y aplicar explícitamente `busy_timeout` de 5000 ms y claves foráneas en cada conexión de la fábrica. | CA02–CA03; [plan → Persistencia y dependencias](plan.md#persistencia-y-dependencias) y [Garantías técnicas](plan.md#garantías-técnicas), V02–V03 | T03 | Abrir conexiones al archivo temporal y consultar los PRAGMA; resultado esperado: WAL activo, timeout de 5000 ms y claves foráneas activas en ambas conexiones. | completada | `SqliteConnectionFactory` ejecuta WAL, busy_timeout=5000 y foreign_keys=ON. Tests temporales verifican estos valores en ambas conexiones independientes; 2/2 correctos, 19-X-2026. |
| T05 | Añadir tests xUnit focalizados para SQL con Dapper y las opciones SQLite de una y dos conexiones independientes. | CA01–CA03; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V01–V03 | T04 | Ejecutar el filtro `FullyQualifiedName~SqliteConnection`; resultado esperado: consulta escalar por Dapper y comprobaciones de PRAGMA pasan con un archivo temporal, sin requerir esquema. | completada | `SqliteConnectionFactoryTests`: `SELECT 1` por Dapper y PRAGMA en una y dos conexiones; filtro ejecutado 19-X-2026, 2/2 correctos. |
| T06 | Ejecutar validación integrada y resolver solo fallos del alcance S5.1. | CA01–CA03; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V04 | T05 | Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteConnection --verbosity minimal` y `dotnet build Linkubator.sln --no-restore --verbosity minimal`; resultado esperado: tests focalizados y build correctos, con cualquier límite de validación explícito. | completada | Ejecutados ambos comandos el 19-X-2026 con .NET SDK 10.0.12: tests 2/2 correctos y build de `Linkubator.sln` correcto. `data/database` existe; `Linkubator.db` aún no se ha creado porque no se ha abierto una conexión de la aplicación. |
| T07 | Revisar la evidencia por criterio, sincronizar el estado de S5.1 en el roadmap y solicitar aceptación humana del resultado. | CA01–CA03; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T06 | Confirmar evidencia satisfactoria de CA01–CA03 y actualizar el roadmap sin declarar terminadas otras tareas S5; resultado esperado: aceptación explícita de DLG o bloqueo documentado. | completada | Evidencia CA01–CA03 revisada y roadmap sincronizado. DLG acepta el resultado el 19-X-2026 con «Tenemos BBDD». |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02–T03, T05–T06 | V01, V04 | Infrastructure incluye SQLite y Dapper; la fábrica común configurada se prueba mediante SELECT 1 con Dapper. | 19-X-2026, .NET SDK 10.0.12 | Tests `FullyQualifiedName~SqliteConnection`: 2/2 correctos; build de solución correcto. |
| CA02 | T04–T06 | V02, V04 | La conexión temporal observa journal_mode=wal, busy_timeout=5000 y foreign_keys=1. | 19-X-2026, .NET SDK 10.0.12 | `SqliteConnectionFactoryTests.CreateOpenConnection_SetsWALBusyTimeoutAndForeignKeys`, correcto. |
| CA03 | T04–T06 | V03–V04 | Dos conexiones independientes al mismo archivo observan WAL, busy_timeout=5000 y foreign_keys=1. | 19-X-2026, .NET SDK 10.0.12 | `SqliteConnectionFactoryTests.CreateOpenConnection_IndependentConnectionsKeepSameSettings`, correcto. |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: completado y aceptado.
- Criterios sin evidencia satisfactoria: ninguno para el alcance S5.1.
- Bloqueos y riesgos restantes: el archivo físico `data/database/Linkubator.db` se crea al abrir la aplicación y todavía no existe; el directorio está preparado. Esto no invalida las pruebas con bases temporales.
- Aceptación humana del resultado: DLG, 19-X-2026 («Tenemos BBDD»).
- Archivado: pendiente; DLG solicitó archivar los trabajos S5 cerrados el 19-X-2026 («Archiva las tareas pendientes»).
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso autorizado: S5.1 queda cerrada para su alcance; el archivado no se ha solicitado.