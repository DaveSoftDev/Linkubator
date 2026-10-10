# Especificación: S0.2 — Crear solución y referencias entre capas

## Estado y aprobación

- Estado: Aprobado por DLG el 7-X-2026.
- Referencia: Leído y comprendido.
- Esta especificación describe lo que S0.2 debe entregar; los resultados de su ejecución se registran en [tasks.md](tasks.md).

## Objetivo

Crear una solución compilable con los proyectos, referencias y punto de composición previstos para [S0.2](../../../plans/mvp0-plan.md#s0-fundaciones-m), sin incorporar comportamiento funcional.

## Fuentes

- [architecture.md → «Tecnologías»](../../../context/architecture.md#tecnologías): tecnologías previstas.
- [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura): responsabilidades de cada capa de la arquitectura.
- [architecture.md → «Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos): matriz vinculante, Tests fuera del grafo productivo y límite del composition root.
- [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m): alcance de S0.2 y finalización del bloque.
- [mvp0-plan.md → «3. Definition of Done común (aplica a todos los sprints)»](../../../plans/mvp0-plan.md#3-definition-of-done-común-aplica-a-todos-los-sprints): compilación y tests del sprint.
- [AGENTS.md → «Reglas de documentación»](../../../AGENTS.md#reglas-de-documentación): trazabilidad y fuente única.

## Alcance

- Incluye: crear la solución y los proyectos previstos, agregarlos a la solución, establecer su arquitectura y referencias, y preparar la composición de Infrastructure desde Web.
- Excluye: analizadores, configuración del entorno, logs, Tailwind, páginas funcionales, adaptadores, persistencia y reglas de dominio.
- La configuración de xUnit, la autoría de las pruebas de arquitectura y la verificación del arranque Web no forman parte de este trabajo. S0.2 deja preparada la estructura de Tests.

## Dependencias y prerrequisitos

- Estar disponible el SDK previsto en [architecture.md → «Tecnologías»](../../../context/architecture.md#tecnologías) para crear y compilar los proyectos.
- Si una condición ajena a este trabajo impide comprobar un criterio, registrarla en [tasks.md](tasks.md); no ampliar el alcance para subsanarla sin acuerdo.

## Criterios de aceptación

Cada criterio deriva de las fuentes anteriores; la matriz de referencias permanece únicamente en `architecture.md`.

| ID | Fuente propietaria | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [Plan → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m) | La tarea S0.2 autorizada. | Se crea la solución y se enumeran sus proyectos. | Los proyectos previstos por la tarea figuran en la solución. |
| CA02 | [Arquitectura → «Tecnologías»](../../../context/architecture.md#tecnologías) y [plan → «Definition of Done común»](../../../plans/mvp0-plan.md#3-definition-of-done-común-aplica-a-todos-los-sprints) | Los proyectos recién creados y el SDK disponible. | Se inspeccionan framework y SDK y se compila la solución. | Su tecnología es la prevista y la compilación no presenta errores ni warnings. |
| CA03 | [Arquitectura → «Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos) | Los proyectos productivos. | Se comprueban sus referencias declaradas. | Respetan la matriz propietaria y no aparecen dependencias prohibidas. |
| CA04 | [Arquitectura → «Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos) | El proyecto de pruebas y los productivos. | Se comprueban las referencias en ambos sentidos. | Tests puede validar producción y ningún proyecto productivo depende de Tests. |
| CA05 | [Arquitectura → «Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos) | La capa Web y su arranque. | Se revisan el consumo de tipos de Infrastructure y la composición del host. | El consumo de Infrastructure se limita al composition root, que lo registra al arrancar; el resto de Web no consume sus implementaciones. |

## Bloqueos y preguntas pendientes

- La especificación y el [plan técnico](plan.md) están aprobados por DLG el 7-X-2026. Los posibles impedimentos de ejecución se anotarán en [tasks.md](tasks.md) cuando se comprueben.

## Artefactos relacionados

- [Plan técnico y estrategia de validación](plan.md).
- [Tareas y evidencia](tasks.md).