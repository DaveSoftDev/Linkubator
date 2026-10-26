# Especificación: S9.6 — Revisión de la presentación de enlaces

## Estado y aprobación

- Estado: aprobada.
- Preparada el 26-X-2026 a petición de DLG («Crea la spec S9.6»).
- Aprobación: DLG, 26-X-2026 («Siguiente»).

## Objetivo

Registrar una revisión manual reproducible de cómo se presentan los campos de imagen, título y descripción en los formularios privados de creación y edición de enlaces, y dejar constancia de la decisión humana sobre el resultado.

## Fuentes

- [mvp0-plan.md → «S9: Gestión de enlaces (L)»](../../plans/mvp0-plan.md#s9-gestión-de-enlaces-l): asignación y alcance de S9.6.
- [Criterios de aceptación del MVP0 → «AC-LINK-01 — Crear enlace»](../../plans/mvp0-acceptance-criteria.md#ac-link-01--crear-enlace) y [«AC-LINK-02 — Editar enlace»](../../plans/mvp0-acceptance-criteria.md#ac-link-02--editar-enlace): datos del enlace que se presentan en los formularios.
- [requirements.md → «Enlaces»](../../context/requirements.md#enlaces) y [«Accesibilidad»](../../context/requirements.md#accesibilidad): capacidades disponibles y expectativas de uso de los controles y errores.
- [specifications.md → «Ajuste de `Image`»](../../context/specifications.md#ajuste-de-image) y [«Errores controlados»](../../context/specifications.md#errores-controlados): tratamiento de la imagen y presentación de errores.
- [S9.1 — Crear enlace → «Criterios de aceptación»](../s9.1-crear-enlace/spec.md#criterios-de-aceptación) y [S9.3 — Gestionar enlaces → «Criterios de aceptación»](../s9.3-gestionar-enlaces/spec.md#criterios-de-aceptación): formularios Web disponibles para la revisión.

## Alcance

- **Incluye:** revisión manual en navegador de la presentación de los campos de título, descripción e imagen en los formularios de creación y edición; observación de sus etiquetas, indicación de opcionalidad, contenido introducido o guardado y errores visibles asociados a esos campos; registro reproducible de observaciones y hallazgos.
- **Excluye:** cambios de código o de reglas; revisión de navegación, duplicados y privacidad cubierta por S9.5; scraping y edición automática de metadatos; presentación de enlaces en páginas públicas, cuya revisión corresponde a la etapa que entregue esas páginas; pruebas automatizadas.
- **Trabajo vecino y responsable de sus entregables:** S9.1 y S9.3 proporcionan los formularios; S9.4 cubre pruebas automatizadas HTTP y S9.5 revisa manualmente la creación, duplicidad y privacidad. S9.6 no sustituye esos trabajos ni evalúa la presentación en páginas públicas.

## Dependencias y prerrequisitos

- S9.1 y S9.3 tienen implementación y aceptación registradas; los formularios de creación y edición están disponibles.
- Se necesita una instancia Web ejecutable, un navegador, una sesión privada y enlaces propios preparados para observar campos con valores y vacíos. Los datos usados para la revisión deben pertenecer a la persona revisora.
- Los errores de imagen se inspeccionan solo si pueden provocarse con datos de prueba propios sin afectar otros recursos; no son necesarios para revisar los estados normales de los formularios.
- La revisión puede cerrarse con evidencia de los formularios privados y no depende de S9.4, S9.5, del scraping ni de la implementación de páginas públicas.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [AC-LINK-01](../../plans/mvp0-acceptance-criteria.md#ac-link-01--crear-enlace), [requirements.md → «Enlaces»](../../context/requirements.md#enlaces) y [«Accesibilidad»](../../context/requirements.md#accesibilidad) | El formulario privado de creación está disponible en el navegador. | Se revisa la presentación de los campos de título, descripción e imagen, su asociación con las etiquetas y la indicación de que pueden dejarse vacíos. | Se registra qué se observó en cada campo y si la presentación permite identificarlo y usarlo conforme a las fuentes; cualquier problema queda descrito con pasos y contexto para reproducirlo. |
| CA02 | [AC-LINK-02](../../plans/mvp0-acceptance-criteria.md#ac-link-02--editar-enlace), [requirements.md → «Enlaces»](../../context/requirements.md#enlaces) y [«Errores controlados»](../../context/specifications.md#errores-controlados) | El formulario privado de edición carga un enlace propio preparado con los campos opcionales informados o vacíos. | Se revisa cómo se presentan el título, la descripción y la imagen guardados, así como los errores visibles de esos campos cuando el entorno permite provocarlos con seguridad. | Se registra por campo el contenido o estado observado y la presentación del error si se probó; los hallazgos identifican el enlace de prueba sin datos sensibles y los pasos para reproducirlos. |
| CA03 | [S9: Gestión de enlaces — Definition of Done](../../plans/mvp0-plan.md#s9-gestión-de-enlaces-l), [CA01](#criterios-de-aceptación) y [CA02](#criterios-de-aceptación) | Se han completado las observaciones propias de la revisión. | Se consolidan los resultados y se presentan para decisión humana. | Quedan registrados fecha, entorno, navegador y versión, estados revisados, resultados, límites y hallazgos; DLG acepta explícitamente el resultado o solicita ajustes, sin atribuir a esta revisión cambios productivos ni comprobaciones no realizadas. |

La revisión visual es cualitativa y se limita a los formularios disponibles. No concluye que la imagen se represente correctamente en una página pública ni que se haya ejecutado scraping.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para redactar la especificación. El navegador, el entorno y los datos propios de prueba se concretarán en el plan técnico. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md): aprobado por DLG el 26-X-2026 («Siguiente»).
- [Tareas, evidencia y aceptación](tasks.md): pendientes de aprobación.
