# Tareas: S7.8 — Registro de peticiones Web

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 24-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 24-X-2026 («Siguiente»).
  - Tareas: DLG, 24-X-2026 («Vamos»).
- Implementación:
  - Estado: en curso; comenzó el 24-X-2026 tras la instrucción «Inicia la implementación».
  - Finalización: pendiente; falta validar el evento real emitido por el middleware y completar la revisión negativa.
  - Evidencia consolidada: prueba focalizada del helper de saneado, 1/1 pasada el 24-X-2026; no demuestra aún la captura del evento real de request logging.
- Aceptación humana: DLG, 24-X-2026 («Completado»), registrada según la instrucción explícita. No se declara cerrado el trabajo mientras falte evidencia técnica satisfactoria de CA01–CA04.

## Descomposición

La comprobación del middleware debe observar eventos reales emitidos por el host de pruebas. No es preciso que S7.9 ni las rutas funcionales futuras existan para validar las garantías de registro propias de esta especificación.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Preparar captura aislada de eventos Serilog en el host de `WebApplicationFactory`, sin alterar los destinos globales de la aplicación. | CA01; componentes y contratos de [plan.md](plan.md#diseño-de-implementación). | Ninguna | Compilar el proyecto de tests y comprobar que el sink recibe eventos del host de prueba sin afectar a otros tests. | bloqueada | El sink propuesto con `ConfigureLogging`/`AddSerilog` y luego la sustitución de `Serilog.ILogger` en test no capturó eventos del middleware; hace falta resolver la conexión en el host antes de afirmar evidencia del pipeline real. |
| T02 | Configurar el registro de peticiones en el pipeline Web para omitir la query de rutas privadas y no emitir URL completa en las rutas con tokens. | CA01–CA04; flujos y garantías de [plan.md](plan.md#flujos-internos). | T01 | Revisar el pipeline y ejecutar tests focalizados: se registra la petición con información operativa no sensible, sin query privada ni URL tokenizada. | en curso | El helper está conectado a `UseSerilogRequestLogging` mediante `EnrichDiagnosticContext`; la prueba focalizada confirma la redacción del helper, no el evento real del middleware. Revisión de [Program.cs](../../src/Linkubator.Web/Program.cs) y [RequestLoggingDiagnosticContext.cs](../../src/Linkubator.Web/Logging/RequestLoggingDiagnosticContext.cs); check de integración pendiente. |
| T03 | Añadir pruebas de integración HTTP con valores sintéticos que verifiquen el registro normal, la búsqueda privada y una solicitud con token sintético, inspeccionando mensaje y propiedades del evento. | CA01–CA04; V01–V03 de [plan.md](plan.md#estrategia-de-validación). | T01, T02 | Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"`; pasan aserciones de presencia del evento y ausencia de los valores prohibidos. | bloqueada | La prueba focalizada actual pasa (1/1, 24-X-2026), pero comprueba directamente `RequestLoggingDiagnosticContext.Enrich`; dos intentos de prueba HTTP no recibieron eventos del middleware (`Assert.NotEmpty` falló). No satisface aún V01–V03. |
| T04 | Verificar sensibilidad de las aserciones mediante una perturbación temporal y revisar el cambio integrado. | CA01–CA04; V04 de [plan.md](plan.md#estrategia-de-validación). | T03 | La perturbación controlada hace fallar la aserción correspondiente; tras restaurarla, los tests focalizados pasan y `git diff --check` queda limpio. | pendiente | Aún no ejecutada la perturbación. `git diff --check` pasó el 24-X-2026 junto con la prueba del helper; la revisión de configuración integrada queda pendiente. |
| T05 | Revisar evidencia y registrar la decisión de aceptación o los bloqueos restantes. | CA01–CA04; [Aceptación y cierre](#aceptación-y-cierre). | T04 | La revisión confirma evidencia satisfactoria para todos los criterios o deja explícitos los criterios sin demostrar; sin decisión humana no se marca aceptada. | bloqueada | Aceptación humana registrada por DLG el 24-X-2026 («Completado»), pero el cierre técnico sigue bloqueado hasta evidenciar los eventos reales del middleware y completar CA01–CA04. |
| T06 | Sincronizar la trazabilidad de implementación de S7.8 en el roadmap y en el índice tras iniciar o completar el trabajo, sin declarar cerrada la etapa S7. | Proceso SDD, [roadmap.md → «Now»](../../context/roadmap.md#now). | T04 | Los enlaces y el estado reflejan únicamente la situación y evidencia reales de esta spec; `git diff --check` no reporta errores. | en curso | Inicio sincronizado en roadmap e índice el 24-X-2026; la implementación permanece en curso y la etapa S7 no se declara completada. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01–T03 | V01 de [plan.md](plan.md#estrategia-de-validación) | Parcial: el helper se prueba y el middleware está conectado en `Program.cs`, pero no se capturó un evento real de request logging en el host de pruebas. | 24-X-2026; `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~RequestLogging_OmitsPrivateQueryStringAndTokenUrls" --nologo`; 1 pasada; `git diff --check` limpio. | [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs); intentos HTTP fallidos por colección de eventos vacía, no evidencia satisfactoria. |
| CA02 | T02–T03 | V02 de [plan.md](plan.md#estrategia-de-validación) | Parcial: aserciones del helper acreditan query vacía en ruta de listado; no se ha demostrado la propiedad en un evento emitido por Serilog. | 24-X-2026; misma prueba focalizada, 1 pasada. | [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs); prueba directa del helper solamente. |
| CA03 | T02–T03 | V03 de [plan.md](plan.md#estrategia-de-validación) | Parcial: aserciones del helper acreditan reemplazo de ruta y query vacía; no se ha demostrado el evento real del middleware. | 24-X-2026; misma prueba focalizada, 1 pasada. | [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs); prueba directa del helper solamente. |
| CA04 | T02–T04 | V03–V04 de [plan.md](plan.md#estrategia-de-validación) | No demostrada: no se inspeccionaron eventos reales para todas las categorías sensibles ni se ejecutó la perturbación negativa. | 24-X-2026; implementación del helper revisada y prueba focalizada, 1 pasada. | [RequestLoggingDiagnosticContext.cs](../../src/Linkubator.Web/Logging/RequestLoggingDiagnosticContext.cs); validación insuficiente para cerrar CA04. |

## Aceptación y cierre

- Estado del trabajo: implementación en curso; cierre técnico pendiente.
- Criterios sin evidencia satisfactoria: CA01, CA02, CA03 y CA04; la prueba actual solo cubre directamente el helper y no la emisión real del middleware.
- Bloqueos y riesgos restantes: conectar un sink de captura funcional al host Serilog de `WebApplicationFactory`; ejecutar V01–V04 y revisar que la configuración no altere los destinos globales.
- Aceptación humana: DLG, 24-X-2026 («Completado»), según instrucción explícita. La aceptación queda registrada, pero no convierte la evidencia parcial en cierre técnico.
- Archivado: no solicitado.
- Ubicación archivada: no aplica.
- Fecha efectiva de archivado y comprobación de enlaces: no aplica.
- Siguiente paso autorizado: resolver la captura del logger del host de pruebas y completar las validaciones V01–V04; después, actualizar este estado con evidencia verificable antes de declarar completada la implementación.
