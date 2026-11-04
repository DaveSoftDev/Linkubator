# Plan técnico: S12.4 — Revisión y sincronización de la documentación

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 31-X-2026 con la referencia «Siguiente».
  - Plan: aprobado por DLG el 31-X-2026 con la referencia «Siguiente».

## Diseño de implementación

### Componentes y contratos

- La skill [requirements-review](../../skills/requirements-review/SKILL.md) define el procedimiento de revisión, las categorías de hallazgos y el formato del informe.
- El conjunto revisado comprende las fuentes documentales identificadas en [spec.md](spec.md#fuentes), las reglas editoriales de [AGENTS.md](../../AGENTS.md) y los diagramas de [context/diagrams/](../../context/diagrams/).
- El resultado es un informe trazable que distingue el estado general, los hallazgos, las decisiones pendientes y los cambios documentales realizados. Se consolidará como evidencia de la tarea en `tasks.md`; no se crea un artefacto adicional por defecto.

### Flujos internos

1. Verificar que las fuentes y diagramas del alcance existen y leerlos completos antes de editar.
2. Revisar ubicación y fuente única, referencias, terminología, consistencia entre documentos, alcance de MVP y las áreas de consistencia definidas por la skill.
3. Clasificar y priorizar los hallazgos con archivo y línea. Entregar primero el informe y distinguir las propuestas de las decisiones confirmadas.
4. Aplicar solo cambios documentales respaldados por la regla propietaria o una decisión confirmada. Si una corrección requiere una decisión, registrar la pregunta y no modificar la regla afectada.
5. Verificar las referencias y el contenido modificado; consolidar la evidencia y los riesgos residuales.

### Persistencia y dependencias

- No hay persistencia de aplicación, transacciones ni dependencias de código para este trabajo.
- Son prerrequisitos la disponibilidad de las fuentes y diagramas enumerados en [spec.md](spec.md#fuentes), y de la skill `requirements-review`.
- La revisión no depende de la finalización de tareas futuras de S12 ni asume sus evidencias.

### Garantías técnicas

- No se cambia código, SQL, pruebas ni configuración.
- Las reglas se mantienen en sus documentos propietarios; las demás referencias enlazan a la fuente.
- No se inventan reglas ni se resuelven ambigüedades por inferencia. Las decisiones necesarias se registran; únicamente bloquean el cambio afectado.
- La revisión y sus cambios no declaran cubiertas auditorías de implementación ni revisiones manuales asignadas a otras tareas de S12.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Seguir el flujo de `requirements-review`, incluido informar antes de editar. | confirmado | [requirements-review](../../skills/requirements-review/SKILL.md) | Mantiene los hallazgos separados de las correcciones y evita presentar propuestas como decisiones. |
| Consolidar el informe como evidencia en `tasks.md`, sin crear otro archivo por defecto. | propuesta | [Plantilla de tareas SDD](../../skills/sdd-writer/templates/tasks-template.md) | Se confirma al aprobar las tareas; si el formato no basta para una evidencia verificable, se podrá proponer un artefacto específico antes de ejecutarlo. |
| Un hallazgo contradictorio puede necesitar decisión humana antes de corregirlo. | confirmado | [spec.md → «Alcance»](spec.md#alcance), [requirements-review](../../skills/requirements-review/SKILL.md) | Registrar la decisión pendiente y dejar sin modificar solo el texto afectado; no impide cerrar la revisión con los hallazgos y límites documentados. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Revisión documental completa de las fuentes y diagramas | Fuentes accesibles en el repositorio | Ejecutar «Gather context» y «Build a consistency matrix» de [requirements-review](../../skills/requirements-review/SKILL.md); anotar el estado de cada documento, diagrama y área de consistencia | Todas las fuentes y áreas se revisaron, o cada ausencia, no aplicabilidad o límite quedó indicado explícitamente | Inventario de fuentes revisadas y cobertura por área en `tasks.md` |
| V02 | CA02 | Revisión técnica de clasificación y trazabilidad | V01 completada | Aplicar «Check placement», «Check single source», «Classify findings» y el orden de impacto de la skill; contrastar cada hallazgo con los documentos completos y registrar archivo y línea | Cada hallazgo tiene categoría, prioridad, ubicaciones, destino cuando aplica y recomendación; no hay propuestas presentadas como decisiones | Informe de hallazgos y decisiones necesarias en `tasks.md` |
| V03 | CA03 | Revisión documental de los cambios sincronizados | V02 completada; fuentes confirmadas para cada cambio | Seguir «Report before editing» y «Apply synchronized documentation changes»; contrastar cada modificación con la regla propietaria o decisión confirmada | Solo se aplican cambios respaldados; las preguntas abiertas permanecen registradas y no se modifica la regla en disputa | Relación de cambios documentales y fuente que los respalda, o constancia de que no hubo cambios aplicables |
| V04 | CA04 | Revisión técnica de referencias y contenido modificado | V03 completada | Inspeccionar enlaces Markdown y destinos relativos de los documentos y diagramas revisados; comprobar anclas contra sus títulos y revalidar responsabilidades, terminología y alcance | No quedan referencias rotas introducidas por los cambios y el contenido modificado sigue las fuentes y convenciones vigentes | Resultado de la comprobación de enlaces, anclas y sincronización en `tasks.md` |
| V05 | CA05 | Revisión final de integridad del informe y evidencia | V01–V04 completadas | Contrastar el informe con «Expected output» de [requirements-review](../../skills/requirements-review/SKILL.md) y con las evidencias registradas | El estado general, hallazgos, decisiones, cambios aplicados o no necesarios y riesgos residuales están registrados sin atribuir resultados o aprobaciones inexistentes | Informe final y revisión de completitud en `tasks.md` |

Estas comprobaciones son revisiones documentales y no demuestran que la implementación cumpla las reglas funcionales o técnicas. La ausencia de hallazgos se registrará solo tras revisar la fuente completa, no por una búsqueda parcial.

## Orden de ejecución y cierre

- Crear `tasks.md` después de que este plan obtenga aprobación humana explícita.
- Orden propuesto: inventario y lectura (V01); clasificación e informe previo (V02); sincronización respaldada (V03); verificación (V04); consolidación y revisión final (V05). V03 no puede preceder al informe de V02.
- Cerrar cuando CA01–CA05 tengan evidencia satisfactoria, las tareas propias estén completadas y la aceptación humana se registre en `tasks.md`. Las decisiones pendientes que no permitan un cambio concreto quedan documentadas y no condicionan el cierre de la revisión.
- Si aparece una contradicción o regla nueva, detener únicamente el cambio afectado y tramitar su aprobación en el documento propietario antes de sincronizarlo.
