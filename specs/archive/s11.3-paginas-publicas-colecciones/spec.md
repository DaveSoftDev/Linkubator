# Especificación: S11.3 — Páginas públicas de colecciones

## Estado y aprobación

- Estado: aprobada.
- Aprobada por DLG el 27-X-2026 con la referencia «Siguiente».

## Objetivo

Las rutas públicas de colecciones y de colección presentan el contenido público correspondiente, con las respuestas, el orden, la paginación y los metadatos definidos por sus fuentes.

## Fuentes

- [requirements.md → «Páginas públicas»](../../../context/requirements.md#páginas-públicas)
- [specifications.md → «Páginas públicas»](../../../context/specifications.md#páginas-públicas), en particular [«Página de colecciones»](../../../context/specifications.md#página-de-colecciones), [«Página de colección»](../../../context/specifications.md#página-de-colección) y [«Paginación de las páginas públicas»](../../../context/specifications.md#paginación-de-las-páginas-públicas)
- [specifications.md → «Listados»](../../../context/specifications.md#listados)
- [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado)
- [architecture.md → «Enrutamiento»](../../../context/architecture.md#enrutamiento)
- [mvp0-acceptance-criteria.md → «AC-COLLS-01 — Página pública de colecciones»](../../../plans/mvp0-acceptance-criteria.md#ac-colls-01--página-pública-de-colecciones)
- [mvp0-acceptance-criteria.md → «AC-COLLECTION-01 — Página pública de una colección»](../../../plans/mvp0-acceptance-criteria.md#ac-collection-01--página-pública-de-una-colección)
- [mvp0-plan.md → «S11: Área pública y SEO (L)»](../../../plans/mvp0-plan.md#s11-área-pública-y-seo-l)
- [roadmap.md → «Área pública y SEO»](../../../context/roadmap.md#área-pública-y-seo)

## Alcance

- Incluye:
  - La página pública con todas las colecciones del usuario que se muestran en público.
  - La página pública de una colección y sus enlaces públicos.
  - El contenido, orden, estados vacíos, respuestas `404`, metadatos y canonical de ambas páginas.
  - La paginación `p` de la página de colección y la canonical correspondiente a la página servida.
  - El renderizado en servidor de ambas páginas.
- Excluye:
  - La landing pública y la página pública de usuario, cubiertas respectivamente por S11.1 y S11.2.
  - La aplicación transversal de atributos `rel`, `target`, `alt`, encabezados e idioma, asignada a S11.4.
  - Las redirecciones transversales de variantes de URL y el tratamiento común de alias antiguos, asignados a S11.5. Esta exclusión no sustituye las respuestas `404` específicas exigidas por los criterios de esta spec.
  - La escritura de la suite transversal de tests de S11.6 y el SEO avanzado fuera del alcance comprometido.
- Trabajo vecino y responsable de sus entregables: S11.1 implementa la landing; S11.2, la página pública de usuario; S11.4, los atributos y aspectos transversales de presentación; S11.5, las variantes de URL; S11.6, los tests derivados de los criterios aprobados. Los entregables vecinos no bloquean el cierre de esta spec.

## Dependencias y prerrequisitos

- Los criterios aprobados [AC-COLLS-01](../../../plans/mvp0-acceptance-criteria.md#ac-colls-01--página-pública-de-colecciones) y [AC-COLLECTION-01](../../../plans/mvp0-acceptance-criteria.md#ac-collection-01--página-pública-de-una-colección), junto con sus fuentes funcionales, definen el resultado observable.
- S11 está activa según el [roadmap](../../../context/roadmap.md#área-pública-y-seo). La preparación de esta spec no depende de que S11.2 u otra tarea vecina haya terminado.
- No se identifican dependencias adicionales necesarias para definir este alcance.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [AC-COLLS-01](../../../plans/mvp0-acceptance-criteria.md#ac-colls-01--página-pública-de-colecciones), [specifications.md → «Página de colecciones»](../../../context/specifications.md#página-de-colecciones), [specifications.md → «Listados»](../../../context/specifications.md#listados), [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado) | Un alias existente con colecciones que se muestran en público, o sin ellas | Se solicita `/{alias}/colecciones/` | Se muestran todas y solo las colecciones que se muestran en público, con nombre, descripción y enlace, en el orden establecido. Si no hay ninguna, aparece el estado vacío definido; si el alias no existe, la respuesta es `404`. La página presenta los metadatos, encabezado y canonical definidos por su fuente. |
| CA02 | [AC-COLLECTION-01](../../../plans/mvp0-acceptance-criteria.md#ac-collection-01--página-pública-de-una-colección), [specifications.md → «Página de colección»](../../../context/specifications.md#página-de-colección), [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado) | Una colección identificada por alias y slug, pública o no accesible públicamente | Se solicita `/{alias}/colecciones/{slug}` | Una colección pública con enlaces públicos muestra solo esos enlaces, con el contenido y orden definidos; una colección privada, inexistente, solicitada mediante un slug anterior distinto del actual o sin enlaces públicos responde `404`, sin exponer su contenido privado. La página presenta los metadatos y encabezado definidos por su fuente. |
| CA03 | [AC-COLLECTION-01](../../../plans/mvp0-acceptance-criteria.md#ac-collection-01--página-pública-de-una-colección), [specifications.md → «Paginación de las páginas públicas»](../../../context/specifications.md#paginación-de-las-páginas-públicas), [specifications.md → «Listados»](../../../context/specifications.md#listados) | Una colección pública con enlaces públicos y una solicitud con o sin `p` | Se solicita una página de la colección | Se muestra la página resultante de interpretar `p` según la fuente, y la canonical corresponde a esa página normalizada. Las páginas posteriores a la primera conservan el orden definido y la paginación establecida. |
| CA04 | [specifications.md → «Páginas públicas»](../../../context/specifications.md#páginas-públicas), [architecture.md → «Enrutamiento»](../../../context/architecture.md#enrutamiento) | Cualquiera de las dos páginas públicas puede responder con contenido | Se solicita su ruta | El contenido de la página se entrega renderizado en servidor. |

El requisito aprobado AC-COLLECTION-01 también menciona atributos transversales de enlaces externos; su aplicación uniforme corresponde a S11.4 y no condiciona el cierre de esta spec.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para el alcance de esta spec. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md) — aprobado por DLG el 27-X-2026 («Siguiente»).
- [Tareas](tasks.md) — aprobadas por DLG el 27-X-2026 («Siguiente»); implementación completada y aceptación humana registrada en [«Aceptación y cierre»](tasks.md#aceptación-y-cierre).
