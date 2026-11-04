# Plan técnico: S12.1 — Matriz de trazabilidad de checklist y tests

## Estado y aprobación del plan

- Estado: aprobado.
- Preparado por IA el 31-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 31-X-2026 con la referencia «Siguiente».
  - Plan: aprobado por DLG el 31-X-2026 con la referencia «Siguiente».

## Diseño de implementación

### Componentes y contratos

- La checklist de calidad del MVP0 en [roadmap.md](../../context/roadmap.md#checklist-de-calidad-del-mvp0) y las reglas enlazadas desde ella son la fuente del comportamiento esperado. La matriz no copiará esas reglas.
- La matriz se propone como `plans/mvp0-checklist-test-matrix.md`, independiente del registro de auditoría de tests. Cada fila identificará un escenario atómico, su punto de checklist y fuente, el tipo de comprobación, el test concreto o responsable de la revisión, el estado y la evidencia.
- La revisión automatizada se limita al proyecto existente `tests/Linkubator.Tests/` y sus tests actuales. Se reutilizan xUnit y `WebApplicationFactory`; no se añaden frameworks, dependencias ni automatización de navegador.
- El plan existente de auditoría de tests aporta historial y clasificaciones previas, pero cada cobertura se contrasta con la implementación vigente y con la aserción del test correspondiente.

### Flujos internos

1. Enumerar cada punto y subescenario comprobable de la checklist, mantener una referencia estable por fila y enlazar su regla propietaria.
2. Obtener un inventario actual de los tests ejecutables y reconciliarlo con los informes previos; incluir casos incorporados, modificados o no revisados después de esos informes.
3. Para cada fila automatizada, comprobar el caso de test concreto y que sus aserciones distinguen el escenario declarado. Registrar por separado lo pendiente, los huecos y las comprobaciones manuales asignadas a su tarea propietaria.
4. Añadir o reforzar solo tests de huecos dentro del alcance aprobado. No cambiar reglas funcionales. Si una prueba revela una discrepancia de implementación, registrar el caso y detener esa corrección hasta que esté confirmada su resolución dentro del alcance autorizado.
5. Ejecutar las pruebas afectadas y la suite completa; actualizar la matriz con comandos, resultados y referencias de evidencia actuales.
6. Recorrer la matriz en ambos sentidos: checklist → comprobación para localizar huecos, y comprobación → fuente para evitar atribuciones obsoletas o sin fundamento.

### Persistencia y dependencias

- No hay cambios en base de datos, persistencia de aplicación, servicios externos ni dependencias de software.
- El entregable de matriz propuesto es documentación versionada bajo `plans/`; las pruebas nuevas o reforzadas, si se aprueban, permanecen en el proyecto de tests existente.
- La tarea puede mapear revisiones manuales y comprobaciones de otras tareas de S12, pero no las ejecuta ni espera su evidencia para cerrarse. Los tests específicos de eventos y logs permanecen en S12.2; las revisiones manuales y demás responsabilidades conservan las asignaciones de [mvp0-plan.md → «S12: Cierre y hardening (M)»](../../plans/mvp0-plan.md#s12-cierre-y-hardening-m).

### Garantías técnicas

- Ningún test se considera cobertura por su nombre, archivo o pertenencia a una suite: debe verificarse la aserción del escenario concreto.
- Un resultado de ejecución solo acredita los casos realmente ejecutados en el código identificado por esa evidencia; los resultados históricos de auditoría no se presentan como validación actual.
- Las revisiones manuales pendientes se identifican como tales, con su responsable y tarea, sin representarlas como evidencia satisfactoria.
- Una diferencia que exija decidir o cambiar una regla se remite a su documento propietario y no se resuelve en la matriz ni en los tests.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Mantener la matriz en un documento independiente dentro de `plans/`. | Confirmado | Aprobación del plan por DLG el 31-X-2026 con la referencia «Siguiente» | Facilita consultar el cruce completo sin convertir el plan de auditoría en una segunda matriz. |
| Reconciliar evidencia histórica con el inventario de tests y la ejecución actuales. | Confirmado | [test-audit-plan.md → «Control de completitud»](../../plans/test-audit-plan.md#control-de-completitud) | Evita atribuir cobertura a casos que hayan cambiado o no se hayan revisado; repetir inventario y validación antes del cierre. |
| Un fallo que indique comportamiento contrario a una regla puede requerir un cambio de producción fuera del alcance de una matriz de tests. | Pendiente | [spec.md → «Alcance»](spec.md#alcance) | Registrar el hallazgo y pedir confirmación de alcance antes de modificar comportamiento; no ocultar el caso como cubierto mientras el test no pase. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Revisión técnica de todas las secciones y subescenarios de la checklist | S12.1 iniciada y matriz disponible | Contrastar fila a fila la checklist con la matriz y abrir cada enlace a su regla fuente | Cada escenario está representado una vez con referencia válida; no quedan omisiones ni filas sin fuente | Matriz revisada, fecha y registro de cobertura checklist → fila |
| V02 | CA02 | Revisión técnica de cada caso automatizado y validación de su ejecución | Inventario de tests reconciliado y código vigente | Inspeccionar la aserción del test referenciado y ejecutar la suite con `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-restore --logger "trx;LogFileName=s12.1.trx" --verbosity minimal` | Cada atribución corresponde a un test y aserción que prueban el escenario; todos los casos reportados como pasados aparecen en el resultado de esa ejecución | Comando, resultado, fecha, TRX o referencia estable a su ubicación, nombres completos de los tests y revisión de sus aserciones |
| V03 | CA03 | Revisión documental de comprobaciones manuales y responsabilidades vecinas | Tareas y responsables definidos en el plan del MVP0 | Contrastar cada entrada manual o ajena a S12.1 con la tarea propietaria y distinguir pendiente de ejecutada | Cada comprobación tiene responsable y estado real; ninguna evidencia de otra tarea se exige para cerrar S12.1 | Matriz revisada y referencias a las tareas propietarias |
| V04 | CA04 | Pruebas automatizadas focalizadas y regresión | Huecos aprobados, tests añadidos o reforzados y discrepancias de alcance resueltas | Ejecutar los tests afectados mediante `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-restore --filter "FullyQualifiedName~<FullyQualifiedName del test afectado>" --verbosity minimal`; después ejecutar V02 | Todos los tests focalizados y la suite completa pasan; los resultados coinciden con la matriz | Filtros concretos usados, resultados, fecha, evidencia del test y de la regresión |
| V05 | CA05 | Revisión final técnica de cobertura y evidencia | V01–V04 completadas | Recorrer checklist → comprobación → resultado/evidencia y test → fuente, verificando estados pendientes y bloqueos | No quedan huecos automatizados de S12.1, filas sin estado ni resultados sin evidencia; lo manual y lo asignado a otras tareas puede seguir pendiente | Matriz final, lista de comprobación firmada por responsable y referencias de validación |

La suite completa demuestra que los casos se ejecutaron y pasaron, pero no demuestra por sí sola que cada aserción pruebe la regla asignada. V01, V02 y V05 aportan esa revisión. La revisión manual de accesibilidad y navegadores no forma parte de estas validaciones.

## Orden de ejecución y cierre

- Crear `tasks.md` solo después de la aprobación explícita de este plan.
- Las tareas identificarán los casos de test afectados durante la reconciliación; los filtros exactos de V04 se fijarán con esos nombres y no se inventan antes del inventario.
- El cierre exige evidencia satisfactoria para CA01–CA05, sin bloqueos de alcance, y aceptación humana registrada en `tasks.md`. No depende de completar las revisiones manuales ni otras tareas de S12.
- Ante una regla ambigua o una decisión funcional nueva, detener el caso afectado y tramitarla en su documento propietario antes de continuar.
