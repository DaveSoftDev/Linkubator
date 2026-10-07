# Plan técnico: S0.2 — Crear solución y referencias entre capas

## Estado y aprobación del plan

- Estado: aprobado.
- Especificación: [spec.md](spec.md).
- Aprobaciones:
  - Especificación: DLG, 07-X-2026 («Leído y comprendido»).
  - Plan y tareas: DLG, 07-X-2026 («Plan revisado y correcto»). Esta aprobación no implica ejecución de tareas ni aceptación del resultado.

## Diseño de implementación

### Componentes y contratos

Crear la solución `Linkubator.sln`, con los proyectos productivos en `src/` y Tests en `tests/`. Seleccionar SDK y framework a partir de [architecture.md → «Plataforma»](../../../context/architecture.md#plataforma). Sus responsabilidades y la matriz vinculante se consultan en [architecture.md → «Capas»](../../../context/architecture.md#capas) y [«Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos); este plan no reproduce la matriz.

Situar el composition root en `Web/Program.cs`. Preparar un único punto público de registro `AddInfrastructure()` en Infrastructure, que reciba y devuelva `IServiceCollection`, e invocarlo antes de construir el host. Las implementaciones concretas no se expondrán como API pública para el resto de Web; el flujo de negocio seguirá por Application.

Crear la estructura del proyecto Tests y sus referencias a producción sin convertirlo en dependencia productiva, según [architecture.md → «Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos). La configuración de xUnit y la autoría de las pruebas quedan fuera de este diseño. No se implementan aquí adaptadores, casos de uso, páginas ni persistencia.

### Flujos internos

1. Crear los proyectos y agregarlos a la solución con sus referencias según la matriz fuente.
2. Preparar en Infrastructure el punto público de registro y llamarlo desde `Web/Program.cs` antes de construir el host.
3. Compilar la solución y revisar su estructura y composición conforme a los criterios.

### Persistencia y dependencias

S0.2 solo prepara la separación de dependencias. No incorpora persistencia ni adaptadores. Serilog, Tailwind y la configuración de páginas quedan fuera de sus tareas; el build de S0.2 se comprobará sobre la solución mínima de este bloque.

### Garantías técnicas

- Las referencias productivas y la relación de Tests con producción siguen [architecture.md → «Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos).
- Solo el composition root de Web consume Infrastructure; no se infiere la garantía a partir de un `using` ni únicamente de la matriz entre proyectos.
- El proyecto Tests no se añade como dependencia de ninguno de los proyectos productivos.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado | Fuente o referencia | Comprobación o resolución |
| --- | --- | --- | --- |
| Registrar Infrastructure mediante `AddInfrastructure()` en `Program` | Diseño técnico aprobado con el plan | [architecture.md → «Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos) | Comprobar responsabilidad, API pública y orden de registro. |
| La estructura de Tests forma parte de S0.2; la configuración y las pruebas, no | Alcance definido por el plan | [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m) | Evitar atribuir a S0.2 las pruebas de arquitectura del bloque posterior. |

## Estrategia de validación

Estas verificaciones se realizarán al implementar S0.2. Sus resultados se registrarán en [tasks.md](tasks.md); la aprobación del plan no acredita su ejecución. Las pruebas automatizadas de arquitectura no forman parte de este trabajo.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comprobación | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Inspección de solución | SDK disponible | `dotnet sln Linkubator.sln list`; contrastar rutas y existencia de proyectos | Proyectos previstos presentes y listados. | Listado y fecha de ejecución. |
| V02 | CA02 | Inspección y compilación | SDK disponible | Revisar SDK/TFM y ejecutar `dotnet build Linkubator.sln` | Plataforma de [architecture.md → «Plataforma»](../../../context/architecture.md#plataforma) y compilación sin errores ni warnings según [DoD común](../../../plans/mvp0-plan.md#3-definition-of-done-común-aplica-a-todos-los-sprints). | SDK empleado y salida del build. |
| V03 | CA03, CA04 | Revisión de proyectos | Proyectos creados | Inspeccionar las referencias declaradas en los proyectos y cotejarlas con [architecture.md → «Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos) | Ninguna referencia prohibida; Tests fuera del grafo productivo. | Matriz de comparación y revisión. |
| V04 | CA05 | Revisión técnica | Composition root preparado | Revisar consumidores de tipos de Infrastructure en Web, firma del punto de registro y orden de invocación en `Program` | Solo el composition root consume Infrastructure y registra antes de construir el host. | Revisión de dependencias y orden de composición. |

La revisión estructural de V03 y V04 deja identificados los límites para las pruebas automatizadas de arquitectura previstas más adelante en el [plan de fundaciones](../../../plans/mvp0-plan.md#s0-fundaciones-m); no se exige que esos tests existan en S0.2.

## Orden de ejecución y puerta de salida

- [Tareas de implementación](tasks.md): aprobaciones, incrementos pequeños y verificaciones inmediatas.
- Cerrar solo con resultados registrados para todos los criterios, sin bloqueos, con tareas completadas y aceptación humana explícita; mover a `archive/` requiere además una petición inequívoca y la verificación de enlaces de [sdd-writer](../../../skills/sdd-writer/SKILL.md).
- Una ambigüedad que afecte al alcance o a las fuentes detiene la tarea afectada y se resuelve en el documento propietario antes de continuar.