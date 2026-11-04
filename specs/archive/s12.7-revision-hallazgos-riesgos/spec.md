# Especificación: S12.7 — Revisión de hallazgos de auditoría y aceptación de riesgos

## Estado y aprobación

- Estado: aprobada.
- Preparada por IA el 31-X-2026.
- Aprobada por DLG el 31-X-2026 con la referencia «Siguiente».

## Objetivo

Revisar el resultado aceptado de la auditoría S12.3 y registrar una decisión humana trazable sobre sus conclusiones, límites y riesgos residuales, sin presentar una aceptación como corrección técnica ni como cierre de S12.

## Fuentes

- [mvp0-plan.md → «S12: Cierre y hardening (M)»](../../../plans/mvp0-plan.md#s12-cierre-y-hardening-m)
- [spec.md de S12.3](../s12.3-auditoria-invariantes-criticas/spec.md)
- [tasks.md de S12.3 → «Informe de auditoría»](../s12.3-auditoria-invariantes-criticas/tasks.md#informe-de-auditoría)
- [tasks.md de S12.3 → «Aceptación y cierre»](../s12.3-auditoria-invariantes-criticas/tasks.md#aceptación-y-cierre)
- [decisions.md → «Identidad de desarrollo»](../../../context/decisions.md#identidad-de-desarrollo)
- [AGENTS.md → «Invariantes críticas»](../../../AGENTS.md#invariantes-críticas)

## Alcance

- Incluye:
  - Revisar las conclusiones de CA01–CA06 del informe aceptado de S12.3, incluidos los casos no aplicables y los límites de la evidencia.
  - Revisar el riesgo de identidad de desarrollo ya aceptado y decidir si se mantiene esa aceptación.
  - Decidir por separado el tratamiento de los riesgos preventivos de logging de credenciales y de validación del formato del hash descritos en el informe de S12.3.
  - Registrar la disposición humana de cada conclusión o riesgo: aceptación explícita con su contexto y alcance, o seguimiento/mitigación pendiente con su referencia y responsable, si se conocen.
- Excluye:
  - Repetir la auditoría de código y SQL, o modificar código, SQL, pruebas o reglas documentadas.
  - Implementar mitigaciones o decidir requisitos de operaciones de cuenta que estén pendientes.
  - Declarar satisfecha la Definition of Done de S12 o cerrar el MVP0.
- Trabajo vecino y responsable de sus entregables: S12.3 produce y mantiene la auditoría técnica; S12.7 revisa sus resultados y registra la decisión humana. Cualquier mitigación que se acuerde queda fuera de esta revisión y conserva su propio seguimiento.

## Dependencias y prerrequisitos

- S12.3 está aceptada y su informe, evidencia y límites están disponibles en los enlaces anteriores.
- La aceptación previa del riesgo de identidad de desarrollo se documenta en [decisions.md → «Identidad de desarrollo»](../../../context/decisions.md#identidad-de-desarrollo); S12.7 revisará si se mantiene, sin atribuir una nueva decisión antes de registrarla.
- No se requiere implementación técnica para realizar esta revisión. La aprobación de esta especificación no equivale a la decisión humana sobre los riesgos.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [S12.3 → «Informe de auditoría»](../s12.3-auditoria-invariantes-criticas/tasks.md#informe-de-auditoría) | El informe aceptado contiene conclusiones, límites y elementos no aplicables para CA01–CA06 | La persona responsable revisa el resultado completo de la auditoría | La revisión registra una disposición para cada conclusión y límite; los elementos no aplicables no se presentan como garantías demostradas. |
| CA02 | [S12.3 → «CA04 — Contraseñas, tokens y logs»](../s12.3-auditoria-invariantes-criticas/tasks.md#informe-de-auditoría), [specifications.md → «Contraseñas»](../../../context/specifications.md#contraseñas), [specifications.md → «Tokens»](../../../context/specifications.md#tokens) | El informe identifica los riesgos preventivos de logging de credenciales y validación del formato del hash | La persona responsable evalúa cada riesgo por separado | Queda registrada para cada riesgo una aceptación explícita con su alcance y contexto, o una acción de seguimiento/mitigación; ninguno se considera aceptado por silencio o por la existencia de una prueba que no cubre el caso. |
| CA03 | [S12.3 → «CA01 — Propiedad de los datos»](../s12.3-auditoria-invariantes-criticas/tasks.md#informe-de-auditoría), [S12.3 → «CA02 — Público y privado»](../s12.3-auditoria-invariantes-criticas/tasks.md#informe-de-auditoría), [decisions.md → «Identidad de desarrollo»](../../../context/decisions.md#identidad-de-desarrollo) | El informe registra el riesgo de identidad de desarrollo previamente aceptado y el límite de que SQL no impone la relación entre los estados público/privado | La persona responsable revisa si las decisiones existentes siguen siendo aceptables para el alcance actual | La revisión confirma explícitamente si se mantienen o cambian las disposiciones previas, y registra cualquier seguimiento necesario sin convertirlo en una regla nueva. |
| CA04 | [mvp0-plan.md → «S12: Cierre y hardening (M)»](../../../plans/mvp0-plan.md#s12-cierre-y-hardening-m) | Se han revisado las conclusiones y los riesgos del informe | Se consolida el resultado humano de S12.7 | Existe un registro trazable con decisiones, contexto y referencias; cualquier mitigación no ejecutada queda identificada como pendiente y no se presenta como riesgo aceptado ni como Definition of Done satisfecha. |
| CA05 | [S12.3 → «Aceptación y cierre»](../s12.3-auditoria-invariantes-criticas/tasks.md#aceptación-y-cierre) | La revisión y sus disposiciones están documentadas | DLG acepta explícitamente el resultado de S12.7 | La aceptación incluye fecha y referencia real, y permanece diferenciada de la aceptación de riesgos individuales, de la ejecución de mitigaciones y del cierre de S12. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| La disposición actual de cada conclusión y riesgo residual está pendiente hasta ejecutar la revisión; esto no bloquea la definición de la especificación. | [Informe de auditoría S12.3](../s12.3-auditoria-invariantes-criticas/tasks.md#informe-de-auditoría) | CA01–CA05 | Revisar cada elemento y registrar su decisión; no anticiparla antes de ejecutar la revisión. |

## Artefactos relacionados

- [Informe de auditoría S12.3](../s12.3-auditoria-invariantes-criticas/tasks.md#informe-de-auditoría) — fuente revisada por este trabajo.
- [Plan técnico](plan.md) — aprobado por DLG el 31-X-2026 con la referencia «Siguiente».
- [Tareas](tasks.md) — aprobadas por DLG el 31-X-2026 con la referencia «Vamos»; revisión no iniciada.
