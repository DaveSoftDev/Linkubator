# Plan técnico: S8.4 — Identificadores y respuestas de colecciones

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 25-X-2026 («Siguiente»).
  - Plan: DLG, 25-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

- Las páginas `EditModel` y `DeleteModel` de Web son la frontera que interpreta el `id` de ruta y traduce los resultados de `ICollectionService`; no introducir lógica de persistencia ni cambiar validadores, resultados o reglas de Application.
- Reutilizar el presentador y parcial `PrivateResourceUnavailablePresenter` / `_PrivateResourceUnavailable` para las respuestas GET de recurso no accesible, con la ruta de listado de colecciones como destino contextual.
- Para POST de eliminación, preservar la traducción de `ResourceNotAccessible` en el aviso y redirección al listado según la fuente propietaria. Las respuestas de identificador inválido mantienen la validación ya definida.
- Mantener el límite de capas Web → Application → Domain y la identidad procedente del servidor; comprobar las firmas y errores existentes durante la preparación de implementación.

### Flujos internos

1. GET de edición y eliminación valida el formato del identificador antes de invocar el servicio. Si es válido, la página consume `GetCollection` con la identidad de servidor y traduce un resultado inaccesible al mensaje común GET aplicable.
2. POST de eliminación valida el identificador antes de invocar `DeleteCollection`. Un resultado inaccesible utiliza el aviso de eliminación y redirige al listado; un identificador inválido no modifica datos.
3. Las pruebas integradas HTTP establecen usuarios/datos aislados, contrastan la colección inexistente y la ajena, y observan respuesta, contenido y estado persistido sin fiarse solo de la inspección del markup.

### Persistencia y dependencias

- No se añade esquema, SQL ni transacción a Web. Se reutilizan los casos de uso y persistencia existentes.
- Prerrequisitos disponibles: páginas de operaciones de S8.2, servicio de colecciones y presentador Web común de S7.7. La suite propia de S8.4 debe poder ejecutarse de forma focalizada con las pruebas HTTP existentes; S8.5 no es dependencia de cierre.
- No esperar al usuario ni realizar E/S externa dentro de transacciones; las operaciones relevantes aplican las garantías existentes de Application.

### Garantías técnicas

- Validar identificadores inválidos antes de consultar o modificar un recurso.
- No aceptar un identificador de usuario del cliente como autoridad; aislar datos por identidad de servidor.
- No revelar si una colección no existe o pertenece a otro usuario y no incluir datos de colección en respuestas inaccesibles.
- Comprobar respuestas GET y POST y estado antes/después. Incluir una prueba negativa cambiando temporalmente la expectativa de equivalencia para que el caso de colección ajena falle; restaurar el test a su expectativa aprobada y ejecutar de nuevo.
- No atribuir a la suite transversal S8.5 ni a inspección de código la evidencia que corresponde a los criterios de esta spec.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Reutilizar `EditModel`, `DeleteModel`, el servicio y el presentador compartido de respuestas privadas; limitar cambios al tratamiento y las pruebas de integración necesarias. | propuesta | [spec.md → «Alcance»](spec.md#alcance), [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura), implementación actual de Web. | Reutiliza el límite existente y evita reglas duplicadas; confirmar contratos/error codes al iniciar la implementación. |
| Cubrir en tests propios los casos discriminantes de edición/eliminación, GET y POST según aplique, incluso con una prueba negativa de sensibilidad. | propuesta | [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación), [S8.5](../../plans/mvp0-plan.md#s8-gestión-de-colecciones-l). | Permite cierre independiente de S8.5; seleccionar los fixtures/servidor HTTP existente en la tarea de inspección. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada HTTP; GET de edición y eliminación con identificadores inválidos. Comprobar ausencia de llamada a la lectura cuando sea observable, respuesta de validación y estado persistido sin cambio. | Test host y cliente HTTP; ruta de colecciones existente. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~CollectionsListingIntegrationTests" --no-restore -m:1 -p:BuildInParallel=false`. | Ambas rutas rechazan el ID antes de consultar/modificar y no revelan datos. La expectativa debe ser sensible a una llamada no autorizada. | Suite `CollectionsListingIntegrationTests`: 12/12 pasó el 25-X-2026; los casos GET inválidos devuelven validación sin contenido de colección. La ausencia de consulta se infiere del manejador antes de Application y no se instrumentó directamente. |
| V02 | CA02 | Automatizada HTTP; comparar edición GET y eliminación GET para ID inexistente y de otra persona; verificar ausencia de datos y respuesta común pertinente. | Dos usuarios/identidades aisladas y colecciones de prueba. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~CollectionsListingIntegrationTests" --no-restore -m:1 -p:BuildInParallel=false`. | Cada par de causas produce la misma clase de respuesta y contenido no discriminante; eliminación incluye su enlace al listado. | Suite focalizada 12/12 el 25-X-2026. Los HTML de edición son iguales tras normalizar antiforgery; eliminación presenta mismo mensaje y enlace. Los cuerpos íntegros de DELETE GET no se compararon por el antiforgery. |
| V03 | CA03 | Automatizada HTTP; POST de eliminación con identificador inválido, existente inexistente y ajeno; comprobar respuesta/redirección y estado persistido. | Cliente HTTP con soporte antiforgery; fixture aislada. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~CollectionsListingIntegrationTests" --no-restore -m:1 -p:BuildInParallel=false`. | La validación o el aviso/redirección coinciden con las fuentes y ninguna colección resulta alterada o eliminada. | Suite focalizada 12/12 el 25-X-2026. POST inválido conserva el conteo; IDs inexistente/ajeno redirigen al mismo listado, el GET siguiente muestra el aviso y el conteo propio no cambia. |
| V04 | CA04 | Automatizada HTTP y comprobación negativa; comparar respuestas GET y POST inaccesibles y verificar no alteración para ambos usuarios. Invertir temporalmente una aserción de equivalencia y restaurarla. | V02 y V03 implementados; fixture de dos usuarios. | Ejecutar el test de casos GET en estado normal, luego con expectativa temporalmente invertida y finalmente la suite focalizada restaurada. | La aserción de equivalencia responde a diferencias reales: con la expectativa invertida el test falla porque los cuerpos comparados son iguales; restaurada, la suite focalizada pasa. | 25-X-2026: la aserción temporal `Assert.NotEqual` falló en `GetCollectionOperations_WhenCollectionIsMissingOrForeign_ReturnsNonDisclosingResponses` con “Strings are equal”; se restauró a `Assert.Equal`. Suite restaurada `CollectionsListingIntegrationTests` pasó 12/12. La prueba negativa valida sensibilidad de la aserción sobre igualdad de HTML, no simula una vulnerabilidad de autorización. |

El filtro y el comando concretos se descubrirán mediante la tarea de inspección de tests; no se atribuye sensibilidad a una compilación ni se usa una validación futura de S8.5 como evidencia.

## Orden de ejecución y cierre

- Tras aprobar el plan se creará `tasks.md` a partir de su plantilla, con tareas acotadas, dependencias, checks inmediatos y validación de CA01–CA04.
- La inspección de las pruebas/fixtures HTTP precede a la implementación de casos de integración; cierre requiere ejecución de esas pruebas y evidencia satisfactoria por criterio.
- La aceptación humana se registra en las tareas y no sustituye la evidencia técnica.
- Ante discrepancia de regla o contrato, se bloquea la tarea y se tramita la decisión en su documento propietario antes de modificar el comportamiento.
