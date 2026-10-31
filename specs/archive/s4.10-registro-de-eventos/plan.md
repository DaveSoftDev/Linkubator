# Plan técnico: S4.10 — Registro de eventos

> Este plan conserva el diseño aprobado originalmente. La regla vigente es [specifications.md → «Registro de eventos»](../../../context/specifications.md#registro-de-eventos); la exclusión de búsquedas y contenido introducido por el usuario descrita aquí no es una exigencia vigente.

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.
	- Plan: aprobado por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- **Inventario de eventos.** Derivar de [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación), [AC-TRANS-03](../../../plans/mvp0-acceptance-criteria.md#ac-trans-03--registro-de-eventos) y [specifications.md → «Registro de eventos»](../../../context/specifications.md#registro-de-eventos) la matriz de casos de uso/métodos → evento requerido → datos permitidos/prohibidos → escenario de prueba. La matriz delimita los eventos de S4.10 y no crea reglas o categorías funcionales nuevas.
- **Instrumentación de Application.** Inyectar `ILogger<T>` por constructor en los servicios de S4.6–S4.8 que deban registrar los eventos identificados por la matriz. Usar el logger de la clase actual y logging estructurado; no añadir una abstracción propietaria ni dependencias al proveedor. El inventario decide en qué ramas registrar, de acuerdo con las fuentes y sin cambiar resultados de `Result` ni la semántica de los casos de uso.
- **Errores de persistencia.** Identificar en el plan de tareas los límites donde se propagan fallos de repositorio/Unit of Work. El registro debe satisfacer la obligación de eventos de persistencia sin incluir mensajes, parámetros, valores o datos de excepción que puedan revelar información prohibida. Si la excepción no puede registrarse con seguridad con la evidencia disponible, dejar el detalle completo fuera del evento y elevar cualquier conflicto con la fuente propietaria; no capturar ni silenciar excepciones como efecto colateral del logging.
- **Pruebas.** Añadir pruebas enfocadas en la instrumentación al proyecto existente `tests/Linkubator.Tests`, con un logger de prueba que conserve nivel, estado estructurado, excepción y categoría. Colocar tests en la organización existente de Application. Las pruebas de S4.9 se reutilizan como contexto de comportamiento, pero no se modifican salvo que una dependencia concreta de constructor lo requiera y quede justificada.

### Flujos internos

1. Inventariar los métodos y ramas de S4.6–S4.8 frente a AC-TRANS-03 y las categorías de las fuentes propietarias; registrar qué eventos corresponden y qué valores pueden incluirse.
2. Revisar los límites de propagación de errores de persistencia para definir una comprobación segura sin alterar rollback, resultados ni propagación de excepciones.
3. Instrumentar de forma incremental los servicios y ramas identificados, manteniendo los contratos y las decisiones funcionales existentes.
4. Por cada evento, probar la condición que lo emite y la condición negativa que debe evitarlo; inspeccionar mensaje, estado estructurado y excepción capturada por el logger.
5. Usar valores centinela en búsquedas y otros datos prohibidos, y en las entradas/excepciones pertinentes. Buscar esos valores en todos los campos capturados, no solo en el texto renderizado del mensaje.
6. Ejecutar las pruebas específicas tras cada cambio, luego la suite, el build y las verificaciones arquitectónicas; actualizar evidencia solo con resultados observados.
7. Si una categoría no puede conciliarse con las fuentes o requiere cambiar una regla, detener el punto afectado y elevarlo al documento propietario; no resolverlo por inferencia en código o plan.

### Persistencia y dependencias

- El trabajo modifica Application y sus pruebas en Tests; no implementa repositorios, transacciones ni SQL. Los eventos pueden observar resultados de operaciones de repositorio, pero no deben mantener una transacción abierta ni introducir operaciones externas.
- `ILogger<T>` es la abstracción de logging de .NET indicada por arquitectura. La configuración de Serilog, destinos, niveles efectivos y registro de peticiones sigue perteneciendo a sus etapas de configuración/Infrastructure/Web.
- No se prevén paquetes o servicios externos nuevos. El logger de prueba se implementará con utilidades existentes del proyecto de tests, sin introducir un framework general de logging de pruebas.

### Garantías técnicas

- La instrumentación no cambia resultados, códigos de error, persistencia, orden de operaciones ni decisiones de autorización de los casos de uso.
- Ningún mensaje, estado estructurado ni excepción adjunta contiene datos prohibidos; la aserción inspecciona todos los campos recibidos por el logger.
- Los eventos son verificables sin depender de Serilog ni del formato final de consola/archivo.
- Application mantiene las dependencias autorizadas; los tipos de logging y los tests no se ubican en Domain ni crean dependencias a Web/Infrastructure.
- Las pruebas no acreditan la configuración de destinos ni el filtrado del registro HTTP; esas garantías se validan por sus etapas propietarias.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Inyectar `ILogger<T>` desde el constructor de cada servicio que requiera eventos | Propuesta | [architecture.md → «Logs»](../../../context/architecture.md#logs), [csharp.instructions.md](../../../.github/instructions/csharp.instructions.md) | Mantiene la abstracción de logging y la inyección por constructor; confirmar en la aprobación del plan y revisar impacto en composición/tests. |
| Usar logging estructurado y no adjuntar excepciones no revisadas que puedan filtrar valores prohibidos | Propuesta técnica | [specifications.md → «Registro de eventos»](../../../context/specifications.md#registro-de-eventos) | Permite auditar los datos capturados; el inventario de errores de persistencia debe demostrar cómo se registra el evento sin exponer datos sensibles ni ocultar el fallo. |
| El test logger debe inspeccionar estado, excepción y mensaje, no solo texto renderizado | Propuesta técnica | CA02–CA04 de [spec.md](spec.md#criterios-de-aceptación) | Evita falsos negativos por valores estructurados o excepciones; validar que la captura representa todos los parámetros entregados a `ILogger`. |
| Compatibilizar la categoría «errores de persistencia» con excepciones potencialmente sensibles | Pendiente de análisis en tareas | [specifications.md → «Registro de eventos»](../../../context/specifications.md#registro-de-eventos) y contratos de S4.2 | No ampliar ni debilitar la regla; documentar el tratamiento seguro derivado de las fuentes. Si no existe opción compatible, bloquear y solicitar decisión en el documento propietario. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA04 | Técnica: matriz de eventos | Tareas aprobadas; fuentes disponibles | Revisar métodos públicos y ramas de S4.6–S4.8 frente a AC-TRANS-03 y `specifications.md` | Cada evento aplicable queda asociado a fuente, rama y datos permitidos; nada se inventa ni se omite sin registrar bloqueo | Matriz revisada, fuentes, rutas de código y fecha. |
| V02 | CA01, CA03 | Automatizada: emisión y no emisión de eventos | Logger de prueba y casos de uso instrumentados | Ejecutar el filtro real de los tests S4.10, que se fijará en tasks una vez establecidos los nombres de las clases | Los escenarios aplicables emiten el evento esperado, las ramas que no cumplen la condición no lo emiten y el resultado de negocio permanece intacto | Comando/filtro concreto, SDK, pruebas y resultados. |
| V03 | CA02 | Automatizada: datos prohibidos | Logger de prueba que capture estado, excepción y mensaje | Ejecutar los tests con centinelas para búsquedas y los demás datos prohibidos aplicables | Ningún centinela aparece en ningún campo capturado; la prueba detecta también inserciones estructuradas o excepciones adjuntas | Centinelas usados, campos inspeccionados, resultado y fecha. |
| V04 | CA01–CA04 | Automatizada e integrada | Instrumentación integrada | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --verbosity minimal` | Suite disponible completa sin regresiones; el resultado solo acredita tests ejecutados, no el proveedor ni HTTP | Comando, SDK, exit code y resumen. |
| V05 | CA03–CA04 | Técnica: arquitectura y compilación | Cambios integrados | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --verbosity minimal --filter "FullyQualifiedName~ProjectReferenceMatrixTests"`; `dotnet build Linkubator.sln --no-restore --verbosity minimal`; `git diff --check` | Matriz de referencias y build pasan; diff limpio y Application no adquiere acoplamiento al proveedor o a otras capas | Comandos, SDK, exit codes y resultado de revisión. |

Las pruebas con logger de captura no demuestran que Serilog persista los eventos en consola/archivo ni que los logs HTTP excluyan consultas; estas comprobaciones pertenecen a las etapas que configuran esos componentes.

## Orden de ejecución y cierre

- Tras la aprobación de este plan, crear `tasks.md` desde la plantilla; incluir primero el inventario y la resolución del análisis de excepciones de persistencia, y luego instrumentación y pruebas por bloques con checks inmediatos.
- No iniciar cambios productivos ni crear tareas hasta que este plan tenga aprobación humana explícita.
- El cierre requiere evidencia satisfactoria de CA01–CA04 dentro del alcance S4.10, tareas propias completadas y aceptación humana registrada en tasks. No depende de la configuración de Serilog, Infrastructure/Web ni registro HTTP.
- Ante un conflicto con reglas fuente, detener la tarea afectada y solicitar su resolución en el documento propietario; la suite verde no constituye aceptación ni autoriza modificar el alcance.
