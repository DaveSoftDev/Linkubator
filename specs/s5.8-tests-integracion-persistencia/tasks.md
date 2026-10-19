# Tareas: S5.8 — Tests de integración de persistencia

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 19-X-2026 («Siguiente»).
    - Tareas: aprobadas por DLG el 19-X-2026 («Vamos»), instrucción explícita en esta conversación.
- Implementación:
    - Estado: completada el 19-X-2026.
    - Inicio: DLG, 19-X-2026 («Implementa»), instrucción explícita en esta conversación.
    - Finalización: 19-X-2026; tests focalizados 13/13, suite completa 1045/1045, build de solución y `git diff --check` correctos.
    - Evidencia consolidada: [T01–T06](#descomposición), [evidencia por criterio](#evidencia-por-criterio), [SqlitePersistenceIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/SqlitePersistenceIntegrationTests.cs), [LinkService.cs](../../src/Linkubator.Application/UseCases/Links/LinkService.cs).
- La implementación se ejecutó tras la aprobación de las tareas y quedó completada el 19-X-2026.

## Descomposición

Las tareas se ejecutan secuencialmente porque comparten la fixture de base temporal y la suite de integración.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Añadir tests de integración para las claves únicas aplicables a `User`, `Collection` y `Link`, y para la FK compuesta, usando SQL directo sobre la base temporal migrada. | CA01–CA02; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [V01](plan.md#estrategia-de-validación) | Ninguna | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~SqlitePersistenceIntegration --verbosity minimal`; resultado esperado: SQLite rechaza duplicados y relaciones inconsistentes, permite alias nulos admitidos y no deja filas parciales. | completada | 19-X-2026: `SqlitePersistenceIntegrationTests`, 6/6; migraciones aplicadas a bases temporales, build y tests correctos en .NET 10.0.12. |
| T02 | Añadir test de `MoveLink` con repositorios y Unit of Work reales, verificando el rechazo de una colección ajena y el estado persistido sin cambios. | CA05; [plan → Pruebas integradas de Application e Infrastructure](plan.md#componentes-y-contratos), [V03](plan.md#estrategia-de-validación) | T01 | Ejecutar el test focalizado de integración; resultado esperado: Application rechaza el destino ajeno, SQLite conserva la FK compuesta y el enlace original no cambia. | completada | 19-X-2026: `SqlitePersistenceIntegrationTests`, 9/9; `MoveLink` rechaza el destino ajeno, la FK rechaza el `UPDATE` manipulado y el enlace queda intacto. |
| T03 | Añadir pruebas concurrentes de eliminación de colecciones y de privatización frente a publicación de un enlace existente, verificando estado final y atomicidad. | CA03–CA04; [plan → Pruebas de concurrencia restantes](plan.md#componentes-y-contratos), [V02](plan.md#estrategia-de-validación) | T02 | Ejecutar los escenarios coordinados sin esperas artificiales; resultado esperado: sobrevive al menos una colección y no queda enlace público en colección privada ni estado parcial. | completada | 19-X-2026: `SqlitePersistenceIntegrationTests`, 9/9; ambas carreras conservan las invariantes y los rechazos no dejan escrituras parciales. |
| T04 | Añadir la intercalación determinista de CA07 y trasladar las comprobaciones de colección/duplicidad de `LinkService.CreateLink` dentro de `BEGIN IMMEDIATE`, con rollback en los resultados no exitosos. | CA07; [plan → Transacción de creación de enlaces](plan.md#componentes-y-contratos), [V05](plan.md#estrategia-de-validación) | T03 | Confirmar primero que el test controlado falla con el flujo anterior; después ejecutar el test y los tests de `LinkService`; resultado esperado: tras privatizar la colección, la creación pública se rechaza y no persiste enlace ni escritura parcial. | completada | 19-X-2026: el test de CA07 falló con el flujo previo (se creó el enlace público tras privatizar la colección) y pasó después del ajuste; `SqlitePersistenceIntegrationTests` + `LinkServiceTests`, 86/86. |
| T05 | Completar tests de orden integrado para listados privados de colecciones, públicos de colecciones y públicos de enlaces de una colección, incluidos equivalencias lingüísticas y desempate por `Id`. | CA06; [plan → Listados](plan.md#componentes-y-contratos), [V04](plan.md#estrategia-de-validación) | T04 | Ejecutar los tests de listados con la base temporal; resultado esperado: cada secuencia completa coincide con el orden propietario y su desempate. | completada | 19-X-2026: `SqlitePersistenceIntegrationTests`, 13/13; listados privados/públicos y fallback de URL respetan la colación y el desempate por Id. |
| T06 | Ejecutar validación focalizada, suite completa, build y revisión de alcance; consolidar evidencia por criterio y sincronizar el estado real de implementación en roadmap y tareas. | CA01–CA07; [plan → V01–V06](plan.md#estrategia-de-validación) | T01–T05 | Ejecutar los comandos de V06; resultado esperado: tests focalizados y suite completa pasan, build y `git diff --check` terminan correctamente, y cada criterio tiene evidencia registrada sin atribuir cobertura de FTS5 o de revisiones manuales. | completada | 19-X-2026: filtro `FullyQualifiedName~SqlitePersistenceIntegrationTests` 13/13; suite completa 1045/1045; build de solución y `git diff --check` correctos; diagnóstico de archivos sin errores. .NET 10.0.12. |
| T07 | Revisar evidencias y solicitar/registrar la aceptación humana; actualizar índice y roadmap solo con el estado y referencia reales. | CA01–CA07; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T06 | Revisar cobertura CA→tarea→validación→evidencia y registrar aceptación explícita o bloqueo; resultado esperado: el estado de cierre refleja la decisión humana sin inferirla de los tests. | completada | DLG aceptó S5.8 el 19-X-2026 con «Completada», instrucción explícita en esta conversación; índice y roadmap sincronizados. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T06 | V01, V06 | Evidencia satisfactoria: SQLite rechaza duplicados de email, alias, slug por usuario y URL normalizada por usuario; admite alias nulos múltiples. | 19-X-2026, .NET 10.0.12 | [SqlitePersistenceIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/SqlitePersistenceIntegrationTests.cs), 13/13; suite completa 1045/1045. |
| CA02 | T01, T06 | V01, V06 | Evidencia satisfactoria: la FK compuesta rechaza la colección de otro usuario y no crea el enlace. | 19-X-2026, .NET 10.0.12 | [SqlitePersistenceIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/SqlitePersistenceIntegrationTests.cs), 13/13; suite completa 1045/1045. |
| CA03 | T03, T06 | V02, V06 | Evidencia satisfactoria: dos eliminaciones concurrentes no eliminan ambas colecciones. | 19-X-2026, .NET 10.0.12 | [SqlitePersistenceIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/SqlitePersistenceIntegrationTests.cs), 13/13; suite completa 1045/1045. |
| CA04 | T03, T06 | V02, V06 | Evidencia satisfactoria: privatización y publicación concurrentes dejan privadas tanto la colección como el enlace. | 19-X-2026, .NET 10.0.12 | [SqlitePersistenceIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/SqlitePersistenceIntegrationTests.cs), 13/13; suite completa 1045/1045. |
| CA05 | T02, T06 | V03, V06 | Evidencia satisfactoria: Application rechaza el destino ajeno y la FK rechaza una actualización SQL manipulada sin cambiar el enlace original. | 19-X-2026, .NET 10.0.12 | [SqlitePersistenceIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/SqlitePersistenceIntegrationTests.cs), 13/13; suite completa 1045/1045. |
| CA06 | T05, T06 | V04, V06 | Evidencia satisfactoria: orden de colecciones privadas, colecciones públicas y enlaces públicos de colección; incluye desempate por `Id` y fallback a URL. | 19-X-2026, .NET 10.0.12 | [SqlitePersistenceIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/SqlitePersistenceIntegrationTests.cs), 13/13; suite completa 1045/1045. |
| CA07 | T04, T06 | V05, V06 | Evidencia satisfactoria roja/verde: la intercalación reproduce el enlace público en colección privada antes del arreglo y verifica rechazo posterior. | 19-X-2026, .NET 10.0.12 | `CreatePublicLink_WhenCollectionBecomesPrivateBeforeUnitOfWork_RejectsCreation`; falla antes y pasa después; [suite completa](../../tests/Linkubator.Tests/Integration/SqlitePersistenceIntegrationTests.cs), 1045/1045. |

## Aceptación y cierre

- Estado del trabajo: completado y aceptado por DLG para el alcance S5.8.
- Criterios sin evidencia satisfactoria: ninguno conocido; CA01–CA07 cuentan con evidencia satisfactoria.
- Bloqueos y riesgos restantes: no hay bloqueos técnicos conocidos. Esta implementación no sustituye la revisión manual de S5.9/S5.10 ni completa la etapa Persistencia.
- Aceptación humana: DLG, 19-X-2026 («Completada»), instrucción explícita en esta conversación.
- Archivado: pendiente; DLG solicitó archivar los trabajos S5 cerrados el 19-X-2026 («Archiva las tareas pendientes»).
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso autorizado, sin cerrar otras etapas: S5.8 queda cerrada para su alcance. S5.9 y S5.10 mantienen sus revisiones propias; el archivado de S5.8 no se ha solicitado.