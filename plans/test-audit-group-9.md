# Auditoría individual del grupo 9: base de aplicación, presentación y observabilidad

## Alcance y resultado

**Estado actual:** refuerzos y huecos aprobados aplicados. La selección focalizada pasó **94/94** y la suite global anterior **1.833/1.833**. Los 31 casos nuevos o variantes se revisaron después, se aplicaron los cuatro refuerzos y se resolvió la lectura pública con una sentencia. S12 sigue pendiente; ver [revisión de impacto](test-audit-impact-review.md).

Diagnóstico inicial del 2026-10-30, sin modificar tests ni producción en esa fase: 217 casos de 139 métodos, 208 vigentes, siete a reforzar y dos duplicados demostrados. La clasificación por variante distingue fecha y desempate en el dashboard. El [inventario actual](test-audit-group-9.csv) recoge **222 casos vigentes**.

La suite completa se ejecutó con código actual recompilado y generó el TRX usado para el inventario: **1.827/1.827 superados**, sin omitidos.

```powershell
dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-restore -v minimal
```

Que la suite pase no demuestra que todos los casos detecten una regresión ni sustituye las revisiones humanas. No se han implementado las propuestas siguientes.

## Reconciliación de los nueve grupos

Los nueve CSV se han reconciliado con el TRX actual usando identificadores únicos del runner y asignación exclusiva por responsabilidad. Recuento: grupo 1 **130**, grupo 2 **231**, grupo 3 **69**, grupo 4 **200**, grupo 5 **194**, grupo 6 **651**, grupo 7 **82**, grupo 8 **54** y grupo 9 **222**: total **1.833**.

Los casos cuya identidad anterior coincide conservan su clasificación y metadatos históricos. Se retiraron del inventario actual 17 variantes de métodos de conteo público y una del listado del grupo 2 que ya no existen; doce casos actuales de lectura pública se asignaron al grupo 1 como **no revisados**, sin presumir equivalencia. Además, se registraron como no revisadas seis variantes de presentación del grupo 1, cuatro casos del grupo 3, tres del grupo 4, uno del grupo 5 y cinco del grupo 8. Total: **31 casos sin revisión individual**. Los grupos 7 y 8 conservan evidencia histórica, pero el estado global exige revisar esas altas y el impacto de los refactors.

La coincidencia de nombres o identificadores no demuestra que no haya cambiado el cuerpo de un test o del código que ejercita. Los refactors de lectura pública, servicios y repositorios ya han alterado pruebas y resultados: la suite verde acredita el estado ejecutado, pero no renueva automáticamente la revisión individual de todos los casos afectados. Antes del cierre global hay que revisar las altas y los cambios relevantes desde la revisión de cada grupo; no existe aquí una instantánea histórica completa de todos los archivos por grupo que permita certificar automáticamente esa ausencia de cambios.

## Fuentes y alcance

