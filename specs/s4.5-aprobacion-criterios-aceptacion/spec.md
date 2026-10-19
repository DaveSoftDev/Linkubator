# Especificación: S4.5 — Revisión y aprobación de los criterios de aceptación

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 15-X-2026 («Adelante»).
- Actualización: ajuste de los criterios de capa y de los resultados CA04/CA05 por instrucción de DLG, 16-X-2026; no constituye una nueva aprobación de la especificación ni del catálogo.

## Objetivo

Obtener y registrar la decisión explícita de DLG sobre el catálogo de criterios de aceptación de S4, verificando que su cobertura y trazabilidad permiten usarlo como puerta para la implementación posterior.

## Fuentes

- [mvp0-plan.md → «Puerta de entrada de la implementación (criterios de aceptación de S4)»](../../plans/mvp0-plan.md#puerta-de-entrada-de-la-implementación-criterios-de-aceptación-de-s4) y [«S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l): propósito de la aprobación, entregable y límite de inicio de los sprints posteriores.
- [S4.4 — Criterios de aceptación de los casos de uso](../s4.4-criterios-aceptacion-casos-uso/spec.md) y [sus tareas → «Aceptación y cierre»](../s4.4-criterios-aceptacion-casos-uso/tasks.md#aceptación-y-cierre): preparación aceptada y alcance trazado del catálogo.
- [Catálogo de criterios de aceptación del MVP0](../../plans/mvp0-acceptance-criteria.md): artefacto sometido a revisión y aprobación.
- [roadmap.md → «Now»](../../context/roadmap.md#now): estado actual de S4 y estado de la puerta de criterios.
- [AGENTS.md → «Principio de fuente única»](../../AGENTS.md#dónde-vive-cada-cosa): los criterios remiten a reglas propietarias y no las sustituyen.

## Alcance

- Incluye:
  - Revisar el catálogo preparado en S4.4 frente al alcance de S4 y comprobar su cobertura de operaciones, lecturas, casos negativos aplicables y trazabilidad a fuentes propietarias.
  - Registrar la decisión explícita de DLG sobre el catálogo, con fecha y referencia real, manteniendo diferenciadas la aprobación de esta especificación y la decisión S4.5.
  - Si el catálogo requiere cambios, identificar los criterios afectados y devolverlos a preparación y revisión antes de registrar su aprobación.
  - Mantener bloqueado el inicio de S4.6–S4.10 hasta que la aprobación del catálogo esté registrada.
- Excluye:
  - Cambiar reglas de producto o resolver contradicciones en este artefacto; cualquier cambio necesario debe tramitarse en su documento propietario.
  - Implementar casos de uso, repositorios concretos, fakes, pruebas de consumidor, páginas, persistencia o registro de eventos.
  - Aprobar esta especificación, su plan o sus tareas en nombre de DLG.
- Trabajo vecino y responsable de sus entregables:
  - S4.4 preparó y validó documentalmente el catálogo.
  - S4.5 corresponde a la revisión y decisión humana de DLG sobre el catálogo.
  - S4.6–S4.10 implementan los casos de uso y sus pruebas únicamente después de la aprobación S4.5.

## Dependencias y prerrequisitos

- S4.4 está cerrada para su alcance y aceptada por DLG; el catálogo existe y está entregado para revisión, según [roadmap.md → «Now»](../../context/roadmap.md#now) y [S4.4 → «Aceptación y cierre»](../s4.4-criterios-aceptacion-casos-uso/tasks.md#aceptación-y-cierre).
- La preparación documental de S4.4 registra validación de enlaces, anclas, identificadores y trazabilidad; no acredita implementación ni comportamiento de los casos de uso.
- La aprobación del catálogo queda registrada en S4.5 con una decisión explícita de DLG. Esa aprobación no inicia por sí sola S4.6–S4.10, que requieren sus propios artefactos SDD aprobados.
- La documentación de reglas propietarias vigente prevalece ante cualquier discrepancia detectada durante la revisión; el catálogo no puede resolverla por sí solo.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) y [S4.4 → «Criterios de aceptación»](../s4.4-criterios-aceptacion-casos-uso/spec.md#criterios-de-aceptación) | El alcance de casos de uso MVP0 enumerado para S4 y el catálogo preparado. | Se contrasta el catálogo con cada operación y lectura del alcance. | Cada caso de uso tiene criterios localizables y no se incorporan capacidades ajenas al alcance; las omisiones quedan identificadas antes de la decisión de aprobación. |
| CA02 | [mvp0-plan.md → «Puerta de entrada de la implementación (criterios de aceptación de S4)»](../../plans/mvp0-plan.md#puerta-de-entrada-de-la-implementación-criterios-de-aceptación-de-s4) y [AGENTS.md → «Principio de fuente única»](../../AGENTS.md#dónde-vive-cada-cosa) | Los criterios del catálogo y las fuentes funcionales y de dominio que estos enlazan. | DLG revisa la trazabilidad y el carácter verificable de los criterios. | Cada criterio remite a una fuente propietaria pertinente y puede comprobarse sin convertirse en una regla paralela; las dudas o contradicciones quedan señaladas para resolución en su fuente. |
| CA03 | [S4.4 → «Criterios de aceptación»](../s4.4-criterios-aceptacion-casos-uso/spec.md#criterios-de-aceptación) y [Catálogo → «Criterios de aceptación»](../../plans/mvp0-acceptance-criteria.md#criterios-de-aceptación) | Los criterios del catálogo que cubren rechazos, propiedad, identidad, privacidad, lecturas públicas y límites técnicos aplicables. | Se revisa si los resultados descritos están respaldados por las fuentes y tienen comprobación observable. | Los casos negativos y límites aplicables están representados sin inventar reglas, revelar datos privados ni atribuir responsabilidades de implementación a S4.5. |
| CA04 | [mvp0-plan.md → «Puerta de entrada de la implementación (criterios de aceptación de S4)»](../../plans/mvp0-plan.md#puerta-de-entrada-de-la-implementación-criterios-de-aceptación-de-s4) y [roadmap.md → «Now»](../../context/roadmap.md#now) | El catálogo revisado y DLG comunica una decisión. | Se registra el resultado recibido con fecha, responsable y referencia real. | La decisión queda registrada sin reinterpretarla: una aprobación explícita abre la puerta de criterios; una solicitud de cambios se registra como no aprobatoria y mantiene la puerta cerrada. La aprobación del catálogo no declara que S4.6–S4.10 hayan comenzado ni sustituye sus artefactos SDD aprobados. |
| CA05 | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) y [S4.4 → «Aceptación y cierre»](../s4.4-criterios-aceptacion-casos-uso/tasks.md#aceptación-y-cierre) | La revisión encuentra un hallazgo bloqueante o DLG solicita correcciones. | Se determina y registra qué criterio y fuente requieren acción. | El catálogo no se declara aprobado y la puerta permanece cerrada hasta corregir el criterio o resolver la fuente propietaria y obtener una decisión explícita posterior de DLG. |

La revisión se limita a la cobertura, coherencia y decisión del catálogo preparado. Las reglas exactas y los casos negativos se consultan en sus fuentes propietarias; esta especificación no los vuelve a definir. Para las eliminaciones, la validación del formato del identificador y el acceso a recursos válidos se rigen por sus fuentes propietarias; Web consume categoría y código para presentar el mensaje y tratamiento HTTP correspondiente. S4.5 comprueba la coherencia documental de esta frontera, no implementa ni prueba Web.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| No hay bloqueos documentales pendientes para esta especificación tras la aprobación del catálogo. | — | — | Mantener la trazabilidad; la aprobación del catálogo no sustituye la aprobación de los artefactos SDD de S4.6–S4.10. |

## Artefactos relacionados

- [Plan técnico](plan.md): aprobado por DLG el 15-X-2026 («Vamos»).
- [Catálogo de criterios de aceptación del MVP0](../../plans/mvp0-acceptance-criteria.md): preparado en S4.4 y aprobado por DLG en S4.5 el 16-X-2026 («Leído y comprendido»).