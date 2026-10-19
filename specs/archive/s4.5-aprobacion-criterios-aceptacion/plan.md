# Plan técnico: S4.5 — Revisión y aprobación de los criterios de aceptación

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 15-X-2026 («Adelante»).
	- Plan: aprobado por DLG el 15-X-2026 («Vamos»).

## Diseño de implementación

### Componentes y contratos

El trabajo es documental y de revisión humana. El artefacto sometido a decisión es [plans/mvp0-acceptance-criteria.md](../../../plans/mvp0-acceptance-criteria.md); no se implementan contratos ni componentes de software.

| Componente | Responsabilidad | Criterios |
| --- | --- | --- |
| Revisión de cobertura | Contrastar cada operación y lectura prevista para S4 con el catálogo y señalar ausencias o capacidades fuera del alcance. | CA01 |
| Revisión de trazabilidad y verificabilidad | Comprobar que los criterios enlazan a fuentes propietarias pertinentes, tienen resultados observables y cubren rechazos y límites aplicables sin duplicar reglas. | CA02–CA03 |
| Decisión S4.5 | Presentar el catálogo a DLG y registrar la aprobación o las correcciones solicitadas con fecha y referencia reales. | CA04–CA05 |
| Sincronización documental | Reflejar únicamente el estado comprobado del catálogo y de la puerta S4.5 en sus documentos de seguimiento. | CA04–CA05 |

