# Plan técnico: S12.2 — Pruebas de eventos y ausencia de datos prohibidos en logs

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 31-X-2026 con la referencia «Siguiente».
  - Plan: aprobado por DLG el 31-X-2026 con la referencia «Siguiente».
  - Tareas: [tasks.md](tasks.md), aprobadas por DLG el 31-X-2026 con la referencia «Vamos».

## Diseño de implementación

### Componentes y contratos

- El proyecto existente `tests/Linkubator.Tests/` contiene pruebas de logging de Application en [LoggingTests.cs](../../tests/Linkubator.Tests/Application/LoggingTests.cs). Se reconciliarán sus aserciones con CA02–CA04 antes de atribuir cobertura; se ampliará solo donde falte una comprobación aprobada.
- La configuración y el middleware de Serilog viven en `Linkubator.Web`: [Program.cs](../../src/Linkubator.Web/Program.cs), [appsettings.json](../../src/Linkubator.Web/appsettings.json) y `RequestLoggingDiagnosticContext`. Las pruebas de integración reutilizarán el host de pruebas existente y observarán eventos emitidos por la aplicación, no una copia independiente de la configuración.
- Para CA01 se preparará una salida de logging aislada para la prueba, incluyendo consola y archivo rotativo. Los archivos de prueba residirán en un directorio temporal; la prueba verificará la escritura efectiva sin depender de logs previos ni dejar archivos en el repositorio.
- Los eventos estructurados y las excepciones se inspeccionarán como `LogEvent`, incluyendo mensaje renderizado, propiedades y excepción. Para las peticiones HTTP se observará el evento de request logging generado por el middleware y su contexto diagnóstico.

### Flujos internos

1. Reconciliar las pruebas existentes de eventos y el registro actual de las operaciones MVP0 con las reglas propietarias y las filas A15–A18 de la matriz.
2. Ejecutar operaciones representativas mediante los servicios y el host Web existente; capturar eventos completos y distinguir los datos permitidos por el uso de los que la regla prohíbe.
3. Comprobar la salida real por consola y el archivo rotativo con una configuración temporal de prueba equivalente a la de la aplicación.
4. Añadir o reforzar únicamente las aserciones que falten para CA01–CA04. Incluir un control negativo aislado para confirmar que la comprobación de CA03 detecta un valor prohibido introducido deliberadamente.
5. Ejecutar los tests focalizados y la suite de `Linkubator.Tests`; registrar los resultados actuales y actualizar la matriz de checklist con la evidencia de A15–A18.

### Persistencia y dependencias

