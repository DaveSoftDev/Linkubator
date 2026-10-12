# Especificación: S3.4 — Tests de invariantes del dominio

## Estado y aprobación

- Estado: implementada y cerrada por DLG el 12-X-2026 («Implementado»).
- Aprobación: DLG, 12-X-2026 («Implementado»).

## Objetivo

Asegurar mediante tests automatizados que las invariantes de `User`, `Collection` y `Link` aplicables al MVP0, incluida la propiedad y las transiciones de privacidad, quedan cubiertas en Domain sin duplicar innecesariamente las pruebas ya aportadas por S3.1–S3.3.

## Fuentes

- [mvp0-plan.md → «S3: Dominio III: entidades e invariantes (M)»](../../plans/mvp0-plan.md#s3-dominio-iii-entidades-e-invariantes-m): tarea S3.4 y Definition of Done.
- [domain-model.md → «User»](../../context/domain-model.md#user), [«Collection»](../../context/domain-model.md#collection) y [«Link»](../../context/domain-model.md#link): invariantes de las entidades incluidas en MVP0.
- [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos): coherencia de propiedad entre enlaces y colecciones.
- [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado): estados y transiciones de colecciones y enlaces.
- [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura): independencia de Domain y sus límites de dependencias.
- [spec S3.1 → «Criterios de aceptación»](../s3.1-entidades-user-collection-link/spec.md#criterios-de-aceptación): criterios aprobados de entidades y sus invariantes propias.
- [spec S3.2 → «Criterios de aceptación»](../s3.2-invariantes-propiedad/spec.md#criterios-de-aceptación): criterios aprobados de propiedad al crear y mover enlaces.
- [spec S3.3 → «Criterios de aceptación»](../s3.3-transiciones-publico-privado/spec.md#criterios-de-aceptación): criterios aprobados de creación y transiciones de privacidad.

## Alcance

- Incluye:
  - Revisar la cobertura de tests existente para las invariantes aplicables al MVP0 de `User`, `Collection` y `Link`, usando como referencia los criterios aprobados de S3.1.
  - Completar los tests que falten para verificar la coherencia de propiedad entre `Link` y `Collection` al crear y mover enlaces, incluidos los rechazos por propietarios distintos.
  - Completar los tests que falten para verificar el estado privado inicial, las condiciones de publicación y las transiciones de privacidad de colecciones y enlaces.
  - Comprobar con tests que los rechazos de operaciones de Domain no dejan mutaciones parciales en las entidades afectadas.
  - Mantener la trazabilidad entre las invariantes propietarias, los criterios aprobados de S3.1–S3.3 y los tests que las comprueban.
- Excluye:
  - Cambios de reglas de producto, entidades o comportamiento productivo de Domain; cualquier hallazgo que los requiera se deriva a su fuente propietaria y bloquea el criterio afectado hasta su aprobación.
  - Invariantes de cuentas, tokens, etiquetas y scraping reservadas a etapas posteriores, y las comprobaciones de existencia de datos que requieren Application o persistencia.
  - Autorización de peticiones, restricciones de base de datos, transacciones, concurrencia de persistencia, integración HTTP y tests de interfaz.
  - La generación y revisión de fichas de entidades, que corresponden a S3.5 y S3.6.
- Trabajo vecino y responsable de sus entregables:
  - S3.1 implementa las entidades y sus invariantes propias; sus criterios aprobados son la base para revisar la cobertura ya existente.
  - S3.2 implementa la coherencia de propiedad de enlaces y colecciones.
  - S3.3 implementa las transiciones de público y privado.
  - S3.4 revisa y completa los tests de las invariantes aplicables al MVP0; no sustituye las pruebas específicas que validaron S3.1–S3.3.
  - S3.5 genera las fichas de entidades y S3.6 las revisa y aprueba.

## Dependencias y prerrequisitos

- S3.1 está implementada y validada; sus criterios CA01–CA14 y sus tests permiten reutilizar la cobertura de las invariantes propias de las entidades.
- S3.2 y S3.3 están implementadas, probadas y aceptadas; sus criterios aprobados fijan los casos de propiedad y privacidad que deben quedar cubiertos.
- Las reglas aplicables al MVP0 están definidas en `domain-model.md`; no se requiere una decisión funcional nueva para escribir los tests.
- La revisión debe distinguir las invariantes comprobables en Domain de las que requieren datos, autorización o coordinación de capas posteriores.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [domain-model.md → «User»](../../context/domain-model.md#user), [«Collection»](../../context/domain-model.md#collection), [«Link»](../../context/domain-model.md#link) y [spec S3.1 → «Criterios de aceptación»](../s3.1-entidades-user-collection-link/spec.md#criterios-de-aceptación) | Las invariantes propias de las entidades que son aplicables al MVP0 y los tests de S3.1 existentes. | Se revisa la cobertura trazando cada invariante comprobable en Domain a un test. | Cada invariante aplicable tiene cobertura existente o se añade el test que falta; las reglas que dependen de datos externos quedan identificadas como fuera de esta comprobación, sin duplicar pruebas equivalentes ya aprobadas en S3.1. |
| CA02 | [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [spec S3.2 → «Criterios de aceptación»](../s3.2-invariantes-propiedad/spec.md#criterios-de-aceptación) | Enlaces y colecciones con igual o distinto propietario. | Se comprueban mediante tests la creación y el movimiento a una colección. | Los casos permitidos mantienen la propiedad coherente y los casos con propietarios distintos se rechazan conservando el estado anterior cuando ya existe un enlace. |
| CA03 | [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado) y [spec S3.3 → «Criterios de aceptación»](../s3.3-transiciones-publico-privado/spec.md#criterios-de-aceptación) | Colecciones y enlaces en los estados y relaciones previstos por la fuente. | Se comprueban mediante tests la creación, el cambio de privacidad y el movimiento de enlaces. | Los casos válidos y rechazados cubren las transiciones aprobadas de S3.3, incluida la imposibilidad de dejar público un enlace en una colección privada. |
| CA04 | [spec S3.2 → «Criterios de aceptación»](../s3.2-invariantes-propiedad/spec.md#criterios-de-aceptación) y [spec S3.3 → «Criterios de aceptación»](../s3.3-transiciones-publico-privado/spec.md#criterios-de-aceptación) | Una operación de Domain que debe rechazarse por propiedad, privacidad o relación inconsistente. | El test ejecuta la operación y comprueba las entidades después del rechazo. | Se demuestra que no queda una mutación parcial en el estado de colección o enlace afectado. |
| CA05 | [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura) y [mvp0-plan.md → «S3: Dominio III: entidades e invariantes (M)»](../../plans/mvp0-plan.md#s3-dominio-iii-entidades-e-invariantes-m) | Los tests de invariantes de S3.1–S3.4 y la solución. | Se ejecutan la suite completa y la compilación. | La suite pasa y Domain conserva los límites de dependencia definidos por la arquitectura. |

Los casos negativos de propiedad, publicación en colecciones privadas y rechazo sin mutación parcial se comprueban en CA02–CA04. Los criterios aprobados de S3.2 y S3.3 son las referencias para el detalle de esos casos; esta especificación no vuelve a definir sus reglas.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno comprobado. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 12-X-2026 («Adelante»).
- [Tareas](tasks.md), aprobadas por DLG el 12-X-2026 («Vamos») y ejecutadas con evidencia final.
- Implementación concluida y validada con la suite completa de la solución.
