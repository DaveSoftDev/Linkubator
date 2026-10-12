# Plan técnico: S3.2 — Invariantes de propiedad

## Estado y aprobación del plan

- Estado: aprobado por DLG el 12-X-2026 («Listo»).
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 12-X-2026 («VAlidado»).
	- Plan: aprobado por DLG el 12-X-2026 («Listo»).

## Diseño de implementación

### Componentes y contratos

Todo el código permanece en `Linkubator.Domain`, sin referencias a otros proyectos ni a paquetes nuevos ([architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura)).

| Componente | Ubicación propuesta | Responsabilidad | Criterios |
| --- | --- | --- | --- |
| `Link` | `src/Linkubator.Domain/Entities/Link.cs` | Crear un enlace con el `UserId` del usuario identificado y una `Collection` materializada, rechazando propietarios distintos; moverlo a una colección destino solo si su `UserId` coincide con el del enlace. Validar antes de cambiar estado. | CA01–CA04 |
| Tests de `Link` | `tests/Linkubator.Tests/Domain/Entities/Link/LinkTests.cs` | Comprobar creación y movimiento con propietarios coincidentes y distintos, y que un rechazo al mover conserva la colección anterior y el `UserId`. | CA01–CA04 |

La colección entregada debe tener un `Id` positivo, como exige el contrato actual de creación de `Link`. La creación recibe el `UserId` del usuario identificado y la entidad `Collection`, compara sus propietarios y toma de la colección su `Id`; así no recibe un `CollectionId` independiente que pueda señalar otra colección. La firma actual de `Link.Create` se adapta conforme a esta propuesta; el método de movimiento recibe también la colección destino.

La comprobación de que la petición opera sobre el usuario identificado no pertenece a este contrato de entidad: Application obtiene la colección autorizada y S5 mantiene la garantía de persistencia mediante su clave foránea compuesta ([architecture.md → «Persistencia»](../../../context/architecture.md#persistencia)).

### Flujos internos

1. **Creación.** Recibir el `UserId` del usuario identificado y la colección, comprobar que el `Id` de esta es positivo y que ambos `UserId` coinciden, aplicar las validaciones actuales de los campos del enlace y construirlo con el `UserId` recibido y el `CollectionId` de la colección. Ante un error, no devolver una entidad.
2. **Movimiento.** Comprobar el `Id` de la colección destino y comparar su `UserId` con el del enlace antes de asignar `CollectionId`. Si la comprobación falla, propagar la excepción de dominio sin cambiar el enlace.
3. **Errores.** Reutilizar `NotValidInputException` para el rechazo de una relación con propietario distinto, sujeto a aprobación del plan. No capturar ni registrar excepciones en Domain.

### Persistencia y dependencias

- No se añade persistencia, transacciones, integraciones ni paquetes.
- El plan no modifica el esquema ni las restricciones de base de datos. S5 es responsable de la clave foránea compuesta que verifica la propiedad durante la escritura ([architecture.md → «Persistencia»](../../../context/architecture.md#persistencia)).
- Application es responsable de resolver el usuario identificado y obtener la colección que corresponda antes de invocar el dominio; esto no autoriza confiar en IDs recibidos del cliente ([architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura)).
- S3.4 conserva la responsabilidad de escribir la cobertura general de invariantes; los tests de este trabajo solo comprueban sus criterios de aceptación.

### Garantías técnicas

- Cada enlace creado o movido mediante la API de Domain mantiene el mismo `UserId` que su colección ([domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos)).
- Una colección de otro usuario no puede ser la colección de destino del enlace.
- Un movimiento rechazado no cambia `CollectionId` ni `UserId`.
- Domain continúa sin dependencias de Application, Infrastructure ni Web ([architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura)).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| D01. Cambiar la creación de `Link` para recibir el `UserId` identificado y una `Collection` materializada, compararlos y derivar de ella `CollectionId`; añadir el movimiento mediante una colección destino. | Propuesta | [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación); [spec S3.1 → «Decisiones técnicas y riesgos»](../s3.1-entidades-user-collection-link/plan.md#decisiones-técnicas-y-riesgos), R02 | Cambia la firma de creación acordada en S3.1; requiere aprobación de este plan antes de implementar. |
| D02. Rechazar una colección destino de otro usuario con `NotValidInputException`. | Propuesta | Excepciones existentes en Domain y CA02/CA04 | Evita añadir un tipo de error sin necesidad indicada por las fuentes; confirmar al aprobar el plan. |
| R01. Las colecciones recién creadas tienen `Id = 0`; el enlace requiere una colección materializada con `Id` positivo. | Confirmado | [spec S3.1 → «Decisiones técnicas y riesgos»](../s3.1-entidades-user-collection-link/plan.md#decisiones-técnicas-y-riesgos), D01; [CollectionTests.cs](../../../tests/Linkubator.Tests/Domain/Entities/Collection/CollectionTests.cs) | Mantener la validación positiva actual. Los tests deben preparar una colección materializada con un ID, sin cambiar el comportamiento de creación de `Collection`. |
| R02. Recibir una instancia de `Collection` no demuestra por sí solo que el llamador esté autorizado a usarla. | Confirmado | [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura) y [«Persistencia»](../../../context/architecture.md#persistencia) | La autorización corresponde a Application y la garantía final al esquema de S5; no ampliar Domain para simular esas capas. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | — | Automatizada: línea base | SDK .NET 10 y dependencias restauradas | `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Suite existente verde antes de los cambios | Comando, SDK, conteo y código de salida. |
| V02 | CA01, CA02 | Automatizada: creación de `Link` | V01 | `dotnet test Linkubator.sln --no-restore --filter "FullyQualifiedName~LinkTests" --verbosity minimal` con casos de propietarios coincidentes y distintos | La creación válida conserva ambos propietarios y usa el ID de la colección; la distinta propiedad se rechaza sin producir un enlace inconsistente | Nombres de tests, resultado y código de salida. |
| V03 | CA03, CA04 | Automatizada: movimiento de `Link` | V02 | El mismo filtro de V02 con casos de movimiento a colección propia y ajena | El movimiento válido conserva el `UserId` y cambia la colección; el inválido lanza el error esperado y deja el estado previo intacto | Nombres de tests, valores antes y después, resultado y código de salida. |
| V04 | CA05 | Automatizada integrada y build | V02–V03 | `dotnet test Linkubator.sln --no-restore --verbosity minimal` y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal` | Suite completa, incluidos tests de arquitectura, verde; build sin advertencias | Comandos, SDK, código de salida y resumen. |
| V05 | CA01–CA05 | Revisión técnica | V04 | Revisar `git diff` y trazabilidad criterio → test | Los cambios se limitan a Domain y sus tests; todas las rutas de creación y movimiento conservan la propiedad | Resumen del diff y matriz de cobertura. |

V02 y V03 ejercitan los casos negativos con entidades válidas de usuarios distintos; no se altera el código para simular una infracción. Estas pruebas verifican la invariante de Domain, no la autorización de una petición ni la clave foránea de SQLite, que corresponden a Application y S5.

## Orden de ejecución y cierre

- Tareas: crear `tasks.md` tras aprobar este plan; contendrá dependencias, paralelismo justificado y checks inmediatos.
- Ejecutar V01 antes de modificar Domain; después implementar y probar creación/movimiento, y finalizar con V04 y V05.
- Criterios con evidencia satisfactoria, sin bloqueos y aceptación humana registrada en las tareas.
- Ante un cambio de regla, detener la tarea afectada y tramitar su aprobación en la fuente propietaria antes de continuar.