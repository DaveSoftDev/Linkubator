# Especificación: S5.7 — Proveedor de identidad de desarrollo

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Objetivo

Implementar en Infrastructure el proveedor concreto de identidad de desarrollo, registrarlo en todos los entornos mientras no exista autenticación real y comprobar que resuelve la cuenta completada aprovisionada antes de atender peticiones privadas.

## Fuentes

- [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l): entregable S5.7 y Definition of Done de persistencia.
- [roadmap.md → «Persistencia»](../../../context/roadmap.md#persistencia): orden y criterio de finalización de la etapa.
- [specifications.md → «Identidad de desarrollo»](../../../context/specifications.md#identidad-de-desarrollo): preparación y uso de la identidad.
- [architecture.md → «Autenticación»](../../../context/architecture.md#autenticación), [«Infrastructure»](../../../context/architecture.md#infrastructure) y [«Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos): resolución de la identidad, responsabilidad de capas y registro del proveedor.
- [domain-model.md → «User»](../../../context/domain-model.md#user) y [«Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos): cuenta completada y origen confiable de la identidad.
- [decisions.md → «Identidad de desarrollo»](../../../context/decisions.md#identidad-de-desarrollo): propósito y alcance provisional de esta identidad.
- [S4.3 — Contrato del proveedor de identidad de desarrollo](../s4.3-proveedor-identidad-desarrollo/spec.md): contrato de Application y límites de esta implementación.
- [S5.6 — Aprovisionamiento SQL de identidad de desarrollo](../s5.6-sql-local-identidad-desarrollo/spec.md): aprovisionamiento previo y cuenta que resolverá el proveedor.

## Alcance

- Incluye:
  - Implementar el contrato `IDevelopmentIdentityProvider` de Application en Infrastructure.
  - Resolver la cuenta de desarrollo usando el email de configuración y devolver su `UserId` mediante el contrato existente.
  - Proporcionar ese identificador en cada petición privada de `/app`, sin sustituir la resolución por datos de la petición.
  - Registrar el proveedor concreto para todos los entornos hasta que LoginV0 incorpore autenticación real.
  - Comprobar durante el arranque, después de las migraciones y del aprovisionamiento de S5.6, que la cuenta puede resolverse y está completada; si no, impedir que el host atienda peticiones privadas.
  - Validar con pruebas automatizadas el proveedor, su registro y el fallo de inicialización cuando no haya una cuenta elegible.
- Excluye:
  - Cambiar el contrato de Application, las reglas de identidad, el esquema, el aprovisionamiento SQL o el orden migraciones-aprovisionamiento.
  - Aceptar `UserId`, email, alias o slug del cliente como autoridad de identidad.
  - Implementar autenticación real, cookies, LoginV0 o un proveedor alternativo para otros entornos.
  - Cambiar casos de uso o formularios; este trabajo implementa y registra el proveedor definido por S4.3.
  - Las pruebas integradas de restricciones, carreras y orden de listados asignadas a S5.8, y las revisiones humanas de S5.9 y S5.10.
- Trabajo vecino y responsable de sus entregables:
  - S4.3 define el contrato de Application; S5.6 provisiona la cuenta y ordena su ejecución tras las migraciones; S5.7 implementa y registra el proveedor concreto.
  - S5.8 valida el comportamiento integrado de persistencia; sus garantías no se atribuyen al proveedor aisladamente.
  - LoginV0 sustituirá el uso de esta identidad por autenticación real; S5.7 no anticipa esa implementación.

## Dependencias y prerrequisitos

- S4.3 está completada y aceptada; `IDevelopmentIdentityProvider` es el contrato propietario.
- S5.1–S5.3 y S5.6 están implementadas y aceptadas; la conexión, el esquema y el aprovisionamiento necesarios están disponibles.
- S5.5 está completada y aceptada; el acceso a datos existente puede consultar la cuenta provisionada.
- La configuración de identidad definida por las fuentes propietarias está disponible en todos los entornos.
- La validación propia usa una base SQLite temporal con migraciones y aprovisionamiento controlados; no depende de S5.8 ni de LoginV0.
- La existencia de código relacionado en el repositorio no constituye por sí sola evidencia de cumplimiento ni aceptación de esta especificación.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [S4.3 → CA01–CA04](../s4.3-proveedor-identidad-desarrollo/spec.md#criterios-de-aceptación), [architecture.md → «Infrastructure»](../../../context/architecture.md#infrastructure) y [«Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos) | El contrato `IDevelopmentIdentityProvider` y la implementación concreta de Infrastructure. | Se revisan tipos, referencias y resolución del servicio. | Infrastructure implementa el contrato sin trasladar dependencias de persistencia a Application ni modificar el contrato de S4.3. |
| CA02 | [specifications.md → «Identidad de desarrollo»](../../../context/specifications.md#identidad-de-desarrollo), [architecture.md → «Autenticación»](../../../context/architecture.md#autenticación) y [domain-model.md → «User»](../../../context/domain-model.md#user) | Una base aprovisionada con la cuenta completada del email configurado y, además, casos de cuenta inexistente o sin completar. | Se valida la resolución inicial y la llamada al proveedor desde una petición privada. | El proveedor devuelve en cada petición privada el `UserId` de la cuenta completada identificada por el email configurado; si no existe una cuenta elegible durante la inicialización, esta falla y no se habilita el acceso privado. |
| CA03 | [mvp0-plan.md → tarea S5.7 y Definition of Done](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l), [S4.3 → CA05](../s4.3-proveedor-identidad-desarrollo/spec.md#criterios-de-aceptación) y [architecture.md → «Autenticación»](../../../context/architecture.md#autenticación) | El contenedor de servicios de Infrastructure bajo distintos entornos de host. | Se inspecciona el registro y se resuelve `IDevelopmentIdentityProvider`. | El proveedor está registrado en todos los entornos mientras no se incorpore LoginV0; el registro no queda condicionado solo a Development. |
| CA04 | [architecture.md → «Autenticación»](../../../context/architecture.md#autenticación), [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos) y [AGENTS.md → «Invariantes críticas»](../../../AGENTS.md#invariantes-críticas) | Una petición privada que contiene identificadores o valores de identidad arbitrarios y la cuenta de desarrollo ya resuelta. | El consumidor invoca el contrato para obtener el identificador que utilizará en la operación privada. | El identificador procede exclusivamente de la cuenta configurada y resuelta; ningún valor aportado por la petición puede elegir otra identidad. |
| CA05 | [architecture.md → «Autenticación»](../../../context/architecture.md#autenticación), [«Persistencia»](../../../context/architecture.md#persistencia) y [S5.6 → CA01 y CA05](../s5.6-sql-local-identidad-desarrollo/spec.md#criterios-de-aceptación) | La inicialización completa del host y el proveedor registrado. | Se observa el orden de arranque y se revisan los efectos de una resolución correcta y fallida. | La resolución se realiza después de migrar y aprovisionar la cuenta; si falla, el host no queda disponible para peticiones privadas y el proveedor no modifica los datos de la cuenta. |

Los casos negativos de cuenta inexistente o incompleta se comprueban mediante CA02. Las garantías de carreras de negocio, restricciones y listados integrados permanecen en S5.8.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ningún bloqueo funcional conocido para redactar esta especificación. La estrategia concreta de acceso a datos, el ciclo de vida del proveedor y la integración en el composition root son decisiones de diseño para el plan. | [architecture.md → «Autenticación»](../../../context/architecture.md#autenticación), [«Persistencia»](../../../context/architecture.md#persistencia) y [S4.3 → «Componentes y contratos»](../s4.3-proveedor-identidad-desarrollo/plan.md#componentes-y-contratos) | Ninguno | Revisar las propuestas técnicas al aprobar el plan. |

## Artefactos relacionados

[Plan técnico](plan.md), propuesta pendiente de aprobación.
