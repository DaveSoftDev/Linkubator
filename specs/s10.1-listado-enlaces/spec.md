# Especificación: S10.1 — Listado y búsqueda de enlaces

## Estado y aprobación

- Estado: aprobada.
- Preparada el 27-X-2026 por el asistente, a petición de DLG («Crea la spec S10.1»).
- Aprobación: DLG, 27-X-2026 («Siguiente»), decisión explícita en esta conversación.

## Objetivo

Completar el listado privado de enlaces en `/app/enlaces` para presentar resultados y estado vacío, y mostrar de forma accesible el aviso de rechazo de privacidad que llega desde la creación de enlaces, manteniendo la búsqueda, el filtro de colección y la paginación definidos por las fuentes aprobadas.

## Fuentes

- [mvp0-plan.md → «S10: Listado y búsqueda de enlaces (L)»](../../plans/mvp0-plan.md#s10-listado-y-búsqueda-de-enlaces-l): alcance de S10.1, entrega anterior y Definition of Done de S10.
- [Criterios de aceptación del MVP0 → «AC-LINK-06 — Listar enlaces del usuario»](../../plans/mvp0-acceptance-criteria.md#ac-link-06-listar-enlaces-del-usuario): criterio aprobado propietario del listado.
- [requirements.md → «Búsqueda y filtros»](../../context/requirements.md#búsqueda-y-filtros) y [«Accesibilidad»](../../context/requirements.md#accesibilidad): capacidades y expectativas de la interfaz.
- [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros), [«Listados»](../../context/specifications.md#listados), [«Rutas privadas»](../../context/specifications.md#rutas-privadas) y [«Errores controlados»](../../context/specifications.md#errores-controlados): comportamiento del listado, parámetros, estados y presentación accesible del aviso.
- [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [«Público y privado»](../../context/domain-model.md#público-y-privado): aislamiento por usuario y privacidad de los enlaces.
- [architecture.md → «Web»](../../context/architecture.md#web), [«Application»](../../context/architecture.md#application) y [«FTS5»](../../context/architecture.md#fts5): límites de presentación, aplicación y búsqueda.
- [S9.2 — Publicación de enlaces en colecciones privadas](../s9.2-publicacion-enlaces/spec.md): origen del aviso cuya presentación accesible queda pendiente.
- [S9.3 — Gestionar enlaces](../s9.3-gestionar-enlaces/spec.md) y [S7.5 — Componentes de presentación accesible](../s7.5-componentes-presentacion/spec.md): presentación existente de tarjetas/lista y componentes disponibles, sin ampliar sus alcances.

## Alcance

- **Incluye:** completar la ruta privada `/app/enlaces` con búsqueda textual y filtro por colección, orden, paginación y estado vacío según las fuentes propietarias; presentar accesiblemente el aviso de rechazo de privacidad transportado desde el POST de creación; conservar la navegación y presentación ya disponibles donde sean aplicables.
- **Excluye:** las reglas especiales de parámetros inválidos, fuera de rango, inexistentes o ajenos y el parámetro `e` (S10.2); conservación específica de filtros al buscar o paginar y descarte de `p` en el formulario (S10.3); conexión del buscador del dashboard (S10.4); tests HTTP y de datos derivados del criterio aprobado, asignados a S10.5; prueba manual con datos de ejemplo (S10.6); creación, edición y eliminación de enlaces, revisión manual general, nuevas reglas de producto y cambios de infraestructura.
- **Trabajo vecino y responsable de sus entregables:** S9.2 transporta el aviso de rechazo hasta `/app/enlaces`; S9.3 aporta las operaciones de gestión; S7.5 aporta los componentes base. S10.2–S10.6 son trabajos separados de la etapa S10. El criterio de esta especificación se limita a la integración y presentación del aviso y no espera esas etapas para poder cerrarse.

## Dependencias y prerrequisitos

- S9.2 está completada y aceptada; su POST manipulado rechaza la creación, redirige a `/app/enlaces` y transporta el aviso en TempData. S10.1 verifica que la página destino lo muestra de forma perceptible y accesible; no vuelve a implementar el rechazo.
- La ruta privada y la presentación inicial del listado de enlaces están disponibles como entrega parcial anticipada de S10, según [roadmap.md → «Next»](../../context/roadmap.md#next).
- S9.3 y S7.5 están completadas y aceptadas; sus resultados pueden reutilizarse sin reabrirlas.
- Los criterios de esta especificación se pueden validar en el trabajo propio y no dependen de los tests, la revisión manual ni la implementación de S10.2–S10.6.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [AC-LINK-06](../../plans/mvp0-acceptance-criteria.md#ac-link-06-listar-enlaces-del-usuario), [«Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros), [«Listados»](../../context/specifications.md#listados), [«Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) | Un usuario identificado accede al listado privado con los datos de enlaces que le pertenecen. | Se renderiza `/app/enlaces` sin un criterio de búsqueda, o con una búsqueda válida y un filtro de colección aplicable. | Se presentan únicamente enlaces del usuario identificado, en el orden previsto para el caso y con la paginación correspondiente; la búsqueda y el filtro no mezclan entidades ni datos de otros usuarios. Los casos negativos y límites de parámetros permanecen en el trabajo propietario S10.2 y no amplían este criterio. |
| CA02 | [«Páginas privadas»](../../context/specifications.md#páginas-privadas), [«Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros), [«Accesibilidad»](../../context/requirements.md#accesibilidad) | El resultado del listado no contiene enlaces que mostrar. | Se renderiza la primera página del resultado. | Se presenta un estado vacío comprensible y accesible; no se muestra una página de resultados como si contuviera elementos. |
| CA03 | [S9.2 → CA03](../s9.2-publicacion-enlaces/spec.md#criterios-de-aceptación), [«Rutas privadas»](../../context/specifications.md#rutas-privadas), [«Errores controlados»](../../context/specifications.md#errores-controlados) y [«Accesibilidad»](../../context/requirements.md#accesibilidad) | El POST de creación rechazado por intentar hacer público un enlace de una colección privada ha redirigido al listado y transportado el aviso. | El usuario llega a `/app/enlaces`. | El aviso se presenta de forma perceptible y accesible para tecnologías de asistencia en el listado, sin repetir la operación, crear el enlace ni alterar su privacidad. La presentación consume o conserva el aviso conforme al mecanismo Web existente, sin revelar información adicional. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para redactar la especificación. El diseño técnico y las comprobaciones concretas se detallarán en `plan.md` tras la aprobación de esta spec. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md): propuesta generada tras aprobar la especificación; aprobación pendiente.
