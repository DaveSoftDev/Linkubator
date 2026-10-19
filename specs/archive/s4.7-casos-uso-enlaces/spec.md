# Especificación: S4.7 — Casos de uso de enlaces

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Objetivo

Implementar en Application los casos de uso MVP0 para crear, editar, mover, cambiar la visibilidad, eliminar y listar enlaces, consumiendo la identidad fiable y los contratos existentes, y respetando los criterios de aceptación aprobados para cada operación.

## Fuentes

- [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l): alcance y separación de los casos de uso, pruebas de consumidores y registro de eventos.
- [roadmap.md → «Now»](../../../context/roadmap.md#now): estado de S4, aprobación de la puerta de criterios y trabajo previo.
- [Catálogo de criterios de aceptación del MVP0 → «Enlaces»](../../../plans/mvp0-acceptance-criteria.md#enlaces): criterios aprobados AC-LINK-01, AC-LINK-01b y AC-LINK-02–AC-LINK-06.
- [requirements.md → «Enlaces»](../../../context/requirements.md#enlaces): capacidades funcionales incluidas.
- [specifications.md → «URLs de los enlaces»](../../../context/specifications.md#urls-de-los-enlaces), [«Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros), [«Listados»](../../../context/specifications.md#listados), [«Rutas privadas»](../../../context/specifications.md#rutas-privadas) y [«Errores controlados»](../../../context/specifications.md#errores-controlados): comportamiento y resultados exactos aplicables.
- [domain-model.md → «Link»](../../../context/domain-model.md#link), [«Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos) y [«Público y privado»](../../../context/domain-model.md#público-y-privado): invariantes que deben preservarse.
- [architecture.md → «Application»](../../../context/architecture.md#application) y [«Persistencia»](../../../context/architecture.md#persistencia): responsabilidades de capa, contratos y garantías técnicas.
- [AGENTS.md → «Invariantes críticas»](../../../AGENTS.md#invariantes-críticas) y [«Convención de estructura del código»](../../../AGENTS.md#convención-de-estructura-del-código): identidad, privacidad y organización de tipos.
- [S4.1 — contratos de aplicación](../s4.1-contratos-aplicacion/spec.md), [S4.2 — contratos de repositorios, Unit of Work e identidad identificada](../s4.2-contratos-repositorios-identidad/spec.md) y [S4.3 — contrato del proveedor de identidad de desarrollo](../s4.3-proveedor-identidad-desarrollo/spec.md): contratos existentes que consumirá Application.
- [S4.4 — criterios de aceptación de los casos de uso](../s4.4-criterios-aceptacion-casos-uso/spec.md) y [S4.5 — revisión y aprobación de los criterios de aceptación](../s4.5-aprobacion-criterios-aceptacion/spec.md): preparación y aprobación del catálogo propietario.
- [S4.6 — casos de uso de colecciones](../s4.6-casos-uso-colecciones/spec.md): trabajo precedente aceptado y separación de responsabilidades entre bloques.

## Alcance

- Incluye:
  - Casos de uso de Application para crear, editar, mover, cambiar visibilidad, eliminar y listar enlaces del usuario identificado.
  - Validación de entradas conforme a los contratos existentes; delegación de reglas e invariantes al Domain; coordinación con los contratos de repositorios y Unit of Work; y resultados mediante los contratos de Application.
  - Aplicación de las comprobaciones de propiedad, URL, duplicados, colección asociada, privacidad, errores y listado definidas por los criterios aprobados.
- Excluye:
  - Cambiar reglas funcionales, criterios aprobados, contratos existentes o mensajes. Si se descubre una contradicción, se detiene el criterio afectado y se resuelve en su fuente propietaria.
  - Asociar etiquetas, ejecutar scraping o implementar registro de eventos; las etiquetas y el scraping están fuera del MVP0, y el registro corresponde a S4.10.
  - Implementar repositorios concretos, SQL, SQLite, FTS5, páginas Razor, integración HTTP, fakes o tests de consumidores, asignados a sus trabajos y etapas correspondientes.
  - Dashboard y lecturas públicas (S4.8), gestión Web de enlaces (S9) y búsqueda Web de enlaces (S10).
- Trabajo vecino y responsable de sus entregables:
  - S4.1–S4.3 aportan contratos de Application, repositorios/identidad y proveedor de desarrollo.
  - S4.4 preparó los criterios y S4.5 obtuvo su aprobación humana.
  - S4.6 implementó los casos de uso de colecciones; S4.9 prepara fakes y tests de los casos de uso; S4.10 registra eventos.
  - S5 y S6 implementan persistencia concreta y proyecciones de búsqueda; S9 y S10 implementan la gestión y el listado Web de enlaces.

## Dependencias y prerrequisitos

- S4.1, S4.2 y S4.3 están completadas y aceptadas para sus respectivos alcances; sus contratos y DTOs relevantes están disponibles en Application.
- S4.5 aprobó el catálogo de criterios el 16-X-2026, por lo que la puerta de criterios está satisfecha.
- S4.6 está completada y aceptada por DLG el 17-X-2026. Es antecedente de organización y flujo, no una dependencia funcional del comportamiento de enlaces.
- Los criterios de comportamiento de consumidores se validarán con los fakes y tests asignados a S4.9; no se presume esa cobertura al preparar esta especificación.
- La aprobación de esta especificación solo autoriza preparar el plan técnico. Las tareas se prepararán después de la aprobación explícita del plan; no se autoriza iniciar la implementación todavía.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [AC-LINK-01](../../../plans/mvp0-acceptance-criteria.md#ac-link-01--crear-enlace) y [AC-LINK-01b](../../../plans/mvp0-acceptance-criteria.md#ac-link-01b--crear-enlace-público), [specifications.md → «URLs de los enlaces»](../../../context/specifications.md#urls-de-los-enlaces) | Una solicitud de creación de enlace y una identidad fiable. | Se ejecuta el caso de uso con datos que pueden ser válidos, inválidos o duplicados, y con una colección propia o no propia. | El resultado observa los criterios aprobados de ajuste y validación de URL, unicidad, asociación y visibilidad; cualquier rechazo deja el enlace sin crear y no expone datos de otro usuario. |
| CA02 | [AC-LINK-02](../../../plans/mvp0-acceptance-criteria.md#ac-link-02--editar-enlace), [domain-model.md → «Link»](../../../context/domain-model.md#link) | Un enlace propio y una solicitud de edición. | Se editan los campos permitidos, se intenta cambiar la URL base o se solicita hacerlo público desde una colección privada. | Solo se aplican cambios permitidos; la URL base no se modifica y un rechazo de edición no deja cambios parciales, conforme al criterio aprobado. |
| CA03 | [AC-LINK-03](../../../plans/mvp0-acceptance-criteria.md#ac-link-03--mover-enlace), [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos) y [«Público y privado»](../../../context/domain-model.md#público-y-privado) | Un enlace propio y una colección destino que puede pertenecer al usuario o a otro. | Se solicita mover el enlace. | Solo se acepta un destino permitido por los criterios; el movimiento conserva la identidad y datos que correspondan, aplica el estado de visibilidad especificado y rechaza destinos ajenos o manipulados sin cambios. |
| CA04 | [AC-LINK-04](../../../plans/mvp0-acceptance-criteria.md#ac-link-04--cambiar-visibilidad-de-un-enlace), [domain-model.md → «Público y privado»](../../../context/domain-model.md#público-y-privado) y [specifications.md → «Rutas privadas»](../../../context/specifications.md#rutas-privadas) | Un enlace propio y una solicitud de cambio de visibilidad. | Se solicita un estado destino permitido, inválido o incompatible con la colección. | El resultado respeta las invariantes público/privado y el tratamiento de rechazo aprobado; un rechazo no modifica el enlace. |
| CA05 | [AC-LINK-05](../../../plans/mvp0-acceptance-criteria.md#ac-link-05--eliminar-enlace), [architecture.md → «Application»](../../../context/architecture.md#application) y [specifications.md → «Errores controlados»](../../../context/specifications.md#errores-controlados) | Una solicitud de eliminación de un enlace propio, con identificador inválido o con identificador válido inexistente/ajeno. | Application valida el identificador y ejecuta o rechaza la operación. | Solo se elimina el enlace propio permitido; un identificador inválido produce error de validación, y un identificador válido inexistente o ajeno produce el mismo resultado `ResourceNotAccessible` y código estable, sin modificar datos ni revelar cuál de esas dos causas ocurrió. |
| CA06 | [AC-LINK-06](../../../plans/mvp0-acceptance-criteria.md#ac-link-06--listar-enlaces-del-usuario), [specifications.md → «Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros) y [«Listados»](../../../context/specifications.md#listados) | Un usuario identificado y enlaces de uno o más usuarios, con parámetros aplicables al listado. | Se solicita una página del listado con búsqueda y filtro de colección, si corresponden. | El resultado contiene solo enlaces del usuario identificado y aplica búsqueda, filtro, orden, paginación y tratamiento de parámetros conforme al criterio aprobado; no incluye otras entidades. |
| CA07 | [AC-TRANS-01](../../../plans/mvp0-acceptance-criteria.md#ac-trans-01--identidad-propiedad-y-límites-de-acceso), [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos) y [AGENTS.md → «Invariantes críticas»](../../../AGENTS.md#invariantes-críticas) | Una operación privada con identidad, enlace, colección y recursos de usuarios distintos. | Se ejecuta una lectura, creación o modificación. | La identidad procede de la abstracción fiable, nunca de datos de cliente; se verifica la propiedad del enlace y de sus relaciones y ninguna operación expone o modifica recursos ajenos. |
| CA08 | [AC-TRANS-02](../../../plans/mvp0-acceptance-criteria.md#ac-trans-02--errores-y-mensajes-accesibles), [specifications.md → «Errores controlados»](../../../context/specifications.md#errores-controlados) y [architecture.md → «Application»](../../../context/architecture.md#application) | Una operación rechazada por validación, regla de dominio o acceso a un recurso. | Application comunica su resultado al consumidor. | El resultado emplea los contratos de error existentes y los códigos requeridos, sin textos de presentación, datos privados ni detalles de persistencia; la traducción HTTP queda en Web. |
| CA09 | [architecture.md → «Application»](../../../context/architecture.md#application), [«Persistencia»](../../../context/architecture.md#persistencia), [S4.1 → «Criterios de aceptación»](../s4.1-contratos-aplicacion/spec.md#criterios-de-aceptación), [S4.2 → «Criterios de aceptación»](../s4.2-contratos-repositorios-identidad/spec.md#criterios-de-aceptación) y [AGENTS.md → «Convención de estructura del código»](../../../AGENTS.md#convención-de-estructura-del-código) | Los casos de uso de enlaces y los contratos aprobados de Application. | Se revisan las responsabilidades, dependencias y estructura de los tipos. | La orquestación permanece en Application, consume contratos existentes sin acoplarse a Infrastructure o Web y mantiene los tipos públicos organizados por responsabilidad; las transacciones no contienen esperas ni operaciones externas. |

Los casos negativos y límites concretos se verifican mediante las fuentes y criterios aprobados enlazados; esta especificación no los redefine. La cobertura con fakes y tests de comportamiento pertenece a S4.9, el registro de eventos a S4.10 y la garantía final frente a condiciones de carrera y duplicados persistentes a las etapas de persistencia.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| No se identifica una contradicción funcional bloqueante para redactar esta especificación. Si al diseñar se detecta incompatibilidad de los contratos o un hueco en las reglas aprobadas, se detendrá el criterio afectado y se solicitará resolverlo en su fuente propietaria. | [requirements.md](../../../context/requirements.md), [specifications.md](../../../context/specifications.md), [domain-model.md](../../../context/domain-model.md), [architecture.md](../../../context/architecture.md) y [Catálogo de criterios](../../../plans/mvp0-acceptance-criteria.md) | Según el hallazgo | Resolver el hueco en la fuente propietaria y revisar los artefactos dependientes antes de implementar. |

## Artefactos relacionados

- [S4.6 — casos de uso de colecciones](../s4.6-casos-uso-colecciones/spec.md), precedente completado y aceptado.