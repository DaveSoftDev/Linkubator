# Plan técnico: S1.7 — Resolución de ambigüedades y aprobación del comportamiento

## Estado y aprobación del plan

- Estado: aprobado por DLG el 10-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 10-X-2026 («Nada que modificar»).
  - Plan: aprobado por DLG, 10-X-2026 («De acuerdo»).

## Diseño de implementación

### Componentes y contratos

- **Sin código de producción ni pruebas nuevas.** S1.7 es una tarea humana de decisión: no modifica `Linkubator.Domain` ni `Linkubator.Tests`. El comportamiento revisado es el de [AsciiTransformationPolicy.cs](../../../src/Linkubator.Domain/Policies/AsciiTransformationPolicy.cs) y [SlugPolicy.cs](../../../src/Linkubator.Domain/Policies/SlugPolicy.cs), aceptado en S1.4 y S1.5 y revisado en S1.6.
- **Entrada de la decisión.** La sección [«Hallazgos para S1.7»](../s1.6-revision-de-simbolos-y-nombres-reales/tasks.md#hallazgos-para-s17) de S1.6 y los resultados y juicios de [«Nombres reales»](../s1.6-revision-de-simbolos-y-nombres-reales/tasks.md#nombres-reales). No se repiten ni se vuelven a ejecutar los nombres ya juzgados.
- **Registro de decisiones.** `tasks.md` recogerá una tabla con un hallazgo o caso ambiguo por fila: clasificación (aceptado, rechazado o pendiente), decisión de DLG con su referencia y fecha, y destino (sin cambio, cambio de regla en su documento propietario, o fuera del alcance del MVP0 con su registro en [decisions.md](../../../context/decisions.md)).
- **Reglas que no cambian por defecto.** Si DLG confirma que no hay nada que modificar, la fuente y el código quedan como están y la decisión se registra sin editar [specifications.md](../../../context/specifications.md).
- **Si una decisión cambia una regla.** El cambio se tramita en su documento propietario y, una vez aprobado, requeriría una spec nueva o la reapertura de S1.4 y S1.5 para el código y las pruebas; S1.7 no los modifica.

### Flujos internos

1. Listar los hallazgos de S1.6 y los casos de [«Nombres reales»](../s1.6-revision-de-simbolos-y-nombres-reales/tasks.md#nombres-reales) cuyo juicio no sea aceptable o cuyo resultado sea ambiguo, sin omitir ninguno.
2. Presentar a DLG cada caso con su sección fuente y su resultado actual, y registrar su decisión.
3. Para cada decisión que implique cambiar una regla, detener la tarea afectada y proponer el cambio en el documento propietario; no editar reglas por iniciativa propia.
4. Contrastar con [decisions.md → «Riesgos aceptados»](../../../context/decisions.md#riesgos-aceptados) los casos que ya estén aceptados como riesgo y dejar constancia de ello sin duplicar el texto.
5. Comprobar que todos los hallazgos de S1.6 tienen decisión y que no quedan ambigüedades abiertas sin registrar.
6. Sincronizar [roadmap.md](../../../context/roadmap.md) con el estado real de S1.7 y, tras la aceptación de DLG, con el cierre del sprint S1 según su Definition of Done.

### Persistencia y dependencias

- No hay persistencia, transacciones, servicios externos ni paquetes NuGet nuevos.
- Dependen de este trabajo las decisiones de S1.6 ya registradas. Las colisiones, la persistencia y la integración en entidades siguen siendo de S3–S5.
- La [Definition of Done de S1](../../../plans/mvp0-plan.md#s1-dominio-i-textos-alias-y-slugs-l) se marca en el plan solo cuando exista la aceptación humana de DLG; ese plan no se edita desde este trabajo salvo que DLG lo indique.

### Garantías técnicas

- Cada decisión enlaza su fuente propietaria sin copiar reglas, valores ni mensajes.
- La decisión y su referencia las aporta DLG; no se infiere ninguna aprobación a partir de la ausencia de objeciones.
- Aislamiento de Domain según [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura): sin cambios en su código ni en sus dependencias.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| S1.7 no añade código, pruebas ni reglas por sí misma | Confirmada | [spec.md → «Alcance»](spec.md#alcance) | Una decisión que cambie el comportamiento requiere tramitarse en la fuente propietaria y en una spec de implementación aparte. |
| Los hallazgos H01 y H02 de S1.6 constan como resueltos en validación | Propuesta | [«Hallazgos para S1.7»](../s1.6-revision-de-simbolos-y-nombres-reales/tasks.md#hallazgos-para-s17) | DLG confirma que no requieren más decisión o los reabre; no se asume su cierre. |
| La evidencia de S1.6 recoge 24 nombres (la spec de S1.6 pedía 20) y el resumen de T06 menciona 30 | Pendiente | [«Nombres reales»](../s1.6-revision-de-simbolos-y-nombres-reales/tasks.md#nombres-reales) | DLG confirma qué lista vale como los «20 nombres reales validados» de la Definition of Done de S1; la discrepancia se registra, no se corrige en la evidencia de S1.6 aceptada. |
| El caso `日本語 pa mi bro` se rechaza por letras no ASCII | Propuesta | [decisions.md → «Riesgos aceptados»](../../../context/decisions.md#riesgos-aceptados) | DLG confirma que el rechazo es el comportamiento aprobado para el MVP0 o lo propone para otra etapa. |
| El roadmap describe S1.6 como abierta a la aceptación humana, aunque sus tareas registran la aceptación de DLG del 10-X-2026 | Pendiente | [roadmap.md → «Dominio y aplicación»](../../../context/roadmap.md#dominio-y-aplicación); [tasks.md → «Aceptación y cierre»](../s1.6-revision-de-simbolos-y-nombres-reales/tasks.md#aceptación-y-cierre) | Se sincroniza el roadmap en una tarea documental, con la referencia real de las tareas de S1.6. |

## Estrategia de validación

Las comprobaciones V01–V04 son revisión documental y humana; V05 y V06 reutilizan los comandos de la solución usados en S1.4–S1.6.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Revisión técnica: inventario de casos | Evidencia de S1.6 accesible | Comparar la lista de hallazgos y de nombres no aceptables o ambiguos de S1.6 con las filas del registro de decisiones | Cada caso de S1.6 tiene su fila y ninguno se omite | Tabla de decisiones con identificadores de origen. |
| V02 | CA02, CA05 | Revisión técnica: contraste con la fuente | V01 | Enlazar cada decisión con su sección de [specifications.md](../../../context/specifications.md) o de [decisions.md → «Riesgos aceptados»](../../../context/decisions.md#riesgos-aceptados) | Cada decisión está alineada con la fuente o figura como cambio de regla propuesto | Columna de sección fuente y destino de cada decisión. |
| V03 | CA03, CA04 | Revisión humana | V01, V02 | DLG decide cada caso (aceptado, ajuste de regla o fuera del alcance del MVP0) | Cada caso tiene decisión, justificación, referencia y fecha de DLG | Tabla de decisiones con la referencia de DLG. |
| V04 | CA06 | Revisión técnica y humana: cierre | V03 | Comprobar que no queda ningún caso pendiente y que el roadmap refleja el estado real | Sin ambigüedades abiertas sin registrar; roadmap sincronizado sin cerrar el sprint antes de la aceptación | Revisión del registro y del roadmap. |
| V05 | CA01–CA06 | Automatizada integrada, si no hay cambios de código basta confirmar su ausencia | Dependencias restauradas | `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Todos los tests pasan | Comando, código de salida y resumen. |
| V06 | CA01–CA06 | Build y revisión técnica: sin cambios de producción | V05 | `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal`, y `git status` y `git diff` de `src/Linkubator.Domain` | Build sin errores ni advertencias y sin cambios en Domain | Comandos, SDK, código de salida y salida de `git status`. |

V01–V04 no demuestran que el comportamiento sea correcto, solo que cada caso conocido tiene una decisión humana trazable. V05 y V06 no sustituyen la decisión de DLG. No se planifica una comprobación negativa de sensibilidad porque no se añaden pruebas ni reglas automáticas.

## Orden de ejecución y cierre

- Tareas: crear `tasks.md` tras aprobar este plan; contendrá el inventario de casos, el registro de decisiones con DLG, el contraste con las fuentes, las validaciones integradas, la sincronización del roadmap y la aceptación, con dependencias y checks inmediatos.
- S1.7 se cerrará cuando CA01–CA06 tengan la evidencia indicada, no queden bloqueos y DLG acepte el resultado. La Definition of Done del sprint S1 se marca solo con esa aceptación.
- Ante un cambio de regla, detener la tarea afectada y tramitar su aprobación en la fuente propietaria antes de continuar.
