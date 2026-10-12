# Especificación: S3.3 — Transiciones público y privado

## Estado y aprobación

- Estado: aceptada por DLG el 12-X-2026 («Implementado»).
- Aprobación: DLG, 12-X-2026 («Adelante»).
- Aceptación del resultado: DLG, 12-X-2026 («Implementado»), tras completar y validar los criterios CA01–CA08.

## Objetivo

Permitir en Domain las transiciones de privacidad de `Collection` y `Link` sin que ninguna operación deje un enlace público en una colección privada, incluyendo su creación y movimiento.

## Fuentes

- [mvp0-plan.md → «S3: Dominio III: entidades e invariantes (M)»](../../plans/mvp0-plan.md#s3-dominio-iii-entidades-e-invariantes-m): tarea S3.3 y reparto con S3.1, S3.2 y S3.4.
- [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado): estados y transiciones de colecciones y enlaces.
- [domain-model.md → «Collection»](../../context/domain-model.md#collection) y [«Link»](../../context/domain-model.md#link): propiedades e invariantes de las entidades afectadas.
- [architecture.md → «Persistencia»](../../context/architecture.md#persistencia): garantía transaccional de las operaciones que afectan a la privacidad de colecciones y enlaces.
- [spec S3.1 → «Alcance»](../s3.1-entidades-user-collection-link/spec.md#alcance): comportamiento inicial de las entidades y reparto de S3.3.
- [spec S3.2 → «Criterios de aceptación»](../s3.2-invariantes-propiedad/spec.md#criterios-de-aceptación): contrato de propiedad al crear y mover enlaces.

## Alcance

- Incluye:
  - Crear colecciones y enlaces con la privacidad solicitada explícitamente, manteniendo el estado privado cuando no se solicita hacerlos públicos.
  - Rechazar la creación de un enlace público cuando su colección sea privada.
  - Hacer pública o privada una colección; al hacerla privada, dejar privados también sus enlaces, y al hacerla pública, conservar la privacidad de sus enlaces.
  - Hacer público un enlace solo si su colección está pública, y permitir hacerlo privado.
  - Hacer privado un enlace al moverlo a otra colección, independientemente de la privacidad de origen y destino.
  - Evitar que cualquier operación disponible en Domain produzca o conserve un enlace público asociado a una colección privada.
- Excluye:
  - Autorización de peticiones, resolución del usuario identificado, lectura de datos ajenos y casos de uso; corresponden a Application y Web.
  - Persistencia, transacciones y coordinación de escrituras entre varias entidades; corresponden a S4 y S5 según la arquitectura.
  - Tests generales del conjunto de invariantes, que corresponden a S3.4; los tests necesarios para los criterios de esta spec sí forman parte de su validación.
  - Reglas de presentación de páginas públicas o privadas.
- Trabajo vecino y responsable de sus entregables:
  - S3.1 proporciona `User`, `Collection` y `Link` con estado inicial privado.
  - S3.2 garantiza la coherencia de propiedad entre enlace y colección.
  - S3.3 implementa las transiciones de público y privado en Domain.
  - S3.4 escribe los tests generales de invariantes; S3.5 genera las fichas de entidades y S3.6 las revisa.
  - S4 y S5 coordinan casos de uso y escrituras persistidas, incluidas las operaciones que deben ser atómicas.

## Dependencias y prerrequisitos

- S3.1 está implementada y validada; proporciona las entidades base y su estado inicial privado.
- S3.2 está implementada y validada; la creación y el movimiento de enlaces disponen de la comprobación de propiedad definida en su spec.
- Las reglas de producto necesarias están definidas en [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado); no se identificó una decisión funcional pendiente para esta tarea.
- S3.3 se limita a Domain. La garantía de atomicidad de las escrituras coordinadas queda fuera de esta tarea y deberá respetarse en las etapas de aplicación y persistencia.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado) y [«Collection»](../../context/domain-model.md#collection) | Los datos para crear una colección, con o sin solicitud explícita de hacerla pública. | Se crea la colección, esté vacía o no. | Sin solicitud explícita queda privada; si se solicita hacerla pública, queda pública, incluso si está vacía. |
| CA02 | [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado) | Una colección pública y los datos para crear un enlace. | Se crea el enlace sin solicitud de hacerlo público y con solicitud de hacerlo público. | El primer enlace queda privado y el segundo público. |
| CA03 | [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado) | Una colección privada y los datos para crear un enlace. | Se intenta crear el enlace como público. | La operación se rechaza y no produce un enlace público. |
| CA04 | [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado) | Una colección y los enlaces que le pertenecen. | Se hace pública la colección. | La colección queda pública y el estado de privacidad de sus enlaces no cambia; los enlaces privados permanecen privados. |
| CA05 | [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado) y [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) | Una colección pública con enlaces públicos y privados. | Se hace privada la colección. | La colección y todos sus enlaces quedan privados. |
| CA06 | [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado) | Un enlace privado en una colección pública y un enlace en una colección privada. | Se intenta hacer público cada enlace. | El enlace de la colección pública puede hacerse público; el enlace de la colección privada se rechaza y conserva su estado anterior. |
| CA07 | [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado) | Un enlace y una colección destino cuya propiedad es válida según S3.2. | Se mueve el enlace. | El enlace queda privado, tanto si era público o privado antes del movimiento como si la colección destino es pública o privada. |
| CA08 | [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado), [spec S3.2 → «Criterios de aceptación»](../s3.2-invariantes-propiedad/spec.md#criterios-de-aceptación) y [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura) | Las operaciones de creación, cambio de privacidad y movimiento implementadas en Domain. | Se prueban sus casos válidos, límites y rechazos, y se compila y ejecuta la suite de tests. | Ningún camino de Domain permite que un enlace quede público en una colección privada; los rechazos no dejan un estado parcialmente cambiado, y Domain no incorpora dependencias de otros proyectos ni de infraestructura. |

Los casos negativos de creación o cambio a público en una colección privada, y los rechazos sin mutación parcial, se comprueban en CA03, CA06 y CA08. Los casos de movimiento incluyen ambos estados iniciales y ambos estados destino en CA07.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno comprobado. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 12-X-2026 («Vamos»).
- [Tareas](tasks.md), aprobadas y cerradas con aceptación de DLG el 12-X-2026 («Implementado»).
