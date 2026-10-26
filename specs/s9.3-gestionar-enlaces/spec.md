# Especificación: S9.3 — Gestionar enlaces

## Estado y aprobación

- Estado: aprobada.
- Preparada el 26-X-2026 por el asistente, a petición de DLG («Crea la spec S9.3»).
- Aprobación: DLG, 26-X-2026, decisión explícita en esta conversación («Aprobar la spec y generar plan.md como propuesta»).
- Aclaración confirmada por DLG el 26-X-2026: el estado público o privado del enlace forma parte del checkbox incluido en el formulario integral de edición.
- Aclaración de alcance confirmada por DLG el 26-X-2026: la colección se puede cambiar en el formulario de edición; guardar aplica la regla de privacidad del movimiento y la página independiente para mover enlaces se conserva.

## Objetivo

Completar en la zona privada la edición integral de los campos permitidos de un enlace, incluido su estado público o privado, sin cambiar su URL; moverlo entre colecciones propias mediante su página independiente y eliminarlo mediante confirmación explícita, respetando la propiedad, la privacidad y la consistencia de los datos.

## Fuentes

- [mvp0-plan.md → «S9: Gestión de enlaces (L)»](../../plans/mvp0-plan.md#s9-gestión-de-enlaces-l): entregable S9.3, límites frente a las tareas vecinas y Definition of Done de S9.
- [Criterios de aceptación del MVP0 → «AC-LINK-02 — Editar enlace»](../../plans/mvp0-acceptance-criteria.md#ac-link-02--editar-enlace), [«AC-LINK-03 — Mover enlace»](../../plans/mvp0-acceptance-criteria.md#ac-link-03--mover-enlace) y [«AC-LINK-05 — Eliminar enlace»](../../plans/mvp0-acceptance-criteria.md#ac-link-05--eliminar-enlace): comportamiento aprobado propietario de las operaciones; AC-LINK-02 incluye el estado público o privado en la edición integral.
- [requirements.md → «Enlaces»](../../context/requirements.md#enlaces), [«Público y privado»](../../context/requirements.md#público-y-privado) y [«Accesibilidad»](../../context/requirements.md#accesibilidad): capacidades y expectativas de interfaz.
- [specifications.md → «Páginas privadas»](../../context/specifications.md#páginas-privadas) y [«Errores controlados»](../../context/specifications.md#errores-controlados): rutas, propiedad, formularios y tratamiento de errores.
- [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [«Público y privado»](../../context/domain-model.md#público-y-privado): invariantes de propiedad, movimiento y privacidad.
- [architecture.md → «Application»](../../context/architecture.md#application), [«Seguridad web»](../../context/architecture.md#seguridad-web) y [«Persistencia»](../../context/architecture.md#persistencia): límites entre capas, protección de operaciones y consistencia de escrituras.
- [S9.1 — Crear enlace](../s9.1-crear-enlace/spec.md) y [S9.2 — Publicación de enlaces en colecciones privadas](../s9.2-publicacion-enlaces/spec.md): trabajos previos del flujo de enlaces y del rechazo de publicación indebida.

## Alcance

- **Incluye:** edición integral de título, descripción, imagen, colección y estado público o privado sin cambiar la URL; cambio de colección desde el formulario de edición con la regla de privacidad del movimiento; movimiento a otra colección propia mediante su página independiente; eliminación después de una confirmación explícita; validación, propiedad, errores y consistencia de las proyecciones afectadas.
- **Excluye:** creación de enlaces y disponibilidad de la opción pública en el formulario de creación (S9.1 y S9.2); listado y búsqueda de enlaces (S10); la batería transversal de tests HTTP (S9.4); la guía de revisión manual con URLs reales (S9.5); la revisión manual de presentación de imagen, título y descripción (S9.6); etiquetas y scraping, fuera del alcance confirmado para MVP0.
- **Trabajo vecino y responsable de sus entregables:** S9.1 y S9.2 proporcionan el flujo de creación ya aceptado. S9.4 añade los tests HTTP transversales; S9.5 y S9.6 realizan sus revisiones manuales. Ninguno de esos trabajos posteriores es prerrequisito para demostrar los criterios de esta especificación.

## Dependencias y prerrequisitos

- S9.1 y S9.2 tienen sus especificaciones, planes y tareas completados y aceptados; sus formularios y reglas de creación no se vuelven a implementar aquí.
- La identidad de la zona privada y las rutas para operaciones sobre enlaces están disponibles desde trabajos anteriores.
- Los criterios de esta especificación se limitan a edición integral, movimiento y eliminación; su validación y evidencia deben poder completarse sin esperar S9.4, S9.5 ni S9.6.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [«Páginas privadas»](../../context/specifications.md#páginas-privadas), [AC-LINK-02](../../plans/mvp0-acceptance-criteria.md#ac-link-02--editar-enlace), [requirements.md → «Enlaces»](../../context/requirements.md#enlaces) y [«Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) | Un usuario identificado solicita la edición de un enlace propio. | Se carga el formulario de edición. | Se cargan los datos editables y todas las colecciones propias, con la colección actual seleccionada, sin modificar el enlace; la URL no se ofrece como dato editable. Un identificador inválido no consulta ni modifica un enlace y uno inexistente o ajeno recibe el tratamiento común de recurso no accesible. |
| CA02 | [AC-LINK-02](../../plans/mvp0-acceptance-criteria.md#ac-link-02--editar-enlace), [«Páginas privadas»](../../context/specifications.md#páginas-privadas), [«Errores controlados»](../../context/specifications.md#errores-controlados) y [«Público y privado»](../../context/domain-model.md#público-y-privado) | Un usuario identificado presenta cambios válidos en un enlace propio, incluida una colección propia y el estado indicado por el checkbox público/privado. | Envía la edición integral. | Se actualizan el título, la descripción y la imagen; el checkbox establece el estado del enlace conforme a las reglas de privacidad. La URL original permanece igual. Si cambia la colección, se aplica la regla de privacidad del movimiento. Si un valor del checkbox no se puede interpretar como booleano, o la edición se rechaza por una colección inválida o una imagen inválida, ningún campo del enlace cambia y se presentan los errores previstos. |
| CA03 | [AC-LINK-02](../../plans/mvp0-acceptance-criteria.md#ac-link-02--editar-enlace), [«Errores controlados»](../../context/specifications.md#errores-controlados) y [«Público y privado»](../../context/domain-model.md#público-y-privado) | Un enlace conserva su colección privada y la petición de edición intenta hacerlo público junto con otros cambios válidos. | Se procesa la edición. | Se rechaza la operación completa, no cambia ningún campo del enlace y se presenta «No se puede hacer público un enlace de una colección privada». |
| CA04 | [AC-LINK-03](../../plans/mvp0-acceptance-criteria.md#ac-link-03--mover-enlace), [«Páginas privadas»](../../context/specifications.md#páginas-privadas), [«Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [«Público y privado»](../../context/domain-model.md#público-y-privado) | Un usuario identificado mueve un enlace propio a una colección destino válida. | Envía la operación de movimiento. | El enlace conserva su identidad y sus datos editables, queda asignado al destino y pasa a ser privado. Si el destino es inválido, inexistente o ajeno, o la petición está manipulada, no se aplica ningún cambio. |
| CA06 | [AC-LINK-05](../../plans/mvp0-acceptance-criteria.md#ac-link-05--eliminar-enlace), [S9.3](../../plans/mvp0-plan.md#s9-gestión-de-enlaces-l) y [«Páginas privadas»](../../context/specifications.md#páginas-privadas) | Un usuario identificado solicita eliminar un enlace propio. | Abre la confirmación y luego la confirma mediante la acción explícita. | La solicitud de confirmación no elimina el enlace; solo la confirmación explícita lo elimina. La operación elimina también sus relaciones y la entrada de su proyección de búsqueda. |
| CA07 | [AC-LINK-05](../../plans/mvp0-acceptance-criteria.md#ac-link-05--eliminar-enlace), [«Páginas privadas»](../../context/specifications.md#páginas-privadas) y [«Application»](../../context/architecture.md#application) | La solicitud de eliminación contiene un identificador inválido, o uno válido de un enlace inexistente o ajeno. | Se procesa la solicitud. | Un identificador inválido produce el error de validación sin modificar datos. Un identificador válido inexistente o ajeno produce el resultado común de recurso no accesible y su código estable de eliminación, sin revelar cuál caso ocurrió ni modificar datos. |
| CA08 | [Definition of Done de S9](../../plans/mvp0-plan.md#s9-gestión-de-enlaces-l) y [«Persistencia»](../../context/architecture.md#persistencia) | Una operación incluida en esta especificación modifica o elimina un enlace. | La escritura se confirma. | La proyección FTS5 afectada queda sincronizada con el estado persistido en la misma operación atómica, incluido el cambio de colección desde edición; tras eliminar, no permanece una entrada de búsqueda obsoleta. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno detectado en las fuentes consultadas. | — | — | — |

## Artefactos relacionados

- [S9.1 — Crear enlace](../s9.1-crear-enlace/spec.md): trabajo previo completado y aceptado.
- [S9.2 — Publicación de enlaces en colecciones privadas](../s9.2-publicacion-enlaces/spec.md): trabajo previo completado y aceptado.
- [Plan técnico](plan.md): aprobado por DLG el 26-X-2026.
- [Tareas y evidencia](tasks.md#aceptación-y-cierre): aprobadas por DLG el 26-X-2026 («Vamos»); implementación iniciada por instrucción expresa el mismo día, ampliada tras la aclaración confirmada sobre edición de colección y aceptada por DLG el 26-X-2026 («Completado»).
