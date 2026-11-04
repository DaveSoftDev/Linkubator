# Plan técnico: S12.7 — Revisión de hallazgos de auditoría y aceptación de riesgos

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 31-X-2026 con la referencia «Siguiente».
  - Plan: aprobado por DLG el 31-X-2026 con la referencia «Siguiente».

## Diseño de implementación

### Componentes y contratos

- No se crearán ni modificarán componentes de producto. El entregable es un registro de revisión humana en las tareas de S12.7, trazado a las conclusiones y evidencias aceptadas de [S12.3](../s12.3-auditoria-invariantes-criticas/tasks.md#informe-de-auditoría).
- La revisión utilizará como entradas el informe completo de S12.3 y las decisiones documentadas sobre la identidad de desarrollo. No volverá a auditar código ni SQL.
- El registro distinguirá cada conclusión auditada, cada riesgo residual, su disposición explícita y cualquier seguimiento acordado. La aceptación del riesgo y la aceptación del resultado de S12.7 se registrarán por separado.

### Flujos internos

1. Reunir las conclusiones, los elementos no aplicables y los límites de evidencia de CA01–CA06 del informe aceptado de S12.3.
2. Revisar la disposición previa del riesgo de identidad de desarrollo y el límite señalado sobre la relación de estados público/privado en SQL.
3. Decidir por separado el tratamiento de los riesgos preventivos de logging de credenciales y de validación del formato del hash.
4. Registrar para cada elemento su aceptación explícita y alcance, o bien el seguimiento/mitigación pendiente y su responsable y referencia cuando se conozcan.
5. Comprobar que no hay aceptación implícita, que los elementos pendientes no se presentan como resueltos y que el resultado no declara cerrado S12.

### Persistencia y dependencias

- No hay cambios de persistencia, transacciones, integraciones ni dependencias de software.
- El único prerrequisito es el informe aceptado de S12.3 y las fuentes de decisión enlazadas en [spec.md → «Fuentes»](spec.md#fuentes).
- La evidencia se consolidará en el artefacto de tareas de S12.7, que se creará solo después de aprobar este plan. No se requiere infraestructura ni una ejecución de tests para esta revisión humana.

### Garantías técnicas

- Cada conclusión y riesgo recibe una disposición explícita; la ausencia de comentarios no equivale a aceptación.
- Los elementos no aplicables y los límites de evidencia permanecen diferenciados de hallazgos corregidos o garantías demostradas.
- Las mitigaciones no ejecutadas se conservan como pendientes y no se usan para declarar satisfecha la Definition of Done de S12.
- La revisión no modifica código, SQL, pruebas ni reglas propietarias, conforme al [alcance aprobado](spec.md#alcance).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Mantener la revisión como trabajo humano, sin cambios productivos ni repetición de la auditoría. | Confirmado | [spec.md → «Alcance»](spec.md#alcance) | La evidencia consiste en decisiones trazables sobre el informe existente, no en nuevas pruebas técnicas. |
| Consolidar las disposiciones y su evidencia en las tareas S12.7, sin crear un informe independiente por ahora. | Propuesta | Convención de [sdd-writer](../../../skills/sdd-writer/SKILL.md) | Confirmar al aprobar las tareas; separar un informe solo si el volumen de evidencia lo requiere. |
| La decisión sobre los riesgos preventivos de logging y validación del hash aún no está tomada. | Pendiente | [Informe de auditoría S12.3](../s12.3-auditoria-invariantes-criticas/tasks.md#informe-de-auditoría) | Debe decidirse cada riesgo por separado; no asumir aceptación ni implementar mitigaciones como parte de S12.7. |
| La aceptación previa del riesgo de identidad de desarrollo debe revisarse sin atribuirle una nueva aprobación. | Pendiente | [decisions.md → «Identidad de desarrollo»](../../../context/decisions.md#identidad-de-desarrollo) | Registrar si se mantiene la disposición previa o si se solicita seguimiento adicional. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Revisión manual de todas las conclusiones, límites y elementos no aplicables de CA01–CA06 | Informe aceptado de S12.3 disponible | Contrastar el registro de revisión contra cada criterio y conclusión del informe | Cada elemento tiene una disposición registrada; los no aplicables y límites no se presentan como cumplimiento demostrado | Referencia al informe, lista de elementos revisados y disposiciones en las tareas S12.7 |
| V02 | CA02, CA03 | Revisión manual, separada por riesgo o decisión previa | V01 completada; fuentes de decisión disponibles | Comprobar que se dispone por separado el riesgo de identidad, el límite de estados público/privado en SQL y los dos riesgos preventivos de logging y hash | Cada riesgo y disposición previa tiene decisión explícita o seguimiento pendiente; no hay aceptación por silencio | Decisión, contexto, alcance y referencias por elemento en las tareas S12.7 |
| V03 | CA04 | Revisión técnica del registro consolidado, sin ejecución de código | V01 y V02 completadas | Verificar trazabilidad de cada disposición y que los pendientes no se describen como mitigados ni como cierre de S12 | Registro completo, sin contradicciones entre decisión, evidencia y estado de seguimiento | Resultado del contraste y cualquier pendiente en las tareas S12.7 |
| V04 | CA05 | Aceptación humana | Revisión consolidada disponible | Registrar la aceptación explícita de DLG del resultado de S12.7 | Aceptación con fecha y referencia real, diferenciada de las decisiones individuales sobre riesgos y del cierre de S12 | Referencia explícita, responsable y fecha en las tareas S12.7 |

Estas comprobaciones son revisiones documentales y humanas; no demuestran por sí mismas la corrección técnica de código o SQL ni la ejecución de mitigaciones.

## Orden de ejecución y cierre

- Crear `tasks.md` solo después de que este plan tenga aprobación humana explícita.
- Ejecutar V01–V03 y registrar evidencia por criterio en las tareas; después solicitar la aceptación humana prevista en V04.
- El cierre de S12.7 requiere evidencia satisfactoria para sus criterios y aceptación humana registrada. Cualquier mitigación pendiente conserva su seguimiento independiente y no cierra S12.
