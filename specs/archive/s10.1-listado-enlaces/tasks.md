# Tareas: S10.1 — Listado y búsqueda de enlaces

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»), decisión explícita en esta conversación.
  - Plan: [plan.md](plan.md), aprobado por DLG el 27-X-2026 («Siguiente»), decisión explícita en esta conversación.
  - Tareas: DLG, 27-X-2026 («Vamos»), decisión explícita en esta conversación.
- Implementación:
  - Estado: completada.
  - Inicio: DLG, 27-X-2026 («Vamos»), instrucción explícita en esta conversación.
  - Finalización: 27-X-2026; tests HTTP focalizados 153/153 y `git diff --check` satisfactorios.
  - Evidencia consolidada: [Evidencia por criterio](#evidencia-por-criterio) y T01–T03. No se requirieron cambios productivos; la integración preexistente quedó cubierta con pruebas end-to-end.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T01 completa la comprobación integrada del flujo de aviso y el listado con sus casos de CA01–CA03; T02 implementa solo las correcciones que los tests focales demuestren necesarias para esos criterios, conservando los componentes y comportamiento ya disponibles. T03 realiza la validación focalizada y revisa la evidencia local. T04 presenta los resultados para aceptación y sincroniza la trazabilidad sin cerrar la etapa S10.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Añadir o adaptar tests HTTP de listado para verificar resultados propios con búsqueda/filtro, estado vacío y el POST de privacidad rechazado seguido del GET que presenta el aviso de manera accesible. Incluir un caso negativo sin aviso para evitar mensajes espurios. | CA01–CA03; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [V01–V03](plan.md#estrategia-de-validación). | Ninguna | Ejecutar los tests añadidos con el filtro focal adecuado; los escenarios verdes distinguen resultados, vacío, aviso y ausencia de aviso, y no persisten el enlace rechazado. | completada | Se añadieron pruebas de integración end-to-end para el aviso tras redirección y para su ausencia sin TempData; validación combinada 153/153 el 27-X-2026. |
| T02 | Corregir quirúrgicamente la integración de la página/aviso solo si T01 muestra que uno de los criterios no se cumple; reutilizar `_LinkCreationNotice`, `_PageAlert` y los componentes existentes. | CA01–CA03; [plan.md → «Flujos internos»](plan.md#flujos-internos), [«Garantías técnicas»](plan.md#garantías-técnicas). | T01 | Build Web y los tests implicados pasan; el GET destino muestra el aviso transferido en región accesible y conserva resultados/vacío existentes. Si no se necesita código productivo, dejar constancia verificable y pasar al check integrado. | completada | Los tests confirman que el listado ya consume TempData mediante la alerta `role="status"` y `aria-live="polite"`; no fue necesario cambiar código productivo. Suite focalizada 153/153 el 27-X-2026. |
| T03 | Ejecutar validación focalizada y revisar enlaces, diferencias y cobertura criterio → test → evidencia; registrar resultados reales y límites de la inspección HTTP. | CA01–CA03; [plan.md → V01–V04](plan.md#estrategia-de-validación). | T01, T02 | Los tests de listado y creación seleccionados pasan, `git diff --check` queda limpio y no hay enlaces documentales rotos. | completada | 153/153 tests; `git diff --check` satisfactorio el 27-X-2026. Aviso Git de conversión LF→CRLF preexistente en `src/Linkubator.Web/wwwroot/css/styles.css`; sin errores de whitespace. |
| T04 | Revisar resultados con DLG, registrar aceptación o correcciones y sincronizar el estado de S10.1 en roadmap e índice, sin adelantar el cierre de S10. | CA01–CA03; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre). | T03 | Se registra la decisión humana explícita o sus correcciones; índice y roadmap reflejan solo el estado comprobado. | completada | DLG aceptó S10.1 el 27-X-2026 («Completado»), instrucción explícita en esta conversación; estado sincronizado en roadmap e índice. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T03 | V01 y V04 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Satisfactorio: suite de listado y creación focalizada; cubre aislamiento, búsqueda/filtro, orden y paginación en casos existentes. | 27-X-2026; Windows, .NET SDK 10.0.401, SQLite en memoria | `LinkListingIntegrationTests` y `LinkCreationIntegrationTests`; ejecución combinada 153/153. |
| CA02 | T01, T03 | V02 y V04 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Satisfactorio: prueba del estado vacío para colección sin enlaces y regresión focalizada. | 27-X-2026; Windows, .NET SDK 10.0.401, SQLite en memoria | `GetLinks_WithEmptyCollection_ShowsEmptyStateNotOtherCollectionLinks`; suite combinada 153/153. |
| CA03 | T01–T03 | V03 y V04 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Satisfactorio: POST manipulado rechazado, TempData transferido al GET, mensaje en región `role="status"`/`aria-live="polite"` y ningún enlace persistido; sin aviso cuando no existe TempData. | 27-X-2026; Windows, .NET SDK 10.0.401, SQLite en memoria | `PostNewLink_WithPublicRequestInPrivateCollection_ShowsAccessibleErrorOnListing` y `GetLinks_WithoutPrivacyError_DoesNotRenderPrivacyNotice`; suite combinada 153/153. |

La ejecución focalizada acredita el HTML resultante y el flujo de TempData, pero no demuestra anuncio real por un lector de pantalla ni constituye aceptación humana. La suite cubrió 153 pruebas y no se ejecutó la suite completa.

## Aceptación y cierre

- Estado del trabajo: aceptado; S10.1 cerrada. Esto no cierra la etapa S10 ni sus trabajos vecinos.
- Criterios sin evidencia satisfactoria: ninguno dentro del alcance S10.1.
- Bloqueos y riesgos restantes: ninguno conocido para el alcance aceptado. La evidencia HTTP acredita HTML y transporte de TempData; no representa una prueba con lector de pantalla ni una ejecución de la suite completa.
- Aceptación humana: DLG, 27-X-2026 («Completado»), instrucción explícita en esta conversación.
- Archivado: no solicitado.
- Ubicación archivada: no aplica; la carpeta permanece activa.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes, solo si se solicita archivar.
- Siguiente paso: continuar S10 con sus trabajos restantes; S10.1 queda aceptada y no condiciona etapas futuras.
