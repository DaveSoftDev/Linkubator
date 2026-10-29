# Tareas: S11.5 — Variantes de URL y respuestas 404 públicas

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 27-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 27-X-2026 con la referencia «Vamos».
- Implementación:
  - Estado: completada.
  - Inicio: DLG, 27-X-2026, referencia «Inicia la implementación».
  - Finalización: 28-X-2026, tras completar T04–T05; aceptación humana de T06 registrada en esa fecha.
  - Evidencia consolidada: [«Validación integrada de T04»](#validación-integrada-de-t04) y [«Aceptación y cierre»](#aceptación-y-cierre).
- Se conservan las comprobaciones iniciales del 27-X-2026 y se registra su validación integrada del 28-X-2026.

## Descomposición

El prerrequisito de validación integrada del [plan técnico](plan.md#persistencia-y-dependencias) queda satisfecho por el [cierre de S11.2](../s11.2-pagina-publica-usuario/tasks.md#aceptación-y-cierre) del 28-X-2026. Esta dependencia no adelanta ni incorpora entregables de S11.2 a S11.5.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Tras aprobar estas tareas e iniciar el trabajo, actualizar aquí el estado de implementación y sincronizar el roadmap y el índice con el inicio real de S11.5. | Seguimiento SDD; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | Ninguna | Confirmar la aprobación y el inicio reales; registrar fecha y referencia, y reflejar S11.5 como iniciada sin modificar el estado de otras etapas. | completada | DLG autorizó el inicio el 27-X-2026 con la referencia «Inicia la implementación»; estado actualizado aquí, en el roadmap y en el índice. |
| T02 | Extender las páginas públicas y las pruebas HTTP para que las variantes de rutas existentes redirijan a su forma canónica, manteniendo el resultado de paginación y la query string. Aplicar la redirección solo después de confirmar que el recurso público existe. | CA01, CA02; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos), V01 y V02 | T01 | Ejecutar las comprobaciones V01 y V02 con el cliente sin seguimiento automático; las variantes de recursos existentes redirigen localmente, y la página resultante conserva la paginación y su canonical. | completada | V01–V02 satisfactorios; `WebStartupTests` pasó 7/7 casos focalizados de redirección y paginación. |
| T03 | Ampliar las pruebas HTTP de alias antiguos, slugs antiguos y colecciones públicas sin enlaces, comprobando que responden `404` sin redirección ni exposición de contenido. Corregir únicamente discrepancias dentro del alcance si las pruebas las revelan. | CA03, CA04; [plan.md → «Garantías técnicas»](plan.md#garantías-técnicas), V03 | T02 | Ejecutar V03; las rutas obsoletas y la colección pública vacía responden `404`, no incluyen `Location` y no exponen contenido. | completada | V03 satisfactorio; `WebStartupTests` pasó 2/2 casos focalizados de alias/slug antiguo y colección vacía, sin redirección ni contenido expuesto. |
| T04 | Ejecutar la validación integrada de CA01–CA04 y compilar Web si se modificaron páginas o componentes de producción; registrar resultados y límites reales. | CA01–CA04; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), [«Garantías técnicas»](plan.md#garantías-técnicas), V01–V03 | T03; cierre de S11.2 | Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"` y, si hubo cambios de producción, `dotnet build src/Linkubator.Web/Linkubator.Web.csproj`; todas las comprobaciones aplicables pasan. | completada | 28-X-2026: prerrequisito de S11.2 satisfecho; `WebStartupTests` 95/95, incluidos los nueve casos de V01–V03. Compilación Debug realizada por el runner sin restauración y `git diff --check` satisfactorio; detalle en [«Validación integrada de T04»](#validación-integrada-de-t04). |
| T05 | Consolidar la evidencia por criterio y actualizar el estado de implementación, el roadmap y el índice conforme al resultado técnico, sin registrar aceptación humana todavía. | CA01–CA04; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T04 | Comprobar que cada CA enlaza validación y resultado real, y que roadmap e índice reflejan el estado técnico sin declarar aceptación. | completada | 28-X-2026: evidencia CA01–CA04 consolidada con entorno, comando, resultados y límites reales; implementación completada y roadmap e índice sincronizados. La aceptación se registra separadamente en T06. |
| T06 | Revisar la evidencia y registrar la decisión humana de aceptación o las correcciones requeridas. | CA01–CA04; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T05 | Revisar V01–V03 y la evidencia por criterio; registrar aceptación humana explícita o el bloqueo y su siguiente acción. | completada | 28-X-2026: DLG seleccionó «Aceptar como “Completado” y cerrar S11.5, sin archivar» en esta conversación, tras presentar los resultados satisfactorios de T04. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T04 | V01 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: seis variantes redirigen localmente a su ruta canónica. | 28-X-2026; SDK 10.0.401, `net10.0`, Debug, VSTest/xUnit 2 | [«Validación integrada de T04»](#validación-integrada-de-t04), suite 95/95. |
| CA02 | T02, T04 | V02 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: se conserva `p=2`, se sirve la página esperada y se emite su canonical normalizada. | 28-X-2026; SDK 10.0.401, `net10.0`, Debug, VSTest/xUnit 2 | [«Validación integrada de T04»](#validación-integrada-de-t04), suite 95/95. |
| CA03 | T03, T04 | V03 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: alias y slug antiguos responden `404` sin redirección ni exposición del contenido de la colección renombrada. | 28-X-2026; SDK 10.0.401, `net10.0`, Debug, VSTest/xUnit 2 | [«Validación integrada de T04»](#validación-integrada-de-t04), suite 95/95. |
| CA04 | T03, T04 | V03 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: la colección pública vacía responde `404` sin `Location` ni su nombre en la respuesta. | 28-X-2026; SDK 10.0.401, `net10.0`, Debug, VSTest/xUnit 2 | [«Validación integrada de T04»](#validación-integrada-de-t04), suite 95/95. |

## Validación integrada de T04

- Fecha y autorización: 28-X-2026, petición de DLG «Puedes completar T04-T06 de S11.5?» en esta conversación; prerrequisito de S11.2 satisfecho.
- Entorno: Windows, SDK 10.0.401, `net10.0`, Debug, VSTest/xUnit 2. HEAD `5988520536cffa4bcee711ccd9125022310b65fb`, con cambios locales preexistentes de documentación, archivado de S7.8 y favicon. No se modifican ni se revierten esos cambios ajenos.
- Comando ejecutado:

```powershell
dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~WebStartupTests"
```

- Resultado: 95/95 superadas, ninguna fallida ni omitida, código 0. La ejecución compila los proyectos y los recursos Web; no se requiere build separado al no modificar código ni componentes de producción en esta intervención. `git diff --check` también termina con código 0.
- Cobertura de V01–V03 en [WebStartupTests.cs](../../../tests/Linkubator.Tests/Integration/WebStartupTests.cs), nueve casos incluidos en la suite ejecutada, sin una segunda ejecución focalizada:
  - V01 / CA01: seis casos de `PublicPages_WithNonCanonicalPaths_RedirectToCanonicalPaths`, cliente sin seguimiento automático, verifican status y `Location` para mayúsculas y barra final en usuario, colecciones y colección.
  - V02 / CA02: `PublicCollectionPage_WithNonCanonicalPathAndPageQuery_PreservesPageAndCanonical` verifica la query conservada, solicita el destino y comprueba contenido y canonical de la segunda página con otro `Host` permitido.
  - V03 / CA03–CA04: `PublicCollectionPage_ReturnsNotFoundForPrivateMissingAndEmptyCollections` y `UserPage_ReturnsNotFoundForUnknownAndFormerAliases` verifican `404` directos para alias/slug anteriores y colección vacía, con ausencia de `Location` y de contenido cuando corresponde.
- La prueba del favicon pasa en este checkout. El archivo ya presentaba cambios locales al iniciar la validación; no se atribuye su corrección a S11.5 ni se altera la evidencia histórica de S11.4.
- Límites: pruebas de integración HTTP, sin automatización de navegador ni garantías de indexación efectiva. No se instalaron dependencias ni se modificó código en esta tarea.

## Aceptación y cierre

- Estado del trabajo: completado, aceptado y cerrado el 28-X-2026; T01–T06 completadas.
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA04 validados en la suite integrada 95/95.
- Bloqueos y riesgos restantes: ninguno identificado dentro del alcance.
- Aceptación humana: DLG, 28-X-2026, referencia «Completado», mediante la selección explícita «Aceptar como “Completado” y cerrar S11.5, sin archivar» en esta conversación.
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso: archivar solo ante una petición expresa y tras verificar traslado y enlaces. No se cierran otras specs ni la etapa S11.
