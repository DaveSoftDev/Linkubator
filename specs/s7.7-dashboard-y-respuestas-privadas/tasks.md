# Tareas: S7.7 — Dashboard y respuestas privadas

## Referencias y autorización

- Estado: completada y aceptada.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 23-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 23-X-2026 («Siguiente»).
  - Tareas: DLG, aprobadas el 23-X-2026 («Vamos»).
  - Aceptación humana final: DLG, 24-X-2026 («Prueba superada»).
- Implementación:
  - Estado: completada.
  - Inicio: 23-X-2026; instrucción explícita de DLG: «Inicia la implementación».
  - Finalización: 24-X-2026, con aceptación humana explícita: «Prueba superada».
  - Evidencia consolidada: disponible en esta misma tarea y en las validaciones focalizadas del proyecto Web/Tests.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

Las tareas de respuesta HTTP se mantienen acotadas al host y las rutas existentes; para la respuesta por recurso no accesible se requiere un endpoint exclusivo de tests, sin añadir páginas productivas de etapas posteriores. S7.6, S7.8, S7.9 y las revisiones manuales posteriores no son prerrequisitos de cierre. La inspección mostró que S7.6 ya está en curso en el mismo checkout; S7.7 no editará ni validará sus archivos sin coordinación, y cualquier build/test integrado que falle por esos cambios compartidos se registra como bloqueo externo, no se corrige dentro de S7.7.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Inspeccionar el dashboard, el PageModel, los modelos de presentación, la navegación lateral y los componentes de S7.5; ajustar la vista para usar los resultados del caso de uso y presentar solo accesos disponibles dentro de S7.7. | CA02; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos) | Ninguna | Tests focalizados de estado vacío y enlace reciente de `WebStartupTests`; resultado esperado: dashboard desde el caso de uso real, búsqueda GET, estado vacío cuando no hay enlaces, enlace/campo de dominio/descripcion cuando existen, y ausencia de acciones ficticias. | completada | El 23-X-2026 los tests Release `AppDashboard_RendersSearchAndDatabaseBackedEmptyState` y `AppDashboard_RendersRecentLinksFromDashboardUseCase` pasaron 1/1 cada uno; el último sembró usuario, colección y enlace en SQLite y comprobó el contenido servido. El dashboard usa el caso de uso, estado vacío y formulario S7.5; se eliminaron quick-add y acciones mock no funcionales y se restringió la navegación del dashboard a opciones existentes. Build Web Release y suite focalizada indicada en T05 también pasaron; queda la limitación de rutas de destino pertenecientes a sprints posteriores. |
| T02 | Completar o verificar el acceso a `/app`, la resolución de identidad de servidor y la ausencia de dependencia en `UserId` aportado por el cliente; añadir cobertura HTTP focalizada si falta. | CA01; [plan.md → «Flujos internos»](plan.md#flujos-internos) | T01 | Solicitar `/app` con y sin `userId` manipulado; resultado esperado: se muestra el dashboard y el identificador consumido procede del proveedor de identidad, no de la petición. | completada | La prueba HTTP solicita `/app?userId=99` y confirma HTML de dashboard, `UserId` capturado del proveedor igual a 42 y una sola resolución. Test `WebStartupTests.AppDashboard_UsesResolvedIdentityEvenWhenRequestContainsUserId` pasó 1/1 el 23-X-2026, Release. |
| T03 | Verificar y, si hace falta, implementar el `404` genérico por método no permitido en rutas privadas disponibles; añadir test de integración HTTP sensible al resultado. | CA03, CA05; [plan.md → «Flujos internos»](plan.md#flujos-internos) | Ninguna | Usar una ruta privada existente y un método no permitido; resultado esperado: respuesta indistinguible del `404` genérico definido, sin detalle del recurso. El test debe detectar si se devuelve otro estado o cuerpo diferenciado. | completada | El 24-X-2026 se confirmó el comportamiento en el host Web con `PUT /app`, que devuelve `404 NotFound` y renderiza la página privada genérica de `/app/404`; la validación se realiza con `WebStartupTests.AppDashboard_WhenRequestedWithUnsupportedMethod_ReturnsNotFound`. Los cambios en el middleware de rutas privadas quedaron encapsulados en el flujo de `Program.cs` y no alteran el resto de rutas. |
| T04 | Añadir un presentador Web reutilizable para mapear `ResourceNotAccessible` a aviso genérico y probar su salida en un punto de observación exclusivo de tests. | CA04, CA05; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos) y [«Estrategia de validación»](plan.md#estrategia-de-validación) | Ninguna | Ejecutar el test HTTP focalizado del endpoint de test; resultado esperado: aviso genérico accesible, destino de listado y ausencia de identificador/código/mensaje internos; sin añadir una ruta productiva. | completada | Test Release `WebStartupTests.PrivateResourceUnavailable_RendersGenericAccessibleNoticeWithoutInternalDetails` pasó 1/1 el 23-X-2026; el test renderiza el partial Razor real `_PrivateResourceUnavailable.cshtml`, que reutiliza `_PageAlert.cshtml`. La comprobación negativa confirmó ausencia del código, mensaje e identificador internos y presencia del aviso accesible y enlace al listado. No se añadió ruta productiva. |
| T05 | Ejecutar validación integrada del proyecto Web y Tests, revisar diff y confirmar cobertura de todos los criterios. | CA01–CA05; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | T01, T02, T03, T04 | `dotnet build src/Linkubator.Web/Linkubator.Web.csproj -c Release --no-restore`; `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~WebStartupTests"`; `git diff --check`. Resultado esperado: build y tests satisfactorios, diff limpio y evidencia trazable por criterio. | completada | Build Web Release: 0 warnings/0 errors; tests `WebStartupTests`: 11/11; la prueba focalizada `AppDashboard_WhenRequestedWithUnsupportedMethod_ReturnsNotFound` pasó 1/1. `git diff --check` quedó limpio. La validación cubre CA01, CA02, CA03, CA04 y la parte de CA05 disponible en el host y el partial real. |
| T06 | Revisar la evidencia de cierre, registrar aceptación humana o bloquear con correcciones solicitadas. | CA01–CA05; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T05 | Resultado esperado: decisión explícita, responsable, fecha y referencia registrados; no completar si falta aceptación. | aceptada | Aceptación humana registrada por DLG el 24-X-2026 con la referencia «Prueba superada». |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T05 | V01, V04 en [plan.md](plan.md#estrategia-de-validación) | Satisfactoria: `/app` entrega dashboard y no acepta identidad desde query | 23-X-2026; host HTTP de tests con provider controlado, Release | `WebStartupTests.AppDashboard_UsesResolvedIdentityEvenWhenRequestContainsUserId`, 1/1 |
| CA02 | T01, T05 | V01, V04 en [plan.md](plan.md#estrategia-de-validación) | Satisfactoria para dashboard vacío y con enlace real, búsqueda GET y exclusión de acciones de etiquetas | 23-X-2026; host Web con SQLite temporal, Release | `WebStartupTests.AppDashboard_RendersSearchAndDatabaseBackedEmptyState`, 1/1; `WebStartupTests.AppDashboard_RendersRecentLinksFromDashboardUseCase`, 1/1 |
| CA03 | T03, T05 | V02, V04 en [plan.md](plan.md#estrategia-de-validación) | Satisfactoria: la ruta privada responde `404 NotFound` para métodos no permitidos y renderiza la página genérica de la app | 24-X-2026; host Web de tests, Release | `WebStartupTests.AppDashboard_WhenRequestedWithUnsupportedMethod_ReturnsNotFound`, 1/1 |
| CA04 | T04, T05 | V03, V04 en [plan.md](plan.md#estrategia-de-validación) | Satisfactoria para el presentador genérico y el renderizado del partial Razor real con su región accesible | 23-X-2026; host Web de tests, Release | `WebStartupTests.PrivateResourceUnavailable_RendersGenericAccessibleNoticeWithoutInternalDetails`, 1/1 |
| CA05 | T03, T04, T05 | V02–V04 en [plan.md](plan.md#estrategia-de-validación) | Satisfactoria: la respuesta de CA03 y la vista de CA04 dan el mismo principio de privacidad y se mantienen los componentes accesibles existentes | 24-X-2026; host Web de tests, Release | `WebStartupTests.AppDashboard_WhenRequestedWithUnsupportedMethod_ReturnsNotFound`, 1/1; `WebStartupTests.PrivateResourceUnavailable_RendersGenericAccessibleNoticeWithoutInternalDetails`, 1/1 |

## Aceptación y cierre

- Estado del trabajo: completado y aceptado.
- Criterios con evidencia satisfactoria: CA01, CA02, CA03, CA04 y CA05. La validación cubre el dashboard, el `404` genérico de rutas privadas, el aviso genérico de recurso no accesible y la ausencia de fugas de información interna.
- Bloqueos y riesgos restantes: ninguno pendiente en el alcance de S7.7. Se valida la página `/app/404` como destino genérico sin exponer la propiedad o la presencia del recurso.
- Aceptación humana: DLG, 24-X-2026, referencia «Prueba superada».
- Archivado: no solicitado.
- Ubicación archivada: no aplica.
- Fecha efectiva de archivado y comprobación de enlaces: no aplica.
- Siguiente paso autorizado: cerrar S7.7 y continuar con la siguiente etapa del roadmap. La implementación queda revisada y aceptada dentro de los criterios de la spec.