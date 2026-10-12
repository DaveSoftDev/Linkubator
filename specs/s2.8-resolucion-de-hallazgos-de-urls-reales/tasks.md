# Tareas: S2.8 — Resolución de hallazgos de URLs reales

## Referencias y autorización

- Estado: aprobado por DLG el 12-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 12-X-2026 («Aprobada»).
  - Plan: [plan.md](plan.md), aprobado por DLG, 12-X-2026 («Listo»).
  - Tareas: aprobadas por DLG, 12-X-2026 («Vamos»).
- Implementación:
  - Estado: completada el 12-X-2026 («Solucionado»).
  - Inicio: 12-X-2026, tras la aprobación de tareas por DLG («Vamos»).
  - Comprobación: tests de normalización 17/17; suite completa 436/436; build con `-warnaserror` correcto.
  - Evidencia consolidada: [Evidencia por criterio](#evidencia-por-criterio).

## Descomposición

T03 y T06 son independientes tras T02 y pueden avanzar en paralelo: T03 prepara y verifica H01, mientras T06 registra H03 por referencia, sin cambiar la fuente ni el código. T04 depende de T03. T05 depende de T02 y T04; su salida determina si hay corrección condicional de Domain o solo evidencia del problema de captura. T07 integra ambas ramas. T01 registró la aprobación de DLG antes de iniciar T02; la implementación y validación técnica constan completadas y la aceptación humana sigue pendiente.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas. | CA01–CA06; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Aprobación explícita de DLG registrada; resultado esperado: tareas aprobadas, sin alterar los estados de spec ni plan. | completada | DLG aprobó las tareas el 12-X-2026 («Vamos»). |
| T02 | Comprobar SDK, runner y filtro; ejecutar la línea base de normalización y registrar el inicio de implementación en el roadmap. | CA05; [plan → Decisiones técnicas y riesgos](plan.md#decisiones-técnicas-y-riesgos), V01 | T01 | Ejecutar `dotnet test tests/Linkubator.Tests --no-restore --filter "FullyQualifiedName~UrlNormalization"`; resultado esperado: filtro válido y tests actuales pasan. El roadmap refleja inicio solo tras aprobar las tareas. | completada | 12-X-2026: filtro reconocido, 16 tests correctos y 0 fallos; Windows, .NET 10.0.12, xUnit VSTest Adapter 3.1.4. Ejecución anterior a la ampliación de cobertura H01/H02. |
| T03 | Añadir `gclsrc` a la lista fuente y actualizar la expectativa del test de la URL real de la fila 6, sin modificar Domain. | CA01; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V02 | T02 | Revisar el diff y ejecutar el test de la fila 6; resultado esperado: la fuente lista `gclsrc` y el test falla porque Domain aún lo incorpora a la clave. | completada | La fuente y Domain incluyen `gclsrc`; se añadió el caso real de MediaMarkt con expectativa sin parámetros de marketing. No se registró la comprobación en rojo previa porque Domain ya contenía el cambio al comenzar esta validación; el caso final pasa en T07. |
| T04 | Añadir `gclsrc` al conjunto de parámetros de marketing de `UrlNormalizationPolicy`. | CA01; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V03 | T03 | Repetir el test de la URL de la fila 6; resultado esperado: pasa y el parámetro ya no contribuye a la clave. | completada | 12-X-2026: caso de regresión H01 incluido en la ejecución focalizada; 17 tests correctos, 0 fallos. |
| T05 | Añadir el test H02 que compara la URL percent-encoded de la fila 25 con la misma ruta Unicode y con la clave esperada derivada de la fuente; determinar en qué capa se introduce el mojibake. Corregir Domain solo si el test demuestra divergencia. | CA02, CA03; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V04 | T02, T04 | Ejecutar el test, inspeccionando el valor en memoria; resultado esperado: ambas entradas producen la clave de la fuente o, si falla, corrección acotada en Domain y test en verde. Si pasa sin cambio productivo, registrar diagnóstico de captura/representación. | completada | 12-X-2026: Amazon #25 y su ruta Unicode equivalente producen en memoria la clave japonesa esperada. No se modificó la semántica Unicode de Domain; el mojibake reportado en S2.7 no lo devuelve la política. |
| T06 | Cerrar H03 en la evidencia de S2.8 enlazando la regla del puerto por defecto, su ejemplo y la lista de parámetros de marketing. | CA04; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V05 | T02 | Derivar las tres claves descritas en el plan; resultado esperado: misma clave por reglas existentes, sin nueva decisión ni cambio en `decisions.md`, la fuente o Domain. | completada | Las tres URLs producen `fotokesaffy2336`: se descarta el puerto HTTPS predeterminado y se elimina `utm_source` según la fuente. Cierre por referencia, sin cambio de regla ni decisión. |
| T07 | Ejecutar los tests de normalización, la suite completa y el build; revisar que los cambios productivos se limitan a `UrlNormalizationPolicy` y, solo si procede por H02, a la corrección demostrada. | CA05; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V06 y V07 | T04, T05, T06 | Ejecutar `dotnet test tests/Linkubator.Tests --no-restore --filter "FullyQualifiedName~UrlNormalization"`, `dotnet test Linkubator.sln --no-restore --verbosity minimal` y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal`; resultado esperado: tests pasan, build sin errores ni warnings y diff acotado. | completada | 12-X-2026: normalización 17/17; suite completa 436/436; build con `-warnaserror` correcto, sin warnings de compilación. La suite registra un warning de runtime sobre el puerto HTTPS para redirect. |
| T08 | Revisar cobertura H01–H03, actualizar el roadmap al completar implementación/validación y presentar evidencia a DLG para aceptación. | CA06; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V08 | T07 | Comprobar la tabla de cobertura y la referencia de aceptación; resultado esperado: H01 resuelto, H02 diagnosticado y resuelto según resultado, H03 cerrado por referencia; roadmap refleja la evidencia sin declarar aceptación antes de la decisión de DLG. | completada | 12-X-2026: cobertura y roadmap sincronizados. Evidencia disponible para DLG; aceptación humana pendiente. |

## Resolución de hallazgos

La referencia de H01 y H02 es la evidencia de S2.7 y las decisiones registradas en esta tabla de trabajo; no se inventan citas literales. H03 no es una decisión nueva y se cierra por referencia, sin una nueva entrada en `decisions.md`.

| ID | Hallazgo y fuente | Resolución prevista | Evidencia | Estado |
| --- | --- | --- | --- | --- |
| H01 | [`gclsrc`, fila 6 de S2.7](../s2.7-revision-con-urls-reales/tasks.md#urls-reales) | Añadido a la lista propietaria y a Domain; el test real fija una clave sin parámetros de marketing. | [CA01](#evidencia-por-criterio), [T03–T04](#descomposición) | resuelto |
| H02 | [Mojibake reportado, fila 25 de S2.7](../s2.7-revision-con-urls-reales/tasks.md#hallazgos-para-s28) | La URL percent-encoded y su ruta Unicode equivalente producen la salida japonesa esperada en memoria; el mojibake previo se atribuye a captura/representación, sin alterar Domain. | [CA02–CA03](#evidencia-por-criterio), [T05](#descomposición) | resuelto |
| H03 | [Filas 9 y 10 de S2.7](../s2.7-revision-con-urls-reales/tasks.md#colisiones) | Cerrado por referencia a la equivalencia del puerto HTTPS predeterminado y la eliminación de parámetros de marketing ya especificadas. | [CA04](#evidencia-por-criterio), [T06](#descomposición) | cerrado por referencia |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T03, T04 | [V02, V03](plan.md#estrategia-de-validación) | Conforme | 12-X-2026; Windows; .NET 10.0.12; xUnit VSTest Adapter 3.1.4. | Fuente y Domain eliminan `gclsrc`; el test real de MediaMarkt pasó en los 17 tests focalizados. La comprobación negativa previa al cambio productivo no quedó registrada porque este ya existía al iniciar la validación. |
| CA02 | T05 | [V04](plan.md#estrategia-de-validación) | Conforme | 12-X-2026; Windows; .NET 10.0.12; xUnit VSTest Adapter 3.1.4. | Amazon #25 y su ruta Unicode equivalente producen la misma clave japonesa esperada; test focalizado correcto. |
| CA03 | T05 | [V04](plan.md#estrategia-de-validación) | Conforme | 12-X-2026; ejecución de tests .NET 10.0.12. | La salida en memoria conserva japonés y coincide con la fuente; no se modificó la regla Unicode. |
| CA04 | T06 | [V05](plan.md#estrategia-de-validación) | Conforme por derivación de la regla | 12-X-2026; revisión de las URLs y reglas fuente. | Las tres variantes producen `fotokesaffy2336`; no requiere decisión ni cambio de código. |
| CA05 | T02, T04, T05, T07 | [V01, V06, V07](plan.md#estrategia-de-validación) | Conforme | 12-X-2026; Windows; .NET 10.0.12; xUnit VSTest Adapter 3.1.4. | Filtro focalizado: 17/17. Suite completa: 436/436. Build con `-warnaserror`: correcto, sin warnings de compilación. |
| CA06 | T08 | [V08](plan.md#estrategia-de-validación) | Conforme técnicamente; aceptación pendiente | 12-X-2026. | H01 resuelto; H02 diagnosticado; H03 cerrado por referencia. Evidencia disponible para aceptación humana de DLG. |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no se registran resultados esperados como reales.

## Aceptación y cierre

- Estado de implementación y validación: completadas el 12-X-2026 («Solucionado»).
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos técnicos restantes: ninguno.
- Aceptación humana: pendiente; responsable, fecha y referencia cuando exista.
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso: aceptación humana del resultado por DLG. La implementación completada no cierra ni archiva el trabajo por sí sola.