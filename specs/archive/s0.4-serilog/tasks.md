# Tareas: S0.4 — Serilog y observabilidad base

## Referencias y autorización

- [Especificación](spec.md): aprobada por DLG el 07-X-2026 mediante la instrucción «Cumple expectativas» del usuario DLG.
- [Plan técnico y validaciones](plan.md): aprobado por DLG el 07-X-2026 con la misma instrucción.
- Las aprobaciones documentales no sustituyen evidencias de ejecución; las comprobaciones de hoy describen el estado actual y no la fecha histórica de implementación.

## Descomposición

Estados: pendiente, en curso, completada o bloqueada. No mostrar contenido privado de logs en evidencia o conversaciones.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Aprobar la [especificación](spec.md) y el [plan](plan.md). | [CA-01 a CA-04](spec.md#criterios-de-aceptación); [diseño](plan.md#diseño-de-implementación) | Ninguna | Aprobación explícita de DLG registrada. | completada | DLG, 07-X-2026: «Cumple expectativas» para spec y plan. |
| T02 | Integrar Serilog en el host y mantener el registro a través de `ILogger`. | [CA-01](spec.md#criterios-de-aceptación); [V-01](plan.md#estrategia-de-validación) | T01 | Revisar composición del host y configuración cargada. | completada | 07-X-2026: `Program.cs` usa `UseSerilog`, `ReadFrom.Configuration` y `ReadFrom.Services`. |
| T03 | Configurar las salidas y los niveles por entorno. | [CA-02](spec.md#criterios-de-aceptación); [V-02](plan.md#estrategia-de-validación) | T01 | Revisar ambos archivos `appsettings`. | completada | 07-X-2026: consola y archivo en `logs/linkubator-.log`, rotación diaria y nivel mínimo configurado; Development define también nivel y salidas. |
| T04 | Revisar logs de ejecución y descartar contenido sensible. | [CA-03](spec.md#criterios-de-aceptación); [V-03](plan.md#estrategia-de-validación) | T02, T03 | Revisar mensajes de forma que no se revele su contenido y comprobar archivos locales ignorados. | completada | 07-X-2026: un archivo local de log, el más reciente examinado automáticamente con patrones de secretos; cero coincidencias. Configuración revisada sin valores de secreto; `logs/` aparece en `.gitignore`. No se reproduce el contenido. |
| T05 | Compilar y probar la solución integrada. | [CA-04](spec.md#criterios-de-aceptación); [V-04](plan.md#estrategia-de-validación) | T02, T03, T04 | Build sin warnings/errores y pruebas descubiertas correctas. | completada | V-04, 07-X-2026: `dotnet build Linkubator.sln --nologo --verbosity minimal` finalizó con 0 advertencias y 0 errores; `dotnet test Linkubator.sln --no-build --no-restore --logger 'console;verbosity=minimal'` descubrió y pasó 3/3 tests. |
| T06 | Revisar el resultado y cerrar el trabajo. | [Criterios](spec.md#criterios-de-aceptación); [puerta de salida](plan.md#orden-de-ejecución-y-puerta-de-salida) | T05 | Aceptación de DLG y archivo solo después de verificar todas las rutas y enlaces. | completada | DLG, 07-X-2026: instrucción «Convierte la s0.4 al nuevo sistema, aprueba cada documento, pasa los tests de las tareas ... y finalmente archivas la tarea» con aprobación «Cumple expectativas»; build correcto y 3/3 tests superados. |

T02, T03 y T04 se comprobaron sobre el estado actual del repositorio, sin afirmar cuándo se implementaron. T05 es la validación integrada; si falla, bloquear T06 y no archivar.

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA-01 | T02, T05 | [V-01](plan.md#estrategia-de-validación) | Conforme: Serilog está conectado al host por `UseSerilog` y configuration. | 07-X-2026; .NET SDK 10.0.401, Windows. | `src/Linkubator.Web/Program.cs`. |
| CA-02 | T03, T05 | [V-02](plan.md#estrategia-de-validación) | Conforme: consola, archivo rotativo y niveles configurados externamente. | 07-X-2026; revisión de configuración. | `src/Linkubator.Web/appsettings.json` y configuración Development. |
| CA-03 | T04 | [V-03](plan.md#estrategia-de-validación) | Conforme con alcance observado: cero coincidencias sensibles en el archivo más reciente y configuración sin secretos. | 07-X-2026; revisión local. | Revisión automatizada sin imprimir contenido; `.gitignore` cubre `logs/`. |
| CA-04 | T05 | [V-04](plan.md#estrategia-de-validación) | Conforme: build sin advertencias/errores y suite con 3/3 tests superados. | 07-X-2026; .NET SDK 10.0.401, Windows. | Comandos y resultados registrados en T05. |

## Aceptación y cierre

- Estado del trabajo: resultado aceptado por DLG y archivado el 07-X-2026.
- Criterios sin evidencia satisfactoria: ninguno para el alcance documentado.
- Bloqueos y riesgos: la revisión de contenido sensible fue acotada al archivo de log más reciente disponible; no se inspeccionó contenido de logs en claro. No representa una auditoría histórica de todos los mensajes emitidos.
- Aceptación del resultado: DLG, 07-X-2026; instrucción de archivar S0.4 tras aprobar los documentos y pasar las comprobaciones.
- Archivado: completado el 07-X-2026; los tres artefactos están en `specs/archive/s0.4-serilog/` y la carpeta activa ya no existe.
- Comprobación de enlaces: 07-X-2026; 8 enlaces entrantes y 49 enlaces/anclas relacionados resueltos, sin referencias a la ruta anterior.
- Siguiente paso: continuar con el siguiente trabajo del plan; este cierre no acepta ni archiva trabajos vecinos.