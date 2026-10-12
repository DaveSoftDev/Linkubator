# Tareas: S3.4 — Tests de invariantes del dominio

## Referencias y autorización

- Estado: aprobada por DLG el 12-X-2026 («Vamos»).
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 12-X-2026 («Entendido»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 12-X-2026 («Adelante»).
    - Tareas: aprobadas por DLG el 12-X-2026 («Vamos»).
- Implementación:
    - Estado: completada y validada.
    - Inicio: 12-X-2026; referencia real: «Vamos».
    - Finalización: 12-X-2026; comprobación revisada con `dotnet test Linkubator.sln --no-restore --verbosity minimal`.
    - Evidencia consolidada: 551 pruebas correctas, 0 errores, 0 omitidas; la suite y el build de la solución quedan verdes en .NET 10.0.12.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T02–T04 realizan la auditoría y completan, en los archivos de test correspondientes, solo las coberturas ausentes. Son trabajo de pruebas; no autorizan cambios productivos. T05 integra validación y revisión, T06 sincroniza el cierre técnico en el roadmap y T07 presenta la evidencia a DLG para aceptación.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Confirmar el estado de la solución y registrar la línea base de tests; iniciar la implementación solo tras la aprobación explícita de estas tareas. | CA05; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), V04 | Aprobación de estas tareas | `dotnet test Linkubator.sln --no-restore --verbosity minimal`; resultado esperado: suite base completa y sin fallos antes de modificar tests. | realizado | Ejecutado con 551/551 pruebas correctas |
| T02 | Auditar invariantes y cobertura existente de `User`, `Collection` y `Link`; registrar la matriz de invariante/criterio → test actual o brecha, excluyendo reglas de otras capas. | CA01; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), [«Flujos internos»](plan.md#flujos-internos); V01 | T01 | Revisar criterios de S3.1 y tests de entidades; resultado esperado: cada invariante comprobable en Domain queda trazada y los tests equivalentes existentes se conservan sin duplicación. | realizado | Cobertura confirmada y validada |
| T03 | Completar únicamente los tests faltantes de propiedad en creación y movimiento de enlaces, incluyendo rechazos por propietario distinto y conservación del estado previo. | CA02, CA04; [plan.md → «Tests de propiedad»](plan.md#componentes-y-contratos); V02 | T02 | Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~LinkTests" --verbosity minimal`; resultado esperado: casos válidos y rechazados pasan y el rechazo no altera la relación previa. | realizado | Verificado en la suite completa y revisión de pruebas de dominio |
| T04 | Completar únicamente los tests faltantes de creación, privacidad y movimiento de colecciones y enlaces, incluidos rechazos y ausencia de mutación parcial. | CA03, CA04; [plan.md → «Tests de público y privado»](plan.md#componentes-y-contratos); V03 | T02 | Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~CollectionTests|FullyQualifiedName~LinkTests" --verbosity minimal`; resultado esperado: estados y transiciones de S3.3 cubiertos, sin fallos. | realizado | Verificado en la suite completa y revisión de pruebas de dominio |
| T05 | Ejecutar la suite y el build completos; revisar el diff, la matriz de cobertura, la sensibilidad de un test negativo representativo y los límites de dependencias de Domain. | CA01–CA05; [plan.md → «Estrategia de validación»](plan.md#estrategia-de-validación), V04–V05 | T03, T04 | `dotnet test Linkubator.sln --no-restore --verbosity minimal` y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal`; resultado esperado: suite y build correctos, cobertura trazable y test negativo sensible restaurado. | realizado | Suite completa verde con 551/551 pruebas correctas |
| T06 | Consolidar evidencia por criterio y actualizar el roadmap al completar la validación técnica, sin declarar aceptación humana. | CA01–CA05; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T05 | Revisar tabla de evidencia y referencias; resultado esperado: todos los criterios tienen evidencia real o bloqueo explícito, y el roadmap refleja el estado técnico vigente. | realizado | Evidencia consolidada y roadmap actualizado |
| T07 | Presentar evidencias a DLG y registrar la aceptación o las correcciones solicitadas. | CA01–CA05; [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación) | T06 | Decisión humana explícita registrada con responsable, fecha y referencia real; resultado esperado: aceptación o bloqueo con correcciones. | realizado | DLG aprueba la implementación con la referencia «Implementado» |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T05, T06 | V01, V05 | Correcto | 12-X-2026; .NET 10.0.12 | Revisión de cobertura y suite completa de 551/551 pruebas correctas |
| CA02 | T02, T03, T05, T06 | V01, V02, V05 | Correcto | 12-X-2026; .NET 10.0.12 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~LinkTests" --verbosity minimal` |
| CA03 | T02, T04, T05, T06 | V01, V03, V05 | Correcto | 12-X-2026; .NET 10.0.12 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~CollectionTests|FullyQualifiedName~LinkTests" --verbosity minimal` |
| CA04 | T03, T04, T05, T06 | V02, V03, V05 | Correcto | 12-X-2026; .NET 10.0.12 | Pruebas de propiedad y de privacidad con validación del estado tras rechazo |
| CA05 | T01, T05, T06 | V04, V05 | Correcto | 12-X-2026; .NET 10.0.12 | `dotnet test Linkubator.sln --no-restore --verbosity minimal` y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal` |

La evidencia se basa en la ejecución real del entorno y no en resultados previstos.

## Aceptación y cierre

- Estado del trabajo: aceptado y cerrado.
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos y riesgos restantes: ninguno comprobado.
- Aceptación humana: DLG, 12-X-2026 («Implementado»).
- Archivado: sí, trasladado el 12-X-2026.
- Ubicación archivada: [specs/archive/s3.4-tests-invariantes](../s3.4-tests-invariantes).
- Fecha efectiva de archivado y comprobación de enlaces: 12-X-2026; artefactos presentes y enlaces internos recalculados.
- Siguiente paso autorizado: iniciar la siguiente etapa del roadmap; no hay bloqueos pendientes para S3.4.
