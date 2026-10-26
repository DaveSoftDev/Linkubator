# Especificación: S8.4 — Identificadores y respuestas de colecciones

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 25-X-2026 («Siguiente»).

## Objetivo

Asegurar que las páginas de edición y eliminación de colecciones procesan identificadores inválidos y recursos válidos no accesibles en la frontera Web según las respuestas ya definidas, sin consultar ni modificar recursos ajenos.

## Fuentes

- [mvp0-plan.md → «S8: Gestión de colecciones (L)»](../../../plans/mvp0-plan.md#s8-gestión-de-colecciones-l): alcance de S8.4 y Definition of Done de S8.
- [specifications.md → «Rutas privadas»](../../../context/specifications.md#rutas-privadas): formato y tratamiento de identificadores, respuesta GET y redirección POST.
- [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos): aislamiento por usuario.
- [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura) y [«Web»](../../../context/architecture.md#web): límites de capa y presentación de respuestas.
- [AC-COL-04 — Eliminar colección](../../../plans/mvp0-acceptance-criteria.md#ac-col-04-eliminar-colección): criterio aprobado de eliminación y respuestas de Application para identificadores inválidos y recursos inaccesibles.
- [S8.2 — Operaciones de colecciones](../../s8.2-operaciones-colecciones/spec.md): rutas de edición y eliminación de colección a las que se aplica este trabajo.
- [S7.7 — Dashboard y respuestas privadas](../../s7.7-dashboard-y-respuestas-privadas/spec.md#criterios-de-aceptación): resultado común ya implementado para presentar la respuesta GET de recurso no accesible, sin depender de esta spec.

## Alcance

- **Incluye:** tratamiento en las páginas canónicas de edición y eliminación de colección de identificadores de ruta inválidos y de identificadores válidos que refieran a recursos inexistentes o ajenos; respuestas GET y POST; comprobación de que el tratamiento no filtra datos ni modifica recursos.
- **Excluye:** formularios nuevos o cambios a las reglas de negocio, contratos de Application, catálogo de errores o mensajes de las fuentes; creación, actualización y borrado exitosos; presentación de errores de validación junto al campo y foco (S8.3); suite transversal de pruebas HTTP (S8.5); revisión manual funcional o accesible (S8.6–S8.7); despliegue.
- **Trabajo vecino y responsable de sus entregables:** S8.2 implementa las páginas y operaciones; S8.3 presenta los errores de validación y reglas de dominio de los formularios; S8.5 añade la suite HTTP transversal. Esta spec valida el tratamiento de los identificadores en las rutas de colección dentro de pruebas propias y puede cerrarse sin trabajo ni evidencia de esos vecinos.

## Dependencias y prerrequisitos

- S8.2 tiene sus páginas de edición y eliminación implementadas; se reutilizan como consumidoras sin reabrir sus criterios de operación.
- El tratamiento común de GET para recursos válidos no accesibles se implementó en S7.7 y su criterio aprobado proporciona el límite común de presentación. S8.4 valida que las rutas de colección integran ese resultado correctamente.
- Application ya distingue identificadores inválidos de recursos válidos no accesibles según AC-COL-04. Si la comprobación de contratos detecta una discrepancia, se bloquea la tarea y se escala al documento propietario; esta spec no inventa ni cambia el contrato.
- No se requiere que S8.3, S8.5, S8.6, S8.7 ni ninguna etapa posterior aporte código, tests o evidencia para cerrar esta spec.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [«Rutas privadas»](../../../context/specifications.md#rutas-privadas), [«Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos) y [AC-COL-04](../../../plans/mvp0-acceptance-criteria.md#ac-col-04-eliminar-colección) | Una petición GET de edición o eliminación de colección incluye un identificador que no cumple el formato válido. | Se procesa la ruta. | Se responde con error de validación sin consultar ni modificar el recurso, sin cargar datos de colección ni revelar datos ajenos. |
| CA02 | [«Rutas privadas»](../../../context/specifications.md#rutas-privadas), [«Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos) y [AC-COL-04](../../../plans/mvp0-acceptance-criteria.md#ac-col-04-eliminar-colección) | Una petición GET identifica una colección válida inexistente o perteneciente a otra persona. | Se solicita la edición o confirmación de eliminación. | En edición se presenta la respuesta GET común de recurso no accesible; en eliminación se presenta la respuesta de eliminación definida para GET. En ambos casos no se muestran datos de colección ni se distingue entre inexistencia y propiedad ajena, y la respuesta de eliminación ofrece el enlace al listado correspondiente. |
| CA03 | [«Rutas privadas»](../../../context/specifications.md#rutas-privadas), [«Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos) y [AC-COL-04](../../../plans/mvp0-acceptance-criteria.md#ac-col-04-eliminar-colección) | Un POST de eliminación incluye un identificador inválido o uno válido que identifica una colección inexistente o ajena. | Se envía la solicitud de eliminación. | El ID inválido produce el resultado de validación previsto; los dos casos válidos no accesibles producen el aviso genérico específico de eliminación y redirigen al listado de colecciones. Ningún caso modifica datos ni revela la causa de inaccesibilidad. |
| CA04 | [«Rutas privadas»](../../../context/specifications.md#rutas-privadas), [«Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos) y [AC-COL-04](../../../plans/mvp0-acceptance-criteria.md#ac-col-04-eliminar-colección) | Se comparan rutas de edición/eliminación con identificador válido inexistente y válido de una colección de otro usuario, en GET y, para eliminación, POST. | Se comparan los datos de respuesta observables y el estado persistido del usuario propietario y del usuario de prueba. | Se demuestra que las respuestas no diferencian las dos causas de inaccesibilidad y que los datos de ambos usuarios no se filtran ni alteran. Una comprobación negativa confirma que las pruebas fallan ante una expectativa de respuesta discriminante. |

Los formatos inválidos y las respuestas de cada método se comprueban mediante las referencias fuente; esta spec no redefine códigos, mensajes ni formatos.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado durante la preparación de esta especificación. La integración concreta con los resultados existentes de Application y el componente común Web se verificará al diseñar el plan, después de aprobar esta spec. | — | — | — |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 25-X-2026 («Siguiente»).
