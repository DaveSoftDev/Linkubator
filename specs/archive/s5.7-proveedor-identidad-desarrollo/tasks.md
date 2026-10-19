# Tareas: S5.7 — Proveedor de identidad de desarrollo

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 19-X-2026 («Siguiente»).
    - Tareas: aprobadas por DLG el 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.
- Implementación:
    - Estado: completada.
    - Inicio: DLG, 19-X-2026 («Implementa»), instrucción explícita en esta conversación.
    - Finalización: 19-X-2026; tests focalizados 11/11, suite 1032/1032 y build de solución correctos.
    - Evidencia consolidada: [T01–T05](#descomposición), [evidencia por criterio](#evidencia-por-criterio), [DevelopmentIdentityProvider.cs](../../../src/Linkubator.Infrastructure/DevelopmentIdentity/DevelopmentIdentityProvider.cs), [DevelopmentIdentityMiddleware.cs](../../../src/Linkubator.Web/DevelopmentIdentity/DevelopmentIdentityMiddleware.cs), [DevelopmentIdentityProviderTests.cs](../../../tests/Linkubator.Tests/Infrastructure/DevelopmentIdentityProviderTests.cs), [DevelopmentIdentityMiddlewareTests.cs](../../../tests/Linkubator.Tests/Integration/DevelopmentIdentityMiddlewareTests.cs) y [WebStartupTests.cs](../../../tests/Linkubator.Tests/Integration/WebStartupTests.cs).
- La preparación de estas tareas no aprueba su ejecución ni inventa evidencias de implementación.

## Descomposición

La integración preserva los contratos existentes: middleware asíncrono limitado a `/app` obtiene el identificador del proveedor y lo carga en un `ICurrentUser` scoped antes de ejecutar el endpoint. No hay bloqueo síncrono; la composición Web es responsable de esta adaptación.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Resolver el límite de integración entre `IDevelopmentIdentityProvider.GetUserId(CancellationToken)` y el `ICurrentUser.UserId` síncrono; documentar una solución compatible con las fuentes propietarias y revisar el plan y las tareas afectados para aprobación explícita antes de implementar si cambia su alcance. | CA02, CA04, CA05; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [Flujos internos](plan.md#flujos-internos) y [Decisiones técnicas y riesgos](plan.md#decisiones-técnicas-y-riesgos) | Ninguna | Confirmar que la adaptación no bloquea sincrónicamente, no cambia contratos de Application y no acepta identidad de la petición; resultado esperado: middleware asíncrono por petición privada que inicializa el contexto scoped antes del endpoint. | completada | 19-X-2026: la composición Web añadirá middleware asíncrono en `/app` que resuelve `IDevelopmentIdentityProvider` y establece el `ICurrentUser` scoped; mantiene la firma síncrona consumida por los casos de uso y no requiere modificar el contrato aprobado. Esta precisión de implementación queda dentro de la composición existente del plan; no se cambia alcance ni contrato. |
| T02 | Implementar el proveedor de Infrastructure y su lectura de la cuenta configurada. | CA01–CA02; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [Persistencia y dependencias](plan.md#persistencia-y-dependencias), V01–V02 | T01 | Ejecutar pruebas focalizadas con cuenta completada, inexistente e incompleta; resultado esperado: solo la cuenta completada del email configurado produce `UserId`, sin escrituras. | completada | [DevelopmentIdentityProvider.cs](../../../src/Linkubator.Infrastructure/DevelopmentIdentity/DevelopmentIdentityProvider.cs); tests del proveedor 3/3, incluido snapshot del perfil antes/después para confirmar que la lectura no escribe. |
| T03 | Registrar el proveedor en todos los entornos e integrarlo con la identidad que consumen las peticiones privadas, conforme a la resolución aprobada de T01. | CA03–CA04; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [Flujos internos](plan.md#flujos-internos), V03 | T02 | Resolver los servicios en hosts de distintos entornos y verificar la identidad en el límite privado existente; resultado esperado: registro no limitado a Development y ninguna sustitución por datos de solicitud. | completada | Registro incondicional en `AddInfrastructure`; middleware asíncrono de `/app` rellena el `ICurrentUser` scoped antes del endpoint. Tests de registro y middleware 3/3, incluido `?userId=99` frente al ID del proveedor. No hay páginas privadas en este trabajo, así que el check cubre el límite del middleware y no un caso de uso HTTP completo. |
| T04 | Validar el orden de inicialización y el fallo cerrado cuando el proveedor no puede resolver una cuenta elegible. | CA05; [plan → Garantías técnicas](plan.md#garantías-técnicas), [Estrategia de validación](plan.md#estrategia-de-validación), V04 | T02, T03 | Ejecutar pruebas de arranque con SQLite temporal; resultado esperado: migraciones y aprovisionamiento preceden a la resolución, y un fallo impide servir solicitudes. | completada | `WebStartupTests` 4/4; el nuevo escenario comprueba que, al fallar el proveedor, la base ya tiene migraciones y cuenta provisionada, pero `CreateClient()` no inicia el host. |
| T05 | Ejecutar validación integrada, sincronizar el roadmap con el estado real y consolidar evidencia por criterio. | CA01–CA05; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V01–V05; [roadmap → «Persistencia»](../../../context/roadmap.md#persistencia) | T02, T03, T04 | Ejecutar las pruebas focalizadas y el build definidos en V05, revisar cobertura CA→validación→evidencia y actualizar la trazabilidad; resultado esperado: evidencia reproducible y sin atribuir los controles de S5.8–S5.10 a S5.7. | completada | 19-X-2026: tests focalizados 11/11; suite completa 1032/1032; `dotnet build Linkubator.sln --no-restore --verbosity minimal` correcto; diagnóstico de archivos modificados sin errores. Roadmap sincronizado. |
| T06 | Revisar la evidencia completa y solicitar/registrar la aceptación humana del resultado. | CA01–CA05; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T05 | Revisar resultado y límites de todas las validaciones; resultado esperado: aceptación humana explícita registrada o bloqueo concreto, sin inferirla de los tests. | completada | DLG aceptó el resultado el 19-X-2026 con la referencia «Completado», instrucción explícita en esta conversación. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T03, T05 | V01, V05 | Proveedor registrado en Infrastructure; matriz de referencias y build correctos. | 19-X-2026, .NET 10 | [DevelopmentIdentityProvider.cs](../../../src/Linkubator.Infrastructure/DevelopmentIdentity/DevelopmentIdentityProvider.cs), `SqliteInfrastructureRegistrationTests`, `ProjectReferenceMatrixTests`, build de solución. |
| CA02 | T01, T02, T05 | V02, V05 | Resuelve únicamente la cuenta completa configurada; cuenta ausente o incompleta falla; lectura no muta el perfil. | 19-X-2026, .NET 10 | [DevelopmentIdentityProviderTests.cs](../../../tests/Linkubator.Tests/Infrastructure/DevelopmentIdentityProviderTests.cs), 3/3. |
| CA03 | T03, T05 | V03, V05 | Servicio registrado incondicionalmente por Infrastructure. | 19-X-2026, .NET 10 | `SqliteInfrastructureRegistrationTests` dentro de la validación focalizada; suite 1032/1032. |
| CA04 | T01, T03, T05 | V03, V05 | Middleware de `/app` establece el ID del proveedor en `ICurrentUser`; un `userId` de query no reemplaza la identidad. | 19-X-2026, .NET 10 | [DevelopmentIdentityMiddlewareTests.cs](../../../tests/Linkubator.Tests/Integration/DevelopmentIdentityMiddlewareTests.cs), 2/2. El test verifica el límite de middleware, no un endpoint privado funcional. |
| CA05 | T01, T04, T05 | V04, V05 | El host migra y provisiona antes de comprobar identidad; si la resolución falla no atiende solicitudes. | 19-X-2026, .NET 10 | [WebStartupTests.cs](../../../tests/Linkubator.Tests/Integration/WebStartupTests.cs), 4/4; suite 1032/1032 y build correcto. |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: completado y aceptado por DLG para el alcance S5.7.
- Criterios sin evidencia satisfactoria: ninguno conocido para CA01–CA05; la integración de operaciones privadas completas no se prueba porque las rutas privadas/casos de uso HTTP quedan fuera de este alcance.
- Bloqueos y riesgos restantes: sin bloqueo técnico conocido en S5.7. La prueba de CA04 valida el middleware y no una página privada completa; S5.8 y las etapas de interfaz conservan sus validaciones propias.
- Aceptación humana: DLG, 19-X-2026 («Completado»), instrucción explícita en esta conversación.
- Archivado: completado el 19-X-2026; traslado y enlaces entrantes/salientes comprobados.
- Ubicación archivada: `specs/archive/s5.7-proveedor-identidad-desarrollo/`.
- Fecha efectiva de archivado y comprobación de enlaces: 19-X-2026; referencias recalculadas y verificadas.
- Aprobación de tareas: DLG, 19-X-2026 («Siguiente»).
- Aceptación humana del resultado: DLG, 19-X-2026 («Completado»).
- Siguiente paso autorizado, sin cerrar otras etapas: S5.7 queda cerrada para su alcance. El archivado es independiente y no se ha solicitado; la etapa Persistencia continúa según S5.8–S5.10.
