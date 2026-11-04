# Tareas: S12.1 — Matriz de trazabilidad de checklist y tests

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 31-X-2026 con la referencia «Siguiente».
  - Plan: [plan.md](plan.md), aprobado por DLG el 31-X-2026 con la referencia «Siguiente».
  - Tareas: aprobadas por DLG el 31-X-2026 con la referencia «Vamos».
- Implementación:
  - Estado: completada y aceptada por DLG.
  - Inicio: 31-X-2026; solicitud de inicio de implementación de DLG tras la aprobación de tareas «Vamos».
  - Finalización y aceptación: 31-X-2026; T01–T04 completadas y T05 aceptada por DLG con la referencia «Acepto S12.1».
  - Evidencia consolidada: [TRX final](../../../tests/Linkubator.Tests/TestResults/s12.1-final.trx), [matriz](../../../plans/mvp0-checklist-test-matrix.md) y resultados por tarea.
- La preparación de estas tareas no implica aprobación ni evidencia de implementación.

## Descomposición

Las tareas son secuenciales: la matriz e inventario determinan qué casos se inspeccionan; el diagnóstico determina si hacen falta cambios de tests; la validación final depende de esos cambios o de dejar evidencia de que no eran necesarios.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Iniciar la ejecución, actualizar el estado de roadmap e índice, y desglosar la checklist en filas atómicas para `plans/mvp0-checklist-test-matrix.md`, con identificadores estables y enlaces a las reglas fuente. | CA01; [componentes](plan.md#componentes-y-contratos), [V01](plan.md#estrategia-de-validación) | Ninguna | Los artefactos registran la implementación en curso y la matriz representa cada punto y subescenario con una fuente válida. | completada | 169 filas con IDs únicos; enlaces y anclas locales validados el 31-X-2026. Matriz: [mvp0-checklist-test-matrix.md](../../../plans/mvp0-checklist-test-matrix.md). |
| T02 | Reconciliar cada fila automatizada con un test ejecutable y su aserción; registrar manuales, responsabilidades vecinas, estado y huecos. | CA02–CA03; [flujo de reconciliación](plan.md#flujos-internos), [V02–V03](plan.md#estrategia-de-validación) | T01 | Ejecutar `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-restore --logger "trx;LogFileName=s12.1-inventory.trx" --verbosity minimal` e inspeccionar cada aserción asignada; ninguna cobertura se atribuye solo por nombre o por resultado global. | completada | Suite Debug: **1.840/1.840**, sin omitidos, con VSTest, xUnit v2 y SDK 10.0.403; [TRX local](../../../tests/Linkubator.Tests/TestResults/s12.1-inventory.trx). Se reconciliaron las 169 filas, se asignaron revisiones manuales y responsables vecinos, y se validaron IDs únicos y enlaces locales. CSV de auditoría: 1.834 filas; P17 explica la diferencia: siete variantes actuales sin correspondencia exacta y una fila obsoleta, con expectativas actuales verificadas. Huecos para T03: R04, R18, R23, R29, D30 y W18; D24 requiere reforzar una aserción de idempotencia. |
| T03 | Añadir o reforzar los tests aprobados para los huecos automatizados de S12.1; si no hay huecos, registrar el resultado del análisis. | CA04; [flujo de cobertura](plan.md#flujos-internos), [V04](plan.md#estrategia-de-validación) | T02 | Ejecutar los casos afectados con el filtro `FullyQualifiedName` concreto; pasan los tests añadidos/reforzados o queda registrada la ausencia de huecos. Una discrepancia de comportamiento fuera del alcance queda bloqueada y se escala, no se oculta. | completada | Se añadieron casos para R04, R18, R23, R29, D30 y W18, y se reforzó D24 con el conteo de la colección provisionada. Validación focalizada Debug: **446/446**, sin omitidos, con VSTest, xUnit v2 y SDK 10.0.403; filtro `FullyQualifiedName` sobre las seis clases modificadas. La matriz registra el método y la aserción de cada escenario. |
| T04 | Ejecutar validación integrada, completar la matriz con evidencia actual y actualizar el estado de implementación y su referencia en el roadmap. | CA01–CA05; [garantías](plan.md#garantías-técnicas), [V01–V05](plan.md#estrategia-de-validación) | T03 | Ejecutar `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-restore --logger "trx;LogFileName=s12.1-final.trx" --verbosity minimal`; la suite pasa, las filas quedan trazables y los estados pendientes no se presentan como evidencia. Registrar implementación completada sin aceptación humana y enlazar la evidencia desde el roadmap. | completada | Suite Debug: **1.849/1.849**, sin omitidos, con VSTest, xUnit v2 y SDK 10.0.403. [TRX final](../../../tests/Linkubator.Tests/TestResults/s12.1-final.trx). Se revisaron las 169 filas, sus estados y destinos locales; no quedan huecos automatizados asignados a S12.1. |
| T05 | Revisar la evidencia y registrar la aceptación o el rechazo humano; tras aceptación, actualizar el estado de S12.1 en el roadmap. | CA01–CA05; [cierre](plan.md#orden-de-ejecución-y-cierre) | T04 | La decisión humana queda registrada con referencia; si se acepta, los criterios tienen evidencia satisfactoria y la actualización del roadmap enlaza esta evidencia. Si se rechaza, registrar correcciones y bloqueo sin cerrar el trabajo. | completada | DLG aceptó S12.1 el 31-X-2026 con la referencia «Acepto S12.1»; [roadmap actualizado](../../../context/roadmap.md#now). |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T04, T05 | V01, V05 en [plan.md](plan.md#estrategia-de-validación) | Matriz revisada: 169 filas, IDs únicos y destinos locales existentes; referencias y estados finales contrastados. | 31-X-2026; validación documental local | [mvp0-checklist-test-matrix.md](../../../plans/mvp0-checklist-test-matrix.md); [TRX final](../../../tests/Linkubator.Tests/TestResults/s12.1-final.trx) |
| CA02 | T02, T04, T05 | V02, V05 en [plan.md](plan.md#estrategia-de-validación) | Reconciliación completada y contrastada con la ejecución integrada: métodos y aserciones concretos registrados; suite completa **1.849/1.849**, sin omitidos. | 31-X-2026; Debug, .NET SDK 10.0.403, VSTest, xUnit v2 | [TRX final](../../../tests/Linkubator.Tests/TestResults/s12.1-final.trx); [matriz](../../../plans/mvp0-checklist-test-matrix.md) |
| CA03 | T02, T04, T05 | V03, V05 en [plan.md](plan.md#estrategia-de-validación) | Comprobaciones manuales y tareas vecinas asignadas a responsables y con estado real; las revisiones humanas pendientes no se presentan como evidencia ejecutada. | 31-X-2026; revisión documental local | [matriz](../../../plans/mvp0-checklist-test-matrix.md), filas W25, A01–A18, P18–P21 y P27 |
| CA04 | T03, T04, T05 | V04, V05 en [plan.md](plan.md#estrategia-de-validación) | Seis gaps automatizados cubiertos y D24 reforzado; pasaron los tests focalizados (**446/446**) y la suite integrada (**1.849/1.849**), sin omitidos. | 31-X-2026; Debug, .NET SDK 10.0.403, VSTest, xUnit v2 | T03 y [TRX final](../../../tests/Linkubator.Tests/TestResults/s12.1-final.trx); [matriz](../../../plans/mvp0-checklist-test-matrix.md) |
| CA05 | T04, T05 | V05 en [plan.md](plan.md#estrategia-de-validación) | Revisión técnica final completada: todas las filas tienen estado y ruta de comprobación; los gaps de T03 están cerrados y la evidencia de ejecución enlazada. Las revisiones manuales y tareas vecinas pendientes conservan su estado real. | 31-X-2026; revisión documental y ejecución local | [matriz](../../../plans/mvp0-checklist-test-matrix.md); [TRX final](../../../tests/Linkubator.Tests/TestResults/s12.1-final.trx) |

Un resultado no disponible o fallido seguirá pendiente o bloqueado; los resultados esperados no se registran como evidencia real.

## Aceptación y cierre

- Estado del trabajo: completado y aceptado.
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos y riesgos restantes: ninguno dentro del alcance de S12.1; la etapa S12 no se declara cerrada y las revisiones manuales permanecen asignadas a sus tareas propietarias.
- Aceptación humana: DLG, 31-X-2026, referencia «Acepto S12.1».
- Archivado: completado por @DaveSoftDev el 04-XI-2026 («Archiva las specs activas»).
- Ubicación archivada: `specs/archive/s12.1-matriz-trazabilidad-checklist-tests/`.
- Fecha efectiva de archivado y comprobación de enlaces: 04-XI-2026; carpeta, artefactos y enlaces entrantes/salientes comprobados.
- Siguiente paso: continuar con S12 conforme al roadmap; no cerrar S12 con esta tarea.
