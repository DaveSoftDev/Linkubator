# Tareas: S4.10 — Registro de eventos

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.
    - Plan: [plan.md](plan.md), aprobado por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.
    - Tareas: aprobadas por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.
- Implementación:
    - Estado: no iniciada.
    - Inicio: pendiente; registrar fecha y referencia real al iniciarla.
    - Finalización: pendiente; registrar fecha, comprobación y referencia real al completar las tareas de implementación y validación.
    - Evidencia consolidada: pendiente; enlazar las tareas y validaciones que sustentarán el estado.
- La preparación de tareas no inicia cambios productivos ni inventa aprobaciones o evidencias.

## Descomposición

T02 debe resolver primero el inventario de escenarios y el tratamiento seguro de fallos de persistencia, conforme a las fuentes aprobadas. Si el análisis detecta que la regla fuente no puede cumplirse de forma compatible, se detiene el criterio afectado antes de instrumentar. Las tareas de cambios de servicios y pruebas podrán paralelizarse solo si el inventario confirma que modifican clases y fakes independientes; de lo contrario, se ejecutarán secuencialmente para evitar conflictos.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar esta descomposición; confirmar que CA01–CA04 tienen tareas, validaciones y precedencias, y que no se inicia implementación antes de aprobar las tareas. | CA01–CA04; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Revisar trazabilidad CA → tareas → V01–V05 y el límite entre preparar/aprobar tasks e iniciar implementación; resultado esperado: aprobación explícita de tasks separada de spec y plan. | completada | Aprobadas por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación. |
| T02 | Inventariar casos de uso, métodos y ramas S4.6–S4.8 contra AC-TRANS-03 y la especificación de eventos; construir matriz de eventos, datos permitidos/prohibidos y escenarios, incluyendo la revisión de fallos de persistencia. Resolver cualquier bloqueo en la fuente propietaria, sin alterar reglas ni capturar excepciones inseguras. | CA01, CA02, CA04; [plan → «Componentes y contratos» y «Flujos internos»](plan.md#diseño-de-implementación), V01 | T01 | Revisar fuentes y código de Application; resultado esperado: matriz trazable de cada evento aplicable, rama y datos, y tratamiento compatible de fallos o bloqueo explícito donde la fuente no permita resolverlo. | pendiente | No ejecutada. |
| T03 | Diseñar e implementar el logger de prueba en Tests que capture nivel, categoría, estado estructurado, mensaje y excepción sin truncar campos. | CA02–CA04; [plan → «Componentes y contratos»](plan.md#diseño-de-implementación), V03 | T02 | Ejecutar pruebas específicas del logger con valores ordinarios y centinelas estructurados y en excepciones; resultado esperado: la captura expone íntegramente todos los datos entregados por `ILogger` para aserción. | pendiente | No ejecutada. |
| T04 | Inyectar `ILogger<T>` e instrumentar de forma incremental los casos de uso de colecciones y enlaces de acuerdo con la matriz aprobada, manteniendo resultados y propagación de fallos. | CA01–CA03; [plan → «Instrumentación de Application» y «Flujos internos»](plan.md#diseño-de-implementación), V02–V03 | T02, T03 | Ejecutar los tests de logging de las clases modificadas; resultado esperado: eventos en las ramas requeridas, ausencia en negativas aplicables, mismos resultados funcionales y ningún dato prohibido. | pendiente | No ejecutada. |
| T05 | Instrumentar dashboard y lecturas públicas solo si la matriz demuestra eventos aplicables, manteniendo el alcance de Application y sin registrar datos prohibidos. | CA01–CA03; [plan → «Instrumentación de Application» y «Flujos internos»](plan.md#diseño-de-implementación), V02–V03 | T02, T03 | Ejecutar tests específicos de las clases afectadas; resultado esperado: cobertura de los eventos respaldados por fuentes y verificación de no emisión en condiciones negativas. Si no hay ramas aplicables, registrar la justificación con trazabilidad y no añadir instrumentación. | pendiente | No ejecutada. |
| T06 | Integrar la matriz final y verificar resultados, errores, privacidad de datos capturados, ubicación de tests y dependencias de proyectos. | CA01–CA04; [plan → «Garantías técnicas»](plan.md#diseño-de-implementación), V01–V05 | T04, T05 | Revisar matriz, inspeccionar todos los campos del logger y ejecutar la matriz arquitectónica; resultado esperado: categorías aplicables cubiertas, ningún dato prohibido en las capturas y sin dependencias de proveedor/capas no permitidas. | pendiente | No ejecutada. |
| T07 | Ejecutar las validaciones integradas: tests de S4.10, suite, prueba de matriz de referencias, build y `git diff --check`; registrar resultados y límites. | CA01–CA04; [plan → «Estrategia de validación»](plan.md#estrategia-de-validación), V02–V05 | T06 | Ejecutar los comandos concretos de V02–V05; resultado esperado: tests y build pasan, matriz arquitectónica correcta y diff limpio; toda validación no disponible o fallida se mantiene explícita. | pendiente | No ejecutada. |
| T08 | Sincronizar el estado real de implementación y evidencias en estas tareas y en roadmap, presentar la entrega a DLG y registrar su aceptación por separado. | CA01–CA04; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T07 | Comprobar enlaces, matriz criterio→validación→evidencia, fechas y resultados; resultado esperado: documentos sincronizados y aceptación humana pendiente hasta decisión explícita. | pendiente | No ejecutada; no implica inicio de implementación ni aceptación. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T04–T07 | V01, V02, V04–V05 | Pendiente | No ejecutada | Sin evidencia |
| CA02 | T02–T04, T06–T07 | V01, V03–V05 | Pendiente | No ejecutada | Sin evidencia |
| CA03 | T03–T07 | V02, V04–V05 | Pendiente | No ejecutada | Sin evidencia |
| CA04 | T01–T02, T06–T08 | V01, V04–V05 | Pendiente | No ejecutada | Sin evidencia |

Un resultado no disponible o fallido sigue pendiente o bloqueado. Las pruebas con logger no prueban el proveedor Serilog, destinos configurados ni filtrado de logs HTTP.

## Aceptación y cierre

- Estado del trabajo: pendiente.
- Criterios sin evidencia satisfactoria: CA01–CA04.
- Bloqueos y riesgos restantes: el análisis de excepciones de persistencia debe respetar las fuentes propietarias; si no puede cumplirse sin exponer datos prohibidos o cambiar una regla, bloquear y solicitar decisión antes de continuar.
- Aceptación humana: pendiente; responsable, fecha y referencia cuando exista.
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso autorizado: iniciar la implementación tras instrucción explícita; la implementación permanece no iniciada hasta recibirla.
