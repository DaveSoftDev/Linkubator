# Plan técnico: S4.4 — Criterios de aceptación de los casos de uso

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 15-X-2026 («Revisada»).
	- Plan: aprobado por DLG el 15-X-2026 («Comprendido»).

## Diseño de implementación

### Componentes y contratos

El entregable es `plans/mvp0-acceptance-criteria.md`, según la puerta de S4 definida en [mvp0-plan.md → «Puerta de entrada de la implementación (criterios de aceptación de S4)»](../../plans/mvp0-plan.md#puerta-de-entrada-de-la-implementación-criterios-de-aceptación-de-s4). Será un artefacto derivado y trazable, no una fuente paralela de reglas.

| Componente | Responsabilidad | Criterios |
| --- | --- | --- |
| Catálogo de criterios | Organizar los casos por operaciones y escenarios de S4, con identificadores estables, formato GIVEN/WHEN/THEN y enlace directo a sus fuentes propietarias. | CA01–CA05, CA08–CA09 |
| Cobertura transversal | Hacer trazables propiedad, público/privado, atomicidad, lecturas públicas, listados y registro a las fuentes que los definen, manteniendo separadas las responsabilidades de S4.6–S4.10. | CA02–CA08 |
| Puerta S4.5 | Presentar el catálogo como pendiente de revisión/aprobación y dejar claramente separada esa decisión de la aprobación de esta spec y de este plan. | CA09 |

El alcance de operaciones se deriva del catálogo S4 y de la matriz aceptada de [S4.2 → «Matriz de operaciones MVP0 → contratos»](../s4.2-contratos-repositorios-identidad/tasks.md#matriz-de-operaciones-mvp0--contratos). Los contratos aceptados de [S4.1](../s4.1-contratos-aplicacion/spec.md) y [S4.2](../s4.2-contratos-repositorios-identidad/spec.md) orientan la comprobabilidad; no sustituyen las reglas propietarias enlazadas por la spec.

### Flujos internos

1. Confirmar las reglas fuente vigentes para cada operación, junto con la cobertura contractual ya existente, sin inferir comportamientos no escritos.
2. Inventariar operaciones y lecturas incluidas en S4; organizar los escenarios exitosos, de rechazo y límite que se deriven de sus fuentes.
3. Redactar cada criterio con identificador estable y una condición observable; enlazar la sección propietaria concreta y, cuando ayude a verificar el contexto, las invariantes de dominio pertinentes.
4. Comprobar que no se duplican reglas, mensajes, valores o decisiones y que cada operación prevista tiene cobertura; registrar como bloqueo cualquier vacío o contradicción de fuente, sin resolverlo en este artefacto.
5. Revisar los límites entre Application, el proveedor de identidad, Infrastructure y Web; no convertir criterios de aplicación en criterios de integración HTTP o visual de etapas posteriores.
6. Marcar el documento de criterios como pendiente de aprobación humana y entregarlo a DLG para S4.5. No iniciar S4.6–S4.10.

### Persistencia y dependencias

- El trabajo es documental: no introduce persistencia, dependencias, paquetes ni cambios de código.
- Prerrequisitos disponibles: S4.1, S4.2 y S4.3 constan como aceptadas en [roadmap.md → «Now»](../../context/roadmap.md#now); sus contratos y la matriz de operaciones son contexto técnico, no prueba de comportamiento de consumidores.
- Las reglas exactas se consultan en `context/`; si se encuentra una ambigüedad que afecte un criterio, se bloquea ese criterio hasta resolverla en su documento propietario.
- `plans/mvp0-acceptance-criteria.md` se crea como salida de la ejecución documental después de aprobar este plan. S4.5 es una aprobación separada y posterior.

### Garantías técnicas

- Cada criterio es rastreable hasta una regla propietaria aprobada y comprobable; no añade reglas de negocio ni compite con `context/` como fuente.
- Las operaciones privadas se describen bajo la identidad fiable y las garantías de propiedad establecidas en [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos) y las invariantes críticas de [AGENTS.md](../../AGENTS.md#invariantes-críticas).
- Las lecturas públicas y los casos privados conservan su separación conforme a [architecture.md → «Casos de uso iniciales»](../../context/architecture.md#casos-de-uso-iniciales) y [domain-model.md → «Público y privado»](../../context/domain-model.md#público-y-privado).
- Las referencias a atomicidad, transacciones y eventos solo expresan controles trazados a sus fuentes técnicas y funcionales; no especifican una implementación nueva.
- El documento mantiene la puerta S4.5 pendiente hasta que exista aprobación humana explícita del propio catálogo de criterios.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| D01. Usar `plans/mvp0-acceptance-criteria.md` como catálogo común de criterios de todos los casos de uso S4 del MVP0. | Confirmado | [mvp0-plan.md → «Puerta de entrada de la implementación (criterios de aceptación de S4)»](../../plans/mvp0-plan.md#puerta-de-entrada-de-la-implementación-criterios-de-aceptación-de-s4). | Respeta el entregable ya previsto; su contenido se prepara al ejecutar las tareas aprobadas y se somete a DLG mediante S4.5. |
| D02. Trazar cada criterio a sus fuentes propietarias sin copiar reglas ni valores. | Confirmado | [AGENTS.md → «Dónde vive cada cosa»](../../AGENTS.md#dónde-vive-cada-cosa) y [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación). | Reduce el riesgo de deriva documental; si una regla no es verificable, el criterio afectado se bloquea. |
| R01. El inventario de operaciones de S4 y el detalle funcional podrían no coincidir en granularidad. | Pendiente de comprobación durante la ejecución | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l) y matriz S4.2 enlazada arriba. | Contrastar cobertura por operación sin ampliar el alcance; registrar preguntas reales si la diferencia impide determinar el comportamiento esperado. |
| R02. Las pruebas de consumidores y la integración con Web/Infrastructure son posteriores a la aprobación S4.5. | Confirmado | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l). | No presentar la redacción de criterios ni las pruebas contractuales como evidencia de implementación; detener cualquier trabajo de S4.6–S4.10 antes de S4.5. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA02, CA08 | Revisión documental de inventario y trazabilidad | Spec aprobada; S4.1–S4.3 aceptadas; fuentes funcionales consultables | Comparar cada operación S4 y lectura del catálogo del plan con una fila de criterio y su enlace a la fuente propietaria | Todas las operaciones del alcance tienen criterios vinculados a fuentes vigentes; no se añaden operaciones ajenas | Matriz operación → identificadores de criterio → fuentes |
| V02 | CA03, CA04, CA05 | Revisión funcional de casos exitosos y negativos | Inventario V01 | Recorrer reglas aplicables de `specifications.md` y `domain-model.md` para listados, propiedad, transiciones, rechazos y límites | Los criterios incluyen comprobaciones observables de los casos aplicables, sin inventar comportamiento, mensajes ni resultados | Revisión por criterio, fuentes y escenarios cubiertos; preguntas bloqueantes explícitas |
| V03 | CA02, CA04, CA06 | Revisión de privacidad, identidad y límites técnicos | Inventario V01 y reglas de dominio/arquitectura | Inspeccionar los criterios de operaciones privadas, lecturas públicas y operaciones atómicas frente a las fuentes enlazadas | Ningún criterio permite identidad del cliente ni acceso cruzado; la separación pública/privada y los límites de transacción son verificables sin imponer una implementación | Lista de comprobaciones de seguridad/técnicas con referencias de origen |
| V04 | CA07 | Revisión de frontera de logging | Fuentes de registro de eventos y catálogo S4 | Comparar las referencias a registro con S4.10 y la regla propietaria; verificar que no se copian secretos ni datos prohibidos | El documento deja la implementación a S4.10 y no convierte los criterios en evidencia de logging | Referencia a la sección propietaria y nota de responsabilidad S4.10 |
| V05 | CA09 | Revisión de aprobación y estados | Documento de criterios preparado | Verificar que indica pendiente de aprobación de DLG, que no registra S4.5 como aprobada y que no se inicia implementación | La puerta humana queda inequívocamente pendiente | Estado del artefacto, revisión final y referencia de entrega a DLG cuando exista |
| V06 | CA01–CA09 | Revisión integrada de enlaces, identificadores y fuente única | V01–V05 | Comprobar enlaces/anclas, unicidad de identificadores, cobertura de todos los criterios y ausencia de reglas duplicadas | Referencias funcionales, identificadores y cadena de trazabilidad coherentes; bloqueos no ocultos | Informe de validación documental y lista final de enlaces/IDs |

Las revisiones documentales validan cobertura y trazabilidad, no que los casos de uso estén implementados o funcionen. No se ejecutarán pruebas de código, integración HTTP ni UI en este trabajo. Cualquier fallo de fuente o ambigüedad se registrará como bloqueo y se resolverá antes de someter el criterio afectado a aprobación.

## Orden de ejecución y cierre

- Crear `tasks.md` solo tras la aprobación explícita de este plan. Las tareas descompondrán el inventario, la redacción por grupos de operaciones, las revisiones y la presentación a DLG, con dependencias y checks inmediatos.
- Tras aprobar las tareas, preparar `plans/mvp0-acceptance-criteria.md` y validar V01–V06 sin comenzar la implementación de casos de uso.
- Presentar el catálogo a DLG para la aprobación independiente S4.5; registrar la referencia real en el documento correspondiente solo tras recibirla.
- S4.6–S4.10 permanecen bloqueadas hasta aprobación explícita de S4.5.
