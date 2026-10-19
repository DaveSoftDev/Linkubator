# Plan técnico: S5.1 — Configuración de SQLite y Dapper

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»).
	- Plan: aprobado por DLG el 19-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

- **Dependencias de Infrastructure.** Añadir `Microsoft.Data.Sqlite` y Dapper al proyecto `Linkubator.Infrastructure`, con versiones explícitas compatibles con .NET 10. Application y Domain no referenciarán esos paquetes.
- **Configuración de conexión.** Obtener la ruta/connection string SQLite de la configuración de la aplicación y validarla al iniciar Infrastructure; no incluir una ruta local específica ni credenciales en código versionado.
- **Fábrica de conexiones.** Añadir en Infrastructure un componente común responsable de crear y abrir `SqliteConnection` con las opciones de conexión acordadas. El nombre y la API concretos son propuesta de este plan. Dapper se utilizará sobre esas conexiones para ejecutar SQL explícito en los trabajos que lo consuman.
- **Registro.** Ampliar `AddInfrastructure` para recibir la configuración necesaria y registrar la fábrica. Web seguirá siendo el composition root; no se trasladará lógica de persistencia a Application.
- **WAL.** Inicializar `journal_mode=WAL` una vez para el archivo de base de datos, antes de que se use concurrentemente. WAL es un modo persistente del archivo, no una opción independiente que deba cambiarse con cada apertura. Las conexiones abiertas por la fábrica deben observar ese modo.
- **Opciones por conexión.** Establecer explícitamente `busy_timeout` y claves foráneas en cada conexión creada. Propuesta para revisión: timeout finito de 5000 ms, centralizado en configuración y sin reintentos adicionales dentro de una transacción.
- **Inicialización.** La inicialización de WAL y el registro de la fábrica deben funcionar sin depender del esquema ni de los scripts versionados, que pertenecen a S5.2–S5.3.

### Flujos internos

1. Web proporciona la configuración a `AddInfrastructure` durante el registro de servicios.
2. Infrastructure valida y conserva la configuración SQLite necesaria para crear conexiones.
3. La inicialización prepara WAL una vez para el archivo configurado antes de su uso normal.
4. Cada llamada a la fábrica devuelve una conexión abierta con `busy_timeout` y claves foráneas activos.
5. Los consumidores posteriores ejecutarán SQL con Dapper sobre la conexión; el ciclo de transacción y los repositorios quedan en S5.5.

### Persistencia y dependencias

- La conexión se dirige al archivo indicado por configuración; las pruebas usarán archivos temporales independientes.
- La opción WAL se inicializa a nivel de base de datos. `busy_timeout` y claves foráneas se aplican por conexión y se verifican consultando las opciones SQLite activas.
- El valor propuesto de `busy_timeout` es configurable en un único punto. No se combinará con reintentos no acotados ni se ejecutarán esperas deliberadas dentro de transacciones.
- S5.2 es responsable del ejecutor de scripts y S5.3 del esquema. S5.4 registra la colación en cada conexión; la fábrica se diseñará de forma que ese registro pueda incorporarse allí sin cambiar el contrato de apertura, pero su implementación queda fuera de S5.1.
- S5.5 es responsable de Unit of Work, transacciones y repositorios Dapper. S5.8 cubre restricciones, carreras y orden alfabético; no se incorporan esos escenarios al trabajo S5.1.

### Garantías técnicas

