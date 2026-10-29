# Especificación: S11.2 — Página pública de usuario

## Estado y aprobación

- Estado: aprobada.
- Aprobada por DLG el 27-X-2026 con la referencia «Siguiente».

## Objetivo

La ruta pública de un usuario presenta su página con el contenido y las respuestas establecidos para ella.

## Fuentes

- [requirements.md → «Páginas públicas»](../../../context/requirements.md#páginas-públicas)
- [specifications.md → «Páginas públicas»](../../../context/specifications.md#páginas-públicas), en particular [«Página de usuario»](../../../context/specifications.md#página-de-usuario) y [«Listados»](../../../context/specifications.md#listados)
- [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado)
- [architecture.md → «Enrutamiento»](../../../context/architecture.md#enrutamiento)
- [mvp0-acceptance-criteria.md → «AC-USER-01 — Página pública de usuario»](../../../plans/mvp0-acceptance-criteria.md#ac-user-01--página-pública-de-usuario)
- [mvp0-plan.md → «S11: Área pública y SEO (L)»](../../../plans/mvp0-plan.md#s11-área-pública-y-seo-l)
- [roadmap.md → «Later»](../../../context/roadmap.md#later)

## Alcance

- Incluye:
  - La página pública servida en la ruta canónica del alias.
  - Las secciones de enlaces y colecciones, sus estados vacíos, el acceso al listado completo de colecciones cuando corresponda y los metadatos de esta página.
  - La respuesta de página inexistente para alias inexistentes y alias antiguos.
- Excluye:
  - La landing pública, cubierta por S11.1.
  - Las páginas públicas de colecciones y de colección, asignadas a S11.3 en el reparto de [S11](../../../plans/mvp0-plan.md#s11-área-pública-y-seo-l).
  - La aplicación transversal de atributos de enlaces, imágenes, encabezados e idioma, y el tratamiento de variantes de URL, asignados respectivamente a S11.4 y S11.5 en el mismo reparto.
  - El SEO avanzado fuera del alcance comprometido.
- Trabajo vecino y responsable de sus entregables: S11.1 implementa la landing; S11.3 implementa las páginas públicas de colecciones; S11.4 aplica los aspectos transversales de presentación; S11.5 trata las variantes de URL. S11.6 cubre los tests generales de la etapa. Esta spec no amplía esos entregables.

## Dependencias y prerrequisitos

- El criterio aprobado [AC-USER-01](../../../plans/mvp0-acceptance-criteria.md#ac-user-01--página-pública-de-usuario) y sus fuentes funcionales definen el resultado observable de la página.
- La etapa S11 está situada en [«Later»](../../../context/roadmap.md#later). Ese orden condiciona el inicio de su implementación, no la preparación ni la revisión de esta spec.
- No se identifican dependencias adicionales necesarias para definir este alcance.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [AC-USER-01](../../../plans/mvp0-acceptance-criteria.md#ac-user-01--página-pública-de-usuario), [specifications.md → «Página de usuario»](../../../context/specifications.md#página-de-usuario), [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado) | Un alias público existente o inexistente | Se solicita su ruta pública | La respuesta satisface AC-USER-01, incluidos el contenido público y sus estados, los metadatos, la canonical y el resultado para alias inexistentes o antiguos. |
| CA02 | [specifications.md → «Páginas públicas»](../../../context/specifications.md#páginas-públicas), [architecture.md → «Enrutamiento»](../../../context/architecture.md#enrutamiento) | Se solicita la página pública de un usuario existente | Se recibe la respuesta de la página | El contenido de la página se entrega renderizado en servidor. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para el alcance de esta spec. | — | — | — |

## Artefactos relacionados

No hay otros artefactos de esta spec creados todavía.
