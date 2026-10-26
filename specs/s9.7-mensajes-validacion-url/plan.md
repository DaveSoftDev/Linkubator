# Plan técnico: S9.7 — Mensaje de validación de URL

## Estado y aprobación del plan

- Estado: propuesta.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 26-X-2026 («Siguiente»).
  - Plan: pendiente; registrar responsable, fecha y referencia real cuando se apruebe.

## Diseño de implementación

### Componentes y contratos

- Los formularios privados de creación y edición ya consumen errores de validación con código y campo desde Application. Web es responsable de convertirlos en mensajes presentables.
- La creación resuelve los mensajes de campo en [New.cshtml.cs](../../src/Linkubator.Web/Pages/App/Links/New.cshtml.cs); la edición los aplica en [Edit.cshtml.cs](../../src/Linkubator.Web/Pages/App/Links/Edit.cshtml.cs). La corrección debe presentar el mismo mensaje propietario en el resumen y junto al campo.
- Se centralizará la resolución del mensaje de validación de enlace en la capa de presentación Web para evitar que creación y edición diverjan. El plan no modifica el contrato ni los identificadores estables de Domain o Application.
- La cobertura de extremo a extremo se añadirá a [LinkCreationIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkCreationIntegrationTests.cs) y [LinkManagementIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkManagementIntegrationTests.cs), siguiendo sus fixtures de integración existentes.

### Flujos internos

1. Application devuelve el error de esquema no permitido junto con el campo afectado.
2. Web resuelve el mensaje para la persona usuaria antes de poblar `ModelState` y el resumen; el formulario conserva los datos introducidos y no representa el código técnico.
3. Las pruebas HTTP de integración ejercitan creación y edición, inspeccionan el resumen y el error del campo, y comprueban que las operaciones rechazadas no crean ni modifican datos.

### Persistencia y dependencias

No se modifican persistencia, migraciones, infraestructura ni dependencias. La validación de URL y de imagen, sus contratos y sus códigos de error permanecen en las capas actuales. El cambio se limita a presentación Web y pruebas de integración.

### Garantías técnicas

- El mensaje visible se obtiene de la regla propietaria en [specifications.md → «Errores controlados»](../../context/specifications.md#errores-controlados); no se expone el identificador estable del dominio.
- El mismo texto se presenta en el resumen y junto al control afectado.
- Las entradas inválidas no crean enlaces ni modifican los campos persistidos o la proyección de búsqueda de un enlace editado.
- Los mensajes de errores no cubiertos por esta corrección y el comportamiento de entradas válidas no cambian.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Mantener en Domain y Application sus códigos estables y resolver el texto en Web | Confirmado | [decisions.md → «Tecnología»](../../context/decisions.md#tecnología) y [«Errores controlados»](../../context/specifications.md#errores-controlados) | Evita mezclar identificadores técnicos con mensajes visibles. |
| Compartir la resolución del mensaje entre creación y edición | Propuesta | [spec.md → «Alcance»](spec.md#alcance) | El diseño concreto del helper se confirmará al implementar, reutilizando la estructura Web existente. |
| Ampliar las pruebas de integración HTTP existentes en vez de añadir una infraestructura de UI | Confirmado | [LinkCreationIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkCreationIntegrationTests.cs), [LinkManagementIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkManagementIntegrationTests.cs) | Verifica la respuesta observable sin introducir tooling o dependencias nuevas. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada; integración HTTP de creación y edición, campos URL e imagen | Web y SQLite de integración configurados por el test project | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkCreationIntegrationTests|FullyQualifiedName~LinkManagementIntegrationTests"` | El resumen y el campo afectado muestran el mensaje propietario y no contienen el identificador técnico; el alta inválida no crea un enlace y la edición inválida conserva sus datos. | Filtro ejecutado, cantidad de tests, resultado y aserciones de cuerpo/estado y persistencia. |
| V02 | CA02 | Automatizada; regresión de valores válidos y errores distintos de esquema en los formularios cubiertos | Mismo entorno integrado que V01 | El mismo filtro focalizado de V01 | Las entradas válidas conservan su resultado; los casos negativos preexistentes ajenos al cambio mantienen su presentación. | Casos cubiertos, resultado y cualquier diferencia observada. |

Las pruebas verifican el HTML de las respuestas privadas y el estado persistido; no sustituyen una revisión visual completa ni amplían el alcance a páginas públicas.

## Orden de ejecución y cierre

- Las tareas se crearán después de la aprobación de este plan; incluirán actualización de la presentación, pruebas de integración focalizadas y sincronización documental.
- No se iniciará ningún cambio productivo hasta que `tasks.md` esté aprobado.
- S9.6 conserva un cierre independiente; su revalidación manual y aceptación se registran en [sus tareas y evidencia](../s9.6-revision-presentacion-enlaces/tasks.md#aceptación-y-cierre).
