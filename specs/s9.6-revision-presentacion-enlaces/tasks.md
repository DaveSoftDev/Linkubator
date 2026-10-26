# Tareas: S9.6 — Revisión de la presentación de enlaces

## Referencias y autorización

- Estado: aprobadas por DLG el 26-X-2026 («Vamos»).
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 26-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 26-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 26-X-2026 («Vamos»).
- Implementación:
  - Estado: completada.
  - Inicio: 26-X-2026; T01 iniciada tras la aprobación explícita de DLG («Vamos»).
  - Finalización: 26-X-2026; revisión y revalidación manual registradas en CA01–CA03.
  - Evidencia consolidada: [CA01–CA03](#evidencia-por-criterio), respaldadas por T01–T05 y V01–V03.
- La ejecución comienza con T01 y no registra como realizadas las comprobaciones todavía pendientes.

## Descomposición

T02 y T03 son revisiones independientes de creación y edición, realizables tras la preparación común de T01. T03 incluye la inspección de errores de imagen solo si se pueden provocar de forma segura con datos propios; omitir ese caso se registra como límite y no impide revisar los estados normales. T04 consolida resultados y sincroniza el estado real del roadmap; T05 presenta la evidencia para aceptación humana.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Confirmar la aplicación Web, la sesión privada, el navegador y la versión; identificar o preparar enlaces propios con campos opcionales informados y vacíos. Registrar el estado inicial sin datos sensibles. | CA01–CA02; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Persistencia y dependencias»](plan.md#persistencia-y-dependencias). | Ninguna | La aplicación y la sesión están disponibles, y hay datos propios adecuados para observar los dos estados; registrar entorno y estado inicial. Si falta una condición, documentar el bloqueo concreto sin modificar datos ajenos. | completada | 26-X-2026: aplicación ejecutada en `https://localhost:58973` con base SQLite temporal aislada, sin usar la base de desarrollo existente. GET de creación respondió 200; se provisionó la identidad de desarrollo `Demo User` y su colección privada. Navegador Chrome 150.0.7871.250 (Electron 43.7.7), Windows 10 x64, viewport 1024×768. Se prepararon dos enlaces propios de prueba, uno con título/descripción informados y otro con los tres campos vacíos. Un error de compilación inicial por ensamblado bloqueado por otro proceso se evitó usando la compilación existente; no se detuvo ni modificó ese proceso. El dato temporal con imagen produjo dos intentos de carga a `example.com`, ambos fallidos con `ERR_BLOCKED_BY_ORB`; se limpió el campo de imagen y no se repitieron. |
| T02 | Revisar en navegador la presentación de título, descripción e imagen en el formulario privado de creación, incluidas etiquetas, opcionalidad y asociación con sus controles. | CA01; [plan.md → V01](plan.md#estrategia-de-validación), [«Flujos internos»](plan.md#flujos-internos). | T01 | Se documenta la observación de cada campo y cualquier hallazgo tiene pasos y contexto suficientes para reproducirlo; no se cambia ni envía el formulario. | completada | 26-X-2026: los tres campos aparecen con su etiqueta y la indicación «Opcional»; el snapshot accesible los expuso como controles con nombre. No se observó un problema de presentación en el formulario de creación. No se envió el formulario durante esta inspección. Evidencia visual observada en el navegador de esta conversación; no se guardó una captura como archivo. |
| T03 | Revisar en navegador título, descripción e imagen en formularios privados de edición con valores informados y vacíos. Inspeccionar un error de imagen solo si se puede probar sin riesgo ni cambios persistentes. | CA02; [plan.md → V02](plan.md#estrategia-de-validación), [«Garantías técnicas»](plan.md#garantías-técnicas). | T01 | Se documenta el estado observado por campo; si se intenta una entrada inválida, se confirma que no se guardaron cambios. Si el caso no es seguro o no está disponible, se anota el motivo y no se presenta como validado. | completada | 26-X-2026: edición muestra los valores propios informados y otro enlace muestra vacíos los tres campos, con sus indicaciones «Opcional». Al enviar `javascript:alert(1)` en Imagen, el resumen y el campo muestran `UrlSchemeNotAllowed`; un GET posterior confirmó que los valores persistidos no cambiaron. Se observó el código técnico en vez de un mensaje para usuario, contrario a la responsabilidad de presentación de Web descrita en [decisions.md](../../context/decisions.md) y al tratamiento de errores de [specifications.md → «Errores controlados»](../../context/specifications.md#errores-controlados). Hallazgo registrado para seguimiento fuera de S9.6; no se modificó código. Después se vació y guardó la imagen del registro de prueba para impedir nuevas solicitudes externas. Evidencia visual observada en el navegador de esta conversación; no se guardó una captura como archivo. |
| T04 | Consolidar evidencia, límites y hallazgos de CA01–CA02; actualizar la referencia S9.6 del roadmap para reflejar el estado real de la revisión. | CA01–CA03; [plan.md → V03](plan.md#estrategia-de-validación), [«Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre). | T02, T03 | La evidencia de cada criterio está consolidada o su límite está explícito; el roadmap no afirma resultados, aceptación ni cambios que no hayan ocurrido. | completada | 26-X-2026: evidencia de CA01–CA02 y el hallazgo de mensaje técnico consolidados en este archivo; roadmap actualizado. No se declara aceptación ni corrección del hallazgo. |
| T05 | Presentar la evidencia consolidada a DLG, registrar la decisión explícita y actualizar el roadmap conforme al resultado aceptado o a los ajustes solicitados. | CA03; [plan.md → V03](plan.md#estrategia-de-validación), [«Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre). | T04 | La decisión queda registrada con responsable, fecha y referencia real. Si se solicitan cambios, S9.6 permanece abierto y las correcciones se tramitan por separado. | completada | DLG confirmó la revalidación manual el 26-X-2026 («El mismo navegador de antes») y aceptó S9.6 con la referencia «Completado». Se registra Chrome 150.0.7871.250 (Electron 43.7.7), Windows 10 x64, viewport 1024×768, según el entorno de T01–T03. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T02, T04 | V01 y V03 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: campos identificables, indicación de opcionalidad visible y nombres accesibles en el formulario de creación. | 26-X-2026; Chrome 150.0.7871.250 (Electron 43.7.7), Windows 10 x64, 1024×768 | T02; observación del navegador en esta conversación |
| CA02 | T01, T03, T04 | V02 y V03 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: valores informados y vacíos revisados; el rechazo no cambió los datos persistidos. DLG revalidó manualmente el mensaje amigable en el mismo navegador; las pruebas de [S9.7](../s9.7-mensajes-validacion-url/tasks.md#evidencia-por-criterio) verifican texto, resumen, campo y persistencia. | 26-X-2026; Chrome 150.0.7871.250 (Electron 43.7.7), Windows 10 x64, 1024×768; tests Web .NET 10.0 para S9.7 | T03 y T05; confirmación de DLG y evidencia de S9.7 |
| CA03 | T04–T05 | V03 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: evidencia consolidada; revalidación manual confirmada y resultado aceptado explícitamente por DLG («Completado»). | 26-X-2026; Chrome 150.0.7871.250 (Electron 43.7.7), Windows 10 x64, 1024×768 | Confirmación y aceptación de DLG en esta conversación; ver T05 |

Un resultado no disponible o fallido seguirá pendiente o bloqueado; los resultados esperados no se registrarán como evidencia real.

## Aceptación y cierre

- Estado del trabajo: completado y aceptado por DLG el 26-X-2026 («Completado»).
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos y riesgos restantes: ninguno identificado.
- Aceptación humana: DLG, 26-X-2026 («Completado»).
- Archivado: no solicitado.
- Ubicación archivada: no aplica.
- Fecha efectiva de archivado y comprobación de enlaces: no aplica.
- Siguiente paso: ninguno; archivado no solicitado.
