# Especificación: S9.4 — Pruebas de integración HTTP de enlaces

## Estado y aprobación

- Estado: aprobada por DLG el 26-X-2026 («Aprobar la especificación S9.4 y continuar con el plan»).
- Preparada el 26-X-2026 a petición de DLG («crea la spec S9.4»).
- La aprobación corresponde únicamente a esta especificación; el plan técnico requiere aprobación independiente.

## Objetivo

Demostrar mediante peticiones HTTP a la aplicación que los flujos de enlaces respetan los criterios aprobados de propiedad, validación, privacidad y consistencia ante solicitudes inválidas o manipuladas.

## Fuentes

- [mvp0-plan.md → «S9: Gestión de enlaces (L)»](../../plans/mvp0-plan.md#s9-gestión-de-enlaces-l): alcance y entregable de S9.4.
- [Criterios de aceptación del MVP0 → «AC-LINK-01 — Crear enlace»](../../plans/mvp0-acceptance-criteria.md#ac-link-01--crear-enlace), [«AC-LINK-02 — Editar enlace»](../../plans/mvp0-acceptance-criteria.md#ac-link-02--editar-enlace), [«AC-LINK-03 — Mover enlace»](../../plans/mvp0-acceptance-criteria.md#ac-link-03--mover-enlace), [«AC-LINK-05 — Eliminar enlace»](../../plans/mvp0-acceptance-criteria.md#ac-link-05--eliminar-enlace) y [«AC-TRANS-01 — Identidad, propiedad y límites de acceso»](../../plans/mvp0-acceptance-criteria.md#ac-trans-01--identidad-propiedad-y-límites-de-acceso): comportamiento aprobado que debe demostrar la cobertura HTTP.
- [S4.7 — Casos de uso de enlaces → «Criterios de aceptación»](../archive/s4.7-casos-uso-enlaces/spec.md#criterios-de-aceptación): criterios de Application propietarios de los flujos y límites de acceso.
- [requirements.md → «Enlaces»](../../context/requirements.md#enlaces), [specifications.md → «Páginas privadas»](../../context/specifications.md#páginas-privadas) y [«Errores controlados»](../../context/specifications.md#errores-controlados): comportamiento Web y respuestas previstas.
- [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [«Público y privado»](../../context/domain-model.md#público-y-privado): invariantes que deben preservarse.
- [architecture.md → «Seguridad web»](../../context/architecture.md#seguridad-web) y [«Persistencia»](../../context/architecture.md#persistencia): protección de solicitudes y consistencia de las escrituras.
- [S9.1 — Crear enlace](../s9.1-crear-enlace/spec.md), [S9.2 — Publicación de enlaces en colecciones privadas](../s9.2-publicacion-enlaces/spec.md) y [S9.3 — Gestionar enlaces](../s9.3-gestionar-enlaces/spec.md): flujos Web a los que se aplica la cobertura transversal.

## Alcance

- **Incluye:** pruebas de integración HTTP de creación, edición integral —incluido el estado público o privado en su checkbox—, movimiento y eliminación de enlaces, centradas en colecciones inválidas, inexistentes o ajenas, peticiones manipuladas, propiedad y ausencia de cambios parciales ante rechazos.
- **Excluye:** cambios de comportamiento o de reglas aprobadas; pruebas unitarias de Application o persistencia; revisión manual de URLs reales y de presentación, asignadas a trabajos vecinos.
- **Trabajo vecino y responsable de sus entregables:** S9.1 y S9.2 cubren la creación y su regla de privacidad; S9.3 cubre la edición integral, el movimiento y la eliminación. S9.5 conserva la revisión manual y no depende de los resultados de esta batería para sus propios criterios.

## Dependencias y prerrequisitos

- S9.1, S9.2 y S9.3 tienen su implementación y validación aceptadas; sus flujos son el objeto de las pruebas, no se vuelven a implementar aquí.
- La identidad y la infraestructura de pruebas HTTP de la aplicación están disponibles en el proyecto.
- Los criterios de esta especificación se limitan a comprobar por HTTP las fuentes aprobadas; no requieren decisiones nuevas de producto.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [AC-LINK-01](../../plans/mvp0-acceptance-criteria.md#ac-link-01--crear-enlace), [S4.7 CA01](../archive/s4.7-casos-uso-enlaces/spec.md#criterios-de-aceptación), [S9.1 CA04](../s9.1-crear-enlace/spec.md#criterios-de-aceptación) y [«Páginas privadas»](../../context/specifications.md#páginas-privadas) | Un usuario identificado intenta crear un enlace seleccionando una colección con identificador inválido, inexistente o perteneciente a otro usuario. | Se envía directamente la solicitud HTTP de creación. | La respuesta sigue el tratamiento previsto para el formulario, no se crea el enlace y no se expone información de la colección ajena. |
| CA02 | [AC-LINK-02](../../plans/mvp0-acceptance-criteria.md#ac-link-02--editar-enlace), [AC-LINK-03](../../plans/mvp0-acceptance-criteria.md#ac-link-03--mover-enlace), [S4.7 CA02 y CA03](../archive/s4.7-casos-uso-enlaces/spec.md#criterios-de-aceptación), [S9.3 CA02 y CA04](../s9.3-gestionar-enlaces/spec.md#criterios-de-aceptación) y [«Páginas privadas»](../../context/specifications.md#páginas-privadas) | Un usuario identificado envía cambios a un enlace propio e incluye una colección de destino inválida, inexistente o ajena. | Se envía directamente la solicitud HTTP de edición o movimiento. | Se aplica el rechazo previsto y el estado previo del enlace y su asociación se conserva, sin revelar datos de otro usuario. |
| CA03 | [AC-LINK-01b](../../plans/mvp0-acceptance-criteria.md#ac-link-01b--crear-enlace-público), [AC-LINK-02](../../plans/mvp0-acceptance-criteria.md#ac-link-02--editar-enlace), [AC-TRANS-01](../../plans/mvp0-acceptance-criteria.md#ac-trans-01--identidad-propiedad-y-límites-de-acceso), [S9.2 CA03](../s9.2-publicacion-enlaces/spec.md#criterios-de-aceptación) y [S9.3 CA03](../s9.3-gestionar-enlaces/spec.md#criterios-de-aceptación) | Una solicitud de creación o edición elude la interfaz y envía datos manipulados, como intentar publicar un enlace en una colección privada o proporcionar datos de cliente como identidad o propietario. | Se procesa la solicitud HTTP de creación o edición. | La aplicación aplica los rechazos aprobados, no autoriza la operación con datos de identidad del cliente y no altera recursos fuera de las reglas de propiedad y privacidad. |
| CA04 | [AC-LINK-05](../../plans/mvp0-acceptance-criteria.md#ac-link-05--eliminar-enlace), [AC-TRANS-01](../../plans/mvp0-acceptance-criteria.md#ac-trans-01--identidad-propiedad-y-límites-de-acceso), [S4.7 CA05 y CA07](../archive/s4.7-casos-uso-enlaces/spec.md#criterios-de-aceptación) y [«Páginas privadas»](../../context/specifications.md#páginas-privadas) | Un usuario identificado intenta consultar o modificar enlaces con identificadores inválidos, inexistentes o pertenecientes a otro usuario. | Se envían las solicitudes HTTP a las rutas privadas aplicables. | Las respuestas corresponden al tratamiento aprobado para cada caso; los casos de recurso inexistente y ajeno no revelan cuál ocurrió, y los recursos ajenos permanecen sin cambios ni exposición de sus datos. |
| CA05 | [AC-LINK-02](../../plans/mvp0-acceptance-criteria.md#ac-link-02--editar-enlace), [AC-LINK-03](../../plans/mvp0-acceptance-criteria.md#ac-link-03--mover-enlace), [AC-TRANS-01](../../plans/mvp0-acceptance-criteria.md#ac-trans-01--identidad-propiedad-y-límites-de-acceso), [S4.7 CA01–CA05](../archive/s4.7-casos-uso-enlaces/spec.md#criterios-de-aceptación) y [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) | Una escritura HTTP de enlace incluye cambios válidos y se rechaza por una condición de validación, propiedad o privacidad. | Se comprueba el estado persistido tras la respuesta. | No queda ningún cambio parcial en el enlace, sus relaciones ni las proyecciones afectadas por la operación, conforme a las garantías de persistencia aplicables. |

Los casos negativos concretos y sus respuestas se verifican mediante las fuentes enlazadas; esta especificación no establece nuevos códigos, mensajes ni reglas funcionales.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| No se detectan bloqueos para preparar esta especificación. | — | — | — |

## Artefactos relacionados

El plan técnico y las tareas se prepararán únicamente tras las aprobaciones explícitas de los artefactos precedentes.
