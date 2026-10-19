# Especificación: S4.4 — Criterios de aceptación de los casos de uso

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 15-X-2026 («Revisada»).

## Objetivo

Preparar los criterios de aceptación verificables de cada caso de uso MVP0 previsto en S4, trazados a sus reglas propietarias, y dejarlos en el artefacto establecido para su revisión y aprobación separada en S4.5.

## Fuentes

- [mvp0-plan.md → «Puerta de entrada de la implementación (criterios de aceptación de S4)»](../../../plans/mvp0-plan.md#puerta-de-entrada-de-la-implementación-criterios-de-aceptación-de-s4) y [«S4: Aplicación: casos de uso (L)»](../../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l): alcance, entregable `plans/mvp0-acceptance-criteria.md`, dependencias y puerta de aprobación.
- [roadmap.md → «Now»](../../../context/roadmap.md#now): etapa activa y separación entre los criterios, su aprobación y la implementación posterior.
- [requirements.md → «Colecciones»](../../../context/requirements.md#colecciones), [«Enlaces»](../../../context/requirements.md#enlaces), [«Público y privado»](../../../context/requirements.md#público-y-privado), [«Búsqueda y filtros»](../../../context/requirements.md#búsqueda-y-filtros) y [«Páginas privadas»](../../../context/requirements.md#páginas-privadas): capacidades funcionales del alcance.
- [specifications.md → «Páginas privadas»](../../../context/specifications.md#páginas-privadas), [«Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros), [«Listados»](../../../context/specifications.md#listados), [«Páginas públicas»](../../../context/specifications.md#páginas-públicas), [«Errores controlados»](../../../context/specifications.md#errores-controlados) y [«Registro de eventos»](../../../context/specifications.md#registro-de-eventos): comportamiento observable y límites aplicables.
- [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos), [«Collection»](../../../context/domain-model.md#collection), [«Link»](../../../context/domain-model.md#link) y [«Público y privado»](../../../context/domain-model.md#público-y-privado): invariantes que los criterios deben cubrir.
- [architecture.md → «Casos de uso iniciales»](../../../context/architecture.md#casos-de-uso-iniciales), [«Persistencia»](../../../context/architecture.md#persistencia) y [«Logs»](../../../context/architecture.md#logs): límites de capas, atomicidad y registro técnico.
- [S4.1 — Result, errores, DTOs y validadores de aplicación](../s4.1-contratos-aplicacion/spec.md) y [sus tareas → «Aceptación y cierre»](../s4.1-contratos-aplicacion/tasks.md#aceptación-y-cierre): contratos implementados que deben emplear los criterios sin redefinirlos.
- [S4.2 — Contratos de repositorios, Unit of Work e identidad identificada](../s4.2-contratos-repositorios-identidad/spec.md) y [sus tareas → «Matriz de operaciones MVP0 → contratos»](../s4.2-contratos-repositorios-identidad/tasks.md#matriz-de-operaciones-mvp0--contratos): matriz de operaciones y contratos aceptados.
- [S4.3 — Contrato del proveedor de identidad de desarrollo](../s4.3-proveedor-identidad-desarrollo/spec.md): límite respecto a la resolución de identidad de desarrollo.
- [AGENTS.md → «Invariantes críticas»](../../../AGENTS.md#invariantes-críticas): privacidad, propiedad, identidad confiable, secretos y límites de transacción.

Como material derivado de contraste —no como fuente de reglas— pueden consultarse las fichas S3.5 de [Collection](../s3.5-fichas-entidades/cards/collection.md), [Link](../s3.5-fichas-entidades/cards/link.md) y [User](../s3.5-fichas-entidades/cards/user.md). Ante cualquier diferencia prevalecen las fuentes propietarias `context/`; las fichas no amplían el alcance de S4.

## Alcance

- Incluye:
  - Redactar y organizar en `plans/mvp0-acceptance-criteria.md` los criterios GIVEN/WHEN/THEN para todos los casos de uso S4 del MVP0: crear, renombrar, cambiar visibilidad, eliminar y listar colecciones; crear, editar, mover, cambiar visibilidad, eliminar y listar enlaces; obtener el dashboard; y las lecturas públicas previstas para las páginas de usuario, colecciones y colección.
  - Vincular cada criterio a la sección propietaria de `context/` y, cuando aplique, a las invariantes del dominio; describir resultados observables sin convertir los criterios en una fuente paralela de reglas.
  - Cubrir los caminos satisfactorios y los rechazos, límites y condiciones negativas aplicables que estén definidos por las fuentes.
  - Dejar el artefacto preparado para la revisión y aprobación humana de S4.5, sin confundir la preparación con dicha aprobación.
- Excluye:
  - Cambiar, completar o decidir reglas de producto, dominio, errores o contratos. Una omisión o contradicción de fuentes se registra como bloqueo y se resuelve en el documento propietario antes de aprobar criterios afectados.
  - Aprobar los criterios, responsabilidad de DLG en S4.5.
  - Implementar casos de uso, fakes, pruebas, repositorios concretos, páginas, persistencia o logs; corresponden a S4.6–S4.10 y no empiezan antes de la aprobación de S4.5.
  - Definir criterios de MVP1, autenticación real, etiquetas, scraping, correo o cualquier otra capacidad fuera del alcance S4 del plan MVP0.
- Trabajo vecino y responsable de sus entregables:
  - S4.1 y S4.2 aportan contratos ya implementados y aceptados; S4.3 define el contrato del proveedor de identidad de desarrollo.
  - S4.4 prepara `plans/mvp0-acceptance-criteria.md`; S4.5 revisa y aprueba explícitamente ese artefacto.
  - S4.6–S4.8 implementan los casos de uso; S4.9 añade fakes y pruebas trazables a los criterios aprobados; S4.10 registra los eventos permitidos.

## Dependencias y prerrequisitos

- S4.1, S4.2 y S4.3 constan como completadas y aceptadas para sus respectivos alcances en [roadmap.md → «Now»](../../../context/roadmap.md#now); sus contratos y matriz de operaciones constituyen las dependencias disponibles.
- Las fichas derivadas de S3.5 pueden servir para comprobar que el desglose considere las invariantes y riesgos relevantes de colecciones y enlaces, pero no sustituyen los documentos propietarios ni añaden casos de uso. Para cualquier regla prevalecen las fuentes propietarias actuales de `context/`.
- El alcance de casos de uso se toma de S4 en [mvp0-plan.md](../../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l), y las reglas funcionales se consultan en sus fuentes propietarias. La existencia de los contratos no acredita comportamiento de consumidores.
- El archivo `plans/mvp0-acceptance-criteria.md` no existe al preparar esta especificación; su contenido forma parte del trabajo posterior a la aprobación de esta spec y su plan técnico.
- Ninguna implementación de S4.6–S4.10 queda autorizada por esta especificación ni por la creación del futuro artefacto. La aprobación humana de los criterios sigue siendo la puerta S4.5.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) | El catálogo de operaciones de colecciones y enlaces de S4. | Se revisa el artefacto de criterios preparado. | Hay criterios identificables para cada operación enumerada en el alcance de S4, incluidos sus listados; ninguna operación S4 queda sin criterio ni se incorpora una capacidad ajena al MVP0. |
| CA02 | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l), [requirements.md → «Páginas privadas»](../../../context/requirements.md#páginas-privadas) y [specifications.md → «Páginas privadas»](../../../context/specifications.md#páginas-privadas) | Los casos de uso del dashboard y de las lecturas públicas enumerados en S4. | Se revisa su cobertura en el artefacto. | El dashboard y cada lectura pública prevista tienen criterios propios que permiten comprobar el resultado observable y la separación entre lecturas públicas y operaciones privadas. |
| CA03 | [specifications.md → «Páginas privadas»](../../../context/specifications.md#páginas-privadas), [«Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros) y [«Listados»](../../../context/specifications.md#listados) | Los casos de listado incluidos en S4. | Se contrastan sus criterios con las fuentes funcionales. | Los criterios cubren parámetros, filtros, orden y paginación aplicables a cada operación del alcance, sin repetir valores o reglas como fuente independiente y sin atribuir capacidades posteriores al MVP0. |
| CA04 | [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos), [«Collection»](../../../context/domain-model.md#collection), [«Link»](../../../context/domain-model.md#link) y [«Público y privado»](../../../context/domain-model.md#público-y-privado) | Una operación privada o una transición de visibilidad definida para S4. | Se inspeccionan sus criterios satisfactorios y negativos. | Se puede verificar la identidad confiable, la propiedad del recurso y sus relaciones, y las invariantes público/privado aplicables; no se acepta como identidad un dato autoritativo enviado por el cliente. |
| CA05 | [specifications.md → «Errores controlados»](../../../context/specifications.md#errores-controlados) y [«Páginas privadas»](../../../context/specifications.md#páginas-privadas) | Una entrada inválida, recurso inaccesible o rechazo de una regla de dominio previsto por las fuentes. | Se revisa la cobertura negativa del criterio correspondiente. | Los casos de rechazo especificados tienen una comprobación observable que permite verificar que no se aplica indebidamente la operación y que no se exponen datos privados; no se inventan errores ni mensajes. |
| CA06 | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) y [AGENTS.md → «Invariantes críticas»](../../../AGENTS.md#invariantes-críticas) | Una operación compuesta o sujeta a propiedad, carrera o atomicidad en las fuentes. | Se revisa el criterio derivado. | La comprobación permite detectar incumplimientos de la garantía técnica aplicable, sin prescribir implementación ni colocar espera del usuario u operaciones externas dentro de una transacción. |
| CA07 | [specifications.md → «Registro de eventos»](../../../context/specifications.md#registro-de-eventos), [architecture.md → «Logs»](../../../context/architecture.md#logs) y [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) | Un caso de uso de S4 sujeto a registro de eventos. | Se revisan los criterios y su límite frente a S4.10. | Los criterios aplicables enlazan la regla de registro sin incluir datos prohibidos, y dejan la implementación del registro para S4.10; no asumen que la redacción de criterios aporta evidencia de logging. |
| CA08 | [S4.1 → «Criterios de aceptación»](../s4.1-contratos-aplicacion/spec.md#criterios-de-aceptación), [S4.2 → «Criterios de aceptación»](../s4.2-contratos-repositorios-identidad/spec.md#criterios-de-aceptación) y [S4.3 → «Criterios de aceptación»](../s4.3-proveedor-identidad-desarrollo/spec.md#criterios-de-aceptación) | Los contratos aceptados que utilizarán los futuros casos de uso. | Se contrasta cada criterio con dichos contratos y sus límites de responsabilidad. | Los criterios son verificables con los contratos existentes, no los redefinen, no atribuyen a S4.3 el proveedor concreto y no presentan las pruebas diferidas de consumidores como ya ejecutadas. |
| CA09 | [mvp0-plan.md → «Puerta de entrada de la implementación (criterios de aceptación de S4)»](../../../plans/mvp0-plan.md#puerta-de-entrada-de-la-implementación-criterios-de-aceptación-de-s4) y [«S4: Aplicación: casos de uso (L)»](../../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) | El artefacto de criterios preparado para S4.5. | Se revisan su estado, trazabilidad y referencias. | `plans/mvp0-acceptance-criteria.md` queda claramente pendiente de aprobación humana; cada criterio tiene identificador y enlace a su regla propietaria, y no se presenta S4.5 como aprobada ni se inicia S4.6–S4.10. |

Los casos negativos, los límites y las reglas exactas se remiten a las secciones propietarias enlazadas; esta especificación no los copia ni resuelve ambigüedades funcionales.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguna contradicción bloqueante identificada en las fuentes revisadas. Si durante la redacción del artefacto aparece una regla incompleta o contradictoria, se detendrá el criterio afectado y se solicitará resolución en su documento propietario. | [requirements.md](../../../context/requirements.md), [specifications.md](../../../context/specifications.md), [domain-model.md](../../../context/domain-model.md) | CA01–CA09, según el hallazgo | Resolver la regla en su fuente propietaria antes de someter el criterio afectado a aprobación. |

## Artefactos relacionados

- [Plan técnico](plan.md), propuesta; pendiente de aprobación.

Los criterios de aceptación y las tareas aún no existen; se prepararán en sus fases autorizadas del flujo SDD.