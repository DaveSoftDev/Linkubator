# Especificación: S8.1 — Listado de colecciones

## Estado y aprobación

- Estado: aprobada con regla fuente actualizada.
- Aprobaciones:
  - DLG, 25-X-2026 («Siguiente»).
  - Resolución de `p` inválido/fuera de rango para este trabajo: DLG autorizó mostrar la primera página sin error el 25-X-2026 («Ignorar el valor y mostrar la primera página sin error»), y autorizó actualizar la fuente propietaria el 25-X-2026.

## Objetivo

Completar el listado privado de colecciones en `/app/colecciones`, integrando la búsqueda y la paginación existentes con los criterios aprobados y mostrando el estado vacío cuando corresponda.

## Fuentes

- [mvp0-plan.md → «S8: Gestión de colecciones (L)»](../../../plans/mvp0-plan.md): alcance de S8.1 y Definition of Done de S8.
- [Criterios de aceptación de S4 → «AC-COL-05 — Listar colecciones del usuario»](../../../plans/mvp0-acceptance-criteria.md): criterio aprobado propietario del listado.
- [requirements.md → «Búsqueda y filtros»](../../../context/requirements.md): comportamiento esperado de la búsqueda de colecciones.
- [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md), [«Listados»](../../../context/specifications.md) y [«Páginas privadas»](../../../context/specifications.md): parámetros, orden, paginación, ruta y estados del listado.
- [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md): aislamiento por usuario.
- [requirements.md → «Accesibilidad»](../../../context/requirements.md): expectativas de accesibilidad aplicables a la página.

## Alcance

- **Incluye:** página GET `/app/colecciones`; consulta de colecciones del usuario identificado; integración de búsqueda por `q`, orden y paginación; presentación de resultados y del estado vacío; tratamiento de parámetros de listado aplicables.
- **Excluye:** crear, editar, eliminar o cambiar la visibilidad de colecciones; gestión de enlaces y etiquetas; cambios de reglas de negocio, de criterios aprobados o de la infraestructura común de análisis de parámetros; despliegue.
- **Trabajo vecino y responsable de sus entregables:** S8.2–S8.4 cubren operaciones y errores de sus rutas; S8.5 cubre la suite HTTP de la gestión; S8.6–S8.7 cubren las revisiones manuales indicadas en el plan de S8. Esta spec debe demostrar sus propios criterios sin esperar a esos trabajos.

## Dependencias y prerrequisitos

- La infraestructura Web privada y el análisis base de parámetros de S7 están disponibles. El cierre de S7.6 aún registra integración de consumidores de listados pendiente; por ello, el uso del analizador en esta ruta —si resulta adecuado— y su validación pertenecen a esta spec, y no se presupone cerrada la integración anterior.
- S4 aporta el criterio aprobado AC-COL-05. La operación de Application y la consulta de persistencia que consume la página deben estar disponibles para la validación de extremo a extremo; cualquier ausencia o incompatibilidad se registra como bloqueo técnico, sin cambiar el criterio aprobado.
- La identidad de desarrollo de S7 permite ejecutar y probar la ruta privada. No se acepta ningún identificador de usuario proporcionado por la petición como autoridad de acceso.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [AC-COL-05](../../../plans/mvp0-acceptance-criteria.md), [«Rutas privadas»](../../../context/specifications.md) y [«Propiedad de los datos»](../../../context/domain-model.md) | Un usuario identificado accede al listado de colecciones. | La ruta carga y presenta el resultado del listado, con o sin búsqueda. | La respuesta satisface AC-COL-05 y no expone colecciones de otro usuario; la identidad procede del servidor y no de datos de la petición. |
| CA02 | [«Búsqueda y filtros»](../../../context/specifications.md), [«Listados»](../../../context/specifications.md) y [«Rutas privadas»](../../../context/specifications.md) | El usuario envía una búsqueda o navega entre páginas del listado. | La página procesa `q` y `p`, incluidos valores vacíos, repetidos, inválidos o no aplicables. | El listado conserva la semántica, el orden y la paginación de las fuentes; `p` inválido/fuera de rango se ignora y muestra la primera página sin error; el formulario de búsqueda envía a la ruta del listado y reinicia la navegación en la primera página. |
| CA03 | [«Rutas privadas»](../../../context/specifications.md) y [«Listados»](../../../context/specifications.md) | La primera página del resultado no contiene colecciones, bien porque no hay resultados o porque la búsqueda no encuentra coincidencias. | La página representa el resultado. | Se presenta el estado vacío accesible, sin filas de colección ni páginas de resultados inexistentes; no se introduce un texto funcional nuevo no aprobado. |
| CA04 | [requirements.md → «Accesibilidad»](../../../context/requirements.md) | El listado presenta resultados, controles de búsqueda, paginación o estado vacío. | Se revisa la respuesta renderizada de la página. | La estructura y los controles permiten identificar y operar el listado de forma accesible conforme a los requisitos, sin atribuir a esta revisión manual garantías que no se hayan comprobado. |

Los casos negativos y límites de parámetros se validan mediante las referencias de CA02; este trabajo no redefine sus valores ni sus mensajes.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| La regla de `p` en la sección general de rutas privadas distinguía entre inválido/fuera de rango y la sección de paginación pública definía primera página sin error. Se resolvió actualizando el documento propietario para precisar el comportamiento del listado privado de colecciones y el alcance de la regla pública. | [«Rutas privadas»](../../../context/specifications.md) y [«Paginación de las páginas públicas»](../../../context/specifications.md) | CA02 | DLG eligió primera página sin error y autorizó la actualización de `specifications.md` el 25-X-2026. La spec derivada se sincroniza con esa decisión. |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 25-X-2026 («Siguiente»).
- [Tareas y cierre](tasks.md#aceptación-y-cierre), aprobadas por DLG el 25-X-2026 («Vamos») y completadas/aceptadas el 25-X-2026 («Completado»).
