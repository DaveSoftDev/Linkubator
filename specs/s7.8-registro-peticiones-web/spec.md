# Especificación: S7.8 — Registro de peticiones Web

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 24-X-2026 («Siguiente»).

## Objetivo

Configurar el registro de peticiones HTTP para que aporte trazabilidad operativa sin exponer en los registros datos que las fuentes funcionales prohíben registrar.

## Fuentes

- [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l): tarea S7.8 y su alcance.
- [specifications.md → «Registro de eventos»](../../context/specifications.md#registro-de-eventos): datos prohibidos en logs, incluidas las consultas privadas y las URL completas de peticiones que llevan tokens.
- [architecture.md → «Logs»](../../context/architecture.md#logs): registro estructurado con Serilog y requisitos del registro de peticiones del framework.
- [architecture.md → «Seguridad web»](../../context/architecture.md#seguridad-web): política de peticiones y cabeceras web.

## Alcance

- **Incluye:** configuración del registro de peticiones del framework Web; exclusión de la cadena de consulta de los listados privados y de la URL completa en las solicitudes que contienen tokens; comprobación de que el registro conserva información operativa no sensible de la petición.
- **Excluye:** rediseño del registro de eventos de negocio; cambios en configuración global de Serilog o sus destinos; ampliar los datos que se registran; suite de integración HTTP transversal S7.9; despliegue.
- **Trabajo vecino y responsable de sus entregables:** S7.9 cubre las pruebas de integración HTTP transversales; no es requisito para verificar esta configuración de forma focalizada.

## Dependencias y prerrequisitos

- La aplicación Web y el pipeline de registro estructurado existentes están disponibles, según [architecture.md → «Logs»](../../context/architecture.md#logs).
- Las reglas de exclusión de datos sensibles ya están definidas en [specifications.md → «Registro de eventos»](../../context/specifications.md#registro-de-eventos); esta spec no las amplía ni modifica.
- La suite transversal de S7.9 es trabajo independiente y no bloquea el cierre de esta especificación.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [Arquitectura → «Logs»](../../context/architecture.md#logs) | Una petición atendida por la aplicación Web. | Se emite su registro de petición. | El registro se produce mediante el pipeline existente de logging y permite identificar la petición con datos operativos no sensibles. |
| CA02 | [Especificaciones → «Registro de eventos»](../../context/specifications.md#registro-de-eventos), [Arquitectura → «Logs»](../../context/architecture.md#logs) | Una petición a una página de listado privado incluye una cadena de consulta con texto de búsqueda. | El middleware registra la petición. | El evento de registro no contiene la cadena de consulta ni el texto de búsqueda. |
| CA03 | [Especificaciones → «Registro de eventos»](../../context/specifications.md#registro-de-eventos), [Arquitectura → «Logs»](../../context/architecture.md#logs) | Una petición a un endpoint cuyo token viaja en la URL. | El middleware registra la petición. | El evento no contiene la URL completa ni el token. |
| CA04 | [Especificaciones → «Registro de eventos»](../../context/specifications.md#registro-de-eventos) | Se inspeccionan los eventos de registro de peticiones generados en CA02 y CA03. | Se buscan datos sensibles prohibidos por la fuente. | No se registran contraseñas, tokens, hashes innecesarios, credenciales contenidas en URL ni texto de búsqueda. |

Los casos negativos y límites aplicables se derivan de [specifications.md → «Registro de eventos»](../../context/specifications.md#registro-de-eventos): la inspección debe comprobar tanto la ausencia de la cadena de consulta en listados privados como la ausencia de la URL completa en peticiones con token. No se define aquí una nueva política de anonimización para otros datos.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado. | — | — | — |

## Artefactos relacionados

Aún no hay plan técnico ni tareas: se crearán tras las aprobaciones correspondientes.