- Cada conexión obtenida por la fábrica apunta a la base de datos configurada y permite ejecutar SQL mediante Microsoft.Data.Sqlite y Dapper.
- Cada conexión expone claves foráneas activas y el timeout explícito acordado; el archivo opera en WAL.
- Abrir una segunda conexión independiente no depende de que la primera permanezca abierta para aplicar sus opciones por conexión.
- Application y Domain permanecen independientes de SQLite y Dapper.
- La configuración no implementa esquema, scripts, colación, transacciones ni reglas de negocio, de acuerdo con el alcance de [spec.md](spec.md#alcance).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Usar `Microsoft.Data.Sqlite` y Dapper en Infrastructure | Confirmada por arquitectura | [architecture.md → «Tecnologías»](../../../context/architecture.md#tecnologías) y [«Infrastructure»](../../../context/architecture.md#infrastructure) | Mantiene el acceso a datos en Infrastructure; seleccionar versiones compatibles al implementar. |
| Centralizar la creación y apertura de conexiones en una fábrica de Infrastructure | Propuesta | CA01 y CA03 de [spec.md](spec.md#criterios-de-aceptación) | Evita que cada consumidor omita opciones comunes; aprobar en la revisión del plan. |
| Inicializar WAL una vez por archivo y verificarlo desde cada conexión | Propuesta técnica conforme al modo persistente de SQLite | CA02–CA03 de [spec.md](spec.md#criterios-de-aceptación) | Evita tratar el modo de diario como una opción volátil de cada conexión; revisar el punto de inicialización del archivo. |
| Configurar `busy_timeout` de forma explícita; valor de 5000 ms | Aprobada por DLG con este plan | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), CA02 de [spec.md](spec.md#criterios-de-aceptación) y aprobación del plan («Siguiente», 19-X-2026) | Afecta cuánto espera una operación bloqueada antes de fallar; mantener el valor centralizado y explícito. |
| Aplicar las claves foráneas en cada conexión de la fábrica | Confirmada por [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) y S5.1 | CA02–CA03 de [spec.md](spec.md#criterios-de-aceptación) | La conexión SQLite debe llevar la opción activa, no depender del estado de una conexión previa. |
| Añadir pruebas de configuración de conexión en S5.1, separadas de la suite de restricciones/carreras de S5.8 | Propuesta | CA01–CA03 de [spec.md](spec.md#criterios-de-aceptación) y [S5 → tareas S5.1/S5.8](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Hace cerrable S5.1 sin esperar al esquema, repositorios ni pruebas de S5.8. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada y técnica: dependencias y ejecución SQL | Paquetes restaurados; fábrica implementada | Inspeccionar el `.csproj` de Infrastructure y ejecutar una consulta escalar `SELECT 1` mediante Dapper sobre una conexión de la fábrica | Infrastructure referencia ambos paquetes, Application/Domain no los referencian y Dapper ejecuta la consulta en la conexión creada | Referencias revisadas, test/comando, SDK y resultado. |
| V02 | CA02 | Automatizada: opciones de una conexión | Fábrica e inicialización implementadas; archivo temporal | Test xUnit que abre una conexión y consulta `PRAGMA journal_mode`, `PRAGMA busy_timeout` y `PRAGMA foreign_keys` | WAL activo, timeout igual al valor configurado y claves foráneas activas | Test, valores observados, versión de .NET y resultado. |
| V03 | CA03 | Automatizada: independencia de conexiones | V02 disponible; archivo temporal | Test xUnit que abre dos conexiones de forma independiente al mismo archivo y consulta las tres opciones en ambas | Las dos conexiones observan WAL y tienen el timeout y las claves foráneas configurados, sin depender del estado de apertura de la primera | Test, resultados por conexión y resultado de la ejecución. |
| V04 | CA01–CA03 | Integrada: compilación y suite focalizada | Cambios de S5.1 completados; dependencias restauradas | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteConnection --verbosity minimal` y `dotnet build Linkubator.sln --no-restore --verbosity minimal` | Los tests focalizados y la compilación pasan; no se infiere aquí la validación de esquema, restricciones o transacciones de tareas posteriores | Comandos, SDK, códigos de salida y resumen de resultados. |

V02–V03 prueban la configuración de las conexiones de la fábrica, no el comportamiento de claves foráneas sobre tablas del esquema, las migraciones, ni las garantías de atomicidad y concurrencia, que pertenecen a S5.2–S5.8. La inspección de referencias comprueba la ubicación de paquetes, no el funcionamiento de las capas posteriores.

## Orden de ejecución y cierre

- Tras aprobar este plan, crear `tasks.md` desde la plantilla. Sus tareas implementarán los paquetes y la configuración, la fábrica/registro, y las pruebas V01–V04 con checks inmediatos.
- La aprobación de este plan confirma el `busy_timeout` de 5000 ms. Las tareas proponen `ConnectionStrings:DefaultConnection` como clave estándar de configuración; su aprobación debe confirmar ese contrato técnico antes de implementar.
- No iniciar S5.2–S5.8 como parte de este trabajo. Su esquema, scripts, colación, Unit of Work, identidad y pruebas propias no son prerrequisitos para cerrar CA01–CA03.
- S5.1 podrá cerrarse cuando todos sus criterios tengan evidencia satisfactoria, sus tareas estén completadas y DLG acepte explícitamente el resultado en `tasks.md`.
- Si la implementación requiere cambiar reglas de persistencia fuera de este alcance, detener la tarea afectada y tramitar el cambio en su documento propietario antes de continuar.