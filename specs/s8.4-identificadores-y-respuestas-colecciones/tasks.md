# Tareas: S8.4 — Identificadores y respuestas de colecciones

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 25-X-2026 («Siguiente»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 25-X-2026 («Siguiente»).
  - Tareas: DLG, 25-X-2026 («Vamos»).
- Implementación:
  - Estado: completada.
  - Inicio: 25-X-2026, DLG («Inicia la implementación»).
  - Finalización: 25-X-2026; suite focalizada `CollectionsListingIntegrationTests` 12/12, suite completa `Linkubator.Tests` 1,203/1,203 y `git diff --check` satisfactorios.
  - Evidencia consolidada: [evidencia por criterio](#evidencia-por-criterio); aceptación humana pendiente.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T01 detecta los tests HTTP y fixtures disponibles; después se implementan validaciones independientes para los criterios GET/POST y, por último, se ejecutan pruebas integradas y la comprobación negativa de sensibilidad. S8.5 no es dependencia; S8.4 mantiene evidencia suficiente en su propia suite focalizada.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Inspeccionar las páginas de colección, los resultados del servicio, el test host, las fixtures HTTP y sus filtros; confirmar cómo observar llamadas, respuestas y estado de persistencia. | CA01–CA04; [plan → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Estrategia de validación»](plan.md#estrategia-de-validación) | Ninguna | Identificar los tests/fixtures reutilizables y el filtro de test válido; confirmar qué mecanismo permite crear usuario ajeno y observar la DB. | completada | 25-X-2026: inspeccionados `EditModel`, `DeleteModel`, `CollectionsModel`, `PrivateResourceUnavailablePresenter`, `CollectionService`, `CollectionsListingIntegrationTests` y `WebStartupTests`. Tests usan xUnit/VSTest por `dotnet test` y test host con SQLite en archivo temporal; `SeedOtherUsersCollection` se amplió para devolver el ID ajeno y se añadieron helpers de consulta y token antiforgery. Se puede comprobar estado persistido con SQLite/Dapper. El filtro `FullyQualifiedName~CollectionsListingIntegrationTests` no llegó a ejecutar tests: MSB3026 indica ensamblados Web bloqueados por `Linkubator.Web (22780)` y `Visual Studio Debug Adapter for .NET (16712)`; no se detuvieron procesos. |
| T02 | Añadir tests HTTP de GET para IDs inválidos e IDs válidos inexistentes/ajenos en edición y eliminación; verificar respuesta, contenido y que no se invoca consulta con ID inválido. | CA01–CA02; [plan → «Flujos internos»](plan.md#flujos-internos), V01–V02 | T01 | Ejecutar los nuevos casos GET; inválidos no consultan recurso y causas válidas no accesibles comparten la respuesta fuente por ruta. | completada | Añadidas pruebas para GET inválido y colección inexistente/ajena en edición y eliminación; suite focalizada posterior 12/12. El test observa respuesta y contenido, sin instrumentar directamente la llamada Web al servicio. |
| T03 | Añadir tests HTTP de POST de eliminación para ID inválido, inexistente y ajeno; comprobar validación, aviso/redirección y conservación de datos. | CA03; [plan → «Flujos internos»](plan.md#flujos-internos), V03 | T01 | Ejecutar los casos POST con antiforgery; resultados conforme a fuentes y base de datos sin cambios. | completada | Añadidas y ejecutadas pruebas POST con antiforgery, respuesta de validación, redirección/aviso y estado persistido sin cambios; suite focalizada 12/12. |
| T04 | Añadir y ejecutar comprobación negativa de sensibilidad que detecte una respuesta discriminante o acceso no autorizado, restaurando después la expectativa correcta. | CA04; [plan → «Garantías técnicas»](plan.md#garantías-técnicas), V04 | T02, T03 | Ejecutar prueba negativa temporalmente invertida y suite focalizada restaurada; la primera falla por la respuesta observada y la última pasa. | completada | 25-X-2026: invertida temporalmente la aserción de igualdad de respuestas a `Assert.NotEqual`; `GetCollectionOperations_WhenCollectionIsMissingOrForeign_ReturnsNonDisclosingResponses` falló con “Strings are equal”. Restaurada la expectativa y suite focalizada 12/12. Comprueba sensibilidad de igualdad observable, no inyección de un defecto de autorización. |
| T05 | Ejecutar validación integrada focalizada, revisar cobertura fuente→criterio→test y comprobar cambios documentales y diff. | CA01–CA04; [plan → «Estrategia de validación»](plan.md#estrategia-de-validación) | T04 | Ejecutar el filtro focalizado descubierto en T01 y `git diff --check`; los tests pasan, el diff es limpio y no hay cambios a reglas propietarias. | completada | 25-X-2026: suite focalizada `dotnet test tests\\Linkubator.Tests\\Linkubator.Tests.csproj --filter "FullyQualifiedName~CollectionsListingIntegrationTests" --no-restore -m:1 -p:BuildInParallel=false`: PASS 12/12. Tras corregir dos fallos existentes —expectativa de logging obsoleta y respuesta de método no permitido en ruta privada—, el filtro de esos dos tests pasó 2/2 y la suite completa `dotnet test tests\\Linkubator.Tests\\Linkubator.Tests.csproj --no-restore -m:1 -p:BuildInParallel=false` pasó 1,203/1,203, 0 fallidos, 0 omitidos. `git diff --check`: PASS. Los dos arreglos son validación global y no se atribuyen como cambios exclusivos de S8.4. Los límites de cobertura S8.4 constan en «Evidencia por criterio». |
| T06 | Revisar evidencia y registrar aceptación humana por separado de la validación técnica. | CA01–CA04; [plan → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T05 | Presentar evidencia por criterio y registrar decisión explícita, sin cerrar trabajo por pruebas verdes. | pendiente | Sin aceptación |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T02, T05 | V01, V05 en [plan.md](plan.md#estrategia-de-validación) | GET de edición y eliminación con ID inválido devuelven validación y no muestran nombre de colección; no se instrumentó directamente la llamada interna al servicio. | 25-X-2026; .NET 10.0.401, xUnit/VSTest | `CollectionsListingIntegrationTests` 12/12 |
| CA02 | T01, T02, T05 | V02, V05 en [plan.md](plan.md#estrategia-de-validación) | Para IDs inexistente/ajeno, edición presenta aviso común con HTML igual tras normalizar antiforgery; eliminación GET presenta mensaje específico y enlace al listado, sin nombre extranjero. | 25-X-2026; .NET 10.0.401 | `GetCollectionOperations_WhenCollectionIsMissingOrForeign_ReturnsNonDisclosingResponses`; suite 12/12 |
| CA03 | T01, T03, T05 | V03, V05 en [plan.md](plan.md#estrategia-de-validación) | POST inválido muestra validación sin alterar conteo; IDs inexistente/ajeno redirigen al mismo listado, se muestra aviso tras redirect y el conteo del usuario identificado no cambia. | 25-X-2026; .NET 10.0.401 | `DeleteCollection_WhenIdIsInvalid_ReturnsValidationAndLeavesCollectionsUnchanged`; `DeleteCollection_WhenIdIsMissingOrForeign_RedirectsWithSameNoticeAndDoesNotChangeData`; suite 12/12 |
| CA04 | T01–T05 | V04, V05 en [plan.md](plan.md#estrategia-de-validación) | La aserción de igualdad de HTML detectó una inversión temporal (`Assert.NotEqual` falló porque las respuestas normalizadas eran iguales); respuestas de eliminación también coinciden en status y contenido de mensaje/enlace observados. La sensibilidad comprueba la aserción de no discriminación, no un defecto deliberado de autorización. | 25-X-2026; .NET 10.0.401 | `GetCollectionOperations_WhenCollectionIsMissingOrForeign_ReturnsNonDisclosingResponses`; ejecución negativa falló como esperado, restauración pasó; suite 12/12 |

## Aceptación y cierre

- Estado del trabajo: pendiente de aceptación humana.
- Criterios sin evidencia satisfactoria: ninguno para el alcance probado; límites de cobertura registrados por criterio, incluyendo la ausencia de instrumentación directa de la llamada Web/Application para CA01 y la comparación de DELETE GET con tokens antiforgery.
- Bloqueos y riesgos restantes: los primeros builds focalizados estuvieron bloqueados por ensamblados Web activos; en ejecuciones posteriores la validación focalizada y suite completa compilaron y pasaron. La prueba negativa verifica sensibilidad de la aserción de igualdad, no la detección de una vulnerabilidad de autorización. Los resultados completos no equivalen a aceptación humana del trabajo.
- Aprobación de tareas: DLG, 25-X-2026 («Vamos»).
- Aceptación humana del resultado: pendiente; la validación técnica no constituye aceptación. Registrar responsable, fecha y referencia cuando exista.
- Archivado: no solicitado.
- Siguiente paso autorizado: solicitar aprobación explícita de estas tareas; después podrá iniciarse su ejecución.
