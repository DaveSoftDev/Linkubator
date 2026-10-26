# Especificación: S8.6 — Revisión manual de colecciones

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 25-X-2026 («Siguiente»).

## Objetivo

Registrar una revisión manual reproducible de los flujos privados de colecciones asignados a S8.6, comprobando las operaciones funcionales desde el navegador y dejando constancia de sus resultados y de la decisión humana sobre la revisión.

## Fuentes

- [mvp0-plan.md → «S8: Gestión de colecciones (L)»](../../../plans/mvp0-plan.md): alcance de S8.6 y criterios de finalización de S8.
- [requirements.md → «Colecciones»](../../../context/requirements.md): operaciones que el usuario puede realizar y límites de eliminación.
- [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md), [«Listados»](../../../context/specifications.md) y [«Rutas privadas»](../../../context/specifications.md): comportamiento esperado del listado, las rutas y sus operaciones.
- [domain-model.md → «Collection»](../../../context/domain-model.md) y [«Propiedad de los datos»](../../../context/domain-model.md): restricciones permanentes de las colecciones revisadas.
- [S8.1 — Listado de colecciones](../s8.1-listado-colecciones/spec.md), [S8.2 — Operaciones de colecciones](../../s8.2-operaciones-colecciones/spec.md), [S8.3 — Errores de formularios de colecciones](../s8.3-errores-formularios-colecciones/spec.md), [S8.4 — Identificadores y respuestas de colecciones](../s8.4-identificadores-y-respuestas-colecciones/spec.md) y [S8.5 — Pruebas de integración HTTP de colecciones](../s8.5-pruebas-integracion-http-colecciones/spec.md): límites de los flujos ya definidos y comprobaciones automatizadas vecinas; esta revisión no sustituye sus entregables.

## Alcance

- **Incluye:** revisión manual en navegador de crear una colección, cambiar su nombre mediante edición, buscar y recorrer páginas del listado, eliminar una colección elegible y comprobar los rechazos por las restricciones de borrado; registro de entorno, pasos y resultados.
- **Excluye:** cambios de código o reglas, pruebas automatizadas, revisión detallada de textos y mensajes, evaluación de accesibilidad de formularios asignada a S8.7, validación de seguridad transversal, despliegue y aceptación de trabajos vecinos.
- **Trabajo vecino y responsable de sus entregables:** S8.1 entrega el listado; S8.2, las operaciones; S8.3, la presentación de errores de formulario; S8.4, las respuestas de identificadores; S8.5, las pruebas HTTP; S8.7 realiza la revisión manual de textos, mensajes y accesibilidad. S8.6 ejecuta una revisión funcional manual acotada y puede cerrarse con su propia evidencia, sin depender de la aceptación o cierre formal de esas tareas vecinas.

## Dependencias y prerrequisitos

- Deben estar disponibles en el entorno de revisión la zona privada y las páginas de listado, creación, edición y eliminación de colecciones. S8.2 tiene implementación presente, pero su cierre técnico continúa pendiente según el roadmap; esto no se interpreta como aceptación previa de sus operaciones.
- Las fuentes funcionales y los criterios propietarios de S8.1–S8.4 determinan los resultados esperados. Si se observa una discrepancia, se registra con pasos reproducibles y se escala al trabajo o documento propietario; S8.6 no cambia reglas ni corrige la implementación.
- La revisión no requiere evidencia de S8.7 ni una ejecución de la suite automatizada completa. Sus resultados no sustituyen las pruebas de integración HTTP de S8.5.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [S8 del plan](../../../plans/mvp0-plan.md), [«Colecciones»](../../../context/requirements.md), [AC-COL-01](../../../plans/mvp0-acceptance-criteria.md) y [AC-COL-02](../../../plans/mvp0-acceptance-criteria.md) | Se dispone de la zona privada y de una cuenta con colecciones que puedan usarse para la revisión. | Se crea una colección y se cambia el nombre de una colección propia desde sus formularios canónicos. | La revisión registra los pasos y resultados observables, y confirma que las operaciones y el estado final corresponden a sus criterios propietarios; cualquier discrepancia queda documentada sin corregirse en este trabajo. |
| CA02 | [S8 del plan](../../../plans/mvp0-plan.md), [AC-COL-05](../../../plans/mvp0-acceptance-criteria.md), [«Búsqueda y filtros»](../../../context/specifications.md) y [«Listados»](../../../context/specifications.md) | El listado contiene colecciones que permitan distinguir coincidencias y resultados entre páginas. | Se busca por nombre y se recorren las páginas del resultado desde la interfaz. | Se registran los resultados observados y se comprueba que coinciden con la búsqueda, el orden y la paginación definidos por las fuentes; si los datos de prueba no permiten observar una condición, se registra como no comprobada. |
| CA03 | [S8 del plan](../../../plans/mvp0-plan.md), [AC-COL-04](../../../plans/mvp0-acceptance-criteria.md) y [«Rutas privadas»](../../../context/specifications.md) | Existe una colección propia que puede eliminarse conforme a sus criterios propietarios. | Se abre la confirmación y se confirma la eliminación mediante el flujo de interfaz. | Se verifica que la confirmación GET por sí sola no elimina la colección y que, tras confirmar, el resultado y el listado reflejan la operación definida. |
| CA04 | [S8 del plan](../../../plans/mvp0-plan.md), [AC-COL-04b](../../../plans/mvp0-acceptance-criteria.md), [«Collection»](../../../context/domain-model.md) y [«Errores controlados»](../../../context/specifications.md) | Hay una colección propia con enlaces y una colección que constituya la última del usuario, o se registra qué caso no puede prepararse en el entorno. | Se intenta eliminar cada colección desde su flujo canónico. | Para cada caso preparado se comprueba que la operación se rechaza según la fuente y que los datos permanecen; los casos no ejecutables se identifican expresamente, sin inferir que pasaron. |
| CA05 | [S8 del plan](../../../plans/mvp0-plan.md) y CA01–CA04 | Se han ejecutado los recorridos manuales que el entorno permite. | Se consolidan resultados, anomalías y límites de cobertura. | Quedan registrados fecha, navegador y entorno usados, datos previos relevantes, pasos, resultado de cada criterio y hallazgos reproducibles; la aceptación o solicitud de ajustes se registra como decisión humana explícita y no se atribuyen cambios productivos a esta revisión. |

Los límites y los casos negativos de cada flujo se comprueban según las referencias propietarias; esta especificación no redefine mensajes, validaciones ni contratos.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado para redactar esta especificación. Disponibilidad del entorno, datos adecuados para probar paginación y preparación de colecciones con enlaces o de la última colección se concretarán en el plan técnico, sin cambiar los criterios propietarios. | — | — | — |

## Artefactos relacionados

