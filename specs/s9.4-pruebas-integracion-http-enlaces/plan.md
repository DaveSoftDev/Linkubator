# Plan técnico: S9.4 — Pruebas de integración HTTP de enlaces

## Estado y aprobación del plan

- Estado: aprobado por DLG el 26-X-2026 («Aprobar el plan y continuar con la preparación de las tareas»).
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 26-X-2026 («Aprobar la especificación S9.4 y continuar con el plan»).
  - Plan: aprobado por DLG el 26-X-2026 («Aprobar el plan y continuar con la preparación de las tareas»).

## Diseño de implementación

### Componentes y contratos

- Mantener las pruebas en `tests/Linkubator.Tests/Integration`, usando xUnit, `WebApplicationFactory<Program>` y `ConfiguredWebApplicationFactory`, con una base SQLite temporal aislada por test, como en [LinkCreationIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkCreationIntegrationTests.cs) y [LinkManagementIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkManagementIntegrationTests.cs).
- Completar la cobertura HTTP en esas clases existentes, reutilizando sus helpers para obtener antiforgery, preparar identidades y crear enlaces y colecciones. Añadir casos solo donde la cobertura actual no demuestre íntegramente un criterio de [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación).
- Ejecutar las páginas y handlers productivos a través de solicitudes HTTP; no invocar directamente PageModels, servicios ni repositorios para afirmar el comportamiento Web.
- No añadir endpoints, filtros de inicio, paquetes ni cambios en código productivo. La prueba envía datos directamente al formulario HTTP, sin depender de que la interfaz ofrezca o permita esos valores.

### Flujos internos

