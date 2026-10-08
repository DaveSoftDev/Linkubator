# Tareas: S1.2 — Errores de dominio base y enumeradores

## Referencias y autorización

- Estado: aprobado por DLG, 09-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 09-X-2026 («Excepciones de dominio identificadas»).
  - Plan: [plan.md](plan.md), aprobado por DLG, 09-X-2026 («Listo para tareas»).
  - Actualización del contrato: aprobada por DLG, 09-X-2026 («hazlo con la base y las específicas»).
  - Tareas: aprobadas por DLG, 09-X-2026 (confirmación explícita en la conversación de implementación).
- Implementación:
  - Estado: implementación y validación completadas; S1.2 aceptada y cerrada por DLG el 09-X-2026.
  - Inicio: 09-X-2026; autorización registrada tras la confirmación explícita de DLG en la conversación de implementación.
  - Finalización: 09-X-2026; T02–T05 completadas. Evidencia detallada en tareas y criterios de aceptación.
  - Evidencia consolidada: T02–T05 y CA01–CA06; ver las filas siguientes.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

Estados: pendiente, en curso, completada o bloqueada. Enlazar al criterio y a la sección de diseño o validación correspondiente.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas. | CA01–CA06; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Aprobación explícita de DLG registrada; resultado esperado: tasks aprobado, sin alterar estados de spec/plan. | completada | DLG confirmó explícitamente la aprobación en la conversación de implementación, 09-X-2026. |
| T02 | Implementar `DomainException` abstracta, `TextExceedsMaximumLengthException` con su código estable y los enums `ScrapingStatus` y `Purpose` en Domain. | CA01–CA05; [plan → Componentes y contratos](plan.md#componentes-y-contratos) | T01 | Compilar `Linkubator.Domain`; resultado esperado: tipos públicos disponibles con los nombres y valores del plan, sin dependencias nuevas. | completada | `dotnet build Linkubator.sln --no-restore --verbosity minimal`: correcto, SDK 10.0.401. Domain no declara referencias ni paquetes externos. |
| T03 | Añadir tests unitarios de abstracción y constructor base, captura por subtipo específico, código estable, ausencia de datos sensibles y valores exactos de ambos enums. | CA01–CA05; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V01–V05 | T02 | Ejecutar los tests `DomainExceptionTests` y `DomainEnumerationsTests`; resultado esperado: todos los casos pasan. | completada | `DomainExceptionTests`: 5/5 correctos; suite integrada con ambas clases: 24/24 correctos, SDK 10.0.401. |
| T04 | Verificar el aislamiento arquitectónico y ejecutar las validaciones integradas. | CA06 y cobertura integrada CA01–CA05; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V06–V09 | T03 | Ejecutar la matriz de referencias, la comprobación negativa temporal, `dotnet test Linkubator.sln --no-restore --verbosity minimal` y `dotnet build Linkubator.sln --no-restore --verbosity minimal`; resultado esperado: infracción detectada por la prueba negativa y solución con tests/build correctos. | completada | Matriz: 1/1 correcta. La referencia temporal prohibida hizo fallar la matriz; tras retirarla, volvió a pasar 1/1. Suite: 24/24 correctos. Build integrado correcto. SDK 10.0.401. La suite informó una advertencia de puerto HTTPS en `WebStartupTests`. |
| T05 | Revisar trazabilidad y evidencia; presentar el resultado a DLG para aceptación. | CA01–CA06; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T04 | Confirmar que cada criterio tiene evidencia o un bloqueo explícito, que no se integraron entidades de S3 y registrar aceptación o reparos de DLG. | completada | DLG acepta y da por terminado S1.2 el 09-X-2026 («Por terminada... Que no hay tareas pendientes, spec aprobada, plan aprobada, tasks aprobado, implementación iniciada y terminada»). La integración de CA02 en una entidad queda aceptada para S3, fuera de las tareas de S1.2. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02–T03 | V01 | Satisfecho: `DomainException` es abstracta y `TextExceedsMaximumLengthException` se captura por su tipo específico o por la base. | 09-X-2026, Windows, SDK 10.0.401 | `DomainExceptionTests`; 5/5 pruebas correctas. |
| CA02 | T02–T03; la integración en una entidad queda en S3 según el alcance aprobado | V02 | Contrato satisfecho en S1.2: el subtipo expone `TextExceedsMaximumLength`. DLG acepta que el lanzamiento desde una entidad se implemente y valide en S3; no queda como tarea pendiente de S1.2. | 09-X-2026, Windows, SDK 10.0.401 | `DomainExceptionTests`; la integración de entidad pertenece a S3 y no forma parte de este trabajo. |
| CA03 | T02–T03 | V03 | Satisfecho: la base solo admite su código por constructor protegido; el subtipo no recibe datos y la propiedad pública declarada es `Code`. | 09-X-2026, Windows, SDK 10.0.401 | `DomainExceptionTests`; 5/5 pruebas correctas. |
| CA04 | T02–T03 | V04 | Satisfecho: `ScrapingStatus` contiene exactamente los cinco valores especificados. | 09-X-2026, Windows, SDK 10.0.401 | `DomainEnumerationsTests`; 5/5 pruebas focalizadas correctas. |
| CA05 | T02–T03 | V05 | Satisfecho: `Purpose` contiene exactamente los cuatro valores especificados. | 09-X-2026, Windows, SDK 10.0.401 | `DomainEnumerationsTests`; 5/5 pruebas focalizadas correctas. |
| CA06 | T04 | V06 | Satisfecho: matriz correcta y sensible a una referencia prohibida temporal. | 09-X-2026, Windows, SDK 10.0.401 | `ProjectReferenceMatrixTests`; pasó con el proyecto limpio y falló con la referencia temporal. `Linkubator.Domain.csproj` no declara referencias de proyecto ni paquetes externos. |

CA02 conserva su formulación de la spec, que requiere que una entidad lance el error. DLG acepta cerrar S1.2 con el contrato tipado probado y deja la integración efectiva de la entidad para S3; no bloquea este cierre.

## Aceptación y cierre

- Estado del trabajo: cerrado; implementación y validación completadas y aceptadas por DLG el 09-X-2026.
- Criterios sin evidencia satisfactoria: ninguno pendiente para el cierre de S1.2; el lanzamiento de CA02 desde una entidad se implementará y validará en S3, según aceptación de DLG.
- Bloqueos y riesgos restantes: ninguno para S1.2.
- Aceptación humana: DLG, 09-X-2026; confirmación explícita «Por terminada...». 
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso: continuar con las tareas posteriores del roadmap; integrar y validar el lanzamiento de CA02 desde las entidades en S3.