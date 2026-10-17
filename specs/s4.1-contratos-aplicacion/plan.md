# Plan técnico: S4.1 — Result, errores, DTOs y validadores de aplicación

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 15-X-2026 («Aprobado»).
  - Plan: aprobado por DLG el 15-X-2026 («Comprendido»).

## Diseño de implementación

### Componentes y contratos

- **Resultado.** Crear en Application un tipo `Result` y su variante genérica para resultados con valor. El resultado expone un estado inequívoco de éxito o fallo y, en el fallo, una colección de errores tipados; no usa excepciones como flujo ordinario ni añade textos de presentación.
- **Errores de aplicación.** Introducir un error inmutable con código estable, categoría y, cuando aplique, nombre del campo. Las categorías distinguen validación, regla de dominio y recurso no accesible. Los códigos se definen en Application y no sustituyen los códigos ni las excepciones de Domain.
- **DTOs.** Añadir records de entrada y salida en Application para las operaciones MVP0 listadas en [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l): colecciones, enlaces, dashboard y consultas públicas. Los DTOs solo transportan valores requeridos por los casos de uso; no contienen entidades de Domain ni tipos HTTP, Razor, Dapper o SQLite.
- **Validación.** Los validadores pertenecen a Application y trabajan sobre los DTOs de entrada. Aplican las comprobaciones de entrada que les corresponden y producen errores asociados a campos. Cuando una regla o transformación pertenece a Domain, Application invoca el contrato de Domain y traduce su excepción conocida a un error de aplicación; no reproduce la regla.
- **Límites de identidad.** Los DTOs no incluyen `UserId` como valor de identidad proporcionado por el cliente. El origen de la identidad y los contratos para acceder a ella son responsabilidad de S4.2/S4.3. Los datos de ruta pública pueden transportar alias o slug para localizar una vista pública, pero no autorizan lecturas privadas.
- Las firmas y nombres concretos anteriores son propuestas técnicas que requieren aprobación de este plan; no alteran el comportamiento funcional.

### Flujos internos

1. El consumidor construye un DTO de entrada con los valores de la solicitud, sin identidad de usuario autoritativa.
2. El validador de Application revisa la forma de la entrada y devuelve los errores de campo que correspondan.
3. Si la entrada supera esas comprobaciones, el futuro caso de uso podrá aplicar las políticas y entidades de Domain y traducir rechazos conocidos al contrato de error de Application.
4. Un consumidor inspecciona el estado del `Result` y maneja los errores por código/categoría, aplicando la respuesta funcional indicada por su criterio aprobado.
5. S4.1 entrega contratos y validadores; no ejecuta operaciones de datos ni decide los mensajes o códigos HTTP de Web.

### Persistencia y dependencias

- No se añaden paquetes externos, persistencia, transacciones ni llamadas a servicios externos.
- Application mantiene únicamente su referencia existente a Domain, según [architecture.md → «Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos).
- S4.2 es responsable de repositorios, Unit of Work e identidad; Infrastructure y Web no son dependencias nuevas de Application.

### Garantías técnicas

