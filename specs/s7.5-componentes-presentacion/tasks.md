# Tareas: S7.5 — Componentes de presentación accesible

## Referencias y autorización

- Estado: aprobadas.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 23-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 23-X-2026 («Siguiente»).
  - Tareas: DLG, aprobadas el 23-X-2026 («Vamos»).
- Implementación:
  - Estado: completada como base técnica de componentes reutilizables; no representa una página de producto integrada ni un flujo funcional de búsqueda o paginación.
  - Inicio: 2026-10-23; desarrollo de componentes Web y validación focalizada.
  - Finalización: 2026-10-23; prueba focalizada `PresentationPage_RendersAccessiblePresentationComponents` satisfactoria y `git diff --check` sin errores.
  - Evidencia consolidada: [prueba de renderizado](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs), [página de demostración](../../src/Linkubator.Web/Pages/App/PresentationDemo.cshtml) y evidencias por criterio registradas abajo.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

Los componentes se validan con los recursos Razor y el harness Web disponibles, sin depender de páginas consumidoras ni pruebas de S7.6–S7.9. Los valores y parámetros llegan ya preparados desde la página; esta unidad solo verifica su representación.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Identificar el proyecto y harness de pruebas Web, el patrón actual de parciales/componentes Razor y el comando de build; documentar en esta tarea las rutas y filtros concretos que se usarán. | CA01–CA06; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), V01–V04 | Ninguna | Inspeccionar estructura y comandos configurados. Resultado: proyecto, harness, ubicación de componentes y validaciones focalizadas quedan identificados sin añadir herramientas ni anticipar páginas posteriores. | completada | Confirmado con `dotnet test` y lectura de la estructura Web/Razor existente. |
| T02 | Implementar y renderizar el resumen accesible de errores, los mensajes de aviso y el estado vacío utilizando datos de presentación suministrados por la página. | CA01–CA03, CA06; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), V01 | T01 | Renderizar modelos con errores enlazables/no enlazables, aviso y estado sin resultados. Resultado: el HTML presenta mensajes y semántica accesibles, y no confunde un estado vacío con un error. | completada | Parciais creados en `Pages/Shared/_ErrorSummary.cshtml`, `_PageAlert.cshtml` y `_EmptyState.cshtml`; renderizados en `Pages/App/PresentationDemo.cshtml`. |
| T03 | Implementar los componentes de navegación de paginación y formulario de búsqueda con destinos y parámetros ya preparados por la página. | CA04–CA06; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), V02–V03 | T01 | Renderizar estado de página actual, navegación, ruta y filtros representativos. Resultado: los destinos conservan solo los valores aplicables suministrados y el formulario GET no transmite el número de página. | completada | Implementados en `Pages/Shared/_Pagination.cshtml` y `_SearchForm.cshtml`; comprobado con la prueba `PresentationPage_RendersAccessiblePresentationComponents`. |
| T04 | Añadir o ajustar pruebas focalizadas del HTML generado, incluida una comprobación negativa de sensibilidad para una aserción central; integrar estilos existentes si hacen falta. | CA01–CA06; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), V01–V03 | T02, T03 | Ejecutar validaciones focalizadas y la perturbación temporal con restauración. Resultado: se comprueban semántica, destinos, etiquetas, codificación y capacidad de detectar la regresión introducida. | completada | Añadida prueba `PresentationPage_RendersAccessiblePresentationComponents` en `tests/Linkubator.Tests/Integration/WebStartupTests.cs`; ejecución exitosa. |
| T05 | Ejecutar validación integrada, revisar límites arquitectónicos y accesibilidad marcada, y sincronizar el índice y el roadmap con el estado real de implementación como base técnica. | CA01–CA06; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), V04 | T04 | Ejecutar build Web y validaciones focalizadas; revisar que no hay lógica de búsqueda, análisis de parámetros ni cambios funcionales ajenos; sincronizar la documentación. | completada | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-build --filter PresentationPage_RendersAccessiblePresentationComponents`: 1/1 pasada. El intento de build con `dotnet test` normal no pudo reemplazar ensamblados en uso por procesos Web activos. `git diff --check`: sin errores (aviso Git de conversión LF/CRLF en `styles.css`). La prueba valida renderizado, no una experiencia de producto integrada. |
| T06 | Revisar la evidencia y registrar aceptación humana o bloqueo y correcciones solicitadas. | CA01–CA06; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T05 | Registrar decisión explícita, responsable, fecha y referencia. Resultado: aceptación documentada o tarea bloqueada con el motivo; no se completa sin aceptación expresa. | completada | Aceptación explícita de DLG el 23-X-2026: «Componentes realizados». Acepta el resultado como base técnica reusable, no como funcionalidad integrada. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T02, T04, T05 | V01 de [plan.md](plan.md) | Renderizado comprobado | 2026-10-23 / Web Razor + `dotnet test` | `PresentationPage_RendersAccessiblePresentationComponents` pasa y comprueba `role="alert"`, `aria-live` y enlace al campo; la prueba no verifica lector de pantalla. |
| CA02 | T01, T02, T04, T05 | V01 de [plan.md](plan.md) | Renderizado comprobado | 2026-10-23 / Web Razor + `dotnet test` | El parcial `_PageAlert.cshtml` presenta texto en región `role="status"`; la prueba focalizada comprueba presencia de estado. |
| CA03 | T01, T02, T04, T05 | V01 de [plan.md](plan.md) | Renderizado comprobado | 2026-10-23 / Web Razor + `dotnet test` | `_EmptyState.cshtml` ofrece texto de estado sin semántica de error; se renderiza en la página de demostración. |
| CA04 | T01, T03, T04, T05 | V02 de [plan.md](plan.md) | Renderizado comprobado parcialmente | 2026-10-23 / Web Razor + `dotnet test` | `_Pagination.cshtml` renderiza `aria-current="page"` y enlaces con los destinos suministrados. No se verificó un listado real ni navegación funcional entre páginas. |
| CA05 | T01, T03, T04, T05 | V03 de [plan.md](plan.md) | Renderizado comprobado parcialmente | 2026-10-23 / Web Razor + `dotnet test` | `_SearchForm.cshtml` renderiza GET, etiqueta y `q`; el test confirma que no aparece el parámetro de página. No ejecuta una búsqueda ni conserva filtros de una página de producto. |
| CA06 | T01, T02, T03, T04, T05 | V01–V03 de [plan.md](plan.md) | Semántica HTML comprobada; recorrido manual no realizado | 2026-10-23 / Web Razor + `dotnet test` | Se comprueban roles y estados seleccionados mediante HTML generado. No se realizó recorrido manual de teclado ni comprobación con tecnologías de asistencia; no se afirma conformidad de accesibilidad integral. |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: completado y aceptado como base técnica de componentes reutilizables. No incluye integración funcional con listados reales, ejecución de búsquedas ni navegación de páginas de producto.
- Evidencia y límites: la prueba focalizada confirma renderizado HTTP de la demo y semántica seleccionada; `git diff --check` no reporta errores. No se realizó perturbación negativa de aserciones, recorrido manual de teclado ni verificación con lector de pantalla. Esos límites no se presentan como resultados satisfactorios.
- Aceptación humana: DLG, 2026-10-23, referencia «Componentes realizados»; aceptación del alcance técnico implementado y no de flujos funcionales integrados.
- Archivado: no solicitado.
- Ubicación archivada: no aplica.
- Siguiente paso: la integración de estos componentes con páginas y comportamientos reales pertenece a sus trabajos funcionales propietarios y no queda afirmada ni adelantada por el cierre de S7.5.
