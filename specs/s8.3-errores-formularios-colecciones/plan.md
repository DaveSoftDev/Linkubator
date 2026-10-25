# Plan técnico: S8.3 — Errores de formularios de colecciones

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 25-X-2026 («Siguiente»).
  - Plan: DLG, 25-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

- Ajustar `NewModel` y `EditModel` para conservar la separación entre errores de campo y errores generales que ya consumen sus vistas; los resultados proceden de `ICollectionService` y los tipos de `ApplicationError` existentes. No crear nuevas reglas de negocio ni modificar contratos de Application.
- Reutilizar los componentes compartidos `PageErrorSummaryViewModel`, `PageErrorSummaryItemViewModel`, `_ErrorSummary.cshtml` y `PageAlertViewModel` para mensajes de resumen y de reglas de dominio.
- Ajustar el marcado de los formularios Razor de nueva y edición para asociar cada error al control afectado, exponer el estado de error a tecnologías de asistencia y distinguirlo visualmente, conservando el modelo de entrada y el contenido presentado tras un rechazo.
- El formulario de eliminación no tiene campos editables: los rechazos de reglas de dominio se presentan como error general dentro de la página de confirmación, preservando el estado de la colección.
- S8.4 mantiene la responsabilidad de los identificadores inválidos y de los recursos válidos no accesibles. No redefinir ni duplicar ese tratamiento en estos cambios.

### Flujos internos

1. En creación y edición, cada error devuelto se traduce según su categoría y campo: los errores de validación con campo se asocian al control; errores generales y reglas de dominio no asociados a campo se presentan mediante el aviso general existente. Un error de dominio que el contrato vincule a un campo puede mostrarse asociado a él solo si esa asociación es explícita en el resultado.
2. Al re-renderizar el formulario, mantener los valores enviados y el resumen o aviso correspondiente. La información persistida solo cambia cuando el caso de uso confirma éxito; los rechazos no deben provocar escrituras parciales.
3. Tras representar el formulario con errores, establecer el foco en el primer control que tenga error, en el orden del formulario. La carga inicial sin errores conserva el comportamiento de foco ya definido.
4. En la eliminación, el rechazo por regla del dominio retorna la página de confirmación con su motivo; no se cambia el flujo GET/POST ni se presentan errores de identificador de S8.4 como parte de S8.3.

### Persistencia y dependencias

- Application e Infrastructure son responsables de validaciones, reglas de dominio y atomicidad de sus operaciones. Web consume los resultados sin acceder a repositorios ni implementar reglas.
- La dependencia operativa es `ICollectionService` y las páginas de colecciones existentes. Los tests se ejecutan en el test project actual; los criterios de esta spec no esperan a la suite transversal de S8.5.

### Garantías técnicas

- Los errores permanecen asociados al campo correcto y las etiquetas, descripciones y anuncios accesibles siguen vinculados al control afectado, según [requirements.md → «Accesibilidad»](../../context/requirements.md#accesibilidad).
- El foco se dirige al primer campo inválido únicamente tras un rechazo de validación, preservando el foco inicial de los formularios sin errores.
- Los mensajes reflejan el resultado existente y no exponen detalles privados no autorizados. Las reglas y mensajes funcionales siguen siendo propiedad de [specifications.md → «Errores controlados»](../../context/specifications.md#errores-controlados).
- El rechazo de una operación no cambia la colección ni sus enlaces; la garantía transaccional corresponde a Application/Infrastructure conforme a [architecture.md → «Persistencia»](../../context/architecture.md#persistencia).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Mantener los modelos compartidos de resumen y alerta y completar el mapeo de resultados en las páginas Razor de colecciones. | propuesta | Código actual de Web y [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación). | Reduce duplicación; verificar las categorías y el campo que realmente proporcionan los resultados antes de codificar el mapeo final. |
| Aplicar foco al primer campo inválido con una mejora local al formulario, sin establecer una dependencia global que afecte a otros formularios. | propuesta | [requirements.md → «Accesibilidad»](../../context/requirements.md#accesibilidad). | Evita cambios colaterales en páginas ajenas a S8.3; validar con prueba del HTML/cliente apropiada. |
| Mantener fuera de alcance la respuesta de rutas por identificador inválido o recurso no accesible. | confirmado | [S8.3 → «Alcance»](spec.md#alcance) y S8.4 en [mvp0-plan.md → «S8: Gestión de colecciones (L)»](../../plans/mvp0-plan.md#s8-gestión-de-colecciones-l). | Evita solaparse con S8.4; comprobar que sus resultados no se reclasifican como errores de campo en esta entrega. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA03, CA04 | Automatizada, integración Web con base persistente para POSTs de nueva, edición y eliminación; forzar errores de validación y reglas de dominio con fixtures propios, inspeccionar respuesta y estado persistido. | Host Web de tests, migraciones y fixtures de colecciones, dependencias locales restauradas. | Detectar el filtro disponible en los tests Web actuales y ejecutar la clase específica de validación de formularios de colecciones que se añada. | Los errores de validación se representan en su control o resumen apropiado; los errores de dominio aparecen como aviso; entradas editables se conservan y los rechazos dejan estado persistido sin cambios. | Comando/filtro real, resultados por caso, estado antes/después y limitaciones de cada test. |
| V02 | CA02 | Automatizada y técnica: comprobar en la respuesta de error el control asociado y el mecanismo que enfoca el primer error; inspección de la ruta de foco para los dos formularios y de foco inicial cuando no hay errores. | Cambios Web implementados; herramienta DOM/browser existente o mecanismo de test HTML disponible. | Añadir o reutilizar test focalizado de Web; ejecutar la comprobación automatizada adecuada y revisar el orden de controles en markup. | Un formulario fallido enfoca el primer control con error en orden lógico; formularios sin errores conservan el foco inicial. | Evidencia del test y de la inspección, distinguiendo el foco probado en HTML/DOM de cualquier lectura real de tecnología de asistencia. |
| V03 | CA01–CA04 | Build focalizado y diff documental/técnico. | Tareas de implementación y tests propios completados. | `dotnet build src\Linkubator.Web\Linkubator.Web.csproj --no-restore --nologo`; `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --no-build --filter "FullyQualifiedName~CollectionFormError" --nologo`; `git diff --check`. Ajustar el filtro al nombre real adoptado por los tests. | Build sin errores, tests focalizados verdes, diff sin errores de whitespace; ningún resultado esperado se registra como evidencia hasta su ejecución real. | Salida real, comando y filtro finales, estado de test/build y revisión del diff. |

V01 prueba los resultados que cubran sus casos, no todas las combinaciones de validación. V02 no sustituye una revisión manual con lector de pantalla; esa revisión final corresponde al trabajo vecino y no bloquea el cierre de esta spec. Antes de fijar la validación, identificar el mecanismo de test Web y DOM ya disponible; no instalar herramientas salvo que una validación autorizada falle por herramienta ausente.

## Orden de ejecución y cierre

- Crear `tasks.md` solo después de que este plan reciba aprobación humana explícita.
- Todos los criterios deben tener validación y evidencia satisfactoria dentro de las tareas de S8.3; ninguna tarea posterior aporta un prerrequisito de cierre.
- Si el contrato real no distingue un resultado requerido sin ambigüedad, registrar el bloqueo y esperar la decisión necesaria, sin cambiar la regla fuente ni su contrato por iniciativa propia.
