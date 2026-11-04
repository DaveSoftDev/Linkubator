# Especificación: S12.4 — Revisión y sincronización de la documentación

## Estado y aprobación

- Estado: aprobada.
- Preparada por IA el 31-X-2026.
- Aprobada por DLG el 31-X-2026 con la referencia «Siguiente».

## Objetivo

Revisar la documentación funcional y sus vistas derivadas conforme a la skill `requirements-review`, registrar hallazgos trazables y sincronizar los documentos cuando la corrección esté respaldada por una regla propietaria o una decisión confirmada, sin inventar ni resolver reglas por iniciativa propia.

## Fuentes

- [mvp0-plan.md → «S12: Cierre y hardening (M)»](../../plans/mvp0-plan.md#s12-cierre-y-hardening-m)
- [roadmap.md → «Checklist de calidad del MVP0»](../../context/roadmap.md#checklist-de-calidad-del-mvp0)
- [AGENTS.md → «Dónde vive cada cosa»](../../AGENTS.md#dónde-vive-cada-cosa)
- [AGENTS.md → «Dónde va cada texto»](../../AGENTS.md#dónde-va-cada-texto)
- [AGENTS.md → «Reglas de documentación»](../../AGENTS.md#reglas-de-documentación)
- [requirements-review](../../skills/requirements-review/SKILL.md)
- [requirements.md](../../context/requirements.md)
- [specifications.md](../../context/specifications.md)
- [decisions.md](../../context/decisions.md)
- [architecture.md](../../context/architecture.md)
- [domain-model.md](../../context/domain-model.md)
- [roadmap.md](../../context/roadmap.md)

## Alcance

- Incluye:
  - Revisar completos los documentos de la fuente de verdad indicados por `requirements-review`, las reglas editoriales de [AGENTS.md](../../AGENTS.md) y los diagramas de [context/diagrams/](../../context/diagrams/).
  - Comprobar la ubicación y fuente única de las reglas, las referencias y la terminología, la coherencia entre documentos, el alcance de MVP y las áreas de consistencia definidas por la skill.
  - Entregar un informe con el estado general, hallazgos priorizados y clasificados, ubicaciones, recomendaciones, decisiones necesarias y riesgos o límites residuales.
  - Aplicar y verificar los cambios documentales necesarios cuando estén respaldados por una regla en su documento propietario o por una decisión ya confirmada. Si un hallazgo requiere decidir o cambiar una regla, registrarlo y dejarlo pendiente de resolución humana.
- Excluye:
  - Cambiar código, SQL, pruebas, configuración o comportamiento del producto.
  - Crear o decidir reglas de producto, resolver contradicciones ambiguas sin aprobación o ampliar el alcance confirmado.
  - Auditar la implementación, los logs o las invariantes en código y SQL, responsabilidades cubiertas por otras tareas de S12.
- Trabajo vecino y responsable de sus entregables: S12.1 mantiene la matriz de trazabilidad; S12.2 prueba eventos y logs; S12.3 audita invariantes críticas en código y SQL; S12.5 y S12.6 realizan revisiones manuales; S12.7 revisa hallazgos de seguridad y riesgos; S12.8 cierra el roadmap. S12.4 no depende de la ejecución de las tareas posteriores ni sustituye sus entregables.

## Dependencias y prerrequisitos

- La skill `requirements-review` y los documentos fuente enumerados están disponibles en el repositorio.
- El roadmap sitúa S12 como etapa actual. La preparación o aprobación de esta especificación no implica que la revisión se haya ejecutado ni que S12 esté cerrada.
- No se identifica un bloqueo para definir el alcance. Las contradicciones, decisiones no confirmadas o fuentes ausentes que aparezcan durante la revisión se registrarán como hallazgos; no se resolverán por inferencia.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [requirements-review → «Source of truth» y «Review workflow»](../../skills/requirements-review/SKILL.md) | Están disponibles los documentos y diagramas incluidos en la revisión | Se completa la lectura y el contraste de la fuente de verdad | El informe cubre los documentos aplicables y los diagramas; cada área de consistencia de la skill queda revisada o identificada explícitamente como no aplicable, ausente o no verificable. |
| CA02 | [AGENTS.md → «Dónde va cada texto»](../../AGENTS.md#dónde-va-cada-texto), [requirements-review → «Check placement», «Check single source» y «Classify findings»](../../skills/requirements-review/SKILL.md) | Se ha contrastado la documentación completa | Se clasifican y priorizan las discrepancias encontradas | Cada hallazgo tiene categoría, impacto, archivo y línea de cada lugar implicado, destino cuando corresponda y recomendación; los enlaces inválidos, duplicados, problemas de ubicación, incoherencias, omisiones, ambigüedades, deriva de alcance y riesgos se distinguen sin presentar propuestas como decisiones. |
| CA03 | [requirements-review → «Report before editing» y «Apply synchronized documentation changes»](../../skills/requirements-review/SKILL.md), [AGENTS.md → «Reglas de documentación»](../../AGENTS.md#reglas-de-documentación) | El informe identifica cambios de documentación respaldados por fuentes propietarias o decisiones confirmadas | Se sincronizan los documentos afectados | Las reglas quedan en su documento propietario y los demás documentos las enlazan; se aplican solo los cambios respaldados, se evitan ediciones no relacionadas y cualquier cambio que requiera una decisión humana queda pendiente, sin alterar la regla en disputa. |
| CA04 | [AGENTS.md → «Reglas de documentación»](../../AGENTS.md#reglas-de-documentación), [requirements-review → «Review workflow»](../../skills/requirements-review/SKILL.md) | Se han aplicado los cambios documentales autorizados | Se verifican los documentos actualizados y sus referencias | Las referencias de los documentos y diagramas revisados se comprueban, no quedan enlaces o anclas rotos introducidos por los cambios, y la documentación actualizada respeta las responsabilidades, la terminología y las restricciones de alcance de sus fuentes. |
| CA05 | [requirements-review → «Expected output»](../../skills/requirements-review/SKILL.md), [mvp0-plan.md → «S12: Cierre y hardening (M)»](../../plans/mvp0-plan.md#s12-cierre-y-hardening-m) | Se ha completado la revisión y la sincronización que permiten las fuentes confirmadas | Se consolida la evidencia de S12.4 | Quedan registrados el estado general, los hallazgos y decisiones pendientes, los cambios aplicados —o que no fueron necesarios— y los riesgos residuales; ningún resultado esperado, decisión o aceptación se presenta como evidencia real. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguna identificada al preparar la especificación. Los hallazgos que requieran una decisión se registrarán durante la revisión y bloquearán solo el cambio afectado. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md) — propuesta, pendiente de aprobación.
- Las tareas se crearán tras la aprobación del plan técnico.
