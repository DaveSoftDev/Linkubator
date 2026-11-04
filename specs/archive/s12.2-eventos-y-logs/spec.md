# Especificación: S12.2 — Pruebas de eventos y ausencia de datos prohibidos en logs

## Estado y aprobación

- Estado: aprobada.
- Aprobada por DLG el 31-X-2026 con la referencia «Siguiente».

## Objetivo

Demostrar mediante pruebas y evidencia actual que el registro del MVP0 llega a sus salidas previstas, cubre los eventos aplicables y no expone en ellos datos prohibidos por las reglas vigentes.

## Fuentes

- [roadmap.md → «Checklist de calidad del MVP0»](../../context/roadmap.md#checklist-de-calidad-del-mvp0)
- [mvp0-plan.md → «S12: Cierre y hardening (M)»](../../plans/mvp0-plan.md#s12-cierre-y-hardening-m)
- [mvp0-checklist-test-matrix.md](../../plans/mvp0-checklist-test-matrix.md), filas A15–A18
- [architecture.md → «Logs»](../../context/architecture.md#logs)
- [specifications.md → «Registro de eventos»](../../context/specifications.md#registro-de-eventos)
- [decisions.md → «Pendiente de rutas de cuenta»](../../context/decisions.md#pendiente-de-rutas-de-cuenta)
- [decisions.md → «Fuera de alcance del MVP0»](../../context/decisions.md#fuera-de-alcance-del-mvp0)

## Alcance

- Incluye:
  - Comprobar la disponibilidad efectiva de logs por consola y archivo rotativo.
  - Reconciliar los eventos aplicables a las operaciones actuales del MVP0 con las reglas de registro y aportar pruebas para sus datos y excepciones.
  - Comprobar que los valores prohibidos no aparecen en mensajes, propiedades estructuradas, excepciones ni logs de peticiones del MVP0.
  - Distinguir los datos prohibidos de los contenidos introducidos por el usuario que las reglas permiten registrar, aunque su apariencia se parezca a un secreto.
- Excluye:
  - Cambiar las reglas de registro o imponer una sanitización general del contenido introducido por el usuario.
  - Probar operaciones de cuenta, transporte de tokens o scraping automático que no forman parte del alcance actual del MVP0.
  - La auditoría de invariantes críticas, la sincronización documental general, las revisiones manuales y el cierre de S12, asignados a otras tareas.
- Trabajo vecino y responsable de sus entregables: S12.1 mantiene la matriz de trazabilidad; S12.3 audita las invariantes críticas; S12.4 sincroniza la documentación; S12.5 y S12.6 corresponden a revisiones humanas; S12.7 y S12.8 revisan riesgos y cierran el MVP0. Esta spec no depende de la ejecución de esos trabajos.

## Dependencias y prerrequisitos

- S12.1 está completada y aceptada; su matriz asigna a S12.2 las comprobaciones A15–A18.
- Las reglas de registro y las salidas previstas están definidas en las fuentes enlazadas. Las rutas y operaciones de cuenta aún pendientes de definición no son prerrequisito ni bloquean el cierre de esta spec.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [architecture.md → «Logs»](../../context/architecture.md#logs), checklist A15–A16 de [mvp0-checklist-test-matrix.md](../../plans/mvp0-checklist-test-matrix.md) | La aplicación ejecuta una operación del MVP0 que genera un log | Se inspeccionan sus salidas efectivas | El log está disponible por consola y en el archivo rotativo; la evidencia comprueba la salida real, no solo su configuración. |
| CA02 | [specifications.md → «Registro de eventos»](../../context/specifications.md#registro-de-eventos), checklist A17 de [mvp0-checklist-test-matrix.md](../../plans/mvp0-checklist-test-matrix.md) | Se ha identificado el registro de eventos y excepciones producido por las operaciones actuales del MVP0 | Se contrasta con las reglas y se ejecutan las pruebas correspondientes | Cada comportamiento de registro aplicable a esas operaciones tiene una prueba que verifica el evento y los datos permitidos; no se atribuye cobertura por el nombre del test o por el resultado global de la suite. |
| CA03 | [specifications.md → «Registro de eventos»](../../context/specifications.md#registro-de-eventos), [architecture.md → «Logs»](../../context/architecture.md#logs), checklist A18 de [mvp0-checklist-test-matrix.md](../../plans/mvp0-checklist-test-matrix.md) | Se ejecutan las operaciones actuales del MVP0 con valores de prueba que permiten reconocer datos prohibidos en los logs | Se inspeccionan los mensajes, las propiedades, las excepciones y los logs de peticiones generados | No aparece ningún valor prohibido en esas superficies; la comprobación no clasifica como prohibido un contenido introducido por el usuario que las reglas permiten registrar por su uso. |
| CA04 | [specifications.md → «Registro de eventos»](../../context/specifications.md#registro-de-eventos) | Las pruebas de CA03 comprueban la ausencia de datos prohibidos | Se valida también la capacidad de detección de esas comprobaciones | Un control negativo, aislado de la ejecución normal del producto, demuestra que la comprobación detecta un valor prohibido cuando se introduce deliberadamente en una superficie de log inspeccionada. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguna pregunta de alcance identificada en las fuentes disponibles. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md) — propuesta, pendiente de aprobación.
