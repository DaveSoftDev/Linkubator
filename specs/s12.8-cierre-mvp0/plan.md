# Plan técnico: S12.8 — Cierre del MVP0

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 31-X-2026 con la referencia «Siguiente».
  - Plan: aprobado por DLG el 31-X-2026 con la referencia «Siguiente».

## Diseño de implementación

### Componentes y contratos

- No se crearán ni modificarán componentes de producto. El trabajo es un cierre humano sustentado en la [Definition of Done de S12](../../plans/mvp0-plan.md#s12-cierre-y-hardening-m) y en la evidencia que hayan registrado las tareas anteriores.
- La evidencia final del cierre se consolidará en el futuro `tasks.md` de S12.8, que se creará solo después de aprobar este plan.
- Los entregables son la actualización del estado del MVP0 en el roadmap, una etiqueta de versión sobre la revisión que contiene el cierre y el registro de la retrospectiva final.

### Flujos internos

1. Contrastar cada punto de la Definition of Done de S12, excepto el punto de actualizar el roadmap, con la evidencia de las tareas responsables. Si falta evidencia o queda un requisito previo pendiente, detener el cierre y registrar el bloqueo.
2. Actualizar el roadmap para reflejar que el MVP0 está en «Hecho» y que S12 ya no es la etapa actual; marcar como completado en `mvp0-plan.md` el punto correspondiente de la Definition of Done.
3. Acordar con DLG el identificador de versión antes de crear la etiqueta. Crear la etiqueta solo sobre la revisión que incluya el cierre aceptado y registrar su nombre y destino.
4. Realizar la retrospectiva final y registrar en la evidencia de cierre que tuvo lugar.
5. Revisar la trazabilidad de CA01–CA04 y solicitar la aceptación humana explícita del resultado.

### Persistencia y dependencias

- No hay cambios en persistencia de aplicación, transacciones, contratos de código ni dependencias de software.
- El prerrequisito es que los puntos anteriores de la Definition of Done estén completados y respaldados por la evidencia de sus tareas responsables. S12.8 no sustituye ni completa sus resultados.
- La etiqueta requiere un identificador acordado y una revisión que ya contenga el cierre; no se presupone que una etiqueta o revisión específica exista.

### Garantías técnicas

- No se mueve el MVP0 a «Hecho» si falta algún prerrequisito o evidencia de cierre.
- La etiqueta apunta a la revisión que contiene el cierre aceptado; el nombre y la revisión quedan registrados para su verificación.
- La retrospectiva y la aceptación del resultado se registran como acciones distintas; ninguna aceptación se infiere de la ejecución de los pasos.
- El plan no cambia las reglas del producto ni modifica código, tests o datos de aplicación.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Ejecutar el cierre como revisión humana de la Definition of Done y sus evidencias, sin cambios de producto. | Confirmado | [spec.md → «Alcance»](spec.md#alcance) | Los puntos previos incompletos bloquean el cierre; S12.8 no los repara ni los acepta en nombre de sus responsables. |
| Registrar la evidencia de roadmap, etiqueta, retrospectiva y aceptación en las tareas de S12.8. | Confirmado | [spec.md → CA01–CA04](spec.md#criterios-de-aceptación) | Mantiene una evidencia trazable sin crear un informe independiente. |
| Identificador concreto de la etiqueta de versión. | Pendiente | [spec.md → «Dependencias y prerrequisitos»](spec.md#dependencias-y-prerrequisitos) | DLG debe acordarlo antes de crear la etiqueta; mientras tanto, CA03 no puede validarse. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Revisión manual de los puntos de la Definition of Done anteriores a S12.8 y sus evidencias | Evidencia de las tareas responsables disponible | Contrastar cada punto aplicable de la [Definition of Done de S12](../../plans/mvp0-plan.md#s12-cierre-y-hardening-m) con su evidencia enlazada | Todos los prerrequisitos están satisfechos; cualquier ausencia queda registrada como bloqueo y no se cierra S12.8 | Lista de puntos revisados, enlaces a evidencia y bloqueos, si los hay |
| V02 | CA02 | Revisión documental del estado final del roadmap y del plan | V01 satisfactorio | Revisar el estado de «Hecho», el bloque «Now» y el último punto de la Definition of Done en `mvp0-plan.md` | El MVP0 figura como terminado, S12 ya no aparece como etapa actual y el último punto de la Definition of Done está marcado como completado con evidencia | Referencias a las secciones actualizadas y evidencia del cierre |
| V03 | CA03 | Verificación técnica de la etiqueta Git | Identificador acordado y revisión del cierre disponible | Comparar `git rev-parse --verify "<etiqueta>^{commit}"` con `git rev-parse "<revision-cierre>^{commit}"` | Ambos comandos resuelven al mismo commit, que contiene el cierre aceptado | Identificador de etiqueta, commit resuelto y referencia a la revisión de cierre |
| V04 | CA04 | Revisión manual de la evidencia de retrospectiva | Retrospectiva realizada | Comprobar el registro de realización en las tareas de S12.8 | La evidencia deja constancia de que la retrospectiva tuvo lugar | Fecha y referencia o nota de evidencia |
| V05 | CA01–CA04 | Revisión técnica de trazabilidad y aceptación humana | V01–V04 satisfactorios | Contrastar criterios, evidencias y bloqueos; registrar la decisión explícita de DLG | Cada criterio tiene evidencia satisfactoria, no quedan bloqueos y la aceptación consta con fecha y referencia reales | Tabla de evidencia y aceptación en `tasks.md` |

Estas comprobaciones son una revisión documental, una verificación de Git y acciones humanas. No sustituyen la ejecución de las tareas previas ni demuestran por sí mismas la corrección del producto.

## Orden de ejecución y cierre

- Crear `tasks.md` solo después de que este plan tenga aprobación humana explícita.
- Orden propuesto: V01 como prerrequisito; completar el punto de roadmap y V02; acordar el identificador y ejecutar V03 sobre la revisión de cierre; realizar la retrospectiva y V04; terminar con V05 y solicitar aceptación.
- Si V01 identifica un punto incompleto, detener el cambio de estado y la creación de la etiqueta. El trabajo responsable debe resolver su propio bloqueo.
- Cerrar S12.8 solo cuando CA01–CA04 tengan evidencia satisfactoria, las tareas propias estén completadas y la aceptación humana esté registrada. La aprobación del plan o la finalización de la retrospectiva no cierran por sí solas S12.
