# Tareas: S5.10 — Revisión manual de la aplicación repetida del script local

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 19-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 19-X-2026 («Vamos»), instrucción explícita en esta conversación.
- Implementación:
  - Estado: completada y aceptada por DLG el 19-X-2026 («Completado»).
  - Inicio: 19-X-2026, instrucción explícita de DLG «Implementa» en esta conversación.
  - Finalización: 19-X-2026; V01 y V02 satisfactorias, revisión de evidencia V03 satisfactoria; ver [«Evidencia por criterio»](#evidencia-por-criterio).
  - Evidencia consolidada: T02–T04 y matriz CA01–CA03.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

La revisión es manual y no requiere cambios de código. T02 y T03 son secuenciales: la segunda ejecución debe usar la misma base aislada de la primera, con valores reconocibles preparados después de registrar el estado inicial.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas como autorización separada de la spec y del plan; comprobar trazabilidad CA01–CA03 con V01–V03. | CA01–CA03; [plan → «Estrategia de validación»](plan.md#estrategia-de-validación) | Plan aprobado | Revisar el desglose, sus checks y el alcance humano; resultado esperado: aprobación explícita de DLG o bloqueo documentado. | completada | DLG aprobó las tareas el 19-X-2026 con «Vamos». |
| T02 | Iniciar la revisión, registrar el estado de implementación y preparar una base SQLite aislada; ejecutar una vez el script local y registrar la cuenta y colecciones resultantes. | CA01–CA02; [plan → «Flujos internos»](plan.md#flujos-internos), V01–V02 | T01; S5.1–S5.6 aceptadas | Comprobar que la base es aislada, que el script se ejecutó y que existe la identidad configurada con perfil completado y al menos una colección; resultado esperado: estado inicial válido y evidencia de referencia para la segunda ejecución. | completada | 19-X-2026: ejecutor temporal .NET 10.0.401 con Microsoft.Data.Sqlite 10.0.0; la migración V004 se aplicó a `C:\Users\Dave\AppData\Local\Temp\Linkubator-S5.10-ManualReview\s510-20261019-1.db` y el SQL incrustado creó una cuenta (`S510 Review User`, `s510-review-user`) y una colección (`Bandeja de entrada`, privada). SQLite 3.53.3. |
| T03 | Preparar valores reconocibles en el perfil completado y una colección existente; aplicar por segunda vez el mismo script con la misma configuración y comparar el resultado. | CA01–CA02; [plan → «Flujos internos»](plan.md#flujos-internos), V01–V02 | T02 | Inspeccionar identidad, conteos y valores tras la segunda ejecución; resultado esperado: sin cuenta ni colecciones duplicadas por la repetición y sin cambios en los valores reconocibles. | completada | 19-X-2026: en la misma base temporal, tras la primera ejecución se cambiaron Name/Alias a `S510 Marker Name`/`s510-marker-alias` y los campos de la colección a `S510 Marker Collection`, `s510-marker-collection`, `S510 marker description`, pública. Tras ejecutar por segunda vez el recurso SQL, siguió habiendo una cuenta y una colección y todos esos marcadores permanecieron sin cambios. SQLite 3.53.3 mediante Microsoft.Data.Sqlite 10.0.0 y .NET SDK 10.0.401. |
| T04 | Consolidar la evidencia de V01–V02, registrar hallazgos/resoluciones y sincronizar el resumen de implementación, roadmap e índice antes de solicitar aceptación. | CA01–CA03; [plan → «Estrategia de validación»](plan.md#estrategia-de-validación), V01–V03 | T03 | Revisar que la evidencia identifica herramienta/versión y la misma base, incluye observaciones pertinentes y no expone credenciales ni hashes completos; resultado esperado: matriz CA01–CA03 completa o bloqueos explícitos y enlaces de estado coherentes. | completada | 19-X-2026: evidencia V01/V02 identifica una base temporal común y Microsoft.Data.Sqlite 10.0.0/.NET SDK 10.0.401/SQLite 3.53.3; no se registraron credenciales ni hashes. El ejecutor temporal, la base y los residuos de compilación se eliminaron. Tareas, índice y roadmap quedaron sincronizados antes de solicitar aceptación. |
| T05 | Revisar la evidencia y solicitar la aceptación humana del resultado de S5.10. | CA01–CA03; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T04 | Registrar la decisión explícita de DLG; resultado esperado: aceptación registrada o rechazo/bloqueo con correcciones concretas. | completada | DLG aceptó y cerró S5.10 para su alcance el 19-X-2026 con «Completado», instrucción explícita en esta conversación. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02–T04 | V01 | Cumplido. Dos ejecuciones del recurso SQL sobre la misma base conservaron una cuenta y una colección. | 19-X-2026; .NET SDK 10.0.401, Microsoft.Data.Sqlite 10.0.0, SQLite 3.53.3 | Salida antes/después de la segunda ejecución en T03; base temporal `s510-20261019-1.db`. |
| CA02 | T02–T04 | V02 | Cumplido. El perfil completado y todos los campos centinela de la colección conservaron sus valores después de repetir el SQL. | 19-X-2026; .NET SDK 10.0.401, Microsoft.Data.Sqlite 10.0.0, SQLite 3.53.3 | Comparación manual de los valores impresos tras preparar centinelas y tras la segunda ejecución, registrada en T03. |
| CA03 | T04–T05 | V03 | Cumplido. La evidencia identifica herramienta y versiones, la misma base y los estados antes/después; no contiene credenciales ni hashes completos. | 19-X-2026; .NET SDK 10.0.401, Microsoft.Data.Sqlite 10.0.0, SQLite 3.53.3 | Revisión de la matriz CA01–CA03 y limpieza documentadas en T04. |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: cerrado y aceptado por DLG el 19-X-2026 («Completado»).
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA03 cuentan con evidencia satisfactoria.
- Bloqueos y riesgos restantes: ninguno conocido; las herramientas y la base concreta se elegirán al ejecutar las tareas aprobadas.
- Aceptación humana: DLG, 19-X-2026 («Completado»), instrucción explícita en esta conversación.
- Archivado: completado el 19-X-2026; traslado y enlaces entrantes/salientes comprobados.
- Ubicación archivada: `specs/archive/s5.10-revision-manual-aplicacion-script-local/`.
- Fecha efectiva de archivado y comprobación de enlaces: 19-X-2026; referencias recalculadas y verificadas.
- Siguiente paso autorizado, sin cerrar otras etapas: S5.10 queda cerrada para su alcance; archivado no solicitado.