# Especificación: S0.3 — Configuración base del repositorio

## Estado y aprobación

- Estado: Aprobado por DLG el 7-X-2026.
- Referencia: Cumple lo esperado.
- Este documento describe el resultado esperado; la evidencia obtenida tras construirlo pertenece a [tasks.md](tasks.md).

## Objetivo

Configurar la base de análisis y estilo de los proyectos y excluir del repositorio los artefactos locales indicados en [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m), sin incorporar comportamiento de producto.

## Fuentes

- [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m): tarea S0.3 y su reparto con los trabajos siguientes.
- [mvp0-plan.md → «3. Definition of Done común (aplica a todos los sprints)»](../../../plans/mvp0-plan.md#3-definition-of-done-común-aplica-a-todos-los-sprints): comprobación de compilación.
- [architecture.md → «Plataforma»](../../../context/architecture.md#plataforma): entorno de proyectos sobre el que se aplica la configuración.
- [AGENTS.md → «Reglas de documentación»](../../../AGENTS.md#reglas-de-documentación): límites de este documento derivado.

## Alcance

- Incluye: habilitar análisis y `nullable` en los proyectos, definir convenciones compartidas en `.editorconfig` y excluir artefactos de compilación, datos locales, logs y configuración local mediante `.gitignore`.
- Excluye: Serilog y su salida, Razor Pages, Tailwind, seguridad y arranque web, valores de entorno, casos de uso, reglas de dominio y despliegue; pertenecen a otros trabajos del [plan](../../../plans/mvp0-plan.md#s0-fundaciones-m).
- No se exige en este trabajo que todas las advertencias sean errores ni que cada sugerencia de estilo falle en build; la condición de compilación sin advertencias se comprueba por separado en la Definition of Done.

## Dependencias y prerrequisitos

- La estructura de solución y proyectos procede de [S0.2](../s0.2-crear-solucion/spec.md).
- Para compilar se requiere el SDK previsto en [architecture.md → «Plataforma»](../../../context/architecture.md#plataforma).

## Criterios de aceptación

| ID | Fuente propietaria | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [Plan → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m) | Los proyectos de la solución preparados en S0.2. | Se consulta la configuración efectiva del SDK/MSBuild para cada proyecto. | `nullable` está habilitado y los analizadores del SDK participan en el análisis de los proyectos. |
| CA02 | [Plan → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m) | Los archivos de la solución. | Se comprueban las convenciones de estilo aplicables. | `.editorconfig` define una base compartida de estilo y formato para los archivos del proyecto. |
| CA03 | [Plan → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m) | El repositorio de desarrollo. | Se comprueban las exclusiones declaradas y efectivas para rutas de prueba no creadas. | `.gitignore` excluye artefactos de compilación, datos y logs locales, así como configuración local; no excluye indiscriminadamente archivos fuente. |
| CA04 | [Plan → «3. Definition of Done común (aplica a todos los sprints)»](../../../plans/mvp0-plan.md#3-definition-of-done-común-aplica-a-todos-los-sprints) | La solución configurada y el SDK disponible. | Se compila la solución con la configuración base. | El build finaliza sin errores ni advertencias. |

## Bloqueos y preguntas pendientes

- Las comprobaciones y el cierre se registran en [tasks.md](tasks.md); aprobar este texto no acredita que la configuración cumpla los criterios.

## Artefactos relacionados

- [Plan técnico](plan.md).
- [Tareas y evidencia](tasks.md).