1. Cada prueba crea su host, cliente autenticado y datos SQLite aislados; los escenarios entre usuarios incluyen recursos propios y ajenos.
2. La prueba envía la solicitud HTTP a la ruta existente de creación, edición, movimiento o eliminación. Los POST protegidos obtienen y envían el token antiforgery según el patrón de las pruebas actuales.
3. La respuesta se comprueba conforme a [specifications.md → «Páginas privadas»](../../context/specifications.md#páginas-privadas) y [«Errores controlados»](../../context/specifications.md#errores-controlados), sin introducir códigos ni mensajes nuevos.
4. Cuando una escritura se rechaza, se consultan los datos persistidos después de la respuesta y se comparan con el estado previo. Los escenarios de control válidos confirman que el fixture puede completar la operación permitida.
5. En peticiones que intentan usar identificadores o identidad enviados por el cliente, las aserciones verifican la propiedad real de los datos y que ningún recurso ajeno se haya expuesto o alterado.

### Persistencia y dependencias

- No se requieren cambios de esquema, migraciones ni SQL de aplicación. Las consultas directas a SQLite se limitan a preparar y verificar el estado de la base aislada de pruebas, como en las clases de integración existentes.
- Reutilizar las fábricas, la identidad de prueba y el almacenamiento SQLite que configuran las pruebas de enlaces; no iniciar procesos Web externos ni compartir estado entre tests.
- Prerrequisitos disponibles: flujos Web de creación, publicación, edición integral, movimiento y eliminación cubiertos por S9.1, S9.2 y S9.3, con implementación y validación aceptadas. S9.5 y S9.6 no son dependencias de esta batería ni de su cierre.

### Garantías técnicas

- Las comprobaciones de propiedad incluyen solicitudes con recursos propios y ajenos, y verifican que los datos de identidad del cliente no sustituyan la identidad autenticada. La garantía propietaria está en [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos).
- Las comprobaciones de privacidad envían directamente las peticiones manipuladas y verifican el estado persistido; no infieren el rechazo únicamente de que la interfaz oculte o deshabilite una opción. La regla propietaria está en [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado).
- Para los rechazos se comparan los campos persistidos del enlace, su colección, las relaciones relevantes y las proyecciones afectadas; un escenario válido de control verifica que la prueba detecta tanto la escritura permitida como la ausencia de cambios no permitidos. Las garantías transaccionales pertenecen a [architecture.md → «Persistencia»](../../context/architecture.md#persistencia).
- Las respuestas para recursos no accesibles se comparan según la regla fuente, sin exponer datos del recurso ni diferenciar los casos que deban ser indistinguibles.
- La evidencia es de integración HTTP dentro de `TestServer`; no acredita transporte TLS real ni sustituye la revisión manual de URLs o de presentación asignada a trabajos vecinos.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Reutilizar xUnit, `WebApplicationFactory` y SQLite temporal ya configurados. | Confirmado por la estructura existente | [Linkubator.Tests.csproj](../../tests/Linkubator.Tests/Linkubator.Tests.csproj), [LinkCreationIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkCreationIntegrationTests.cs) y [LinkManagementIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkManagementIntegrationTests.cs) | Sin dependencias ni infraestructura nueva; aislar la base por test. |
| Limitar los cambios a cobertura de pruebas y documentación de evidencia, sin tocar código productivo. | Confirmado por el alcance aprobado | [spec.md → «Alcance»](spec.md#alcance) y [mvp0-plan.md → «S9: Gestión de enlaces (L)»](../../plans/mvp0-plan.md#s9-gestión-de-enlaces-l) | Si un test descubre una discrepancia de comportamiento, no cambiar la regla ni el código de producto como parte de esta propuesta; registrar y escalar el bloqueo a su fuente o trabajo propietario. |
| Existe cobertura de enlaces en S9.1 y S9.3 que puede solaparse con estos criterios. | Riesgo a comprobar durante la implementación | [LinkCreationIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkCreationIntegrationTests.cs), [LinkManagementIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkManagementIntegrationTests.cs) y [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación) | Reutilizar evidencia solo cuando cubra la solicitud HTTP y las aserciones completas del criterio; añadir únicamente los casos faltantes, sin duplicar pruebas equivalentes. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada; POST HTTP de creación con colección inválida, inexistente o ajena. | `ConfiguredWebApplicationFactory`, identidad autenticada y SQLite temporal con colecciones propias y ajenas. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkCreationIntegrationTests" --nologo` | Se aplica el tratamiento del formulario, no se crea el enlace y no se revela información de la colección ajena. Un caso válido de creación confirma el control positivo. | Casos ejecutados, respuesta y comparación del número/propiedad de enlaces antes y después. |
| V02 | CA02, CA05 | Automatizada; POST HTTP de edición y movimiento hacia colecciones inválidas, inexistentes o ajenas, con datos válidos acompañantes. | Fixture de gestión con enlace propio, destinos propios y colección ajena. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkManagementIntegrationTests" --nologo` | El rechazo conserva el enlace y las asociaciones/proyecciones afectadas; un control válido demuestra que una escritura autorizada sí se persiste. | Solicitud, respuesta, campos del enlace, colección, relaciones relevantes y filas de proyección comparadas antes/después. |
| V03 | CA03 | Automatizada y negativa; POST de creación o edición con datos manipulados de identidad/propiedad y solicitud directa para hacer público un enlace en una colección privada. | Fixtures de creación y gestión con identidad autenticada y recursos de dos usuarios. | Mismos filtros focalizados `LinkCreationIntegrationTests` y `LinkManagementIntegrationTests`. | El servidor usa la identidad autenticada, las operaciones no autorizadas se rechazan y ningún recurso ajeno se publica o altera. Los controles válidos confirman que la solicitud llega al flujo productivo. | Campos manipulados enviados, respuesta, propietario y estado persistidos y recursos ajenos antes/después. |
| V04 | CA04 | Automatizada; GET y POST de rutas privadas con identificadores inválidos, inexistentes y ajenos, para las operaciones aplicables. | Fixture HTTP de gestión con enlaces propios y ajenos; casos inexistentes equivalentes. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkManagementIntegrationTests" --nologo` | Cada respuesta coincide con el tratamiento aprobado; los casos que deben ser indistinguibles no revelan si el enlace existe o pertenece a otro usuario, y los recursos ajenos no cambian. | Rutas, respuestas y contenido comparado; estado persistido de los recursos ajenos antes/después. |
| V05 | CA05 | Automatizada; verificación de ausencia de escrituras parciales ante rechazos por validación, propiedad o privacidad. | Fixture SQLite con enlace y proyección registrados; datos relacionados cuando apliquen. | Filtros focalizados de V01 y V02. | Todos los campos, relaciones y proyecciones afectados conservan su estado anterior tras el rechazo; los casos válidos de control detectan que la operación permitida sí se aplica. | Comparación antes/después de los datos relevantes y resultado del control positivo por escenario. |
| V06 | CA01–CA05 | Regresión integrada de las pruebas HTTP de enlaces. | V01–V05. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkCreationIntegrationTests|FullyQualifiedName~LinkManagementIntegrationTests" --nologo` | Todos los casos focalizados pasan y no dependen del orden de ejecución. No se atribuye a este filtro resultado de la suite completa. | Comando, versión del SDK, total aprobado/fallido/omitido y enlaces a las pruebas que evidencian cada criterio. |

## Orden de ejecución y cierre

- Tras aprobar este plan, crear `tasks.md` desde la plantilla y pedir su aprobación antes de iniciar cambios productivos.
- En las tareas, inventariar primero la cobertura actual frente a cada criterio, completar únicamente las pruebas ausentes y ejecutar los filtros focalizados; registrar evidencia por criterio.
- La prueba automatizada no sustituye la aceptación humana ni la revisión manual asignada a S9.5/S9.6. Cerrar esta spec únicamente cuando todos sus criterios tengan evidencia HTTP satisfactoria, las tareas propias estén completadas y DLG acepte explícitamente el resultado.
