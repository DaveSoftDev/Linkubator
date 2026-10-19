# Plan técnico: S3.3 — Transiciones público y privado

## Estado y aprobación del plan

- Estado: aprobado por DLG el 12-X-2026 («Vamos»).
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 12-X-2026 («Adelante»).
	- Plan: DLG, 12-X-2026 («Vamos»).

## Diseño de implementación

### Componentes y contratos

Todo el código permanece en `Linkubator.Domain` y sus tests, sin referencias a otros proyectos ni a paquetes nuevos ([architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura)).

| Componente | Ubicación propuesta | Responsabilidad | Criterios |
| --- | --- | --- | --- |
| `Collection` | `src/Linkubator.Domain/Entities/Collection.cs` | Aceptar el estado solicitado al crearla y exponer una operación de cambio de privacidad; al pasar a privada, coordinar que los enlaces recibidos también queden privados. | CA01, CA04, CA05 |
| `Link` | `src/Linkubator.Domain/Entities/Link.cs` | Crear a partir de una colección materializada, aceptar estado público solo si la colección está pública, permitir hacer público o privado con comprobación de colección y forzar privacidad tras movimiento. | CA02, CA03, CA06, CA07 |
| Tests de `Collection` y `Link` | `tests/Linkubator.Tests/Domain/Entities/CollectionTests.cs` y `tests/Linkubator.Tests/Domain/Entities/LinkTests.cs` | Comprobar las transiciones, precondiciones, rechazos y ausencia de mutación parcial. | CA01–CA08 |

El diseño aprobado conserva el valor predeterminado privado de las entidades y añade `isPublic = false` a `Collection.Create` y a la creación de `Link`. `Link.Create` recibe una `Collection` materializada para comprobar propiedad y privacidad; se retiró la ruta que recibía solo `collectionId`. La colección destino sigue siendo responsable de la verificación de propiedad introducida en S3.2.

Las transiciones implementadas usan `Collection.ChangePrivacy(bool isPublic, IEnumerable<Link> links)`, que valida de antemano los enlaces asociados por `CollectionId` y `UserId`, y al privatizar los enlaces los hace privados antes de actualizar la colección. Hacer pública la colección no cambia la privacidad de los enlaces. `Link.ChangePrivacy(bool isPublic, Collection collection)` verifica la relación y la privacidad de la colección antes de permitir el estado público; cambiar a privado no requiere una colección pública. `Link.MoveToCollection` conserva la comprobación de propiedad de S3.2 y actualiza `CollectionId` y `IsPublic` después de validar el destino.

Las firmas anteriores corresponden al diseño aprobado e implementado. La operación de colección necesita los enlaces relacionados materializados: Domain no consulta repositorios ni conoce persistencia.

### Flujos internos

1. **Creación de colección.** Validar los datos actuales, generar el slug y asignar el estado solicitado; si no se proporciona, conservar el estado privado por defecto.
2. **Creación de enlace.** Recibir la colección materializada, validar el ID y la propiedad conforme al contrato de S3.2, y rechazar antes de construir la entidad si se solicita estado público para una colección privada. Con colección pública, aceptar o no la publicación solicitada; si no se solicita, crear privado.
3. **Cambio de privacidad de colección.** Comprobar previamente la relación y propiedad de todos los enlaces proporcionados. Si se solicita privacidad, actualizar todos los enlaces a privados y luego la colección; si se solicita publicidad, cambiar solo la colección. Un rechazo previo no muta colección ni enlaces.
4. **Cambio de privacidad de enlace.** Comprobar que la colección entregada corresponda al `UserId` y `CollectionId` del enlace. Si se solicita publicidad, rechazar cuando la colección sea privada; si se solicita privacidad, actualizar únicamente el enlace.
5. **Movimiento.** Validar la colección destino y la propiedad primero; después actualizar `CollectionId` y dejar el enlace privado, sin mutaciones parciales en caso de rechazo.
6. **Errores.** Reutilizar las excepciones de dominio existentes para entradas inválidas, a menos que la revisión del plan justifique y apruebe otro contrato. No capturar ni registrar excepciones en Domain.

### Persistencia y dependencias

- No se añade persistencia, consultas, transacciones ni paquetes. Domain recibe las entidades necesarias en sus operaciones.
- El cambio de privacidad de colección y de sus enlaces se ejecutará posteriormente dentro de la operación atómica indicada por [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia); S4/S5 resuelven la lectura y escritura de enlaces relacionados y su coordinación transaccional.
- Este plan no implementa persistencia ni la transacción. La operación de Domain debe ser predecible sobre las entidades recibidas y no presentarse como garantía de atomicidad de base de datos.
- Application/Web son responsables de obtener la colección y los enlaces del usuario identificado; recibir entidades en Domain no constituye autorización para actuar sobre datos ajenos ([architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura)).
- S3.4 conserva la responsabilidad de los tests generales de invariantes; los tests aquí comprueban los criterios de este trabajo.

### Garantías técnicas

