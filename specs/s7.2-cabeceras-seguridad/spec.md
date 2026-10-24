# Especificación: S7.2 — Cabeceras de seguridad web

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 22-X-2026 («Siguiente»).

## Objetivo

Añadir a las respuestas de la aplicación las cabeceras de seguridad previstas, sin ampliar el comportamiento funcional del host.

## Fuentes

- [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l): tarea S7.2 y su alcance.
- [architecture.md → «Seguridad web»](../../context/architecture.md#seguridad-web): cabeceras y política de referencia.

## Alcance

- Incluye: presencia y valor de `X-Content-Type-Options`, `Content-Security-Policy` con `frame-ancestors 'none'` y `Referrer-Policy` en las respuestas HTTP de la aplicación, según la fuente de arquitectura.
- Excluye: HTTPS, antiforgery y validación de `Host` (S7.1); autenticación y resolución de identidad (S7.3); pruebas de integración HTTP consolidadas (S7.9); despliegue.
- Trabajo vecino y responsable de sus entregables: S7.9 proporciona las pruebas de integración HTTP amplias, según [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l). La verificación focalizada de esta spec no depende de ese trabajo.

## Dependencias y prerrequisitos

- [S7.1 — Seguridad web básica](../s7.1-seguridad-web/spec.md) está aprobada y su implementación fue aceptada; reutilizar el host/pipeline Web existente.
- La tarea S7.2 está definida en el plan de implementación. No se requieren decisiones funcionales adicionales para especificar la tarea.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [Arquitectura → «Seguridad web»](../../context/architecture.md#seguridad-web) | Una respuesta HTTP generada por la aplicación Web. | Se inspeccionan sus encabezados. | Incluye `X-Content-Type-Options` con `nosniff`. |
| CA02 | [Arquitectura → «Seguridad web»](../../context/architecture.md#seguridad-web) | Una respuesta HTTP generada por la aplicación Web. | Se inspecciona su política de seguridad de contenido. | Incluye `Content-Security-Policy` con `frame-ancestors 'none'`. |
| CA03 | [Arquitectura → «Seguridad web»](../../context/architecture.md#seguridad-web) | Una respuesta HTTP generada por la aplicación Web. | Se inspecciona su política de referencia. | Incluye `Referrer-Policy` con `strict-origin-when-cross-origin`. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado. | — | — | — |

## Artefactos relacionados

Aún no hay plan técnico ni tareas: se crearán tras las aprobaciones correspondientes.
