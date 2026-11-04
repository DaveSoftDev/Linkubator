# Especificación: S12.3 — Auditoría de invariantes críticas en código y SQL

## Estado y aprobación

- Estado: aprobada.
- Preparada por IA el 31-X-2026.
- Aprobada por DLG el 31-X-2026 con la referencia «Siguiente».

## Objetivo

Auditar las invariantes críticas aplicables a la implementación actual, contrastando el código y el SQL pertinentes con sus fuentes propietarias, y entregar un informe trazable de la cobertura, los hallazgos y los límites de la revisión.

## Fuentes

- [AGENTS.md → «Invariantes críticas»](../../../AGENTS.md#invariantes-críticas)
- [mvp0-plan.md → «S12: Cierre y hardening (M)»](../../../plans/mvp0-plan.md#s12-cierre-y-hardening-m)
- [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos)
- [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado)
- [specifications.md → «Respuestas que no revelan si una cuenta existe»](../../../context/specifications.md#respuestas-que-no-revelan-si-una-cuenta-existe)
- [specifications.md → «Contraseñas»](../../../context/specifications.md#contraseñas)
- [specifications.md → «Tokens»](../../../context/specifications.md#tokens)
- [specifications.md → «Registro de eventos»](../../../context/specifications.md#registro-de-eventos)
- [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia)
- [decisions.md → «Pendiente de rutas de cuenta»](../../../context/decisions.md#pendiente-de-rutas-de-cuenta)

## Alcance

- Incluye:
  - Revisar las rutas de código y las instrucciones SQL que participan en las invariantes críticas de propiedad de datos y de estado público o privado.
  - Revisar el comportamiento de las operaciones de cuenta existentes en cuanto a respuestas que no revelen la existencia de una cuenta y al tratamiento de contraseñas y tokens.
  - Revisar los límites de las transacciones de base de datos frente a esperas, llamadas HTTP, scraping y envíos de correo.
  - Informar por invariante de la evidencia inspeccionada, los hallazgos y las partes no aplicables o no verificables en la implementación actual.
- Excluye:
  - Corregir hallazgos, cambiar reglas o modificar código, SQL, esquemas y migraciones.
  - Implementar operaciones de cuenta que estén pendientes de definición o fuera del alcance actual.
  - Auditar prácticas de seguridad que no formen parte de las invariantes críticas indicadas en las fuentes.
- Trabajo vecino y responsable de sus entregables: S12.2 cubre eventos y logs; S12.4 sincroniza la documentación; S12.7 revisa los hallazgos y acepta los riesgos restantes. Esta auditoría no asume sus entregables ni depende de que se completen.

## Dependencias y prerrequisitos

- Las invariantes y sus fuentes propietarias están documentadas y enlazadas arriba.
- No hay dependencia técnica de S12.2 ni de S12.4 para ejecutar la auditoría.
- Las operaciones de cuenta todavía no implementadas se identificarán conforme a [decisions.md → «Pendiente de rutas de cuenta»](../../../context/decisions.md#pendiente-de-rutas-de-cuenta); su ausencia no se tratará como incumplimiento de una operación implementada.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos) | La implementación contiene lecturas o modificaciones de datos privados y las instrucciones SQL relacionadas | Se contrastan sus controles de identidad, propiedad y relaciones entre datos en código y SQL | El informe identifica las rutas y garantías revisadas, con referencias a código y SQL, y registra cualquier discrepancia o límite de evidencia. |
| CA02 | [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado) | La implementación contiene consultas públicas, cambios de privacidad o persistencia relacionada | Se contrastan el acceso público y las transiciones de privacidad en código y SQL | El informe identifica los flujos y garantías revisados, con referencias a código y SQL, y registra cualquier discrepancia o límite de evidencia. |
| CA03 | [specifications.md → «Respuestas que no revelan si una cuenta existe»](../../../context/specifications.md#respuestas-que-no-revelan-si-una-cuenta-existe) | Hay operaciones de login, registro, recuperación o cambio de email implementadas en el alcance actual | Se comparan sus respuestas observables y, cuando corresponda, las consultas que las condicionan | El informe documenta las operaciones revisadas y cualquier diferencia de contenido o tipo de respuesta. Las operaciones no implementadas se identifican como fuera del alcance actual, no como incumplimientos de código ausente. |
| CA04 | [specifications.md → «Contraseñas»](../../../context/specifications.md#contraseñas), [«Tokens»](../../../context/specifications.md#tokens) y [«Registro de eventos»](../../../context/specifications.md#registro-de-eventos) | El código, el SQL o el registro de eventos manipulan contraseñas o tokens de operaciones de cuenta | Se rastrea su persistencia y exposición en las superficies de registro pertinentes | El informe documenta las rutas y superficies revisadas, con referencias a código y SQL, y registra cualquier almacenamiento o registro en claro, o cualquier límite de evidencia. |
| CA05 | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) | El código abre o usa transacciones de base de datos | Se revisa el trabajo realizado dentro de cada transacción y sus llamadas asociadas | El informe identifica los límites transaccionales inspeccionados y registra cualquier espera o llamada externa dentro de una transacción, o el límite de evidencia correspondiente. |
| CA06 | [mvp0-plan.md → «S12: Cierre y hardening (M)»](../../../plans/mvp0-plan.md#s12-cierre-y-hardening-m), [AGENTS.md → «Invariantes críticas»](../../../AGENTS.md#invariantes-críticas) | Se ha completado la revisión de las cinco invariantes anteriores | Se consolida el resultado de la auditoría | Hay un informe trazable que cubre cada invariante, identifica la evidencia revisada, distingue hallazgos de elementos no aplicables o no verificables y no presenta la revisión como una corrección de los hallazgos. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguna pregunta de alcance identificada en las fuentes disponibles. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md) — propuesta, pendiente de aprobación.
