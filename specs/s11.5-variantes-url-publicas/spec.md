# Especificación: S11.5 — Variantes de URL y respuestas 404 públicas

## Estado y aprobación

- Estado: aprobada.
- Aprobada por DLG el 27-X-2026 con la referencia «Siguiente».

## Objetivo

Las variantes equivalentes de las rutas públicas conducen a su forma canónica, mientras que las rutas públicas que ya no representan recursos accesibles mantienen las respuestas `404` definidas por sus fuentes.

## Fuentes

- [requirements.md → «Páginas públicas»](../../context/requirements.md#páginas-públicas)
- [specifications.md → «Rutas públicas»](../../context/specifications.md#rutas-públicas)
- [specifications.md → «Páginas públicas»](../../context/specifications.md#páginas-públicas), en particular [«Página de usuario»](../../context/specifications.md#página-de-usuario), [«Página de colecciones»](../../context/specifications.md#página-de-colecciones), [«Página de colección»](../../context/specifications.md#página-de-colección) y [«Paginación de las páginas públicas»](../../context/specifications.md#paginación-de-las-páginas-públicas)
- [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado)
- [decisions.md → «Producto y alcance»](../../context/decisions.md#producto-y-alcance)
- [mvp0-acceptance-criteria.md → «AC-USER-01 — Página pública de usuario»](../../plans/mvp0-acceptance-criteria.md#ac-user-01--página-pública-de-usuario)
- [mvp0-acceptance-criteria.md → «AC-COLLS-01 — Página pública de colecciones»](../../plans/mvp0-acceptance-criteria.md#ac-colls-01--página-pública-de-colecciones)
- [mvp0-acceptance-criteria.md → «AC-COLLECTION-01 — Página pública de una colección»](../../plans/mvp0-acceptance-criteria.md#ac-collection-01--página-pública-de-una-colección)
- [mvp0-plan.md → «S11: Área pública y SEO (L)»](../../plans/mvp0-plan.md#s11-área-pública-y-seo-l)
- [roadmap.md → «Área pública y SEO»](../../context/roadmap.md#área-pública-y-seo)

## Alcance

- Incluye:
  - Redirigir las variantes equivalentes de las rutas públicas causadas por mayúsculas o por una barra final incorrecta a la ruta canónica en minúsculas y con la forma de barra definida para cada página.
  - Mantener la página solicitada y su canonical de paginación cuando una variante de ruta corresponde a una página de colección paginada.
  - Comprobar las respuestas `404` de alias antiguos, slugs antiguos de colección y colecciones públicas sin enlaces públicos.
- Excluye:
  - El contenido, los metadatos y la canonical propios de cada página, cubiertos por S11.1, S11.2 y S11.3.
  - Fijar un código HTTP concreto para las redirecciones: las fuentes exigen redirigir las variantes, pero no especifican ese código. Las redirecciones `301` están fuera del MVP0 según [decisions.md → «Producto y alcance»](../../context/decisions.md#producto-y-alcance).
  - Las respuestas `404` de otros recursos y estados ya cubiertas por S11.2 y S11.3.
  - La suite transversal de tests de S11.6, la revisión manual general del HTML de S11.7 y la prueba manual de navegación de S11.8.
- Trabajo vecino y responsable de sus entregables: S11.1 implementa la landing; S11.2, la página pública de usuario; S11.3, las páginas públicas de colecciones y colección; S11.4, los atributos transversales; S11.6, los tests generales de la etapa; S11.7 y S11.8, las revisiones humanas correspondientes. Esta spec no amplía esos entregables.

## Dependencias y prerrequisitos

- Las fuentes funcionales y los criterios AC-USER-01, AC-COLLS-01 y AC-COLLECTION-01 definen las rutas, canonical y respuestas públicas aplicables.
- La preparación de esta spec no depende de la finalización de otros trabajos. Para validar las rutas públicas integradas deben estar disponibles las páginas de S11.1–S11.3: S11.1 y S11.3 constan completadas en el [roadmap](../../context/roadmap.md#área-pública-y-seo), mientras que S11.2 sigue en curso.
- S11.4 no es un prerrequisito de esta spec; sus atributos transversales no cambian la normalización de rutas ni las respuestas cubiertas aquí.
- S11.6–S11.8 no son prerrequisitos para cerrar S11.5.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [specifications.md → «Rutas públicas»](../../context/specifications.md#rutas-públicas), [mvp0-plan.md → «S11: Área pública y SEO (L)»](../../plans/mvp0-plan.md#s11-área-pública-y-seo-l) | Una ruta pública existente de usuario, colecciones o colección se solicita con mayúsculas o una barra final distinta de la forma indicada para esa ruta | Se envía una petición a la variante equivalente | La respuesta redirige a la ruta canónica en minúsculas y con la forma de barra definida por la fuente propietaria. No se presupone un código HTTP de redirección que la fuente no haya establecido. |
| CA02 | [specifications.md → «Rutas públicas»](../../context/specifications.md#rutas-públicas), [specifications.md → «Paginación de las páginas públicas»](../../context/specifications.md#paginación-de-las-páginas-públicas), [AC-COLLECTION-01](../../plans/mvp0-acceptance-criteria.md#ac-collection-01--página-pública-de-una-colección) | Una ruta variante de una colección pública corresponde a una página de paginación solicitada | Se sigue la redirección de normalización | Se conserva el resultado de paginación especificado y la canonical corresponde a la página resultante según la fuente propietaria. |
| CA03 | [AC-USER-01](../../plans/mvp0-acceptance-criteria.md#ac-user-01--página-pública-de-usuario), [AC-COLLS-01](../../plans/mvp0-acceptance-criteria.md#ac-colls-01--página-pública-de-colecciones), [AC-COLLECTION-01](../../plans/mvp0-acceptance-criteria.md#ac-collection-01--página-pública-de-una-colección), [decisions.md → «Producto y alcance»](../../context/decisions.md#producto-y-alcance) | La ruta pública usa un alias antiguo tras un cambio de alias, o un slug antiguo tras renombrar la colección | Se solicita la página pública correspondiente | La respuesta es `404`, sin redirigir el alias o slug antiguo a otro recurso. |
| CA04 | [AC-COLLECTION-01](../../plans/mvp0-acceptance-criteria.md#ac-collection-01--página-pública-de-una-colección), [specifications.md → «Página de colección»](../../context/specifications.md#página-de-colección), [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado) | Se solicita una colección pública que no tiene enlaces públicos | Se accede a su ruta pública | La respuesta es `404` y no se expone el contenido de la colección. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para definir el alcance de esta spec. El código HTTP concreto de la redirección no está fijado por las fuentes y queda fuera de estos criterios. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md) — aprobado por DLG el 27-X-2026 («Siguiente»).
- [Tareas](tasks.md) — aprobadas por DLG el 27-X-2026 («Vamos»).
