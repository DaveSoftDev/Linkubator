# Tareas: S9.7 — Mensaje de validación de URL

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 26-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 26-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 26-X-2026 («Vamos»).
- Implementación:
  - Estado: completada.
  - Inicio: 26-X-2026, tras aprobación de DLG («Vamos»).
  - Finalización: 26-X-2026; pruebas focalizadas de integración 179/179 correctas, sin errores ni omitidas.
  - Evidencia consolidada: [CA01–CA02](#evidencia-por-criterio), respaldadas por T01–T03 y V01–V02.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T01 agrupa el cambio de presentación y sus pruebas de integración estrechamente relacionadas. T02 ejecuta la validación focalizada; T03 consolida la evidencia y sincroniza el roadmap; T04 presenta el resultado para aceptación humana.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Unificar en Web la resolución del error de esquema no permitido para los formularios privados de creación y edición; ampliar las pruebas de integración para URL e imagen. | CA01–CA02; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos). | Ninguna | Las pruebas cubren el resumen y el campo afectado, no exponen el identificador técnico, preservan la entrada y comprueban que un rechazo no crea ni modifica datos. | completada | 26-X-2026: presenter compartido conectado en creación y edición; pruebas añadidas para URL e imagen con esquema rechazado y para un error ajeno al cambio. |
| T02 | Ejecutar las pruebas focalizadas de creación y gestión de enlaces. | CA01–CA02; [plan.md → V01–V02](plan.md#estrategia-de-validación). | T01 | El filtro de integración finaliza correctamente y sus aserciones demuestran el mensaje esperado, la ausencia del código técnico y la conservación de datos. | completada | 26-X-2026: filtro `LinkCreationIntegrationTests|LinkManagementIntegrationTests`, 179/179 correctas, 0 fallidas, 0 omitidas. La salida de compilación se aisló en `%TEMP%` porque un proceso Web activo bloqueaba los ensamblados habituales; no se detuvo ese proceso. Una aserción inicial sobre el caso vacío se acotó al caso de identificador mal formado, que sí sigue el error de Application. |
| T03 | Consolidar resultados, límites y referencias; sincronizar el estado S9.7 del roadmap y el índice. | CA01–CA02; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre). | T02 | La evidencia por criterio refleja comprobaciones reales; el roadmap y el índice indican el estado real sin afirmar aceptación ni cambios no realizados. | completada | 26-X-2026: evidencia de V01–V02 consolidada; [roadmap.md](../../context/roadmap.md) y [specs-index.md](../specs-index.md) sincronizados. |
| T04 | Presentar la evidencia a DLG y registrar la decisión explícita de aceptación o los ajustes solicitados. | CA01–CA02; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre). | T03 | La decisión queda registrada con responsable, fecha y referencia real; una solicitud de ajustes deja el trabajo abierto. | completada | DLG aceptó S9.7 el 26-X-2026 («Aceptar S9.7; mantener S9.6 abierta»). |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01–T03 | V01 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: creación con URL o imagen y edición con imagen rechazan el esquema, muestran el mensaje propietario en resumen y campo y no exponen el código. La creación no guarda un enlace y la edición conserva campos y proyección de búsqueda. | 26-X-2026; tests de integración Web sobre .NET 10.0 | T01–T02; 179/179 |
| CA02 | T01–T03 | V02 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: los casos válidos y otros errores cubiertos conservan el resultado esperado; el error de selección de colección mantiene su mensaje y no presenta el código de esquema. | 26-X-2026; tests de integración Web sobre .NET 10.0 | T01–T02; 179/179 |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: completado y aceptado por DLG el 26-X-2026 («Aceptar S9.7; mantener S9.6 abierta»).
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos y riesgos restantes: ninguno para S9.7. S9.6 también quedó revalidada y aceptada por DLG el 26-X-2026 («Completado»); su evidencia está en [sus tareas](../s9.6-revision-presentacion-enlaces/tasks.md#aceptación-y-cierre).
- Aceptación humana: DLG, 26-X-2026 («Aceptar S9.7; mantener S9.6 abierta»).
- Archivado: no solicitado.
- Ubicación archivada: no aplica.
- Fecha efectiva de archivado y comprobación de enlaces: no aplica.
- Siguiente paso autorizado: ninguno para S9.7; no archivar sin petición expresa.