- No hay cambios en persistencia de aplicación, esquema, migraciones ni dependencias de software.
- Se reutilizan xUnit, `WebApplicationFactory`, Serilog y el `CapturingSerilogSink` existentes en el proyecto de tests.
- Las pruebas de sinks usarán rutas temporales y limpiarán solo los archivos que ellas creen. Ninguna prueba escribirá logs de validación en `src/Linkubator.Web/logs/`.
- No se depende de rutas de cuenta, transporte de tokens ni scraping automático; quedan fuera del MVP0 o pendientes de definición según las fuentes enlazadas en [spec.md → «Alcance»](spec.md#alcance).

### Garantías técnicas

- Las comprobaciones de privacidad inspeccionan mensaje renderizado, propiedades estructuradas, excepciones y eventos de request logging; no se limitan a un sink ni a la plantilla visible.
- Los valores introducidos por el usuario se evalúan según su uso y las reglas de [specifications.md → «Registro de eventos»](../../context/specifications.md#registro-de-eventos), no por contener palabras o secuencias que parezcan secretos.
- La evidencia distingue que el pipeline haya emitido eventos de que cada evento esté cubierto por aserciones pertinentes; un resultado verde global no demuestra por sí solo esa relación.
- El control negativo de CA04 debe demostrar que el verificador falla ante la infracción controlada, sin alterar configuración de producción ni persistir el valor de prueba.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Reutilizar el proyecto y los mecanismos de captura de logging ya existentes. | Confirmado | Implementación actual en `tests/Linkubator.Tests/` | Evita introducir un framework de pruebas paralelo; se comprobará que cada aserción cubra el criterio concreto. |
| Sobrescribir solo las rutas de salida en pruebas de sinks y usar un directorio temporal. | Propuesta | CA01 y [architecture.md → «Logs»](../../context/architecture.md#logs) | Aísla la ejecución y evita contaminar los logs locales; ajustar la prueba al mecanismo de configuración real del host. |
| La salida de consola puede ser compartida entre pruebas y producir interferencias si se captura globalmente. | Pendiente de comprobación | Diseño actual del host de pruebas | Preferir un sink capturable asociado al host; si no es posible, aislar la prueba y su recurso global con el mecanismo ya usado por el proyecto. |
| Los contenidos introducidos por el usuario pueden incluir datos con apariencia de secreto y estar permitidos en el log. | Confirmado | [specifications.md → «Registro de eventos»](../../context/specifications.md#registro-de-eventos) | Las pruebas no deben exigir un filtrado general ni declarar incumplimiento por la apariencia del valor. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada, integración de la configuración efectiva de Serilog | Host Web de pruebas disponible; directorio temporal aislado | Prueba de integración que inicia el host, emite una petición y comprueba la salida capturada por consola y el archivo rotativo configurado | Se observa el evento emitido por la aplicación en ambas salidas; el archivo se escribe bajo la política rotativa configurada y no se usan rutas de salida reales del entorno | Nombre completo del test, salida inspeccionada, ubicación temporal, configuración efectiva y resultado |
| V02 | CA02 | Revisión técnica y pruebas automatizadas de servicios | Inventario de eventos y tests actuales reconciliado con las reglas | Revisar las aserciones de [LoggingTests.cs](../../tests/Linkubator.Tests/Application/LoggingTests.cs) y ejecutar los casos que comprueban eventos, datos permitidos y excepciones de operaciones MVP0 | Cada comportamiento aplicable tiene una prueba cuyo evento y aserción se identifican; huecos quedan cubiertos o bloqueados si exigen cambiar una regla | Tabla A17 actualizada, tests y aserciones concretas, resultado y fecha |
| V03 | CA03 | Automatizada, integración de eventos y peticiones | Captura estructurada disponible en el host de pruebas | Ejecutar operaciones con sentinelas de prueba y examinar mensaje, propiedades, excepción y request log; contrastar cada valor con su clasificación en las reglas | No hay valores prohibidos en las superficies cubiertas y el contenido de usuario permitido no se rechaza solo por su apariencia | Tests completos, sentinelas, superficies inspeccionadas, tabla A18 actualizada y resultado |
| V04 | CA04 | Automatizada, control negativo de la aserción de seguridad | Verificador de ausencia de valores prohibidos implementado | Introducir un valor prohibido solo en un evento de prueba aislado y comprobar que el verificador lo detecta; descartar el registro temporal al terminar | El control falla de forma intencional al detectar el sentinel y no altera el comportamiento ni los logs de producción | Test de control negativo, superficie inyectada y resultado esperado/observado |
| V05 | CA01–CA04 | Automatizada, regresión del proyecto de tests | V01–V04 completadas | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-restore --filter "FullyQualifiedName~LoggingTests|FullyQualifiedName~WebStartupTests|FullyQualifiedName~LoggingSinksIntegrationTests" --logger "trx;LogFileName=s12.2-focused.trx" --results-directory tests\Linkubator.Tests\TestResults\s12.2-20261031 --verbosity minimal`; después `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-restore --logger "trx;LogFileName=s12.2-full.trx" --results-directory tests\Linkubator.Tests\TestResults\s12.2-20261031 --verbosity minimal` | Pasan los tests focalizados y la suite completa, sin omisiones atribuibles a los filtros o al runner | Comandos, salida completa resumida, conteos, fecha y resultado de cada ejecución |

V05 acredita ejecución y regresión, no que las aserciones sean adecuadas; V02 y V03 revisan esa correspondencia. El control de CA04 debe probar la sensibilidad de la comprobación de privacidad, no basta con que una prueba de compilación o una ejecución normal pase.

## Orden de ejecución y cierre

- [Tareas](tasks.md) — aprobadas por DLG el 31-X-2026 con la referencia «Vamos».
- Las tareas asignarán evidencia actual a CA01–CA04 y cubrirán V01–V05 sin depender de trabajos futuros.
- El cierre requiere evidencia satisfactoria para todos los criterios, tareas propias completadas y aceptación humana registrada en `tasks.md`.
- Ante una discrepancia que exija cambiar una regla, detener la tarea afectada y solicitar la aprobación correspondiente en su documento propietario.
