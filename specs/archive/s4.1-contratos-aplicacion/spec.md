# Especificación: S4.1 — Result, errores, DTOs y validadores de aplicación

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 15-X-2026 («Aprobado»).

## Objetivo

Definir los contratos compartidos de Application para representar resultados y errores, transportar datos de entrada y salida de los casos de uso MVP0 y validar sus entradas, manteniendo las reglas en sus fuentes propietarias y la identidad fuera de los datos controlados por el cliente.

## Fuentes

- [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l): alcance de S4.1, casos de uso MVP0 y dependencias posteriores.
- [roadmap.md → «Now»](../../../context/roadmap.md#now): estado y trazabilidad de la etapa.
- [architecture.md → «Application»](../../../context/architecture.md#application), [«Casos de uso iniciales»](../../../context/architecture.md#casos-de-uso-iniciales) y [«Arquitectura»](../../../context/architecture.md#arquitectura): responsabilidades, patrón `Result` y límites de capa.
- [requirements.md → «Colecciones»](../../../context/requirements.md#colecciones), [«Enlaces»](../../../context/requirements.md#enlaces), [«Público y privado»](../../../context/requirements.md#público-y-privado), [«Búsqueda y filtros»](../../../context/requirements.md#búsqueda-y-filtros), [«Páginas privadas»](../../../context/requirements.md#páginas-privadas) y [«Páginas públicas»](../../../context/requirements.md#páginas-públicas): capacidades que consumen los contratos.
- [specifications.md → «Textos introducidos por el usuario»](../../../context/specifications.md#textos-introducidos-por-el-usuario), [«Longitudes máximas»](../../../context/specifications.md#longitudes-máximas), [«Generación del alias»](../../../context/specifications.md#generación-del-alias), [«Generación de slugs»](../../../context/specifications.md#generación-de-slugs), [«URLs de los enlaces»](../../../context/specifications.md#urls-de-los-enlaces), [«Duplicados»](../../../context/specifications.md#duplicados), [«Búsqueda y filtros»](../../../context/specifications.md#búsqueda-y-filtros), [«Listados»](../../../context/specifications.md#listados), [«Páginas privadas»](../../../context/specifications.md#páginas-privadas) y [«Errores controlados»](../../../context/specifications.md#errores-controlados): validación y resultados funcionales.
- [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos), [«Público y privado»](../../../context/domain-model.md#público-y-privado), [«Collection»](../../../context/domain-model.md#collection) y [«Link»](../../../context/domain-model.md#link): invariantes que Application no puede eludir.
- [AGENTS.md → «Invariantes críticas»](../../../AGENTS.md#invariantes-críticas): límites obligatorios de identidad, privacidad y datos sensibles.

## Alcance

- Incluye:
  - Contrato común `Result` para comunicar de forma explícita el éxito o el rechazo de una operación de Application.
  - Errores de aplicación que permiten a los consumidores distinguir rechazos de validación, reglas del dominio y recursos no accesibles, sin depender del texto de excepciones.
  - DTOs de entrada y salida necesarios para los casos de uso de colecciones, enlaces, dashboard y lecturas públicas incluidos en S4.
  - Validadores de entrada de Application y la traducción de sus resultados al contrato de error, respetando la responsabilidad de Domain sobre sus invariantes y transformaciones.
  - Contratos aptos para que S4.4 redacte criterios por caso de uso y para que S4.2 los consuma al definir repositorios, Unit of Work e identidad.
- Excluye:
  - Interfaces de repositorios, Unit of Work y abstracción del usuario identificado, que pertenecen a S4.2.
  - Contrato e implementación del proveedor de identidad de desarrollo, que pertenecen a S4.3.
  - Los criterios de aceptación de cada caso de uso, que pertenecen a S4.4, y su aprobación humana en S4.5.
  - Implementar los casos de uso, repositorios concretos, fakes, páginas Razor, persistencia, transacciones o integración HTTP.
  - Añadir o cambiar reglas de producto, mensajes, rutas o invariantes.
- Trabajo vecino y responsable de sus entregables:
  - S4.2 define repositorios, Unit of Work e identidad identificada; S4.3 define el proveedor de identidad de desarrollo.
  - S4.4 define criterios de aceptación trazables a las fuentes funcionales y S4.5 los somete a aprobación humana.
  - S4.6–S4.8 implementan los casos de uso; S4.9 añade los fakes y tests de esos casos; S4.10 registra los eventos permitidos.

## Dependencias y prerrequisitos

- S3 está completada y las entidades e invariantes aplicables están aceptadas.
- La solución contiene `Linkubator.Application`. La implementación de S4.1 se realizó después de la aprobación de sus tareas y quedó aceptada para el alcance documentado en [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre).
- El alcance de casos de uso es el enumerado en S4 del plan MVP0. Las reglas de comportamiento siguen siendo las fuentes de `context/`; S4.1 no las sustituye.
- S4.2 depende de los contratos que se aprueben en este trabajo. S4.4 los necesita para redactar criterios, pero ningún caso de uso se implementará antes de la aprobación de S4.5.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [architecture.md → «Application»](../../../context/architecture.md#application) y [«Casos de uso iniciales»](../../../context/architecture.md#casos-de-uso-iniciales) | Los contratos compartidos de Application definidos por S4.1. | Se revisa su ubicación y las referencias entre proyectos. | Los tipos pertenecen a Application, no incorporan dependencias de Infrastructure o Web y no trasladan reglas de negocio a la presentación. |
| CA02 | [architecture.md → «Application»](../../../context/architecture.md#application) | Una operación de Application que puede completarse o rechazarse. | El consumidor inspecciona el resultado. | Puede distinguir el resultado satisfactorio del fallido mediante el contrato `Result`, sin inferir el estado a partir de mensajes. |
| CA03 | [specifications.md → «Errores controlados»](../../../context/specifications.md#errores-controlados), [«Páginas privadas»](../../../context/specifications.md#páginas-privadas) y [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos) | Una operación que falla por entrada inválida, una regla del dominio o un recurso que no puede exponerse al usuario identificado. | Application expresa el fallo a su consumidor. | El tipo de error permite elegir el tratamiento correspondiente sin incluir datos del recurso ajeno ni depender de mensajes de excepciones. Los casos en que la fuente exige una respuesta común conservan esa indistinguibilidad. |
| CA04 | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) y [architecture.md → «Application»](../../../context/architecture.md#application) | Los casos de uso MVP0 de colecciones, enlaces, dashboard y lecturas públicas enumerados en S4. | Se revisa el catálogo de DTOs producido por S4.1. | Cada caso previsto cuenta con los datos de entrada y salida necesarios en DTOs de Application; esos DTOs no exponen entidades de persistencia ni acoplan los contratos a Razor Pages o HTTP. |
| CA05 | [AGENTS.md → «Invariantes críticas»](../../../AGENTS.md#invariantes-críticas) y [domain-model.md → «Propiedad de los datos»](../../../context/domain-model.md#propiedad-de-los-datos) | Los DTOs de operaciones privadas definidos para los casos MVP0. | Se inspeccionan sus campos y los contratos de uso de la identidad. | Ninguna operación privada acepta del cliente un `UserId` como autoridad; el alias o el slug recibido tampoco basta para conceder acceso a datos privados. |
| CA06 | [architecture.md → «Domain»](../../../context/architecture.md#domain) y [«Application»](../../../context/architecture.md#application), [specifications.md → «Textos introducidos por el usuario»](../../../context/specifications.md#textos-introducidos-por-el-usuario), [«Longitudes máximas»](../../../context/specifications.md#longitudes-máximas), [«URLs de los enlaces»](../../../context/specifications.md#urls-de-los-enlaces) y [«Generación del alias»](../../../context/specifications.md#generación-del-alias) | Una entrada de usuario sujeta a validación de formato, obligatoriedad, longitud o transformación de dominio. | Se ejecuta la validación antes de invocar la operación correspondiente. | Los validadores comprueban las reglas aplicables y permiten asociar los errores de entrada al campo; no duplican ni contradicen las transformaciones e invariantes propietarias de Domain. |
| CA07 | [domain-model.md → «Collection»](../../../context/domain-model.md#collection), [«Link»](../../../context/domain-model.md#link) y [«Público y privado»](../../../context/domain-model.md#público-y-privado), y [specifications.md → «Duplicados»](../../../context/specifications.md#duplicados) | Una entrada que infringe una invariante de entidad, propiedad, privacidad o unicidad funcional. | Se valida o se intenta ejecutar desde Application. | El contrato permite comunicar el rechazo en lugar de un resultado satisfactorio; la regla sigue protegida por Domain y, donde corresponda, por la operación de aplicación. |
| CA08 | [AGENTS.md → «Invariantes críticas»](../../../AGENTS.md#invariantes-críticas) y [specifications.md → «Registro de eventos»](../../../context/specifications.md#registro-de-eventos) | Datos de entrada, errores de validación y resultados de aplicación relacionados con credenciales, tokens, consultas o recursos privados. | Se revisa qué información incluyen los contratos de error. | Los errores no requieren ni exponen contraseñas, tokens, texto de búsqueda ni datos privados de otros usuarios. |

Los casos negativos y límites de cada operación se concretarán en los criterios S4.4 a partir de sus fuentes propietarias; esta especificación no vuelve a enumerarlos ni los convierte en nuevas reglas.

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Las fuentes no fijan nombres, forma genérica ni granularidad de los errores de aplicación y DTOs. | [architecture.md → «Application»](../../../context/architecture.md#application) y [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) | Ninguno: el plan técnico puede proponer contratos concretos sin cambiar reglas funcionales. | Revisar y aprobar el diseño técnico en `plan.md` antes de crear `tasks.md`. |

## Artefactos relacionados

- [Plan técnico](plan.md), aprobado por DLG el 15-X-2026 («Comprendido»).
- [Tareas](tasks.md), aprobadas por DLG el 15-X-2026 («Vamos»); implementación aceptada por DLG el 15-X-2026 al solicitar «Completada».