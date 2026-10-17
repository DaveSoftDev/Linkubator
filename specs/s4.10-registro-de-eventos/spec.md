# Especificación: S4.10 — Registro de eventos

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Objetivo

Instrumentar los casos de uso de Application de S4.6–S4.8 con el registro de eventos previsto para el MVP0, usando la abstracción de logging de .NET y sin registrar datos prohibidos ni alterar los resultados funcionales.

## Fuentes

- [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l): alcance de S4.10 y dependencia de los casos de uso S4.6–S4.8.
- [roadmap.md → «Now»](../../context/roadmap.md#now): estado de S4 y trazabilidad de los trabajos precedentes.
- [Catálogo de criterios de aceptación del MVP0 → «Reglas transversales»](../../plans/mvp0-acceptance-criteria.md#reglas-transversales): AC-TRANS-03, criterio propietario del registro de eventos.
- [requirements.md → «Operación»](../../context/requirements.md#operación): necesidad de registrar los eventos relevantes sin datos sensibles.
- [specifications.md → «Registro de eventos»](../../context/specifications.md#registro-de-eventos): categorías de eventos y datos que no se registran.
- [architecture.md → «Application»](../../context/architecture.md#application) y [«Logs»](../../context/architecture.md#logs): abstracción de logging en las capas y configuración del proveedor fuera de los casos de uso.
- [AGENTS.md → «Invariantes críticas»](../../AGENTS.md#invariantes-críticas) y [«Convención de estructura del código»](../../AGENTS.md#convención-de-estructura-del-código): protección de secretos y estructura de tipos.
- [S4.6 — casos de uso de colecciones](../s4.6-casos-uso-colecciones/spec.md), [S4.7 — casos de uso de enlaces](../s4.7-casos-uso-enlaces/spec.md) y [S4.8 — dashboard y lecturas públicas](../s4.8-dashboard-y-lecturas-publicas/spec.md): casos de uso de Application a instrumentar.
- [S4.9 — fakes en memoria y pruebas de casos de uso](../s4.9-fakes-y-tests-casos-de-uso/spec.md): separación entre las pruebas de comportamiento de S4.9 y la instrumentación de S4.10.

## Alcance

- Incluye:
  - Registrar desde Application los eventos relevantes de los casos de uso de colecciones, enlaces, dashboard y lecturas públicas ya implementados en S4.6–S4.8.
  - Cubrir las categorías aplicables de operaciones, validaciones, errores de persistencia y cambios entre público y privado indicadas en las fuentes propietarias.
  - Comprobar que los eventos registrados y sus datos estructurados no incluyen información prohibida.
  - Mantener intactos los resultados de Application y sus contratos, incluidos los errores y rechazos ya especificados.
- Excluye:
  - Cambiar reglas funcionales, resultados, códigos de error o criterios aprobados. Una carencia o contradicción bloqueante se resuelve en su documento propietario.
  - Configurar Serilog, sus destinos, retención, niveles, formato o registro de peticiones HTTP; son responsabilidad de configuración y de las etapas de Infrastructure/Web.
  - Añadir eventos de login, cuenta o scraping, que pertenecen a sus etapas respectivas.
  - Implementar persistencia concreta o afirmar que los logs de Infrastructure/Web ya cumplen las reglas; esas garantías se comprueban en sus etapas propietarias.
  - Repetir las pruebas de comportamiento de los casos de uso o modificar sus implementaciones fuera de lo necesario para la instrumentación aprobada.
- Trabajo vecino y responsable de sus entregables:
  - S4.6–S4.8 aportan los casos de uso de Application que se instrumentan.
  - S4.9 mantiene las pruebas de comportamiento con fakes; S4.10 verifica específicamente eventos y ausencia de datos prohibidos.
  - Infrastructure configura el proveedor de logging y Web configura el registro de peticiones conforme a las fuentes propietarias.

## Dependencias y prerrequisitos

- S4.5 aprobó el catálogo de criterios de aceptación el 16-X-2026; la puerta de criterios está satisfecha.
- S4.6, S4.7 y S4.8 están completadas y aceptadas para sus respectivos alcances de Application; sus casos de uso están disponibles.
- S4.9 está completada y aceptada para su alcance de pruebas de comportamiento. Sus pruebas no se consideran evidencia de registro de eventos.
- La configuración de `ILogger` detrás del proveedor documentado en arquitectura no es un prerrequisito para comprobar la instrumentación de Application: el comportamiento se validará mediante un logger de prueba en el límite de esta capa.
- La aprobación de esta especificación autoriza preparar el plan técnico, no crear tareas ni iniciar cambios productivos. Las tareas se prepararán solo después de la aprobación explícita del plan.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [AC-TRANS-03](../../plans/mvp0-acceptance-criteria.md#ac-trans-03--registro-de-eventos), [specifications.md → «Registro de eventos»](../../context/specifications.md#registro-de-eventos) y [requirements.md → «Operación»](../../context/requirements.md#operación) | Los casos de uso de S4.6–S4.8 y los escenarios de éxito, validación, persistencia fallida y cambios de visibilidad aplicables a las fuentes están disponibles. | Se ejecuta cada escenario instrumentado con un logger de prueba. | Se emiten eventos para las categorías exigidas que resulten aplicables, con datos suficientes para identificar el evento sin cambiar el resultado del caso de uso; los escenarios y categorías se trazan a las fuentes propietarias, sin inventar tipos de eventos ni mensajes funcionales. |
| CA02 | [AC-TRANS-03](../../plans/mvp0-acceptance-criteria.md#ac-trans-03--registro-de-eventos), [specifications.md → «Registro de eventos»](../../context/specifications.md#registro-de-eventos) y [AGENTS.md → «Invariantes críticas»](../../AGENTS.md#invariantes-críticas) | Las entradas y excepciones de los casos de uso contienen valores centinela que representan datos prohibidos por la especificación. | Se ejercitan rutas de éxito, rechazo y error y se inspeccionan el mensaje, el estado estructurado y los datos de excepción que recibe el logger. | Ningún dato prohibido aparece en los eventos; los casos aplicables incluyen búsquedas, secretos, contraseñas, tokens, hashes innecesarios y credenciales de URL. |
| CA03 | [AC-TRANS-03](../../plans/mvp0-acceptance-criteria.md#ac-trans-03--registro-de-eventos), [architecture.md → «Application»](../../context/architecture.md#application) y [«Logs»](../../context/architecture.md#logs) | Los casos de uso de S4.6–S4.8 se ejecutan con logging habilitado y deshabilitado/no-op. | Se comparan resultados, errores y efectos funcionales con los eventos observados. | El logging usa la abstracción prevista, no cambia contratos, códigos ni resultados de Application y no acopla esta capa al proveedor, a Web ni a Infrastructure. |
| CA04 | [AC-TRANS-03](../../plans/mvp0-acceptance-criteria.md#ac-trans-03--registro-de-eventos), [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) y [architecture.md → «Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos) | La instrumentación y las pruebas de S4.10 están integradas en la solución. | Se revisa la trazabilidad de los eventos y se ejecutan las pruebas, la suite y las verificaciones arquitectónicas aplicables. | Cada categoría aplicable del criterio aprobado tiene una comprobación automatizada; las pruebas residen en Tests, y Application conserva las dependencias permitidas. La configuración del proveedor y el registro HTTP no se presentan como evidencias de S4.10. |

Los ejemplos negativos y los datos prohibidos concretos se derivan de [specifications.md → «Registro de eventos»](../../context/specifications.md#registro-de-eventos); esta especificación no los redefine. La validación se limita a la instrumentación y a los valores que cruzan el límite de Application; la configuración efectiva de destinos, filtros y peticiones se comprueba en las etapas propietarias de Infrastructure y Web.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| No se identifica una contradicción funcional bloqueante para redactar esta especificación. Durante el plan se debe inventariar cada escenario/categoría del criterio aprobado y confirmar cómo comprobar excepciones de persistencia en Application sin adelantar la implementación del repositorio concreto. | [AC-TRANS-03](../../plans/mvp0-acceptance-criteria.md#ac-trans-03--registro-de-eventos), [specifications.md → «Registro de eventos»](../../context/specifications.md#registro-de-eventos) y [architecture.md → «Logs»](../../context/architecture.md#logs) | CA01, CA04 | Si la comprobación requiere cambiar una regla, el contrato de Application o una configuración de otra capa, detener ese punto y elevar una propuesta a su documento propietario antes de continuar. |

## Artefactos relacionados

- [Plan técnico](plan.md), propuesta con aprobación pendiente. Las tareas se prepararán solo después de la aprobación explícita del plan, según [sdd-writer](../../skills/sdd-writer/SKILL.md).
