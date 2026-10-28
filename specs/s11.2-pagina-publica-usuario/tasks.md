# Tareas: S11.2 — Página pública de usuario

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 27-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 27-X-2026 («Vamos»).
- Implementación:
  - Estado: en curso.
  - Inicio: 27-X-2026, por DLG («Inicia la implementación»; autorización para promover S11 y pausar S7).
  - Finalización: pendiente; registrar fecha, comprobación y referencia real al completar las tareas de implementación y validación.
  - Evidencia consolidada: pendiente; enlazar las tareas y validaciones que sustenten el estado.
- La aprobación de estas tareas no inicia la implementación ni inventa evidencias.

## Descomposición

Las tareas son secuenciales: la vista depende de que la consulta proporcione las colecciones correctas, las pruebas HTTP dependen de la página integrada y la aceptación depende de la evidencia de todas las validaciones. S11 está ahora en [«Now»](../../context/roadmap.md#now); S7 está pausada en Next y debe reanudarse antes de S8.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T06 | Sincronizar el inicio de S11.2 en el roadmap y el índice, dejando constancia de la pausa de S7 sin declararla cerrada. | Seguimiento SDD, [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | Ninguna | Confirmar que S11 aparece en Now, S7 queda pausada en Next y el índice refleja el inicio y T01 en curso. | completada | 27-X-2026: roadmap e índice actualizados conforme a la autorización de DLG; S7 queda abierta y pausada. |
| T01 | Ajustar la consulta de perfil para devolver las colecciones recientes en su orden previsto, conservar los filtros de publicación y añadir cobertura del límite, orden y empates en las pruebas del repositorio. | CA01; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [V03 y V04](plan.md#estrategia-de-validación) | T06 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~SqlitePublicReadRepositoryTests.GetUserPage"`; pasan los casos de colecciones recientes y enlaces públicos, incluido su límite y desempate. | completada | 27-X-2026: prueba focalizada 5/5 en .NET SDK 10.0.402; detectó inicialmente el orden alfabético anterior, y pasó tras aplicar el orden y límite del perfil. |
| T02 | Añadir la Razor Page pública y su `PageModel`, registrar `IPublicReadService`, renderizar las secciones y estados de la página, preparar sus metadatos/canonical y traducir solo el resultado de usuario no accesible a `404`. | CA01, CA02; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos) | T01 | `dotnet build src/Linkubator.Web/Linkubator.Web.csproj`; la Web compila con la ruta pública y el servicio conectados, sin errores. | completada | 27-X-2026: `dotnet build src\Linkubator.Web\Linkubator.Web.csproj`; compilación correcta, 0 advertencias y 0 errores. Canonical construida desde `PublicOrigin`; el layout no deriva el origen de `Request.Host`. |
| T03 | Añadir pruebas de integración HTTP para el perfil existente, estados vacíos, alias inexistente y antiguo, canonical con origen configurado, contenido privado ausente y HTML renderizado en servidor. | CA01, CA02; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), [V01 y V02](plan.md#estrategia-de-validación) | T02 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests.UserPage"`; pasan las aserciones HTTP de contenido, canonical, estados, `404`, privacidad y renderizado. | completada | 27-X-2026: pruebas focalizadas de `WebStartupTests.UserPage`, 3/3; perfil renderizado en servidor, canonical conserva el origen configurado frente a otro `Host`, contenido privado ausente, estados vacíos y `404` para alias inexistente/anterior. |
| T04 | Ejecutar la validación focalizada completa, revisar la integración de página, servicio, consulta y pruebas, y comprobar que el cambio no contiene errores de formato de diff. | CA01, CA02; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), [«Garantías técnicas»](plan.md#garantías-técnicas) | T03 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests.UserPage|FullyQualifiedName~SqlitePublicReadRepositoryTests.GetUserPage|FullyQualifiedName~WebStartupTests.HomePage_Renders_PublicLandingMetadataAndActions"` y `git diff --check`; pasan las pruebas focalizadas y no hay errores de whitespace. | bloqueada | 27-X-2026: pruebas combinadas 9/9 (3 HTTP de usuario, 5 de repositorio y 1 regresión de landing). `git diff --check` no se pudo ejecutar: `FEATURE\S11-web-5` no es un worktree Git. |
| T05 | Revisar la evidencia de CA01 y CA02 y registrar la decisión humana de aceptación o las correcciones requeridas. Mantener el estado de [roadmap.md](../../context/roadmap.md) acorde con la posición real de S11. | CA01, CA02; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T04 | Comprobar que V01–V04 tienen resultados satisfactorios y evidencia enlazada; registrar aceptación explícita o dejar constancia del bloqueo y su siguiente acción. | pendiente | Sin aceptación |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T02, T03, T04 | V01, V02, V03 y V04 en [plan.md](plan.md#estrategia-de-validación) | Verificado funcionalmente: 3/3 pruebas HTTP y 5/5 pruebas de repositorio; T04 conserva pendiente `git diff --check`. | 27-X-2026; .NET SDK 10.0.402 | T01, T03 y T04; comandos y resultados en la tabla de descomposición. |
| CA02 | T02, T03, T04 | V01 en [plan.md](plan.md#estrategia-de-validación) | Verificado: la prueba HTTP recibe el HTML renderizado en servidor; T04 conserva pendiente `git diff --check`. | 27-X-2026; .NET SDK 10.0.402 | T03 y T04; prueba `UserPage_RendersPublicContentMetadataAndConfiguredCanonical`. |

## Aceptación y cierre

- Estado del trabajo: en curso.
- Criterios sin evidencia satisfactoria: ninguno funcional; CA01 y CA02 tienen evidencia automatizada. El cierre técnico sigue pendiente de T04.
- Bloqueos y riesgos restantes: no se pudo ejecutar `git diff --check` porque la carpeta de trabajo no es un worktree Git. S11 sigue en «Now» y S7 permanece pausada, sin cierre ni archivo.
- Aceptación humana: pendiente; responsable, fecha y referencia cuando exista.
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso autorizado: ejecutar `git diff --check` en un worktree válido, completar T04 y someter la evidencia a aceptación humana; S7 permanece pausada y sin cierre o archivo.
