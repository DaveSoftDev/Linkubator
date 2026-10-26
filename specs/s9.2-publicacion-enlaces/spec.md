# Especificación: S9.2 — Publicación de enlaces en colecciones privadas

## Estado y aprobación

- Estado: aprobada.
- Preparada el 26-X-2026 por el asistente, a petición de DLG («Crea la spec S9.2» en esta conversación).
- Aprobación: DLG, 26-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Objetivo

Completar el flujo Web de creación de enlaces para que la opción de publicarlos refleje la privacidad de la colección seleccionada y para que una petición manipulada no cree un enlace público en una colección privada.

## Fuentes

- [mvp0-plan.md → «S9: Gestión de enlaces (L)»](../../plans/mvp0-plan.md#s9-gestión-de-enlaces-l): entregable S9.2 y límites frente a trabajos vecinos.
- [Criterios de aceptación del MVP0 → «AC-LINK-01b — Crear enlace público»](../../plans/mvp0-acceptance-criteria.md#ac-link-01b--crear-enlace-público): resultado aprobado para crear enlaces públicos.
- [specifications.md → «Páginas privadas»](../../context/specifications.md#páginas-privadas) y [«Errores controlados»](../../context/specifications.md#errores-controlados): comportamiento del formulario, rechazo y respuesta.
- [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado): invariantes de privacidad.
- [S9.1 — Crear enlace](../s9.1-crear-enlace/spec.md): flujo de creación y criterios aprobados que esta especificación complementa.

## Alcance

- **Incluye:** habilitación y explicación de la opción pública en el formulario de creación según la colección seleccionada; desmarcado de la opción cuando no puede usarse; rechazo en servidor de una solicitud de creación manipulada que intente publicar el enlace en una colección privada.
- **Excluye:** edición de enlaces existentes, incluido su estado público o privado (S9.3); reglas de propiedad, formato y existencia de colecciones ya cubiertas por los criterios aprobados de S9.1; tests HTTP transversales de S9.4; revisión manual de la etapa.
- **Trabajo vecino y responsable de sus entregables:** S9.1 proporciona el flujo base de creación. S9.3 cubre la edición integral de enlaces y S9.4 sus pruebas HTTP transversales; S9.5 cubre la revisión manual. Sus entregables no son prerrequisitos para cerrar esta especificación.

## Dependencias y prerrequisitos

- S9.1 tiene especificación, plan y tareas completados y aceptados; proporciona el formulario y la operación de creación que se amplían aquí.
- La identidad del usuario y las rutas privadas necesarias están disponibles desde la base Web existente.
- Los criterios y validaciones de esta especificación se limitan al flujo de creación y pueden completarse sin esperar a las tareas vecinas.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [«Páginas privadas»](../../context/specifications.md#páginas-privadas), [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado) | Se presenta el formulario de creación sin colección seleccionada o con una colección privada. | Se muestra o actualiza el estado de la opción pública. | La opción permanece deshabilitada y desmarcada, se explica el motivo y seleccionar o volver a seleccionar una colección privada no permite conservarla marcada. |
| CA02 | [«Páginas privadas»](../../context/specifications.md#páginas-privadas) y [«Público y privado»](../../context/domain-model.md#público-y-privado) | Se presenta el formulario con una colección pública seleccionada. | Se actualiza la opción pública. | La opción queda habilitada y no se marca automáticamente. |
| CA03 | [AC-LINK-01b](../../plans/mvp0-acceptance-criteria.md#ac-link-01b--crear-enlace-público), [«Errores controlados»](../../context/specifications.md#errores-controlados), [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado) | Un usuario identificado envía una creación para una colección privada propia solicitando que el enlace sea público, aunque haya eludido la interfaz. | La aplicación procesa el POST manipulado. | Se rechaza la operación, no se crea el enlace y se presenta el mensaje definido por la fuente mediante la respuesta accesible prevista para el rechazo. |
| CA04 | [S9.1 → «Criterios de aceptación» (CA04)](../s9.1-crear-enlace/spec.md#criterios-de-aceptación), [«Rutas privadas»](../../context/specifications.md#rutas-privadas) | La solicitud incluye una colección inválida, inexistente o ajena al usuario. | Se procesa el POST de creación. | Se conserva el rechazo y el tratamiento de propiedad aprobados en S9.1; esta especificación no altera ni duplica ese contrato. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno detectado en las fuentes consultadas. | — | — | — |

## Artefactos relacionados

- [S9.1 — Crear enlace](../s9.1-crear-enlace/spec.md): trabajo previo del flujo base de creación.
