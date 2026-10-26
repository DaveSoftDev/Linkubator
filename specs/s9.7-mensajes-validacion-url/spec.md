# Especificación: S9.7 — Mensaje de validación de URL

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 26-X-2026 («Siguiente»).

## Objetivo

Presentar los errores de esquema de URL en los formularios privados de enlaces con el mensaje definido en [specifications.md → «Errores controlados»](../../context/specifications.md#errores-controlados), sin exponer el identificador técnico.

## Fuentes

- [mvp0-plan.md → «S9: Gestión de enlaces (L)»](../../plans/mvp0-plan.md#s9-gestión-de-enlaces-l): trabajo de corrección solicitado para la etapa.
- [requirements.md → «Enlaces»](../../context/requirements.md#enlaces) y [«Accesibilidad»](../../context/requirements.md#accesibilidad): comportamiento esperado al rechazar una URL y presentación de los errores.
- [specifications.md → «URLs de los enlaces»](../../context/specifications.md#urls-de-los-enlaces), [«Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) y [«Errores controlados»](../../context/specifications.md#errores-controlados): reglas de validación y mensaje exacto propietario.
- [decisions.md → «Tecnología»](../../context/decisions.md#tecnología): responsabilidad de Web para los mensajes que ve la persona usuaria.
- [S9.1 — Crear enlace → «Criterios de aceptación»](../s9.1-crear-enlace/spec.md#criterios-de-aceptación) y [S9.3 — Gestionar enlaces → «Criterios de aceptación»](../s9.3-gestionar-enlaces/spec.md#criterios-de-aceptación): formularios privados de creación y edición.
- [S9.6 — Revisión de la presentación de enlaces → «Evidencia por criterio»](../s9.6-revision-presentacion-enlaces/tasks.md#evidencia-por-criterio): hallazgo que motiva este seguimiento.

## Alcance

- **Incluye:** tratamiento en Web de los errores de esquema no permitido para URL e imagen en los formularios privados de creación y edición; presentación del mensaje propietario en el resumen y junto al campo afectado; comprobación de que el identificador técnico no se expone.
- **Excluye:** cambios en las reglas de validación de URL o imagen; otros errores de validación; formularios o páginas públicas; scraping; cambios de datos o persistencia.
- **Trabajo vecino y responsable de sus entregables:** S9.1 y S9.3 proporcionan los formularios; S9.6 documenta la observación manual original. Esta spec corrige la presentación del mensaje; S9.6 conserva su revisión y aceptación independientes.

## Dependencias y prerrequisitos

- S9.1 y S9.3 tienen implementación y aceptación registradas; los formularios privados están disponibles.
- S9.6 registró el error técnico visible en la edición de imagen; la corrección se tramita aquí, por separado de la revisión manual.
- La regla exacta del mensaje está definida en [specifications.md → «Errores controlados»](../../context/specifications.md#errores-controlados).

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [«Errores controlados»](../../context/specifications.md#errores-controlados), [«Accesibilidad»](../../context/requirements.md#accesibilidad) y [S9.6 → CA02](../s9.6-revision-presentacion-enlaces/spec.md#criterios-de-aceptación) | Un formulario privado de creación o edición recibe una URL de enlace o imagen con un esquema no permitido. | Se procesa la petición inválida. | La operación se rechaza y el resumen y el campo afectado muestran el mensaje propietario; el identificador técnico no aparece en la interfaz. En edición, los datos persistidos permanecen sin cambios. |
| CA02 | [«Errores controlados»](../../context/specifications.md#errores-controlados) y [«URLs de los enlaces»](../../context/specifications.md#urls-de-los-enlaces) | Los formularios gestionan una URL o imagen con esquema permitido, o un error de validación distinto de esquema. | Se procesan las peticiones correspondientes. | Los casos válidos conservan su comportamiento y los errores no incluidos en esta corrección mantienen su presentación existente. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para redactar la especificación. La solución técnica y las comprobaciones concretas se detallarán en el plan. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md): aprobado por DLG el 26-X-2026 («Siguiente»).
- [Tareas, evidencia y aceptación](tasks.md): pendientes de aprobación.
