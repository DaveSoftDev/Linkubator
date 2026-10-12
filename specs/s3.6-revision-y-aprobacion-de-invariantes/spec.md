# Especificación: S3.6 — Revisión y aprobación de invariantes

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 12-X-2026 («Validado»).

## Objetivo

Contrastar las fichas derivadas de S3.5 con las entidades y tests de Domain, y registrar la decisión humana de DLG sobre las invariantes de `User`, `Collection` y `Link` aplicables al alcance de S3, sin presentar la revisión documental o los tests como aprobación humana.

## Fuentes

- [mvp0-plan.md → «S3: Dominio III: entidades e invariantes (M)»](../../plans/mvp0-plan.md#s3-dominio-iii-entidades-e-invariantes-m): responsabilidad y entregable S3.6.
- [roadmap.md → «Now»](../../context/roadmap.md#now): estado y trazabilidad vigente de S3.
- [domain-model.md → «User»](../../context/domain-model.md#user), [«Collection»](../../context/domain-model.md#collection), [«Link»](../../context/domain-model.md#link), [«Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [«Público y privado»](../../context/domain-model.md#público-y-privado): invariantes propietarias.
- [architecture.md → «Domain»](../../context/architecture.md#domain): responsabilidad y límites de la capa Domain.
- [S3.1 → «Criterios de aceptación»](../s3.1-entidades-user-collection-link/spec.md#criterios-de-aceptación), [S3.2 → «Criterios de aceptación»](../s3.2-invariantes-propiedad/spec.md#criterios-de-aceptación), [S3.3 → «Criterios de aceptación»](../s3.3-transiciones-publico-privado/spec.md#criterios-de-aceptación) y [S3.4 → «Criterios de aceptación»](../s3.4-tests-invariantes/spec.md#criterios-de-aceptación): contratos aprobados y cobertura de S3.1–S3.4.
- [S3.5 → «Criterios de aceptación»](../s3.5-fichas-entidades/spec.md#criterios-de-aceptación): propósito y requisitos de las fichas.
- [S3.5 — Ficha de `User`](../s3.5-fichas-entidades/user.md), [ficha de `Collection`](../s3.5-fichas-entidades/collection.md) y [ficha de `Link`](../s3.5-fichas-entidades/link.md): material derivado que se contrasta, no fuente de reglas.

## Alcance

- Incluye:
  - Contrastar las fichas de `User`, `Collection` y `Link` con las invariantes propietarias, la implementación actual de Domain y los tests existentes.
  - Valorar las invariantes aplicables al alcance MVP0 de S3; separar reglas de cuenta y demás reglas reservadas a etapas posteriores.
  - Registrar discrepancias, cobertura ausente y límites de lo que la API actual puede garantizar, con referencias al código y a los tests revisados.
  - Presentar a DLG los resultados y registrar su decisión explícita sobre la aprobación de las invariantes; una discrepancia funcional debe volver a su documento propietario antes de aprobarse.
  - Verificar el contrato de Domain de `Collection.ChangePrivacy`: exige la colección completa de enlaces asociados, rechaza conjuntos incompletos o duplicados antes de mutar y cumple las transiciones aprobadas. La carga de las entidades desde persistencia y la coordinación transaccional pertenecen a etapas posteriores.
- Excluye:
  - Cambiar código, tests, fichas o fuentes funcionales como parte de esta revisión. Si una decisión aprobada exige cambios, se tramitará en un trabajo posterior autorizado.
  - Aprobar por inferencia a partir de tests verdes, resultados históricos o la existencia de las fichas.
  - Aprobar reglas de cuenta, tokens, etiquetas y scraping reservadas a etapas posteriores.
  - Validar autorización de aplicación, persistencia, transacciones, concurrencia o comportamiento web, fuera del alcance de Domain en S3.
- Trabajo vecino y responsable de sus entregables:
  - S3.1 implementa las entidades; S3.2 y S3.3 implementan propiedad y transiciones de privacidad; S3.4 aporta los tests de invariantes; S3.5 genera las fichas derivadas.
  - S3.6 revisa esa evidencia y recoge la decisión humana; no reabre ni altera por sí misma los trabajos aceptados.

## Dependencias y prerrequisitos

- Las specs y tareas de S3.1–S3.4 registran implementación y validaciones aceptadas; sus criterios sirven de referencia, sin sustituir la revisión actual.
- S3.5 contiene las fichas necesarias y el roadmap registra que su entrega fue aceptada por DLG; ese estado no sustituye la revisión de las fichas contra el código ni la decisión de S3.6.
- `Collection.ChangePrivacy` recibe la secuencia de enlaces en Domain. Esta revisión comprueba que la entidad exige el conjunto exacto de enlaces que conoce y rechaza subconjuntos o duplicados antes de mutar. La consulta/carga desde persistencia y la atomicidad de las escrituras son responsabilidades posteriores de aplicación e infraestructura; su ausencia en S3 no bloquea la aceptación del contrato de Domain.
- Las fichas S3.5 citan fuentes mediante secciones, mientras CA01 de S3.5 requiere referencias verificables a archivo y línea. Verificar la trazabilidad y registrar la observación; no corregir las fichas dentro de este trabajo sin autorización.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [S3.5 → «Criterios de aceptación»](../s3.5-fichas-entidades/spec.md#criterios-de-aceptación), [domain-model.md → «User»](../../context/domain-model.md#user), [«Collection»](../../context/domain-model.md#collection) y [«Link»](../../context/domain-model.md#link), y criterios aprobados de [S3.1](../s3.1-entidades-user-collection-link/spec.md#criterios-de-aceptación) | Las tres fichas derivadas, las entidades de Domain y los criterios aprobados de S3.1. | Se contrasta cada afirmación de las fichas pertinente a S3 con su fuente propietaria, la implementación y los tests existentes. | Se registra por entidad qué invariantes del alcance coinciden, cuáles tienen cobertura comprobable y cuáles presentan discrepancias, lagunas o referencias insuficientes; las reglas reservadas a etapas posteriores quedan expresamente fuera de la aprobación S3. |
| CA02 | [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [S3.2 → «Criterios de aceptación»](../s3.2-invariantes-propiedad/spec.md#criterios-de-aceptación) | Las relaciones de propietario entre `Link` y `Collection` y los casos aprobados de S3.2. | Se revisan código y tests de creación y movimiento, incluidos rechazos y conservación del estado. | Se documenta si la implementación observada y sus tests respaldan los casos aprobados; cualquier hueco o discrepancia queda identificado, sin convertir revisión estática o resultados históricos en evidencia de una ejecución nueva. |
| CA03 | [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado), [S3.3 → «Criterios de aceptación»](../s3.3-transiciones-publico-privado/spec.md#criterios-de-aceptación) y [S3.4 → «Criterios de aceptación»](../s3.4-tests-invariantes/spec.md#criterios-de-aceptación) | Los casos de creación, transición y movimiento de colecciones y enlaces cubiertos por S3.3 y S3.4. | Se verifica el contrato de las entidades de Domain, incluidas las transiciones públicas/privadas, propiedad, rechazo de conjuntos incompletos o duplicados y ausencia de mutación parcial. | El código y los tests sustentan las invariantes del alcance de Domain. La obtención de entidades desde persistencia y la atomicidad de los casos de uso se reservan a sus etapas propietarias y no bloquean este criterio. |
| CA04 | [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado) y [S3.3 → «Criterios de aceptación»](../s3.3-transiciones-publico-privado/spec.md#criterios-de-aceptación) | El plan aprobado y las tareas de S3.6. | Se revisa el contrato de `Collection.ChangePrivacy` y sus usos disponibles dentro del alcance Domain. | `tasks.md` registra la inspección de referencias y tests. La ausencia de un consumidor de Application/Web se documenta como límite de etapa, no como incumplimiento de Domain; la validación del caso de uso y la transacción se realizará en las etapas de aplicación/persistencia. |
| CA05 | [architecture.md → «Domain»](../../context/architecture.md#domain) y [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) | Las entidades y tests de las invariantes de S3. | Se completa la revisión técnica y se presenta el resultado a DLG. | La decisión de DLG (aprobar, rechazar o solicitar cambios) queda registrada con fecha y referencia real, separada de las validaciones técnicas. |

Los casos negativos y las transiciones de privacidad se remiten a los criterios aprobados de S3.2–S3.4. Esta especificación no redefine sus reglas. Domain valida el conjunto exacto que recibe; la aplicación futura será responsable de cargar y persistir el agregado completo dentro de las garantías transaccionales definidas para esa etapa.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| No se identificó un consumidor de Application/Web porque esos casos de uso pertenecen a etapas posteriores; no bloquea la revisión del contrato de Domain. | [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura) y [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) | Ninguno en S3.6; registrar límite de alcance en CA04 | Validar la entidad y sus tests ahora; comprobar carga completa y transacción cuando se implemente el caso de uso correspondiente. |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 12-X-2026 («Adelante»).
- [Tareas](tasks.md), aprobadas por DLG el 12-X-2026 («Vamos»), con ejecución validada y decisión de cierre registrada allí.
