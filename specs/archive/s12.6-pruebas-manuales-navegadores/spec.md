# Especificación: S12.6 — Pruebas manuales en navegadores actuales

## Estado y aprobación

- Estado: aprobada.
- Preparada por IA el 31-X-2026.
- Aprobada por DLG el 31-X-2026 con la referencia «Siguiente».

## Objetivo

Comprobar y registrar manualmente el comportamiento de las páginas públicas y privadas de Linkubator en navegadores actuales, incluyendo formularios, listados, mensajes de error y estados vacíos, conforme a las fuentes funcionales aplicables.

## Fuentes

- [mvp0-plan.md → «S12: Cierre y hardening (M)»](../../../plans/mvp0-plan.md#s12-cierre-y-hardening-m)
- [roadmap.md → «Checklist de calidad del MVP0»](../../../context/roadmap.md#checklist-de-calidad-del-mvp0)
- [mvp0-checklist-test-matrix.md, A12](../../../plans/mvp0-checklist-test-matrix.md)
- [requirements.md → «Colecciones»](../../../context/requirements.md#colecciones)
- [requirements.md → «Enlaces»](../../../context/requirements.md#enlaces)
- [requirements.md → «Búsqueda y filtros»](../../../context/requirements.md#búsqueda-y-filtros)
- [requirements.md → «Páginas públicas»](../../../context/requirements.md#páginas-públicas)
- [requirements.md → «Páginas privadas»](../../../context/requirements.md#páginas-privadas)
- [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros)
- [specifications.md → «Listados»](../../../context/specifications.md#listados)
- [specifications.md → «Rutas privadas»](../../../context/specifications.md#rutas-privadas)
- [specifications.md → «Páginas públicas»](../../../context/specifications.md#páginas-públicas)
- [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado)

## Alcance

- Incluye:
  - Recorrer manualmente las páginas públicas y privadas en los navegadores actuales seleccionados para la ejecución.
  - Comprobar las superficies aplicables de formularios, listados, mensajes de error y estados vacíos, contrastando lo observado con las fuentes funcionales.
  - Registrar resultados por navegador y escenario, incluidos los comportamientos inesperados y los casos que no hayan podido comprobarse.
- Excluye:
  - Automatizar pruebas de UI o usar herramientas de automatización de navegador.
  - Modificar código, configuración o reglas funcionales para corregir hallazgos; cualquier corrección requiere su propio alcance autorizado.
  - Sustituir la revisión específica de accesibilidad asignada a S12.5.
- Trabajo vecino y responsable de sus entregables: S12.1 mantiene la matriz de trazabilidad y la entrada A12; S12.5 realiza la revisión de accesibilidad; S12.8 se ocupa del cierre del roadmap. S12.6 documenta sus propias comprobaciones manuales y no depende de la ejecución de esos trabajos para cerrarse.

## Dependencias y prerrequisitos

- S12.1 está completada y aceptada; la matriz identifica A12 como revisión manual pendiente, asignada a S12.6.
- Para ejecutar la revisión, las páginas y operaciones incluidas en cada escenario deben estar disponibles en un entorno accesible y con datos adecuados. La preparación concreta se definirá en el plan técnico.
- La fuente exige navegadores actuales, pero no enumera productos ni versiones. El plan identificará los navegadores seleccionados para la ejecución y la evidencia registrará sus versiones; esto no define una matriz permanente de compatibilidad.
- La revisión de accesibilidad de S12.5 es un trabajo independiente y no constituye un prerrequisito.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [roadmap.md → «Checklist de calidad del MVP0»](../../../context/roadmap.md#checklist-de-calidad-del-mvp0), [requirements.md → «Páginas públicas»](../../../context/requirements.md#páginas-públicas), [requirements.md → «Páginas privadas»](../../../context/requirements.md#páginas-privadas), [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado) | Están disponibles las páginas públicas y privadas que correspondan a los escenarios de la revisión | Se recorren manualmente en los navegadores seleccionados | Se comprueba y registra el comportamiento de ambas áreas frente a sus fuentes, incluida la ausencia de exposición pública de contenido privado; los casos no verificables quedan identificados, no se presentan como satisfactorios. |
| CA02 | [roadmap.md → «Checklist de calidad del MVP0»](../../../context/roadmap.md#checklist-de-calidad-del-mvp0), [requirements.md → «Colecciones»](../../../context/requirements.md#colecciones), [requirements.md → «Enlaces»](../../../context/requirements.md#enlaces), [requirements.md → «Búsqueda y filtros»](../../../context/requirements.md#búsqueda-y-filtros), [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros), [specifications.md → «Listados»](../../../context/specifications.md#listados), [specifications.md → «Rutas privadas»](../../../context/specifications.md#rutas-privadas) | Hay formularios, listados, mensajes de error y estados vacíos aplicables a las áreas revisadas | Se recorren manualmente sus escenarios representativos en los navegadores seleccionados | Cada categoría aplicable se contrasta con su fuente funcional y se registra su resultado; una categoría no disponible o no comprobada queda explícitamente identificada. |
| CA03 | [mvp0-plan.md → «S12: Cierre y hardening (M)»](../../../plans/mvp0-plan.md#s12-cierre-y-hardening-m), [mvp0-checklist-test-matrix.md, A12](../../../plans/mvp0-checklist-test-matrix.md) | Se ha ejecutado un escenario manual o se ha determinado que no puede ejecutarse | Se consolida la evidencia de la revisión | Cada resultado identifica navegador y versión, área y escenario, observación y estado; los hallazgos y límites quedan registrados y no se declara satisfactoria una comprobación sin evidencia. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguna pregunta bloqueante identificada. La selección concreta de navegadores y versiones se documentará en el plan antes de ejecutar la revisión. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md) — aprobado por DLG el 31-X-2026 con la referencia «Siguiente».
- [Tareas](tasks.md) — pendientes de aprobación.
