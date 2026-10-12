# Tareas: S3.3 — Transiciones público y privado

## Referencias y autorización

- Estado: aceptada por DLG el 12-X-2026 («Implementado»).
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 12-X-2026 («Adelante»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 12-X-2026 («Vamos»).
    - Tareas: aprobadas por DLG el 12-X-2026 («Vamos»).
- Implementación:
    - Estado: completada y aceptada.
    - Inicio: 12-X-2026; referencia real: «Vamos».
    - Finalización y aceptación: DLG, 12-X-2026 («Implementado»).
    - Comprobación: `dotnet test Linkubator.sln --no-restore --verbosity minimal` (551 correctas, 0 errores, 0 omitidas) y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal` (correcto), .NET 10.0.12.
    - Evidencia consolidada: CA01–CA08 tienen tests y validación integrada; Domain no incorpora dependencias de otras capas ni infraestructura.

## Descomposición

T01 confirma usos y validación; T02 registra el inicio en el roadmap; T03–T05 implementan y prueban creación, transiciones y movimiento; T06 integra las validaciones; T07 sincroniza el cierre técnico y T08 registra la aceptación humana.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Confirmar los usos públicos de `Collection.Create`, `Link.Create`, `MoveToCollection`, `IsPublic` y `CollectionId`, y registrar la validación de cierre. | Preparación de CA01–CA08; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Estrategia de validación»](plan.md#estrategia-de-validación) V01–V05 | Ninguna | Las pruebas y el build completos pasan; la revisión confirma que las creaciones de `Link` reciben una `Collection` materializada. | completada | Validación final: 551/551 pruebas y build correcto. La línea base anterior a los cambios no quedó registrada. |
| T02 | Registrar en el roadmap el inicio real de implementación de S3.3 y la referencia de aprobación de tareas. | Trazabilidad; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T01 y aprobación explícita de estas tareas | `context/roadmap.md` identifica S3.3 como en curso y enlaza los artefactos; la cabecera de este archivo refleja el inicio con fecha y referencia reales. | completada | Roadmap actualizado con la implementación en curso y la referencia «Vamos». |
| T03 | Actualizar creación de `Collection` y `Link` para aceptar privacidad explícita manteniendo estado privado por defecto; crear y validar colección pública vacía y creación de enlace público solo dentro de colección pública. Añadir los tests unitarios correspondientes. | CA01–CA03; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos); V01 | T02 | Tests enfocados de `CollectionTests` y `LinkTests`; estados válidos correctos y solicitud pública para colección privada rechazada sin crear entidad. | completada | Tests verifican el estado por defecto, creación pública y rechazo al pedir publicación con colección privada; `Link.Create` recibe solo colecciones materializadas. |
| T04 | Implementar los cambios de privacidad de `Collection` y `Link`, validando relaciones antes de mutar y aplicando la propagación requerida al hacer privada una colección; añadir tests de transiciones, lote inválido y ausencia de mutación parcial. | CA04–CA06; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos); V02 | T03 | Tests enfocados de privacidad: hacer pública la colección no publica enlaces; hacerla privada privatiza todos; enlaces de colecciones privadas no pueden hacerse públicos; el conjunto permanece intacto si falla una precondición. | completada | Tests cubren ambas transiciones de colección, las transiciones permitida y rechazada de enlace, y lote inconsistente sin mutación parcial. |
| T05 | Actualizar movimiento de enlaces para dejarlo privado solo después de validar propiedad y destino; cubrir los estados de origen y destino y el rechazo sin mutación. | CA07; [plan.md → «Flujos internos»](plan.md#flujos-internos); V03 | T03 | Tests enfocados de `MoveToCollection`; movimientos válidos dejan enlace privado y destino inválido conserva colección y privacidad anteriores. | completada | Pruebas para las cuatro combinaciones de privacidad de origen/destino y rechazo a destino ajeno conservando estado. |
| T06 | Ejecutar suite y build completos, revisar superficie pública, trazabilidad de los criterios y sensibilidad de pruebas negativas. | CA08 y CA01–CA07; [plan.md → «Garantías técnicas»](plan.md#garantías-técnicas), [«Estrategia de validación»](plan.md#estrategia-de-validación) V04–V05 | T04, T05 | `dotnet test Linkubator.sln --no-restore --verbosity minimal` y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal` pasan; las pruebas detectan el intento de creación/transición no permitido; Domain mantiene las dependencias autorizadas. | completada | Suite completa: 551 correctas, 0 errores ni omitidas; build `-warnaserror` correcto en .NET 10.0.12. Mutación temporal del guardado de publicación provocó el fallo esperado del test negativo; condición restaurada y prueba focalizada correcta. |
| T07 | Sincronizar el resumen de implementación de esta cabecera y el roadmap al completar las tareas técnicas, adjuntando fecha, comprobación y referencias reales. | CA08; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T06 | `tasks.md` refleja implementación completada y evidencia consolidada; `roadmap.md` enlaza al cierre técnico, pero mantiene pendiente la aceptación humana. | completada | Cambios documentados en esta cabecera y en el roadmap. |
| T08 | Revisar evidencias contra cada criterio y registrar la aceptación humana de DLG, sin confundirla con tests verdes. | CA01–CA08; [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación), [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T07 | Matriz de evidencia completa; aceptación o rechazo explícito queda registrado con fecha y referencia real. | completada | Aceptación de DLG, 12-X-2026 («Implementado»). |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T03, T06 | V01, V04 | Cumplido | 12-X-2026 | Tests de colección privada por defecto y pública explícita; suite completa 551/551 |
| CA02 | T03, T06 | V01, V04 | Cumplido | 12-X-2026 | Test de creación de enlace privado y público en colección pública |
| CA03 | T03, T06 | V01, V04 | Cumplido | 12-X-2026 | Test de rechazo al crear enlace público en colección privada |
| CA04 | T04, T06 | V02, V04 | Cumplido | 12-X-2026 | Test confirma que hacer pública la colección no publica sus enlaces |
| CA05 | T04, T06 | V02, V04 | Cumplido | 12-X-2026 | Test confirma que hacer privada la colección privatiza todos sus enlaces |
| CA06 | T04, T06 | V02, V04 | Cumplido | 12-X-2026 | Tests de publicación permitida en colección pública y rechazada en privada |
| CA07 | T05, T06 | V03, V04 | Cumplido | 12-X-2026 | Cuatro combinaciones origen/destino; movimiento válido deja enlace privado y rechazo conserva estado |
| CA08 | T01, T02, T06, T07, T08 | V04, V05 | Cumplido | 12-X-2026 | Suite completa 551/551 y build `-warnaserror`; Domain sin dependencias no permitidas |


## Aceptación y cierre

- Estado del trabajo: aceptado y cerrado.
- Criterios con evidencia satisfactoria: CA01–CA08.
- Bloqueos y riesgos restantes: ninguno comprobado. La línea base anterior a los cambios no quedó registrada; la suite y el build finales se ejecutaron correctamente.
- Aceptación humana: DLG, 12-X-2026 («Implementado»).
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso autorizado: ninguno para este trabajo; archivado no solicitado.
