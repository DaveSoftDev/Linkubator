# Tareas: S11.4 — Atributos transversales de páginas públicas

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 27-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 27-X-2026 («Vamos»).
- Implementación:
  - Estado: en curso.
  - Inicio: DLG, 27-X-2026, referencia «Inicia la implementación».
  - Finalización: pendiente; registrar fecha, comprobación y referencia real al completar las tareas de implementación y validación.
  - Evidencia consolidada: pendiente; enlazar las tareas y validaciones que sustenten el estado.
- La preparación de estas tareas no registra resultados ni evidencias de implementación.

## Descomposición

La validación de cierre requiere que estén disponibles las cuatro páginas de S11.1–S11.3; S11.2 figura en curso en el [roadmap](../../context/roadmap.md#área-pública-y-seo). Las tareas de verificación y corrección se ejecutan en secuencia sobre las respuestas HTTP, sin depender de S11.5–S11.7.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Tras la aprobación de estas tareas y la autorización explícita para iniciar, actualizar aquí el estado de implementación y sincronizar el roadmap y el índice con el inicio real de S11.4. | Seguimiento SDD; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | Ninguna | Confirmar aprobación y autorización de inicio; registrar fecha y referencia reales y reflejar S11.4 como iniciada, sin alterar el estado de otras etapas. | completada | DLG autorizó el inicio el 27-X-2026 con la referencia «Inicia la implementación»; estado actualizado aquí, en el roadmap y en el índice. |
| T02 | Ampliar las pruebas HTTP de las páginas de usuario y colección para demostrar CA01–CA02, con imágenes con título informado y no informado; corregir únicamente discrepancias de las vistas públicas que muestren las comprobaciones. | CA01, CA02; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Estrategia de validación»](plan.md#estrategia-de-validación), V01 y V02 | T01 | Ejecutar V01 y V02; pasan los casos de atributos de enlaces y texto alternativo para ambos valores de título. | completada | V01–V02 satisfactorios; las comprobaciones focalizadas pasaron 8/8. Se añadieron casos HTTP para `alt` informado, fallback y texto con caracteres de marcado; no se requirieron cambios en las vistas. |
| T03 | Ampliar las pruebas HTTP de las cuatro páginas públicas para demostrar CA03–CA04; corregir solo discrepancias del encabezado o del layout público que detecten las pruebas. | CA03, CA04; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Estrategia de validación»](plan.md#estrategia-de-validación), V03 y V04 | T02 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"`; las cuatro respuestas contienen exactamente un `h1` y declaran el idioma establecido en la fuente propietaria. | completada | V03–V04 satisfactorios; `WebStartupTests` pasó 38/38. Se cubrieron las cuatro rutas y no se requirieron cambios en vistas ni layout. |
| T04 | Ejecutar la validación integrada de CA01–CA04 y compilar el proyecto Web si se modificaron vistas o layout; registrar resultados y límites reales. | CA01–CA04; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), [«Garantías técnicas»](plan.md#garantías-técnicas), V01–V04 | T03 | Ejecutar los filtros V01–V04 del plan y, si hubo cambios de producción, `dotnet build src/Linkubator.Web/Linkubator.Web.csproj`; todas las rutas y criterios aplicables pasan. | bloqueada | Las pruebas focalizadas pasaron 8/8 y `WebStartupTests` 38/38; no se registra el cierre de validación hasta completar S11.2, prerrequisito indicado en el plan aprobado. |
| T05 | Consolidar la evidencia por criterio y actualizar el estado de implementación, el roadmap y el índice conforme al resultado técnico, sin registrar aceptación humana todavía. | CA01–CA04; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T04 | Comprobar que cada CA enlaza su validación y resultado real, y que roadmap e índice reflejan implementación y validación completadas o el bloqueo real, sin declarar aceptación. | pendiente | No ejecutada |
| T06 | Revisar la evidencia y registrar la decisión humana de aceptación o las correcciones requeridas. | CA01–CA04; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T05 | Revisar los resultados de V01–V04 y registrar la aceptación explícita o el bloqueo y su siguiente acción. | pendiente | Sin aceptación |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T04 | V01 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: las respuestas de usuario y colección incluyen los atributos esperados para enlaces externos. | 27-X-2026; .NET SDK 10.0.402, `net10.0` | `WebStartupTests` 38/38 y ejecución focalizada 8/8; [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs) |
| CA02 | T02, T04 | V02 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: se verifican título, fallback a URL y serialización de texto con caracteres de marcado en ambas rutas con imágenes. | 27-X-2026; .NET SDK 10.0.402, `net10.0` | Ejecución focalizada 8/8; [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs) |
| CA03 | T03, T04 | V03 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: cada una de las cuatro páginas responde con un único `h1`. | 27-X-2026; .NET SDK 10.0.402, `net10.0` | `WebStartupTests` 38/38 y ejecución focalizada 8/8; [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs) |
| CA04 | T03, T04 | V04 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: las cuatro respuestas declaran el idioma esperado en el elemento raíz. | 27-X-2026; .NET SDK 10.0.402, `net10.0` | `WebStartupTests` 38/38 y ejecución focalizada 8/8; [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs) |

## Aceptación y cierre

- Estado del trabajo: bloqueado.
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA04 tienen comprobaciones focalizadas satisfactorias.
- Bloqueos y riesgos restantes: el plan aprobado exige completar S11.2 antes de registrar la validación integrada de cierre; S11.2 sigue en curso.
- Aceptación humana: pendiente; responsable, fecha y referencia cuando exista.
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso autorizado, sin cerrar otras etapas: esperar el cierre de S11.2 y después completar T04–T06; no se declara aceptación ni cierre de S11.4.
