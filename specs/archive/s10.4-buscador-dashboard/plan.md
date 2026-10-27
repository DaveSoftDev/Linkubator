# Plan técnico: S10.4 — Conectar el buscador del dashboard

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»), decisión explícita en esta conversación.
  - Plan: DLG, 27-X-2026 («Siguiente»), decisión explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- [DashboardModel](../../../src/Linkubator.Web/Pages/App/Dashboard.cshtml.cs) configura el `SearchFormViewModel` que consume la vista del dashboard. Mantener la búsqueda en el límite de presentación Web: el formulario navega al listado, sin invocar un caso de uso de búsqueda desde el GET del dashboard.
- [Dashboard.cshtml](../../../src/Linkubator.Web/Pages/App/Dashboard.cshtml) reutiliza el componente compartido [_SearchForm.cshtml](../../../src/Linkubator.Web/Pages/Shared/_SearchForm.cshtml). Revisar el `action`, el método, el parámetro de consulta y los campos ocultos que renderiza el modelo; modificar solo lo que resulte necesario para satisfacer los criterios.
- [LinksModel](../../../src/Linkubator.Web/Pages/App/Links.cshtml.cs) ya es el consumidor del listado privado. Reutilizar su análisis y flujo de consulta sin alterar los contratos ni las reglas entregadas por S10.1–S10.3.
- La prueba existente [WebStartupTests.cs](../../../tests/Linkubator.Tests/Integration/WebStartupTests.cs) comprueba que el dashboard renderiza el destino `/app/enlaces` y un campo `q`, además de estado vacío y ausencia de contenido no pertinente. Inspeccionar si sus aserciones bastan; añadir comprobación de extremo a extremo focalizada solo si hace falta para distinguir la navegación real y la búsqueda del listado.

### Flujos internos

1. El GET del dashboard renderiza su formulario compartido configurado para enviar una consulta al listado privado de enlaces.
2. Al enviar el formulario, el navegador solicita el destino por GET con el campo `q`.
3. El listado procesa la consulta mediante el flujo ya existente, con la identidad obtenida por los mecanismos privados establecidos; no se cambia el resultado ni se devuelve la búsqueda al dashboard.
4. Los errores y límites de la consulta permanecen bajo el tratamiento ya implementado en el listado y sus fuentes propietarias.

### Persistencia y dependencias

- No se prevén cambios de Application, persistencia, esquema, transacciones ni dependencias externas.
- Reutilizar la factoría Web de integración y la base temporal existente en los tests, cuando sea suficiente para verificar la navegación. Si se requiere un fixture de enlaces para demostrar resultados, usar únicamente los mecanismos de tests ya existentes y con aislamiento de datos.
- Los trabajos S10.1–S10.3 y S7.7 están disponibles. La suite transversal de S10.5 y la revisión manual de S10.6 no son prerrequisitos de cierre.

### Garantías técnicas