La revisión no amplía el alcance ni cambia reglas funcionales. Si se detecta una laguna o contradicción, se identifica la fuente propietaria y se detiene la decisión afectada hasta su resolución conforme a [AGENTS.md → «Principio de fuente única»](../../../AGENTS.md#dónde-vive-cada-cosa).

### Flujos internos

1. Confirmar que el catálogo sometido a revisión es el artefacto preparado en S4.4 y establecer su estado vigente antes de la decisión de DLG.
2. Cotejar el inventario de operaciones y lecturas de S4 con los identificadores del catálogo; registrar faltantes, duplicados relevantes o alcance añadido.
3. Revisar trazabilidad a fuentes, verificabilidad de resultados y cobertura de casos negativos y límites, sin reinterpretar reglas.
4. Separar observaciones editoriales de hallazgos que requieren cambio de criterio o de una fuente propietaria. Elevar los últimos para resolución antes de aprobar los criterios afectados.
5. Presentar a DLG el catálogo revisado para una decisión explícita. Registrar solo la decisión efectivamente recibida, indicando responsable, fecha y referencia literal o verificable.
6. Si DLG aprueba, actualizar el estado del catálogo y los enlaces/estado necesarios en el roadmap e índice. Si solicita cambios o no aprueba, conservar la puerta bloqueada, registrar el resultado real y delimitar la corrección o decisión pendiente.
7. No iniciar S4.6–S4.10 como parte de esta revisión; la aprobación documentada del catálogo es su prerrequisito independiente.

### Persistencia y dependencias

- No hay persistencia de aplicación, dependencias externas, paquetes ni cambios de código en este trabajo.
- Prerrequisitos: especificación S4.5 aprobada; S4.4 cerrada y aceptada; catálogo existente en `plans/mvp0-acceptance-criteria.md`.
- La decisión humana sobre el catálogo es posterior a la aprobación de este plan y de las tareas que se creen en la fase autorizada.
- La revisión usa como fuentes de reglas los documentos propietarios enlazados por el catálogo; el catálogo no desplaza esas fuentes.

### Garantías técnicas

- La puerta S4.5 solo se considera aprobada cuando existe una decisión explícita de DLG registrada con referencia real; aprobar esta spec o su plan no equivale a aprobar el catálogo.
- La identidad, propiedad, privacidad, comportamiento y límites técnicos se contrastan contra las fuentes propietarias; no se reinterpretan ni se copian al catálogo como reglas nuevas.
- La verificación documental no se presenta como prueba de implementación ni de comportamiento de consumidores.
- Si se identifica una contradicción, no se normaliza mediante edición silenciosa del catálogo: se deja explícita y se deriva a su fuente propietaria.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| D01. La decisión S4.5 se registra en el catálogo y se refleja en los documentos de navegación/seguimiento pertinentes. | Propuesta | [spec.md → «Objetivo»](spec.md#objetivo), [mvp0-plan.md → «Puerta de entrada de la implementación (criterios de aceptación de S4)»](../../../plans/mvp0-plan.md#puerta-de-entrada-de-la-implementación-criterios-de-aceptación-de-s4) | Mantiene un estado verificable sin duplicar el contenido de los criterios. Requiere aprobación de este plan. |
| D02. La revisión evalúa cobertura, trazabilidad, verificabilidad y límites de alcance; no vuelve a definir las reglas de producto. | Confirmado | [AGENTS.md → «Principio de fuente única»](../../../AGENTS.md#dónde-vive-cada-cosa), [spec.md → «Alcance»](spec.md#alcance) | Una discrepancia se devuelve a la fuente propietaria antes de aprobar el criterio afectado. |
| R01. La revisión puede encontrar observaciones que exijan modificar un criterio o resolver una fuente. | Pendiente de revisión | [spec.md → «Bloqueos y preguntas pendientes»](spec.md#bloqueos-y-preguntas-pendientes) | El criterio afectado y la puerta quedan pendientes; no se registra aprobación parcial como aprobación global ni se empieza S4.6–S4.10. |
| R02. La aprobación de esta especificación podría confundirse con la aprobación del catálogo. | Riesgo controlado | Referencia de spec «Adelante», 15-X-2026; [roadmap.md → «Now»](../../../context/roadmap.md#now) | Mantener ambos estados explícitos y separados; la puerta solo cambia tras una decisión independiente sobre el catálogo. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Revisión documental de cobertura | Catálogo existente y plan/tareas aprobados | Recorrer el inventario de S4 en [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) y cotejar operación por operación con el catálogo | Toda operación y lectura previstas cuentan con criterios identificables; se anotan ausencias o extensiones de alcance | Matriz operación/lectura → ID(s) de criterio, con resultado y referencias |
| V02 | CA02–CA03 | Revisión de trazabilidad y cobertura de escenarios | V01; acceso a las fuentes propietarias | Inspeccionar para cada criterio sus enlaces, resultado observable y casos negativos/límites aplicables frente a las reglas enlazadas | Trazabilidad pertinente, comprobabilidad y cobertura suficiente; cualquier ambigüedad queda explícita y vinculada a su fuente | Registro de revisión por identificador, fuentes comprobadas y hallazgos |
| V03 | CA02–CA03 | Validación técnica de referencias | Catálogo revisado | Comprobar rutas, anclas e identificadores del catálogo y sus fuentes; repetir `git diff --check` sobre los documentos modificados al cerrar los cambios | Enlaces locales resolubles, IDs no ambiguos y diff sin errores de whitespace; esta revisión no prueba el comportamiento de la aplicación | Resultado de validación con archivos revisados, comando/medio y hallazgos |
| V04 | CA04–CA05 | Revisión y decisión manual de DLG | V01–V03 satisfactorias y sin hallazgos bloqueantes pendientes | Obtener la decisión explícita de DLG sobre el catálogo; contrastar responsable, fecha y referencia antes de actualizar su estado | Se registra fielmente la decisión. Solo una aprobación explícita aprueba la puerta; una solicitud de cambios se registra como no aprobatoria y la puerta permanece cerrada hasta corregir los hallazgos y obtener una decisión posterior | Texto/referencia real de la decisión, fecha, responsable y estado resultante |
| V05 | CA04–CA05 | Revisión integrada de consistencia documental | Resultado de V04 | Contrastar estado del catálogo, roadmap e índice; confirmar que S4.6–S4.10 no se presentan como iniciadas sin la puerta aprobada | Todos los documentos expresan el mismo estado real; sin enlaces rotos ni aprobación inferida | Lista de archivos y referencias revisadas, resultado y limitaciones |

La revisión manual de DLG constituye la decisión de aceptación del catálogo, no una prueba técnica. V01–V03 y V05 verifican documentación y trazabilidad; no demuestran que los casos de uso estén implementados ni que funcionen. Si V02 encuentra una regla incompleta o contradictoria, no se fuerza un resultado esperado: se bloquea el criterio afectado y se deriva la resolución a su documento propietario. Los hallazgos bloqueantes deben resolverse antes de presentar el catálogo para la decisión S4.5.

## Orden de ejecución y cierre

- Crear `tasks.md` únicamente después de la aprobación explícita de este plan. Las tareas descompondrán el cotejo de cobertura, la revisión de fuentes/escenarios, la validación documental, la decisión de DLG y la sincronización de estado. La validación de Application comprueba categoría/código; el mapeo Web al mensaje y al tratamiento HTTP especificados queda fuera de S4.5.
- Las tareas asignarán evidencia a V01–V05 y mantendrán separadas la revisión técnica y la decisión humana. La ejecución de S4.5 quedó completada y aceptada por DLG el 16-X-2026 («Leído y comprendido»); el estado y la evidencia real constan en [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre).
- La puerta S4.5 solo queda aprobada cuando la decisión del catálogo se registra en el artefacto correspondiente con fecha, responsable y referencia real.
- Si el catálogo no se aprueba o aparecen bloqueos, mantener el estado pendiente y no habilitar S4.6–S4.10.
- No se autoriza implementación productiva por la aprobación de este plan; las tareas requieren aprobación propia antes de comenzar.