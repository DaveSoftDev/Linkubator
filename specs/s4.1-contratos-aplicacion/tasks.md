# Tareas: S4.1 — Result, errores, DTOs y validadores de aplicación

## Referencias y autorización

- Estado: aprobada.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 15-X-2026 («Aprobado»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 15-X-2026 («Comprendido»).
    - Tareas: aprobada por DLG el 15-X-2026 («Vamos»).
- Implementación:
    - Estado: iniciada.
    - Inicio: 15-X-2026 («Vamos»).
    - Finalización: pendiente; registrar fecha, comprobación y referencia real al completar las tareas de implementación y validación.
    - Evidencia consolidada: pendiente; enlazar las tareas y validaciones que sustentan el estado.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T02 inventaría los contratos concretos cubiertos antes de implementarlos. T03–T05 separan el resultado/errores, los DTOs y los validadores para limitar el alcance de cada cambio; T06 integra las pruebas y garantías. No se inicia ninguna tarea productiva antes de la aprobación explícita de estas tareas.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas; registrar la referencia explícita de DLG. | CA01–CA08; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Confirmar que todos los criterios tienen tareas y validaciones, y que el alcance excluye trabajo S4.2–S4.10 no autorizado aquí; resultado esperado: aprobación de tareas registrada separada de spec y plan. | pendiente | Pendiente de aprobación humana. |
| T02 | Elaborar la matriz de operaciones MVP0 incluidas en S4.1 y asignar a cada una sus DTOs de entrada/salida y validadores, enlazando su fuente funcional; incorporarla a esta sección o como tabla documental en `tasks.md`. | CA04–CA06; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V03–V04 | T01 | Contrastar todas las operaciones de S4 del plan MVP0 con el catálogo; resultado esperado: ninguna operación aplicable omitida y ninguna capacidad posterior añadida. | pendiente | No ejecutada. |
| T03 | Implementar `Result`, `Result<T>` y los errores tipados, con invariantes de estados y categorías aprobadas. | CA02–CA03, CA08; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V02, V06 | T02 | Ejecutar los tests unitarios de contratos; resultado esperado: éxito/error distinguibles, errores consultables por código/categoría/campo y sin datos sensibles. | pendiente | No ejecutada. |
| T04 | Añadir los DTOs de entrada y salida de la matriz T02, solo con valores de transporte requeridos. | CA01, CA04–CA05, CA08; [plan → Componentes y contratos](plan.md#componentes-y-contratos), V01, V03 | T02 | Compilar Application y revisar las firmas y propiedades de todos los DTOs; resultado esperado: cubren las operaciones inventariadas sin entidades de persistencia, contratos Web ni identidad de usuario proporcionada por el cliente. | pendiente | No ejecutada. |
| T05 | Implementar validadores para los DTOs de entrada de T04 y traducir a errores de aplicación los rechazos conocidos de las políticas de Domain que correspondan. | CA03, CA06–CA08; [plan → Flujos internos](plan.md#flujos-internos), V04–V06 | T03, T04 | Ejecutar tests con entradas válidas e inválidas y traducción de rechazos Domain; resultado esperado: errores asociados a campos y reglas propietarias reutilizadas, sin mensajes de presentación ni valores sensibles. | pendiente | No ejecutada. |
| T06 | Ejecutar validaciones integradas de contratos, dependencias, build y suite; corregir solo incidencias pertenecientes al alcance S4.1. | CA01–CA08; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V01–V07 | T03–T05 | Ejecutar la prueba de referencias, los tests específicos y los comandos integrados V07; resultado esperado: todos pasan y se registra la evidencia real, SDK y warnings. | pendiente | No ejecutada. |
| T07 | Revisar trazabilidad de criterio a evidencia, sincronizar el roadmap si corresponde y solicitar aceptación humana del resultado. | CA01–CA08; [plan → Garantías técnicas](plan.md#garantías-técnicas) y [«Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre) | T06 | Comprobar que cada criterio tiene validación satisfactoria y evidencia fechada, y que no se presenta esta aceptación como aprobación de S4.5; resultado esperado: decisión explícita de DLG registrada o bloqueo documentado. | pendiente | Sin aceptación. |

## Matriz de operaciones MVP0 → DTOs

Pendiente de completar en T02, después de la aprobación de las tareas. Cada fila deberá enlazar a los criterios/reglas propietarios y señalar DTOs de entrada y salida o justificar que la operación no los necesita.

| Área / operación S4 | Fuente funcional | DTO de entrada | DTO(s) de salida | Validador | Estado |
| --- | --- | --- | --- | --- | --- |
| Pendiente de inventario | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) | Pendiente | Pendiente | Pendiente | No iniciado |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T04, T06 | V01, V07 | Pendiente | No ejecutada | Sin evidencia |
| CA02 | T03, T06 | V02, V07 | Pendiente | No ejecutada | Sin evidencia |
| CA03 | T03, T05, T06 | V02, V04, V05, V07 | Pendiente | No ejecutada | Sin evidencia |
| CA04 | T02, T04, T06 | V03, V07 | Pendiente | No ejecutada | Sin evidencia |
| CA05 | T02, T04, T06 | V03, V07 | Pendiente | No ejecutada | Sin evidencia |
| CA06 | T02, T05, T06 | V04, V07 | Pendiente | No ejecutada | Sin evidencia |
| CA07 | T05, T06 | V05, V07 | Pendiente | No ejecutada | Sin evidencia |
| CA08 | T02–T06 | V02, V03, V06, V07 | Pendiente | No ejecutada | Sin evidencia |

Un resultado no disponible o fallido sigue pendiente o bloqueado; no registrar resultados esperados como reales.

## Aceptación y cierre

- Estado del trabajo: en curso.
- Criterios sin evidencia satisfactoria: CA01–CA08.
- Bloqueos y riesgos restantes: completar la implementación y la validación del contrato S4.1; el inventario de DTOs se completará en T02 y queda no bloqueante para la capa base.
- Aceptación humana: aprobada por DLG con la referencia «Vamos» el 15-X-2026; la aceptación corresponde al inicio de la implementación documentada por esta tarea.
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes.
- Siguiente paso autorizado: iniciar la implementación de S4.1 bajo esta aprobación; la puerta S4.5 sigue siendo obligatoria antes de implementar casos de uso S4.6–S4.10.
