# Especificación: S7.11 — Validación del estilo base

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 24-X-2026 («Siguiente»).

## Objetivo

Obtener una revisión humana del estilo base compartido de la zona privada, documentar la evaluación y los ajustes solicitados, si los hay, y registrar la decisión de aceptación del estilo.

## Fuentes

- [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l): asigna a S7.11 la validación del estilo base y la solicitud de ajustes; la Definition of Done requiere aceptación humana.
- [requirements.md → «Accesibilidad»](../../context/requirements.md#accesibilidad): expectativas de accesibilidad que deben considerarse en la revisión visual.
- [S7.4 — Layout privado accesible → CA01–CA06](../s7.4-layout-privado/spec.md#criterios-de-aceptación): criterios aprobados del layout privado compartido.
- [S7.5 — Componentes de presentación accesible → CA01–CA06](../s7.5-componentes-presentacion/spec.md#criterios-de-aceptación): criterios aprobados de los componentes de presentación comunes.
- [S7.10 — Revisión manual del dashboard → alcance](../s7.10-revision-manual-dashboard/spec.md#alcance): límite entre la revisión específica del dashboard y esta validación del estilo base.

## Alcance

- **Incluye:** revisión humana del estilo base compartido actualmente disponible en la zona privada: layout de S7.4 y componentes reutilizables de S7.5, en las páginas y estados existentes que los consumen; evaluación del resultado visual y de su coherencia como base común; consideración de las expectativas de accesibilidad de la fuente; registro del entorno, los elementos revisados, los hallazgos, los ajustes solicitados y la decisión humana final.
- **Excluye:** implementación de cambios de estilo o de componentes; revisión de reglas funcionales y de contratos HTTP; una auditoría completa de accesibilidad con tecnologías de asistencia; revisión de páginas públicas, rutas o estados que aún no existan; repetir como objetivo principal la revisión del dashboard de teclado, foco, contraste y encabezados de S7.10; despliegue.
- **Trabajo vecino y responsable de sus entregables:** S7.4 aporta el layout privado y S7.5 los componentes comunes; S7.10 revisa específicamente el dashboard. S7.11 evalúa la base visual compartida y registra si se solicitan ajustes; cualquier implementación de esos ajustes requiere trabajo autorizado separado.

## Dependencias y prerrequisitos

- S7.4 y S7.5 están completadas y aceptadas; el layout y la base de componentes pueden revisarse en el entorno de desarrollo.
- S7.10 está completada y aceptada; sus resultados son contexto sobre el dashboard, pero no sustituyen la evaluación del estilo compartido ni son un requisito para volver a ejecutar sus comprobaciones.
- Debe estar disponible una instancia Web con las páginas y componentes existentes. El entorno, las páginas o estados representativos y el método para registrar observaciones se concretarán en el plan técnico.
- La validación puede terminar con ajustes solicitados. Esos cambios no se implementan dentro de esta especificación ni condicionan su cierre si la revisión y su decisión quedan registradas.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l), [S7.4 → CA01–CA06](../s7.4-layout-privado/spec.md#criterios-de-aceptación) y [S7.5 → CA01–CA06](../s7.5-componentes-presentacion/spec.md#criterios-de-aceptación) | El layout compartido y los componentes de presentación existentes están disponibles para inspección. | Se revisa el estilo base en las páginas y estados existentes que lo utilizan, identificando los elementos examinados y el entorno. | Queda registrado qué partes de la base común se revisaron y qué limitaciones de cobertura hubo; no se presenta como examinada una página, componente o estado que no estuviera disponible. |
| CA02 | [Requisitos → «Accesibilidad»](../../context/requirements.md#accesibilidad), [S7.4 → CA02–CA05](../s7.4-layout-privado/spec.md#criterios-de-aceptación) y [S7.5 → CA01–CA06](../s7.5-componentes-presentacion/spec.md#criterios-de-aceptación) | Se inspeccionan los elementos visuales comunes cubiertos por CA01. | Se considera su presentación frente a las expectativas de accesibilidad aplicables y se documenta cualquier hallazgo observado. | Cada observación queda vinculada al elemento y estado revisados, o se deja constancia explícita de que no se encontraron hallazgos; la revisión no se presenta como una auditoría completa de accesibilidad. |
| CA03 | [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l) | La revisión del estilo base ha producido una evaluación, con o sin hallazgos. | El revisor decide si solicita ajustes. | La decisión y, cuando corresponda, cada ajuste solicitado quedan registrados con ubicación y descripción suficientes para su seguimiento; no se atribuye a S7.11 la implementación de esos ajustes. |
| CA04 | [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l) | La evaluación y los hallazgos o su ausencia están documentados. | El humano revisa el estilo base y emite una decisión explícita. | La aceptación o la solicitud de ajustes queda registrada con responsable, fecha y referencia; si no hay aceptación, no se declara aceptado el estilo base. |

Los casos negativos incluyen estados comunes con problemas visuales o de accesibilidad y cualquier componente o página prevista para revisar que no esté disponible; se registran como hallazgo o limitación, respectivamente. La cobertura queda acotada a las partes existentes que se hayan inspeccionado y no implica conformidad completa de toda la aplicación.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para redactar el borrador. El entorno, las páginas/estados representativos y el formato de evidencia se precisarán en el plan técnico, sin ampliar el alcance a páginas futuras ni a la implementación de ajustes. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 24-X-2026 («Siguiente»).
- [Tareas, evidencia y cierre](tasks.md#aceptación-y-cierre): aprobadas por DLG el 24-X-2026 («Vamos»); revisión aceptada y completada por DLG el 24-X-2026 («Pues por mi lo daría por finalizado a falta de pulir flecos de diseño»); los flecos quedan como seguimiento separado y no se declara que estén implementados.
