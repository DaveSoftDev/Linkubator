# Plan técnico: S8.2 — Operaciones de colecciones

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 25-X-2026 («Siguiente»).
  - Plan: DLG, 25-X-2026 («Siguiente»).
- Implementación aceptada por DLG el 25-X-2026 («Completado»); cierre técnico completado el 26-X-2026 tras satisfacer CA02, según [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre).

## Diseño de implementación

### Componentes y contratos

- Crear páginas Razor privadas para las rutas canónicas de alta, edición y confirmación/eliminación de colecciones especificadas en [spec.md → «Fuentes»](spec.md#fuentes). Cada `PageModel` será delgada y consumirá `ICollectionService`; no consultará repositorios ni incorporará reglas de dominio.
- Usar `GetCollection`, `CreateCollection`, `UpdateCollection` y `DeleteCollection` desde Web. `UpdateCollection` recibe nombre, descripción y visibilidad como una edición compuesta; Application verifica propiedad y actualiza colección/enlaces dentro de su transacción. Se mantienen los contratos legacy de renombrado y visibilidad para compatibilidad de consumidores existentes, pero Web no presenta rutas ni acciones independientes para ellos, conforme a [decisions.md → «Producto y alcance»](../../../context/decisions.md#producto-y-alcance).
- Los formularios Web transportarán únicamente datos funcionales y el identificador de recurso requerido por la ruta. El propietario procede de la identidad de servidor a través de Application. El estado destino de visibilidad es explícito y la capa Web preservará la distinción entre petición válida e inválida que exigen las fuentes.
- Reutilizar los modelos y parciales de presentación existentes —resumen de errores, aviso, alerta de recurso no accesible y scripts de validación— cuando su contrato encaje. No introducir una segunda implementación de mensajes comunes ni copiar textos funcionales fuera de sus fuentes.
- Las rutas y el flujo HTTP se toman de [specifications.md → «Rutas privadas»](../../../context/specifications.md#rutas-privadas); las páginas implementadas y sus modelos son `NewModel`, `EditModel` y `DeleteModel` bajo `Pages/App/Collections`.

### Flujos internos

1. GET de creación muestra el formulario vacío; POST valida la entrada y llama a Application. Éxito redirige al listado canónico; el fallo de validación devuelve el formulario con sus valores y errores disponibles.
2. GET de edición valida el identificador en Web antes de invocar Application, carga mediante `GetCollection` con ámbito del usuario identificado y presenta el formulario.
3. POST de edición recibe conjuntamente nombre, descripción y un único estado destino válido; delega en el caso de uso unificado de edición. Application actualiza colección y, cuando corresponda, privacidad de sus enlaces dentro de la transacción existente; éxito redirige al listado, mientras validación/regla de dominio conservan los datos persistidos y devuelven el formulario.
4. GET de eliminación muestra la confirmación sin invocar eliminación. POST confirmado delega en `DeleteCollection`; éxito redirige al listado, mientras errores controlados se presentan en el flujo conforme a las rutas privadas.
5. Las respuestas de identificador inválido, recurso válido no accesible, métodos no permitidos y antiforgery se mantienen en sus fronteras Web y no exponen detalles del recurso. Las pruebas de S8.4 y S8.5 y la evidencia directa de CA02 registrada en [tasks.md → «Evidencia por criterio»](tasks.md#evidencia-por-criterio) verifican estos comportamientos.

### Persistencia y dependencias

- Application ya dispone de los casos de uso y realiza la comprobación de propiedad, las validaciones de dominio y las transacciones necesarias. La capa Web solo prepara las solicitudes y traduce sus resultados.
- La operación de cambio a privado trata colección y enlaces dentro de una operación de Application. Las pruebas de persistencia comprueban el resultado y el rollback ante un fallo de actualización de enlace; no se introduce SQL, esquema ni transacción en Web.
- La operación de borrado está sujeta a las invariantes de colección y a sus resultados de dominio. Ningún GET ejecuta la operación de escritura.
- Prerrequisito disponible: listado implementado por S8.1. S8.4–S8.7 aportan evidencia posterior para criterios relacionados, sin ser dependencias formales de S8.2. CA02 se completó con pruebas directas de actualización compuesta y atomicidad; sus resultados constan en [tasks.md → «Evidencia por criterio»](tasks.md#evidencia-por-criterio).

### Garantías técnicas

- Mantener la dirección Web → Application → Domain y evitar referencias Web directas a repositorios.
- No aceptar un `UserId` del formulario, de la query ni de la ruta como autoridad; verificar aislamiento con identidades/datos de dos usuarios y una comprobación negativa que envíe un identificador ajeno/manipulado.
- Mantener las escrituras como POST con antiforgery y asegurar que GET de confirmación no cambia el estado persistido.
- Preservar los resultados y errores de Application sin filtrar existencia o propiedad en casos válidos no accesibles; mostrar los errores de dominio conforme a sus categorías y códigos ya definidos.
- Codificar los valores mediante Razor y conservar la semántica accesible de etiquetas, encabezados, enlaces, botones, estados y errores existentes. La revisión visual/manual detallada sigue siendo un trabajo vecino, no condición de cierre de esta spec; sí se debe validar el HTML entregado dentro de S8.2.
- No extender transacciones a la presentación ni a la espera de usuario; el comportamiento transaccional de los casos de uso existentes es el que debe validarse.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Consumir los casos de uso existentes desde páginas Razor privadas, sin lógica de persistencia en Web. | confirmado | [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura), contratos actuales de `ICollectionService`. | Aísla responsabilidades; confirmar antes de implementar que los resultados y validadores actuales cubren las rutas. |
| Integrar nombre, descripción y visibilidad en un único formulario Web y usar la edición integral de Application; no exponer rutas Web separadas de visibilidad/renombrado. | confirmado | [decisions.md → «Producto y alcance»](../../../context/decisions.md#producto-y-alcance), decisión de DLG del 25-X-2026; [specifications.md → «Rutas privadas»](../../../context/specifications.md#rutas-privadas). | Application conserva sus contratos legacy para consumidores existentes y ofrece además `UpdateCollection`; la ruta única de edición envía los tres valores conjuntamente. |
| Reutilizar los componentes de presentación compartidos si encajan y mantener la presentación de errores específica del formulario en sus límites de tarea. | confirmado | [requirements.md → «Accesibilidad»](../../../context/requirements.md#accesibilidad) y componentes Web existentes. | Se usan los parciales comunes de error/alerta y el layout privado; la presentación detallada de validación permanece en el alcance de S8.3. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA02 | Automatizada; tests de Application para crear y actualizar colección con campos compuestos, colisión de nombre, propietario, privacidad de enlaces y restricciones del dominio. | Tests existentes de Application y persistencia. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter CollectionServiceTests --nologo`. | Los casos positivos y negativos existentes pasan. | Resultado real: 62/62 tests `CollectionServiceTests` pasaron el 25-X-2026, pero la inspección posterior confirmó que no hay casos que invoquen `UpdateCollection`; por ello el resultado es evidencia para los casos existentes del servicio y no prueba el nuevo contrato integral. La suite completa posterior reportó un fallo en la expectativa de logging legacy, que permanece sin modificar; debe repetirse tras investigar. No es cobertura HTTP. || V02 | CA03 | Build y revisión del flujo de eliminación de Web; GET solo consulta y POST delega en `DeleteCollection`. | Página de confirmación y configuración Razor. | Build de solución; revisión del código de `DeleteModel` y `DeleteCollection`. | GET no invoca la operación de borrado; POST es el único handler que la invoca y respeta los errores de dominio de Application. | Build completo sin warnings/errores y comprobación de código confirmada. Sin test HTTP directo de eliminación. |
| V03 | CA04 | Revisión técnica de identificador por ruta, ámbito de usuario y antiforgery en los formularios de escritura. | PageModels y configuración Web existente. | Revisión de los PageModels; `dotnet build Linkubator.sln --nologo`. | El ID se obtiene de la ruta/POST correspondiente, la identidad la aporta el servidor y formularios POST conservan antiforgery de Razor. | Revisión de `EditModel`, `DeleteModel`, `NewModel` y markup; build completo limpio. Sin test adversarial HTTP que pruebe alteraciones de identidad/ID. |
| V04 | CA05 | Revisión del HTML y navegación del listado con los enlaces de edición y confirmación; build de Razor. | Página de listado y páginas enlazadas. | `dotnet build Linkubator.sln --nologo`; inspección de `Collections.cshtml`. | Navegación de acciones enlazada a las rutas canónicas; tabla mantiene nombres, descripción, privacidad, enlaces y acciones accesibles con etiquetas. | Código de `Collections.cshtml` inspeccionado; la vista enlaza editar/eliminar por colección y preserva la tabla y paginación. Sin comprobación visual/manual de accesibilidad. La suite completa reportó que no encontró el enlace de paginación que preserva la búsqueda; su causa sigue pendiente de investigación. || V05 | CA01–CA05 | Validación integrada disponible: build y tests focalizados. | Implementación actual. | `dotnet build Linkubator.sln --nologo`; `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter CollectionServiceTests --nologo`. | Build y tests focalizados verdes. | Build exitoso con 0 warnings/0 errors y tests focalizados 62/62 el 25-X-2026. La ejecución final de `git diff --check` y reconciliación de las regresiones encontradas en la suite completa siguen pendientes. |

### Actualización de evidencia al 26-X-2026

Los resultados de la tabla V01–V05 describen las comprobaciones de la implementación inicial. La prueba de `CollectionServiceTests` que invoca `UpdateCollection` solo comprueba el uso de un validador personalizado; no verifica el comportamiento funcional de la edición compuesta. Después, S8.4 aportó pruebas HTTP de identificadores y respuestas no discriminantes (12/12); S8.5 añadió pruebas de operaciones y antiforgery (19/19); S8.7 registró revisión manual de formularios y accesibilidad.

Para completar CA02, se añadieron y ejecutaron pruebas que verifican por HTTP los valores persistidos de nombre, slug, descripción y visibilidad, el cambio a privado de un enlace público, y la conservación del estado al rechazar un nombre duplicado. Una prueba de persistencia SQLite inyecta un fallo durante la actualización del enlace y confirma el rollback de la colección y ambas proyecciones de búsqueda. El filtro combinado de pruebas de colección y rollback pasó 21/21. La validación integrada final pasó build de solución con 0 warnings/0 errores y suite completa `Linkubator.Tests` 1.179/1.179; `git diff --check` también pasó. El cierre técnico está registrado en [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre).

## Orden de ejecución y cierre

- La validación integrada de S8.2 incluye revisión de referencias y de la suite; la aceptación humana no convierte una validación fallida o incompleta en evidencia satisfactoria.
- Los cambios de código y la validación focalizada pertenecen a esta spec; no se declara cobertura HTTP que no se haya ejecutado.
- La aceptación humana se registra separadamente del estado técnico. El cierre técnico de CA01–CA05 se completó el 26-X-2026 con evidencia directa de `UpdateCollection` para campos compuestos, rechazo sin cambios y atomicidad transaccional.
- Si durante el diseño o la implementación aparece una discrepancia entre el contrato existente y las reglas funcionales, detener la tarea afectada y pedir aprobación para modificar el documento propietario antes de cambiar el comportamiento.
