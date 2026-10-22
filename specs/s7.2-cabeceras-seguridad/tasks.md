# Tareas: S7.2 — Cabeceras de seguridad web

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 22-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 22-X-2026 («Siguiente»).
  - Tareas: DLG, aprobadas el 22-X-2026 («Vamos»).
- Implementación:
  - Estado: completada.
  - Inicio: DLG autorizó el inicio el 22-X-2026 («Inicia la implementación»).
  - Finalización: 22-X-2026; test focalizado 5/5, comprobación negativa de las tres cabeceras, revisión técnica integrada y `git diff --check` satisfactorios.
  - Evidencia consolidada: [T01–T05](#descomposición) y [evidencia por criterio](#evidencia-por-criterio).
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T02 depende de identificar el proyecto de tests/harness HTTP ya disponible para asegurar que la comprobación ejecuta el pipeline real del host. T03 implementa la configuración en Web y añade la prueba focalizada correspondiente; las perturbaciones de sensibilidad y la validación integrada se realizan después, sin depender de S7.9.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Confirmar el harness existente para alojar el host Web y probar una respuesta HTTP; identificar una ruta estable para la comprobación. | CA01–CA03; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), V01 | Ninguna | Inspeccionar los proyectos y referencias de tests existentes; anotar el proyecto, framework/harness y comando focalizado disponible. Resultado: una ruta concreta para probar la respuesta del pipeline queda identificada, o se documenta un bloqueo real. | completada | Identificados `tests/Linkubator.Tests/Integration/WebStartupTests.cs`, xUnit + `WebApplicationFactory<Program>` y GET `/`; comprobado el 22-X-2026. |
| T02 | Configurar las cabeceras requeridas en el pipeline Web para las respuestas de la aplicación, evitando valores duplicados y sin modificar capas ajenas. | CA01–CA03; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos) y [«Flujos internos»](plan.md#flujos-internos) | T01 | Ejecutar la compilación focalizada de Web. Resultado: compila y no se incorporan paquetes ni cambios a Domain, Application o Infrastructure. | completada | Cabeceras incorporadas al pipeline Web en `src/Linkubator.Web/Program.cs`; compilación correcta dentro del test focalizado del 22-X-2026; sin cambios de paquetes ni de otras capas para S7.2. |
| T03 | Añadir o adaptar un test de integración HTTP focalizado que compruebe en la respuesta las tres cabeceras y sus valores; usar la ruta/harness identificado en T01. | CA01–CA03; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), V01 | T01, T02 | Ejecutar el test nuevo focalizado. Resultado: pasa para una respuesta generada por el host Web. | completada | Añadido `HomePage_IncludesSecurityHeaders` en `tests/Linkubator.Tests/Integration/WebStartupTests.cs`; `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~Linkubator.Tests.Integration.WebStartupTests`: 5/5 pasaron el 22-X-2026. |
| T04 | Verificar la sensibilidad de cada aserción mediante perturbaciones temporales, aisladas y revertidas, de ausencia o valor incorrecto de cada encabezado. | CA01–CA03; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), V02 | T03 | Ejecutar una perturbación por cabecera y comprobar que falla la aserción respectiva; restaurar la configuración y repetir el test. Resultado: se detectan las seis variantes previstas y la prueba restaurada pasa. | completada | El 22-X-2026, cada una de las tres cabeceras se alteró temporalmente y su aserción falló con el valor esperado; tras restaurar, el test focalizado pasó. Se verificó una perturbación de valor incorrecto por cabecera. |
| T05 | Ejecutar revisión técnica integrada, recopilar evidencia de CA01–CA03 y sincronizar el roadmap con el estado real del trabajo. | CA01–CA03; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), V03 y [«Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T04 | Ejecutar los tests focalizados y `git diff --check`; revisar pipeline, duplicados y alcance del diff. Resultado: validaciones correctas, cambios acotados y evidencia real registrada abajo. Actualizar roadmap solo con el estado comprobado, sin declarar aceptación. | completada | Revisión integrada sin hallazgos; test focalizado 5/5, perturbaciones negativas detectadas y `git diff --check` correcto el 22-X-2026. |
| T06 | Revisar la evidencia y registrar aceptación humana o bloqueo y correcciones solicitadas. | CA01–CA03; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T05 | Registrar la decisión explícita, responsable, fecha y referencia. Resultado: aceptación documentada o tarea bloqueada con el motivo; esta tarea no se completa sin aceptación expresa. | completada | DLG aceptó el resultado el 22-X-2026 con «Completado». |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02–T05 | V01–V03 de [plan.md](plan.md) | Satisfactoria: respuesta HTTP incluye `X-Content-Type-Options: nosniff`; la aserción falla si se cambia el valor. | 22-X-2026; test Web con `WebApplicationFactory<Program>` | `HomePage_IncludesSecurityHeaders`; resultado focalizado 5/5. |
| CA02 | T02–T05 | V01–V03 de [plan.md](plan.md) | Satisfactoria: respuesta HTTP incluye `Content-Security-Policy: frame-ancestors 'none'`; la aserción falla si se cambia el valor. | 22-X-2026; test Web con `WebApplicationFactory<Program>` | `HomePage_IncludesSecurityHeaders`; perturbación negativa temporal revertida. |
| CA03 | T02–T05 | V01–V03 de [plan.md](plan.md) | Satisfactoria: respuesta HTTP incluye `Referrer-Policy: strict-origin-when-cross-origin`; la aserción falla si se cambia el valor. | 22-X-2026; test Web con `WebApplicationFactory<Program>` | `HomePage_IncludesSecurityHeaders`; perturbación negativa temporal revertida. |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: aceptado.
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA03 tienen evidencia técnica satisfactoria.
- Bloqueos y riesgos restantes: ninguno identificado.
- Aceptación humana: DLG, 22-X-2026 («Completado»).
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso autorizado, sin cerrar otras etapas: archivar S7.2 solo ante petición explícita de archivado.
