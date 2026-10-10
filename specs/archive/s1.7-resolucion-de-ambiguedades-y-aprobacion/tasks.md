# Tareas: S1.7 — Resolución de ambigüedades y aprobación del comportamiento

## Referencias y autorización

- Estado: aprobado por DLG el 10-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 10-X-2026 («Nada que modificar»).
  - Plan: [plan.md](plan.md), aprobado por DLG, 10-X-2026 («De acuerdo»).
  - Tareas: aprobadas por DLG, 10-X-2026 («Aprobadas»).
- Implementación:
  - Estado: completada.
  - Inicio: 10-X-2026 («Implementa, sí»).
  - Finalización: 10-X-2026; T02–T06 completadas, con 208 pruebas correctas, build sin advertencias y sin cambios en Domain.
  - Evidencia consolidada: [«Registro de decisiones»](#registro-de-decisiones) y [«Evidencia por criterio»](#evidencia-por-criterio). No es aceptación humana.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

Las tareas son secuenciales: cada decisión depende del inventario y del contraste previos. T04 es de DLG y no se ejecuta sin su decisión explícita por caso. Si una decisión cambia una regla, se detiene la tarea afectada y se tramita el cambio en el documento propietario; S1.7 no modifica código, pruebas ni fuentes por iniciativa propia.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas. | CA01–CA06; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Aprobación explícita de DLG registrada; resultado esperado: tareas aprobadas, sin alterar los estados de spec ni plan. | completada | DLG, 10-X-2026: «Aprobadas». |
| T02 | Inventariar los casos de decisión: hallazgos de [«Hallazgos para S1.7»](../s1.6-revision-de-simbolos-y-nombres-reales/tasks.md#hallazgos-para-s17) y nombres de [«Nombres reales»](../s1.6-revision-de-simbolos-y-nombres-reales/tasks.md#nombres-reales) con juicio no aceptable o ambiguo, más los puntos pendientes del [plan](plan.md#decisiones-técnicas-y-riesgos). Completar la sección «Registro de decisiones» con una fila por caso y su origen. | CA01; [plan → Flujos internos](plan.md#flujos-internos), V01 | T01 | Comparar la lista de S1.6 con las filas del registro; resultado esperado: cada caso tiene su fila y ninguno se omite. | completada | 10-X-2026: inventario de D01–D06 en [«Registro de decisiones»](#registro-de-decisiones); solo H01 y H02 figuran en los hallazgos de S1.6, y de los 24 nombres solo el n.º 21 tiene juicio distinto de Ok. |
| T03 | Contrastar cada caso con su sección fuente de [specifications.md](../../../context/specifications.md) y de [decisions.md → «Riesgos aceptados»](../../../context/decisions.md#riesgos-aceptados), y anotar si está alineado o sería un cambio de regla. | CA02, CA05; [plan → Flujos internos](plan.md#flujos-internos), V02 | T02 | Revisar la columna de sección fuente; resultado esperado: cada caso enlaza su fuente y queda marcado como alineado o como cambio de regla propuesto, sin copiar reglas. | completada | 10-X-2026: columna «Alineado con la fuente» de [«Registro de decisiones»](#registro-de-decisiones); ningún caso exige cambiar una regla de las fuentes. |
| T04 | Presentar a DLG cada caso y registrar su decisión (aceptado, ajuste de regla o fuera del alcance del MVP0), con justificación, referencia y fecha. Incluye confirmar qué lista de nombres vale como los 20 nombres validados y el rechazo de `日本語 pa mi bro`. | CA03, CA04, CA05; [plan → Flujos internos](plan.md#flujos-internos), V03 | T03 | Revisión de DLG; resultado esperado: cada caso con decisión, referencia y fecha; los cambios de regla quedan como propuesta en su documento propietario y bloquean sus tareas dependientes. | completada | DLG, 10-X-2026: decisiones D01–D06 en [«Registro de decisiones»](#registro-de-decisiones); ninguna propone cambio de regla. |
| T05 | Ejecutar las validaciones integradas: `dotnet test` y `dotnet build` de la solución, y comprobar que no hay cambios en Domain. | CA01–CA06; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V05 y V06 | T04 | Ejecutar `dotnet test Linkubator.sln --no-restore --verbosity minimal` y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal`, y `git status` y `git diff` sobre `src/Linkubator.Domain`; resultado esperado: tests correctos, build sin errores ni advertencias y sin cambios en Domain. | completada | 10-X-2026, SDK 10.0.401: `dotnet test` 208 pruebas correctas (código 0); `dotnet build -warnaserror` correcto (código 0); `git status` y `git diff` de `src/Linkubator.Domain` vacíos. |
| T06 | Comprobar que no queda ningún caso pendiente y sincronizar [roadmap.md](../../../context/roadmap.md) y [specs-index.md](../../specs-index.md) con el estado real de S1.7, sin declarar aceptación ni cerrar el sprint. | CA06; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V04 | T05 | Revisar el registro, el roadmap y el índice; resultado esperado: sin ambigüedades abiertas sin registrar y documentos coherentes con enlace a este archivo. | completada | 10-X-2026: D01–D06 con decisión y sin casos pendientes; [roadmap.md](../../../context/roadmap.md) actualizado (sin declarar aceptación ni cierre del sprint) y [specs-index.md](../../specs-index.md) ya enlaza este trabajo. |
| T07 | Revisar trazabilidad y evidencia; presentar el resultado a DLG para aceptación. | CA01–CA06; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T06 | Confirmar que cada criterio tiene evidencia o bloqueo explícito, y registrar aceptación o reparos de DLG. | completada | DLG, 10-X-2026: «Cumple lo especificado». |

## Registro de decisiones

Se completa en T02–T04. Cada fila enlaza su origen y su fuente sin copiar la regla.

| ID | Caso | Origen | Sección fuente | Alineado con la fuente | Decisión de DLG | Referencia y fecha |
| --- | --- | --- | --- | --- | --- | --- |
| D01 | H01: la anomalía inicial fue de codificación del script y la consola, no de Domain; resuelta en S1.6 | [H01](../s1.6-revision-de-simbolos-y-nombres-reales/tasks.md#hallazgos-para-s17) | [«Transformación común a ASCII»](../../../context/specifications.md#transformación-común-a-ascii) | Sí: la salida real coincide con la fuente | Cerrado, sin cambios | DLG, 10-X-2026 («Cerrados, sin cambios») |
| D02 | H02: `Napoleón Bonaparte` produce `napoleon-bonaparte` con la entrada bien codificada; sin corrección de Domain | [H02](../s1.6-revision-de-simbolos-y-nombres-reales/tasks.md#hallazgos-para-s17) | [«Generación de slugs»](../../../context/specifications.md#generación-de-slugs) | Sí | Cerrado, sin cambios | DLG, 10-X-2026 («Cerrados, sin cambios») |
| D03 | Nombre n.º 21 (`日本語 pa mi bro`): rechazado por letras no ASCII; juicio de S1.6 «Rechazado: letras no ASCII» | [«Nombres reales»](../s1.6-revision-de-simbolos-y-nombres-reales/tasks.md#nombres-reales) | [«Transformación común a ASCII»](../../../context/specifications.md#transformación-común-a-ascii) y [«Riesgos aceptados»](../../../context/decisions.md#riesgos-aceptados) | Sí: la fuente rechaza este caso y su conversión a ASCII es un riesgo aceptado | Aprobado como comportamiento del MVP0 | DLG, 10-X-2026 («Aprobado como está») |
| D04 | Lista de nombres: la evidencia de S1.6 registra 24 (la spec de S1.6 pedía 20 y el resumen de T06 dice 30); el script contiene 24, de los que los n.º 1–20 parecen nombres de colecciones y los n.º 21–24 casos de comprobación | [«Nombres reales»](../s1.6-revision-de-simbolos-y-nombres-reales/tasks.md#nombres-reales) y [script](../s1.6-revision-de-simbolos-y-nombres-reales/test-nombres-colecciones-with-BOM.ps1) | [mvp0-plan.md → «S1: Dominio I: textos, alias y slugs (L)»](../../../plans/mvp0-plan.md#s1-dominio-i-textos-alias-y-slugs-l) | No aplica: es una cuestión de evidencia, no de regla | Valen los 24 nombres registrados; la referencia a 30 en el resumen de T06 de S1.6 es un error y no se corrige la evidencia aceptada | DLG, 10-X-2026 («Valen los 24») |
| D05 | Resultados que sorprenden pero que están documentados: emoji eliminado (n.º 23) y `℃` convertido en `c` (n.º 24) | [«Nombres reales»](../s1.6-revision-de-simbolos-y-nombres-reales/tasks.md#nombres-reales) | [«Transformación común a ASCII»](../../../context/specifications.md#transformación-común-a-ascii) y [«Riesgos aceptados»](../../../context/decisions.md#riesgos-aceptados) | Sí: la fuente los prevé | Confirmados, sin cambios | DLG, 10-X-2026 («Confirmados, sin cambios») |
| D06 | Texto obsoleto en la evidencia de S1.6: [«Evidencia por criterio»](../s1.6-revision-de-simbolos-y-nombres-reales/tasks.md#evidencia-por-criterio) dice que la etapa sigue pendiente de la aceptación de DLG, ya registrada | [tasks.md de S1.6](../s1.6-revision-de-simbolos-y-nombres-reales/tasks.md#aceptación-y-cierre) | No aplica: es una cuestión de evidencia, no de regla | No aplica | Corregir la frase en la evidencia de S1.6 | DLG, 10-X-2026 («Corregir la frase en S1.6»); corregida el mismo día |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T05 | [V01](plan.md#estrategia-de-validación), [V05, V06](plan.md#estrategia-de-validación) | Cumplido: los casos de S1.6 (H01, H02 y el n.º 21) y los puntos pendientes del plan están en D01–D06, ninguno omitido. | 10-X-2026, SDK 10.0.401 | [«Registro de decisiones»](#registro-de-decisiones), T05 |
| CA02 | T03, T05 | [V02](plan.md#estrategia-de-validación), [V05, V06](plan.md#estrategia-de-validación) | Cumplido: cada caso enlaza su fuente y ninguno exige cambiar una regla. | 10-X-2026, SDK 10.0.401 | [«Registro de decisiones»](#registro-de-decisiones), T05 |
| CA03 | T04, T05 | [V03](plan.md#estrategia-de-validación), [V05, V06](plan.md#estrategia-de-validación) | Cumplido: cada caso tiene decisión, referencia y fecha de DLG. | 10-X-2026, SDK 10.0.401 | [«Registro de decisiones»](#registro-de-decisiones), T05 |
| CA04 | T04, T05 | [V03](plan.md#estrategia-de-validación), [V05, V06](plan.md#estrategia-de-validación) | Cumplido: D03 y D05 aceptados como comportamiento válido; ninguno requiere ajuste ni queda fuera del alcance. | 10-X-2026, SDK 10.0.401 | [«Registro de decisiones»](#registro-de-decisiones), T05 |
| CA05 | T03, T04, T05 | [V02, V03](plan.md#estrategia-de-validación), [V05, V06](plan.md#estrategia-de-validación) | Cumplido: DLG confirma que el riesgo de la conversión a ASCII se acepta sin cambiar la regla. | 10-X-2026, SDK 10.0.401 | [«Registro de decisiones»](#registro-de-decisiones), T05 |
| CA06 | T06, T05 | [V04](plan.md#estrategia-de-validación), [V05, V06](plan.md#estrategia-de-validación) | Cumplido: sin casos abiertos; el sprint S1 no se da por cerrado hasta la aceptación de DLG. | 10-X-2026, SDK 10.0.401 | [«Registro de decisiones»](#registro-de-decisiones), [roadmap.md](../../../context/roadmap.md), T05 |

## Aceptación y cierre

- Estado del trabajo: archivado; aceptado.
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos y riesgos restantes: ninguno. No se cambia ninguna regla, código ni prueba; la conversión a ASCII sigue siendo un riesgo aceptado por [decisions.md → «Riesgos aceptados»](../../../context/decisions.md#riesgos-aceptados).
- Aceptación humana: DLG, 10-X-2026, referencia «Cumple lo especificado».
- Archivado: completado el 10-X-2026 por orden de DLG («Archiva las specs activas»); la aceptación del resultado es la registrada arriba.
- Ubicación archivada: `specs/archive/s1.7-resolucion-de-ambiguedades-y-aprobacion/`.
- Fecha efectiva de archivado y comprobación de enlaces: 10-X-2026; los 732 enlaces locales de los documentos afectados resuelven (archivo y ancla), los tres artefactos están en destino y la carpeta activa no existe.
- Siguiente paso autorizado, sin cerrar otras etapas: continuar con la siguiente etapa del roadmap.
