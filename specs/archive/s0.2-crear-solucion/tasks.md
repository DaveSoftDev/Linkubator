# Tareas: S0.2 — Crear solución y referencias entre capas

## Referencias y autorización

- [Especificación](spec.md): aprobada por DLG el 7-X-2026.
- [Plan técnico y validaciones](plan.md): aprobado por DLG el 7-X-2026.
- Ambas aprobaciones documentales constan; no acreditan la ejecución de las tareas productivas.
- Las comprobaciones registradas el 7-X-2026 verifican los entregables actuales; no acreditan la fecha ni el orden histórico de su creación.

## Descomposición

Estados de tareas: pendiente, en curso, completada o bloqueada. Tras cada tarea se registra el resultado real de su comprobación; un resultado esperado no es evidencia.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar la [especificación](spec.md); comprobar que el [plan técnico](plan.md) está aprobado. | [CA01 a CA05](spec.md#criterios-de-aceptación); [diseño](plan.md#diseño-de-implementación) | Ninguna | Ambas aprobaciones explícitas registradas. | completada | DLG, 7-X-2026: aprobaciones expresas del plan y la especificación; no acreditan implementación previa. |
| T02 | Crear la solución, los proyectos productivos y Tests con la tecnología prevista. | [CA01, CA02](spec.md#criterios-de-aceptación); [componentes](plan.md#componentes-y-contratos) | T01 | Proyectos creados con framework y SDK previstos; comprobar su existencia y configuración. | completada | Entregable comprobado el 7-X-2026: [solución](../../../Linkubator.sln) con proyectos en `src/` y `tests/`; SDK instalado 10.0.401, proyectos `net10.0`. Fecha de creación no verificada. |
| T03 | Agregar proyectos a la solución y establecer sus referencias. | [CA01, CA03, CA04](spec.md#criterios-de-aceptación); [V01 y V03](plan.md#estrategia-de-validación) | T02 | Listado completo; cotejo de referencias con la matriz fuente, sin dependencias prohibidas. | completada | V01/V03, 7-X-2026: `dotnet sln Linkubator.sln list` enumera cinco proyectos; referencias de los cinco `.csproj` cotejadas con [arquitectura](../../../context/architecture.md#dependencias-entre-proyectos), sin dependencia productiva de Tests. Corroboración: prueba de matriz superada. |
| T04 | Preparar el registro público de Infrastructure y el composition root de Web. | [CA05](spec.md#criterios-de-aceptación); [V04](plan.md#estrategia-de-validación) | T03 | Revisar la API y el orden de composición; solo `Program` consume tipos de Infrastructure. | completada | V04, 7-X-2026: [Program.cs](../../../src/Linkubator.Web/Program.cs) registra `AddInfrastructure()` antes de `Build()`; [punto público](../../../src/Linkubator.Infrastructure/InfrastructureServiceCollectionExtensions.cs) devuelve `IServiceCollection`; cuatro fuentes Web y una Infrastructure revisadas; prueba de capa superada. |
| T05 | Compilar la solución y consolidar las verificaciones de S0.2. | [CA01 a CA05](spec.md#criterios-de-aceptación); [V01 a V04](plan.md#estrategia-de-validación) | T04 | Compilación sin errores ni warnings, criterios comprobados y resultados documentados. | completada | V02, 7-X-2026: `dotnet build Linkubator.sln --nologo --verbosity minimal` completado, 0 advertencias y 0 errores; V01/V03/V04 registrados a continuación. Incluye componentes añadidos en otras etapas; no demuestra la fecha de implementación de S0.2. |
| T06 | Revisar los resultados y solicitar aceptación humana. | [Criterios](spec.md#criterios-de-aceptación); [puerta de salida](plan.md#orden-de-ejecución-y-puerta-de-salida) | T05 | Decisión humana explícita con evidencias por criterio; no cerrar ante incumplimientos. | completada | DLG, 7-X-2026: Se cumple lo esperado, tras la verificación documentada de CA01 a CA05. |

T02 a T05 recogen entregables y verificaciones de la solución actual, no una reconstrucción de cuándo se realizaron las tareas. T01 recoge solo las aprobaciones documentales. Las dos pruebas automatizadas ya existentes sirven de corroboración, sin atribuir su autoría a S0.2.

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T03, T05 | [V01](plan.md#estrategia-de-validación) | Conforme: cinco proyectos presentes en la solución. | 7-X-2026; SDK 10.0.401, Windows. | `dotnet sln Linkubator.sln list`; [Linkubator.sln](../../../Linkubator.sln). |
| CA02 | T02, T05 | [V02](plan.md#estrategia-de-validación) | Conforme: SDK/TFM previstos y build con 0 advertencias, 0 errores. | 7-X-2026; SDK 10.0.401, Windows. | Lectura de los `.csproj`; `dotnet build Linkubator.sln --nologo --verbosity minimal`. |
| CA03 | T03, T05 | [V03](plan.md#estrategia-de-validación) | Conforme: referencias productivas coinciden con la matriz fuente. | 7-X-2026; SDK 10.0.401, Windows. | `.csproj` cotejados con [arquitectura](../../../context/architecture.md#dependencias-entre-proyectos); prueba `ProjectReferencesMatchTheAllowedMatrix` correcta. |
| CA04 | T03, T05 | [V03](plan.md#estrategia-de-validación) | Conforme: Tests referencia producción y producción no referencia Tests. | 7-X-2026; SDK 10.0.401, Windows. | [Linkubator.Tests.csproj](../../../tests/Linkubator.Tests/Linkubator.Tests.csproj), cuatro `.csproj` productivos y prueba de matriz correcta. |
| CA05 | T04, T05 | [V04](plan.md#estrategia-de-validación) | Conforme: composición limitada a `Program` y registrada antes del build del host. | 7-X-2026; SDK 10.0.401, Windows. | [Program.cs](../../../src/Linkubator.Web/Program.cs), [AddInfrastructure](../../../src/Linkubator.Infrastructure/InfrastructureServiceCollectionExtensions.cs); prueba `WebTypesOutsideTheCompositionRootDoNotDependOnInfrastructure` correcta. |

Al ejecutar las validaciones, anotar comando o revisión, resultado, fecha, entorno pertinente y referencia real. Una comprobación fallida o no disponible no se marca como satisfactoria.

Corroboración adicional el 7-X-2026: `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --no-restore --filter 'FullyQualifiedName~ProjectReferenceMatrixTests|FullyQualifiedName~LayerDependencyTests' --logger 'console;verbosity=normal'` ejecutó 2 pruebas, ambas correctas. Estas pruebas se incorporaron fuera del alcance de S0.2 y no prueban por sí solas la firma ni el orden de registro, revisados separadamente. El ejecutor del editor no descubrió tests por rutas de archivo; por eso la evidencia se basa en la ejecución filtrada de `dotnet test`.

## Aceptación y cierre

- Estado del trabajo: resultado aceptado por DLG y archivado el 7-X-2026.
- Criterios sin evidencia satisfactoria registrada: ninguno en la comprobación del 7-X-2026.
- Bloqueos y riesgos restantes: la comprobación actual no acredita la secuencia histórica de creación; no bloquea la aceptación de los entregables presentes.
- Aceptación humana del resultado: DLG, 7-X-2026; instrucción «Se cumple lo esperado».
- Archivado: completado el 7-X-2026; los tres artefactos están en `specs/archive/s0.2-crear-solucion/` y la carpeta activa ya no existe.
- Comprobación de enlaces: 7-X-2026; 15 enlaces entrantes y 80 enlaces/anclas relacionados con el archivo resueltos, sin enlaces a la ruta anterior.
- Siguiente paso: continuar con el siguiente trabajo del plan; este cierre no acepta ni archiva trabajos vecinos.