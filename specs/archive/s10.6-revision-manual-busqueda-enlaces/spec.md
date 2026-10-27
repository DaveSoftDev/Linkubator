# Especificación: S10.6 — Revisión manual de búsqueda de enlaces

## Estado y aprobación

- Estado: aprobada.
- Preparada el 27-X-2026 por el asistente, a petición de DLG («Crea la spec S10.6»).
- Aprobación: DLG, 27-X-2026 («Siguiente»), decisión explícita en esta conversación.

## Objetivo

Registrar una revisión manual reproducible de la búsqueda privada de enlaces con los datos locales de ejemplo, contrastando la experiencia Web con las reglas aprobadas de texto, operadores literales, filtro por colección y paginación.

## Fuentes

- [mvp0-plan.md → «S10: Listado y búsqueda de enlaces (L)»](../../../plans/mvp0-plan.md#s10-listado-y-búsqueda-de-enlaces-l): asignación y escenarios de S10.6.
- [S6.8 — Datos locales de ejemplo para búsqueda](../s6.8-datos-local-busqueda/spec.md): origen y propósito de los datos de prueba locales.
- [S10.1 — Listado y búsqueda de enlaces](../s10.1-listado-enlaces/spec.md), [S10.2 — Parámetros del listado de enlaces](../s10.2-parametros-listado-enlaces/spec.md), [S10.3 — Conservar parámetros del listado de enlaces](../s10.3-conservar-parametros-listado-enlaces/spec.md) y [S10.4 — Conectar el buscador del dashboard](../s10.4-buscador-dashboard/spec.md): flujos Web que se observarán, ya implementados y aceptados.
- [S10.5 — Pruebas de integración del listado de enlaces](../s10.5-pruebas-integracion-listado-enlaces/spec.md): cobertura automatizada complementaria; no sustituye la revisión manual.
- [requirements.md → «Búsqueda y filtros»](../../../context/requirements.md#búsqueda-y-filtros): capacidades de búsqueda privadas.
- [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros) y [«Listados»](../../../context/specifications.md#listados): comportamiento de la búsqueda, relevancia y paginación.
- [specifications.md → «Rutas privadas»](../../../context/specifications.md#rutas-privadas): ruta y parámetros del listado privado de enlaces.
- [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos): aislamiento por identidad.

## Alcance

- **Incluye:** recorrer manualmente el listado privado de enlaces con los datos locales de ejemplo para comprobar búsquedas con acentos y formas Unicode equivalentes, varios términos, operadores literales, filtro de colección y paginación; registrar los resultados observados por escenario.
- **Excluye:** preparar o modificar el seed local y la base de ejemplo (S6.8); reconstruir proyecciones FTS5 (S6.6/S6.9); cambiar código, datos de producto o reglas funcionales; repetir pruebas automatizadas de S10.5; auditoría visual/general de interfaz, compatibilidad de navegadores o accesibilidad fuera de los escenarios enumerados; búsquedas de colecciones, etiquetas o gestión de etiquetas.
- **Trabajo vecino y responsable de sus entregables:** S6.8 proporciona los datos locales; S10.1–S10.4 proporcionan el listado y los flujos de búsqueda; S10.5 aporta las pruebas automatizadas. S10.6 observa su comportamiento manualmente y no modifica ni vuelve a implementar esos entregables.

## Dependencias y prerrequisitos

- S6.8 está archivada y aceptada; su base y seed local de ejemplo se prepararon para servir, entre otros, a S10.6. La existencia actual y validez de la base en el entorno de revisión se confirmarán en el plan, sin recrearla como parte de esta spec.
- S10.1–S10.4 están implementadas y aceptadas; el listado privado, parámetros aplicables, navegación y conexión desde el dashboard están disponibles.
- S10.5 cuenta con especificación y plan aprobados y tareas pendientes de aprobación; sus tests no son prerrequisito para esta revisión manual ni condicionan su cierre.
- Se requiere un entorno Web local que utilice la base preparada, la identidad de desarrollo y un navegador. La revisión puede cerrarse con su propia evidencia manual, sin esperar otros trabajos ni la aceptación de toda la etapa S10.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [S6.8 CA02](../s6.8-datos-local-busqueda/spec.md#criterios-de-aceptación), [«Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros) | El listado privado dispone de los datos locales preparados para contrastar acentos y formas Unicode equivalentes. | Se realizan búsquedas con las formas previstas en el conjunto de ejemplo. | Los resultados observados coinciden con la equivalencia definida por la fuente, sin afirmar que cambien los valores originales almacenados. |
| CA02 | [S6.8 CA02](../s6.8-datos-local-busqueda/spec.md#criterios-de-aceptación), [«Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros) | Hay enlaces de ejemplo cuyos textos contienen términos distinguibles y secuencias que parecen operadores. | Se busca con varios términos y con operadores literales incluidos en la consulta. | Se observan únicamente los enlaces que satisfacen la semántica fuente de todos los términos; las palabras de operador no se interpretan como instrucciones de búsqueda. |
| CA03 | [S6.8 CA02](../s6.8-datos-local-busqueda/spec.md#criterios-de-aceptación), [«Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros), [«Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos) | Los datos de ejemplo distribuyen enlaces coincidentes entre colecciones. | Se busca un término con y sin el filtro de una colección propia. | Sin filtro se muestran las coincidencias aplicables del listado; con filtro solo aparecen los enlaces de la colección seleccionada, sin exponer datos ajenos. |
| CA04 | [S6.8 CA02](../s6.8-datos-local-busqueda/spec.md#criterios-de-aceptación), [«Listados»](../../../context/specifications.md#listados), [«Rutas privadas»](../../../context/specifications.md#rutas-privadas) | Los datos de ejemplo contienen más resultados que los que caben en una página. | Se navega entre páginas del resultado, manteniendo la consulta y el filtro cuando corresponda. | Los elementos observados corresponden al resultado filtrado y ordenado, la paginación presenta páginas coherentes y los controles conservan los filtros aplicables conforme a las fuentes. |

Los resultados concretos, incluidos aciertos o discrepancias, se registran como observaciones de revisión; esta especificación no redefine reglas, mensajes, límites ni contenido del seed. Ante una discrepancia se documenta el escenario y se detiene cualquier cambio de producto.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para redactar la spec. La disponibilidad de la base local, el entorno Web y el navegador se comprobará en el plan antes de ejecutar la revisión; una indisponibilidad se documentará como bloqueo de ejecución, no como cambio de alcance. | [S6.8](../s6.8-datos-local-busqueda/spec.md), [S10 del plan](../../../plans/mvp0-plan.md#s10-listado-y-búsqueda-de-enlaces-l) | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md): propuesta generada tras aprobar la especificación; aprobación pendiente.
