# Tareas: S3.5 — Fichas de entidades

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 12-X-2026 («Comprendido»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 12-X-2026 («Adelante»).
    - Tareas: aprobadas por DLG el 12-X-2026 («Vamos»).
- Implementación:
    - Estado: completada.
    - Inicio: 12-X-2026 («Vamos»).
    - Finalización: 12-X-2026; las tres fichas se revisaron y sus citas a fuentes se comprobaron mediante números de línea.
    - Evidencia consolidada: [user.md](cards/user.md), [collection.md](cards/collection.md), [link.md](cards/link.md), más las validaciones locales de referencias.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T02–T04 son independientes por entidad y pueden ejecutarse en paralelo después de T01, siempre que cada ficha aplique el mismo procedimiento y las comprobaciones de referencias correspondientes. T05 integra el control de fuente única y la validación transversal; T06 sincroniza el estado técnico y T07 presenta la entrega para aceptación.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Confirmar las fuentes y criterios de S3.1–S3.4, iniciar la implementación solo tras la aprobación explícita de estas tareas y registrar el punto de partida documental. | CA01–CA03; [plan.md → «Flujos internos»](plan.md#flujos-internos), [«Estrategia de validación»](plan.md#estrategia-de-validación), V01 | Aprobación de estas tareas | Revisar las rutas citadas y la implementación actual de las entidades; resultado esperado: fuentes disponibles, sin cambio de reglas implícito y lista de referencias para investigar por entidad. | completado | [domain-model.md](../../../context/domain-model.md), [requirements.md](../../../context/requirements.md), [specifications.md](../../../context/specifications.md) y [plan.md](plan.md) revisados como fuentes de verdad para la generación. |
| T02 | Generar y guardar la ficha de `User` en `cards/user.md`, siguiendo el procedimiento de la skill y agrupando las reglas y referencias verificables. | CA01–CA03; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos); V01, V03 | T01 | Revisar `cards/user.md` contra las fuentes de verdad completas; resultado esperado: cobertura de las reglas localizadas, referencias `archivo#Lnn` y observaciones finales. | completado | [user.md](cards/user.md) creado y revisado contra [domain-model.md](../../../context/domain-model.md#user), [requirements.md](../../../context/requirements.md#cuenta-de-usuario) y [specifications.md](../../../context/specifications.md#cuenta). |
| T03 | Generar y guardar la ficha de `Collection` en `cards/collection.md`, siguiendo el procedimiento de la skill y agrupando las reglas y referencias verificables. | CA01–CA03; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos); V01, V03 | T01 | Revisar `cards/collection.md` contra las fuentes de verdad completas; resultado esperado: cobertura de las reglas localizadas, referencias `archivo#Lnn` y observaciones finales. | completado | [collection.md](cards/collection.md) creado y revisado contra [domain-model.md](../../../context/domain-model.md#collection), [requirements.md](../../../context/requirements.md#colecciones) y [specifications.md](../../../context/specifications.md#generación-de-slugs). |
| T04 | Generar y guardar la ficha de `Link` en `cards/link.md`, siguiendo el procedimiento de la skill y agrupando las reglas y referencias verificables. | CA01–CA03; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos); V01, V03 | T01 | Revisar `cards/link.md` contra las fuentes de verdad completas; resultado esperado: cobertura de las reglas localizadas, referencias `archivo#Lnn` y observaciones finales. | completado | [link.md](cards/link.md) creado y revisado contra [domain-model.md](../../../context/domain-model.md#link), [requirements.md](../../../context/requirements.md#enlaces) y [specifications.md](../../../context/specifications.md#normalización-para-duplicados). |
| T05 | Revisar conjuntamente las tres fichas: cobertura, rutas y líneas, fuente única, límites de S3 y hallazgos documentales. | CA01–CA03; [plan.md → «Garantías técnicas»](plan.md#garantías-técnicas), [«Estrategia de validación»](plan.md#estrategia-de-validación), V01–V03 | T02, T03, T04 | Revisar diff y referencias; resultado esperado: tres fichas completas dentro de la carpeta, sin cambios en fuentes/código y observaciones explícitas por ficha. | completado | Revisadas las referencias y la cobertura. |
| T06 | Registrar evidencia técnica y sincronizar el roadmap y el índice con el estado real de S3.5, sin declarar aceptación humana. | CA01–CA03; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T05 | Revisar las referencias entrantes y los estados; resultado esperado: roadmap e índice enlazan los artefactos existentes y distinguen el trabajo completado técnicamente de su aceptación pendiente. | completado | Roadmap e índice sincronizados con el cierre de S3.5 y el trabajo pendiente de S3.6. |
| T07 | Presentar las fichas y la evidencia a DLG y registrar aceptación o correcciones solicitadas. | CA01–CA03; [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación) | T06 | Registrar decisión explícita con responsable, fecha y referencia real; resultado esperado: aceptación o bloqueo con correcciones trazables. | completado | DLG acepta el resultado y solicita cerrar la spec el 12-X-2026 con la referencia «Fichas creadas». |

La revisión S3.6 contra el código y la aprobación de las invariantes quedan fuera de estas tareas de S3.5; la generación de las fichas no las inicia ni las declara completadas.

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01–T05 | V01 | Completado; las tres fichas resumen las reglas localizadas y citan archivo y línea. | 12-X-2026, revisión documental | [user.md](cards/user.md), [collection.md](cards/collection.md), [link.md](cards/link.md) |
| CA02 | T02–T05 | V02 | Completado; archivos guardados como vistas derivadas y fuentes propietarias no modificadas. | 12-X-2026, revisión documental en carpeta S3.5 | Carpeta [S3.5](.) y archivos derivados |
| CA03 | T02–T05 | V03 | Completado; observaciones documentales declaradas y referencias comprobadas. | 12-X-2026; revisión de vigencia 15-X-2026 | [user.md](cards/user.md), [collection.md](cards/collection.md#observaciones-finales), [link.md](cards/link.md#observaciones-finales) |

## Aceptación y cierre

- Estado del trabajo: implementación documental completada y aceptada; spec cerrada, no archivada.
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos y riesgos restantes: ninguno respecto a la observación sobre equivalencia de nombres de colecciones. La revisión y aprobación del modelo frente al código siguen siendo S3.6.
- Aceptación humana: DLG, 12-X-2026, referencia «Fichas creadas».
- Archivado: sí, trasladado el 12-X-2026; la revalidación documental posterior no cambia esa fecha ni reabre el cierre.
- Ubicación archivada: carpeta actual `specs/archive/s3.5-fichas-entidades/`.
- Fecha efectiva de archivado y comprobación de enlaces: 12-X-2026; artefactos presentes y enlaces internos recalculados.
- Siguiente paso autorizado: S3.6 puede revisar las fichas contra el código y resolver la aprobación de las invariantes; S3.5 queda cerrada y no archivada.