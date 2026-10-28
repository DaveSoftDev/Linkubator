# Tareas: S7.8 — Registro de peticiones Web

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 24-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 24-X-2026 («Siguiente»).
  - Tareas: DLG, 24-X-2026 («Vamos»).
- Implementación:
  - Estado: en curso; comenzó el 24-X-2026 tras la instrucción «Inicia la implementación».
  - Finalización: pendiente de revisión integrada; CA01–CA04 tienen evidencia satisfactoria y la comprobación negativa se restauró correctamente.
  - Evidencia consolidada: pruebas focalizadas de registro 4/4 y `WebStartupTests` 18/18 el 27-X-2026. `git diff --check` no pudo ejecutarse porque el workspace disponible no es un worktree de Git.
- Aceptación humana: DLG, 24-X-2026 («Completado»), registrada según la instrucción explícita. No se declara cerrado el trabajo mientras falte evidencia técnica satisfactoria de CA01–CA04.

## Descomposición

La comprobación del middleware debe observar eventos reales emitidos por el host de pruebas. No es preciso que S7.9 ni las rutas funcionales futuras existan para validar las garantías de registro propias de esta especificación.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Preparar captura aislada de eventos Serilog en el host de `WebApplicationFactory`, sin alterar los destinos globales de la aplicación. | CA01; componentes y contratos de [plan.md](plan.md#diseño-de-implementación). | Ninguna | Compilar el proyecto de tests y comprobar que el sink recibe eventos del host de prueba sin afectar a otros tests. | completada | Logger aislado asignado a `RequestLoggingOptions.Logger`; captura del evento real `GET /app/enlaces`, estado 200. Test focalizado 1/1 y `WebStartupTests` 18/18, 27-X-2026. |
| T02 | Configurar el registro de peticiones en el pipeline Web para omitir la query de rutas privadas y no emitir URL completa en las rutas con tokens. | CA01–CA04; flujos y garantías de [plan.md](plan.md#flujos-internos). | T01 | Revisar el pipeline y ejecutar tests focalizados: se registra la petición con información operativa no sensible, sin query privada ni URL tokenizada. | completada | Las queries de `/app` se omiten; `GetMessageTemplateProperties` aplica la ruta saneada al `RequestPath` real del middleware. Validado con eventos HTTP reales y las pruebas CA02–CA04. |
| T03 | Añadir pruebas de integración HTTP con valores sintéticos que verifiquen el registro normal, la búsqueda privada y una solicitud con token sintético, inspeccionando mensaje y propiedades del evento. | CA01–CA04; V01–V03 de [plan.md](plan.md#estrategia-de-validación). | T01, T02 | Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"`; pasan aserciones de presencia del evento y ausencia de los valores prohibidos. | completada | Pruebas HTTP reales para petición normal, búsqueda privada, paginación, token en query y token en ruta; las aserciones inspeccionan mensaje y propiedades sin guardar marcadores sensibles en la evidencia. |
| T04 | Verificar sensibilidad de las aserciones mediante una perturbación temporal y revisar el cambio integrado. | CA01–CA04; V04 de [plan.md](plan.md#estrategia-de-validación). | T03 | La perturbación controlada hace fallar la aserción correspondiente; tras restaurarla, los tests focalizados pasan y `git diff --check` queda limpio. | en curso | Desactivar temporalmente la supresión hizo fallar CA02 al observar la query en el evento; tras restaurar la implementación, los checks focalizados pasan 4/4. `git diff --check` no está disponible porque la carpeta de trabajo no es un worktree Git. |
| T05 | Revisar evidencia y registrar la decisión de aceptación o los bloqueos restantes. | CA01–CA04; [Aceptación y cierre](#aceptación-y-cierre). | T04 | La revisión confirma evidencia satisfactoria para todos los criterios o deja explícitos los criterios sin demostrar; sin decisión humana no se marca aceptada. | bloqueada | La aceptación humana está registrada, y CA01–CA04 pasan; falta cerrar T04 con la revisión de diff-check en un worktree Git. |
| T06 | Sincronizar la trazabilidad de implementación de S7.8 en el roadmap y en el índice tras iniciar o completar el trabajo, sin declarar cerrada la etapa S7. | Proceso SDD, [roadmap.md → «Now»](../../context/roadmap.md#now). | T04 | Los enlaces y el estado reflejan únicamente la situación y evidencia reales de esta spec; `git diff --check` no reporta errores. | en curso | Roadmap e índice sincronizados con la evidencia CA01–CA04; S7 permanece activa. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01–T03 | V01 de [plan.md](plan.md#estrategia-de-validación) | Satisfactoria: un evento HTTP real conserva método, ruta y estado operativos no sensibles. | 27-X-2026; SDK 10.0.402, `net10.0`; prueba focalizada y `WebStartupTests` 18/18. | [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs), `RequestLoggingMiddleware_EmitsEventToIsolatedTestSink`. |
| CA02 | T02–T03 | V02 de [plan.md](plan.md#estrategia-de-validación) | Satisfactoria: el evento de búsqueda no contiene query ni texto buscado; una query privada de paginación también se omite. | 27-X-2026; prueba focalizada 4/4 y `WebStartupTests` 18/18. | [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs), `RequestLoggingMiddleware_OmitsPrivateQueryStringAndSearchTextFromEvents`. |
| CA03 | T02–T03 | V03 de [plan.md](plan.md#estrategia-de-validación) | Satisfactoria: eventos HTTP con token sintético en query y ruta no contienen el token ni la URL completa. | 27-X-2026; prueba focalizada 4/4 y `WebStartupTests` 18/18. | [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs), `RequestLoggingMiddleware_RedactsTokenUrlAndSensitiveValuesFromEvent`. |
| CA04 | T02–T04 | V03–V04 de [plan.md](plan.md#estrategia-de-validación) | Satisfactoria: no aparecen texto de búsqueda, contraseña, token, hash innecesario ni credencial sintéticos en los eventos inspeccionados; la prueba negativa detectó la regresión de CA02. | 27-X-2026; prueba focalizada 4/4, perturbación temporal fallida como se esperaba y suite `WebStartupTests` 18/18 tras restaurar. | [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs); [RequestLoggingDiagnosticContext.cs](../../src/Linkubator.Web/Logging/RequestLoggingDiagnosticContext.cs). |

## Aceptación y cierre

- Estado del trabajo: implementación validada; cierre técnico pendiente de completar T04 y la revisión final.
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA04 tienen pruebas HTTP reales satisfactorias.
- Bloqueos y riesgos restantes: `git diff --check` no pudo ejecutarse porque el workspace disponible no es un worktree de Git. La comprobación negativa sí se ejecutó y la implementación quedó restaurada.
- Aceptación humana: DLG, 24-X-2026 («Completado»), según instrucción explícita. El archivado no se ha solicitado.
- Archivado: no solicitado.
- Ubicación archivada: no aplica.
- Fecha efectiva de archivado y comprobación de enlaces: no aplica.
- Siguiente paso autorizado: ejecutar `git diff --check` desde el worktree real y finalizar T04–T06; no se archivó esta spec.
