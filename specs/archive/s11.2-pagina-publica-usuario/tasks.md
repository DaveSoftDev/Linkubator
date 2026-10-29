# Tareas: S11.2 — Página pública de usuario

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 27-X-2026 («Siguiente»).
  - Tareas: aprobadas por DLG el 27-X-2026 («Vamos»).
- Implementación:
  - Estado: completada.
  - Inicio: 27-X-2026, por DLG («Inicia la implementación»; autorización para promover S11 y pausar S7).
  - Finalización técnica: 28-X-2026, tras completar T04; aceptación humana de T05 registrada el 28-X-2026 con la referencia «Completado».
  - Evidencia consolidada: T01–T04 y [«Validación de T04»](#validación-de-t04); V01–V04 satisfactorias sobre el checkout identificado.
- La aprobación de estas tareas no inicia la implementación ni inventa evidencias.

## Descomposición

Las tareas son secuenciales: la vista depende de que la consulta proporcione las colecciones correctas, las pruebas HTTP dependen de la página integrada y la aceptación depende de la evidencia de todas las validaciones. S11 está ahora en [«Now»](../../../context/roadmap.md#now); S7 está pausada en Next y debe reanudarse antes de S8.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T06 | Sincronizar el inicio de S11.2 en el roadmap y el índice, dejando constancia de la pausa de S7 sin declararla cerrada. | Seguimiento SDD, [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | Ninguna | Confirmar que S11 aparece en Now, S7 queda pausada en Next y el índice refleja el inicio y T01 en curso. | completada | 27-X-2026: roadmap e índice actualizados conforme a la autorización de DLG; S7 queda abierta y pausada. |
| T01 | Ajustar la consulta de perfil para devolver las colecciones recientes en su orden previsto, conservar los filtros de publicación y añadir cobertura del límite, orden y empates en las pruebas del repositorio. | CA01; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [V03 y V04](plan.md#estrategia-de-validación) | T06 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~SqlitePublicReadRepositoryTests.GetUserPage"`; pasan los casos de colecciones recientes y enlaces públicos, incluido su límite y desempate. | completada | 27-X-2026: prueba focalizada 5/5 en .NET SDK 10.0.402; detectó inicialmente el orden alfabético anterior, y pasó tras aplicar el orden y límite del perfil. |
| T02 | Añadir la Razor Page pública y su `PageModel`, registrar `IPublicReadService`, renderizar las secciones y estados de la página, preparar sus metadatos/canonical y traducir solo el resultado de usuario no accesible a `404`. | CA01, CA02; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos) | T01 | `dotnet build src/Linkubator.Web/Linkubator.Web.csproj`; la Web compila con la ruta pública y el servicio conectados, sin errores. | completada | 27-X-2026: `dotnet build src\Linkubator.Web\Linkubator.Web.csproj`; compilación correcta, 0 advertencias y 0 errores. Canonical construida desde `PublicOrigin`; el layout no deriva el origen de `Request.Host`. |
| T03 | Añadir pruebas de integración HTTP para el perfil existente, estados vacíos, alias inexistente y antiguo, canonical con origen configurado, contenido privado ausente y HTML renderizado en servidor. | CA01, CA02; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), [V01 y V02](plan.md#estrategia-de-validación) | T02 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests.UserPage"`; pasan las aserciones HTTP de contenido, canonical, estados, `404`, privacidad y renderizado. | completada | 27-X-2026: pruebas focalizadas de `WebStartupTests.UserPage`, 3/3; perfil renderizado en servidor, canonical conserva el origen configurado frente a otro `Host`, contenido privado ausente, estados vacíos y `404` para alias inexistente/anterior. |
| T04 | Ejecutar la validación focalizada completa, revisar la integración de página, servicio, consulta y pruebas, y comprobar que el cambio no contiene errores de formato de diff. | CA01, CA02; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), [«Garantías técnicas»](plan.md#garantías-técnicas) | T03 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests.UserPage|FullyQualifiedName~SqlitePublicReadRepositoryTests.GetUserPage|FullyQualifiedName~WebStartupTests.HomePage_Renders_PublicLandingMetadataAndActions"` y `git diff --check`; pasan las pruebas focalizadas y no hay errores de whitespace. | completada | 28-X-2026: repetición combinada 10/10 en .NET SDK 10.0.401; revisión de integración satisfactoria y comprobaciones de formato actuales e históricas sin errores. El bloqueo Git registrado el 27-X-2026 ya no se reproduce; alcance, comandos y revisión en [«Validación de T04»](#validación-de-t04). |
| T05 | Revisar la evidencia de CA01 y CA02 y registrar la decisión humana de aceptación o las correcciones requeridas. Mantener el estado de [roadmap.md](../../../context/roadmap.md) acorde con la posición real de S11. | CA01, CA02; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T04 | Comprobar que V01–V04 tienen resultados satisfactorios y evidencia enlazada; registrar aceptación explícita o dejar constancia del bloqueo y su siguiente acción. | completada | 28-X-2026: DLG acepta el resultado con la referencia «Completado», mediante la instrucción explícita «Usa "Completado" como aceptación humana para T05» en esta conversación. Roadmap e índice sincronizados con el cierre de S11.2, sin archivado. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T02, T03, T04 | V01, V02, V03 y V04 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: repetición combinada 10/10; contenido público, privacidad, estados, metadatos, canonical, límites y orden comprobados. | 28-X-2026; .NET SDK 10.0.401, `net10.0`, VSTest/xUnit 2 | [«Validación de T04»](#validación-de-t04). |
| CA02 | T02, T03, T04 | V01 en [plan.md](plan.md#estrategia-de-validación) | Satisfactorio: las aserciones HTTP verifican el HTML renderizado en servidor; repetición combinada 10/10. | 28-X-2026; .NET SDK 10.0.401, `net10.0`, VSTest/xUnit 2 | [«Validación de T04»](#validación-de-t04); prueba `UserPage_RendersPublicContentMetadataAndConfiguredCanonical`. |

## Validación de T04

- Fecha y autorización: 28-X-2026, petición de DLG «Puedes coimpletar T04 tu?» en esta conversación.
- Estado validado: commit `5988520536cffa4bcee711ccd9125022310b65fb`; árbol limpio antes de actualizar esta evidencia. SDK 10.0.401, `net10.0`, VSTest con xUnit 2.
- Comprobación automatizada ejecutada:

```powershell
dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~WebStartupTests.UserPage|FullyQualifiedName~SqlitePublicReadRepositoryTests.GetUserPage|FullyQualifiedName~WebStartupTests.HomePage_Renders_PublicLandingMetadataAndActions"
```

- Resultado: 10/10 pruebas superadas, ninguna omitida ni fallida; la ejecución compila los proyectos sin restaurar dependencias. Cubre cuatro pruebas HTTP de perfil, cinco de repositorio y una regresión de landing.
- Revisión de integración: [User.cshtml.cs](../../../src/Linkubator.Web/Pages/Public/User.cshtml.cs) usa `IPublicReadService`, distingue usuario no accesible de fallos técnicos y construye la canonical con el origen configurado; [User.cshtml](../../../src/Linkubator.Web/Pages/Public/User.cshtml) renderiza el DTO público mediante Razor. [Program.cs](../../../src/Linkubator.Web/Program.cs) registra el servicio; [PublicReadService.cs](../../../src/Linkubator.Application/UseCases/PublicReads/PublicReadService.cs) delega en el repositorio y [SqlitePublicReadRepository.cs](../../../src/Linkubator.Infrastructure/Repositories/SqlitePublicReadRepository.cs) conserva filtros públicos, límites y desempates. Las pruebas focalizadas comprueban esa integración y la salida HTTP.
- Formato del checkout: `git diff --check` y `git diff --cached --check` terminaron con código 0. Ambos diffs estaban vacíos; esto no se usa como evidencia del formato histórico.
- Formato histórico focalizado: el siguiente comando terminó con código 0 y sin errores de whitespace sobre el cambio de implementación y los archivos compartidos implicados, sin atribuir cobertura a todo S11:

```powershell
git --no-pager diff --check 5988520^ 5988520 -- src\Linkubator.Web\Pages\Public\User.cshtml src\Linkubator.Web\Pages\Public\User.cshtml.cs src\Linkubator.Web\Pages\Shared\_PublicLayout.cshtml src\Linkubator.Web\Pages\Shared\_Layout.cshtml src\Linkubator.Web\wwwroot\css\public.css src\Linkubator.Web\Extensions\AliasExtensions.cs src\Linkubator.Web\Extensions\UrlExtensions.cs src\Linkubator.Web\Program.cs src\Linkubator.Application\UseCases\PublicReads\PublicReadService.cs src\Linkubator.Infrastructure\Repositories\SqlitePublicReadRepository.cs tests\Linkubator.Tests\Integration\WebStartupTests.cs tests\Linkubator.Tests\Infrastructure\SqlitePublicReadRepositoryTests.cs
```

- Conclusión: T04 completada; el bloqueo Git anterior no se reproduce en el worktree actual. No se modifica código, no se concede aceptación humana y no se cierran otras specs.

## Aceptación y cierre

- Estado del trabajo: completado, aceptado y cerrado el 28-X-2026; T01–T06 completadas.
- Criterios sin evidencia satisfactoria: ninguno; CA01 y CA02 cuentan con evidencia vigente en T04.
- Bloqueos y riesgos restantes: ninguno técnico identificado en T04. S11 sigue en «Now» y S7 permanece pausada, sin cierre ni archivo.
- Aceptación humana: DLG, 28-X-2026, referencia «Completado», autorizada expresamente en esta conversación mediante «Usa "Completado" como aceptación humana para T05».
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso: archivar solo si se solicita expresamente y se comprueban las condiciones de traslado y enlaces. El cierre de S11.2 satisface el prerrequisito de cierre de S11.4 y S11.5, pero no completa sus tareas ni concede su aceptación. S7 permanece pausada y sin cierre o archivo.
