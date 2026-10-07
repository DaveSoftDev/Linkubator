# Plan técnico: S0.7 — Pruebas de arquitectura y arranque Web

## Estado y aprobación del plan

- Estado: aprobado.
- Especificación: [spec.md](spec.md).
- Aprobaciones:
  - Especificación: DLG, 07-X-2026 («Pruebas completadas»).
  - Plan y tareas: DLG, 07-X-2026 («Pruebas completadas»).

## Diseño de implementación

### Componentes y contratos

El proyecto Tests referencia los ensamblados productivos para verificarlos, sin formar parte del grafo productivo. La suite usa xUnit y el adaptador VSTest que ya forma parte del proyecto de pruebas.

La prueba de matriz compara las referencias declaradas por los proyectos con la matriz fuente en [architecture.md → «Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos). La prueba arquitectónica consulta los tipos de Web que dependen de Infrastructure y exige que el consumidor permitido sea el composition root. La prueba de arranque crea `WebApplicationFactory<Program>` y consulta la ruta mínima preparada por S0.5.

### Flujos internos

1. Restaurar y compilar el proyecto Tests junto con sus referencias.
2. Cargar los proyectos declarados y compararlos con la matriz arquitectónica.
3. Inspeccionar dependencias de tipos de Web con NetArchTest.
4. Crear el host de pruebas con `WebApplicationFactory` y solicitar la ruta raíz.
5. Ejecutar build y suite y registrar salidas reales.

### Persistencia y dependencias

No se crea ni se abre base de datos. `WebApplicationFactory` emplea un host de pruebas; no sustituye una prueba live de HTTPS ni una validación de navegador. La solución depende de xUnit, VSTest, NetArchTest y ASP.NET Core Testing ya declarados por el proyecto Tests. No se agregan dependencias como parte de la conversión documental.

### Garantías técnicas

- El test de matriz detecta referencias declaradas no permitidas, incluido un proyecto productivo que dependa de Tests.
- El test de capa detecta tipos de Web con referencias de Infrastructure fuera del composition root.
- La prueba de sensibilidad puede realizarse sin editar fuentes: ejecutar los tests por nombre confirma que ambos detectores corren, pero no demuestra que rechazarían una infracción artificial; no modificar el árbol productivo para simularla en este cierre.
- La prueba de arranque solo acredita la respuesta de TestServer, no HTTPS real.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Comparar las referencias reales con una tabla equivalente a la matriz fuente | Diseño existente revisado | [architecture.md → «Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos) | Un cambio arquitectónico debe actualizar la fuente y el test de forma sincronizada. |
| Inspeccionar el ensamblado Web con NetArchTest | Diseño existente revisado | [Plan principal → S0.7](../../../plans/mvp0-plan.md#s0-fundaciones-m) | Comprueba dependencias compiladas, pero la sensibilidad a una infracción artificial no se altera en el árbol productivo. |
| TestServer no acredita HTTPS live | Límite técnico | [S0.5 → «Estrategia de validación»](../s0.5-configuracion-web/plan.md#estrategia-de-validación) | HTTPS live se validó de forma separada en S0.5. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA05 | Build | SDK disponible | `dotnet build Linkubator.sln --nologo --verbosity minimal` | Solución compila sin warnings/errors. | Salida resumida, SDK, fecha. |
| V02 | CA02 | Automatizada | Test assembly construida | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-build --no-restore --filter 'FullyQualifiedName~ProjectReferenceMatrixTests.ProjectReferencesMatchTheAllowedMatrix' --logger 'console;verbosity=normal'` | Test descubierto y pasa. | Recuento y nombre del test. No demuestra un cambio histórico de referencias. |
| V03 | CA03 | Automatizada | Test assembly construida | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-build --no-restore --filter 'FullyQualifiedName~LayerDependencyTests.WebTypesOutsideTheCompositionRootDoNotDependOnInfrastructure' --logger 'console;verbosity=normal'` | Test descubierto y pasa. | Recuento y nombre del test. Sin editar fuentes para inyectar una infracción. |
| V04 | CA04 | Automatizada | Host TestServer disponible | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-build --no-restore --filter 'FullyQualifiedName~WebStartupTests.HomePageReturnsSuccess' --logger 'console;verbosity=normal'` | Host responde con éxito en ruta raíz. | Recuento, respuesta lógica y advertencias; consignar que no es HTTPS live. |
| V05 | CA05 | Automatizada | V01 completada | `dotnet test Linkubator.sln --no-build --no-restore --logger 'console;verbosity=minimal'` | Todos los tests descubiertos pasan. | Total, aprobados, fallidos y omitidos. |
| V06 | CA01 a CA04 | Revisión técnica | Fuentes presentes | Revisar csproj, tres pruebas y que sus asserts correspondan a matriz, frontera de capa y ruta base. | Las pruebas ejercitan los contratos descritos sin ampliar el alcance. | Archivos revisados y límites de cobertura. |

El test del límite arquitectónico y el de matriz se revisan para confirmar que son sensibles según su lógica; no se fuerza una infracción en las fuentes productivas ni se atribuye cobertura de seguridad extra.

## Orden de ejecución y puerta de salida

- [Tareas](tasks.md) relaciona cada criterio con build, pruebas, revisión y aceptación.
- Cerrar cuando todos los criterios tengan evidencia satisfactoria actual, no haya bloqueos y DLG acepte el resultado.
- El archivo solo se completa tras actualizar enlaces entrantes/salientes, verificar el destino y retirar la ubicación activa.
