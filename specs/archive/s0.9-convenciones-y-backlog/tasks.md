# Tareas: S0.9 — Convenciones de trabajo y referencia al backlog

## Referencias y autorización

- [Especificación](spec.md): aprobada por DLG el 07-X-2026.
- [Plan técnico](plan.md): aprobado por DLG el 07-X-2026.
- Referencia: aprobación expresa de DLG «Convenciones correctas» para los documentos S0.9.
- La reescritura es prospectiva; las comprobaciones de contenido validan el estado actual, no la fecha de adopción histórica.

## Descomposición

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Aprobar especificación, plan y tareas. | [CA-01 a CA-04](spec.md#criterios-de-aceptación); [diseño](plan.md#diseño-de-implementación) | Ninguna | Aprobación expresa de DLG registrada para los tres documentos. | completada | DLG, 07-X-2026: «Convenciones correctas», instrucción de esta conversación para cada documento S0.9. |
| T02 | Revisar consistencia de ramas, commits y PR. | [CA-01](spec.md#criterios-de-aceptación); [V-01](plan.md#estrategia-de-validación) | T01 | Contrastar CONTRIBUTING con el resumen de README. | completada | 07-X-2026: `CONTRIBUTING.md` contiene las cuatro plantillas de rama y cinco prefijos de commit; README repite ambas listas. Checklist detallada de PR en CONTRIBUTING; README conserva una síntesis coherente de trazabilidad/verificación. Revisión textual, no historial Git. |
| T03 | Comprobar referencia al backlog y roadmap. | [CA-02](spec.md#criterios-de-aceptación); [V-02](plan.md#estrategia-de-validación) | T01 | Confirmar que las guías enlazan a mvp0-plan/roadmap sin reproducir su lista de sprints. | completada | 07-X-2026: CONTRIBUTING y README enlazan a `plans/mvp0-plan.md`; README enlaza a `context/roadmap.md`. El plan existe y contiene el backlog de sprints; no hay tabla de sprints duplicada en las guías. No se consultó Git. |
| T04 | Verificar enlaces y límites documentales. | [CA-03](spec.md#criterios-de-aceptación); [V-03](plan.md#estrategia-de-validación) | T02, T03 | Resolver enlaces locales y revisar que no se añadan reglas funcionales o backlog duplicado. | completada | 07-X-2026: enlaces locales de los documentos afectados (CONTRIBUTING, README, índice, README de specs y artefactos S0.9) comprobados, sin roturas. La validación recursiva adicional del propio `plans/mvp0-plan.md` detectó dos referencias preexistentes a `acceptance-criteria.md` ausente; no son enlaces introducidos por S0.9 y quedan fuera de este cambio. |
| T05 | Ejecutar build y suite existente. | [CA-04](spec.md#criterios-de-aceptación); [V-04, V-05](plan.md#estrategia-de-validación) | T02–T04 | Build sin warnings/errors y toda la suite pasa. | completada | 07-X-2026, .NET SDK 10.0.401: `dotnet build Linkubator.sln --nologo --verbosity minimal` terminó con 0 advertencias y 0 errores; `dotnet test Linkubator.sln --no-build --no-restore --logger 'console;verbosity=minimal'`: 3 pasadas, 0 fallidas, 0 omitidas. |
| T06 | Aceptar el resultado y archivar. | [puerta de salida](plan.md#orden-de-ejecución-y-puerta-de-salida) | T05 | Registrar aceptación de DLG según autorización y verificar enlaces tras mover. | completada | DLG, 07-X-2026: «Convenciones correctas». Carpeta trasladada a `specs/archive/s0.9-convenciones-y-backlog/`; índice y enlaces locales comprobados, carpeta activa ausente. |

No se inspeccionan ramas, commits, pull requests ni estado Git. La tarea comprueba documentos y el árbol legible, no un historial de uso real.

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA-01 | T02, T05 | [V-01](plan.md#estrategia-de-validación) | Conforme por revisión documental | 07-X-2026; revisión de CONTRIBUTING/README. | Plantillas de ramas y prefijos de commits iguales; checklist PR detallada y resumen del README coherentes. |
| CA-02 | T03, T05 | [V-02](plan.md#estrategia-de-validación) | Conforme | 07-X-2026; archivos de backlog/guías revisados. | Backlog reside en `mvp0-plan.md`; ambas guías enlazan al plan/roadmap y no duplican tabla de sprints. |
| CA-03 | T04, T05 | [V-03](plan.md#estrategia-de-validación) | Conforme para enlaces propios de S0.9; observación externa anotada | 07-X-2026. | Enlaces de documentos afectados resuelven. El plan principal contiene referencias preexistentes rotas a `acceptance-criteria.md`, no tocadas en S0.9. |
| CA-04 | T05 | [V-04, V-05](plan.md#estrategia-de-validación) | Conforme | 07-X-2026; .NET SDK 10.0.401. | Build 0 warnings/0 errors; suite 3/3. |

## Aceptación y cierre

- Estado del trabajo: archivado; documentos aprobados; CA-01 a CA-04 verificados; T01–T06 completadas; aceptación de DLG registrada.
- Criterios sin evidencia satisfactoria: ninguno para el alcance S0.9; hallazgo externo: enlaces heredados rotos en el propio plan a `acceptance-criteria.md` ausente.
- Bloqueos y riesgos: el rol humano de S0.9 queda cubierto únicamente por la aprobación actual de DLG; no se afirma acuerdo histórico. No se consultó Git ni se inspeccionó su estado.
- Aprobación documental: DLG, 07-X-2026, «Convenciones correctas» para `spec.md` y `plan.md`.
- Aceptación del resultado: DLG, 07-X-2026, «Convenciones correctas», conforme a la instrucción expresa de aprobar la tarea si las validaciones pasan y archivar.
- Archivado: completado el 07-X-2026 en `specs/archive/s0.9-convenciones-y-backlog/`.
- Comprobación de enlaces: 07-X-2026; enlaces locales entrantes/salientes en cinco documentos resuelven, los tres artefactos están en destino y la ubicación activa no existe.
- Siguiente paso: ninguno para S0.9. Hallazgo externo: `mvp0-plan.md` mantiene referencias a `acceptance-criteria.md` inexistente; no se modificó en S0.9.
