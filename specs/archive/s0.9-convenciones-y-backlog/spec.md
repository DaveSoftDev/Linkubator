# Especificación: S0.9 — Convenciones de trabajo y referencia al backlog

## Estado y aprobación

- Estado: Aprobado por DLG el 7-X-2026.
- [Especificación](spec.md): Convenciones correctas.
- Este documento define el resultado esperado; las comprobaciones del estado actual y sus límites se registran en [tasks.md](tasks.md).

## Objetivo

Dejar accesibles y coherentes las pautas de colaboración del repositorio y la referencia al plan de trabajo, sin duplicar el backlog ni presentarlo como acordado por alguien distinto de quien lo aprobó.

## Fuentes

- [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m): alcance y responsable de la tarea S0.9.
- [mvp0-plan.md → «Resumen de sprints»](../../../plans/mvp0-plan.md#2-resumen-de-sprints): plan que contiene el backlog principal.
- [CONTRIBUTING.md](../../../CONTRIBUTING.md): convenciones operativas de colaboración.
- [README.md](../../../README.md): acceso a guía, convenciones y plan.
- [roadmap.md](../../../context/roadmap.md): orden y cierre de etapas.
- [AGENTS.md → «Reglas de documentación»](../../../AGENTS.md#reglas-de-documentación): fuente única, trazabilidad y alcance.

## Alcance

- Incluye: comprobar y armonizar la presentación de convenciones de ramas, commits y pull requests entre `CONTRIBUTING.md` y README; enlazar el backlog existente del plan principal y distinguirlo de la fuente funcional.
- Excluye: inventar o modificar el backlog/sprints, políticas de CI/CD, gestión de Git o configuración del repositorio, y definir convenciones adicionales no respaldadas por el plan/documentos actuales.
- Trabajo vecino: el plan principal y roadmap son propietarios del orden y contenido de las etapas; `CONTRIBUTING.md` es la guía detallada de colaboración; README ofrece una síntesis/enlaces. S0.9 no copia sprints dentro de otro documento.

## Dependencias y prerrequisitos

- El plan principal existe en `plans/mvp0-plan.md`; la guía `CONTRIBUTING.md` y el README están en la raíz.
- La ruta del backlog deberá resolverse desde ambas ubicaciones.
- S0.9 no requiere manipular ramas, commits ni consultar el historial de Git; las validaciones se limitan a documentos y estructura legible.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [Plan principal → S0.9](../../../plans/mvp0-plan.md#s0-fundaciones-m); [CONTRIBUTING.md](../../../CONTRIBUTING.md) | Las convenciones de colaboración existentes. | Se contrastan `CONTRIBUTING.md` y el resumen del README. | Ramas, commits y pull requests se describen de forma coherente y enlazada, sin reglas contradictorias. |
| CA02 | [Plan principal → «Resumen de sprints»](../../../plans/mvp0-plan.md#2-resumen-de-sprints); [roadmap.md](../../../context/roadmap.md) | El backlog y orden ya definidos en sus fuentes. | Se revisan las referencias desde CONTRIBUTING y README. | Ambos orientan a la fuente del backlog/roadmap sin copiar el listado de sprints ni redefinir su orden. |
| CA03 | [Plan principal → S0.9](../../../plans/mvp0-plan.md#s0-fundaciones-m); [AGENTS.md → «Reglas de documentación»](../../../AGENTS.md#reglas-de-documentación) | Los documentos del repositorio y sus enlaces locales. | Se revisan los enlaces y el contenido de alcance. | Las referencias resuelven, no se añaden reglas de producto, alcance funcional ni compromisos ajenos a S0.9. |
| CA04 | [Plan principal → «3. Definition of Done común (aplica a todos los sprints)»](../../../plans/mvp0-plan.md#3-definition-of-done-común-aplica-a-todos-los-sprints) | La solución tras la revisión documental. | Se ejecutan build y pruebas disponibles. | El build compila sin warnings/errors y la suite descubierta pasa. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| El plan asigna al humano la responsabilidad de acordar las convenciones. Una aprobación de DLG en esta tarea sirve como evidencia de acuerdo presente, pero no se presupone un acuerdo histórico. | [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m) | Ninguno para comprobar documentos actuales. | Ninguna adicional; registrar la aprobación real de DLG y no inventar historial. |

## Artefactos relacionados

- [Plan técnico](plan.md).
- [Tareas y evidencia](tasks.md).
