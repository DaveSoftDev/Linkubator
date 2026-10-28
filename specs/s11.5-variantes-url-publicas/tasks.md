# Tareas: S11.5 — Variantes de URL y respuestas 404 públicas

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 27-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 27-X-2026 con la referencia «Vamos».
- Implementación:
  - Estado: en curso.
  - Inicio: DLG, 27-X-2026, referencia «Inicia la implementación».
  - Finalización: pendiente; registrar fecha, comprobación y referencia real al completar las tareas de implementación y validación.
  - Evidencia consolidada: pendiente; enlazar las tareas y validaciones que sustenten el estado.
- La preparación de estas tareas no inventa resultados; la evidencia registrada corresponde a las comprobaciones ejecutadas el 27-X-2026.

## Descomposición

La validación integrada de la ruta pública de usuario debe esperar a que S11.2 esté cerrado, como indica el [plan técnico](plan.md#persistencia-y-dependencias). Esta dependencia no adelanta ni incorpora entregables de S11.2 a S11.5.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Tras aprobar estas tareas e iniciar el trabajo, actualizar aquí el estado de implementación y sincronizar el roadmap y el índice con el inicio real de S11.5. | Seguimiento SDD; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | Ninguna | Confirmar la aprobación y el inicio reales; registrar fecha y referencia, y reflejar S11.5 como iniciada sin modificar el estado de otras etapas. | completada | DLG autorizó el inicio el 27-X-2026 con la referencia «Inicia la implementación»; estado actualizado aquí, en el roadmap y en el índice. |
| T02 | Extender las páginas públicas y las pruebas HTTP para que las variantes de rutas existentes redirijan a su forma canónica, manteniendo el resultado de paginación y la query string. Aplicar la redirección solo después de confirmar que el recurso público existe. | CA01, CA02; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos), V01 y V02 | T01 | Ejecutar las comprobaciones V01 y V02 con el cliente sin seguimiento automático; las variantes de recursos existentes redirigen localmente, y la página resultante conserva la paginación y su canonical. | completada | V01–V02 satisfactorios; `WebStartupTests` pasó 7/7 casos focalizados de redirección y paginación. |
| T03 | Ampliar las pruebas HTTP de alias antiguos, slugs antiguos y colecciones públicas sin enlaces, comprobando que responden `404` sin redirección ni exposición de contenido. Corregir únicamente discrepancias dentro del alcance si las pruebas las revelan. | CA03, CA04; [plan.md → «Garantías técnicas»](plan.md#garantías-técnicas), V03 | T02 | Ejecutar V03; las rutas obsoletas y la colección pública vacía responden `404`, no incluyen `Location` y no exponen contenido. | completada | V03 satisfactorio; `WebStartupTests` pasó 2/2 casos focalizados de alias/slug antiguo y colección vacía, sin redirección ni contenido expuesto. |
| T04 | Ejecutar la validación integrada de CA01–CA04 y compilar Web si se modificaron páginas o componentes de producción; registrar resultados y límites reales. | CA01–CA04; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), [«Garantías técnicas»](plan.md#garantías-técnicas), V01–V03 | T03; cierre de S11.2 | Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"` y, si hubo cambios de producción, `dotnet build src/Linkubator.Web/Linkubator.Web.csproj`; todas las comprobaciones aplicables pasan. | bloqueada | Las comprobaciones V01–V03 focalizadas pasaron 9/9 y el build Release terminó durante `dotnet test`; no se ejecuta la validación integrada hasta que S11.2 se complete. |
| T05 | Consolidar la evidencia por criterio y actualizar el estado de implementación, el roadmap y el índice conforme al resultado técnico, sin registrar aceptación humana todavía. | CA01–CA04; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T04 | Comprobar que cada CA enlaza validación y resultado real, y que roadmap e índice reflejan el estado técnico sin declarar aceptación. | pendiente | No ejecutada |
| T06 | Revisar la evidencia y registrar la decisión humana de aceptación o las correcciones requeridas. | CA01–CA04; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T05 | Revisar V01–V03 y la evidencia por criterio; registrar aceptación humana explícita o el bloqueo y su siguiente acción. | pendiente | Sin aceptación |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T04 | V01 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio en las variantes focalizadas: seis rutas redirigen a su forma canónica. La validación integrada sigue pendiente de T04. | 27-X-2026; .NET SDK 10.0.402, `net10.0`, Release | `WebStartupTests` 9/9 focalizadas; [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs) |
| CA02 | T02, T04 | V02 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: la variante conserva `p=2`, sirve la segunda página y emite su canonical normalizada. La validación integrada sigue pendiente de T04. | 27-X-2026; .NET SDK 10.0.402, `net10.0`, Release | `WebStartupTests` 9/9 focalizadas; [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs) |
| CA03 | T03, T04 | V03 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: alias antiguo y slug antiguo responden directamente `404` sin `Location`; la respuesta no expone enlaces públicos de la colección renombrada. La validación integrada sigue pendiente de T04. | 27-X-2026; .NET SDK 10.0.402, `net10.0`, Release | `WebStartupTests` 9/9 focalizadas; [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs) |
| CA04 | T03, T04 | V03 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio en el caso focalizado: colección pública vacía responde `404` sin `Location` ni su nombre en la respuesta. La validación integrada sigue pendiente de T04. | 27-X-2026; .NET SDK 10.0.402, `net10.0`, Release | `WebStartupTests` 9/9 focalizadas; [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs) |

## Aceptación y cierre

- Estado del trabajo: bloqueado.
- Criterios sin validación integrada: CA01–CA04; las comprobaciones focalizadas V01–V03 son satisfactorias.
- Bloqueos y riesgos restantes: T04 debe esperar al cierre de S11.2; T02 y T03 están completadas y la implementación está en curso.
- Aceptación humana: pendiente; responsable, fecha y referencia cuando exista.
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso: completar T02 y T03, registrar sus checks inmediatos y mantener T04 bloqueada hasta el cierre de S11.2.
