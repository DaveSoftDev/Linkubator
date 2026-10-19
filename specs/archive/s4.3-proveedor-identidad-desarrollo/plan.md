# Plan técnico: S4.3 — Contrato del proveedor de identidad de desarrollo

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 15-X-2026 («Aprobada»).
	- Plan: aprobado por DLG el 15-X-2026 («Adelante»).

## Diseño de implementación

### Componentes y contratos

- **Contrato del proveedor.** La interfaz `IDevelopmentIdentityProvider` en Application expone la operación asíncrona `GetUserId(CancellationToken)` que devuelve el identificador numérico resuelto. La firma no recibe identidad, email, alias ni slug desde la petición. Application define el contrato; la implementación de S5.7 será responsable de obtener el email de configuración local y consultar la cuenta provisionada.
- **Integración con usuario identificado.** El consumidor adapta el identificador resuelto al `CurrentUser` de S4.2 para proporcionar el contexto `ICurrentUser` usado por los casos privados. El contrato de proveedor no duplica esa abstracción ni contiene reglas de autorización.
- **Fallos de resolución.** La operación no debe convertir una identidad ausente o inválida en un usuario ficticio ni permitir una identidad alternativa enviada por el cliente. La verificación de que la cuenta exista y esté completada al arrancar, y el fallo de arranque correspondiente, se implementan y prueban en S5.7.
- **Ubicación.** El tipo público se ubicará bajo Application, en su propio archivo y en una carpeta de contratos apropiada, respetando [AGENTS.md → «Convención de estructura del código»](../../../AGENTS.md#convención-de-estructura-del-código).
- Los nombres y la firma de interfaz son decisiones técnicas aprobadas mediante el plan y las tareas; DLG aceptó el contrato implementado con la referencia «Terminado» el 15-X-2026.

### Flujos internos

1. La composición de la aplicación en desarrollo resolverá el proveedor concreto en el punto autorizado de Infrastructure/Web, sin añadir referencias desde Application a esas capas.
2. El proveedor concreto leerá la configuración local y localizará la cuenta por el email estable según [architecture.md → «Autenticación»](../../../context/architecture.md#autenticación) y [decisions.md → «Identidad de desarrollo»](../../../context/decisions.md#identidad-de-desarrollo).
3. Durante el arranque, la implementación de S5.7 comprobará que la cuenta localizada está completada y detendrá el inicio si no lo está.
4. En cada petición privada de `/app`, el consumidor usará el identificador devuelto para construir el contexto `CurrentUser`; no confiará en valores de identidad de la solicitud.
5. Los errores de configuración o resolución se manejan en las fronteras de composición y persistencia. Este contrato no define mensajes de usuario ni respuestas HTTP.

### Persistencia y dependencias

- S4.3 añade solo el contrato a Application y no incorpora paquetes ni dependencias de persistencia.
- El proveedor concreto de S5.7 usará configuración local y la persistencia para localizar por email la cuenta completada. La consulta al arrancar, el ciclo de vida por petición y el registro exclusivo en desarrollo pertenecen a S5.7.
- No se modifica el SQL local de aprovisionamiento, el esquema ni los repositorios en este trabajo.
- Se conserva la dirección de dependencias descrita en [architecture.md → «Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos).

### Garantías técnicas

- El contrato permanece en Application y no referencia Infrastructure, Web, SQLite, Dapper ni HTTP (CA01).
- La identidad procede del proveedor y no de datos controlados por el cliente; el adaptador a `ICurrentUser` mantiene la separación definida en S4.2 (CA02–CA03).
- La cuenta debe resolverse a partir del email estable configurado y estar completada; la ausencia de una cuenta elegible no puede producir acceso privado. La verificación ejecutable corresponde a S5.7 (CA03–CA04).
- El contrato no define ni habilita un proveedor para producción; el registro y la política de entorno se comprueban en S5.7 (CA05).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Añadir `IDevelopmentIdentityProvider` en Application con `GetUserId(CancellationToken)` | Confirmada por implementación revisada por DLG | [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación), CA01–CA04; aprobación en [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre) | Distingue la provisión de identidad del contexto `ICurrentUser`; el nombre as-built difiere del método `GetUserIdAsync` propuesto originalmente. |
| Retornar `UserId` y dejar la construcción de `CurrentUser` al consumidor | Confirmada en el diseño aceptado | S4.2 define `ICurrentUser` y `CurrentUser`; [spec.md](spec.md#criterios-de-aceptación), CA02 | Evita duplicar el contrato de usuario identificado. |
| Recibir configuración por parámetro en el contrato | Descartada | [architecture.md → «Autenticación»](../../../context/architecture.md#autenticación) | El proveedor concreto obtiene el email desde configuración local; no se acepta como autoridad un dato de petición. |
| Implementar el chequeo de cuenta completada y el registro solo en desarrollo | Confirmada como responsabilidad de S5.7 | [mvp0-plan.md → «S5: Persistencia I: SQLite y transacciones (L)»](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l) | S4.3 define solamente el contrato. La implementación y pruebas de arranque quedan fuera de este trabajo. |
| Invocar la resolución asíncrona en cada petición | Pendiente de S5.7 | [architecture.md → «Autenticación»](../../../context/architecture.md#autenticación) | La estrategia de ciclo de vida y caché se resuelve en el diseño de S5.7 sin debilitar la comprobación al arrancar. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada y técnica: dependencias y forma pública | Contrato implementado; solución restaurada | Ejecutar `ProjectReferenceMatrixTests`; revisar `Linkubator.Application.csproj` y la interfaz nueva | Application conserva referencias permitidas; la firma no filtra tipos de infraestructura o presentación | Test, resultado y referencias revisadas. |
| V02 | CA02–CA03 | Revisión técnica: contrato y autoridad de identidad | Contrato y abstracción S4.2 disponibles | Revisar firma y tipos: no recibe valores de identidad de la petición, email mutable ni claves públicas como autoridad; confirmar separación respecto a `ICurrentUser` | El consumidor solo puede obtener la identidad mediante el proveedor y convertirla al contexto existente | Firma revisada y matriz proveedor → `CurrentUser` → `ICurrentUser`. |
| V03 | CA04–CA05 | Revisión de alcance y trazabilidad | Diseño de S5.7 disponible en el roadmap | Confirmar que los criterios de cuenta completada, fallo de arranque y registro por entorno están asignados a S5.7 y no se declaran demostrados por el contrato | No se atribuyen al contrato comportamientos de infraestructura todavía no implementados | Secciones y responsabilidades revisadas. |
| V04 | CA01–CA05 | Integrada: compilación y suite | SDK y dependencias restauradas; tareas implementadas | `dotnet build Linkubator.sln --no-restore --verbosity minimal` y `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Compilación y suite pasan; registrar resultados reales sin inferir aprobación ni aceptación | Comandos, versión SDK, códigos de salida y resumen. |

V01–V04 validan el contrato, su límite de capa y el reparto de alcance; no demuestran la resolución de la cuenta, el rechazo de una cuenta incompleta, el fallo de arranque ni el registro por entorno. Esos comportamientos requieren la implementación y pruebas de S5.7.

## Orden de ejecución y cierre

- `tasks.md` fue creado tras la aprobación explícita de este plan; DLG aprobó las tareas el 15-X-2026 («Vamos»). El contrato se implementó y validó según el desglose y su evidencia está registrada en ese archivo.
- DLG aceptó el alcance de S4.3 el 15-X-2026 con la referencia «Terminado». Esta aceptación no acredita la implementación de S5.7 ni abre la puerta S4.5.
- Ante una contradicción con las fuentes o un cambio de reglas de identidad, detener la tarea afectada y tramitar el cambio en el documento propietario antes de continuar.
