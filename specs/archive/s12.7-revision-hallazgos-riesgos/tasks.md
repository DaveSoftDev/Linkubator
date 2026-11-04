# Tareas: S12.7 — Revisión de hallazgos de auditoría y aceptación de riesgos

## Referencias y autorización

- Estado: aprobadas.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 31-X-2026 con la referencia «Siguiente».
  - Plan: [plan.md](plan.md), aprobado por DLG el 31-X-2026 con la referencia «Siguiente».
  - Tareas: aprobadas por DLG el 31-X-2026 con la referencia «Vamos».
- Revisión:
    - Estado: completada y aceptada por DLG el 02-XI-2026 con la referencia «Acepto S12.7».
    - Inicio: 02-XI-2026, por solicitud de DLG en esta conversación: «Vamos comprobar que S12.7 está completado».
    - Finalización: 02-XI-2026; T01–T05 completadas, revisiones V01–V04 registradas y aceptación humana explícita.
    - Evidencia consolidada: [inventario y disposiciones](#inventario-de-revisión-t01), [evidencia por criterio](#evidencia-por-criterio) y [aceptación y cierre](#aceptación-y-cierre).
- La preparación de estas tareas no inventa aprobaciones ni evidencia de revisión.
- La revisión comenzó el 02-XI-2026 por solicitud de DLG en esta conversación: «Vamos comprobar que S12.7 está completado».

## Descomposición

Las tareas son secuenciales: T01 prepara el inventario de elementos a decidir; T02 revisa las conclusiones de auditoría; T03 registra por separado las decisiones sobre riesgos; T04 verifica y consolida la trazabilidad; T05 queda reservada a la aceptación humana. Ninguna tarea modifica código, SQL o pruebas.

### Inventario de revisión (T01)

| Criterio | Conclusión, elemento no aplicable o límite inventariado | Fuente | Disposición S12.7 |
| --- | --- | --- | --- |
| CA01 | Sin incumplimiento nuevo observado en propiedad de datos; se usa una identidad privada de desarrollo compartida, riesgo previamente aceptado. | [Informe S12.3 → CA01](../s12.3-auditoria-invariantes-criticas/tasks.md#ca01--propiedad-de-los-datos) | Conclusión confirmada el 02-XI-2026: al alterar el querystring se mantuvo la misma identidad. DLG confirma mantener la aceptación previa hasta LoginV0 y revaluar con la identidad autenticada. |
| CA02 | Sin exposición observada en los caminos revisados; SQL no impone la relación entre privacidad de enlace y colección. | [Informe S12.3 → CA02](../s12.3-auditoria-invariantes-criticas/tasks.md#ca02--público-y-privado) | Conclusión confirmada el 02-XI-2026: no se mezcla lo público y lo privado en los caminos revisados. DLG acepta el límite SQL para MVP0, con los controles actuales de aplicación. |
| CA03 | No aplicable al código actual: no existen flujos de login, registro, recuperación ni cambio de email; su ausencia no demuestra cumplimiento. | [Informe S12.3 → CA03](../s12.3-auditoria-invariantes-criticas/tasks.md#ca03--respuestas-de-operaciones-de-cuenta) | Clasificación confirmada el 02-XI-2026; DLG no conoce operaciones de cuenta implementadas. |
| CA04 | Sin exposición actual observada; el informe identifica dos riesgos preventivos de logging de credenciales y de falta de validación del formato del hash, sin flujos actuales de cuenta que los activen. | [Informe S12.3 → CA04](../s12.3-auditoria-invariantes-criticas/tasks.md#ca04--contraseñas-tokens-y-logs) | DLG indica que no hay riesgos activos en MVP0. Logging: seguimiento para verificar diagnóstico sin credenciales en claro antes de LoginV0. Hash: seguimiento para reevaluarlo al implementar LoginV0/MVP1. |
| CA05 | Sin llamadas HTTP, scraping, correo ni esperas al usuario dentro de las transacciones revisadas; scraping y correo no están implementados. | [Informe S12.3 → CA05](../s12.3-auditoria-invariantes-criticas/tasks.md#ca05--límites-transaccionales) | Confirmado el 02-XI-2026; DLG no ha visto discrepancias en esta revisión del informe. |
| CA06 | Resumen consolidado: CA01, CA02 y CA05 sin incumplimientos observados; CA03 no aplicable; CA04 sin exposición actual observada. CA01 mantiene el riesgo aceptado de identidad compartida hasta LoginV0; CA02 se acepta para MVP0; los puntos preventivos de CA04 quedan con seguimiento para LoginV0/MVP1. | [Informe S12.3 → CA06](../s12.3-auditoria-invariantes-criticas/tasks.md#ca06--resultado-consolidado) | Resumen confirmado por DLG el 02-XI-2026; no equivale a aceptación de S12.7. |

### Disposiciones de riesgos (T03)

| Riesgo | Disposición | Estado y evidencia |
| --- | --- | --- |
| Identidad de desarrollo compartida | Mantener la aceptación previa hasta LoginV0 y revaluar con la identidad autenticada. | Confirmado por DLG el 02-XI-2026; [decisions.md → «Identidad de desarrollo»](../../../context/decisions.md#identidad-de-desarrollo). |
| SQL no impone la relación entre privacidad de enlace y colección | Aceptar este límite para MVP0 con los controles actuales de aplicación. | Confirmado por DLG el 02-XI-2026; [informe S12.3 → CA02](../s12.3-auditoria-invariantes-criticas/tasks.md#ca02--público-y-privado). |
| Logging de credenciales | Seguimiento antes de LoginV0 para verificar diagnóstico sin credenciales en claro; no se acepta registrar contraseñas ni tokens en claro. | Confirmado por DLG el 02-XI-2026; respeta [AGENTS.md → «Invariantes críticas»](../../../AGENTS.md#invariantes-críticas) y [informe S12.3 → CA04](../s12.3-auditoria-invariantes-criticas/tasks.md#ca04--contraseñas-tokens-y-logs). |
| Validación del formato del hash | Seguimiento para reevaluarlo al implementar LoginV0/MVP1, sin afirmar que dominio/SQL ya validen el formato. | Confirmado por DLG el 02-XI-2026; [informe S12.3 → CA04](../s12.3-auditoria-invariantes-criticas/tasks.md#ca04--contraseñas-tokens-y-logs). |

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Al iniciar la revisión autorizada, registrar fecha y referencia reales en este resumen y sincronizar el avance de S12.7 en el roadmap. Preparar una matriz que enumere cada conclusión CA01–CA06, los elementos no aplicables y los límites del informe aceptado de S12.3. | CA01–CA04; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos) | Ninguna | Comparar la matriz con el informe completo de S12.3. Resultado: cada conclusión, límite y elemento no aplicable aparece una vez, con referencia a su evidencia fuente. | completada | Revisión iniciada el 02-XI-2026 por la solicitud citada en «Referencias y autorización»; inventario CA01–CA06 contrastado con el [informe S12.3](../s12.3-auditoria-invariantes-criticas/tasks.md#informe-de-auditoría). |
| T02 | Revisar las conclusiones y límites inventariados, decidir su disposición y registrarla con contexto y referencia. Distinguir los elementos no aplicables de los hallazgos y de las garantías observadas. | CA01, CA04; V01 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | T01 | Contrastar cada elemento de la matriz contra la decisión registrada. Resultado: ningún elemento queda sin revisar y ninguno no aplicable se presenta como cumplimiento demostrado. | completada | CA01–CA06 contrastadas con el informe; CA03 clasificada como no aplicable, no como cumplimiento. CA06 confirmada por DLG el 02-XI-2026; las disposiciones individuales constan en T03. |
| T03 | Revisar por separado el riesgo de identidad de desarrollo, el límite de la garantía público/privado en SQL y los riesgos preventivos de logging de credenciales y validación del formato del hash. Registrar para cada uno la aceptación explícita o el seguimiento/mitigación pendiente. | CA02, CA03, CA04; V02 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | T02 | Confirmar que cada riesgo tiene una disposición propia, con contexto y alcance o con la acción de seguimiento conocida. Resultado: no hay aceptación implícita ni mitigaciones pendientes presentadas como ejecutadas. | completada | Las cuatro disposiciones están registradas por separado arriba: identidad compartida mantenida hasta LoginV0; límite SQL aceptado para MVP0; logging y formato del hash con seguimiento en LoginV0/MVP1. |
| T04 | Verificar la trazabilidad entre criterios, conclusiones, decisiones y evidencia; consolidar pendientes y actualizar el estado de revisión técnica en estas tareas y en el roadmap sin anticipar aceptación humana. | CA01–CA04; V03 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | T02, T03 | Revisar cada CA contra su evidencia y comprobar que el roadmap no declara aceptada la revisión ni cerrada S12. Resultado: registro completo y estado sincronizado con hechos comprobados. | completada | Trazabilidad de CA01–CA06 y las cuatro disposiciones revisada; CA03 permanece como no aplicable; los seguimientos de CA04 no se presentan como mitigaciones ejecutadas. Roadmap sincronizado: T01–T04 completadas, T05/aceptación pendiente y S12 abierta. |
| T05 | Presentar el registro consolidado a DLG y registrar la aceptación explícita del resultado de S12.7, con responsable, fecha y referencia reales; actualizar el roadmap tras la decisión. Si se solicitan cambios, registrar el bloqueo y las correcciones necesarias, sin registrar aceptación. | CA05; V04 de [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | T04 | Comprobar la referencia de la decisión humana y que la aceptación del trabajo está diferenciada de las disposiciones individuales de riesgos y del cierre de S12. Resultado: aceptación explícita registrada, o bloqueo documentado sin cierre. | completada | DLG aceptó S12.7 el 02-XI-2026 con la referencia «Acepto S12.7»; las disposiciones de riesgos siguen diferenciadas de la aceptación del trabajo y del cierre de S12. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T02, T04 | V01, V03 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | La conclusión de propiedad de datos coincide con la observación: al alterar el querystring se mantuvo la misma identidad. DLG mantiene la aceptación previa del riesgo compartido hasta LoginV0; se revaluará con la identidad autenticada. | 02-XI-2026; revisión manual | Confirmación de DLG en esta conversación; [informe S12.3 → CA01](../s12.3-auditoria-invariantes-criticas/tasks.md#ca01--propiedad-de-los-datos) y [decisions.md → «Identidad de desarrollo»](../../../context/decisions.md#identidad-de-desarrollo) |
| CA02 | T01, T03, T04 | V02, V03 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | La conclusión de privacidad coincide con lo revisado: no se mezclan contenidos públicos y privados en los caminos auditados. DLG acepta el límite de SQL para MVP0, con los controles actuales de aplicación. | 02-XI-2026; revisión manual | Disposición explícita de DLG en esta conversación; [informe S12.3 → CA02](../s12.3-auditoria-invariantes-criticas/tasks.md#ca02--público-y-privado) |
| CA03 | T01, T03, T04 | V02, V03 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Confirmada como no aplicable: DLG no conoce operaciones de cuenta implementadas; esto no acredita el cumplimiento de flujos futuros. | 02-XI-2026; revisión manual | Confirmación de DLG en esta conversación; [informe S12.3 → CA03](../s12.3-auditoria-invariantes-criticas/tasks.md#ca03--respuestas-de-operaciones-de-cuenta) |
| CA04 | T02, T03, T04 | V01–V03 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | DLG indica que no hay riesgos activos en MVP0. Logging: seguimiento para verificar diagnóstico sin credenciales en claro antes de LoginV0. Hash: seguimiento al implementar LoginV0/MVP1. | 02-XI-2026; revisión manual | Confirmación de DLG en esta conversación; [informe S12.3 → CA04](../s12.3-auditoria-invariantes-criticas/tasks.md#ca04--contraseñas-tokens-y-logs) y [AGENTS.md → «Invariantes críticas»](../../../AGENTS.md#invariantes-críticas) |
| CA05 | T05 | V04 en [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación) | Aceptación explícita de DLG del resultado de S12.7; no implica cierre de S12 ni ejecución de los seguimientos de LoginV0/MVP1. | 02-XI-2026; revisión humana | «Acepto S12.7»; aceptación registrada en T05 y en [roadmap.md](../../../context/roadmap.md). |

## Aceptación y cierre

- Estado del trabajo: completado y aceptado por DLG el 02-XI-2026 con la referencia «Acepto S12.7».
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA04 cuentan con revisión y disposiciones trazables, y CA05 con aceptación humana explícita.
- Bloqueos y riesgos restantes: seguimiento de logging seguro y validación del formato del hash al implementar LoginV0/MVP1; riesgo aceptado de identidad compartida hasta LoginV0; límite SQL aceptado para MVP0.
- Aceptación humana: DLG, 02-XI-2026, referencia «Acepto S12.7».
- Archivado: completado por @DaveSoftDev el 04-XI-2026 («Archiva las specs activas»).
- Ubicación archivada: `specs/archive/s12.7-revision-hallazgos-riesgos/`.
- Fecha efectiva de archivado y comprobación de enlaces: 04-XI-2026; carpeta, artefactos y enlaces entrantes/salientes comprobados.
- Siguiente paso: ninguno para S12.7. Los seguimientos registrados conservan su propio alcance y no se consideran mitigados; la Definition of Done de S12 sigue pendiente.
