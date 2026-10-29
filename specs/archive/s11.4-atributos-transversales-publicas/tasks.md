# Tareas: S11.4 — Atributos transversales de páginas públicas

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 27-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 27-X-2026 («Vamos»).
- Implementación:
  - Estado: completada.
  - Inicio: DLG, 27-X-2026, referencia «Inicia la implementación».
  - Finalización: 28-X-2026, tras ejecutar T04 y consolidar T05; aceptación humana de T06 registrada en esa fecha.
  - Evidencia consolidada: [«Validación integrada de T04»](#validación-integrada-de-t04) y [«Aceptación y cierre»](#aceptación-y-cierre).
- La preparación de estas tareas no registra resultados ni evidencias de implementación.

## Descomposición

La validación de cierre requiere que estén disponibles las cuatro páginas de S11.1–S11.3; [S11.2 está aceptada y cerrada](../s11.2-pagina-publica-usuario/tasks.md#aceptación-y-cierre) desde el 28-X-2026 y su prerrequisito queda satisfecho. Las tareas de verificación y corrección se ejecutan en secuencia sobre las respuestas HTTP, sin depender de S11.5–S11.7.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Tras la aprobación de estas tareas y la autorización explícita para iniciar, actualizar aquí el estado de implementación y sincronizar el roadmap y el índice con el inicio real de S11.4. | Seguimiento SDD; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | Ninguna | Confirmar aprobación y autorización de inicio; registrar fecha y referencia reales y reflejar S11.4 como iniciada, sin alterar el estado de otras etapas. | completada | DLG autorizó el inicio el 27-X-2026 con la referencia «Inicia la implementación»; estado actualizado aquí, en el roadmap y en el índice. |
| T02 | Ampliar las pruebas HTTP de las páginas de usuario y colección para demostrar CA01–CA02, con imágenes con título informado y no informado; corregir únicamente discrepancias de las vistas públicas que muestren las comprobaciones. | CA01, CA02; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Estrategia de validación»](plan.md#estrategia-de-validación), V01 y V02 | T01 | Ejecutar V01 y V02; pasan los casos de atributos de enlaces y texto alternativo para ambos valores de título. | completada | V01–V02 satisfactorios; las comprobaciones focalizadas pasaron 8/8. Se añadieron casos HTTP para `alt` informado, fallback y texto con caracteres de marcado; no se requirieron cambios en las vistas. |
| T03 | Ampliar las pruebas HTTP de las cuatro páginas públicas para demostrar CA03–CA04; corregir solo discrepancias del encabezado o del layout público que detecten las pruebas. | CA03, CA04; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Estrategia de validación»](plan.md#estrategia-de-validación), V03 y V04 | T02 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"`; las cuatro respuestas contienen exactamente un `h1` y declaran el idioma establecido en la fuente propietaria. | completada | V03–V04 satisfactorios; `WebStartupTests` pasó 38/38. Se cubrieron las cuatro rutas y no se requirieron cambios en vistas ni layout. |
| T04 | Ejecutar la validación integrada de CA01–CA04 y compilar el proyecto Web si se modificaron vistas o layout; registrar resultados y límites reales. | CA01–CA04; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), [«Garantías técnicas»](plan.md#garantías-técnicas), V01–V04 | T03 | Ejecutar los filtros V01–V04 del plan y, si hubo cambios de producción, `dotnet build src/Linkubator.Web/Linkubator.Web.csproj`; todas las rutas y criterios aplicables pasan. | completada | 28-X-2026: prerrequisito de S11.2 satisfecho; V01–V04 focalizadas 8/8. Suite `WebStartupTests` 94/95: fallo del favicon fuera de CA01–CA04, documentado y aceptado como límite, no como suite verde. Sin cambios productivos ni build separado necesario; detalle en [«Validación integrada de T04»](#validación-integrada-de-t04). |
| T05 | Consolidar la evidencia por criterio y actualizar el estado de implementación, el roadmap y el índice conforme al resultado técnico, sin registrar aceptación humana todavía. | CA01–CA04; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T04 | Comprobar que cada CA enlaza su validación y resultado real, y que roadmap e índice reflejan implementación y validación completadas o el bloqueo real, sin declarar aceptación. | completada | 28-X-2026: evidencia por criterio consolidada con V01–V04, entorno, comandos y límites reales; implementación completada y roadmap e índice sincronizados. La aceptación se registra separadamente en T06. |
| T06 | Revisar la evidencia y registrar la decisión humana de aceptación o las correcciones requeridas. | CA01–CA04; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T05 | Revisar los resultados de V01–V04 y registrar la aceptación explícita o el bloqueo y su siguiente acción. | completada | 28-X-2026: DLG seleccionó «Aceptar como “Completado”: cerrar S11.4 con evidencia 8/8 y dejar registrado el fallo ajeno del favicon, sin archivar» tras presentar los resultados de T04 y sus límites en esta conversación. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T04 | V01 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: usuario y colección incluyen los atributos esperados en enlaces externos. | 28-X-2026; SDK 10.0.401, `net10.0`, VSTest/xUnit 2 | [«Validación integrada de T04»](#validación-integrada-de-t04), selección 8/8. |
| CA02 | T02, T04 | V02 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: título, fallback a URL y caracteres de marcado codificados en el HTML original de ambas rutas con imágenes. | 28-X-2026; SDK 10.0.401, `net10.0`, VSTest/xUnit 2 | [«Validación integrada de T04»](#validación-integrada-de-t04), selección 8/8. |
| CA03 | T03, T04 | V03 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: las cuatro páginas tienen un único `h1`. | 28-X-2026; SDK 10.0.401, `net10.0`, VSTest/xUnit 2 | [«Validación integrada de T04»](#validación-integrada-de-t04), selección 8/8. |
| CA04 | T03, T04 | V04 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: las cuatro páginas declaran el idioma esperado en el elemento raíz. | 28-X-2026; SDK 10.0.401, `net10.0`, VSTest/xUnit 2 | [«Validación integrada de T04»](#validación-integrada-de-t04), selección 8/8. |

## Validación integrada de T04

- Fecha: 28-X-2026; autorización de DLG para desbloquear S11.4 y completar T04–T06 en esta conversación. El cierre de S11.2 satisface el prerrequisito aprobado.
- Checkout: `5988520536cffa4bcee711ccd9125022310b65fb`, con cambios documentales de cierre; sin cambios de código en esta intervención. Entorno: SDK 10.0.401, `net10.0`, VSTest/xUnit 2.
- Ejecución integrada prevista por V03–V04:

```powershell
dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~WebStartupTests"
```

- Resultado: 94/95 superadas, 1 fallida, ninguna omitida, código 1. La ejecución compila los proyectos; no hubo cambios de vistas ni layout que requieran un build separado.
- Incidencia ajena al alcance: `FaviconIco_ReturnsFiveResolutionFrames` en [WebStartupTests.cs](../../../tests/Linkubator.Tests/Integration/WebStartupTests.cs), línea 359, espera `000001000500` y recibe `000001000200` (cinco resoluciones frente a dos). No se corrige como parte de S11.4 ni se presenta la suite como satisfactoria. Queda como seguimiento independiente; su corrección no está ejecutada.
- Para separar el fallo ajeno de los criterios de esta spec se ejecutó una selección conjunta de V01–V04 sobre los binarios recién compilados:

```powershell
dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~WebStartupTests.UserPage_RendersPublicContentMetadataAndConfiguredCanonical|FullyQualifiedName~WebStartupTests.PublicCollectionPage_RendersPublicLinksMetadataAndCanonical|FullyQualifiedName~WebStartupTests.PublicPage_WithImages_RendersAltAsText|FullyQualifiedName~WebStartupTests.PublicPages_WithPublicContent_RenderSharedLibraryPresentation|FullyQualifiedName~WebStartupTests.HomePage_Renders_PublicLandingMetadataAndActions"
```

- Resultado: 8/8 superadas, ninguna fallida ni omitida. V01 cubre los atributos de enlaces de usuario y colección; V02 inspecciona el HTML sin decodificar para título, fallback y marcado codificado; V03–V04 se comprueban con la prueba de landing y los tres casos de presentación pública compartida. No se atribuye cobertura de navegador ni tecnologías de asistencia a estas pruebas HTTP.
- `git diff --check` terminó con código 0. No se instalaron paquetes, no se modificó código y no se ejecutaron pruebas de UI.
- DLG aceptó expresamente el resultado de los cuatro criterios con el límite de la suite registrado, mediante la decisión de T06.

## Aceptación y cierre

- Estado del trabajo: completado, aceptado y cerrado el 28-X-2026; T01–T06 completadas.
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA04 tienen evidencia integrada focalizada 8/8.
- Bloqueos y riesgos restantes: ninguno dentro del alcance. La suite Web completa conserva el fallo del favicon documentado en [«Validación integrada de T04»](#validación-integrada-de-t04), aceptado como seguimiento ajeno al cierre de S11.4.
- Aceptación humana: DLG, 28-X-2026, referencia «Completado», mediante la selección explícita «Aceptar como “Completado”: cerrar S11.4 con evidencia 8/8 y dejar registrado el fallo ajeno del favicon, sin archivar» en esta conversación.
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso: mantener el seguimiento independiente del fallo del favicon; archivar S11.4 solo ante una petición expresa y tras verificar traslado y enlaces. No se cierran otras specs ni la etapa S11.
