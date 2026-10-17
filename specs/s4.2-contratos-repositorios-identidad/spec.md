# Especificación: S4.2 — Contratos de repositorios, Unit of Work e identidad identificada

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 15-X-2026 («Planifica»).

## Objetivo

Definir en Application los contratos de acceso a datos, coordinación de operaciones y obtención de la identidad del usuario identificado que necesitan los casos de uso MVP0, manteniendo las dependencias entre capas y la propiedad de los datos.

## Fuentes

- [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l): alcance y dependencia de S4.2, operaciones cubiertas por S4 y separación respecto a los trabajos posteriores.
- [roadmap.md → «Now»](../../context/roadmap.md#now): etapa activa y puerta previa a la implementación de casos de uso.
- [architecture.md → «Application»](../../context/architecture.md#application), [«Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos), [«Autenticación»](../../context/architecture.md#autenticación) y [«Persistencia»](../../context/architecture.md#persistencia): responsabilidades de las abstracciones, dirección de dependencias, origen técnico de la identidad y garantías de unidad de trabajo.
- [decisions.md → «Identidad de desarrollo»](../../context/decisions.md#identidad-de-desarrollo): provisión y resolución de la identidad de desarrollo.
- [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [«Público y privado»](../../context/domain-model.md#público-y-privado): invariantes que deben conservar las operaciones privadas y públicas.
- [S4.1 — Result, errores, DTOs y validadores de aplicación](../s4.1-contratos-aplicacion/spec.md): contratos de resultados y DTOs que consumen o complementan estos contratos.

## Alcance

- Incluye:
  - Contratos de repositorio requeridos por los casos MVP0 de colecciones, enlaces, dashboard y lecturas públicas enumerados en S4.
  - Contrato de Unit of Work para coordinar las operaciones de datos que requieren atomicidad, sin imponer a Application una tecnología de persistencia.
  - Abstracción de Application para obtener la identidad del usuario de la ejecución actual, separada de los datos de entrada controlados por el cliente.
  - Separación entre la abstracción consumida por los casos de uso y el proveedor concreto de identidad de desarrollo, que se define en S4.3.
- Excluye:
  - Implementaciones de repositorios, conexiones, transacciones concretas, SQL, persistencia, fakes y casos de uso.
  - El contrato e implementación del proveedor de identidad de desarrollo, responsabilidad de S4.3.
  - La definición de criterios de aceptación funcionales por caso de uso, responsabilidad de S4.4, y su aprobación en S4.5.
  - Cambios a las reglas de negocio, propiedad, privacidad, respuestas o alcance de MVP0.
  - Contratos de autenticación real, cuentas, etiquetas y otras capacidades posteriores no incluidas en los casos de uso S4 del plan MVP0.
- Trabajo vecino y responsable de sus entregables:
  - S4.1 define `Result`, errores, DTOs y validadores.
  - S4.3 define el contrato del proveedor de identidad de desarrollo.
  - S4.4 define los criterios de aceptación funcionales por caso de uso; S4.5 los somete a aprobación humana.
  - S4.6–S4.8 implementan casos de uso; S4.9 añade fakes y tests de esos casos; S4.10 registra los eventos permitidos.

## Dependencias y prerrequisitos

- S3 está completada y las entidades e invariantes aplicables están aceptadas.
- La especificación y el plan de S4.1 están aprobados; su implementación está en curso. Antes de aprobar el plan técnico de S4.2 se deberá contrastar el diseño con los contratos de S4.1 ya implementados y aceptados, sin asumir firmas todavía no verificadas.
- La solución contiene `Linkubator.Application` y conserva la matriz de dependencias documentada. La redacción de esta especificación no inicia la implementación de los contratos.
- S4.2 depende de S4.1 según el orden del plan MVP0. La implementación de los casos de uso S4.6–S4.10 sigue bloqueada hasta la aprobación de los criterios de S4.5.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [architecture.md → «Application»](../../context/architecture.md#application) y [«Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos) | Los contratos de repositorios, Unit of Work e identidad identificada de Application. | Se revisan su ubicación y las referencias entre proyectos. | Pertenecen a Application y no introducen dependencias de Infrastructure, Web, SQLite, Dapper ni HTTP. |
| CA02 | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l), [architecture.md → «Application»](../../context/architecture.md#application) y [S4.1 → «Criterios de aceptación»](../s4.1-contratos-aplicacion/spec.md#criterios-de-aceptación) | El catálogo de operaciones MVP0 y los contratos de datos definidos por S4.1. | Se revisa la matriz de operaciones frente a los contratos de repositorio. | Las operaciones de colecciones, enlaces, dashboard y lecturas públicas disponen de acceso a datos mediante contratos de Application, sin exponer modelos de persistencia ni omitir operaciones del alcance. |
| CA03 | [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos), [AGENTS.md → «Invariantes críticas»](../../AGENTS.md#invariantes-críticas) y [architecture.md → «Autenticación»](../../context/architecture.md#autenticación) | Una operación privada y el contrato de identidad consumido por Application. | Se inspeccionan los DTOs y las firmas públicas de los contratos. | La identidad procede de la abstracción de usuario identificado, no de un `UserId`, `Alias` o `Slug` autoritativo enviado por el cliente; los contratos permiten a los casos de uso limitar las operaciones privadas a esa identidad y comprobar la propiedad de recursos y relaciones. |
| CA04 | [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos), [«Público y privado»](../../context/domain-model.md#público-y-privado) y [architecture.md → «Casos de uso iniciales»](../../context/architecture.md#casos-de-uso-iniciales) | Operaciones privadas y lecturas públicas de los casos MVP0. | Se revisa qué identidad requieren los contratos y qué datos pueden leer. | El contrato distingue las necesidades de operaciones privadas de las lecturas públicas y no permite que una ruta pública conceda acceso a datos privados. |
| CA05 | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) | Una operación compuesta de escritura que deba ser atómica. | Se revisa el contrato de Unit of Work y su uso previsto por Application. | El contrato permite coordinar una unidad de trabajo y sus operaciones atómicas sin filtrar transacciones concretas ni permitir que las operaciones externas o la espera al usuario formen parte de una transacción. |
| CA06 | [architecture.md → «Application»](../../context/architecture.md#application), [«Autenticación»](../../context/architecture.md#autenticación) y [decisions.md → «Identidad de desarrollo»](../../context/decisions.md#identidad-de-desarrollo) | El contrato de identidad de Application y el proveedor de identidad de desarrollo de S4.3. | Se revisan sus responsabilidades y dependencias. | Application consume una abstracción independiente del modo de provisión; el contrato no incorpora detalles del usuario de desarrollo ni sustituye el entregable de S4.3. |
| CA07 | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) y [«Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos) | Los contratos públicos definidos para repositorios y Unit of Work. | Se revisan sus responsabilidades y alcance de operaciones. | Las responsabilidades son suficientes y acotadas al acceso a datos y coordinación requeridos por S4; no trasladan reglas de negocio desde Domain ni acoplan Application a la futura implementación de Infrastructure. |

Los casos negativos y límites de cada operación se concretarán en los criterios S4.4 a partir de sus fuentes propietarias; esta especificación no redefine el comportamiento funcional ni los detalles de las transacciones.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Las fuentes no fijan nombres ni granularidad de los repositorios, firmas de métodos o forma concreta del ciclo de vida de Unit of Work y de la abstracción de identidad. | [architecture.md → «Application»](../../context/architecture.md#application) y [«Persistencia»](../../context/architecture.md#persistencia) | Ninguno para esta especificación; la propuesta de diseño debe quedar en `plan.md`. | Revisar y aprobar las decisiones técnicas en el plan antes de crear `tasks.md`. |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 15-X-2026 («Adelante»).