- Los constructores y métodos públicos de Domain no dejan crear un enlace público en una colección privada ni hacer público un enlace mientras su colección sea privada ([domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado)).
- Hacer privada una colección privatiza sus enlaces en el conjunto de entidades recibido; hacerla pública no publica enlaces.
- Mover un enlace lo deja privado después de validar la propiedad destino según [spec S3.2 → «Criterios de aceptación»](../s3.2-invariantes-propiedad/spec.md#criterios-de-aceptación).
- Una precondición que falle no deja cambios parciales en las entidades afectadas.
- Domain continúa independiente de Application, Infrastructure, Web y frameworks ([architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura)).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| D01. Crear/mover/cambiar visibilidad de `Link` con una `Collection` materializada para poder comprobar la privacidad y conservar la validación de propiedad de S3.2. | Confirmada e implementada | Aprobación del plan por DLG el 12-X-2026 («Vamos»); [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación) | Se retiró el overload que aceptaba solo `collectionId`; las pruebas usan una colección materializada. |
| D02. Coordinar en Domain la privacidad de una colección y de los enlaces recibidos mediante una operación conjunta, sin cargar consultas ni persistencia en Domain. | Confirmada e implementada | Aprobación del plan por DLG el 12-X-2026 («Vamos»); CA04–CA05 | La operación valida el conjunto antes de mutar y tiene cobertura de lote inválido sin mutación parcial. |
| D03. Conservar privado como valor por defecto y aceptar un parámetro explícito de privacidad en la creación de las entidades. | Confirmada e implementada | Aprobación del plan por DLG el 12-X-2026 («Vamos»); CA01–CA03 | Estado privado por defecto y parámetro `isPublic` comprobados por tests. |
| R01. Domain no puede conocer ni persistir automáticamente todos los enlaces de una colección. | Confirmado | [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura) y [«Persistencia»](../../../context/architecture.md#persistencia) | La operación de dominio recibirá enlaces cargados por Application; su lectura y escritura atómica corresponde a S4/S5. |
| R02. Un contrato basado solo en `CollectionId` no contiene información suficiente para determinar si la colección es pública. | Confirmado | Contrato anterior de `Link.Create` y [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado) | La ruta basada solo en `CollectionId` se retiró; toda creación recibe una colección materializada. |
| R03. Una operación de Domain con varias instancias puede dejar estado en memoria parcialmente cambiado si valida una por una y una validación posterior falla. | Riesgo mitigado | CA05; validación y test de lote inconsistente en [tasks.md](tasks.md) | Se valida el lote antes de mutar; el test confirma que la colección y los enlaces permanecen intactos si una relación es inválida. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01–CA03 | Automatizada: creación de entidades | SDK .NET 10 y suite actual restaurada | Tests enfocados de `CollectionTests` y `LinkTests` para creación privada por defecto, colección pública vacía, enlace público en colección pública y rechazo al pedir publicación en colección privada | Estados correctos; la creación rechazada no produce entidad | Nombres de tests, comando, SDK, resultado y código de salida. |
| V02 | CA04–CA06 | Automatizada: cambios de privacidad | V01 | Tests enfocados de cambio público/privado de colección y enlace; incluir enlaces en ambos estados y lote inconsistente | Hacer pública la colección conserva cada enlace; hacerla privada privatiza todos; enlace no publicable en colección privada es rechazado; ninguna validación tardía produce mutación parcial | Estados antes/después, nombres de tests y resultado. |
| V03 | CA07 | Automatizada: movimiento | V01 | Tests enfocados de `MoveToCollection` con origen público/privado, destino público/privado y destino de otro usuario | Todo movimiento válido deja enlace privado; destino inválido se rechaza y deja `CollectionId` y privacidad anteriores intactos | Matriz de estados probada, nombres de tests y resultado. |
| V04 | CA08 | Automatizada integrada y build | V01–V03 | `dotnet test Linkubator.sln --no-restore --verbosity minimal` y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal` | Suite completa, incluidos tests de arquitectura, verde; build sin advertencias | Comandos, SDK, código de salida y resumen. |
| V05 | CA01–CA08 | Revisión técnica y prueba negativa | V04 | Revisar superficie pública de `Collection` y `Link`, búsqueda de asignaciones a `IsPublic` y `CollectionId`, y confirmar que los tests fallan ante una ruta pública inválida sin modificar el código productivo | No queda un método público que omita las precondiciones; los tests sensibles detectan la infracción; Domain conserva sus límites de dependencia | Rutas revisadas, resultados de búsqueda y test negativo revertido/no persistido, más resumen del diff. |

V01–V03 no demuestran autorización de peticiones, estado persistido ni atomicidad de base de datos; esas comprobaciones pertenecen a Application y S5. La prueba negativa de V05 debe comprobar la sensibilidad de tests existentes mediante el mecanismo de mutación de test apropiado o una expectativa deliberadamente incumplida que se revierta inmediatamente; no debe dejar cambios productivos ni una suite alterada.

## Orden de ejecución y cierre

- Tareas: crear `tasks.md` después de aprobar este plan; contendrá dependencias, paralelismo justificado y checks inmediatos.
- Ejecutar V01 antes de modificar Domain; implementar y comprobar las transiciones de creación y cambio de estado, luego movimiento y suite integrada V04–V05.
- Criterios con evidencia satisfactoria, sin bloqueos y aceptación humana registrada en las tareas.
- Ante un cambio de regla, detener la tarea afectada y tramitar su aprobación en la fuente propietaria antes de continuar.
