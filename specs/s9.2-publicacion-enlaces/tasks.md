# Tareas: S9.2 — Publicación de enlaces en colecciones privadas

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 26-X-2026 («Siguiente»), instrucción explícita en esta conversación.
  - Plan: [plan.md](plan.md), aprobado por DLG el 26-X-2026 («Siguiente»), instrucción explícita en esta conversación.
  - Tareas: aprobadas por DLG el 26-X-2026 («Vamos»), instrucción explícita en esta conversación.
- Implementación:
  - Estado: completada.
  - Inicio: 26-X-2026, por instrucción de DLG («Vamos» en esta conversación).
  - Finalización: 26-X-2026; tests focalizados 58/58, build de solución sin advertencias ni errores y validación manual del formulario.
  - Evidencia consolidada: [«Evidencia por criterio»](#evidencia-por-criterio) y [«Aceptación y cierre»](#aceptación-y-cierre).
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T01 implementa el rechazo Web y el transporte del aviso; T02 comprueba el POST manipulado después de ese cambio. T03 verifica de forma independiente el comportamiento dinámico del formulario. T04 integra las comprobaciones focalizadas y T05 registra revisión y aceptación. La presentación accesible del aviso en la ruta destino queda como comprobación posterior de S10.1, no como dependencia para cerrar estas tareas.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Ajustar el tratamiento Web del rechazo de privacidad durante la creación: conservar el aviso en TempData y redirigir a `/app/enlaces`, sin cambiar el contrato de Application ni la guarda existente. | CA03; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos). | Ninguna | Build Web satisfactorio; al recibir el error de privacidad, el handler prepara la redirección al listado. | completada | `New.cshtml.cs` establece el aviso TempData y redirige. La build de la solución y los tests focalizados compilaron Web satisfactoriamente el 26-X-2026. |
| T02 | Añadir o ajustar el test HTTP de creación manipulada con `Input.IsPublic=true` y colección privada propia. | CA03, CA04; [plan.md → V03 y V04](plan.md#estrategia-de-validación). | T01 | El POST se rechaza, responde con la redirección prescrita, transporta el aviso y no inserta ningún enlace; los casos de propiedad previos continúan pasando. | completada | `LinkCreationIntegrationTests` pasó 58/58 el 26-X-2026; el caso comprueba redirección, cookie TempData y ausencia de enlace. |
| T03 | Verificar en navegador los cambios de colección pública, privada y selección vacía en el formulario de creación. | CA01, CA02; [plan.md → V02](plan.md#estrategia-de-validación), [«Garantías técnicas»](plan.md#garantías-técnicas). | Ninguna | La opción se habilita solo para colección pública, no se marca automáticamente y se desmarca y explica al quedar privada o vacía. | completada | El navegador integrado verificó los cuatro estados y el cambio de pública marcada a privada; el formulario no se envió y no se modificó. La versión del navegador no se expuso. |
| T04 | Ejecutar validación focalizada y revisar la evidencia y enlaces documentales de S9.2. | CA01–CA04; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación). | T02, T03 | La suite de integración focalizada pasa, la build de solución termina sin errores y los documentos y enlaces locales resuelven. | completada | Tests focalizados 58/58; build de solución con 0 advertencias y 0 errores; comprobación documental satisfactoria el 26-X-2026. |
| T05 | Revisar resultados con DLG, registrar aceptación o correcciones y sincronizar el estado y la evidencia de cierre en roadmap e índice. | CA01–CA04; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre). | T04 | La aceptación humana queda registrada con responsable, fecha y referencia real, o se anotan el rechazo y las correcciones necesarias; roadmap e índice no anticipan el cierre. | completada | DLG aceptó y cerró S9.2 el 26-X-2026 («Completado»), instrucción explícita en esta conversación; roadmap e índice sincronizados. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T03, T04 | V01 y V02 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Satisfactorio: el control inicia deshabilitado y desmarcado; al seleccionar una colección pública se habilita y permanece desmarcado; al cambiar a privada o quitar la selección se deshabilita, desmarca y muestra la explicación. | 26-X-2026; navegador integrado, versión no expuesta; test Web en Windows/.NET SDK 10.0.402 | Interacción del formulario en navegador y `LinkCreationIntegrationTests` 58/58. |
| CA02 | T03, T04 | V01 y V02 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Satisfactorio: la colección pública habilita la opción sin marcarla automáticamente. | 26-X-2026; navegador integrado, versión no expuesta; test Web en Windows/.NET SDK 10.0.402 | Interacción del formulario en navegador y `LinkCreationIntegrationTests` 58/58. |
| CA03 | T01, T02, T04 | V03 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Satisfactorio en S9.2: el POST manipulado no crea el enlace, redirige a `/app/enlaces` y transporta el aviso. La presentación accesible en destino queda como seguimiento de S10.1. | 26-X-2026; Windows, .NET SDK 10.0.402, SQLite en memoria | `PostNewLink_WithPublicRequestInPrivateCollection_RedirectsAndDoesNotCreateLink`; `LinkCreationIntegrationTests` 58/58. |
| CA04 | T02, T04 | V04 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Satisfactorio: los escenarios existentes de colección inválida, inexistente o ajena continúan rechazados sin alta ni divulgación. | 26-X-2026; Windows, .NET SDK 10.0.402, SQLite en memoria | Regresión de propiedad en `LinkCreationIntegrationTests`; suite focalizada 58/58. |

La presentación accesible del aviso en la página destino se comprobará en S10.1 según [mvp0-plan.md → S10.1](../../plans/mvp0-plan.md#s10-listado-y-búsqueda-de-enlaces-l). Es un seguimiento de integración posterior, no un criterio ni una dependencia de cierre de S9.2.

## Aceptación y cierre

- Estado del trabajo: aceptado.
- Criterios sin evidencia satisfactoria en el alcance S9.2: ninguno.
- Bloqueos y riesgos restantes: ninguno conocido. La comprobación de presentación accesible del aviso en el destino queda anotada para S10.1 y no bloquea S9.2.
- Aceptación humana: DLG, 26-X-2026 («Completado»), instrucción explícita en esta conversación.
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso: S9.2 queda cerrada; S10.1 verificará por separado la presentación accesible del aviso transferido.
