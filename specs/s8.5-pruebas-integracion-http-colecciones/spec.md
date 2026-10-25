# Especificación: S8.5 — Pruebas de integración HTTP de colecciones

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 25-X-2026 («Siguiente»).

## Objetivo

Contar con pruebas de integración HTTP que comprueben conjuntamente los contratos Web de colecciones ya definidos y la propiedad de los datos, sin introducir comportamiento productivo ni sustituir las pruebas unitarias y de Application.

## Fuentes

- [mvp0-plan.md → «S8: Gestión de colecciones (L)»](../../plans/mvp0-plan.md#s8-gestión-de-colecciones-l): alcance de S8.5 y Definition of Done de S8.
- [Criterios de aceptación de S4 → «AC-COL-01 — Crear colección»](../../plans/mvp0-acceptance-criteria.md#ac-col-01-crear-colección), [«AC-COL-01b — Hacer pública una colección vacía»](../../plans/mvp0-acceptance-criteria.md#ac-col-01b-hacer-pública-una-colección-vacía), [«AC-COL-02 — Editar colección»](../../plans/mvp0-acceptance-criteria.md#ac-col-02-editar-colección), [«AC-COL-04 — Eliminar colección»](../../plans/mvp0-acceptance-criteria.md#ac-col-04-eliminar-colección), [«AC-COL-04b — Eliminar una colección con enlaces o la última colección»](../../plans/mvp0-acceptance-criteria.md#ac-col-04b-eliminar-una-colección-con-enlaces-o-la-última-colección) y [«AC-COL-05 — Listar colecciones del usuario»](../../plans/mvp0-acceptance-criteria.md#ac-col-05-listar-colecciones-del-usuario): contratos funcionales aprobados de colección.
- [requirements.md → «Colecciones»](../../context/requirements.md#colecciones), [«Búsqueda y filtros»](../../context/requirements.md#búsqueda-y-filtros) y [«Accesibilidad»](../../context/requirements.md#accesibilidad): expectativas del listado y de los formularios.
- [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas), [«Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros), [«Listados»](../../context/specifications.md#listados) y [«Errores controlados»](../../context/specifications.md#errores-controlados): rutas, métodos, parámetros y respuestas.
- [domain-model.md → «Collection»](../../context/domain-model.md#collection), [«Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [«Público y privado»](../../context/domain-model.md#público-y-privado): invariantes de propiedad y visibilidad.
- [architecture.md → «Seguridad web»](../../context/architecture.md#seguridad-web), [«Persistencia»](../../context/architecture.md#persistencia) y [«Web»](../../context/architecture.md#web): protección de peticiones y fronteras técnicas.
- [S8.1 — Listado de colecciones](../s8.1-listado-colecciones/spec.md), [S8.2 — Operaciones de colecciones](../s8.2-operaciones-colecciones/spec.md), [S8.3 — Errores de formularios de colecciones](../s8.3-errores-formularios-colecciones/spec.md) y [S8.4 — Identificadores y respuestas de colecciones](../s8.4-identificadores-y-respuestas-colecciones/spec.md): límites de los trabajos funcionales y criterios que esta suite integra sin reabrir.

## Alcance

- **Incluye:** pruebas automatizadas que atraviesan el host HTTP de Web para los flujos de listado y búsqueda, creación, edición y eliminación de colecciones; verificación de respuestas, formularios, antiforgery, aislamiento por usuario y ausencia de cambios no autorizados; identificación de brechas entre los tests de integración existentes y los criterios aprobados.
- **Excluye:** cambios productivos, cambios de reglas o contratos funcionales, pruebas unitarias o de Application, pruebas de UI automatizadas, revisión manual de accesibilidad, despliegue y la suite transversal de seguridad Web perteneciente a S7.9.
- **Trabajo vecino y responsable de sus entregables:** S8.1–S8.4 son propietarios de sus respectivos comportamientos de listado, operación, presentación de errores y tratamiento de identificadores. S8.6 y S8.7 son responsables de las revisiones manuales indicadas en el plan de S8. Esta spec verifica su integración observable por HTTP; no sustituye sus pruebas propias ni necesita su cierre formal para cerrarse.

## Dependencias y prerrequisitos

- El host Web de pruebas, `WebApplicationFactory` y la infraestructura de base de datos aislada están disponibles; la prueba existente [CollectionsListingIntegrationTests.cs](../../tests/Linkubator.Tests/Integration/CollectionsListingIntegrationTests.cs) sirve como evidencia de harness y cobertura previa, no como demostración de que todos los criterios de esta spec estén completos.
- Las rutas y operaciones de colección están implementadas en el estado de trabajo actual. El cierre técnico de S8.2 y S8.4 sigue pendiente según el roadmap; no se presupone su aceptación técnica. Si un contrato no coincide con las fuentes aprobadas, se bloquea la comprobación afectada y se escala al documento propietario.
- S8.1, S8.2, S8.3 y S8.4 aportan criterios aprobados para los casos cubiertos. La suite S8.5 debe poder cerrarse con sus propios tests y evidencia, sin esperar a S8.6, S8.7 ni al cierre global de S8.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [AC-COL-05](../../plans/mvp0-acceptance-criteria.md#ac-col-05-listar-colecciones-del-usuario), [«Rutas privadas»](../../context/specifications.md#rutas-privadas), [«Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros) y [«Listados»](../../context/specifications.md#listados) | Una petición HTTP accede al listado con datos propios y ajenos, búsqueda o paginación. | Se procesa y renderiza la respuesta del listado, incluyendo estado vacío y valores de parámetros aplicables y no aplicables. | La respuesta observable respeta AC-COL-05 y las reglas fuente; no expone colecciones de otro usuario ni acepta datos de identidad proporcionados por el cliente. |
| CA02 | [AC-COL-01](../../plans/mvp0-acceptance-criteria.md#ac-col-01-crear-colección), [AC-COL-01b](../../plans/mvp0-acceptance-criteria.md#ac-col-01b-hacer-pública-una-colección-vacía), [AC-COL-02](../../plans/mvp0-acceptance-criteria.md#ac-col-02-editar-colección) y [«Rutas privadas»](../../context/specifications.md#rutas-privadas) | Un usuario identificado envía por HTTP datos válidos de creación o edición, y se dispone de casos de control inválidos cuando los criterios propietarios los definen. | Se envían los formularios por sus métodos canónicos con la protección requerida. | Las operaciones válidas dejan el estado persistido acorde a sus criterios propietarios; los rechazos no dejan cambios parciales y las respuestas conservan el tratamiento previsto por las fuentes. |
| CA03 | [AC-COL-04](../../plans/mvp0-acceptance-criteria.md#ac-col-04-eliminar-colección), [AC-COL-04b](../../plans/mvp0-acceptance-criteria.md#ac-col-04b-eliminar-una-colección-con-enlaces-o-la-última-colección) y [«Rutas privadas»](../../context/specifications.md#rutas-privadas) | Una colección propia se puede eliminar o la operación está restringida por sus condiciones propietarias. | Se solicita la confirmación por GET y se envía el POST correspondiente. | GET no ejecuta la eliminación; POST produce el resultado HTTP previsto y el estado persistido coincide con el resultado aceptado o rechazado por la operación. |
| CA04 | [«Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos), [AC-COL-02](../../plans/mvp0-acceptance-criteria.md#ac-col-02-editar-colección), [AC-COL-04](../../plans/mvp0-acceptance-criteria.md#ac-col-04-eliminar-colección) y [«Rutas privadas»](../../context/specifications.md#rutas-privadas) | Las rutas de colección reciben un identificador inválido o válido inexistente/ajeno, o una petición intenta operar sobre datos de otra persona. | Se comparan las respuestas HTTP y el estado persistido antes y después. | No se exponen ni modifican datos ajenos; las respuestas para recursos no accesibles no revelan si faltan o pertenecen a otra persona, y los identificadores inválidos conservan su tratamiento definido. |
| CA05 | [«Seguridad web»](../../context/architecture.md#seguridad-web), [«Rutas privadas»](../../context/specifications.md#rutas-privadas) y [AC-COL-01](../../plans/mvp0-acceptance-criteria.md#ac-col-01-crear-colección), [AC-COL-02](../../plans/mvp0-acceptance-criteria.md#ac-col-02-editar-colección), [AC-COL-04](../../plans/mvp0-acceptance-criteria.md#ac-col-04-eliminar-colección) | Un formulario HTTP de modificación se envía con y sin token antiforgery válido. | El host procesa la petición. | La petición sin protección válida es rechazada antes de aplicar cambios, y la petición de control con protección válida continúa según el contrato del formulario. |
| CA06 | Criterios CA01–CA05 y fuentes enlazadas para cada uno | Las pruebas HTTP creadas o reutilizadas afirman los comportamientos del alcance. | Se ejecuta la suite focalizada y se realiza una perturbación negativa reversible sobre una aserción representativa de propiedad/no divulgación o antiforgery. | La suite pasa con la expectativa aprobada; la perturbación demuestra que la aserción detecta la regresión y, tras restaurarla, la suite vuelve a pasar. No se atribuyen a esta suite garantías que no haya ejercitado. |

Los límites y casos negativos se validan mediante las fuentes indicadas; esta spec no redefine valores, mensajes, códigos ni reglas de dominio.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado durante la preparación. El inventario de tests HTTP existente y su correspondencia exacta con cada criterio se confirmará en el plan, una vez aprobada esta spec. | — | — | — |

## Artefactos relacionados

