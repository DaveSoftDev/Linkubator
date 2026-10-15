# Especificación: S4.3 — Contrato del proveedor de identidad de desarrollo

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 15-X-2026 («Aprobada»).

## Objetivo

Definir en Application el contrato que permite proporcionar a la aplicación la identidad de desarrollo configurada, separando esa resolución de la abstracción de usuario identificado consumida por los casos de uso y sin permitir que la petición elija la identidad.

## Fuentes

- [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l): alcance de S4.3 y separación respecto a la implementación del proveedor en S5.7.
- [roadmap.md → «Now»](../../context/roadmap.md#now): etapa activa y estado de los trabajos S4.
- [decisions.md → «Identidad de desarrollo»](../../context/decisions.md#identidad-de-desarrollo): propósito y criterio estable de aprovisionamiento de la identidad.
- [architecture.md → «Application»](../../context/architecture.md#application), [«Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos) y [«Autenticación»](../../context/architecture.md#autenticación): responsabilidades de las capas, registro y resolución técnica del proveedor.
- [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [AGENTS.md → «Invariantes críticas»](../../AGENTS.md#invariantes-críticas): límites de propiedad y origen confiable de la identidad.
- [S4.2 — Contratos de repositorios, Unit of Work e identidad identificada](../s4.2-contratos-repositorios-identidad/spec.md) y [sus tareas → «Matriz de operaciones MVP0 → contratos»](../s4.2-contratos-repositorios-identidad/tasks.md#matriz-de-operaciones-mvp0--contratos): abstracción existente de usuario identificado y contratos ya definidos.

## Alcance

- Incluye:
  - El contrato de Application que necesita la composición de la aplicación para resolver la identidad de desarrollo y conectarla con la abstracción de usuario identificado definida en S4.2.
  - La separación contractual entre resolución de la identidad de desarrollo y su consumo por las operaciones de Application.
  - Criterios verificables de registro exclusivo en desarrollo, uso de la configuración aprobada y fallo de arranque si no se encuentra una cuenta completada.
- Excluye:
  - Implementar o registrar el proveedor concreto, consultar la base de datos al arrancar o resolver la identidad por petición; esas tareas corresponden a S5.7.
  - Provisionar la cuenta o la colección base, modificar el SQL local, el esquema o los repositorios.
  - Implementar casos de uso, autenticación real, sesiones o aceptar identidad desde datos de la petición.
  - Cambiar las reglas de identidad de desarrollo, propiedad de datos o alcance de otras etapas.
- Trabajo vecino y responsable de sus entregables:
  - S4.2 define la abstracción de usuario identificado que consume Application.
  - S4.3 define este contrato, no su proveedor concreto.
  - S5.7 implementa el proveedor, su registro exclusivo en desarrollo y la resolución de la cuenta aprovisionada.
  - S4.4–S4.5 definen y aprueban los criterios de aceptación de los casos de uso; S4.6–S4.10 implementan esos casos y sus tests/logs.

## Dependencias y prerrequisitos

- S4.2 está cerrada y aceptada para su alcance. Define `ICurrentUser` y `CurrentUser` en Application; su contrato constituye el punto de integración de este trabajo.
- La decisión y las instrucciones técnicas de identidad de desarrollo están documentadas en [decisions.md → «Identidad de desarrollo»](../../context/decisions.md#identidad-de-desarrollo) y [architecture.md → «Autenticación»](../../context/architecture.md#autenticación).
- La implementación del contrato de Application está completada y aceptada para el alcance de S4.3. S5.7 sigue siendo responsable del proveedor concreto y depende de que la persistencia y el aprovisionamiento necesarios estén disponibles.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [architecture.md → «Application»](../../context/architecture.md#application) y [«Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos) | El contrato del proveedor de identidad de desarrollo. | Se revisan su ubicación y sus tipos públicos. | El contrato pertenece a Application, puede ser implementado por Infrastructure y no introduce dependencias de Infrastructure, Web, SQLite, Dapper o HTTP en Application. |
| CA02 | [architecture.md → «Autenticación»](../../context/architecture.md#autenticación), [S4.2 → «Criterios de aceptación»](../s4.2-contratos-repositorios-identidad/spec.md#criterios-de-aceptación) y [sus tareas → «Matriz de operaciones MVP0 → contratos»](../s4.2-contratos-repositorios-identidad/tasks.md#matriz-de-operaciones-mvp0--contratos) | El proveedor de desarrollo y la abstracción de usuario identificado de Application. | Se revisa su relación contractual. | El contrato distingue la resolución de la identidad configurada de su consumo como usuario identificado; no duplica ni sustituye `ICurrentUser` y permite que las operaciones privadas reciban esa identidad sin datos autoritativos de la petición. |
| CA03 | [architecture.md → «Autenticación»](../../context/architecture.md#autenticación) y [decisions.md → «Identidad de desarrollo»](../../context/decisions.md#identidad-de-desarrollo) | La configuración de desarrollo y el usuario que debe resolverse. | Se revisan las entradas que el contrato permite usar para identificar al usuario. | La resolución se basa en el email configurado y estable, no en un `Id` generado ni en el alias mutable; el contrato no permite que un `UserId`, `Alias` o `Slug` de la petición determine la identidad. |
| CA04 | [architecture.md → «Autenticación»](../../context/architecture.md#autenticación) | El proveedor concreto que consume el contrato en el entorno de desarrollo. | Se revisa el comportamiento de resolución requerido en el punto de arranque y en las peticiones privadas. | El contrato permite entregar el `UserId` de la cuenta resuelta a `/app` y exige que la ausencia de una cuenta completada impida el arranque, sin recurrir a una identidad aportada por el cliente. |
| CA05 | [architecture.md → «Autenticación»](../../context/architecture.md#autenticación) y [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | El registro del proveedor en la composición de la aplicación. | Se revisa su límite por entorno y la asignación de responsabilidades entre S4.3 y S5.7. | El contrato sirve al proveedor previsto para desarrollo sin definir un mecanismo de identidad para otros entornos; su registro exclusivo en desarrollo y su implementación permanecen en S5.7. |

Los casos negativos y las condiciones de la cuenta se mantienen enlazados a sus fuentes. La comprobación de ejecución del arranque, el registro por entorno y la resolución por petición corresponde a S5.7, no a este contrato aislado.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguna pendiente dentro del alcance S4.3. La firma se definió en el plan aprobado y la implementación quedó aceptada por DLG. | [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos) y [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre) | Ninguno. | No se requiere otra decisión para cerrar S4.3. |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 15-X-2026 («Adelante»).
- [Tareas](tasks.md), aprobadas por DLG el 15-X-2026 («Vamos»); implementación del alcance S4.3 aceptada por DLG el 15-X-2026 («Terminado»).
