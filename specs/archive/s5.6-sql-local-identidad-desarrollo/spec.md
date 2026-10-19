# Especificación: S5.6 — Aprovisionamiento SQL de identidad de desarrollo

## Estado y aprobación

- Estado: aprobada; implementación ejecutada, evidencia y aceptación registradas en [tasks.md](tasks.md).
- Aprobación: DLG, 19-X-2026 («Validado y confirmado»), instrucción explícita en esta conversación.

## Objetivo

Proveer en todos los entornos, durante cada arranque y antes de resolver la identidad privada, el esquema migrado y una cuenta de desarrollo utilizable mediante un aprovisionamiento SQL idempotente, separado del catálogo de migraciones versionadas.

## Fuentes

- [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l): entregable S5.6 y comprobación manual S5.10.
- [roadmap.md → «Persistencia»](../../../context/roadmap.md#persistencia): alcance de la etapa y criterio de identidad de desarrollo.
- [architecture.md → «Autenticación»](../../../context/architecture.md#autenticación): aprovisionamiento e identidad de desarrollo.
- [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia): ejecución de scripts versionados y restricciones de datos.
- [decisions.md → «Identidad de desarrollo»](../../../context/decisions.md#identidad-de-desarrollo): identificación estable por email y origen del hash inicial.
- [domain-model.md → «User»](../../../context/domain-model.md#user) y [«Collection»](../../../context/domain-model.md#collection): estado completado, colección base y privacidad.
- [specifications.md → «Completar el registro»](../../../context/specifications.md#completar-el-registro): estado de perfil y colección privada base.
- [specifications.md → «Identidad de desarrollo»](../../../context/specifications.md#identidad-de-desarrollo): comportamiento y valores iniciales del aprovisionamiento.
- [S5.2 — ejecutor de scripts SQL versionados → «Persistencia y dependencias»](../s5.2-ejecutor-scripts-sql-versionados/plan.md#persistencia-y-dependencias): el aprovisionamiento queda fuera del catálogo e historial de migraciones.
- [S5.3 — esquema de `User`, `Collection` y `Link`](../s5.3-esquema-user-collection-link/spec.md): esquema sobre el que se aprovisionan los datos.
- [S4.3 — contrato del proveedor de identidad de desarrollo](../s4.3-proveedor-identidad-desarrollo/spec.md): contrato que consume el proveedor implementado después del aprovisionamiento.
- [S5 → tarea S5.6 y Definition of Done](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l): responsabilidad y criterio de repetición del aprovisionamiento.

## Alcance

- Incluye:
  - Un artefacto SQL especial, separado del catálogo e historial de migraciones, disponible para el proceso de arranque en cualquier entorno.
  - Invocar en cada arranque, primero, el ejecutor de migraciones versionadas y, después, el aprovisionamiento de la cuenta identificada por el email configurado.
  - Crear la cuenta si no existe; completar los campos ausentes de perfil si existe incompleta; dejar intacta una cuenta ya completada; y crear la colección privada «Bandeja de entrada» solo si la cuenta no tiene ninguna colección.
  - Hacer que el aprovisionamiento sea idempotente y que no se modifique el nombre, privacidad ni otros datos de una colección existente.
  - Ejecutar atómicamente la creación/compleción de la cuenta y la reparación de la colección ausente; un fallo no deja un aprovisionamiento parcial.
  - La validación automatizada del orden de inicialización, del estado provisionado y de la repetición en una base SQLite aislada.
- Excluye:
  - Incorporar el aprovisionamiento al historial o catálogo de migraciones versionadas.
  - La resolución del `UserId` en las peticiones privadas y el registro del proveedor concreto; corresponden a S5.7, que debe registrarlo en todos los entornos hasta LoginV0.
  - La aplicación manual de las migraciones y la inspección del esquema aplicado, asignadas a S5.9.
  - La revisión manual de ejecutar el SQL local dos veces, asignada a S5.10; no sustituye los tests automatizados de esta especificación.
  - La preparación explícita del hash de contraseña para LoginV0, que el roadmap trata en la etapa de LoginV0.
- Trabajo vecino y responsable de sus entregables:
  - S5.1–S5.3 proporcionan la conexión, el ejecutor de migraciones y el esquema; S5.7 implementa el proveedor; S5.9 y S5.10 realizan sus comprobaciones humanas respectivas.
  - Esta especificación no atribuye a los tests automatizados la comprobación manual ni la aceptación de esos trabajos vecinos.

## Dependencias y prerrequisitos

- S5.1–S5.3 están implementadas y aceptadas; el archivo se valida sobre una base temporal con el esquema aplicado.
- El contrato de identidad de desarrollo de S4.3 está aceptado; el proveedor concreto de Infrastructure sigue asignado a S5.7 y no es necesario para validar este aprovisionamiento.
- El email y perfil inicial se obtienen de configuración del entorno; sus valores iniciales están definidos en [specifications.md → «Identidad de desarrollo»](../../../context/specifications.md#identidad-de-desarrollo).
- El ejecutor de migraciones de S5.2 y el esquema de S5.3 están disponibles. S5.6 integrará sus invocaciones en el arranque antes de la resolución de identidad.
- S5.7 implementa la resolución de la cuenta ya aprovisionada. La comprobación manual de aplicación de migraciones y revisión del SQL continúa asignada a S5.9; la repetición manual del aprovisionamiento, a S5.10.
- La validación no depende de S5.4, S5.5, S5.8, S6 ni de autenticación LoginV0.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [specifications.md → «Identidad de desarrollo»](../../../context/specifications.md#identidad-de-desarrollo), [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) y [S5 → tarea S5.6](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | Una aplicación iniciada con una base SQLite nueva y configuración del usuario de desarrollo. | Se observa la secuencia de inicialización. | La aplicación aplica primero las migraciones versionadas y después aprovisiona la identidad; la instancia no queda lista para atender peticiones privadas antes de completar ambos pasos. |
| CA02 | [domain-model.md → «User»](../../../context/domain-model.md#user), [«Collection»](../../../context/domain-model.md#collection) y [specifications.md → «Identidad de desarrollo»](../../../context/specifications.md#identidad-de-desarrollo) | Una base sin cuenta o con cuenta incompleta bajo el email configurado, o una cuenta ya completada. | Se ejecuta el aprovisionamiento y se inspeccionan los datos. | Si la cuenta no existe, queda creada y completada con el perfil configurado; si está incompleta, se completan sus campos ausentes con ese perfil; si ya está completa, se conserva sin cambios. En todos los casos se identifica por email y no por un `Id` supuesto. |
| CA03 | [domain-model.md → «User»](../../../context/domain-model.md#user), [«Collection»](../../../context/domain-model.md#collection) y [specifications.md → «Identidad de desarrollo»](../../../context/specifications.md#identidad-de-desarrollo) | Una cuenta demo existente con cero o más colecciones. | Se ejecuta el aprovisionamiento. | Si no tiene ninguna colección, se crea «Bandeja de entrada» privada; si ya tiene una o más, se conservan todas sin renombrarlas ni cambiar su privacidad. |
| CA04 | [specifications.md → «Hash de la contraseña»](../../../context/specifications.md#hash-de-la-contraseña), [architecture.md → «Autenticación»](../../../context/architecture.md#autenticación) y [AGENTS.md → «Invariantes críticas»](../../../AGENTS.md#invariantes-críticas) | El aprovisionamiento y la cuenta que crea o completa. | Se inspeccionan el recurso SQL y el dato persistido de contraseña. | La cuenta completada tiene un hash Argon2id válido en `User.Password`; el aprovisionamiento no guarda ni registra una contraseña en claro. El hash inicial no se recalcula sobre una cuenta ya existente y LoginV0 podrá sustituirlo mediante la preparación explícita de contraseña. |
| CA05 | [S5 → tarea S5.6 y Definition of Done](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l), [architecture.md → «Autenticación»](../../../context/architecture.md#autenticación) y [S5.2 → «Persistencia y dependencias»](../s5.2-ejecutor-scripts-sql-versionados/plan.md#persistencia-y-dependencias) | Una base temporal y varias instancias del proceso de arranque, incluso concurrentes. | Se ejecuta repetidamente la secuencia de inicio y se inspeccionan los datos e historiales. | El resultado es idempotente, no se duplican la cuenta ni su colección base, el artefacto SQL queda fuera del catálogo e historial de migraciones y las migraciones preceden siempre al aprovisionamiento. |
| CA06 | [specifications.md → «Identidad de desarrollo»](../../../context/specifications.md#identidad-de-desarrollo), [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), [domain-model.md → «User»](../../../context/domain-model.md#user) y [«Collection»](../../../context/domain-model.md#collection) | Un aprovisionamiento de cuenta nueva o incompleta que falla al persistir uno de sus cambios. | Se provoca un error durante la operación y se lee la base desde otra conexión. | No queda una cuenta parcialmente completada ni una cuenta nueva sin sus datos consistentes; la transacción revierte todas las escrituras del aprovisionamiento. |

La cuenta nueva o incompleta usa el email y el perfil inicial definidos por la configuración; una cuenta ya completada se deja intacta. La decisión de DLG de aceptar el uso compartido en todos los entornos permite el acceso privado mediante esa identidad hasta LoginV0. La implementación incluye verificación criptográfica del formato PHC Argon2id, comprobada con vectores de prueba positivos y negativos; no se conserva ni verifica en el arranque la contraseña aleatoria usada para el hash seed. La validación de implementación y sus límites constan en [tasks.md → «Evidencia por criterio»](tasks.md#evidencia-por-criterio).

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno que impida redactar esta especificación. El empaquetado del recurso SQL, la generación y provisión del hash Argon2id inicial y la integración concreta en el host son decisiones técnicas para el plan. | [architecture.md → «Autenticación»](../../../context/architecture.md#autenticación), [«Persistencia»](../../../context/architecture.md#persistencia), [specifications.md → «Identidad de desarrollo»](../../../context/specifications.md#identidad-de-desarrollo) y [S5.2 → «Persistencia y dependencias»](../s5.2-ejecutor-scripts-sql-versionados/plan.md#persistencia-y-dependencias) | Ninguno | Revisar las propuestas del plan técnico antes de aprobarlo. |

La secuencia de arranque actualiza la decisión inicial de S5.2: el ejecutor sigue siendo explícito, pero la aplicación lo invoca antes del aprovisionamiento. Esta integración se asigna a S5.6; S5.9 conserva la revisión humana del esquema.

## Artefactos relacionados

[Plan técnico](plan.md), aprobado por DLG el 19-X-2026 («Siguiente»).