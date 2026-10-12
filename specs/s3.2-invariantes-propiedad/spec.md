# Especificación: S3.2 — Invariantes de propiedad

## Estado y aprobación

- Estado: aprobada por DLG el 12-X-2026 («VAlidado»).
- Aprobación: DLG, 12-X-2026 («VAlidado»).

## Objetivo

Garantizar en Domain que cada `Link` pertenece a la misma persona propietaria que su `Collection`, tanto al crear el enlace como al moverlo.

## Fuentes

- [mvp0-plan.md → «S3: Dominio III: entidades e invariantes (M)»](../../plans/mvp0-plan.md#s3-dominio-iii-entidades-e-invariantes-m): tarea S3.2 y reparto con S3.1, S3.3 y S3.4.
- [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos): coherencia de propiedad entre enlace y colección.
- [domain-model.md → «Link»](../../context/domain-model.md#link): relación del enlace con un único usuario y una única colección, y posibilidad de moverlo.
- [domain-model.md → «Collection»](../../context/domain-model.md#collection): propiedad de la colección.
- [spec S3.1 → «Alcance»](../s3.1-entidades-user-collection-link/spec.md#alcance): límites acordados para esta tarea y comportamiento de las entidades base.

## Alcance

- Incluye:
  - Rechazar la creación de un `Link` cuya colección pertenezca a un `UserId` distinto.
  - Rechazar el movimiento de un `Link` a una colección de otro `UserId`.
  - Mantener consistente la propiedad del enlace y su colección en todas las operaciones de creación y movimiento disponibles en Domain.
  - Conservar la relación y el estado anteriores del enlace si un movimiento se rechaza.
- Excluye:
  - Autorizar lecturas o escrituras privadas según la identidad de la petición, y consultar datos de otros usuarios; corresponden a las capas de aplicación y persistencia.
  - Las invariantes y transiciones de público y privado (S3.3), los tests generales de invariantes (S3.4), las fichas de entidades (S3.5) y su revisión (S3.6).
  - La propiedad de etiquetas, que queda fuera del alcance comprometido del MVP0.
  - Persistencia, restricciones de base de datos y casos de uso.
- Trabajo vecino y responsable de sus entregables:
  - S3.1 proporciona `User`, `Collection` y `Link`.
  - S3.2 implementa en Domain la coherencia entre la propiedad del enlace y la de su colección.
  - S3.3 implementa público y privado; S3.4 escribe los tests generales de invariantes; S3.5 y S3.6 generan y revisan las fichas.
  - S4 y S5 implementan los casos de uso, la autorización de operaciones y la persistencia.

## Dependencias y prerrequisitos

- S3.1 está cerrada como implementada y validada; su spec, plan y tareas están en [specs/s3.1-entidades-user-collection-link](../s3.1-entidades-user-collection-link/spec.md).
- S3.2 modifica el comportamiento de Domain sin depender de persistencia ni de Application.
- La propiedad de etiquetas no se implementa en esta tarea, porque `Tag` y `LinkTag` están fuera del MVP0.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos), [«Link»](../../context/domain-model.md#link) y [«Collection»](../../context/domain-model.md#collection) | Un `Link` y una `Collection` del mismo usuario. | Se crea el enlace asociado a esa colección. | La operación se completa y el enlace conserva la colección recibida y el mismo `UserId` que esta. |
| CA02 | [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) | Un `Link` y una `Collection` con distintos `UserId`. | Se intenta crear el enlace con esa colección. | La operación se rechaza mediante el error de dominio correspondiente y no se obtiene un enlace con propiedad incoherente. |
| CA03 | [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [«Link»](../../context/domain-model.md#link) | Un `Link` y una colección destino del mismo usuario. | Se mueve el enlace a esa colección. | El movimiento se completa, cambia su colección y mantiene el `UserId` propietario. |
| CA04 | [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) | Un `Link` existente y una colección destino de otro usuario. | Se intenta mover el enlace a esa colección. | La operación se rechaza mediante el error de dominio correspondiente y el enlace conserva su colección y propiedad anteriores. |
| CA05 | [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura) | Las invariantes de propiedad implementadas. | Se compila la solución y se ejecutan los tests existentes y los de esta tarea. | La compilación y los tests pasan, y Domain no incorpora dependencias de otros proyectos ni de infraestructura. |

Los casos negativos de propietarios distintos y de operación rechazada se comprueban en CA02 y CA04. La cobertura general de invariantes corresponde a S3.4.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno comprobado. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md), propuesta pendiente de aprobación.
- Las tareas se prepararán después de la aprobación explícita del plan.