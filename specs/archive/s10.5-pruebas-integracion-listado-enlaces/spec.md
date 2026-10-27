# Especificación: S10.5 — Pruebas de integración del listado de enlaces

## Estado y aprobación

- Estado: aprobada.
- Preparada el 27-X-2026 por el asistente, a petición de DLG («Crea la spec S10.5»).
- Aprobación: DLG, 27-X-2026 («Siguiente»), decisión explícita en esta conversación.

## Objetivo

Disponer de pruebas automatizadas HTTP y de datos que comprueben las reglas aprobadas para el listado privado de enlaces, incluyendo la búsqueda combinada con el filtro de colección, el ranking, los límites de página, la propiedad por entidad y el aislamiento entre usuarios.

## Fuentes

- [mvp0-plan.md → «S10: Listado y búsqueda de enlaces (L)»](../../../plans/mvp0-plan.md#s10-listado-y-búsqueda-de-enlaces-l): asignación de S10.5 y Definition of Done de S10.
- [AC-LINK-06 — Listar enlaces del usuario](../../../plans/mvp0-acceptance-criteria.md#ac-link-06-listar-enlaces-del-usuario): criterio aprobado propietario del listado, búsqueda, filtro y paginación.
- [AC-TRANS-01 — Identidad, propiedad y límites de acceso](../../../plans/mvp0-acceptance-criteria.md#ac-trans-01-identidad-propiedad-y-límites-de-acceso): criterio aprobado propietario de identidad y propiedad por usuario.
- [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros), [«Listados»](../../../context/specifications.md#listados) y [«Rutas privadas»](../../../context/specifications.md#rutas-privadas): reglas propietarias para búsqueda, ranking, filtro, paginación y análisis de parámetros.
- [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos): aislamiento de datos por usuario y relaciones.
- [architecture.md → «FTS5»](../../../context/architecture.md#fts5): proyección técnica de búsqueda que debe observarse mediante el comportamiento del caso de uso.
- [S10.1 — Listado y búsqueda de enlaces](../s10.1-listado-enlaces/spec.md), [S10.2 — Parámetros del listado de enlaces](../s10.2-parametros-listado-enlaces/spec.md), [S10.3 — Conservar parámetros del listado de enlaces](../s10.3-conservar-parametros-listado-enlaces/spec.md) y [S10.4 — Conectar el buscador del dashboard](../s10.4-buscador-dashboard/spec.md): flujos de listado y navegación ya implementados que esta suite debe validar de forma transversal, sin reimplementar.

## Alcance

- **Incluye:** pruebas HTTP y de datos derivadas de los criterios aprobados para verificar búsqueda con filtro de colección, orden por relevancia, tamaño de página y paginación, aislamiento por tipo de entidad y por usuario, y comportamiento de parámetros del listado aplicable al conjunto de pruebas de enlaces. El tamaño de página sigue [«Listados»](../../../context/specifications.md#listados), y la descripción de S10.5 en el plan de implementación se corrigió a esa fuente por decisión de DLG el 27-X-2026 («follow_functional» en esta conversación).
- **Excluye:** cambiar reglas funcionales o comportamiento productivo del listado; pruebas unitarias de componentes ya cubiertos por S10.1–S10.3; pruebas visuales o automatización de navegador; revisión manual con los datos de ejemplo (S10.6); gestión y filtrado por etiquetas; cambios de infraestructura, despliegue o arquitectura de búsqueda que no sean necesarios para añadir la cobertura asignada.
- **Trabajo vecino y responsable de sus entregables:** S10.1–S10.4 son responsables de implementar el listado, parámetros, conservación de navegación y conexión desde el dashboard; S10.6 es responsable de la revisión manual con datos de ejemplo. La suite de esta spec no sustituye la revisión manual ni queda pendiente de su resultado.

## Dependencias y prerrequisitos

- S10.1, S10.2, S10.3 y S10.4 están completadas y aceptadas; sus rutas y comportamientos son candidatos a verificación, no se reabren por defecto.
- Están disponibles el catálogo aprobado de criterios de S4, las fuentes funcionales enlazadas y el proyecto de tests de integración existente.
- La suite debe poder cerrarse mediante las pruebas automatizadas y la revisión técnica que pertenezcan a esta especificación. No depende de S10.6 ni de una aceptación de la etapa completa S10.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [AC-LINK-06](../../../plans/mvp0-acceptance-criteria.md#ac-link-06-listar-enlaces-del-usuario), [«Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros), [«Listados»](../../../context/specifications.md#listados) | Un usuario identificado tiene enlaces propios con coincidencias de texto en más de una colección, además de datos que no coinciden. | Consulta el listado con `q` y un filtro `c` válido. | El resultado contiene únicamente sus enlaces que satisfacen la búsqueda y el filtro, sin ampliar el resultado a otra entidad o a otro usuario. |
| CA02 | [AC-LINK-06](../../../plans/mvp0-acceptance-criteria.md#ac-link-06-listar-enlaces-del-usuario), [«Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros), [«Listados»](../../../context/specifications.md#listados) | Un usuario identificado obtiene varios enlaces coincidentes con distintos grados de relevancia y posibles empates. | Consulta el listado con búsqueda válida. | Los enlaces se ordenan según el ranking y los desempates de la fuente propietaria; el resultado no depende del orden accidental de inserción de los datos de prueba. |
| CA03 | [AC-LINK-06](../../../plans/mvp0-acceptance-criteria.md#ac-link-06-listar-enlaces-del-usuario), [«Listados»](../../../context/specifications.md#listados), [«Rutas privadas»](../../../context/specifications.md#rutas-privadas) | El usuario tiene suficientes resultados para llenar más de una página, y se contemplan una página válida y otra fuera del rango de resultados. | Solicita las páginas en el listado de enlaces. | La respuesta respeta el tamaño de página y los límites definidos por la fuente; el caso fuera de rango produce el resultado prescrito sin filtrar datos ni romper la respuesta. |
| CA04 | [AC-LINK-06](../../../plans/mvp0-acceptance-criteria.md#ac-link-06-listar-enlaces-del-usuario), [«Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros), [«Listados»](../../../context/specifications.md#listados) | Existe un texto que coincide con un enlace y con una colección, con resultados de ambas clases. | Se consulta cada listado correspondiente. | Cada lista devuelve solo su propia entidad; las coincidencias de colección no aparecen como enlaces ni las coincidencias de enlace como colecciones. |
| CA05 | [AC-LINK-06](../../../plans/mvp0-acceptance-criteria.md#ac-link-06-listar-enlaces-del-usuario), [AC-TRANS-01](../../../plans/mvp0-acceptance-criteria.md#ac-trans-01-identidad-propiedad-y-límites-de-acceso), [«Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos) | Dos usuarios tienen datos con términos coincidentes, y los identificadores pueden coincidir entre sus ámbitos. | Uno de ellos consulta el listado y aplica búsqueda, filtro de colección o paginación. | Solo se devuelven enlaces y relaciones que pertenecen al usuario identificado; datos ajenos no aparecen ni se usan para decidir resultados, y la identidad no se obtiene de parámetros controlados por el cliente. |
| CA06 | [AC-LINK-06](../../../plans/mvp0-acceptance-criteria.md#ac-link-06-listar-enlaces-del-usuario), [«Rutas privadas»](../../../context/specifications.md#rutas-privadas), [«Listados»](../../../context/specifications.md#listados), [S10.2](../s10.2-parametros-listado-enlaces/spec.md), [S10.3](../s10.3-conservar-parametros-listado-enlaces/spec.md) | Una petición de listado cubre los casos negativos y límites expresamente definidos para enlaces, y el formulario o paginador genera navegación. | Se prueban el análisis de parámetros y la navegación en el límite HTTP. | Las respuestas a parámetros inválidos o fuera de rango, filtros de colección inexistentes o ajenos, parámetros repetidos/no aplicables, consultas vacías o excedidas y conservación de filtros son las establecidas por las fuentes propietarias, sin redefinirlas en esta especificación. |

CA06 se limita a los parámetros del listado de enlaces y a los casos definidos por las fuentes enlazadas; no asigna a esta suite filtros de etiquetas ni funcionalidades reservadas a otras etapas. En la revisión documental se comprobó que AC-LINK-06 describe los casos del listado de enlaces, mientras que la excepción del parámetro `p` inválido para colecciones está delimitada a ese listado en la fuente funcional. La revisión visual y exploratoria con datos de ejemplo no forma parte de estos criterios.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado. La discrepancia del tamaño de página se resolvió: DLG indicó conservar la regla funcional vigente y actualizar S10.5 en el plan de implementación; el cambio está aplicado. La revisión confirmó que la excepción de `p` para colecciones no contradice el alcance AC-LINK-06 de enlaces. | [mvp0-plan.md → S10.5](../../../plans/mvp0-plan.md#s10-listado-y-búsqueda-de-enlaces-l), [«Listados»](../../../context/specifications.md#listados), [«Rutas privadas»](../../../context/specifications.md#rutas-privadas), [AC-LINK-06](../../../plans/mvp0-acceptance-criteria.md#ac-link-06-listar-enlaces-del-usuario) | — | — |
| Los casos concretos de test, fixtures y comandos se definirán en `plan.md` después de aprobar esta especificación. | — | CA01–CA06 | Requiere aprobación de la especificación antes de preparar el plan. |

## Artefactos relacionados

No hay otros artefactos SDD de S10.5 creados todavía. El plan técnico podrá prepararse tras la aprobación explícita de esta especificación.
