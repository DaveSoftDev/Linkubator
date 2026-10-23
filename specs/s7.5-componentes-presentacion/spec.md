# Especificación: S7.5 — Componentes de presentación accesible

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 23-X-2026 («Siguiente»).

## Objetivo

Proporcionar componentes de presentación reutilizables para comunicar errores y avisos, representar listados vacíos y paginados, y permitir iniciar búsquedas, de forma coherente con las reglas de accesibilidad y las páginas privadas existentes.

## Fuentes

- [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l): entregable S7.5 y límites con tareas vecinas.
- [requirements.md → «Accesibilidad»](../../context/requirements.md#accesibilidad): presentación accesible de mensajes y estados vacíos.
- [specifications.md → «Páginas privadas»](../../context/specifications.md#páginas-privadas): formularios de búsqueda, parámetros preservados y comportamiento común de listados.
- [specifications.md → «Listados»](../../context/specifications.md#listados): paginación y resultado de listados.
- [specifications.md → «Errores controlados»](../../context/specifications.md#errores-controlados): presentación de errores funcionales.

## Alcance

- Incluye: componentes Web reutilizables para resumen de errores de página, avisos, estado vacío, enlaces de paginación y formulario de búsqueda; estructura semántica y accesible de esos componentes; renderizado compatible con los valores y estados que les suministren las páginas.
- Excluye: análisis y validación de `p`, `c` y `q` (S7.6); enrutamiento del dashboard, respuestas HTTP por métodos no permitidos y respuesta de recurso no accesible (S7.7); filtrado, búsqueda y paginación de datos de aplicación; registro de peticiones (S7.8); suite integrada HTTP transversal (S7.9); pruebas manuales globales de dashboard y estilo base (S7.10–S7.11); despliegue.
- Trabajo vecino y responsable de sus entregables: S7.6 interpreta los parámetros entrantes y decide los valores que se aplican; S7.7 integra páginas privadas y avisos para respuestas de recursos inaccesibles; S7.9 añade la suite integrada amplia. Ninguno es prerrequisito para demostrar los componentes propios de esta spec.

## Dependencias y prerrequisitos

- La infraestructura Razor Pages y el pipeline de estilos Web existentes están disponibles desde S0.5.
- S7.4 está completada y aceptada; los componentes se integran en el layout privado y conservan el alcance visual y las garantías de accesibilidad de esa base.
- Las reglas funcionales que condicionan la presentación están definidas en las fuentes enlazadas. S7.5 no analiza parámetros ni ejecuta búsquedas, obtiene listados o decide respuestas HTTP.
- Las páginas y validaciones de S7.6–S7.9 no son prerrequisito de cierre: cada componente se puede comprobar en el host o harness Web existente con una página de prueba acotada si aún no existe una página consumidora estable.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [Requisitos → «Accesibilidad»](../../context/requirements.md#accesibilidad) y [Especificaciones → «Páginas privadas»](../../context/specifications.md#páginas-privadas) | Una página privada que acumula errores de validación o de presentación. | Se renderiza el componente común de resumen de errores. | El resumen expone los errores mediante estructura y semántica accesibles, con enlaces a los campos afectados cuando estos existan; no sustituye los mensajes asociados a cada campo que correspondan al formulario. |
| CA02 | [Especificaciones → «Páginas privadas»](../../context/specifications.md#páginas-privadas) y [«Errores controlados»](../../context/specifications.md#errores-controlados) | Una página privada con un aviso no ligado a un campo, incluidos los avisos de resultado o de recurso no accesible que le suministre la página. | Se renderiza el componente de avisos. | El mensaje aparece en una región semántica perceptible para tecnologías de asistencia y mantiene su contenido textual; el componente no decide ni altera el resultado funcional o HTTP. |
| CA03 | [Requisitos → «Accesibilidad»](../../context/requirements.md#accesibilidad) y [Especificaciones → «Listados»](../../context/specifications.md#listados) | Un listado sin elementos para los filtros y búsqueda recibidos. | La página renderiza el estado vacío. | Se presenta un mensaje de estado vacío accesible y distinguible de un error; el componente no realiza consultas ni cambia los filtros. |
| CA04 | [Especificaciones → «Listados»](../../context/specifications.md#listados) y [«Páginas privadas»](../../context/specifications.md#páginas-privadas) | Un resultado de listado paginado con enlaces a páginas disponibles y parámetros aplicables. | Se renderiza la navegación de paginación. | Los controles permiten identificar la página actual y las acciones disponibles; cada destino conserva la búsqueda y filtros aplicables que le suministra la página, y no incluye parámetros ajenos al listado. |
| CA05 | [Especificaciones → «Páginas privadas»](../../context/specifications.md#páginas-privadas) y [«Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros) | Una página de listado con formulario de búsqueda y filtros aplicables. | Se renderiza y envía el formulario por GET a la ruta de ese listado. | El formulario expone un campo de búsqueda etiquetado, conserva los filtros aplicables y no envía el número de página; no incorpora lógica de análisis ni búsqueda en el componente. |
| CA06 | [Requisitos → «Accesibilidad»](../../context/requirements.md#accesibilidad) | Cualquiera de los componentes de esta spec en estado normal y en los estados con error, aviso, vacío, página actual o control deshabilitado que admita. | Se inspecciona su HTML y se recorre con teclado. | Los nombres, relaciones, orden de foco y estados son comprensibles sin depender únicamente del color y sin controles interactivos inaccesibles. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado. Los contratos visuales y la composición de los componentes se concretarán en el plan técnico, sin alterar los comportamientos fuente. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md): aprobado; describe la solución como base técnica de componentes reutilizables.
- [Tareas](tasks.md): aprobadas, implementación documentada y aceptación humana registrada con la referencia «Componentes realizados»; el cierre no afirma integración funcional con páginas de producto.
