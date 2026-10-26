# Especificación: S9.1 — Crear enlace

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 26-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Objetivo

Completar en la zona privada el flujo Web para crear un enlace en una colección propia, con la URL, los datos opcionales y el estado de privacidad conforme a los criterios aprobados.

## Fuentes

- [mvp0-plan.md → «S9: Gestión de enlaces (L)»](../../plans/mvp0-plan.md#s9-gestión-de-enlaces-l): entregable S9.1, límites frente a las tareas vecinas y Definition of Done de S9.
- [Criterios de aceptación del MVP0 → «AC-LINK-01 — Crear enlace»](../../plans/mvp0-acceptance-criteria.md#ac-link-01--crear-enlace) y [«AC-LINK-01b — Crear enlace público»](../../plans/mvp0-acceptance-criteria.md#ac-link-01b--crear-enlace-público): comportamiento aprobado propietario de la creación.
- [requirements.md → «Enlaces»](../../context/requirements.md#enlaces), [«Público y privado»](../../context/requirements.md#público-y-privado) y [«Accesibilidad»](../../context/requirements.md#accesibilidad): capacidades y expectativas de interfaz.
- [specifications.md → «URLs de los enlaces»](../../context/specifications.md#urls-de-los-enlaces), [«Duplicados»](../../context/specifications.md#duplicados), [«Listados»](../../context/specifications.md#listados), [«Rutas privadas»](../../context/specifications.md#rutas-privadas) y [«Errores controlados»](../../context/specifications.md#errores-controlados): rutas, opciones del selector, resultados y errores.
- [domain-model.md → «Link»](../../context/domain-model.md#link), [«Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [«Público y privado»](../../context/domain-model.md#público-y-privado): invariantes de propiedad y privacidad.
- [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura), [«Seguridad web»](../../context/architecture.md#seguridad-web) y [«Persistencia»](../../context/architecture.md#persistencia): límites de capas, formularios protegidos y garantías de escritura.
- [decisions.md → «Producto y alcance»](../../context/decisions.md#producto-y-alcance) y [roadmap.md → «MVP1 confirmado»](../../context/roadmap.md#mvp1-confirmado): alcance de MVP0 y presentación mediante diálogos reservada a MVP1.

## Alcance

- **Incluye:** formulario canónico de creación en `/app/enlaces/nuevo`; selector sin paginar de las colecciones propias; valores opcionales del enlace; creación mediante el caso de uso de Application; estado privado predeterminado y creación pública explícita cuando las fuentes lo permiten; validación, duplicados y respuesta Web de la operación.
- **Excluye:** listado y búsqueda de enlaces (S10); edición de enlaces, incluido su estado público o privado, movimiento y eliminación (S9.3); deshabilitar la opción pública para colecciones privadas y rechazar la petición manipulada que la solicita (S9.2); la batería transversal de tests HTTP de S9.4; revisión manual (S9.5); revisión de la presentación de imagen, título y descripción (S9.6); etiquetas y scraping, fuera del alcance confirmado para MVP0; presentación del formulario mediante diálogo, reservada a MVP1.
- **Trabajo vecino y responsable de sus entregables:** S9.2 cubre la disponibilidad de la opción pública según la colección y el rechazo de una petición manipulada; S9.5 añade los tests HTTP transversales de la etapa; S9.6 y S9.7 realizan las revisiones manuales de sus respectivos alcances. Ninguno es prerrequisito para demostrar los criterios propios de esta especificación.

## Dependencias y prerrequisitos

- S4.7 contiene los casos de uso de Application para enlaces y está archivada como trabajo aprobado; S9.1 consume la capacidad de creación existente, no redefine sus reglas.
- S8.1 proporciona el listado de colecciones aceptado, que sirve de contexto de navegación para iniciar la operación. El selector debe obtener sus opciones conforme a las reglas propietarias de [«Listados»](../../context/specifications.md#listados).
- La identidad privada y las rutas base de Web están disponibles desde S7. No se requiere que el cliente envíe ni seleccione el propietario del enlace.
- Las pruebas y la evidencia de esta especificación deben poder completarse con tareas propias. S9.5 y las revisiones manuales posteriores no condicionan su cierre.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [«Rutas privadas»](../../context/specifications.md#rutas-privadas), [«Listados»](../../context/specifications.md#listados), [AC-LINK-01](../../plans/mvp0-acceptance-criteria.md#ac-link-01--crear-enlace), [domain-model.md → «Link»](../../context/domain-model.md#link), [decisions.md → «Producto y alcance»](../../context/decisions.md#producto-y-alcance) y [roadmap.md → «MVP1 confirmado»](../../context/roadmap.md#mvp1-confirmado) | Un usuario identificado accede a la ruta canónica de creación. | Solicita el formulario por GET. | Se presenta el formulario en la página canónica, no como diálogo; el selector incluye todas sus colecciones sin paginar y el estado público parte desmarcado. GET no modifica datos. |
| CA02 | [AC-LINK-01](../../plans/mvp0-acceptance-criteria.md#ac-link-01--crear-enlace), [AC-LINK-01b](../../plans/mvp0-acceptance-criteria.md#ac-link-01b--crear-enlace-público), [«URLs de los enlaces»](../../context/specifications.md#urls-de-los-enlaces), [«Rutas privadas»](../../context/specifications.md#rutas-privadas), [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [«Público y privado»](../../context/domain-model.md#público-y-privado) | Un usuario identificado envía datos válidos y una colección propia. | Envía el formulario por POST, dejando el estado público desmarcado o solicitándolo para una colección que permite esa transición. | Se crea un único enlace para el usuario identificado y la colección seleccionada; la URL se ajusta y valida según su fuente, los datos opcionales se conservan y el estado resultante cumple los criterios propietarios. La respuesta correcta redirige al listado de enlaces. |
| CA03 | [AC-LINK-01](../../plans/mvp0-acceptance-criteria.md#ac-link-01--crear-enlace), [«Duplicados»](../../context/specifications.md#duplicados) y [«Errores controlados»](../../context/specifications.md#errores-controlados) | La solicitud contiene una URL o imagen inválida, o una URL normalizada que ya existe para el usuario. | Envía el formulario. | No se crea un enlace; el formulario vuelve a presentarse con los datos y errores correspondientes. En caso de duplicado, se identifica la colección donde está el enlace existente, sin crear otro registro. |
| CA04 | [«Rutas privadas»](../../context/specifications.md#rutas-privadas), [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [AC-LINK-01](../../plans/mvp0-acceptance-criteria.md#ac-link-01--crear-enlace) | La solicitud selecciona una colección con identificador inválido, inexistente o ajeno al usuario identificado. | Envía el formulario. | No se crea ni modifica ningún enlace; se devuelve el error de validación previsto sin leer, modificar ni revelar datos de otra persona. |
| CA05 | [«Rutas privadas»](../../context/specifications.md#rutas-privadas), [architecture.md → «Seguridad web»](../../context/architecture.md#seguridad-web), [requirements.md → «Accesibilidad»](../../context/requirements.md#accesibilidad) y [«Errores controlados»](../../context/specifications.md#errores-controlados) | Se presenta o envía el formulario de creación. | Se comprueban el método, la protección y la asociación de controles y errores. | Las modificaciones requieren POST con antiforgery; los controles y errores se exponen con las asociaciones accesibles exigidas por sus fuentes. |

Los casos negativos de URL, imagen, colección y privacidad se verifican contra las fuentes enlazadas; esta especificación no redefine algoritmos, límites, mensajes ni respuestas.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno tras sincronizar la presentación de los formularios canónicos con la regla de diálogos como presentación alternativa. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md): aprobado por DLG el 26-X-2026 («Siguiente»).
- [Tareas y evidencia](tasks.md): aprobadas por DLG el 26-X-2026 («Ok, ahora inicia la implementación de S9.1»); contienen la implementación completada, la validación focalizada y la aceptación humana registrada el 26-X-2026 («Comprobado» en T08; «Completado» en T09).
