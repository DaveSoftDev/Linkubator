# Especificación: S8.7 — Revisión de textos, mensajes y accesibilidad de formularios

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 25-X-2026 («Siguiente»).

## Objetivo

Revisar manualmente los textos, mensajes y accesibilidad de los formularios de colecciones ya implementados, registrar hallazgos verificables y confirmar que la experiencia presentada concuerda con las fuentes aprobadas, sin introducir cambios de comportamiento ni reglas nuevas.

## Fuentes

- [mvp0-plan.md → «S8: Gestión de colecciones (L)»](../../../plans/mvp0-plan.md): alcance de S8.7 y Definition of Done de S8.
- [requirements.md → «Colecciones»](../../../context/requirements.md) y [«Accesibilidad»](../../../context/requirements.md): comportamiento esperado de gestión y requisitos de interacción accesible.
- [specifications.md → «Rutas privadas»](../../../context/specifications.md) y [«Errores controlados»](../../../context/specifications.md): formularios canónicos, respuestas y presentación de errores.
- [AC-COL-01 — Crear colección](../../../plans/mvp0-acceptance-criteria.md), [AC-COL-02 — Editar colección](../../../plans/mvp0-acceptance-criteria.md) y [AC-COL-04b — Eliminar una colección con enlaces o la última colección](../../../plans/mvp0-acceptance-criteria.md): criterios funcionales propietarios de formularios y rechazos.
- [S8.2 — Operaciones de colecciones](../../s8.2-operaciones-colecciones/spec.md), [S8.3 — Errores de formularios de colecciones](../s8.3-errores-formularios-colecciones/spec.md) y [S8.4 — Identificadores y respuestas de colecciones](../s8.4-identificadores-y-respuestas-colecciones/spec.md): páginas, mensajes y flujos que se revisan, sin reabrir su implementación.

## Alcance

- **Incluye:** revisión manual de los formularios privados de creación, edición y confirmación de eliminación de colecciones; revisión de los mensajes de validación y de reglas de dominio que se presentan en esos flujos; inspección de textos visibles, labels y nombres accesibles, estructura semántica, orden y visibilidad del foco, navegación por teclado y percepción de errores conforme a los requisitos aplicables; registro de hallazgos con página/estado y resultado.
- **Excluye:** cambios productivos, redefinir textos exactos o reglas funcionales, volver a probar la lógica de negocio o la propiedad de datos, repetir pruebas HTTP automatizadas, auditoría completa de lectores de pantalla/contraste/navegadores, y despliegue.
- **Trabajo vecino y responsable de sus entregables:** S8.2 implementa los flujos; S8.3 implementa la presentación funcional de errores; S8.4 trata identificadores y accesibilidad de recursos. S8.7 observa sus resultados y registra discrepancias; las correcciones que requieran código deben aprobarse y ejecutarse en un trabajo autorizado separado o en el alcance aprobado correspondiente.

## Dependencias y prerrequisitos

- Los formularios y mensajes de colecciones implementados por S8.2 y S8.3 están disponibles en el entorno de revisión; S8.4 aporta la respuesta común de recursos no accesibles, que se inspecciona solo si aparece en las páginas de formulario incluidas.
- Antes de iniciar la revisión deben estar disponibles una identidad de desarrollo, datos de colecciones que permitan abrir creación, edición y confirmación de eliminación, y los estados de error aplicables. El procedimiento para preparar los estados se concretará en el plan técnico sin modificar datos ajenos.
- El cierre se limita a revisar y registrar el resultado. No depende del cierre técnico de S8.2/S8.4, de nuevas pruebas, ni de corregir hallazgos con cambios productivos. Los hallazgos bloqueantes se documentan como resultado no satisfactorio y requieren decisión humana sobre seguimiento; no se declara conformidad mientras un criterio esté incumplido.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [«Colecciones»](../../../context/requirements.md), [«Rutas privadas»](../../../context/specifications.md), [AC-COL-01](../../../plans/mvp0-acceptance-criteria.md) y [AC-COL-02](../../../plans/mvp0-acceptance-criteria.md) | Se presentan los formularios de creación y edición en sus estados normales y con errores aplicables. | Se leen los textos y se inspecciona la relación entre cada control, su label y la acción que realiza. | Los textos permiten comprender el propósito y la acción de cada formulario y control; los datos y mensajes presentados concuerdan con los criterios y las fuentes propietarias, sin texto contradictorio o pendiente identificado. |
| CA02 | [«Accesibilidad»](../../../context/requirements.md), [«Errores controlados»](../../../context/specifications.md) y [S8.3 → «Criterios de aceptación»](../s8.3-errores-formularios-colecciones/spec.md#criterios-de-aceptación) | Un formulario de creación o edición presenta uno o más errores de validación o de regla del dominio. | Se inspeccionan visualmente los mensajes y se navega con teclado hasta y entre los campos afectados. | Los errores aparecen junto a los campos afectados cuando proceda, son distinguibles sin depender solo del color y permiten identificar qué corregir; el foco y la lectura del error corresponden al comportamiento definido en las fuentes. |
| CA03 | [«Accesibilidad»](../../../context/requirements.md) y [«Rutas privadas»](../../../context/specifications.md) | Se accede mediante teclado a los formularios de creación, edición y confirmación de eliminación. | Se recorre el orden de tabulación, se activa cada control y se observa el foco durante la interacción. | El orden de foco es lógico, el foco permanece visible y los botones, enlaces y controles tienen nombres comprensibles; no se observa una barrera de teclado en el flujo revisado. |
| CA04 | [«Accesibilidad»](../../../context/requirements.md), [«Errores controlados»](../../../context/specifications.md), [AC-COL-04b](../../../plans/mvp0-acceptance-criteria.md) y [S8.3 → «Criterios de aceptación»](../s8.3-errores-formularios-colecciones/spec.md#criterios-de-aceptación) | Se presenta la confirmación de eliminación y un rechazo de eliminación por regla de dominio en un estado de prueba disponible. | Se revisan los textos, el propósito de las acciones y la comunicación del rechazo. | El usuario puede distinguir la acción de confirmar de la de cancelar/volver, y el rechazo explica el motivo de forma coherente con la fuente sin indicar que la operación tuvo éxito. |
| CA05 | [«Accesibilidad»](../../../context/requirements.md) | Se inspeccionan las páginas y estados cubiertos por CA01–CA04. | Se revisan encabezados, navegación y estructura de la página, además de los mensajes de estado pertinentes. | La jerarquía y estructura semántica permiten orientarse en el formulario y los mensajes de error o estado son perceptibles conforme a los requisitos; se registran explícitamente los límites de cualquier tecnología asistiva o medición no comprobada. |

Los casos exactos de validación, dominio, restricciones de eliminación y respuesta de recursos no accesibles se contrastan con sus fuentes propietarias; esta revisión no redefine mensajes, contratos ni reglas.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado durante la preparación. La lista de estados concretos y el método de evidencia de revisión se detallarán en el plan tras la aprobación de esta especificación. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 26-X-2026 («Siguiente»).
- [Tareas, evidencia y aceptación](tasks.md#aceptación-y-cierre), aprobadas por DLG el 26-X-2026 («Vamos»); trabajo completado, aceptado y archivado por DLG el 26-X-2026 («Completado»).
