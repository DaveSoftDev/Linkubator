# Especificación: S11.1 — Landing pública

## Estado y aprobación

- Estado: aprobada.
- Aprobada por DLG el 27-X-2026 con la referencia «Siguiente».

## Objetivo

La ruta raíz presenta la landing pública de Linkubator con el contenido, los metadatos y las acciones definidos para esta página.

## Fuentes

- [requirements.md → «Páginas públicas»](../../context/requirements.md#páginas-públicas)
- [specifications.md → «Páginas públicas»](../../context/specifications.md#páginas-públicas), en particular [«Landing»](../../context/specifications.md#landing)
- [architecture.md → «Enrutamiento»](../../context/architecture.md#enrutamiento)
- [decisions.md → «Producto y alcance»](../../context/decisions.md#producto-y-alcance)
- [mvp0-plan.md → «S11: Área pública y SEO (L)»](../../plans/mvp0-plan.md#s11-área-pública-y-seo-l)

## Alcance

- Incluye:
  - La landing pública servida en la ruta raíz.
  - El texto explicativo, el título, la descripción, el encabezado principal y la URL canonical de la landing.
  - Las acciones de login y registro con el comportamiento comprometido para esta etapa.
- Excluye:
  - Las páginas públicas de usuario, colecciones y colección, así como sus listados y estados vacíos.
  - El tratamiento transversal de variantes de URL y los casos `404` de las demás páginas públicas.
  - El SEO avanzado fuera del alcance comprometido.
- Trabajo vecino y responsable de sus entregables: S11.2 cubre la página de usuario; S11.3, las páginas de colecciones y colección; S11.4, los aspectos transversales de enlaces, imágenes, encabezados e idioma; S11.5, las variantes de URL y respuestas `404`; S11.6, los tests de los criterios aprobados. Estos entregables no forman parte de S11.1.

## Dependencias y prerrequisitos

- Las fuentes funcionales citadas están disponibles y definen el contenido y el comportamiento de la landing.
- No existe un criterio de aceptación S4 específico para esta página en [mvp0-acceptance-criteria.md](../../plans/mvp0-acceptance-criteria.md). Los criterios de esta spec derivan directamente de las fuentes propietarias y no se presentan como criterios S4 aprobados.
- La ejecución de la etapa S11 sigue el orden establecido en [roadmap.md → «Later»](../../context/roadmap.md#later). Esta condición de implementación no bloquea la preparación ni la revisión de esta spec.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [requirements.md → «Páginas públicas»](../../context/requirements.md#páginas-públicas), [specifications.md → «Landing»](../../context/specifications.md#landing), [mvp0-plan.md → «S11: Área pública y SEO (L)»](../../plans/mvp0-plan.md#s11-área-pública-y-seo-l) | Un visitante sin iniciar sesión | Solicita la ruta raíz | Recibe la landing pública renderizada en servidor, que explica qué es Linkubator y presenta las acciones de login y registro. |
| CA02 | [specifications.md → «Landing»](../../context/specifications.md#landing) | La landing está renderizada | Se inspeccionan su encabezado principal y metadatos | El `h1`, el `meta title` y el `meta description` coinciden exactamente con sus valores en la fuente propietaria. |
| CA03 | [specifications.md → «Landing»](../../context/specifications.md#landing), [architecture.md → «Enrutamiento»](../../context/architecture.md#enrutamiento) | La landing está renderizada | Se inspecciona su URL canonical | La canonical apunta a la raíz del origen público configurado para la aplicación y no se construye a partir de la cabecera `Host` de la petición. |
| CA04 | [mvp0-plan.md → «S11: Área pública y SEO (L)»](../../plans/mvp0-plan.md#s11-área-pública-y-seo-l), [decisions.md → «Producto y alcance»](../../context/decisions.md#producto-y-alcance), [specifications.md → «Landing»](../../context/specifications.md#landing) | La landing está renderizada | Se siguen las acciones de login y registro | Login conduce a `/app`; registro conserva `href="#"` y no inicia navegación a un formulario ni envía una solicitud. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para el alcance de esta spec. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md) — propuesta; aprobación pendiente.
