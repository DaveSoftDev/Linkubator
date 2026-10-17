# Especificación: S4.9 — Fakes en memoria y pruebas de casos de uso

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Objetivo

Completar y mantener fakes en memoria y pruebas automatizadas de los casos de uso S4.6–S4.8, con cobertura trazable a los criterios aprobados del MVP0, incluidos los casos de propiedad entre usuarios.

## Fuentes

- [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l): alcance, orden, responsabilidad de fakes y tests y criterio de cobertura por criterio aprobado.
- [roadmap.md → «Now»](../../context/roadmap.md#now): etapa activa y estado de S4.6–S4.8.
- [Catálogo de criterios de aceptación del MVP0 → «Colecciones»](../../plans/mvp0-acceptance-criteria.md#colecciones), [«Enlaces»](../../plans/mvp0-acceptance-criteria.md#enlaces), [«Dashboard y páginas públicas»](../../plans/mvp0-acceptance-criteria.md#dashboard-y-páginas-públicas) y [«Reglas transversales»](../../plans/mvp0-acceptance-criteria.md#reglas-transversales): criterios aprobados propietarios del comportamiento bajo prueba.
- [architecture.md → «Application»](../../context/architecture.md#application), [«Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos) y [«Persistencia»](../../context/architecture.md#persistencia): contratos de capa, límites de dependencias y separación de garantías de persistencia.
- [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [«Público y privado»](../../context/domain-model.md#público-y-privado): invariantes que deben ejercitarse en los escenarios aplicables.
- [S4.4 — criterios de aceptación de los casos de uso](../s4.4-criterios-aceptacion-casos-uso/spec.md) y [S4.5 — revisión y aprobación de los criterios de aceptación](../s4.5-aprobacion-criterios-aceptacion/spec.md): preparación y aprobación del catálogo propietario.
- [S4.6 — casos de uso de colecciones](../s4.6-casos-uso-colecciones/spec.md), [S4.7 — casos de uso de enlaces](../s4.7-casos-uso-enlaces/spec.md) y [S4.8 — dashboard y lecturas públicas](../s4.8-dashboard-y-lecturas-publicas/spec.md): alcance implementado cuyos casos de uso se validan aquí.

## Alcance

- Incluye:
  - Fakes en memoria de los contratos necesarios para probar los casos de uso de colecciones, enlaces, dashboard y lecturas públicas de S4.6–S4.8.
  - Pruebas automatizadas de comportamiento de esos casos de uso, asociadas a los criterios aprobados correspondientes y con escenarios satisfactorios y de rechazo donde apliquen.
  - Cobertura de identidad fiable y propiedad entre usuarios, privacidad, resultados y errores de Application, y coordinación observable entre servicios y contratos, conforme a los criterios propietarios.
  - Revisión de cobertura trazable desde cada criterio aprobado bajo prueba hasta sus pruebas automatizadas.
- Excluye:
  - Cambiar reglas funcionales o criterios aprobados, ni ampliar los contratos de producción por conveniencia de los fakes. Una carencia o contradicción bloqueante se resuelve en su fuente propietaria antes de continuar.
  - Implementar o modificar los casos de uso de producción ya asignados a S4.6–S4.8, salvo correcciones aprobadas que se planifiquen como trabajo separado y no se presuman incluidas aquí.
  - Implementar repositorios concretos, SQL, SQLite, Dapper, FTS5, migraciones o garantías físicas/concurrentes de persistencia, que pertenecen a S5–S6.
  - Probar rutas, respuestas HTTP, renderizado, accesibilidad visual o interacción de Web; corresponden a sus etapas propietarias.
  - Implementar o validar registro de eventos y contenido de logs, asignado a S4.10.
  - Introducir pruebas de navegador, herramientas de automatización de UI o dependencias ajenas a las fuentes aprobadas.
- Trabajo vecino y responsable de sus entregables:
  - S4.6, S4.7 y S4.8 implementan los casos de uso de Application que se prueban aquí.
  - S4.10 implementa el registro de eventos.
  - S5–S6 implementan persistencia concreta y validan sus restricciones, transacciones y proyecciones.
  - Las etapas Web validan páginas, contratos HTTP y presentación.

## Dependencias y prerrequisitos

- S4.5 aprobó el catálogo de criterios de aceptación el 16-X-2026; la puerta de criterios está satisfecha.
- S4.6, S4.7 y S4.8 están completadas y aceptadas para sus respectivos alcances de Application; sus servicios y contratos están disponibles para probar.
- En el estado inspeccionado, existen pruebas de Application para colecciones y enlaces con fakes en memoria; no se encontraron pruebas específicas de dashboard o lecturas públicas. La cobertura actual debe inventariarse y verificarse contra cada criterio al preparar el plan, sin inferir cumplimiento por la existencia de tests.
- Las pruebas con fakes verifican el comportamiento de Application frente a los contratos simulados; no prueban la implementación concreta de Infrastructure ni sustituyen las validaciones de Web.
- La aprobación de esta especificación autoriza preparar el plan técnico. No autoriza crear `tasks.md` ni iniciar cambios productivos; las tareas se preparan únicamente después de la aprobación explícita del plan.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [AC-COL-01–AC-COL-05 y AC-COL-01b](../../plans/mvp0-acceptance-criteria.md#colecciones), [S4.6 → «Criterios de aceptación»](../s4.6-casos-uso-colecciones/spec.md#criterios-de-aceptación) | Los casos de uso de colecciones y sus contratos están disponibles junto con escenarios válidos, inválidos, límite y de distintos usuarios definidos por sus criterios propietarios. | Se ejecuta la batería automatizada de colecciones con los fakes en memoria. | Cada criterio aprobado de colecciones queda cubierto por una o más pruebas que comprueban el resultado correspondiente, incluidos los rechazos aplicables y que los fallos no dejan mutaciones observables indebidas en los fakes. |
| CA02 | [AC-LINK-01–AC-LINK-06 y AC-LINK-01b](../../plans/mvp0-acceptance-criteria.md#enlaces), [S4.7 → «Criterios de aceptación»](../s4.7-casos-uso-enlaces/spec.md#criterios-de-aceptación) | Los casos de uso de enlaces y sus contratos están disponibles junto con escenarios válidos, inválidos, límite y de distintos usuarios definidos por sus criterios propietarios. | Se ejecuta la batería automatizada de enlaces con los fakes en memoria. | Cada criterio aprobado de enlaces queda cubierto por una o más pruebas de sus resultados exitosos y rechazos aplicables, incluida la propiedad y la ausencia de cambios indebidos tras un rechazo. |
| CA03 | [AC-DASH-01, AC-USER-01, AC-COLLS-01 y AC-COLLECTION-01](../../plans/mvp0-acceptance-criteria.md#dashboard-y-páginas-públicas), [S4.8 → «Criterios de aceptación»](../s4.8-dashboard-y-lecturas-publicas/spec.md#criterios-de-aceptación) | Los casos de uso de dashboard y lecturas públicas están disponibles junto con escenarios de identidad, localizadores, ausencia, contenido vacío y privacidad según las fuentes propietarias. | Se ejecuta la batería automatizada de dashboard y lecturas públicas con los fakes en memoria. | Cada criterio aprobado queda cubierto en el límite de Application: la identidad privada procede de su abstracción, los localizadores públicos no otorgan acceso privado y los resultados/errores se ajustan al contrato; las consultas concretas y la respuesta HTTP no se atribuyen a estas pruebas. |
| CA04 | [AC-TRANS-01](../../plans/mvp0-acceptance-criteria.md#ac-trans-01--identidad-propiedad-y-límites-de-acceso), [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [«Público y privado»](../../context/domain-model.md#público-y-privado) | Los escenarios de caso de uso incluyen identidades y recursos de usuarios distintos, y operaciones privadas o lecturas públicas según corresponda. | Se ejecutan las pruebas de propiedad y límites de acceso. | Las pruebas demuestran que los casos privados usan la identidad fiable y no acceden a recursos ajenos, y que los localizadores de lecturas públicas no sirven para recuperar datos privados; los casos aplicables tienen cobertura automatizada explícita. |
| CA05 | [AC-TRANS-02](../../plans/mvp0-acceptance-criteria.md#ac-trans-02--errores-y-mensajes-accesibles), [architecture.md → «Application»](../../context/architecture.md#application) y [S4.1 → «Criterios de aceptación»](../s4.1-contratos-aplicacion/spec.md#criterios-de-aceptación) | Los casos probados pueden devolver errores de validación, dominio o recurso no accesible conforme a sus criterios propietarios. | Las pruebas examinan los resultados devueltos y el estado de los fakes. | Los errores corresponden a los contratos y códigos de Application exigidos por cada criterio, no contienen textos de presentación ni revelan datos privados, y los rechazos no producen mutaciones no autorizadas. |
| CA06 | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l), [architecture.md → «Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos) y [AGENTS.md → «Convención de estructura del código»](../../AGENTS.md#convención-de-estructura-del-código) | Los fakes y pruebas de comportamiento de S4.6–S4.8 están implementados en el proyecto de tests. | Se revisan organización, matriz de cobertura, dependencias y ejecución de la suite. | Cada criterio funcional aplicable del catálogo aprobado para S4.6–S4.8 se vincula a una o más pruebas automatizadas ejecutables; los fakes permanecen en Tests y no introducen dependencias de producción ni alteran los contratos que simulan. |

Los escenarios negativos, límites, mensajes y resultados exactos se comprueban directamente contra los criterios y fuentes enlazados; esta especificación no los vuelve a definir. Las pruebas con fakes solo acreditan el comportamiento del caso de uso respecto de los contratos simulados. La integridad SQL, concurrencia física, consultas de búsqueda, HTTP y presentación se validan en sus etapas propietarias.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| No se identifica una contradicción funcional bloqueante para redactar esta especificación. Si el inventario del plan revela un criterio aprobado que no puede probarse con los contratos existentes, detener la cobertura afectada y elevar la insuficiencia a su documento propietario, sin cambiar reglas ni contratos por inferencia. | [Catálogo de criterios de aceptación del MVP0](../../plans/mvp0-acceptance-criteria.md), [architecture.md → «Application»](../../context/architecture.md#application) y contratos de S4.1–S4.3 | El criterio afectado | Resolver y aprobar el cambio en su fuente propietaria antes de ampliar contratos o pruebas. |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 17-X-2026 («Siguiente»); [tareas](tasks.md), aprobadas por DLG el 17-X-2026 («Siguiente»). La implementación requiere instrucción explícita de inicio, según [sdd-writer](../../skills/sdd-writer/SKILL.md).
