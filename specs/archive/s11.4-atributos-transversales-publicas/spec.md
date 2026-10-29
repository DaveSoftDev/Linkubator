# Especificación: S11.4 — Atributos transversales de páginas públicas

## Estado y aprobación

- Estado: aprobada.
- Aprobada por DLG el 27-X-2026 con la referencia «Siguiente».

## Objetivo

Las páginas públicas aplican de forma coherente los atributos de sus enlaces externos e imágenes y presentan encabezados e idioma conforme a sus fuentes.

## Fuentes

- [requirements.md → «Páginas públicas»](../../../context/requirements.md#páginas-públicas)
- [requirements.md → «Accesibilidad»](../../../context/requirements.md#accesibilidad)
- [specifications.md → «Idioma de las páginas»](../../../context/specifications.md#idioma-de-las-páginas)
- [specifications.md → «Páginas públicas»](../../../context/specifications.md#páginas-públicas), en particular [«Landing»](../../../context/specifications.md#landing), [«Página de usuario»](../../../context/specifications.md#página-de-usuario), [«Página de colecciones»](../../../context/specifications.md#página-de-colecciones) y [«Página de colección»](../../../context/specifications.md#página-de-colección)
- [mvp0-plan.md → «S11: Área pública y SEO (L)»](../../../plans/mvp0-plan.md#s11-área-pública-y-seo-l)
- [roadmap.md → «Área pública y SEO»](../../../context/roadmap.md#área-pública-y-seo)

## Alcance

- Incluye:
  - Los atributos de los enlaces a `UrlOriginal` en las páginas públicas de usuario y de colección.
  - El texto alternativo de las imágenes de enlaces públicos.
  - El encabezado principal de las cuatro páginas públicas.
  - El idioma declarado en el elemento raíz de las páginas públicas.
- Excluye:
  - El contenido, los metadatos, las rutas, las respuestas y la canonical de cada página, cubiertos por S11.1, S11.2 y S11.3.
  - Las páginas privadas, aunque la regla general de idioma también les aplique.
  - Las redirecciones de variantes de URL y las respuestas `404` transversales, asignadas a S11.5.
  - La suite transversal de tests de S11.6 y la revisión manual general de HTML de S11.7.
- Trabajo vecino y responsable de sus entregables: S11.1 implementa la landing; S11.2, la página pública de usuario; S11.3, las páginas públicas de colecciones y colección; S11.5, las variantes de URL; S11.6, los tests de la etapa; S11.7, la revisión humana del HTML. Esta spec no amplía esos entregables.

## Dependencias y prerrequisitos

- Las fuentes funcionales y el reparto de trabajo de S11 están disponibles y definen las cuatro páginas y sus reglas transversales.
- La implementación y validación integradas de S11.4 requieren que estén disponibles las páginas de S11.1, S11.2 y S11.3. S11.1 y S11.3 constan implementadas, y el [cierre de S11.2](../s11.2-pagina-publica-usuario/tasks.md#aceptación-y-cierre) del 28-X-2026 satisface el prerrequisito pendiente.
- La preparación de esta spec no depende de la finalización de S11.2. S11.5–S11.7 no son prerrequisitos para cerrar S11.4.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [specifications.md → «Página de usuario»](../../../context/specifications.md#página-de-usuario), [specifications.md → «Página de colección»](../../../context/specifications.md#página-de-colección) | Se renderiza un enlace a `UrlOriginal` en la página pública de usuario o de colección | Se inspecciona el enlace externo en el HTML | El elemento de enlace contiene los atributos `rel` y `target` definidos por la fuente propietaria. |
| CA02 | [specifications.md → «Páginas públicas»](../../../context/specifications.md#páginas-públicas) | Se renderiza una imagen de un enlace público con título informado o no informado | Se inspecciona el atributo `alt` de la imagen | Su valor es el texto definido por la fuente propietaria y se representa como atributo de texto, no como marcado HTML. |
| CA03 | [mvp0-plan.md → «S11: Área pública y SEO (L)»](../../../plans/mvp0-plan.md#s11-área-pública-y-seo-l), [specifications.md → «Páginas públicas»](../../../context/specifications.md#páginas-públicas) | Se renderiza correctamente cualquiera de las cuatro páginas públicas | Se inspecciona su estructura de encabezados | La página contiene un único elemento `h1`. |
| CA04 | [specifications.md → «Idioma de las páginas»](../../../context/specifications.md#idioma-de-las-páginas), [requirements.md → «Accesibilidad»](../../../context/requirements.md#accesibilidad) | Se entrega como HTML cualquiera de las cuatro páginas públicas | Se inspecciona el elemento raíz del documento | El elemento `<html>` declara el idioma establecido por la fuente propietaria. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para definir el alcance de esta spec. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md) — aprobado por DLG el 27-X-2026 («Siguiente»).
- [Tareas](tasks.md) — aprobadas por DLG el 27-X-2026 («Vamos»); evidencia y aceptación del resultado en su registro de cierre.
