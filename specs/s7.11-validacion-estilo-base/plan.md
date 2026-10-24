# Plan técnico: S7.11 — Validación del estilo base

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 24-X-2026 («Siguiente»).
  - Plan: DLG, aprobado el 24-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

- Reutilizar el layout privado compartido y los componentes de presentación ya existentes; no crear componentes ni modificar estilos en esta revisión. El alcance visual se limita a las partes disponibles identificadas en [spec.md](spec.md), con base en [S7.4 → CA01–CA06](../s7.4-layout-privado/spec.md#criterios-de-aceptación) y [S7.5 → CA01–CA06](../s7.5-componentes-presentacion/spec.md#criterios-de-aceptación).
- Preparar una relación de páginas, componentes y estados comunes que realmente puedan inspeccionarse en el entorno disponible. No se utilizará una página de demostración como prueba de integración funcional de producto; si resulta necesaria para visualizar un componente aislado, se identificará expresamente como tal y no sustituirá la inspección de sus consumidores existentes.
- Registrar la evidencia por criterio: entorno y navegador, página/estado y elemento revisado, pasos o método, observaciones y límites de cobertura. Cada hallazgo contendrá contexto y ubicación suficientes para que pueda revisarse; cada ajuste solicitado se registrará como seguimiento, sin realizarlo en S7.11.
- Considerar las expectativas de accesibilidad aplicables según [requirements.md → «Accesibilidad»](../../context/requirements.md#accesibilidad). Esta inspección visual no se presentará como auditoría completa ni reemplazará la revisión específica del dashboard de [S7.10](../s7.10-revision-manual-dashboard/spec.md).

### Flujos internos

1. Identificar el host Web y el entorno disponible; seleccionar las páginas y estados existentes representativos del layout y los componentes compartidos.
2. Registrar el contexto de la inspección y recorrer la base visual compartida, anotando para cada elemento el resultado y cualquier limitación.
3. Consolidar los hallazgos y determinar si se solicitan ajustes. Si un elemento requerido no está disponible, registrarlo como límite de cobertura, sin simular que se revisó.
4. Presentar la evaluación al humano y registrar explícitamente aceptación o solicitud de ajustes, responsable, fecha y referencia.
5. Mantener cualquier cambio productivo solicitado fuera de esta spec; no editar código, estilos, reglas fuente ni tareas de otras etapas.

### Persistencia y dependencias

No se modifican persistencia, dependencias, infraestructura ni configuración. Se requiere una instancia Web ejecutable con las vistas existentes de la zona privada. S7.4 y S7.5 aportan el layout y los componentes a revisar; S7.10 aporta contexto de la inspección del dashboard, sin sustituir la evaluación de esta spec.

### Garantías técnicas

- La evidencia representa solo las páginas, componentes y estados realmente inspeccionados; la falta de una pantalla se declara como limitación, no como verificación satisfactoria.
- Los hallazgos se vinculan a elementos observados y a las fuentes enlazadas en [spec.md](spec.md), sin convertir la inspección en una nueva regla de producto.
- Los ajustes se registran como solicitudes y no se consideran implementados o corregidos por el hecho de haber sido aceptados como seguimiento.
- La decisión humana queda diferenciada de los resultados de inspección; no se declara aceptado el estilo base sin una aceptación explícita.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Reutilizar el host y las vistas Web existentes; no crear una UI productiva de inspección | Propuesta | [spec.md](spec.md), [architecture.md → «Web»](../../context/architecture.md#web) | Mantiene la revisión sobre el producto disponible; confirmar páginas y estados concretos antes de ejecutar las tareas. |
| Selección de páginas/estados representativos y método de registro de observaciones | Pendiente | [spec.md](spec.md), CA01–CA02 | Concretar al preparar tareas de acuerdo con lo que esté disponible; declarar explícitamente cualquier cobertura ausente. |
| No implementar ajustes solicitados dentro de esta revisión | Confirmado | [spec.md](spec.md), alcance | Los cambios requieren trabajo separado y autorizado; esta revisión puede cerrarse con solicitudes registradas y una decisión explícita. |
| La revisión global no sustituye ni reabre S7.10 | Confirmado | [spec.md](spec.md), [S7.10 → alcance](../s7.10-revision-manual-dashboard/spec.md#alcance) | Evita duplicar el objetivo de revisión específica del dashboard y mantiene la evidencia independiente. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Manual, inventario de cobertura | Host Web disponible; layout y componentes comunes accesibles en páginas existentes | Abrir las páginas disponibles que consumen el layout y los componentes comunes; anotar páginas, componentes y estados observados, además de los previstos pero no disponibles. | El inventario distingue claramente elementos inspeccionados de limitaciones; no se afirma cobertura no realizada. | Entorno/navegador, rutas, componentes/estados, fecha y lista de límites. |
| V02 | CA02 | Manual, inspección visual de presentación accesible | V01 identificado y vistas/estados seleccionados visibles | Inspeccionar la presentación de los elementos comunes incluidos frente a las expectativas enlazadas; registrar todo hallazgo con elemento, estado y contexto reproducible, o documentar que no se observaron hallazgos. | Resultado trazable a los elementos observados, sin afirmar una auditoría completa ni una validación instrumental no ejecutada. | Pasos y método usados, elemento/estado, observación, resultado, fecha y límites del método. |
| V03 | CA03 | Revisión y decisión humana | V01–V02 completadas | Consolidar la evaluación y pedir una decisión explícita sobre si se solicitan ajustes; describir de forma separada cada ajuste, si los hay. | La decisión está registrada y los ajustes solicitados son identificables y no se presentan como implementados. | Decisión, responsable/fecha/referencia y lista de ajustes o constancia de ninguno. |
| V04 | CA04 | Aceptación humana y revisión técnica de evidencia | V01–V03 documentadas | Comprobar que el registro distingue hallazgos, cobertura, limitaciones, solicitudes y decisión; solicitar aceptación explícita del estilo base o dejar constancia de que sigue pendiente. | Solo una aceptación humana explícita permite consignar el estilo base como aceptado; una solicitud de ajustes se registra sin declarar aceptación implícita. | Registro de decisión y evidencia consolidada por criterio, fecha y referencia. |

Las validaciones son manuales y documentales, no pruebas automatizadas. No demuestran accesibilidad completa con tecnologías de asistencia ni el comportamiento de páginas o estados no disponibles. Como comprobación negativa, un componente/estado requerido que no pueda abrirse debe aparecer como límite y no como resultado satisfactorio; una solicitud de ajustes no equivale a aceptación ni a corrección implementada.

## Orden de ejecución y cierre

- Tras aprobar este plan, crear `tasks.md` desde su plantilla y someterlo a aprobación antes de iniciar la revisión.
- Preparar el inventario de cobertura antes de la inspección; completar la revisión y su consolidación antes de registrar la decisión humana.
- Cerrar cuando CA01–CA04 tengan evidencia propia acorde con el resultado real, se documenten los límites y solicitudes, las tareas propias estén completadas y exista aceptación humana explícita. El cierre no depende de implementar los ajustes solicitados.
