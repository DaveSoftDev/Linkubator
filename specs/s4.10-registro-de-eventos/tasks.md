# Tareas: S4.10 — Registro de eventos

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.
    - Plan: [plan.md](plan.md), aprobado por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.
    - Tareas: aprobadas por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.
- Implementación:
    - Estado: completada el 17-X-2026 tras la instrucción de DLG «Implementa».
    - Inicio: 17-X-2026, instrucción explícita en esta conversación.
    - Finalización: 17-X-2026; tests focalizados 7/7, suite 818/818, matriz de referencias 1/1, build correcto y `git diff --check` limpio con .NET SDK 10.0.401.
    - Evidencia consolidada: T02–T07 y «Evidencia por criterio».
- La implementación y la aceptación humana están completadas para el alcance de S4.10; el archivado no se ha solicitado.

## Descomposición

El inventario de escenarios y la matriz evento→escenario→prueba preceden al cierre de la instrumentación. Las ramas de éxito, rechazo y error deben comprobarse con un logger de captura; no se adjuntan excepciones potencialmente sensibles a los eventos de fallos de persistencia, y dichos fallos se propagan sin alteración.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar esta descomposición; confirmar trazabilidad y precedencias. | CA01–CA04; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Confirmar aprobación de tareas separada de spec y plan; no iniciar implementación antes de aprobar tasks. | completada | Aprobadas por DLG el 17-X-2026 («Siguiente»), instrucción explícita en esta conversación. |
| T02 | Inventariar casos de uso, métodos y ramas S4.6–S4.8 contra AC-TRANS-03 y las fuentes de eventos; construir matriz de eventos, campos permitidos/prohibidos y escenarios, incluyendo fallos de persistencia. | CA01, CA02, CA04; [plan → «Componentes y contratos» y «Flujos internos»](plan.md#diseño-de-implementación), V01 | T01 | Resultado esperado: trazabilidad de cada evento aplicable, escenario y campo; tratamiento seguro de fallos compatible con las fuentes o bloqueo explícito. | completada | Inventario de `CollectionService`, `LinkService`, `DashboardService` y `PublicReadService`: operaciones, validaciones/rechazos, cambios de visibilidad, lecturas y fallos no cancelados de repositorios/Unit of Work. Los fallos se registran con evento y metadatos operativos mínimos, sin adjuntar `Exception`, y se relanzan; cancelaciones excluidas. Fuentes contrastadas: AC-TRANS-03, [specifications.md → «Registro de eventos»](../../context/specifications.md#registro-de-eventos), [architecture.md → «Logs»](../../context/architecture.md#logs). No se modificaron reglas propietarias. |
| T03 | Implementar el logger de prueba que capture nivel, categoría, estado, mensaje y excepción sin truncar datos. | CA02–CA04; [plan → «Componentes y contratos»](plan.md#diseño-de-implementación), V03 | T02 | Probar la captura con valores estructurados y excepción; la captura debe conservar íntegros los datos recibidos por `ILogger`. | completada | `LoggingTests.TestLogger_CapturesStructuredStateAndAttachedException` comprueba estado estructurado y referencia de excepción; `LogEntry` conserva la excepción completa. 1/1 prueba, SDK 10.0.401, 17-X-2026. |
| T04 | Instrumentar operaciones de colecciones y enlaces según la matriz aprobada, preservando resultados y propagación de fallos. | CA01–CA03; [plan → «Instrumentación de Application» y «Flujos internos»](plan.md#diseño-de-implementación), V02–V03 | T02, T03 | Comprobar emisiones de éxito, cambios de visibilidad y ramas negativas, inspeccionando todos los campos capturados. | completada | `CollectionService` y `LinkService` instrumentados para operaciones/rechazos y cambios de visibilidad. `LoggingTests` prueba creación de enlace, visibilidad de colección, validación y fallo de listado con búsqueda centinela; verifica que el evento no contiene búsqueda ni excepción y que se relanza la misma instancia. Tests focalizados 7/7, SDK 10.0.401, 17-X-2026. |
| T05 | Instrumentar dashboard y lecturas públicas cuando la matriz confirme eventos aplicables. | CA01–CA03; [plan → «Instrumentación de Application» y «Flujos internos»](plan.md#diseño-de-implementación), V02–V03 | T02, T03 | Comprobar eventos aplicables, condiciones negativas y ausencia de datos prohibidos. | completada | La matriz confirmó lecturas y fallos de repositorio aplicables. `DashboardService` registra éxitos, rechazos y errores; `PublicReadService` registra éxitos, validaciones/no accesibilidad y fallos sin alias/slug en bruto. `LoggingTests` comprueba dashboard, lectura pública y ausencia del alias centinela. Tests focalizados 7/7, SDK 10.0.401, 17-X-2026. |
| T06 | Integrar la matriz final, verificar cobertura de criterios, campos del logger y dependencias de proyectos. | CA01–CA04; [plan → «Garantías técnicas»](plan.md#diseño-de-implementación), V01–V05 | T04, T05 | Resultado esperado: matriz evento→escenario→test completa, privacidad verificada y arquitectura permitida. | completada | Se inspeccionan mensaje, todos los valores estructurados y excepción. Se verificaron centinelas de credencial URL, texto de búsqueda, alias en bruto y excepción de repositorio; los fallos no adjuntan excepción. El logger de prueba captura excepciones cuando se suministran. Matriz de referencias 1/1; Application depende de `Microsoft.Extensions.Logging.Abstractions`, no del proveedor ni de Web/Infrastructure. 17-X-2026. |
| T07 | Repetir validaciones integradas de logging, suite, matriz, build y `git diff --check`. | CA01–CA04; [plan → «Estrategia de validación»](plan.md#estrategia-de-validación), V02–V05 | T06 | Tests y build correctos, matriz arquitectónica aprobada y diff limpio. | completada | SDK 10.0.401, 17-X-2026: tests focalizados `FullyQualifiedName~LoggingTests` 7/7; `dotnet test Linkubator.sln --nologo --verbosity minimal` 818/818; `ProjectReferenceMatrixTests` 1/1; `dotnet build Linkubator.sln --no-restore --verbosity minimal` correcto; `git diff --check` limpio. No valida configuración efectiva de Serilog ni logs HTTP, fuera del alcance. |
| T08 | Sincronizar tasks y roadmap, presentar el resultado a DLG y registrar por separado la aceptación humana si se produce. | CA01–CA04; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T07 | Revisar trazabilidad, enlaces, resultados y fechas; registrar la aceptación solo ante una decisión explícita. | completada | Tasks y [roadmap → «Now»](../../context/roadmap.md#now) sincronizados con la implementación y sus validaciones. La aceptación humana de DLG consta abajo con la referencia «Fase completada». 17-X-2026. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T04–T07 | V01, V02, V04–V05 | Satisfactoria para las categorías aplicables en Application: operaciones, validaciones/rechazos, cambios de visibilidad y fallos no cancelados de persistencia. | 17-X-2026; .NET SDK 10.0.401 | T02, T04–T07; tests focalizados 7/7, suite 818/818. El proveedor y HTTP no son evidencia de esta capa. |
| CA02 | T02–T04, T06–T07 | V01, V03–V05 | Satisfactoria para los centinelas ejercitados: credencial de URL, texto de búsqueda, alias en bruto y excepción de repositorio; inspecciona mensaje, estado y excepción adjunta. | 17-X-2026; .NET SDK 10.0.401 | T03–T07; `LoggingTests` 7/7. Los eventos de fallos de repositorio no adjuntan la excepción. |
| CA03 | T03–T07 | V02, V04–V05 | Satisfactoria: logger opcional/no-op no altera contratos; la prueba de fallo confirma que se relanza la misma excepción; suite sin regresiones. | 17-X-2026; .NET SDK 10.0.401 | T04–T07; tests 7/7, suite 818/818, matriz 1/1, build correcto. |
| CA04 | T01–T02, T06–T08 | V01, V04–V05 | Satisfactoria en trazabilidad y dependencias arquitectónicas; aceptación humana registrada por DLG. | 17-X-2026; .NET SDK 10.0.401 | T02, T06–T08 y aceptación y cierre; matriz 1/1, build correcto, `git diff --check` limpio. |

Las pruebas con logger no prueban el proveedor Serilog, sus destinos ni el filtrado de logs HTTP.

## Aceptación y cierre

- Estado del trabajo: implementación completada y aceptada por DLG para el alcance de S4.10.
- Criterios sin evidencia satisfactoria: ninguno para el alcance de Application; el cierre queda pendiente de aceptación humana explícita.
- Bloqueos y riesgos restantes: configuración efectiva del proveedor/destinos y logs HTTP fuera de esta etapa; no se presentan como evidencia de S4.10.
- Aceptación humana: DLG, 17-X-2026 («Fase completada»), instrucción explícita en esta conversación.
- Archivado: no solicitado.
- Siguiente paso: continuar según el roadmap. El trabajo está aceptado para su alcance; el archivado sigue sin solicitarse.
