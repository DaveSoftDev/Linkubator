# Plan técnico: S12.6 — Pruebas manuales en navegadores actuales

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 31-X-2026 con la referencia «Siguiente».
  - Plan: aprobado por DLG el 31-X-2026 con la referencia «Siguiente».

## Diseño de implementación

### Componentes y contratos

- La persona responsable de la revisión ejecuta los escenarios directamente en navegadores, sin automatización de UI.
- La aplicación web en el entorno de revisión es el sistema bajo prueba. Los comportamientos esperados se obtienen de las fuentes funcionales enlazadas en [spec.md](spec.md), no de observaciones anteriores ni de supuestos sobre la interfaz.
- El registro de resultados se mantiene como evidencia de las tareas de la revisión. Cada entrada relaciona navegador y versión, área, escenario, resultado observado y cualquier hallazgo o limitación.
- S12.6 no añade componentes, contratos ni cambios al producto.

### Flujos internos

1. Confirmar que el entorno de revisión es accesible y que se dispone de datos de prueba apropiados para recorrer las páginas y operaciones seleccionadas.
2. Identificar los navegadores actuales disponibles para la ejecución y anotar nombre y versión. Esta selección describe la ejecución; no establece una matriz permanente de compatibilidad.
3. Preparar una lista de escenarios a partir de las superficies aplicables descritas en [spec.md](spec.md): páginas públicas y privadas, formularios, listados, mensajes de error y estados vacíos.
4. Recorrer manualmente los escenarios en los navegadores seleccionados y contrastar el resultado con las fuentes funcionales.
5. Registrar cada resultado, incluidos los comportamientos inesperados y los escenarios no verificables. No corregir hallazgos dentro de esta revisión; cualquier cambio requiere un trabajo autorizado separado.
6. Revisar la evidencia para confirmar que los tres criterios tienen resultados identificables antes de solicitar la aceptación humana.

### Persistencia y dependencias

- No se requieren cambios de esquema, almacenamiento adicional ni servicios externos.
- La ejecución necesita una instancia accesible de la aplicación y datos que permitan revisar páginas públicas y privadas, formularios, listados, errores y estados vacíos cuando existan.
- Se propone utilizar un entorno local o de pruebas y datos aislados de la revisión. Si un escenario requiere modificar datos, deben ser datos preparados para ese fin y dejarse en un estado conocido al terminar.
- S12.1 está completada y aporta la entrada A12 como referencia de cobertura. S12.5 es independiente y no condiciona la ejecución ni el cierre de S12.6.

### Garantías técnicas

- La comprobación de UI es manual y se realiza en navegadores reales; no se emplean pruebas automatizadas de navegador.
- La revisión contrasta los comportamientos con sus fuentes y no amplía reglas funcionales, compatibilidad comprometida ni alcance de producto.
- Los resultados sin evidencia, los escenarios omitidos y las limitaciones se distinguen de comprobaciones satisfactorias.
- La revisión de accesibilidad permanece en S12.5; S12.6 no sustituye ni declara completada esa evaluación.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Seleccionar para la ejecución los navegadores actuales disponibles y registrar nombre y versión, sin crear una matriz permanente de soporte | propuesta | [spec.md → «Dependencias y prerrequisitos»](spec.md#dependencias-y-prerrequisitos) | El conjunto concreto depende del entorno de ejecución; debe quedar identificado antes de probar y ser aprobado como parte de este plan. |
| Usar un entorno local o de pruebas con datos aislados | propuesta | [spec.md → «Alcance»](spec.md#alcance) | Reduce el riesgo de modificar datos ajenos a la revisión. Confirmar el entorno y los datos antes de ejecutar; si no pueden prepararse, documentar el bloqueo. |
| No corregir hallazgos en S12.6 | confirmado | [spec.md → «Alcance»](spec.md#alcance) | Mantiene la revisión dentro de su objetivo; las correcciones requieren autorización y alcance separados. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Manual: recorrer las superficies públicas y privadas en cada navegador seleccionado y contrastar el resultado con las fuentes; comprobar que el recorrido público no expone contenido privado. | Aplicación accesible, escenarios aplicables preparados y navegadores identificados con sus versiones. | Revisión manual en cada navegador seleccionado; sin comando ni automatización de UI. | Ambas áreas quedan comprobadas o cada escenario no verificable queda explícitamente limitado; no se declara satisfactorio un caso sin observación. | Navegador y versión, área y páginas recorridas, datos de prueba no sensibles, observaciones y hallazgos. |
| V02 | CA02 | Manual: recorrer escenarios representativos de formularios, listados, mensajes de error y estados vacíos que apliquen a las áreas revisadas. | V01 preparado; operaciones y datos adecuados disponibles para cada categoría aplicable. | Revisión manual guiada por los escenarios y las fuentes funcionales; sin comando ni automatización de UI. | Cada categoría aplicable tiene resultado contrastado con su fuente; las categorías no disponibles o no comprobadas se identifican. | Escenario y fuente, navegador y versión, resultado observado y discrepancias o límites. |
| V03 | CA03 | Revisión técnica de la evidencia de V01 y V02; no vuelve a ejecutar ni sustituye los recorridos manuales. | Resultados de V01 y V02 registrados. | Comprobar la trazabilidad de cada resultado y que hallazgos y límites no se presentan como satisfactorios. | Cada criterio tiene evidencia localizable con navegador y versión, área y escenario, observación y estado; los hallazgos y límites están registrados. | Referencias a las entradas de evidencia y lista de discrepancias o limitaciones detectadas en la revisión. |

La revisión manual confirma el comportamiento observado solo en los navegadores y escenarios registrados; no demuestra compatibilidad con navegadores no probados ni reemplaza la revisión de accesibilidad.

## Orden de ejecución y cierre

- Tras aprobar este plan, crear `tasks.md` con tareas independientes solo cuando sus prerrequisitos lo permitan; incluir preparación del entorno, ejecución de los escenarios y consolidación de evidencia.
- La ejecución comienza únicamente tras la aprobación de `tasks.md`.
- S12.6 puede cerrarse cuando CA01–CA03 tienen evidencia verificable, las limitaciones y hallazgos están registrados, las tareas propias están completadas y consta la aceptación humana. El cierre no requiere corregir los hallazgos dentro de esta revisión ni esperar a S12.5.
