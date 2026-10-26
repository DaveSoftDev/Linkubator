# Especificación: S8.2 — Operaciones de colecciones

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 25-X-2026 («Siguiente»).

## Objetivo

Completar en la zona privada los flujos Web para crear, editar y eliminar colecciones, consumiendo los casos de uso de Application y respetando la propiedad del usuario identificado.

## Fuentes

- [mvp0-plan.md → «S8: Gestión de colecciones (L)»](../../plans/mvp0-plan.md#s8-gestión-de-colecciones-l): entregable S8.2, trabajo vecino y Definition of Done de S8.
- [Criterios de aceptación de S4 → «AC-COL-01 — Crear colección»](../../plans/mvp0-acceptance-criteria.md#ac-col-01-crear-colección), [«AC-COL-01b — Hacer pública una colección vacía»](../../plans/mvp0-acceptance-criteria.md#ac-col-01b-hacer-pública-una-colección-vacía), [«AC-COL-02 — Editar colección»](../../plans/mvp0-acceptance-criteria.md#ac-col-02-editar-colección), [«AC-COL-04 — Eliminar colección»](../../plans/mvp0-acceptance-criteria.md#ac-col-04-eliminar-colección) y [«AC-COL-04b — Eliminar una colección con enlaces o la última colección»](../../plans/mvp0-acceptance-criteria.md#ac-col-04b-eliminar-una-colección-con-enlaces-o-la-última-colección): criterios aprobados propietarios de las operaciones.
- [requirements.md → «Colecciones»](../../context/requirements.md#colecciones), [«Páginas privadas»](../../context/requirements.md#páginas-privadas) y [«Accesibilidad»](../../context/requirements.md#accesibilidad): capacidades y expectativas de interfaz.
- [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas): rutas, métodos, validación de identificadores, antiforgery y respuestas privadas.
- [domain-model.md → «Collection»](../../context/domain-model.md#collection), [«Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [«Público y privado»](../../context/domain-model.md#público-y-privado): invariantes de la entidad y sus transiciones.
- [architecture.md → «Web»](../../context/architecture.md#web), [«Persistencia»](../../context/architecture.md#persistencia) y [«Arquitectura»](../../context/architecture.md#arquitectura): límites entre capas, protección de formularios y atomicidad.

## Alcance

- **Incluye:** formularios y flujos privados de crear, editar conjuntamente nombre, descripción y visibilidad, y eliminar colecciones; rutas canónicas definidas por las fuentes; uso de los casos de uso de Application; confirmación explícita antes de eliminar; redirecciones y respuestas según las reglas existentes; integración con el listado de S8.1 para acceder a las operaciones.
- **Excluye:** listado, búsqueda y paginación (S8.1); rediseño de las reglas de dominio, contratos de Application o persistencia; gestión de enlaces y etiquetas; la suite HTTP transversal de S8.5; revisiones manuales de S8.6–S8.7; despliegue.
- **Trabajo vecino y responsable de sus entregables:** S8.3 concreta la presentación de errores de validación junto a campos, el foco en el primer campo con error y los errores de dominio; S8.4 cubre el tratamiento de identificadores inválidos y recursos válidos no accesibles; S8.5 cubre las pruebas HTTP de la etapa. Ninguno es prerrequisito para validar y cerrar los criterios de esta spec en sus propios límites. Los criterios de S8.2 verifican el resultado de la operación y su respuesta inmediata, sin anticipar la evidencia de esas tareas vecinas.

## Dependencias y prerrequisitos

- S8.1 proporciona el listado privado de colecciones desde el que se enlazan las operaciones; su implementación y aceptación constan en [S8.1 → «Aceptación y cierre»](../archive/s8.1-listado-colecciones/tasks.md#aceptación-y-cierre).
- Los casos de uso de colecciones y sus contratos de Application existen como base para las páginas Web. La implementación deberá comprobar sus firmas y resultados antes de cerrar el diseño técnico; una incompatibilidad concreta se registrará como bloqueo técnico, sin alterar los criterios aprobados.
- La identidad privada y las respuestas base de Web están disponibles en la infraestructura existente. Las operaciones no pueden depender de una identidad enviada por el cliente.
- No se requiere que S8.3, S8.4, S8.5 ni etapas posteriores aporten código, tests o evidencia para cerrar esta spec.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [AC-COL-01](../../plans/mvp0-acceptance-criteria.md#ac-col-01-crear-colección), [AC-COL-01b](../../plans/mvp0-acceptance-criteria.md#ac-col-01b-hacer-pública-una-colección-vacía) y [«Rutas privadas»](../../context/specifications.md#rutas-privadas) | Un usuario identificado abre el formulario de nueva colección y presenta datos válidos, indicando el estado público si corresponde. | Envía el formulario por el método de escritura de la ruta. | Se crea una sola colección para el usuario identificado, con los valores y el estado acordes a los criterios propietarios, y se completa la respuesta Web prevista para una operación correcta. Los datos de usuario no determinan el propietario. |
| CA02 | [AC-COL-02](../../plans/mvp0-acceptance-criteria.md#ac-col-02-editar-colección), [«Collection»](../../context/domain-model.md#collection), [«Público y privado»](../../context/domain-model.md#público-y-privado) y [«Rutas privadas»](../../context/specifications.md#rutas-privadas) | El usuario identificado abre el único formulario de edición de una colección propia. | El formulario carga y permite enviar nombre, descripción y estado destino; se prueban valores válidos y los casos de nombre o estado inválidos. | Una edición válida actualiza los tres campos por el mismo flujo; el nombre renueva el slug conforme a su criterio propietario, la transición de visibilidad cumple las invariantes y se confirma la atomicidad. Una edición rechazada no modifica la colección ni sus enlaces. |
| CA03 | [AC-COL-04](../../plans/mvp0-acceptance-criteria.md#ac-col-04-eliminar-colección), [AC-COL-04b](../../plans/mvp0-acceptance-criteria.md#ac-col-04b-eliminar-una-colección-con-enlaces-o-la-última-colección) y [«Rutas privadas»](../../context/specifications.md#rutas-privadas) | El usuario identificado abre la página canónica de eliminación de una colección propia. | Realiza GET para revisar la confirmación o envía el POST de confirmación. | GET solo muestra la confirmación y no modifica datos; POST elimina únicamente si la operación propietaria permite el borrado y, si se rechaza por una regla de dominio, conserva los datos y presenta el resultado correspondiente. La confirmación no ejecuta la eliminación por sí sola. |
| CA04 | [«Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos), [AC-COL-04](../../plans/mvp0-acceptance-criteria.md#ac-col-04-eliminar-colección) y [«Rutas privadas»](../../context/specifications.md#rutas-privadas) | Las páginas o acciones reciben un identificador inválido o uno válido que no existe o pertenece a otra persona. | Se solicita la operación GET o POST correspondiente. | No se lee ni modifica información ajena. Los identificadores inválidos y los recursos válidos no accesibles reciben las respuestas diferenciadas y genéricas que definen las fuentes; no se revela cuál de las causas de inaccesibilidad ocurrió. |
| CA05 | [«Rutas privadas»](../../context/specifications.md#rutas-privadas), [«Web»](../../context/architecture.md#web) y [«Accesibilidad»](../../context/requirements.md#accesibilidad) | Se envía un formulario que modifica una colección, o se accede a una página canónica de formulario. | Se inspeccionan el método, la protección y el HTML de respuesta. | Las modificaciones requieren POST con antiforgery; ningún GET modifica datos. Labels, encabezados, navegación y mensajes expuestos por esta entrega conservan la estructura accesible exigida. La presentación detallada y la revisión manual de errores quedan en sus tareas propietarias S8.3 y S8.7, sin bloquear este criterio. |

Los casos negativos y los límites de nombres, slug, estado destino, propiedad, reglas de eliminación y métodos se verifican contra las fuentes enlazadas; esta spec no redefine sus valores, textos ni resultados.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado en la preparación de esta especificación. Las firmas actuales de Application y los puntos de integración Web se confirmarán durante el diseño técnico, después de aprobar esta spec. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 25-X-2026 («Siguiente»).
- [Tareas, evidencia y aceptación](tasks.md#aceptación-y-cierre): aceptación humana «Completado» registrada el 25-X-2026; cierre técnico completado el 26-X-2026 tras validar CA01–CA05.
