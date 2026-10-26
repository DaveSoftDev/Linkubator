# Tareas: S9.4 — Pruebas de integración HTTP de enlaces

## Referencias y autorización

- Estado: aprobadas por DLG el 26-X-2026 («Vamos»); implementación iniciada y completada con evidencia en el árbol de trabajo actual.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 26-X-2026 («Aprobar la especificación S9.4 y continuar con el plan»).
  - Plan: [plan.md](plan.md), aprobado por DLG el 26-X-2026 («Aprobar el plan y continuar con la preparación de las tareas»).
  - Tareas: aprobadas por DLG el 26-X-2026 («Vamos»).
- Implementación:
  - Estado: completada.
  - Inicio real: 26-X-2026, con la aprobación de las tareas y la puesta en marcha del trabajo de integración HTTP.
  - Finalización real: 26-X-2026, tras la ejecución y superación de la batería focalizada de pruebas HTTP de creación y gestión de enlaces.
  - Evidencia consolidada: [tests/Linkubator.Tests/Integration/LinkCreationIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkCreationIntegrationTests.cs), [tests/Linkubator.Tests/Integration/LinkManagementIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkManagementIntegrationTests.cs) y la validación `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkCreationIntegrationTests|FullyQualifiedName~LinkManagementIntegrationTests" --nologo` con 144/144 pruebas correctas.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación; la evidencia real queda recogida en esta sección con la ejecución efectiva del trabajo.

## Descomposición

T01 registra el inicio autorizado y contrasta la cobertura existente. Tras T01, T02 (creación) y T03 (edición, movimiento, acceso y atomicidad) pueden ejecutarse en paralelo porque usan fixtures aislados. T04 integra la validación después de ambas; T05 registra la revisión y aceptación humana. Ninguna tarea depende de S9.5 o S9.6.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Registrar la fecha y referencia reales de inicio y contrastar la cobertura HTTP existente con CA01–CA05 e identificar solo brechas demostrables. | Todos; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Garantías técnicas»](plan.md#garantías-técnicas) y [«Estrategia de validación»](plan.md#estrategia-de-validación). | Ninguna | El resumen y el roadmap distinguen inicio de aprobación y aceptación; la matriz de cobertura identifica para cada criterio los tests existentes y las comprobaciones que faltan, sin duplicar evidencia equivalente. | completado | Se ejecutó la comprobación del estado real del árbol y la batería HTTP ya existente; la cobertura de CA01–CA05 quedó respaldada por los tests de integración de enlaces y por la validación final focalizada. |
| T02 | Añadir únicamente las pruebas HTTP de creación que falten para colección inválida, inexistente o ajena y para los datos manipulados de propiedad o privacidad. | CA01, CA03; [plan.md → «Flujos internos»](plan.md#flujos-internos), V01 y V03. | T01 | `LinkCreationIntegrationTests` comprueba el tratamiento HTTP, ausencia de creación/divulgación, identidad autenticada y estado de privacidad; un control válido confirma que la creación permitida funciona. | completado | Evidencia en [tests/Linkubator.Tests/Integration/LinkCreationIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkCreationIntegrationTests.cs); la batería ejecutada confirma el caso exitoso y los casos negativos. |
| T03 | Añadir únicamente las pruebas HTTP de gestión que falten para edición y movimiento con destino inválido, inexistente o ajeno, acceso a enlaces no accesibles y ausencia de cambios parciales. | CA02–CA05; [plan.md → «Flujos internos»](plan.md#flujos-internos), [«Garantías técnicas»](plan.md#garantías-técnicas), V02–V05. | T01 | `LinkManagementIntegrationTests` comprueba respuestas aprobadas, igualdad de tratamiento donde corresponda, no divulgación y comparación del enlace, relaciones y proyección antes/después; los controles válidos detectan escrituras permitidas. | completado | Evidencia en [tests/Linkubator.Tests/Integration/LinkManagementIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkManagementIntegrationTests.cs); la validación focalizada cubre los flujos de edición, movimiento, acceso y atomicidad. |
| T04 | Ejecutar los filtros HTTP focalizados combinados y comprobar la trazabilidad de cada criterio y los enlaces documentales. | CA01–CA05; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), V01–V06. | T02, T03 | Los tests focalizados de creación y gestión pasan; cada criterio enlaza a pruebas y evidencia real, los destinos locales resuelven y no se atribuye al filtro focalizado el resultado de la suite completa. | completado | Validación ejecutada: `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter "FullyQualifiedName~LinkCreationIntegrationTests|FullyQualifiedName~LinkManagementIntegrationTests" --nologo` → 144/144 correctas. |
| T05 | Presentar la evidencia a DLG, registrar aceptación o correcciones y sincronizar el resultado final en [roadmap.md](../../context/roadmap.md) y [specs-index.md](../specs-index.md). | CA01–CA05; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre). | T04 | La aceptación humana, si se concede, queda registrada con responsable, fecha y referencia real; si se rechaza, constan las correcciones y el trabajo permanece abierto. Roadmap e índice reflejan el estado real. | completado | DLG confirmó la aceptación humana con la referencia «Completado» el 26-X-2026; roadmap e índice sincronizados. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T02, T04 | V01 y V06 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Completado | 26-X-2026, validación focalizada HTTP | [tests/Linkubator.Tests/Integration/LinkCreationIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkCreationIntegrationTests.cs) |
| CA02 | T01, T03, T04 | V02 y V06 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Completado | 26-X-2026, validación focalizada HTTP | [tests/Linkubator.Tests/Integration/LinkManagementIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkManagementIntegrationTests.cs) |
| CA03 | T01, T02, T03, T04 | V03 y V06 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Completado | 26-X-2026, validación focalizada HTTP | [tests/Linkubator.Tests/Integration/LinkCreationIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkCreationIntegrationTests.cs) y [tests/Linkubator.Tests/Integration/LinkManagementIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkManagementIntegrationTests.cs) |
| CA04 | T01, T03, T04 | V04 y V06 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Completado | 26-X-2026, validación focalizada HTTP | [tests/Linkubator.Tests/Integration/LinkManagementIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkManagementIntegrationTests.cs) |
| CA05 | T01, T03, T04 | V02, V05 y V06 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Completado | 26-X-2026, validación focalizada HTTP | [tests/Linkubator.Tests/Integration/LinkManagementIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/LinkManagementIntegrationTests.cs) |

## Aceptación y cierre

- Estado del trabajo: completado.
- Criterios con evidencia satisfactoria: CA01–CA05.
- Bloqueos y riesgos restantes: ninguno conocidos tras la validación focalizada y la ausencia de regresiones en la batería HTTP relevante.
- Aceptación humana: aceptada por DLG el 26-X-2026 («Completado»).
- Archivado: no solicitado.
- Ubicación archivada: no aplica.
- Fecha efectiva de archivado y comprobación de enlaces: no aplica.
- Siguiente paso autorizado: cerrar la etapa S9.4 con la evidencia registrada y continuar con la siguiente pieza del roadmap.
