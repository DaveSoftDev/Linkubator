# Especificación: S3.5 — Fichas de entidades

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 12-X-2026 («Comprendido»).

## Objetivo

Generar fichas derivadas de las entidades del dominio incluidas en S3 y conservarlas como artefactos de revisión dentro de esta carpeta, para facilitar la comprobación de sus invariantes contra las fuentes aprobadas y el código sin convertirlas en fuentes adicionales de reglas.

## Fuentes

- [mvp0-plan.md → «S3: Dominio III: entidades e invariantes (M)»](../../../plans/mvp0-plan.md#s3-dominio-iii-entidades-e-invariantes-m): entregable S3.5 y revisión posterior S3.6.
- [domain-model.md → «User»](../../../context/domain-model.md#user), [«Collection»](../../../context/domain-model.md#collection) y [«Link»](../../../context/domain-model.md#link): modelo de las entidades incluidas en S3.
- [requirements-review → «Entity sheet»](../../../skills/requirements-review/SKILL.md#entity-sheet): procedimiento y formato de generación de las fichas.
- [AGENTS.md → «Dónde vive cada cosa»](../../../AGENTS.md#dónde-vive-cada-cosa): responsabilidad de las fuentes funcionales y de los artefactos SDD derivados.
- [architecture.md → «Domain»](../../../context/architecture.md#domain): papel de Domain en la arquitectura.
- S3.1–S3.4: sus criterios aprobados y trabajos archivados se consultan desde [specs-index.md → «Trabajos archivados»](../../specs-index.md#trabajos-archivados), como contexto del alcance y comportamiento implementado en S3.

## Alcance

- Incluye:
  - Generar mediante `requirements-review` una ficha para cada entidad de S3: `User`, `Collection` y `Link`.
  - Recoger en cada ficha las reglas y referencias de las fuentes de verdad aplicables a la entidad, y señalar contradicciones, duplicados u omisiones detectados según el procedimiento de la skill.
  - Guardar las fichas como archivos Markdown en `cards/` dentro de esta carpeta archivada para que S3.6 pueda revisarlas contra el código y decidir sobre las invariantes.
- Excluye:
  - Cambiar las fuentes funcionales, el código, los tests o el modelo. Cualquier hallazgo que requiera esos cambios se deriva a su documento propietario y no se resuelve dentro de este trabajo.
  - Generar fichas de `Tag`, `LinkTag` o `UserToken`, entidades que no forman parte del alcance de S3.
  - La revisión y aprobación humana del modelo, correspondiente a S3.6.
- Trabajo vecino y responsable de sus entregables:
  - S3.1 implementó las entidades; S3.2 y S3.3 implementaron propiedad y transiciones de privacidad; S3.4 cubrió mediante tests las invariantes aplicables.
  - S3.5 genera las fichas documentales derivadas.
  - S3.6 contrasta las fichas con el código y aprueba las invariantes; su revisión y decisión no se sustituyen por la generación de S3.5.

## Dependencias y prerrequisitos

- S3.1–S3.4 están implementadas y sus trabajos aceptados; sus criterios aprobados y tests sirven de contexto técnico para la revisión posterior.
- Las fuentes funcionales aplicables están en `context/`; las fichas deben enlazarlas y no reemplazarlas.
- La skill `requirements-review` contiene un procedimiento específico para generar fichas de entidad; esta spec adapta su forma de entrega para conservar los resultados como artefactos de revisión de S3.5.
- La comparación con el código y la aprobación humana quedan pendientes de S3.6.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [mvp0-plan.md → «S3: Dominio III: entidades e invariantes (M)»](../../../plans/mvp0-plan.md#s3-dominio-iii-entidades-e-invariantes-m), [requirements-review → «Entity sheet»](../../../skills/requirements-review/SKILL.md#entity-sheet) y [domain-model.md → «User»](../../../context/domain-model.md#user), [«Collection»](../../../context/domain-model.md#collection), [«Link»](../../../context/domain-model.md#link) | Las fuentes aprobadas y el código de las entidades de S3. | Se ejecuta el procedimiento de ficha para cada entidad incluida en el alcance. | Se entrega una ficha por `User`, `Collection` y `Link`, organizada según el procedimiento de la skill e incluyendo las reglas aplicables encontradas en las fuentes de verdad, con referencias verificables a archivo y línea. |
| CA02 | [requirements-review → «Entity sheet»](../../../skills/requirements-review/SKILL.md#entity-sheet) y [AGENTS.md → «Dónde vive cada cosa»](../../../AGENTS.md#dónde-vive-cada-cosa) | Las tres fichas generadas. | Se revisa su contenido y su forma de entrega. | Las fichas se guardan como Markdown en la carpeta de esta spec, no presentan reglas derivadas como una fuente nueva y no modifican sus documentos propietarios. |
| CA03 | [requirements-review → «Entity sheet»](../../../skills/requirements-review/SKILL.md#entity-sheet), [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos) y [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado) | El contenido de las fuentes consultadas para cada entidad. | Se completa cada ficha y se comprueban sus referencias y observaciones. | Se identifican al final de las fichas las contradicciones, duplicados u omisiones encontrados; si no se detectan, se declara expresamente. Los hallazgos que impliquen cambiar reglas quedan pendientes de decisión en su fuente propietaria y no se incorporan como cambios de esta spec. |

Los casos negativos, las invariantes de propiedad y las reglas de privacidad se recogen desde sus fuentes y criterios aprobados enlazados; esta especificación no los redefine. La revisión de correspondencia entre fichas y código, así como la aprobación de las invariantes, pertenece a S3.6.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno comprobado. | — | — | — |

## Artefactos relacionados

- Los archivos de fichas son [vistas derivadas](cards/collection.md), no fuentes funcionales independientes.
- [Plan técnico](plan.md), aprobado por DLG el 12-X-2026 («Adelante»).