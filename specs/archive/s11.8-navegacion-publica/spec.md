# Especificación: S11.8 — Prueba manual de navegación pública

## Estado y aprobación

- Estado: aprobada.
- Aprobada por DLG el 27-X-2026 con la referencia «Siguiente».

## Objetivo

Comprobar manualmente que la navegación pública refleja los estados público y privado guardados para las colecciones y los enlaces, y que no expone contenido privado.

## Fuentes

- [requirements.md → «Páginas públicas»](../../../context/requirements.md#páginas-públicas)
- [specifications.md → «Páginas públicas»](../../../context/specifications.md#páginas-públicas), en particular [«Página de usuario»](../../../context/specifications.md#página-de-usuario), [«Página de colecciones»](../../../context/specifications.md#página-de-colecciones) y [«Página de colección»](../../../context/specifications.md#página-de-colección)
- [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado)
- [mvp0-acceptance-criteria.md → «AC-LINK-02 — Editar enlace»](../../../plans/mvp0-acceptance-criteria.md#ac-link-02--editar-enlace)
- [mvp0-acceptance-criteria.md → «AC-COLLS-01 — Página pública de colecciones»](../../../plans/mvp0-acceptance-criteria.md#ac-colls-01--página-pública-de-colecciones)
- [mvp0-acceptance-criteria.md → «AC-COLLECTION-01 — Página pública de una colección»](../../../plans/mvp0-acceptance-criteria.md#ac-collection-01--página-pública-de-una-colección)
- [mvp0-plan.md → «S11: Área pública y SEO (L)»](../../../plans/mvp0-plan.md#s11-área-pública-y-seo-l)
- [roadmap.md → «Área pública y SEO»](../../../context/roadmap.md#área-pública-y-seo)

## Alcance

- Incluye:
  - La navegación manual desde las páginas públicas del usuario y de colecciones hasta una colección pública.
  - La comprobación de que los estados seleccionados y guardados en la edición de colecciones y enlaces se reflejan en las páginas públicas.
  - La comprobación de que las colecciones y los enlaces privados no se muestran ni revelan su contenido mediante la navegación pública.
- Excluye:
  - La implementación o modificación de páginas públicas, formularios y reglas de publicación.
  - La suite automatizada de integración HTTP, cubierta por S11.6.
  - La revisión manual de metadatos, encabezados, idioma, atributos `alt` y textos del HTML, cubierta por S11.7.
  - La definición de reglas de privacidad o de presentación no establecidas por las fuentes enlazadas.
- Trabajo vecino y responsable de sus entregables: S11.1–S11.5 implementan las páginas públicas y sus comportamientos; S11.6 cubre las pruebas automatizadas HTTP; S11.7 revisa el HTML renderizado. Esta spec cubre únicamente la comprobación manual de navegación indicada para S11.8.

## Dependencias y prerrequisitos

- La ejecución requiere que estén disponibles las páginas públicas de usuario, colecciones y colección, las rutas públicas y los formularios de edición necesarios para seleccionar y guardar los estados que se comprobarán.
- La preparación de esta especificación no depende de que esos trabajos estén terminados. Según el [roadmap](../../../context/roadmap.md#área-pública-y-seo), S11.2 y S11.4–S11.5 siguen en curso; por ello, la prueba manual se ejecutará cuando sus páginas y comportamientos necesarios estén disponibles.
- S11.6 y S11.7 son trabajos separados: sus resultados no sustituyen la comprobación manual de navegación de esta spec ni son evidencia de sus criterios.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [AC-COLLS-01](../../../plans/mvp0-acceptance-criteria.md#ac-colls-01--página-pública-de-colecciones), [«Página de usuario»](../../../context/specifications.md#página-de-usuario), [«Página de colecciones»](../../../context/specifications.md#página-de-colecciones), [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado) | Un usuario con colecciones públicas y privadas, incluidas colecciones públicas sin enlaces públicos | Se recorren su página pública y la página pública de colecciones | Solo aparecen las colecciones que cumplen las condiciones de publicación de la fuente; las privadas y las que no cumplen esas condiciones no se identifican ni exponen su contenido. |
| CA02 | [AC-COLLECTION-01](../../../plans/mvp0-acceptance-criteria.md#ac-collection-01--página-pública-de-una-colección), [«Página de colección»](../../../context/specifications.md#página-de-colección), [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado) | Una colección pública con enlaces cuyo estado público o privado se ha seleccionado y guardado en edición | Se abre la colección desde la navegación pública y se inspecciona su contenido | Se muestran los enlaces públicos de la colección y no aparecen los privados; los enlaces mostrados corresponden a los estados guardados. |
| CA03 | [AC-LINK-02](../../../plans/mvp0-acceptance-criteria.md#ac-link-02--editar-enlace), [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado) | Una colección pública con enlaces y estados de enlace guardados en edición | Se edita y guarda la colección como privada; después se revisan los estados en edición y se navega a su ruta pública | Todos sus enlaces quedan privados; la colección deja de aparecer en la navegación pública y no se expone contenido privado. |
| CA04 | [AC-COLLECTION-01](../../../plans/mvp0-acceptance-criteria.md#ac-collection-01--página-pública-de-una-colección), [«Página de colección»](../../../context/specifications.md#página-de-colección), [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado) | Una colección privada o una colección pública sin enlaces públicos | Se visita su ruta pública, si se conoce, además de recorrer los enlaces disponibles desde las páginas públicas | La ruta responde `404` sin exponer contenido ni distinguir esos casos de una colección inexistente o no accesible públicamente. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para definir el alcance de esta spec. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md) — aprobado por DLG el 27-X-2026 con la referencia «Siguiente».
- [Tareas](tasks.md) — aprobadas por DLG el 27-X-2026 con la referencia «Vamos»; registran la ejecución y evidencia de la revisión manual.
