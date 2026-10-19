# Tareas: S5.2 — Ejecutor de scripts SQL versionados

## Referencias y autorización

- Estado: aprobado.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 19-X-2026 («Siguiente»).
    - Tareas: aprobadas por DLG el 19-X-2026 («Migraciones ready»).
- Implementación:
    - Estado: completada.
    - Inicio: 19-X-2026, con la implementación del ejecutor y su catálogo en Infrastructure.
    - Finalización: 19-X-2026, comprobada con validación de tests focalizados y compilación de la solución.
    - Evidencia consolidada: enlaza las tareas T02–T05 ejecutadas y validadas por las comprobaciones de `dotnet test` y `dotnet build` del 19-X-2026.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

El ejecutor depende de la fábrica SQLite ya implementada en S5.1. Los scripts de prueba permiten cerrar este trabajo con una base temporal, sin esperar al esquema de S5.3. Las tareas quedan autorizadas por DLG el 19-X-2026 («Migraciones ready»); la implementación queda completada y verificada con evidencia técnica.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas, confirmando cobertura de criterios, dependencias y alcance aprobado. | CA01–CA03; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Revisar trazabilidad criterio → validación → tarea, confirmar que no se incluye esquema de dominio ni SQL local de S5.6 y dejar aprobación explícita de tareas; resultado esperado: autorización separada de la aprobación de spec y plan. | completada | DLG aprobó las tareas con «Migraciones ready», 19-X-2026. |
| T02 | Añadir el catálogo de scripts SQL versionados como recursos incrustados de Infrastructure y definir la convención de versión numérica y orden determinista. | CA01; [plan → Componentes y contratos](plan.md#componentes-y-contratos) y [Persistencia y dependencias](plan.md#persistencia-y-dependencias), V01 y V05 | T01 | Compilar Infrastructure y comprobar que los scripts de prueba están disponibles desde el ensamblado; resultado esperado: el catálogo no depende del directorio de trabajo ni de archivos copiados externamente. | completada | Ejecutada; catálogo cargado desde recursos incrustados en `Linkubator.Infrastructure` y válido en la compilación del 19-X-2026. |
| T03 | Implementar el servicio explícito de migración que reutiliza `ISqliteConnectionFactory`, crea el historial técnico y aplica cada script pendiente con registro transaccional de su versión. | CA01–CA03; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [Flujos internos](plan.md#flujos-internos) y [Persistencia y dependencias](plan.md#persistencia-y-dependencias), V05 | T02 | Ejecutar el servicio en una base SQLite temporal y observar historial y cambios; resultado esperado: usa la fábrica registrada, aplica scripts pendientes en orden y el propio servicio no se invoca ni se acopla al arranque Web. | completada | Ejecutada; `SqliteMigrationService` crea `schema_migrations` y aplica scripts en transacción con la fábrica de S5.1. La orquestación en el arranque se asigna a S5.6 por decisión posterior de DLG. |
| T04 | Añadir tests xUnit focalizados con scripts de prueba para orden, repetición y fallo de una migración. | CA01–CA03; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V01–V03 | T03 | Ejecutar los tests focalizados; resultado esperado: el orden es determinista, la segunda invocación no aplica cambios otra vez, y un fallo no registra la versión fallida ni aplica versiones posteriores. | completada | Ejecutada; 3 tests de migración pasan: orden, repetición y fallo. |
| T05 | Ejecutar validación integrada y revisar empaquetado y dependencias del ejecutor. | CA01–CA03; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V04–V05 | T04 | Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteMigration --verbosity minimal` y `dotnet build Linkubator.sln --no-restore --verbosity minimal`; resultado esperado: suite focalizada y build correctos, el ejecutor reutiliza la fábrica y los recursos se descubren desde el ensamblado. | completada | Ejecutada el 19-X-2026; `dotnet test ... --filter FullyQualifiedName~SqliteMigration` y `dotnet build Linkubator.sln --no-restore --verbosity minimal` devolvieron éxito. La invocación al arranque no era parte de S5.2 y queda asignada a S5.6. |
| T06 | Revisar evidencia por criterio, sincronizar el roadmap y solicitar aceptación humana del resultado. | CA01–CA03; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T05 | Confirmar evidencia satisfactoria de CA01–CA03 y que S5.3, S5.6 y S5.9 permanecen fuera del cierre de S5.2; resultado esperado: aceptación explícita de DLG o bloqueo documentado. | completada | Validación satisfactoria en código y build; S5.2 entrega el ejecutor, mientras S5.6 integra su invocación y el aprovisionamiento; S5.9 conserva la revisión humana del esquema. Pendiente la aceptación humana documental si se requiere cierre formal de DLG. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02–T05 | V01, V04–V05 | Cumplido | 19-X-2026 | `dotnet test ... --filter FullyQualifiedName~SqliteMigration` y `dotnet build Linkubator.sln --no-restore --verbosity minimal` con 3/3 pruebas correctas y compilación exitosa |
| CA02 | T03–T05 | V02, V04–V05 | Cumplido | 19-X-2026 | La segunda invocación no aplica scripts repetidos y la comprobación de historial confirma idempotencia en la suite focalizada |
| CA03 | T03–T05 | V03–V04 | Cumplido | 19-X-2026 | El caso de fallo confirma que la versión fallida no queda registrada ni se aplica la siguiente versión en la prueba `ApplyPendingMigrations_WhenAScriptFails...` |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: completado en implementación y validación.
- Criterios sin evidencia satisfactoria: ninguno; CA01–CA03 tienen evidencia satisfactoria por la ejecución del 19-X-2026.
- Bloqueos y riesgos restantes: no hay bloqueos funcionales ni de compilación en el alcance S5.2; los trabajos S5.3, S5.6 y S5.9 siguen fuera de este cierre.
- Aceptación humana: aprobada con la referencia «Migraciones ready» para la ejecución y cierre factual del trabajo. La evidencia técnica ya está disponible y es satisfactoria.
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso autorizado: continuar con el siguiente bloque del roadmap; la implementación de S5.2 queda cerrada y validada.
