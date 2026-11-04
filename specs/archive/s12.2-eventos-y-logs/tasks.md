# Tareas: S12.2 — Pruebas de eventos y ausencia de datos prohibidos en logs

## Referencias y autorización

- Estado: aprobadas.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 31-X-2026 con la referencia «Siguiente».
  - Plan: [plan.md](plan.md), aprobado por DLG el 31-X-2026 con la referencia «Siguiente».
  - Tareas: aprobadas por DLG el 31-X-2026 con la referencia «Vamos».
- Implementación:
  - Estado: completada.
  - Inicio: 31-X-2026, solicitud de DLG «sigo viendo las tareas como pendientes».
  - Finalización: 31-X-2026; tests focalizados 150/150 y suite completa 1.851/1.851, sin omitidos.
  - Evidencia consolidada: matriz A15–A18 y [TRX focalizado](../../../tests/Linkubator.Tests/TestResults/s12.2-20261031/s12.2-focused.trx), [TRX completo](../../../tests/Linkubator.Tests/TestResults/s12.2-20261031/s12.2-full.trx).

## Descomposición

Las tareas son secuenciales: la reconciliación determina qué cobertura falta; sus resultados guían los cambios de tests y la validación integrada evita atribuir evidencia antes de ejecutar la suite y actualizar la matriz.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Reconciliar los logs actuales del MVP0, los eventos y las aserciones existentes con CA01–CA03 y las filas A15–A18; entregar un mapa de escenario, test, aserción y hueco. | CA01–CA03; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos) | Ninguna | Revisar el inventario de eventos y los tests existentes. Resultado: cada escenario aplicable tiene cobertura concreta identificada o un hueco registrado, sin atribuir cobertura por el nombre del test. | completada | Reconciliación registrada en A15–A18 de la matriz; revisión de los puntos de emisión actuales de Application y de los logs HTTP. |
| T02 | Añadir o reforzar las pruebas de eventos, datos permitidos y prohibidos en mensajes, propiedades, excepciones y request logs, incluido el control negativo. No cambiar reglas funcionales. | CA02–CA04; [plan.md → «Garantías técnicas»](plan.md#garantías-técnicas), [«Estrategia de validación»](plan.md#estrategia-de-validación) | T01 | Ejecutar los tests afectados de logging. Resultado: las aserciones pertinentes pasan y el control negativo detecta el sentinel; cualquier discrepancia que requiera cambiar una regla queda bloqueada. | completada | Se añadió `LoggingEvidenceAssertion_WhenForbiddenValueIsEmitted_DetectsTheViolation`; los tests focalizados pasaron 150/150. |
| T03 | Comprobar la emisión efectiva por consola y archivo rotativo con salidas de prueba aisladas y temporales. | CA01; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Estrategia de validación»](plan.md#estrategia-de-validación) | T02 | Ejecutar la prueba de integración de sinks. Resultado: la aplicación escribe en ambas salidas y la prueba no usa ni deja archivos en las rutas locales de producción. | completada | `WebApplication_EmitsRequestLogsToConsoleAndRollingFile` observó el evento en consola y archivo diario dentro del directorio temporal; pasó en la suite focalizada y completa. |
| T04 | Ejecutar validación focalizada y regresión completa; actualizar las filas A15–A18 de la matriz con los tests concretos, resultados y evidencia. | CA01–CA04; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | T03 | Ejecutar `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-restore --filter "FullyQualifiedName~LoggingTests|FullyQualifiedName~WebStartupTests|FullyQualifiedName~LoggingSinksIntegrationTests" --verbosity minimal` y después `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-restore --verbosity minimal`. Resultado: pasan ambas ejecuciones y la matriz refleja evidencia actual, no solo el resultado global. | completada | 150/150 focalizados y 1.851/1.851 suite completa; se actualizaron A15–A18 con tests y TRX. |
| T05 | Revisar cobertura criterio → prueba → resultado, consolidar la evidencia en estas tareas y sincronizar el estado de S12.2 en el roadmap sin declarar aceptación humana. | CA01–CA04; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T04 | Recorrer la evidencia por criterio y comprobar los enlaces actualizados del roadmap y la matriz. Resultado: no quedan criterios sin evidencia, preguntas de regla no resueltas ni estados presentados como completados sin ejecución. | completada | Criterios, matriz, TRX y estado técnico del roadmap reconciliados el 31-X-2026; aceptación humana sigue pendiente. |
| T06 | Revisar el resultado y registrar aceptación o los cambios solicitados por DLG. | CA01–CA04; [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación) | T05 | Registrar la decisión humana explícita y su referencia. Resultado: solo una aceptación explícita permite marcar el trabajo aceptado; si se solicitan cambios, registrar el bloqueo y las tareas necesarias. | completada | Aceptada por DLG el 31-X-2026 con la referencia «Acepto S12.2». |

## Evidencia por criterio

La línea base previa, ejecutada antes de T01–T03, pasó 148/148 tests focalizados y 1.849/1.849 de la suite completa; no acredita CA01–CA04. La validación posterior a los cambios se ejecutó el 31-X-2026 con .NET SDK 10.0.403: 150/150 tests focalizados y 1.851/1.851 de la suite completa, sin omitidos.

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T03–T05 | V01, V05 en [plan.md](plan.md#estrategia-de-validación) | Cumplido técnicamente | 31-X-2026; .NET 10.0.403 | [Test de sinks](../../../tests/Linkubator.Tests/Integration/LoggingSinksIntegrationTests.cs), [TRX focalizado](../../../tests/Linkubator.Tests/TestResults/s12.2-20261031/s12.2-focused.trx), A15–A16 de [mvp0-checklist-test-matrix.md](../../../plans/mvp0-checklist-test-matrix.md) |
| CA02 | T01–T02, T04–T05 | V02, V05 en [plan.md](plan.md#estrategia-de-validación) | Cumplido técnicamente | 31-X-2026; .NET 10.0.403 | [LoggingTests.cs](../../../tests/Linkubator.Tests/Application/LoggingTests.cs), [WebStartupTests.cs](../../../tests/Linkubator.Tests/Integration/WebStartupTests.cs), [TRX focalizado](../../../tests/Linkubator.Tests/TestResults/s12.2-20261031/s12.2-focused.trx), A17 de [mvp0-checklist-test-matrix.md](../../../plans/mvp0-checklist-test-matrix.md) |
| CA03 | T01–T02, T04–T05 | V03, V05 en [plan.md](plan.md#estrategia-de-validación) | Cumplido técnicamente, dentro de los flujos actuales del MVP0 | 31-X-2026; .NET 10.0.403 | [LoggingTests.cs](../../../tests/Linkubator.Tests/Application/LoggingTests.cs), [WebStartupTests.cs](../../../tests/Linkubator.Tests/Integration/WebStartupTests.cs), revisión de productores en A18 de [mvp0-checklist-test-matrix.md](../../../plans/mvp0-checklist-test-matrix.md), [TRX focalizado](../../../tests/Linkubator.Tests/TestResults/s12.2-20261031/s12.2-focused.trx) |
| CA04 | T02, T04–T05 | V04, V05 en [plan.md](plan.md#estrategia-de-validación) | Cumplido técnicamente | 31-X-2026; .NET 10.0.403 | `LoggingEvidenceAssertion_WhenForbiddenValueIsEmitted_DetectsTheViolation` en [LoggingTests.cs](../../../tests/Linkubator.Tests/Application/LoggingTests.cs), [TRX focalizado](../../../tests/Linkubator.Tests/TestResults/s12.2-20261031/s12.2-focused.trx) |

## Aceptación y cierre

- Estado del trabajo: aceptado.
- Criterios sin evidencia satisfactoria: ninguno en la implementación actual del MVP0.
- Bloqueos y riesgos restantes: no se probaron flujos de cuenta fuera de alcance del MVP0 ni la eliminación por vencimiento de la retención del archivo.
- Aceptación humana: aceptada por DLG el 31-X-2026 con la referencia «Acepto S12.2».
- Archivado: completado por @DaveSoftDev el 04-XI-2026 («Archiva las specs activas»).
- Ubicación archivada: `specs/archive/s12.2-eventos-y-logs/`.
- Fecha efectiva de archivado y comprobación de enlaces: 04-XI-2026; carpeta, artefactos y enlaces entrantes/salientes comprobados.
- Siguiente paso autorizado, sin cerrar otras etapas: continuar S12 según [mvp0-plan.md → «S12: Cierre y hardening (M)»](../../../plans/mvp0-plan.md#s12-cierre-y-hardening-m).
