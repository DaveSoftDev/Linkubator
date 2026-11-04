# Especificación: S12.8 — Cierre del MVP0

## Estado y aprobación

- Estado: aprobada.
- Preparada por IA el 31-X-2026.
- Aprobada por DLG el 31-X-2026 con la referencia «Siguiente».

## Objetivo

Cerrar el MVP0 una vez satisfecha su Definition of Done: reflejar el cierre en el roadmap, etiquetar la versión y realizar la retrospectiva final, dejando evidencia trazable de estas acciones.

## Fuentes

- [mvp0-plan.md → «S12: Cierre y hardening (M)»](../../../plans/mvp0-plan.md#s12-cierre-y-hardening-m)
- [roadmap.md → «Checklist de calidad del MVP0»](../../../context/roadmap.md#checklist-de-calidad-del-mvp0)
- [roadmap.md → «Hecho»](../../../context/roadmap.md#hecho)

## Alcance

- Incluye:
  - Comprobar que todos los puntos de la Definition of Done de S12 tienen evidencia satisfactoria antes de cerrar el MVP0.
  - Actualizar el estado del MVP0 en el roadmap para reflejar su cierre.
  - Crear la etiqueta de versión correspondiente al cierre.
  - Realizar la retrospectiva final y dejar constancia de su realización en la evidencia de cierre.
- Excluye:
  - Completar, sustituir o aceptar evidencias de las tareas S12.1–S12.7.
  - Corregir hallazgos de auditoría o implementar cambios de producto como parte del cierre.
  - Declarar cerrado el MVP0 si algún punto de la Definition of Done sigue pendiente o sin evidencia.
- Trabajo vecino y responsable de sus entregables: las tareas S12.1–S12.7 son responsables de sus propias comprobaciones y evidencias; S12.8 consume ese estado y ejecuta el cierre humano.

## Dependencias y prerrequisitos

- Antes de iniciar el cierre, deben estar completados y respaldados los puntos de la Definition of Done que preceden a S12.8; actualizar el roadmap es el último punto y forma parte de este trabajo.
- La identificación concreta de la etiqueta de versión se acordará antes de crearla; las fuentes disponibles no fijan su nombre.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [S12 — Definition of Done](../../../plans/mvp0-plan.md#s12-cierre-y-hardening-m); [Checklist de calidad del MVP0](../../../context/roadmap.md#checklist-de-calidad-del-mvp0) | La revisión final de S12 está preparada | Se contrasta la evidencia de las tareas anteriores a S12.8 con los puntos aplicables de la Definition of Done | Todos esos puntos están completados y respaldados; si falta alguno, el cierre queda pendiente. |
| CA02 | [S12.8](../../../plans/mvp0-plan.md#s12-cierre-y-hardening-m); [roadmap.md → «Hecho»](../../../context/roadmap.md#hecho) | Los prerrequisitos de CA01 están satisfechos | Se actualiza el roadmap y se completa el punto de cierre correspondiente | El MVP0 figura en «Hecho», S12 deja de figurar como etapa actual y su punto de la Definition of Done queda completado con evidencia. |
| CA03 | [S12.8](../../../plans/mvp0-plan.md#s12-cierre-y-hardening-m) | El cierre está autorizado y la identificación de versión está acordada | Se etiqueta la versión del MVP0 | La etiqueta existe y apunta a la revisión que contiene el cierre aceptado. |
| CA04 | [S12.8](../../../plans/mvp0-plan.md#s12-cierre-y-hardening-m) | Se ha alcanzado el cierre del MVP0 | Se realiza la retrospectiva final | La evidencia de cierre registra que tuvo lugar. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| ¿Qué identificador concreto se utilizará para la etiqueta de versión? | [S12.8](../../../plans/mvp0-plan.md#s12-cierre-y-hardening-m) | CA03 | Resuelta el 02-XI-2026: DLG acordó el identificador; la evidencia consta en [T06 de tasks.md](tasks.md#descomposición). |

## Artefactos relacionados

- [Plan técnico](plan.md) — aprobado por DLG el 31-X-2026 con la referencia «Siguiente».
- [Tareas](tasks.md) — aprobadas por DLG el 31-X-2026 («Vamos»); secuencia revisada aprobada el 02-XI-2026 («Aprobar las tareas revisadas») y actualización de evidencia de retrospectiva aprobada el 02-XI-2026 («Aprobar las tareas actualizadas»).
