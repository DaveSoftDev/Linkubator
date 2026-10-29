# Tareas: S7.8 — Registro de peticiones Web

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 24-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 24-X-2026 («Siguiente»).
  - Tareas: DLG, 24-X-2026 («Vamos»).
- Implementación:
  - Estado: completada; comenzó el 24-X-2026 tras la instrucción «Inicia la implementación».
  - Finalización: 28-X-2026, tras completar la revisión integrada; CA01–CA04 tienen evidencia satisfactoria y la comprobación negativa se restauró correctamente.
  - Evidencia consolidada: pruebas focalizadas de registro 4/4 el 28-X-2026, revisión técnica y `git diff --check` satisfactorios en el worktree real; detalle en [«Revalidación de cierre»](#revalidación-de-cierre). Se conserva la evidencia histórica del 27-X-2026.
- Aceptación humana: DLG, 24-X-2026 («Completado»), registrada según la instrucción explícita. No se declara cerrado el trabajo mientras falte evidencia técnica satisfactoria de CA01–CA04.

## Descomposición

La comprobación del middleware debe observar eventos reales emitidos por el host de pruebas. No es preciso que S7.9 ni las rutas funcionales futuras existan para validar las garantías de registro propias de esta especificación.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Preparar captura aislada de eventos Serilog en el host de `WebApplicationFactory`, sin alterar los destinos globales de la aplicación. | CA01; componentes y contratos de [plan.md](plan.md#diseño-de-implementación). | Ninguna | Compilar el proyecto de tests y comprobar que el sink recibe eventos del host de prueba sin afectar a otros tests. | completada | Logger aislado asignado a `RequestLoggingOptions.Logger`; captura del evento real `GET /app/enlaces`, estado 200. Test focalizado 1/1 y `WebStartupTests` 18/18, 27-X-2026. |
| T02 | Configurar el registro de peticiones en el pipeline Web para omitir la query de rutas privadas y no emitir URL completa en las rutas con tokens. | CA01–CA04; flujos y garantías de [plan.md](plan.md#flujos-internos). | T01 | Revisar el pipeline y ejecutar tests focalizados: se registra la petición con información operativa no sensible, sin query privada ni URL tokenizada. | completada | Las queries de `/app` se omiten; `GetMessageTemplateProperties` aplica la ruta saneada al `RequestPath` real del middleware. Validado con eventos HTTP reales y las pruebas CA02–CA04. |
| T03 | Añadir pruebas de integración HTTP con valores sintéticos que verifiquen el registro normal, la búsqueda privada y una solicitud con token sintético, inspeccionando mensaje y propiedades del evento. | CA01–CA04; V01–V03 de [plan.md](plan.md#estrategia-de-validación). | T01, T02 | Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"`; pasan aserciones de presencia del evento y ausencia de los valores prohibidos. | completada | Pruebas HTTP reales para petición normal, búsqueda privada, paginación, token en query y token en ruta; las aserciones inspeccionan mensaje y propiedades sin guardar marcadores sensibles en la evidencia. |
| T04 | Verificar sensibilidad de las aserciones mediante una perturbación temporal y revisar el cambio integrado. | CA01–CA04; V04 de [plan.md](plan.md#estrategia-de-validación). | T03 | La perturbación controlada hace fallar la aserción correspondiente; tras restaurarla, los tests focalizados pasan y `git diff --check` queda limpio. | completada | Perturbación y restauración verificadas el 27-X-2026. El 28-X-2026: pruebas de registro 4/4, revisión del pipeline sin perturbaciones residuales ni cambios de destinos y `git diff --check` satisfactorio en el worktree real; véase [«Revalidación de cierre»](#revalidación-de-cierre). |
| T05 | Revisar evidencia y registrar la decisión de aceptación o los bloqueos restantes. | CA01–CA04; [Aceptación y cierre](#aceptación-y-cierre). | T04 | La revisión confirma evidencia satisfactoria para todos los criterios o deja explícitos los criterios sin demostrar; sin decisión humana no se marca aceptada. | completada | CA01–CA04 revalidados; aceptación previa de DLG («Completado») y orden de proceder al cierre y archivado el 28-X-2026 («Pues procede»), en respuesta a la lista de pasos para archivar S7.8. |
| T06 | Sincronizar la trazabilidad de implementación de S7.8 en el roadmap y en el índice tras iniciar o completar el trabajo, sin declarar cerrada la etapa S7. | Proceso SDD, [roadmap.md → «Now»](../../../context/roadmap.md#now). | T04 | Los enlaces y el estado reflejan únicamente la situación y evidencia reales de esta spec; `git diff --check` no reporta errores. | completada | Roadmap, índice y checklist del plan sincronizados con el cierre y la evidencia de S7.8 el 28-X-2026; S7 permanece pausada y sin cerrar. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01–T03 | V01 de [plan.md](plan.md#estrategia-de-validación) | Satisfactoria: un evento HTTP real conserva método, ruta y estado operativos no sensibles. | 27-X-2026; SDK 10.0.402, `net10.0`; prueba focalizada y `WebStartupTests` 18/18. | [WebStartupTests.cs](../../../tests/Linkubator.Tests/Integration/WebStartupTests.cs), `RequestLoggingMiddleware_EmitsEventToIsolatedTestSink`. |
| CA02 | T02–T03 | V02 de [plan.md](plan.md#estrategia-de-validación) | Satisfactoria: el evento de búsqueda no contiene query ni texto buscado; una query privada de paginación también se omite. | 27-X-2026; prueba focalizada 4/4 y `WebStartupTests` 18/18. | [WebStartupTests.cs](../../../tests/Linkubator.Tests/Integration/WebStartupTests.cs), `RequestLoggingMiddleware_OmitsPrivateQueryStringAndSearchTextFromEvents`. |
| CA03 | T02–T03 | V03 de [plan.md](plan.md#estrategia-de-validación) | Satisfactoria: eventos HTTP con token sintético en query y ruta no contienen el token ni la URL completa. | 27-X-2026; prueba focalizada 4/4 y `WebStartupTests` 18/18. | [WebStartupTests.cs](../../../tests/Linkubator.Tests/Integration/WebStartupTests.cs), `RequestLoggingMiddleware_RedactsTokenUrlAndSensitiveValuesFromEvent`. |
| CA04 | T02–T04 | V03–V04 de [plan.md](plan.md#estrategia-de-validación) | Satisfactoria: no aparecen texto de búsqueda, contraseña, token, hash innecesario ni credencial sintéticos en los eventos inspeccionados; la prueba negativa detectó la regresión de CA02. | 27-X-2026; prueba focalizada 4/4, perturbación temporal fallida como se esperaba y suite `WebStartupTests` 18/18 tras restaurar. | [WebStartupTests.cs](../../../tests/Linkubator.Tests/Integration/WebStartupTests.cs); [RequestLoggingDiagnosticContext.cs](../../../src/Linkubator.Web/Logging/RequestLoggingDiagnosticContext.cs). |

## Revalidación de cierre

Comprobaciones del 28-X-2026 en el worktree `C:\Repos\Linkubator\FEATURE\S11-web-5`, con SDK 10.0.401, `net10.0` y VSTest:

- `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~WebStartupTests.RequestLogging" --logger "console;verbosity=normal"`: 4/4 satisfactorias; revalida CA01–CA04 sobre el código actual, incluidos los eventos reales del middleware.
- `git diff --check` y `git diff --cached --check`: satisfactorios; el bloqueo de acceso a Git ya no se reproduce.
- Revisión de [RequestLoggingDiagnosticContext.cs](../../../src/Linkubator.Web/Logging/RequestLoggingDiagnosticContext.cs), [Program.cs](../../../src/Linkubator.Web/Program.cs), [appsettings.json](../../../src/Linkubator.Web/appsettings.json) y las pruebas de registro: se conserva el saneamiento de rutas y queries, el middleware y los destinos existentes; no se observan perturbaciones residuales. Se mantiene la evidencia de sensibilidad negativa del 27-X-2026, sin repetir modificaciones temporales.
- Comprobación ampliada `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~WebStartupTests" --logger "console;verbosity=normal"`: 94/95 satisfactorias. Falla únicamente `FaviconIco_ReturnsFiveResolutionFrames` en [WebStartupTests.cs](../../../tests/Linkubator.Tests/Integration/WebStartupTests.cs): espera cinco resoluciones y encuentra dos. Es una incidencia ajena al registro de peticiones; no se modifica ni se presenta la suite completa como satisfactoria.
- Revisión histórica adicional `git show --check --format=fuller 5a172cee13fc30ebce776c6c7b632a94aaca1fbd`: detecta espacios finales en [Dashboard.cshtml.cs](../../../src/Linkubator.Web/Pages/App/Dashboard.cshtml.cs), línea 66 del commit histórico compartido con S7.9 y S7.10. No afecta al diff actual ni al alcance de S7.8; no se reescribe el historial ni se corrige código ajeno.

## Aceptación y cierre

- Estado del trabajo: cerrado el 28-X-2026; implementación y T01–T06 completadas.
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA04 revalidados con pruebas focalizadas satisfactorias.
- Bloqueos del alcance: ninguno. Las incidencias ajenas observadas constan en [«Revalidación de cierre»](#revalidación-de-cierre) y no se declaran resueltas.
- Aceptación humana: DLG, 24-X-2026 («Completado»), según el registro previo; confirmación y autorización de cierre y archivado el 28-X-2026 mediante «Pues procede», en respuesta a «Que hay que hacer para poder archivar S7.8?» y al listado de pasos de esta conversación.
- Archivado: archivado, tras comprobar el traslado completo y los enlaces.
- Ubicación archivada: `specs/archive/s7.8-registro-peticiones-web/`.
- Fecha efectiva de archivado y comprobación de enlaces: 28-X-2026. Verificados los tres documentos en destino, la ausencia de la carpeta activa, 45 enlaces relativos con sus anclas sin errores y las referencias entrantes de roadmap, plan e índice; no quedan referencias a la ruta activa anterior. `git diff --check` satisfactorio.
- Este cierre corresponde solo a S7.8; S7 permanece pausada y sin cerrar.
