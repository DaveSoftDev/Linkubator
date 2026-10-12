# Plan técnico: S3.4 — Tests de invariantes del dominio

## Estado y aprobación del plan

- Estado: aprobado por DLG el 12-X-2026 («Adelante»).
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 12-X-2026 («Entendido»).
	- Plan: aprobado por DLG el 12-X-2026 («Adelante»).

## Diseño de implementación

### Componentes y contratos

El trabajo se limita al proyecto de tests existente; no modifica el comportamiento productivo de Domain ni incorpora dependencias externas ([architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura)).

| Componente | Ubicación propuesta | Responsabilidad | Criterios |
| --- | --- | --- | --- |
| Revisión de tests de entidades | `tests/Linkubator.Tests/Domain/Entities/User/UserTests.cs`, `tests/Linkubator.Tests/Domain/Entities/Collection/CollectionTests.cs` y `tests/Linkubator.Tests/Domain/Entities/Link/LinkTests.cs` | Contrastar la cobertura existente con las invariantes de `User`, `Collection` y `Link` aplicables al MVP0 y los criterios aprobados de S3.1; completar solo los casos ausentes. | CA01 |
| Tests de propiedad | `tests/Linkubator.Tests/Domain/Entities/Link/LinkTests.cs` | Comprobar creación y movimiento con propietario coincidente y distinto, y conservación del estado previo al rechazar un movimiento inválido, conforme al criterio aprobado de S3.2. | CA02, CA04 |
| Tests de público y privado | `tests/Linkubator.Tests/Domain/Entities/Collection/CollectionTests.cs` y `tests/Linkubator.Tests/Domain/Entities/Link/LinkTests.cs` | Comprobar las transiciones de creación, privacidad y movimiento ya aprobadas en S3.3, incluidos los rechazos y la ausencia de mutación parcial. | CA03, CA04 |
| Suite y límites de arquitectura | Proyecto `tests/Linkubator.Tests` y tests de arquitectura existentes | Ejecutar las verificaciones integradas y confirmar que no se añadieron dependencias prohibidas a Domain. | CA05 |

Los nombres de archivos son ubicaciones existentes previstas, no una decisión de fragmentar o reescribir tests. La auditoría de CA01 determina qué pruebas ya proporcionan cobertura; se conserva esa evidencia y se evitan duplicados. Los criterios específicos de S3.2 y S3.3 siguen siendo propietarios de sus reglas; este plan comprueba que sus casos tienen tests y cubre solo faltantes.

### Flujos internos

1. Revisar los tests actuales y contrastar cada invariante comprobable en Domain con los criterios aprobados de S3.1–S3.3.
2. Mantener una matriz de trazabilidad criterio → test existente o por añadir, señalando explícitamente las reglas que requieren datos o capas que no pertenecen a Domain.
3. Añadir tests solo para faltantes. Los casos permitidos verifican el resultado; los rechazados verifican la excepción de dominio y, cuando corresponda, que el estado previo se conserva.
4. Ejecutar los filtros de tests de entidades afectados y después la suite completa y el build; revisar el diff y la matriz de cobertura.
5. Si la revisión descubre una contradicción o hace falta cambiar código productivo o una regla, detener el criterio afectado y remitirlo a su documento propietario; no ampliar esta tarea por cuenta propia.

### Persistencia y dependencias

- No se añaden persistencia, transacciones, proyectos, paquetes ni servicios.
- El trabajo depende de las entidades y comportamiento de Domain entregados por S3.1–S3.3 y de la suite `Linkubator.Tests` existente.
- Estas pruebas unitarias no demuestran autorización de solicitudes, consistencia persistida, atomicidad de base de datos ni concurrencia; esas garantías pertenecen a sus capas y trabajos propietarios.

### Garantías técnicas

