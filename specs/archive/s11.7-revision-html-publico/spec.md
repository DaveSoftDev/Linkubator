# Especificación: S11.7 — Revisión del HTML público

## Estado y aprobación

- Estado: aprobada.
- Aprobada por DLG el 27-X-2026 con la referencia «Siguiente».

## Objetivo

Revisar manualmente el HTML renderizado de las cuatro páginas públicas para comprobar que sus metadatos, encabezados, textos de interfaz e imágenes cumplen las fuentes aprobadas, sin introducir reglas nuevas.

## Fuentes

- [requirements.md → «Páginas públicas»](../../../context/requirements.md#páginas-públicas)
- [specifications.md → «Idioma de las páginas»](../../../context/specifications.md#idioma-de-las-páginas)
- [specifications.md → «Páginas públicas»](../../../context/specifications.md#páginas-públicas), en particular [«Rutas públicas»](../../../context/specifications.md#rutas-públicas), [«Landing»](../../../context/specifications.md#landing), [«Página de usuario»](../../../context/specifications.md#página-de-usuario), [«Página de colecciones»](../../../context/specifications.md#página-de-colecciones) y [«Página de colección»](../../../context/specifications.md#página-de-colección)
- [architecture.md → «Enrutamiento»](../../../context/architecture.md#enrutamiento)
- [mvp0-plan.md → «S11: Área pública y SEO (L)»](../../../plans/mvp0-plan.md#s11-área-pública-y-seo-l)

## Alcance

- Incluye:
  - La revisión humana del HTML renderizado de la landing, la página pública de usuario, la página de colecciones y la página de colección.
  - La comprobación de títulos, descripciones, URL canonical, encabezados, idioma, atributos `alt` y textos de interfaz definidos por las fuentes.
  - La detección de textos provisionales o pendientes en esas páginas.
- Excluye:
  - La implementación o modificación de las páginas y sus reglas funcionales, cubiertas por los trabajos S11.1–S11.5.
  - La suite de pruebas automatizadas HTTP, cubierta por S11.6.
  - La prueba manual de navegación pública asignada a S11.8.
  - La verificación de indexación efectiva por buscadores y el SEO avanzado.
  - Una revisión visual general del diseño que no esté incluida en los elementos de HTML enumerados.

## Dependencias y prerrequisitos

- La ejecución de la revisión requiere que estén disponibles las cuatro páginas y sus atributos transversales y rutas públicas, entregados por S11.1–S11.5, y que se haya completado la validación HTTP de S11.6, de acuerdo con el orden de [S11 en el plan](../../../plans/mvp0-plan.md#s11-área-pública-y-seo-l).
- Según el [roadmap](../../../context/roadmap.md#área-pública-y-seo), S11.1 y S11.3 constan completadas, S11.2 sigue en curso y S11.4–S11.5 no están cerradas; S11.6 está completada. Por tanto, la revisión todavía no está lista para ejecutarse.
- La preparación de esta especificación no depende de que esos trabajos estén terminados. S11.8 no es un prerrequisito para cerrar la revisión de HTML de S11.7.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [specifications.md → «Páginas públicas»](../../../context/specifications.md#páginas-públicas), [«Rutas públicas»](../../../context/specifications.md#rutas-públicas), [«Landing»](../../../context/specifications.md#landing), [«Página de usuario»](../../../context/specifications.md#página-de-usuario), [«Página de colecciones»](../../../context/specifications.md#página-de-colecciones), [«Página de colección»](../../../context/specifications.md#página-de-colección), [architecture.md → «Enrutamiento»](../../../context/architecture.md#enrutamiento) | Las cuatro páginas públicas están disponibles en sus rutas canónicas | Se inspeccionan el `title`, el `meta description` y la canonical del HTML renderizado de cada página | Los títulos y las descripciones coinciden con la fuente propietaria de cada página, y cada canonical corresponde a la ruta canónica y al origen público configurado; para la página de colección se aplica la regla de canonical de paginación. |
| CA02 | [specifications.md → «Idioma de las páginas»](../../../context/specifications.md#idioma-de-las-páginas), [«Landing»](../../../context/specifications.md#landing), [«Página de usuario»](../../../context/specifications.md#página-de-usuario), [«Página de colecciones»](../../../context/specifications.md#página-de-colecciones), [«Página de colección»](../../../context/specifications.md#página-de-colección), [mvp0-plan.md → «S11: Área pública y SEO (L)»](../../../plans/mvp0-plan.md#s11-área-pública-y-seo-l) | El HTML de cada página pública está renderizado | Se inspeccionan su idioma y su jerarquía de encabezados | Cada documento declara `lang="es"`, tiene un solo `h1` y sus encabezados coinciden con los textos y secciones definidos por la fuente propietaria. |
| CA03 | [specifications.md → «Páginas públicas»](../../../context/specifications.md#páginas-públicas) | Se renderiza una página pública que contiene imágenes de enlaces y tarjetas sin imagen | Se inspeccionan los atributos `alt` de las imágenes y el recurso decorativo | Cada imagen de enlace usa como `alt` el título informado o, en su defecto, `UrlOriginal`; el símbolo decorativo de una tarjeta sin imagen no tiene texto alternativo. |
| CA04 | [requirements.md → «Páginas públicas»](../../../context/requirements.md#páginas-públicas), [specifications.md → «Páginas públicas»](../../../context/specifications.md#páginas-públicas) y sus secciones de página, [mvp0-plan.md → «S11: Área pública y SEO (L)»](../../../plans/mvp0-plan.md#s11-área-pública-y-seo-l) | Se inspeccionan los textos visibles de interfaz definidos para las cuatro páginas, incluidos los estados vacíos que correspondan | Se contrastan con las fuentes funcionales | Los textos coinciden con las fuentes y no queda texto provisional o pendiente. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para definir el alcance de esta spec. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md) — propuesta; aprobación pendiente.
