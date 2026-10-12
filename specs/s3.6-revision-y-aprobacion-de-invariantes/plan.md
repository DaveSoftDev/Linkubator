# Plan técnico: S3.6 — Revisión y aprobación de invariantes

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 12-X-2026 («Validado»).
	- Plan: aprobado por DLG el 12-X-2026 («Adelante»).

## Diseño de implementación

### Componentes y contratos

El trabajo contrasta las fichas derivadas con las fuentes propietarias, las entidades de Domain y sus tests, y produce una matriz de revisión y un registro de la decisión humana. No cambia código, tests, fichas ni reglas funcionales.

| Componente | Responsabilidad | Criterios |
| --- | --- | --- |
| Matriz de revisión de `User`, `Collection` y `Link` | Relacionar cada invariante aplicable con su fuente, implementación y tests; distinguir reglas fuera del alcance S3. | CA01–CA03 |
| Verificación del contrato de `Collection.ChangePrivacy` | Confirmar que Domain acepta exactamente sus enlaces asociados y rechaza omisiones o duplicados antes de mutar. La carga desde persistencia y la coordinación transaccional quedan para aplicación e infraestructura. | CA03–CA04 |
| Registro de decisión humana | Guardar la decisión de DLG con fecha y referencia real, separada de las evidencias técnicas. | CA05 |

### Flujos internos

1. Confirmar que las fichas S3.5 y las fuentes, entidades y tests de S3.1–S3.4 están disponibles y enlazados.
2. Construir la matriz de invariantes por entidad usando los documentos fuente y los criterios aprobados; clasificar cada afirmación de las fichas como consistente, discrepante, no cubierta o fuera del alcance de S3.
3. Inspeccionar el código y los tests de propiedad, creación, movimiento y privacidad. Para `Collection.ChangePrivacy`, comprobar que el contrato Domain exige el conjunto completo conocido y rechaza omisiones y duplicados antes de mutar.
4. Registrar como límite de etapa que S3 no incluye un consumidor de Application/Web. La carga completa y la transacción deben validarse al implementar el caso de uso correspondiente, según la arquitectura; su ausencia no bloquea la aceptación de Domain.
5. Presentar matriz y hallazgos a DLG. Registrar aprobación, rechazo o petición de cambios con referencia verificable. Un hallazgo que requiera modificar una regla se devuelve a su documento propietario y bloquea la aceptación afectada.

### Persistencia y dependencias

- No hay persistencia de producto, dependencias técnicas ni cambios de código.
- Depende de los documentos y del código fuente ya existentes en S3.1–S3.5.
- La verificación de carga/persistencia del agregado se reserva al trabajo futuro de aplicación e infraestructura; no forma parte de la aceptación de Domain en S3.6.

### Garantías técnicas

- La matriz referencia las fuentes propietarias y no convierte fichas o criterios derivados en reglas nuevas.
- Los tests históricos se distinguen de ejecuciones realizadas durante esta revisión; solo una validación observada se registra como evidencia actual.
- La garantía de Domain se comprueba validando que `Collection.ChangePrivacy` compara el conjunto recibido con los enlaces asociados al agregado y rechaza cualquier diferencia antes de mutar. La recuperación del agregado completo desde almacenamiento se valida en la etapa de persistencia/aplicación.
- La aceptación técnica no sustituye la decisión humana de DLG ni la aprobación de reglas fuente.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| D01. `Collection.ChangePrivacy` valida en Domain el conjunto completo recibido; los consumidores futuros deben cargar el agregado completo antes de invocarlo. | Confirmado | CA03 de esta spec y límite de capas de [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura). | S3.6 valida el contrato Domain; el caso de uso y su transacción se validan en la etapa propietaria posterior. |
| R01. En S3 no existe un consumidor de Application/Web. | Confirmado, fuera del alcance de S3.6 | [mvp0-plan.md → «S4: Aplicación: casos de uso (L)»](../../plans/mvp0-plan.md#s4-aplicación-casos-de-uso-l). | No bloquea la aceptación de Domain; dejar trazabilidad para validar el caso de uso en S4/S5. |
| R02. Las fichas S3.5 no contienen referencias de línea conforme al criterio que las generó. | Pendiente de verificación en este plan | [S3.5 → «Criterios de aceptación»](../s3.5-fichas-entidades/spec.md#criterios-de-aceptación), CA01. | Registrar referencias insuficientes en la matriz; corregir las fichas solo mediante autorización y trabajo documental correspondiente. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Revisión documental/técnica de invariantes por entidad | Fuentes, fichas S3.5, código y tests disponibles | Trazar cada invariante S3 de `User`, `Collection` y `Link` a fuente, código y test; marcar aparte lo fuera de alcance | Matriz completa, discrepancias y huecos identificados; sin reglas nuevas | Matriz con rutas, secciones y referencias precisas a código/tests |
| V02 | CA02 | Revisión técnica de propiedad | Criterios S3.2 y sus tests | Revisar creación y movimiento de enlaces, rechazos por propietario ajeno y estado después del rechazo | Implementación y cobertura se describen de acuerdo con lo que realmente verifican los tests | Identificadores de tests y resultado de ejecución, si se ejecutan |
| V03 | CA03 | Revisión técnica de privacidad y cobertura | Criterios S3.3–S3.4 | Revisar creación, cambios de privacidad, movimiento, pruebas negativas y rechazo sin mutación; distinguir el contrato de la exhaustividad del argumento | Hallazgos explícitos; ninguna inferencia de que una secuencia parcial representa todos los enlaces | Tests inspeccionados/ejecutados y observación técnica trazable |
| V04 | CA03, CA04 | Revisión del contrato Domain y de la frontera de etapa | Entidad `Collection`, `Link` y tests de Domain | Verificar referencias y tests de transiciones, conjunto omitido y duplicado; comprobar que la búsqueda de usos distingue la implementación Domain de consumidores de capas posteriores | Domain rechaza conjuntos que no coinciden exactamente con los enlaces asociados antes de mutar; la falta de consumidor de Application/Web se clasifica fuera del alcance | Tests de regresión y resultado; lista de referencias encontrada y límite de etapa |
| V05 | CA01–CA05 | Revisión y decisión humana | Matriz y evidencias V01–V04 | Presentar a DLG y registrar respuesta en `tasks.md` una vez aprobadas las tareas | Decisión explícita con fecha y referencia; los puntos no verificados no se informan como aprobados | Registro de aprobación, rechazo o cambios solicitados |

La verificación V04 demuestra el contrato y los límites de Domain. La consulta de persistencia y la transacción del caso de uso quedan para la etapa que implemente esos componentes.

## Orden de ejecución y cierre

- [Tareas](tasks.md), aprobadas por DLG el 12-X-2026 («Vamos»), ejecutadas y validadas con 553 pruebas correctas y compilación sin advertencias.
- La comprobación de `Collection.ChangePrivacy` localizó que no existe consumidor de producción. El contrato de Domain ahora compara exactamente el conjunto recibido con las relaciones registradas y rechaza elementos omitidos o duplicados antes de mutar.
- DLG aprobó el alcance y el cierre el 12-X-2026 («Asumido»); CA03 se satisface con evidencia del contrato de Domain y no requiere un consumidor Application/Web en S3.6.
- El alcance de la evidencia técnica y la aceptación humana constan en `tasks.md`.
- Ante una discrepancia que requiera cambiar reglas o código, detener el criterio afectado y tramitar un trabajo autorizado en el documento propietario.