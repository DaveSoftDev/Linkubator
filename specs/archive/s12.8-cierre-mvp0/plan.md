# Plan técnico: S12.8 — Cierre del MVP0

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 31-X-2026 con la referencia «Siguiente».
  - Plan anterior: aprobado por DLG el 31-X-2026 con la referencia «Siguiente».
  - Secuencia revisada: aprobada por DLG el 02-XI-2026 con la referencia «Aprobar el plan revisado».
  - Registro dedicado de retrospectiva: solicitado por DLG el 02-XI-2026 con la referencia «crear_archivo» y aprobado en esta revisión por DLG el 02-XI-2026 con la referencia «Aprobar esta revisión del plan».

## Diseño de implementación

### Componentes y contratos

- No se crearán ni modificarán componentes de producto. El trabajo es un cierre humano sustentado en la [Definition of Done de S12](../../../plans/mvp0-plan.md#s12-cierre-y-hardening-m) y en la evidencia que hayan registrado las tareas anteriores.
- La evidencia final de los criterios y tareas se consolida en `tasks.md`; el contenido de la retrospectiva final se registra en `retrospectiva.md` y se enlaza desde la tarea correspondiente.
- Los entregables son la actualización del estado del MVP0 en el roadmap, una etiqueta de versión sobre la revisión que contiene el cierre y el registro de la retrospectiva en `retrospectiva.md`.

### Flujos internos

1. Contrastar cada punto de la Definition of Done de S12, excepto el punto de actualizar el roadmap, con la evidencia de las tareas responsables. Si falta evidencia o queda un requisito previo pendiente, detener el cierre y registrar el bloqueo.
2. Actualizar el roadmap para reflejar que el MVP0 está en «Hecho» y que S12 ya no es la etapa actual; marcar como completado en `mvp0-plan.md` el punto correspondiente de la Definition of Done.
3. Realizar la retrospectiva final, conservar su contenido en `retrospectiva.md` y registrar en `tasks.md` su fecha y referencia.
4. Presentar a DLG el contenido del cierre y registrar su aceptación previa para CA01, CA02 y CA04. Esta aceptación no cierra S12.8 porque la etiqueta aún no existe. La revisión aceptada debe quedar en un commit que DLG proporcione antes de etiquetarlo.
5. Acordar con DLG el identificador de versión, crear la etiqueta sobre el commit que contiene la aceptación previa y verificar su destino.
6. Revisar la trazabilidad de CA01–CA04 y solicitar la aceptación humana final de S12.8.

### Persistencia y dependencias

- No hay cambios en persistencia de aplicación, transacciones, contratos de código ni dependencias de software.
- El prerrequisito es que los puntos anteriores de la Definition of Done estén completados y respaldados por la evidencia de sus tareas responsables. S12.8 no sustituye ni completa sus resultados.
- La etiqueta requiere un identificador acordado y un commit disponible que contenga la aceptación previa del cierre; no se presupone que una etiqueta o revisión específica exista. DLG proporciona ese commit; no se etiqueta un worktree sin commit.

### Garantías técnicas

- No se mueve el MVP0 a «Hecho» si falta algún prerrequisito o evidencia de cierre.
- La etiqueta apunta al commit que contiene la aceptación previa del contenido del cierre; el nombre y la revisión quedan registrados para su verificación.
- La retrospectiva, la aceptación previa del contenido y la aceptación final de S12.8 se registran como acciones distintas; ninguna aceptación se infiere de la ejecución de los pasos.
- El plan no cambia las reglas del producto ni modifica código, tests o datos de aplicación.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Ejecutar el cierre como revisión humana de la Definition of Done y sus evidencias, sin cambios de producto. | Confirmado | [spec.md → «Alcance»](spec.md#alcance) | Los puntos previos incompletos bloquean el cierre; S12.8 no los repara ni los acepta en nombre de sus responsables. |
| Registrar la evidencia de criterios en las tareas y mantener el contenido de la retrospectiva en un archivo dedicado. | Confirmado | DLG, 02-XI-2026, referencias «crear_archivo» y «Aprobar esta revisión del plan». | `tasks.md` conserva la trazabilidad; `retrospectiva.md` contiene el registro redactado por DLG. |
| Registrar una aceptación previa del contenido antes de etiquetar su commit, y una aceptación final después de verificar CA03. | Confirmado | DLG, 02-XI-2026, «Aprobar el plan revisado». | La revisión previa queda dentro del commit etiquetado; la aceptación final incluye la verificación de la etiqueta y cierra CA01–CA04. |
| Identificador concreto de la etiqueta de versión. | Confirmado | DLG, 02-XI-2026, identificador «MVP0». | Se creó la etiqueta y se verificó su commit de destino en V03. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Revisión manual de los puntos de la Definition of Done anteriores a S12.8 y sus evidencias | Evidencia de las tareas responsables disponible | Contrastar cada punto aplicable de la [Definition of Done de S12](../../../plans/mvp0-plan.md#s12-cierre-y-hardening-m) con su evidencia enlazada | Todos los prerrequisitos están satisfechos; cualquier ausencia queda registrada como bloqueo y no se cierra S12.8 | Lista de puntos revisados, enlaces a evidencia y bloqueos, si los hay |
| V02 | CA02 | Revisión documental del estado final del roadmap y del plan | V01 satisfactorio | Revisar el estado de «Hecho», el bloque «Now» y el último punto de la Definition of Done en `mvp0-plan.md` | El MVP0 figura como terminado, S12 ya no aparece como etapa actual y el último punto de la Definition of Done está marcado como completado con evidencia | Referencias a las secciones actualizadas y evidencia del cierre |
| V03 | CA03 | Verificación técnica de la etiqueta Git | Identificador acordado, aceptación previa registrada y commit que la contiene disponible | Comparar `git rev-parse --verify "<etiqueta>^{commit}"` con `git rev-parse "<revision-cierre>^{commit}"` | Ambos comandos resuelven al mismo commit, que contiene la aceptación previa del contenido del cierre | Identificador de etiqueta, commit resuelto y referencia a la aceptación previa |
| V04 | CA04 | Revisión manual de la evidencia de retrospectiva | Retrospectiva realizada y `retrospectiva.md` disponible | Comprobar que el documento registra la retrospectiva y que `tasks.md` enlaza la evidencia con su fecha | La evidencia deja constancia de que la retrospectiva tuvo lugar | Fecha y enlace a `retrospectiva.md` registrados en `tasks.md` |
| V05 | CA01–CA04 | Revisión técnica de trazabilidad y aceptación humana final | V01–V04 satisfactorios; etiqueta verificada por V03 | Contrastar criterios, evidencias y bloqueos; registrar la decisión explícita final de DLG | Cada criterio tiene evidencia satisfactoria, no quedan bloqueos y la aceptación final consta con fecha y referencia reales | Tabla de evidencia y aceptación final en `tasks.md` |

Estas comprobaciones son una revisión documental, una verificación de Git y acciones humanas. No sustituyen la ejecución de las tareas previas ni demuestran por sí mismas la corrección del producto.

## Orden de ejecución y cierre

- Crear `tasks.md` solo después de que este plan tenga aprobación humana explícita.
- Orden propuesto: V01 como prerrequisito; completar el punto de roadmap y V02; realizar la retrospectiva y V04; registrar la aceptación previa de CA01, CA02 y CA04 en una revisión que DLG deje disponible como commit; acordar el identificador y ejecutar V03 sobre ese commit; terminar con V05 y solicitar la aceptación final de CA01–CA04.
- Si V01 identifica un punto incompleto, detener el cambio de estado y la creación de la etiqueta. El trabajo responsable debe resolver su propio bloqueo.
- Cerrar S12.8 solo cuando CA01–CA04 tengan evidencia satisfactoria, las tareas propias estén completadas y la aceptación humana final esté registrada. La aceptación previa permite identificar el commit que se etiqueta; no cierra S12.8 por sí sola.