- La estructura de dependencias conserva los límites de [architecture.md → «Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos).
- Los tipos de resultados y errores no transportan secretos, texto de búsqueda ni datos de recursos ajenos, conforme a [AGENTS.md → «Invariantes críticas»](../../AGENTS.md#invariantes-críticas) y [specifications.md → «Registro de eventos»](../../context/specifications.md#registro-de-eventos).
- Los casos privados no pueden sustituir la identidad por campos de los DTOs, y la propiedad permanece verificada conforme a [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos).
- Las reglas funcionales y las respuestas visibles permanecen en sus fuentes propietarias; cada caso de uso las aplicará según los criterios de aceptación de S4.4 aprobados en S4.5.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Definir `Result` y `Result<T>` con errores tipados, en vez de usar excepciones para rechazos esperados | Confirmada por DLG, 15-X-2026 («Comprendido») | [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación), CA02–CA03 | Contrato base aprobado; los tests comprobarán sus invariantes. |
| Error de aplicación con código, categoría y campo opcional, sin mensaje de usuario | Confirmada por DLG, 15-X-2026 («Comprendido») | [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación), CA03, CA06 y CA08 | El mapeo posterior en Web no debe filtrar recursos ajenos ni duplicar mensajes funcionales. |
| Los validadores usan componentes de Domain para transformaciones e invariantes existentes | Confirmada por DLG, 15-X-2026 («Comprendido») | [architecture.md → «Domain»](../../context/architecture.md#domain) y [specifications.md → «Generación del alias»](../../context/specifications.md#generación-del-alias) | Los tests verificarán que no se reproducen reglas propietarias de Domain. |
| Enumerar los DTOs incluidos en S4.1 a partir del catálogo de casos de uso S4 | Confirmada; inventario realizado en T02 | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) y [tasks.md → «Matriz de operaciones MVP0 → DTOs»](tasks.md#matriz-de-operaciones-mvp0--dtos) | La matriz identifica las operaciones cubiertas y sus DTOs sin añadir capacidades ajenas al MVP0. |
| No introducir una biblioteca de validación externa | Confirmada por DLG, 15-X-2026 («Comprendido») | [architecture.md → «Tecnologías»](../../context/architecture.md#tecnologías) | Se mantienen validadores propios en Application sin dependencias externas nuevas. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Revisión técnica de capa | Proyecto Application disponible | Revisar `Linkubator.Application.csproj`, referencias de proyecto y tipos nuevos; ejecutar la prueba existente `ProjectReferenceMatrixTests` | Application depende solo de Domain; no aparecen referencias a Infrastructure o Web | Comando, resultado y archivos revisados. |
| V02 | CA02–CA03 | Automatizada: contratos de resultado y errores | Contratos aprobados e implementados | Tests unitarios de resultados de éxito/error y de consulta de categoría, código y campo; comprobar que éxito no contiene error y fallo no se interpreta como éxito | Estados mutuamente distinguibles y errores consultables sin inspeccionar mensajes | Tests ejecutados, versión del SDK y resumen. |
| V03 | CA04–CA05 | Revisión técnica y automatizada: catálogo DTO | Catálogo de casos de uso de S4 confirmado en el plan | Revisar los DTOs frente al inventario de S4; prueba de reflexión o revisión de firmas para detectar referencias a entidades de persistencia, Web/HTTP y `UserId` cliente | Todos los casos previstos tienen contratos adecuados; no se filtran dependencias ni autoridad de usuario | Matriz caso de uso → DTO y resultado de la prueba/revisión. |
| V04 | CA06 | Automatizada: validadores y traducción de Domain | Políticas de Domain disponibles | Tests de validadores para valores válidos e inválidos de cada DTO; tests de traducción de errores de Domain sin duplicar políticas | Los errores apuntan al campo correspondiente; transformaciones e invariantes siguen procediendo de Domain | Pruebas ejecutadas y matriz campo → regla propietaria → validador. |
| V05 | CA07 | Automatizada: rechazo de errores funcionales | Contratos de errores y reglas Domain disponibles | Tests de errores de dominio traducidos al tipo/código de aplicación y de que el resultado sea fallido | Un rechazo conocido no se expresa como éxito ni altera el contrato de error | Tests ejecutados y casos cubiertos; no atribuir aquí atomicidad a una operación aún no implementada. |
| V06 | CA08 | Revisión técnica y automatizada: minimización de datos | Contrato de error aprobado | Revisar propiedades y construir errores representativos con búsquedas, tokens y recursos privados; comprobar que tales valores no son requeridos ni serializados por los contratos | El error no contiene esos datos ni permite obtenerlos desde el DTO de error | Revisión y test ejecutado. |
| V07 | CA01–CA08 | Integrada: build y suite | SDK y dependencias restauradas | `dotnet build Linkubator.sln --no-restore --verbosity minimal` y `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Compilación y tests correctos; registrar advertencias sin inferir aceptación | Comandos, SDK, códigos de salida y resúmenes. |

V02–V06 no demuestran la corrección de los casos de uso ni la propiedad/atomicidad de operaciones reales: eso se comprobará con los trabajos S4.6–S4.9 y las etapas posteriores. La prueba de V03 debe ser sensible a una referencia no permitida; se verificará con una comprobación negativa controlada en copia temporal o mediante el test arquitectónico existente, sin dejar cambios temporales en el repositorio.

## Orden de ejecución y cierre

- `tasks.md` se creó después de la aprobación de este plan y fue aprobado por DLG el 15-X-2026 («Vamos»); la implementación y validación de S4.1 están completadas según las tareas.
- DLG aceptó el resultado de S4.1 el 15-X-2026 al solicitar «Completada». Esa aceptación no aprueba criterios de casos de uso ni abre por sí sola la puerta S4.5.
- Si la definición de un DTO o validador requiere cambiar una regla funcional, detener la tarea afectada y tramitar el cambio en su fuente propietaria antes de continuar.
