# Especificación: S4.6 — Casos de uso de colecciones

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 16-X-2026 («A planear»).

## Objetivo

Implementar en Application los casos de uso MVP0 para crear, renombrar, cambiar la visibilidad, eliminar y listar colecciones, consumiendo la identidad fiable y los contratos existentes, y respetando los criterios de aceptación aprobados para cada operación.

## Fuentes

- [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l): alcance, orden y separación de S4.6 respecto a pruebas y registro de eventos.
- [roadmap.md → «Now»](../../context/roadmap.md#now): estado de S4, puerta de criterios y estado previo a este trabajo.
- [Catálogo de criterios de aceptación del MVP0 → «Colecciones»](../../plans/mvp0-acceptance-criteria.md#colecciones): criterios aprobados AC-COL-01, AC-COL-01b y AC-COL-02–AC-COL-05.
- [requirements.md → «Colecciones»](../../context/requirements.md#colecciones) y [«Público y privado»](../../context/requirements.md#público-y-privado): capacidades funcionales incluidas.
- [specifications.md → «Generación de slugs»](../../context/specifications.md#generación-de-slugs), [«Rutas privadas»](../../context/specifications.md#rutas-privadas), [«Listados»](../../context/specifications.md#listados) y [«Errores controlados»](../../context/specifications.md#errores-controlados): comportamiento y resultados exactos aplicables.
- [domain-model.md → «Collection»](../../context/domain-model.md#collection), [«Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [«Público y privado»](../../context/domain-model.md#público-y-privado): invariantes que los casos de uso deben preservar.
- [architecture.md → «Application»](../../context/architecture.md#application), [«Casos de uso iniciales»](../../context/architecture.md#casos-de-uso-iniciales) y [«Persistencia»](../../context/architecture.md#persistencia): separación de capas, coordinación de escrituras y garantías técnicas.
- [AGENTS.md → «Invariantes críticas»](../../AGENTS.md#invariantes-críticas) y [«Convención de estructura del código»](../../AGENTS.md#convención-de-estructura-del-código): identidad confiable, privacidad y organización de tipos.
- [S4.1 — contratos de aplicación](../s4.1-contratos-aplicacion/spec.md), [S4.2 — contratos de repositorios, Unit of Work e identidad identificada](../s4.2-contratos-repositorios-identidad/spec.md) y [S4.3 — contrato del proveedor de identidad de desarrollo](../s4.3-proveedor-identidad-desarrollo/spec.md): contratos existentes que consumirá Application.
- [S4.4 — criterios de aceptación de los casos de uso](../s4.4-criterios-aceptacion-casos-uso/spec.md) y [S4.5 — revisión y aprobación de los criterios de aceptación](../s4.5-aprobacion-criterios-aceptacion/spec.md): preparación y aprobación del catálogo propietario.

## Alcance

- Incluye:
  - Casos de uso de Application para crear, renombrar, cambiar visibilidad, eliminar y listar colecciones del usuario identificado.
  - Validar entradas mediante los contratos existentes, delegar invariantes y transformaciones a Domain, coordinar acceso a datos con los contratos de repositorio y Unit of Work y representar resultados con los contratos de Application.
  - Aplicar las comprobaciones de propiedad, unicidad, estado de colecciones y enlaces, privacidad y listado que requieren los criterios aprobados.
  - Mantener separados los rechazos funcionales, la presentación HTTP y los detalles de persistencia.
- Excluye:
  - Cambiar reglas funcionales, criterios aprobados, contratos existentes o mensajes; cualquier contradicción se eleva a la fuente propietaria.
  - Implementar repositorios concretos, SQL, SQLite, FTS5, páginas Razor, integración HTTP o fakes y tests de consumidores, asignados a sus trabajos/etapas correspondientes.
  - Implementar el registro de eventos, asignado a S4.10; esta especificación no afirma que exista logging de los casos de uso.
  - Gestionar enlaces, dashboard, lecturas públicas, etiquetas, autenticación real, correo, scraping o capacidades fuera del alcance S4.6.
- Trabajo vecino y responsable de sus entregables:
  - S4.1–S4.3 aportan los contratos compartidos, de repositorios/identidad y del proveedor de desarrollo.
  - S4.4 preparó los criterios y S4.5 obtuvo su aprobación humana.
  - S4.7 implementa los casos de uso de enlaces; S4.8, dashboard y lecturas públicas; S4.9, fakes y pruebas de casos de uso; S4.10, registro de eventos.
  - S5 implementa persistencia concreta y S8 presenta la gestión de colecciones en Web.

## Dependencias y prerrequisitos

- S4.1, S4.2 y S4.3 están completadas y aceptadas para sus respectivos alcances. Los contratos y DTOs relevantes están presentes en Application; no acreditan por sí mismos comportamiento de los casos de uso.
- S4.5 aprobó el catálogo de criterios de aceptación el 16-X-2026; por tanto, está satisfecha la puerta de criterios del plan.
- El inventario de Application muestra contratos y validación de entrada, pero todavía no contiene casos de uso de colecciones. La implementación de este trabajo no ha comenzado.
- Los criterios de comportamiento de consumidor se verificarán con los fakes y tests de S4.9; la disponibilidad de dicha cobertura no se presume al preparar esta especificación.
- La aprobación de esta especificación autoriza preparar el plan técnico, no crear tareas ni iniciar implementación. Las tareas se prepararán solo tras la aprobación explícita del plan.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [S4 → «Casos de uso de colecciones»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l), [AC-COL-01](../../plans/mvp0-acceptance-criteria.md#ac-col-01--crear-colección), [AC-COL-01b](../../plans/mvp0-acceptance-criteria.md#ac-col-01b--hacer-pública-una-colección-vacía) y [AC-COL-02](../../plans/mvp0-acceptance-criteria.md#ac-col-02--renombrar-colección) | Una solicitud válida de creación o renombrado y una identidad de usuario fiable. | Se ejecuta la operación correspondiente. | El caso de uso realiza el resultado previsto por el criterio aprobado, incluidos los rechazos por entrada o colisión, sin persistir parcialmente una operación fallida. |
| CA02 | [AC-COL-03](../../plans/mvp0-acceptance-criteria.md#ac-col-03--cambiar-visibilidad-de-una-colección) y [AC-COL-03b](../../plans/mvp0-acceptance-criteria.md#ac-col-03b--atomicidad-al-hacer-privada-una-colección) | Una colección propia con el estado de enlaces contemplado en el criterio. | Se solicita un cambio de visibilidad permitido o se rechaza un estado destino no válido. | La operación preserva las invariantes público/privado y la atomicidad especificadas; un rechazo no deja cambios parciales. |
| CA03 | [AC-COL-04](../../plans/mvp0-acceptance-criteria.md#ac-col-04--eliminar-colección) y [AC-COL-04b](../../plans/mvp0-acceptance-criteria.md#ac-col-04b--eliminar-una-colección-con-enlaces-o-la-última-colección), [specifications.md → «Errores controlados»](../../context/specifications.md#errores-controlados) | Una solicitud de eliminación de una colección propia o un identificador no accesible. | Application ejecuta o rechaza la eliminación conforme al caso. | La colección solo se elimina en las condiciones aprobadas; las causas de recurso no accesible conservan la categoría y el código estables definidos para la operación, y las restricciones de dominio no se confunden con ese error. |
| CA04 | [AC-COL-05](../../plans/mvp0-acceptance-criteria.md#ac-col-05--listar-colecciones-del-usuario), [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros) y [«Listados»](../../context/specifications.md#listados) | Un usuario identificado con colecciones que pueden pertenecer a usuarios distintos. | Se solicita una página del listado con sus parámetros. | El resultado se limita a las colecciones del usuario y respeta la búsqueda, orden, paginación y tratamiento de parámetros del criterio aprobado; no se incluyen otras entidades. |
| CA05 | [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos), [AGENTS.md → «Invariantes críticas»](../../AGENTS.md#invariantes-críticas) y [AC-TRANS-01](../../plans/mvp0-acceptance-criteria.md#ac-trans-01--identidad-propiedad-y-límites-de-acceso) | Una operación privada con identidad y recursos de usuarios distintos. | Se ejecuta una lectura o escritura de colecciones. | La identidad procede de la abstracción fiable, no de datos de cliente, y ninguna operación privada expone ni modifica colecciones ajenas. |
| CA06 | [architecture.md → «Application»](../../context/architecture.md#application), [«Persistencia»](../../context/architecture.md#persistencia), [S4.1 → «Criterios de aceptación»](../s4.1-contratos-aplicacion/spec.md#criterios-de-aceptación) y [S4.2 → «Criterios de aceptación»](../s4.2-contratos-repositorios-identidad/spec.md#criterios-de-aceptación) | Los casos de uso de colecciones y los contratos aprobados de Application. | Se revisan su estructura, dependencias y ciclo de escritura. | La orquestación permanece en Application, usa los contratos aprobados sin acoplarse a Infrastructure/Web y no mantiene una transacción durante esperas del usuario ni operaciones externas. |
| CA07 | [AC-TRANS-02](../../plans/mvp0-acceptance-criteria.md#ac-trans-02--errores-y-mensajes-accesibles), [specifications.md → «Errores controlados»](../../context/specifications.md#errores-controlados) y [architecture.md → «Application»](../../context/architecture.md#application) | Una operación que termina con error de validación, dominio o acceso a recurso. | Application devuelve el resultado al consumidor. | El resultado usa los contratos de error existentes y los códigos requeridos, sin incorporar texto de presentación, datos privados ni detalles de persistencia. |
| CA08 | [AGENTS.md → «Convención de estructura del código»](../../AGENTS.md#convención-de-estructura-del-código), [architecture.md → «Application»](../../context/architecture.md#application) y [AC-COL-01–AC-COL-05](../../plans/mvp0-acceptance-criteria.md#criterios-de-aceptación) | La implementación de los casos de uso de colecciones. | Se revisan los tipos públicos, el alcance funcional y su cobertura trazable. | Los tipos públicos están organizados por responsabilidad y caso de uso; se cubren las cinco operaciones de S4.6 sin introducir capacidad fuera de alcance. |

Los valores, mensajes, algoritmos, escenarios negativos y límites se consultan en las fuentes enlazadas y en los criterios aprobados; no se reproducen aquí como reglas paralelas. La prueba de comportamiento de consumidores pertenece a S4.9, y la comprobación de eventos a S4.10.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| No se identifica una contradicción funcional bloqueante para redactar la especificación. Si el diseño técnico descubre un hueco en las reglas, los contratos o la cobertura aprobada, se detendrá el criterio afectado y se solicitará resolverlo en su documento propietario. | [requirements.md](../../context/requirements.md), [specifications.md](../../context/specifications.md), [domain-model.md](../../context/domain-model.md), [architecture.md](../../context/architecture.md) y [Catálogo de criterios](../../plans/mvp0-acceptance-criteria.md) | Según el hallazgo | Resolver el hueco en la fuente propietaria y revisar los artefactos dependientes antes de implementar. |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 16-X-2026 («Aprovado»); [tareas](tasks.md), aprobadas por DLG el 16-X-2026 («Ready»). Implementación, validación y aceptación del alcance registradas en [tareas → «Aceptación y cierre»](tasks.md#aceptación-y-cierre); DLG aceptó el resultado el 17-X-2026 («Fase completada»).
