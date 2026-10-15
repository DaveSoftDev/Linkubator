# Tareas: S4.3 — Contrato del proveedor de identidad de desarrollo

## Referencias y autorización

- Estado: aprobada.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 15-X-2026 («Aprobada»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 15-X-2026 («Adelante»).
    - Tareas: aprobadas por DLG el 15-X-2026 («Vamos»).
- Implementación:
    - Estado: completada y aceptada por DLG para el alcance del contrato S4.3.
    - Inicio: 15-X-2026 («Vamos»).
    - Finalización y aceptación: DLG, 15-X-2026 («Terminado»).
    - Validación: `dotnet test Linkubator.sln --no-restore --verbosity minimal`; suite 737/737 y compilación correcta. La suite filtrada `ContractsTests` también pasó 4/4.
    - Evidencia consolidada: [spec.md](spec.md), [plan.md](plan.md), [roadmap.md](../../context/roadmap.md#now), [IDevelopmentIdentityProvider.cs](../../src/Linkubator.Application/Contracts/IDevelopmentIdentityProvider.cs), [ContractsTests.cs](../../tests/Linkubator.Tests/Application/Contracts/ContractsTests.cs) y validaciones registradas en T02–T04.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T02 implementó únicamente la interfaz contratada en Application. T03 verificó contratos y límites arquitectónicos; T04 integró build, suite y sincronización documental. La implementación del proveedor concreto, su registro por entorno, la consulta de persistencia y el fallo de arranque permanecen en S5.7.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas; registrar la referencia explícita de DLG. | CA01–CA05; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Confirmar cobertura de todos los criterios, dependencias y separación respecto a S5.7; resultado esperado: aprobación de tareas separada de las aprobaciones de spec y plan. | completada | Aprobación registrada por DLG el 15-X-2026 («Vamos»). |
| T02 | Añadir a Application la interfaz pública `IDevelopmentIdentityProvider` con `GetUserId(CancellationToken)`, sin implementación concreta ni dependencias nuevas. | CA01–CA04; [plan → Componentes y contratos](plan.md#componentes-y-contratos) | T01 | Compilar Application y revisar la firma; resultado esperado: contrato asíncrono que devuelve el identificador resuelto y no recibe identidad ni datos de usuario desde la solicitud. | completada | [IDevelopmentIdentityProvider.cs](../../src/Linkubator.Application/Contracts/IDevelopmentIdentityProvider.cs); prueba de contratos verifica tipo de retorno y parámetro de cancelación. La firma as-built `GetUserId` difiere del nombre propuesto originalmente `GetUserIdAsync`. |
| T03 | Revisar la interoperabilidad del contrato con `ICurrentUser`/`CurrentUser` de S4.2 y comprobar referencias permitidas. | CA01–CA03; [plan → Garantías técnicas](plan.md#garantías-técnicas), V01–V02 | T02 | Ejecutar `ProjectReferenceMatrixTests` y revisar los tipos y namespaces; resultado esperado: Application no depende de Infrastructure/Web y el proveedor no duplica `ICurrentUser`. | completada | `ProjectReferenceMatrixTests` y la suite filtrada de contratos confirmaron la separación de capas; no se implementa el proveedor concreto de S5.7. |
| T04 | Ejecutar validación integrada del contrato y sincronizar la trazabilidad del roadmap; revisar que la evidencia no atribuya a S4.3 la implementación de S5.7. | CA01–CA05; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V03–V04; [roadmap.md → «Now»](../../context/roadmap.md#now) | T03 | Ejecutar `dotnet build Linkubator.sln --no-restore --verbosity minimal` y `dotnet test Linkubator.sln --no-restore --verbosity minimal`; resultado esperado: build y suite correctos y roadmap enlazado al estado real. | completada | 15-X-2026: build correcto y suite 737/737; prueba filtrada `ContractsTests` 4/4. Los avisos HTTPS de pruebas web no causaron fallos. |
| T05 | Revisar evidencia por criterio y registrar aceptación humana explícita del resultado. | CA01–CA05; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T04 | Comprobar evidencia satisfactoria para todos los criterios y registrar decisión expresa de DLG; resultado esperado: aceptación o bloqueo concreto, sin inferirla de tests ni de la aprobación de tareas. | completada | Implementación aceptada por DLG el 15-X-2026 con la referencia «Terminado». |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02–T04 | V01, V04 | Contrato en Application; compilación y suite 737/737 correctas. | 15-X-2026, .NET 10.0.12 | [IDevelopmentIdentityProvider.cs](../../src/Linkubator.Application/Contracts/IDevelopmentIdentityProvider.cs); `ProjectReferenceMatrixTests`; build y suite completos. |
| CA02 | T02–T04 | V02, V04 | La firma resuelve un identificador sin recibir identidad del cliente y es distinta de `ICurrentUser`; pruebas correctas. | 15-X-2026, .NET 10.0.12 | [ContractsTests.cs](../../tests/Linkubator.Tests/Application/Contracts/ContractsTests.cs), `ContractsTests` 4/4; suite 737/737. |
| CA03 | T02–T04 | V02, V04 | La firma no acepta `UserId`, alias, slug ni email desde la petición; la resolución por email se implementará en S5.7. | 15-X-2026, .NET 10.0.12 | Revisión de [IDevelopmentIdentityProvider.cs](../../src/Linkubator.Application/Contracts/IDevelopmentIdentityProvider.cs); firma y referencias comprobadas. |
| CA04 | T04 | V03 | El contrato permite obtener el identificador; la comprobación de cuenta completada y el fallo de arranque quedan asignados a S5.7. | 15-X-2026 | [plan.md → Componentes y contratos](plan.md#componentes-y-contratos), [roadmap.md → «Now»](../../context/roadmap.md#now). |
| CA05 | T04 | V03 | No se define proveedor para otros entornos; el registro por entorno queda asignado a S5.7. | 15-X-2026 | [plan.md → Persistencia y dependencias](plan.md#persistencia-y-dependencias), [roadmap.md → «Now»](../../context/roadmap.md#now). |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: completado y aceptado por DLG para el alcance del contrato S4.3.
- Criterios con evidencia satisfactoria: CA01–CA05, con la validación de contratos y suite completa ejecutadas.
- Bloqueos y riesgos restantes: ninguno conocido para el contrato; la implementación y pruebas del proveedor concreto, así como su registro por entorno, quedan asignadas a S5.7.
- Aprobación de tareas: aprobada por DLG el 15-X-2026 («Vamos»).
- Aceptación humana: DLG, 15-X-2026 («Terminado»).
- Archivado: no solicitado; el trabajo está completado y aceptado, pero permanece en `specs/`.
- Ubicación archivada: no aplica mientras no se solicite el archivado.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso autorizado: continuar con la implementación del proveedor concreto en S5.7; esta aceptación no cierra esa etapa ni el flujo de autenticación real. El archivado de S4.3 requiere una petición explícita.
