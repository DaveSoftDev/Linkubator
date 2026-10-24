# Especificación: S7.4 — Layout privado accesible

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 22-X-2026 («Siguiente»).

## Objetivo

Proporcionar a las páginas privadas un layout base coherente y accesible que cubra idioma, estructura semántica, navegación por teclado, presentación visual adaptable a preferencias de movimiento y exclusión de indexación.

## Fuentes

- [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l): alcance de S7.4 y revisión manual relacionada.
- [requirements.md → «Accesibilidad»](../../context/requirements.md#accesibilidad): expectativas de uso con teclado y tecnologías de asistencia.
- [specifications.md → «Idioma de las páginas»](../../context/specifications.md#idioma-de-las-páginas): idioma del documento.
- [specifications.md → «Páginas privadas»](../../context/specifications.md#páginas-privadas): metadato de robots y comportamiento de páginas privadas.
- [architecture.md → «Web»](../../context/architecture.md#web): responsabilidad de presentación accesible.

## Alcance

- Incluye: layout compartido de la zona privada; idioma `es`; landmarks semánticos; enlace de salto al contenido principal; estilo de foco visible; cumplimiento de contraste WCAG AA; respeto de `prefers-reduced-motion`; metadato `noindex, nofollow` en páginas privadas.
- Excluye: componentes de formularios y errores, avisos, estados vacíos, paginación y búsqueda (S7.5); análisis de parámetros, dashboard y manejo HTTP (S7.6–S7.7); suite integrada amplia de seguridad (S7.9); revisión manual de teclado/foco y validación/aceptación del estilo base, que pertenecen a S7.10–S7.11; despliegue.
- Trabajo vecino y responsable de sus entregables: S7.5 suministra componentes de presentación; S7.10 y S7.11 realizan las revisiones humanas del dashboard y estilo global, según [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l). Sus resultados no bloquean el cierre de los criterios propios de esta especificación.

## Dependencias y prerrequisitos

- La configuración base de Razor Pages/Web está disponible desde S0.5.
- S7.1 y S7.2 están completadas y aceptadas; el layout reutiliza el host existente sin modificar sus protecciones.
- S7.3 resuelve la identidad para `/app`; la integración visual del layout puede prepararse sin ampliar ni alterar ese contrato.
- La revisión manual del dashboard S7.10 y la validación global del estilo base S7.11 son seguimientos independientes y no prerrequisitos de cierre.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [Especificaciones → «Idioma de las páginas»](../../context/specifications.md#idioma-de-las-páginas) | Una página servida bajo `/app`. | Se inspecciona el elemento raíz del documento. | Declara el idioma español conforme a la regla fuente. |
| CA02 | [Requisitos → «Accesibilidad»](../../context/requirements.md#accesibilidad) | Una página privada renderizada con el layout. | Se inspecciona la estructura semántica del documento. | Expone landmarks apropiados y un enlace de salto que lleva al contenido principal. |
| CA03 | [Requisitos → «Accesibilidad»](../../context/requirements.md#accesibilidad) | Un control interactivo del layout recibe foco mediante teclado. | Se evalúa su estado enfocado. | El foco tiene una indicación visual perceptible y no depende únicamente del color. |
| CA04 | [Requisitos → «Accesibilidad»](../../context/requirements.md#accesibilidad) | Los colores de texto, controles y superficies usados por el layout. | Se contrastan las combinaciones aplicables con WCAG AA. | Las combinaciones cumplen los umbrales aplicables de WCAG AA. |
| CA05 | [Requisitos → «Accesibilidad»](../../context/requirements.md#accesibilidad) | Una animación o transición visual del layout. | El navegador indica `prefers-reduced-motion: reduce`. | Se reducen o eliminan los movimientos no esenciales del layout. |
| CA06 | [Especificaciones → «Páginas privadas»](../../context/specifications.md#páginas-privadas) | Una página privada renderizada. | Se inspeccionan sus metadatos HTML. | Incluye la directiva de robots privada establecida por la fuente. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado. Las decisiones de diseño visual que no alteren estos criterios se concretarán en el plan técnico. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md): diseño y estrategia de validación aprobados.
- [Tareas y evidencia](tasks.md): implementación completada y aceptación humana registrada.
