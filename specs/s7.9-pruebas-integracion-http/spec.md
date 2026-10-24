# Especificación: S7.9 — Pruebas de integración HTTP

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 24-X-2026 («Siguiente»).

## Objetivo

Disponer de una suite de integración HTTP que compruebe los contratos de seguridad y navegación Web ya implementados y el análisis de parámetros disponible, sin añadir funcionalidades productivas ni sustituir las pruebas unitarias existentes.

## Fuentes

- [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l): alcance de S7.9 y garantías HTTP que debe cubrir.
- [architecture.md → «Seguridad web»](../../context/architecture.md#seguridad-web): redirección HTTPS, antiforgery, cabeceras y validación del host.
- [architecture.md → «Enrutamiento»](../../context/architecture.md#enrutamiento): zona privada y ruta de entrada.
- [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas): destino privado, respuestas por método no permitido y análisis de parámetros.
- [S7.1 — Seguridad web básica → CA01–CA03](../s7.1-seguridad-web/spec.md#criterios-de-aceptación): garantías de seguridad Web ya configuradas.
- [S7.2 — Cabeceras de seguridad web → CA01–CA03](../s7.2-cabeceras-seguridad/spec.md#criterios-de-aceptación): cabeceras de respuesta ya configuradas.
- [S7.6 — Análisis de parámetros de listados → CA01–CA07](../s7.6-analisis-parametros-listados/spec.md#criterios-de-aceptación): contrato existente del analizador de parámetros.
- [S7.7 — Dashboard y respuestas privadas → CA01 y CA03](../s7.7-dashboard-y-respuestas-privadas/spec.md#criterios-de-aceptación): resultado observable de la entrada privada y respuesta genérica por método no permitido.

## Alcance

- **Incluye:** pruebas automatizadas de integración HTTP para la redirección a HTTPS; cabeceras de seguridad; rechazo de un `Host` no admitido; validación antiforgery en una operación POST aislada del host de pruebas; resolución de `/app` al dashboard; respuesta genérica `404` ante un método no permitido en una ruta privada; y análisis de parámetros mediante una petición que atraviese el host de pruebas.
- **Excluye:** cambios en código productivo o en las reglas funcionales; comprobación de TLS real o confianza del certificado en un navegador; navegación automatizada de UI; el registro de eventos y la ausencia de datos sensibles en logs, que tienen su propio alcance; y la integración funcional de parámetros en listados que aún no existan.
- **Trabajo vecino y responsable de sus entregables:** S7.1 y S7.2 son propietarios de la configuración probada; S7.6 es propietario del analizador; S7.7 es propietario de la entrada privada y del tratamiento común de métodos no permitidos; S7.8 es propietario del registro de peticiones. Esta spec verifica contratos existentes y no modifica ni vuelve a implementar esos trabajos.

## Dependencias y prerrequisitos

- El host Web, `WebApplicationFactory` y el proyecto de pruebas están disponibles.
- S7.1 y S7.2 aportan las protecciones HTTP configuradas; S7.7 aporta el dashboard y la respuesta privada de método no permitido.
- El analizador Web de S7.6 y sus pruebas unitarias están disponibles. La verificación HTTP puede consumirlo mediante un punto de prueba exclusivo del host de tests, sin esperar a que se construyan los listados funcionales.
- No se requiere la implementación de S7.8 ni de rutas funcionales de etapas posteriores. La validación de HTTPS mediante `TestServer` demuestra el comportamiento de redirección configurado, no una conexión TLS real.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [Arquitectura → «Seguridad web»](../../context/architecture.md#seguridad-web) y [S7.1 → CA01](../s7.1-seguridad-web/spec.md#criterios-de-aceptación) | Una petición HTTP llega al host de pruebas. | Se procesa la petición sin seguir automáticamente redirecciones. | La respuesta redirige al esquema HTTPS configurado. La prueba no afirma que se haya establecido TLS ni que un certificado sea válido. |
| CA02 | [Arquitectura → «Seguridad web»](../../context/architecture.md#seguridad-web) y [S7.2 → CA01–CA03](../s7.2-cabeceras-seguridad/spec.md#criterios-de-aceptación) | El host genera una respuesta Web. | La integración HTTP inspecciona sus cabeceras. | La respuesta contiene las cabeceras de seguridad definidas por la fuente con los valores ya aprobados. |
| CA03 | [Arquitectura → «Seguridad web»](../../context/architecture.md#seguridad-web) y [S7.1 → CA02](../s7.1-seguridad-web/spec.md#criterios-de-aceptación) | El host recibe solicitudes de control con un `Host` permitido y otro no admitido. | Se comparan las respuestas HTTP. | El `Host` no admitido se rechaza y el permitido puede continuar al endpoint solicitado. |
| CA04 | [Arquitectura → «Seguridad web»](../../context/architecture.md#seguridad-web) y [S7.1 → CA03](../s7.1-seguridad-web/spec.md#criterios-de-aceptación) | El host de pruebas expone una operación POST aislada para comprobar antiforgery. | Se envía una solicitud sin token y otra con token válido emitido por el host. | La solicitud sin token válido se rechaza antes de ejecutar la operación; la de control supera antiforgery. No se crea una operación productiva ni se registra el valor del token en evidencia. |
| CA05 | [Arquitectura → «Enrutamiento»](../../context/architecture.md#enrutamiento), [Especificaciones → «Rutas privadas»](../../context/specifications.md#rutas-privadas) y [S7.7 → CA01](../s7.7-dashboard-y-respuestas-privadas/spec.md#criterios-de-aceptación) | Un cliente HTTP solicita `/app`. | Se sigue la navegación según el comportamiento implementado. | La petición termina en el dashboard privado. La aserción verifica el resultado observable, sin imponer si `/app` sirve la página o redirige a su ruta canónica. |
| CA06 | [Especificaciones → «Rutas privadas»](../../context/specifications.md#rutas-privadas) y [S7.7 → CA03](../s7.7-dashboard-y-respuestas-privadas/spec.md#criterios-de-aceptación) | Una ruta privada existente recibe un método no admitido. | Se compara la respuesta HTTP con la respuesta genérica esperada y, cuando sea posible, con una ruta inexistente de control. | La respuesta es el `404` genérico y no revela detalles del recurso ni su propiedad. |
| CA07 | [Especificaciones → «Rutas privadas»](../../context/specifications.md#rutas-privadas) y [S7.6 → CA01–CA07](../s7.6-analisis-parametros-listados/spec.md#criterios-de-aceptación) | Una petición HTTP al punto de prueba del host contiene combinaciones de parámetros válidos, vacíos, repetidos, inválidos y no aplicables. | El punto de prueba entrega la consulta al analizador de producción. | La respuesta observable corresponde al resultado del analizador aprobado: se preservan los valores aplicables válidos, se ignoran los parámetros no aplicables y los valores erróneos quedan identificados sin usarse como filtros válidos. El punto de prueba no representa una ruta de listado productiva ni demuestra consultas o autorización de datos. |

Los casos negativos y controles positivos se expresan en CA01–CA04 y CA06–CA07. La verificación de cada propiedad debe fallar si se elimina o invierte la garantía correspondiente; la sensibilidad de las aserciones se comprobará con una perturbación temporal segura y restaurada.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado. El harness puede usar puntos de prueba exclusivos para antiforgery y el analizador, sin añadir rutas productivas. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 24-X-2026 («Siguiente»).
- [Tareas, evidencia y cierre](tasks.md#aceptación-y-cierre), aprobadas por DLG el 24-X-2026 («Vamos»); implementación y validación técnica completadas y resultado aceptado por DLG el 24-X-2026 («Completado»).
