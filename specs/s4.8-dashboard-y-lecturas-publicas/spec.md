# Especificación: S4.8 — Dashboard y lecturas públicas

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 17-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Objetivo

Implementar en Application el caso de uso del dashboard y los casos de uso de lectura pública previstos para el MVP0, consumiendo los contratos existentes y respetando los criterios de aceptación aprobados.

## Fuentes

- [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l): alcance y separación respecto a los tests con fakes y al registro de eventos.
- [roadmap.md → «Now»](../../context/roadmap.md#now): etapa activa, puerta de criterios y estado de los trabajos S4 anteriores.
- [Catálogo de criterios de aceptación del MVP0 → «Dashboard y páginas públicas»](../../plans/mvp0-acceptance-criteria.md#dashboard-y-páginas-públicas): criterios aprobados AC-DASH-01, AC-USER-01, AC-COLLS-01 y AC-COLLECTION-01.
- [Catálogo de criterios de aceptación del MVP0 → «Reglas transversales»](../../plans/mvp0-acceptance-criteria.md#reglas-transversales): criterios aprobados AC-TRANS-01 y AC-TRANS-02.
- [requirements.md → «Páginas privadas»](../../context/requirements.md#páginas-privadas) y [«Páginas públicas»](../../context/requirements.md#páginas-públicas): capacidades del dashboard y páginas públicas.
- [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas), [«Listados»](../../context/specifications.md#listados), [«Rutas públicas»](../../context/specifications.md#rutas-públicas), [«Paginación de las páginas públicas»](../../context/specifications.md#paginación-de-las-páginas-públicas), [«Página de usuario»](../../context/specifications.md#página-de-usuario), [«Página de colecciones»](../../context/specifications.md#página-de-colecciones) y [«Página de colección»](../../context/specifications.md#página-de-colección): comportamiento exacto aplicable.
- [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y [«Público y privado»](../../context/domain-model.md#público-y-privado): límites de acceso e invariantes de privacidad.
- [architecture.md → «Application»](../../context/architecture.md#application), [«Casos de uso iniciales»](../../context/architecture.md#casos-de-uso-iniciales) y [«Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos): responsabilidad de la capa y dirección de dependencias.
- [AGENTS.md → «Invariantes críticas»](../../AGENTS.md#invariantes-críticas) y [«Convención de estructura del código»](../../AGENTS.md#convención-de-estructura-del-código): identidad fiable, privacidad y organización de tipos.
- [S4.1 — contratos de aplicación](../s4.1-contratos-aplicacion/spec.md), [S4.2 — contratos de repositorios, Unit of Work e identidad identificada](../s4.2-contratos-repositorios-identidad/spec.md) y [S4.3 — contrato del proveedor de identidad de desarrollo](../s4.3-proveedor-identidad-desarrollo/spec.md): contratos y abstracciones existentes.
- [S4.4 — criterios de aceptación de los casos de uso](../s4.4-criterios-aceptacion-casos-uso/spec.md) y [S4.5 — revisión y aprobación de los criterios de aceptación](../s4.5-aprobacion-criterios-aceptacion/spec.md): preparación y aprobación del catálogo propietario.
- [S4.6 — casos de uso de colecciones](../s4.6-casos-uso-colecciones/spec.md) y [S4.7 — casos de uso de enlaces](../s4.7-casos-uso-enlaces/spec.md): trabajos precedentes aceptados y separación de responsabilidades.

## Alcance

- Incluye:
  - Casos de uso de Application para obtener los datos requeridos por el dashboard del usuario identificado.
  - Casos de uso de Application para las lecturas públicas de la página de usuario, la página de colecciones del usuario y la página de una colección.
  - Coordinación con las abstracciones de identidad y los contratos de lectura existentes, y representación de los resultados con los contratos de Application.
  - Respeto de los criterios aprobados de propiedad, privacidad, selección de contenido, orden y paginación, sin trasladar a Application la presentación HTML ni las reglas de enrutamiento propias de Web.
- Excluye:
  - Cambiar reglas funcionales, criterios aprobados, contratos existentes o mensajes. Si se detecta una contradicción o insuficiencia, se detiene el criterio afectado y se resuelve en su documento propietario.
  - Landing, páginas Razor, renderizado HTML, metadatos y canonical como presentación, rutas, respuestas HTTP e integración HTTP; corresponden a Web y a sus etapas del roadmap.
  - Implementar repositorios concretos, SQL, SQLite, FTS5 o persistencia; corresponden a S5 y S6.
  - Crear fakes o tests de comportamiento de los casos de uso, asignados a S4.9.
  - Implementar registro de eventos, asignado a S4.10.
  - Añadir gestión de etiquetas, autenticación real, scraping u otras capacidades fuera del alcance MVP0 de S4.8.
- Trabajo vecino y responsable de sus entregables:
  - S4.1–S4.3 aportan los contratos de Application, repositorios e identidad.
  - S4.4 preparó el catálogo de criterios y S4.5 obtuvo su aprobación.
  - S4.6 implementó los casos de uso de colecciones y S4.7 los de enlaces.
  - S4.9 prepara fakes y pruebas de los casos de uso; S4.10 registra eventos.
  - S5–S6 implementan la persistencia y búsqueda concretas; S7 y S11 implementan la presentación Web privada y pública, respectivamente.

## Dependencias y prerrequisitos

- S4.1, S4.2 y S4.3 están completadas y aceptadas para sus alcances; están disponibles `IDashboardRepository`, `IPublicReadRepository`, `ICurrentUser`, `DashboardDto`, `PublicUserDto` y `PublicCollectionDto`, además de los modelos de Application usados en sus resultados. Su existencia no acredita el comportamiento de los casos de uso.
- S4.5 aprobó el catálogo de criterios el 16-X-2026; la puerta de criterios está satisfecha.
- S4.6 y S4.7 están completadas y aceptadas; son precedentes de estructura y coordinación de casos de uso, no sustituyen los criterios propios de S4.8.
- Los fakes y tests de comportamiento del consumidor pertenecen a S4.9; no se presume su disponibilidad ni su cobertura al preparar esta especificación.
- La aprobación de esta especificación autoriza preparar el plan técnico, no crear tareas ni iniciar la implementación. Las tareas se prepararán solo después de la aprobación explícita del plan.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [AC-DASH-01](../../plans/mvp0-acceptance-criteria.md#ac-dash-01--dashboard), [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas) y [AC-TRANS-01](../../plans/mvp0-acceptance-criteria.md#ac-trans-01--identidad-propiedad-y-límites-de-acceso) | Una solicitud de datos del dashboard y una identidad fiable. | Se ejecuta el caso de uso privado del dashboard. | El resultado corresponde al usuario identificado y permite al consumidor presentar el dashboard conforme al criterio aprobado; no se acepta identidad del cliente ni se incorporan operaciones de presentación Web a Application. |
| CA02 | [AC-USER-01](../../plans/mvp0-acceptance-criteria.md#ac-user-01--página-pública-de-usuario), [specifications.md → «Página de usuario»](../../context/specifications.md#página-de-usuario) y [«Listados»](../../context/specifications.md#listados) | Una lectura pública localizada por alias, con usuario existente o inexistente y datos de distintos estados de privacidad. | Se obtiene la página pública del usuario. | El resultado aplica el criterio aprobado y las fuentes propietarias para selección, orden y estados vacíos; no expone contenido privado ni datos personales no públicos, y el caso inexistente se representa según el contrato de Application, sin decidir aquí la respuesta HTTP. |
| CA03 | [AC-COLLS-01](../../plans/mvp0-acceptance-criteria.md#ac-colls-01--página-pública-de-colecciones), [specifications.md → «Página de colecciones»](../../context/specifications.md#página-de-colecciones) y [«Listados»](../../context/specifications.md#listados) | Una lectura pública localizada por alias, con colecciones que pueden o no mostrarse en público. | Se obtiene la página pública de colecciones. | El resultado contiene únicamente los elementos que el criterio aprobado permite mostrar, en el orden aplicable, o representa el estado vacío/usuario inexistente mediante el resultado de Application previsto; no revela colecciones privadas ni colecciones que no deban aparecer en público. |
| CA04 | [AC-COLLECTION-01](../../plans/mvp0-acceptance-criteria.md#ac-collection-01--página-pública-de-una-colección), [specifications.md → «Página de colección»](../../context/specifications.md#página-de-colección), [«Paginación de las páginas públicas»](../../context/specifications.md#paginación-de-las-páginas-públicas) y [«Listados»](../../context/specifications.md#listados) | Una lectura pública localizada por alias y slug, con colección pública o no accesible y enlaces de distintos estados. | Se obtiene una página de colección con el número de página aplicable. | El resultado aplica la selección de enlaces, el orden, la paginación y los casos no accesibles del criterio aprobado; ningún enlace privado ni colección privada se hace accesible mediante la lectura. |
| CA05 | [AC-TRANS-01](../../plans/mvp0-acceptance-criteria.md#ac-trans-01--identidad-propiedad-y-límites-de-acceso), [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos), [«Público y privado»](../../context/domain-model.md#público-y-privado) y [AGENTS.md → «Invariantes críticas»](../../AGENTS.md#invariantes-críticas) | Una petición privada o pública con localizadores y recursos pertenecientes a usuarios distintos. | Se ejecuta cualquiera de los casos de uso incluidos. | El dashboard usa solo la identidad fiable y las lecturas públicas usan sus localizadores únicamente según la fuente propietaria; no se accede a datos privados ni se permite que una entrada del cliente autorice una lectura privada. |
| CA06 | [AC-TRANS-02](../../plans/mvp0-acceptance-criteria.md#ac-trans-02--errores-y-mensajes-accesibles), [architecture.md → «Application»](../../context/architecture.md#application) y [«Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos) | Los casos de uso de dashboard y lecturas públicas junto con los contratos disponibles. | Se revisan los resultados, dependencias y tipos públicos de Application. | Los resultados y errores respetan los contratos existentes sin mensajes de presentación ni detalles de infraestructura; los tipos y dependencias permanecen en la capa y estructura aprobadas, sin acoplamiento a Web o Infrastructure. |

Los escenarios negativos y límites se comprueban en las fuentes y criterios aprobados enlazados; esta especificación no vuelve a definirlos. La presentación HTTP de alias, slugs y páginas inexistentes, así como el HTML, canonical y metadatos, se valida en las etapas Web correspondientes. La comprobación de comportamiento de consumidores corresponde a S4.9; el registro de eventos corresponde a S4.10.

El cierre de S4.8 evalúa únicamente los entregables y responsabilidades de Application definidos aquí. Las validaciones de consumidor, persistencia y presentación se planifican y cierran en sus etapas propietarias, sin ser prerrequisitos para cerrar ni motivos para reabrir S4.8.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| No se identifica una contradicción funcional bloqueante para redactar esta especificación. Durante el diseño se debe contrastar que los contratos actuales expresen los resultados necesarios para usuario inexistente, colección no accesible y estado vacío, sin ampliar reglas por inferencia. | [specifications.md → «Página de usuario»](../../context/specifications.md#página-de-usuario), [«Página de colecciones»](../../context/specifications.md#página-de-colecciones), [«Página de colección»](../../context/specifications.md#página-de-colección), contratos de S4.2 y criterios del catálogo | CA02–CA04, según el hallazgo | Si un contrato no permite representar un resultado requerido, detener el criterio y elevar una propuesta respaldada a revisión antes de cambiarlo o implementar. |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 17-X-2026 («Siguiente»); [tareas](tasks.md), aprobadas por DLG el 17-X-2026 («Siguiente») y cerradas para el alcance de Application con aceptación «Fase completada».