- Mantener la búsqueda como GET al listado, sin aceptar identidad desde la consulta del cliente ni añadir lógica de filtrado al dashboard ([AC-DASH-01](../../../plans/mvp0-acceptance-criteria.md#ac-dash-01-dashboard), [«Rutas privadas»](../../../context/specifications.md#rutas-privadas)).
- Mantener el aislamiento por usuario identificado al presentar resultados ([domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos)).
- Mantener la semántica del listado, sus filtros y navegación en sus componentes propietarios, sin duplicar lógica en el dashboard ([S10.1](../s10.1-listado-enlaces/spec.md), [S10.2](../s10.2-parametros-listado-enlaces/spec.md), [S10.3](../s10.3-conservar-parametros-listado-enlaces/spec.md)).
- Reutilizar el componente y clases de presentación existentes, sin introducir variantes visuales ni estilos duplicados.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Mantener la conexión en el formulario Web compartido y su configuración en DashboardModel, sin búsqueda propia en el dashboard. | confirmado | [AC-DASH-01](../../../plans/mvp0-acceptance-criteria.md#ac-dash-01-dashboard), [«Rutas privadas»](../../../context/specifications.md#rutas-privadas) | Preserva la separación entre navegación desde el dashboard y listado destino. |
| El test actual puede no comprobar el envío de la consulta ni sus resultados en el listado, pese a inspeccionar `action` y `q`. | propuesta | [WebStartupTests.cs](../../../tests/Linkubator.Tests/Integration/WebStartupTests.cs); inspección del test no constituye evidencia de ejecución. | Confirmar cobertura durante T01 y añadir una prueba focalizada solo si la brecha persiste. |
| Evitar duplicar pruebas de reglas de búsqueda o parámetros que pertenecen a S10.1–S10.3/S10.5. | confirmado | [mvp0-plan.md → «S10: Listado y búsqueda de enlaces (L)»](../../../plans/mvp0-plan.md#s10-listado-y-búsqueda-de-enlaces-l) | Mantener el alcance de S10.4 cerrado y validable sin trabajos posteriores. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA03 | Automatizada, integración Web: inspeccionar HTML del dashboard para comprobar destino, método GET, campo `q`, etiqueta accesible y ausencia de campos no propios. | Factory Web de tests y acceso al dashboard identificado. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~WebStartupTests.AppDashboard_RendersSearchAndDatabaseBackedEmptyState --nologo` | El formulario renderizado envía `q` a `/app/enlaces` por GET y conserva el propósito accesible; el dashboard no renderiza resultados de búsqueda ni estado de página. | Nombre del test/caso, aserciones observadas, resultado, fecha y versión del SDK. |
| V02 | CA01, CA02 | Automatizada, integración HTTP focalizada: enviar una consulta desde el formulario del dashboard y observar la URL/respuesta del listado; si el test actual no discrimina el comportamiento, ampliar el test con datos propios y ajenos para comprobar el destino sin duplicar toda la matriz de búsqueda. | Host Web de integración; fixture aislado si se requiere observar resultados diferenciados. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~WebStartupTests.AppDashboard_RendersSearchAndDatabaseBackedEmptyState --nologo` (ajustar filtro al nombre del test específico si se añade). | La navegación llega al listado con la consulta enviada y la respuesta del listado usa sus reglas existentes y solo datos del usuario autenticado. | URL de destino y/o contenido observado, test ejecutado, salida de ejecución, fecha/SDK y límites de la cobertura. |
| V03 | CA01–CA03 | Revisión técnica/documental del cambio, su diff y la trazabilidad. | Cambios focalizados implementados; validaciones V01 y V02 ejecutadas. | `git diff --check`; revisar cambios en modelo/vista y tests respecto a esta spec. | Diff limpio, cambios limitados al puente dashboard → listado y criterios ligados a evidencia propia; no se atribuye cobertura transversal S10.5 ni revisión manual S10.6. | Resultado del comando, archivos revisados, fecha y limitaciones anotadas. |

La comprobación de renderizado aislada no demuestra por sí sola que una consulta enviada se procese ni que el listado aísle resultados; V02 debe hacer observable la navegación y el resultado. Estas validaciones no sustituyen la suite general de S10.5 ni la revisión manual de S10.6.

## Orden de ejecución y cierre

- [tasks.md](tasks.md) fue creado tras aprobar este plan; las tareas fueron aprobadas por DLG el 27-X-2026 («Vamos»).
- La implementación técnica y su validación focalizada están completadas; la evidencia para CA01–CA03 y la aceptación humana constan en [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre). DLG aceptó el resultado el 27-X-2026 («Completado»).
- El trabajo se cerró con evidencia satisfactoria para CA01–CA03 y tareas propias completadas; no dependió de S10.5 ni S10.6.
- Ante un cambio de regla, detener la tarea afectada y tramitar su aprobación en la fuente propietaria antes de continuar.
