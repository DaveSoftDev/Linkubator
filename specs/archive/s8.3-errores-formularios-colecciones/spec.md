# Especificación: S8.3 — Errores de formularios de colecciones

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 25-X-2026 («Siguiente»).

## Objetivo

Completar la presentación Web de los errores de validación y de las reglas del dominio en los formularios privados de colecciones, de modo que el usuario pueda identificar el problema y corregirlo sin perder los datos introducidos.

## Fuentes

- [mvp0-plan.md → «S8: Gestión de colecciones (L)»](../../../plans/mvp0-plan.md): alcance de S8.3, trabajo vecino y Definition of Done de S8.
- [Criterios de aceptación de S4 → «AC-COL-01 — Crear colección»](../../../plans/mvp0-acceptance-criteria.md), [«AC-COL-02 — Editar colección»](../../../plans/mvp0-acceptance-criteria.md), [«AC-COL-04b — Eliminar una colección con enlaces o la última colección»](../../../plans/mvp0-acceptance-criteria.md): criterios propietarios de los rechazos de operaciones.
- [requirements.md → «Accesibilidad»](../../../context/requirements.md): asociación, percepción y foco de los errores en formularios.
- [specifications.md → «Rutas privadas»](../../../context/specifications.md) y [«Errores controlados»](../../../context/specifications.md): comportamiento de los formularios y presentación de errores.
- [domain-model.md → «Collection»](../../../context/domain-model.md): restricciones de colección que pueden producir errores de dominio.
- [S8.2 — Operaciones de colecciones](../../s8.2-operaciones-colecciones/spec.md): formularios y operaciones Web a los que se aplica esta presentación.

## Alcance

- **Incluye:** mostrar los errores de validación junto a cada campo afectado en los formularios de creación y edición; conservar los datos enviados cuando se vuelve a mostrar el formulario; marcar visualmente los campos con error y enfocar el primero; mostrar en la página correspondiente el motivo de los rechazos de reglas del dominio al crear, editar o eliminar colecciones, sin cambios persistidos cuando la operación se rechaza.
- **Excluye:** crear o modificar las reglas de validación y del dominio, definir mensajes funcionales nuevos, las respuestas por identificadores inválidos o recursos válidos no accesibles (S8.4), el listado y las operaciones de colecciones como tales (S8.1–S8.2), la suite HTTP transversal (S8.5), la revisión manual final de textos y accesibilidad (S8.7) y despliegue.
- **Trabajo vecino y responsable de sus entregables:** S8.2 proporciona las rutas y operaciones; S8.4 trata los identificadores de ruta y la falta de acceso; S8.5 valida la integración HTTP de la etapa; S8.7 realiza la revisión manual final. Ninguno aporta código, tests ni evidencia necesarios para cerrar los criterios de esta spec.

## Dependencias y prerrequisitos

- Las páginas y operaciones de crear, editar y eliminar colecciones previstas por S8.2 están presentes en el código de trabajo. Su cierre técnico permanece pendiente; esta spec valida sus propios criterios y no presupone que S8.2 esté técnicamente cerrada.
- Los resultados de validación y de dominio se originan en las capas propietarias existentes. Si durante el diseño no se pueden identificar de forma fiable en los contratos Web actuales, se registrará el bloqueo concreto; no se cambiarán reglas ni contratos funcionales sin aprobación.
- Las respuestas de identificadores inválidos o recursos no accesibles se mantienen fuera de esta spec y no son prerrequisito para su cierre.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [«Accesibilidad»](../../../context/requirements.md), [«Errores controlados»](../../../context/specifications.md), [AC-COL-01](../../../plans/mvp0-acceptance-criteria.md) y [AC-COL-02](../../../plans/mvp0-acceptance-criteria.md) | El usuario envía datos inválidos al formulario de creación o edición de una colección. | La operación devuelve errores de validación y se vuelve a presentar el formulario. | Cada error aparece asociado al campo afectado y es anunciable por tecnologías de asistencia; los campos con error quedan identificados visualmente y los demás datos introducidos se conservan. No se presenta un error de campo para una regla no relacionada con ese campo. |
| CA02 | [«Accesibilidad»](../../../context/requirements.md) | Se vuelve a presentar un formulario de creación o edición con uno o más errores de validación. | El formulario queda disponible para continuar la interacción. | El foco queda en el primer campo con error, siguiendo el orden lógico del formulario; si no hay errores, esta spec no altera el foco inicial definido para formularios. |
| CA03 | [«Errores controlados»](../../../context/specifications.md), [AC-COL-01](../../../plans/mvp0-acceptance-criteria.md) y [AC-COL-02](../../../plans/mvp0-acceptance-criteria.md) | Crear o editar una colección se rechaza por una regla del dominio, incluidos nombre o slug no aceptados o una transición de visibilidad rechazada. | El usuario recibe la respuesta del formulario. | Se presenta un mensaje que explica el motivo del rechazo en la página correspondiente; no se atribuye el error a un campo salvo que la operación proporcione un error de validación para ese campo, y no se aplican cambios a la colección ni a sus enlaces. |
| CA04 | [«Errores controlados»](../../../context/specifications.md), [AC-COL-04b](../../../plans/mvp0-acceptance-criteria.md) y [«Collection»](../../../context/domain-model.md) | La eliminación de una colección propia se rechaza por una regla del dominio. | El usuario solicita la eliminación desde el flujo canónico de confirmación. | La página muestra el motivo del rechazo y la colección permanece sin cambios. No se incluye aquí el tratamiento de errores de identificador o de acceso, cubierto por S8.4. |

Los casos negativos de nombre, slug, visibilidad y restricciones de eliminación se contrastan con las fuentes enlazadas; esta spec no redefine sus reglas, mensajes exactos ni resultados.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado durante la preparación de esta especificación. Los contratos concretos de resultados y validación se verificarán en el diseño técnico, tras aprobar esta spec. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 25-X-2026 («Siguiente»).
- [Tareas](tasks.md), pendientes de aprobación.