- [architecture.md → «Arquitectura»](../context/architecture.md#arquitectura): capas, responsabilidades y límites de dependencias.
- [architecture.md → «Persistencia»](../context/architecture.md#persistencia): transacciones y recursos; los tests de SQLite ya auditados se conservan en grupo 8.
- [specifications.md → «Listados»](../context/specifications.md#listados) y [«Páginas privadas»](../context/specifications.md#páginas-privadas): datos y presentación de dashboard.
- [specifications.md → «Registro de eventos»](../context/specifications.md#registro-de-eventos): información observable y tratamiento de datos en logs.
- Las pruebas de presentación no sustituyen revisión visual y accesible manual; login y scraping futuros no se consideran implementados.

## Refuerzos y duplicados

| ID | Prioridad | Hallazgo | Acción propuesta |
|---|---|---|---|
| A1 | Media | `ValidatorsRejectInvalidRequests` mezcla dos validadores en una prueba de resultados y acepta alternativamente dos campos para el enlace. No demuestra cuál validación falló; existen pruebas específicas de los validadores. | Retirar el caso si se confirma equivalencia por escenario o sustituirlo por aserciones concretas en sus grupos propietarios; no atribuirlo a `Result`. |
| A2 | Baja | `SafeLogger_BuildContext_WithAdditionalContext_UsesAllSources` y `SafeLogger_SafeLogState_ExposesCountAndEnumerable` acceden por reflexión a detalles privados. El primero llama una rama que la API pública no expone; el segundo no comprueba todos los elementos enumerados. | Sustituir por garantías observables de `SafeLogger.Log` y del evento capturado, o retirar la rama no utilizada sin inventar un contrato público. |
| A3 | Media | `GetDashboard_WhenUserIsAuthenticated_ReturnsDashboardData` prepara un DTO completo, pero solo comprueba su colección y el nombre. Una pérdida del resto de metadatos pasaría. | Comparar el DTO completo y el usuario solicitado; añadir control de token de cancelación si corresponde. |
| A4 | Media | La variante con fechas distintas de `GetByUser_WhenUserHasMoreThanTwelveLinks_ReturnsTwelveMostRecent` inserta las fechas en el mismo orden que los identificadores. Ordenar exclusivamente por id podría superar el caso. La variante de fechas iguales sí comprueba el desempate. | Sembrar fechas en orden opuesto o permutado a los ids y exigir la secuencia completa y la exclusión del decimotercero. |
| A5 | Media | Los tests de transacción base comprueban Begin y Commit, pero su doble no registra Dispose y la prueba de fallo no fija el error concreto; no acredita la liberación del recurso. | Registrar disposición y orden, comprobar el error retornado y que fallo o excepción no confirman; mantener distinta la prueba de `Rollback` explícito de la extensión. |
| D1 | Baja | `ResolveIdProperty_ThrowsIfPropertyNotFound` no prepara ausencia ni espera excepción: solo verifica que se devuelve una propiedad. Es un subconjunto de `ResolveIdProperty_ReturnsIdPropertyInfo`. | Retirar el duplicado y conservar el test que exige nombre y tipo propietario. |
| D2 | Baja | `CreateSidebar_WhenUserProvided_UsesDevelopmentIdentityData` y `CreateSidebar_WithEmptyCollections_UsesDevelopmentIdentityData` usan la misma preparación y comprueban el mismo nombre y alias; el segundo añade vacío y contador. | Retirar el primero tras conservar la cobertura más fuerte del segundo. |

## Huecos propuestos

| ID | Prioridad | Escenario | Acción propuesta |
|---|---|---|---|
| H1 | Media | Las sobrecargas genérica y no genérica de `ExecuteInTransaction` y la lectura `Execute` con unidad de trabajo no tienen una matriz directa de éxito, fallo, excepción y disposición. | Añadir pruebas con dobles que registren ciclo de vida y resultado; no asumir un rollback explícito donde el contrato es disposición. |
| H2 | Media | `UnitOfWorkFactoryExtensions` no prueba fallo de la acción sin resultado ni cancelación durante la operación y su limpieza. | Verificar propagación de excepción/cancelación y disposición; usar persistencia SQLite cuando se quiera acreditar rollback real y evitar fijar expectativas no documentadas del doble. |
| H3 | Media | Dashboard cubre un usuario ajeno y límite, pero no la combinación de más de doce enlaces ajenos, colección propia privada y pública, y DTO completo con datos opcionales. | Añadir caso discriminante en repositorio y servicio con identidades, privacidad y datos distintos. |
| H4 | Baja | La verificación de dependencias compara proyectos conocidos, pero no detecta automáticamente un nuevo proyecto fuera de la matriz. | Comparar el conjunto real de proyectos de la solución con la matriz o explicitar el alcance del test; no prohibir proyectos nuevos sin decisión de arquitectura. |
| H5 | Baja | El inventario global ha quedado desfasado por refactors posteriores a grupos 1 y 2 y casos nuevos fuera de sus CSV. | Reconciliar los ocho inventarios con el TRX actual, conservar clasificación histórica y documentar bajas/reemplazos sin reclamar una auditoría de casos nuevos no revisados. |

## Estado

Acciones del grupo 9 aplicadas: se retiraron ambos duplicados y la prueba débil de validadores, cubierta por sus pruebas específicas; se sustituyeron las pruebas de logging privadas por la observación de un evento público. Dashboard comprueba DTO completo, orden por fecha independiente del id y aislamiento frente a más de doce enlaces ajenos. Se añadieron pruebas de ciclo de vida de las sobrecargas, excepciones y cancelación con rollback real en SQLite. El test de referencias se renombró para explicitar que cubre proyectos conocidos, sin imponer una prohibición no decidida sobre proyectos nuevos.

Verificación de aquel lote: **94/94** de la selección focalizada, **1/1** del rollback SQLite por cancelación y **1.833/1.833** globales, sin omitidos. La [revisión de impacto](test-audit-impact-review.md) clasificó los 31 casos posteriores y resolvió sus cuatro refuerzos; el último lote pasó **5/5** focalizados y **1.833/1.833** globales. La conclusión histórica no demuestra automáticamente ausencia de cambios en los demás casos; S12 y las comprobaciones humanas permanecen pendientes.
