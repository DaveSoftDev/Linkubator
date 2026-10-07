# Plan técnico: S0.3 — Configuración base del repositorio

## Estado y aprobación del plan

- Estado: plan aprobado por DLG el 07-X-2026.
- [Especificación](spec.md): aprobada por DLG el 07-X-2026.
- Referencia: instrucción Cumple lo esperado; su aprobación no acredita la implementación ni sus pruebas.

## Diseño de implementación

### Componentes y contratos

Sobre la solución definida en [S0.2](../s0.2-crear-solucion/spec.md), habilitar `nullable` y los analizadores provistos por el SDK para los proyectos; consultar la configuración efectiva de MSBuild y no inferirla solo de una declaración textual. Un `.editorconfig` en la raíz define reglas compartidas de indentación, finales de línea y estilo C#. Un `.gitignore` en la raíz describe exclusiones para resultados de build, base de datos y logs de desarrollo, y configuración local. Esta tarea no requiere nuevos contratos de aplicación ni paquetes de analizadores externos.

No se fuerza `TreatWarningsAsErrors` ni `EnforceCodeStyleInBuild` como parte del alcance aprobado. La compilación sin advertencias se comprueba con [mvp0-plan.md → «3. Definition of Done común (aplica a todos los sprints)»](../../../plans/mvp0-plan.md#3-definition-of-done-común-aplica-a-todos-los-sprints).

### Flujos internos

1. Definir `nullable` y analizar la configuración del SDK en cada proyecto de la solución.
2. Crear `.editorconfig` con convenciones de estilo y formato aplicables a los tipos de archivo presentes.
3. Crear `.gitignore` con patrones de exclusión de artefactos locales, sin ignorar archivos fuente necesarios.
4. Comprobar la configuración, compilar la solución y registrar resultados por criterio.

### Persistencia y dependencias

No se crea persistencia de aplicación. El contenido de datos locales y logs queda fuera de este trabajo: solo se preparan sus patrones de exclusión. El registro de eventos, la configuración Web y los valores de desarrollo son propiedad de otros trabajos del [plan](../../../plans/mvp0-plan.md#s0-fundaciones-m).

### Garantías técnicas

- El análisis y `nullable` operan sobre los proyectos de [architecture.md → «Capas»](../../../context/architecture.md#capas) sin añadir dependencias entre ellos.
- `.editorconfig` no sustituye tests ni reglas funcionales.
- Los patrones de `.gitignore` excluyen artefactos locales sin ocultar fuentes necesarias para construir la solución.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado | Fuente o referencia | Comprobación o resolución |
| --- | --- | --- | --- |
| Usar analizadores incluidos con el SDK y `nullable` por proyecto | Diseño técnico aprobado; ejecución por comprobar | [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m) | Consultar propiedades efectivas de los proyectos, no exigir paquete independiente. |
| No exigir warnings-as-errors ni estilo como error de build | Límite aprobado para esta tarea | [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m) | Comprobar cero advertencias en build; no afirmar que el estilo falle la compilación. |
| Las exclusiones son reglas para rutas locales | Diseño técnico aprobado; ejecución por comprobar | [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m) | Inspeccionar patrones y su cobertura sobre rutas de ejemplo sin entrar en directorios que empiecen por punto. |

## Estrategia de validación

Los resultados van en [tasks.md](tasks.md). Las comprobaciones de S0.3 no cubren logging, seguridad web ni arranque de páginas. No acceder a directorios cuyo nombre comience por `.` según [AGENTS.md → «Restricción de acceso de Copilot»](../../../AGENTS.md#restricción-de-acceso-de-copilot); `.editorconfig` y `.gitignore` son archivos de raíz, no directorios.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comprobación | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V-01 | CA-01 | Configuración efectiva de proyectos | SDK disponible | Consultar `EnableNETAnalyzers`, `AnalysisLevel` y `Nullable` mediante MSBuild para los proyectos de la solución | Analizadores del SDK habilitados y `nullable` activo en cada proyecto. | SDK, lista de proyectos y propiedades por proyecto. |
| V-02 | CA-02 | Revisión técnica de estilo | `.editorconfig` en raíz | Revisar estructura y reglas aplicables a C# y proyectos; confirmar configuración raíz y ausencia de contradicciones que invaliden el alcance | Base de estilo/formatos definida sin imponer que sugerencias fallen el build. | Secciones y valores revisados, límites de la inspección. |
| V-03 | CA-03 | Revisión técnica de exclusiones | `.gitignore` en raíz | Inspeccionar patrones de artefactos de build, BD y logs locales y configuración local, junto con rutas de ejemplo para fuentes que no deben excluirse | Cobertura declarada de exclusiones requeridas sin patrones generales que oculten fuentes. | Patrones comprobados y nota sobre comprobación real de Git si no se puede realizar sin acceder a directorios prohibidos. |
| V-04 | CA-04 | Compilación integrada | SDK y dependencias del build disponibles | `dotnet build Linkubator.sln --nologo --verbosity minimal` | Código de salida correcto y cero errores/advertencias. | Comando, fecha, SDK y salida de build. |

V-02 y V-03 validan la configuración declarada; no acreditan por sí solos que un editor o Git hayan aplicado reglas a todos los archivos. Si una herramienta necesita acceder a un directorio prohibido para demostrar comportamiento efectivo, detener esa comprobación y dejar el criterio pendiente en vez de fingir su resultado. V-04 puede compilar elementos añadidos en otras etapas: registrar esa limitación sin ampliar el alcance S0.3.

## Orden de ejecución y puerta de salida

- [Tareas](tasks.md): aprobaciones, comprobaciones inmediatas y verificación integrada.
- Cerrar solo con evidencia suficiente para CA-01 a CA-04, todas las tareas necesarias completadas y aceptación humana explícita; archivar es un paso posterior, condicionado al traslado y los enlaces.
- Si una comprobación demuestra una discrepancia entre el alcance aprobado y la configuración real, bloquear la tarea afectada y resolverla antes de aceptar el resultado.