- La cobertura de los tests se refiere solo a invariantes comprobables en Domain y aplicables al MVP0, según [domain-model.md → «User»](../../context/domain-model.md#user), [«Collection»](../../context/domain-model.md#collection), [«Link»](../../context/domain-model.md#link), [«Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [«Público y privado»](../../context/domain-model.md#público-y-privado).
- Todo caso negativo que compruebe mutación de estado verifica el estado relevante después de la excepción; el nombre del test por sí solo no se considera evidencia.
- Domain conserva su independencia y no se modifica en esta tarea, conforme a [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| D01. Reutilizar los tests existentes de S3.1–S3.3 y añadir únicamente cobertura que falte al contrastarlos con los criterios e invariantes aplicables. | Propuesta | [spec.md → «Alcance»](spec.md#alcance), CA01–CA04 | Evita duplicar pruebas específicas; verificar primero la cobertura actual y documentar cualquier hueco. |
| D02. Mantener los tests en el proyecto existente y usar los archivos de entidad actuales como ubicaciones iniciales. | Propuesta | [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura) y estructura actual de `Linkubator.Tests` | No incorpora frameworks ni dependencias; ajustar ubicación solo si la revisión del código muestra una convención distinta, sin alterar el alcance. |
| R01. Las invariantes que dependen de persistencia, identidad autenticada o coordinación transaccional no son demostrables por tests unitarios de Domain. | Confirmado | [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) | No atribuir esas garantías a los tests de este trabajo; mantenerlas fuera de alcance. |
| R02. Parte de la cobertura requerida puede existir ya como evidencia de S3.1–S3.3. | Confirmado | Tests actuales de `User`, `Collection` y `Link`; [spec.md → «Dependencias y prerrequisitos»](spec.md#dependencias-y-prerrequisitos) | CA01 exige auditar antes de añadir; evitar duplicación sin dejar invariantes aplicables sin cobertura. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Revisión técnica de cobertura | S3.1–S3.3 disponibles; tests existentes | Contrastar las invariantes aplicables con [spec S3.1 → «Criterios de aceptación»](../s3.1-entidades-user-collection-link/spec.md#criterios-de-aceptación) y los tests actuales de entidades | Cada invariante verificable se vincula a uno o más tests; los casos cubiertos se identifican y las reglas de otras capas quedan excluidas | Matriz de cobertura con invariante/criterio, test o ausencia identificada y decisión de completar/no duplicar. |
| V02 | CA02, CA04 | Automatizada: propiedad y rechazo sin mutación | V01 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~LinkTests" --verbosity minimal` | Casos válidos conservan propiedad coherente; colección ajena se rechaza; el movimiento rechazado conserva la colección y el estado del enlace | Tests concretos, resultado y código de salida. |
| V03 | CA03, CA04 | Automatizada: privacidad y transiciones | V01 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~CollectionTests|FullyQualifiedName~LinkTests" --verbosity minimal` | Creación, cambios de privacidad y movimiento cubren los criterios aprobados de S3.3; los rechazos no dejan mutación parcial | Tests concretos y estados relevantes antes/después, resultado y código de salida. |
| V04 | CA05 | Automatizada integrada y build | V02–V03 | `dotnet test Linkubator.sln --no-restore --verbosity minimal` y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal` | Suite completa y build correctos; los tests de arquitectura existentes continúan pasando | Comandos, SDK, resultado, conteos y códigos de salida. |
| V05 | CA01–CA05 | Revisión técnica y prueba negativa | V01–V04 | Revisar diff, referencias de Domain y matriz criterio → test; comprobar que un caso representativo falla si se desactiva temporalmente su guarda o expectativa, restaurando inmediatamente el test y sin dejar cambios productivos | No hay cambios de reglas ni dependencias no permitidas; pruebas negativas detectan la infracción correspondiente y la suite queda restaurada | Resumen del diff, búsqueda/revisión de dependencias, test negativo y su restauración comprobada. |

V01 verifica trazabilidad, no sensibilidad de tests. V02–V04 prueban el comportamiento de Domain y la regresión de la solución, no autorización, persistencia ni atomicidad de base de datos. La prueba negativa V05 debe ser acotada y reversible; no se considera suficiente provocar un fallo de compilación.

## Orden de ejecución y cierre

- Tareas: [tasks.md](tasks.md), aprobadas por DLG el 12-X-2026 («Vamos»); implementación y validación completadas, con evidencia y aceptación en ese documento.
- Ejecutar V01 primero para identificar brechas; luego completar y validar los tests focalizados V02–V03, y terminar con V04–V05.
- Criterios con evidencia satisfactoria, sin bloqueos y aceptación humana registrada en las tareas.
- Ante un cambio de regla, detener la tarea afectada y tramitar su aprobación en la fuente propietaria antes de continuar.
