# Plan técnico: S3.5 — Fichas de entidades

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 12-X-2026 («Comprendido»).
	- Plan: aprobado por DLG el 12-X-2026 («Adelante»).

## Diseño de implementación

### Componentes y contratos

El trabajo produce tres fichas Markdown en la carpeta de esta spec. Son vistas derivadas y de revisión, no documentos fuente. Cada una reúne las referencias de las fuentes que afectan a la entidad, siguiendo [requirements-review → «Entity sheet»](../../skills/requirements-review/SKILL.md#entity-sheet), y cierra con las contradicciones, duplicados u omisiones detectados o indica explícitamente que no se detectaron.

| Componente | Ubicación | Responsabilidad | Criterios |
| --- | --- | --- | --- |
| Ficha de `User` | `user.md` | Sintetizar las reglas de fuente que afectan a `User`, con referencias al documento y línea correspondientes. | CA01–CA03 |
| Ficha de `Collection` | `collection.md` | Sintetizar las reglas de fuente que afectan a `Collection`, con referencias al documento y línea correspondientes. | CA01–CA03 |
| Ficha de `Link` | `link.md` | Sintetizar las reglas de fuente que afectan a `Link`, con referencias al documento y línea correspondientes. | CA01–CA03 |

Las ubicaciones propuestas siguen la adaptación de entrega aprobada en [spec.md → «Alcance»](spec.md#alcance). Las fichas enlazan sus fuentes; no copian tablas, reglas completas ni valores como especificaciones independientes. Se limitan a `User`, `Collection` y `Link`, alcance de S3 definido en [mvp0-plan.md → «S3: Dominio III: entidades e invariantes (M)»](../../plans/mvp0-plan.md#s3-dominio-iii-entidades-e-invariantes-m).

### Flujos internos

1. Consultar las fuentes de verdad completas y localizar en ellas todas las reglas que afectan a cada entidad, incluidas menciones funcionales y pruebas de checklist, según el procedimiento de la skill.
2. Consultar la implementación de las tres entidades y los criterios aprobados S3.1–S3.4 para obtener contexto de revisión, sin tratarlos como sustituto de las fuentes propietarias ni como evidencia de aprobación de las invariantes por S3.6.
3. Redactar y guardar cada ficha en su archivo propuesto, agrupando el material por los temas que define el procedimiento de la skill, usando enlaces `archivo#Lnn` a sus fuentes.
4. Comprobar que los enlaces y líneas citados resuelven, que no se introdujeron reglas nuevas ni contradicciones como decisiones y que las tres fichas contienen su apartado final de observaciones.
5. Si aparece una contradicción, duplicado, omisión o regla que requiera decisión, registrarla como hallazgo con las fuentes implicadas y no editar las fuentes ni el código dentro de S3.5.

La revisión de las fichas contra el comportamiento real del código y la aprobación de las invariantes son responsabilidad de S3.6, según [mvp0-plan.md → «S3: Dominio III: entidades e invariantes (M)»](../../plans/mvp0-plan.md#s3-dominio-iii-entidades-e-invariantes-m).

### Persistencia y dependencias

- No hay persistencia de producto, cambios de código, paquetes ni dependencias técnicas.
- La salida documental permanece dentro de `specs/s3.5-fichas-entidades/` y depende de las fuentes de verdad, la implementación actual de Domain y la skill `requirements-review`.
- S3.1–S3.4 están implementadas y aceptadas; se consultan para preparar la revisión, sin reabrir sus cierres.

### Garantías técnicas

- Las fuentes propietarias permanecen como única fuente de reglas conforme a [AGENTS.md → «Dónde vive cada cosa»](../../AGENTS.md#dónde-vive-cada-cosa).
- Cada afirmación que represente una regla lleva referencia a su fuente y ubicación verificable; las fichas no alteran los documentos citados.
- La existencia de fichas no constituye revisión ni aceptación humana del modelo; esa decisión queda en S3.6.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| D01. Conservar las fichas como archivos Markdown dentro de la carpeta S3.5. | Confirmado | [spec.md → «Alcance»](spec.md#alcance), aprobada por DLG el 12-X-2026 («Comprendido»). | Permite revisión posterior y conserva un registro físico derivado; mantener los enlaces a las fuentes para evitar que se interpreten como fuente de reglas. |
| D02. Usar `user.md`, `collection.md` y `link.md` como nombres de salida. | Propuesta | [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación) y [mvp0-plan.md → «S3: Dominio III: entidades e invariantes (M)»](../../plans/mvp0-plan.md#s3-dominio-iii-entidades-e-invariantes-m). | Facilita identificar cada ficha; puede ajustarse antes de implementar el plan, sin alterar el alcance. |
| R01. Las fuentes funcionales pueden cambiar después de crear las fichas. | Confirmado | [AGENTS.md → «Dónde vive cada cosa»](../../AGENTS.md#dónde-vive-cada-cosa). | Las fichas podrían quedar desactualizadas; S3.6 debe comprobar las referencias y tratarlas como instantánea derivada, no como autoridad. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Revisión documental de cobertura por entidad | Fuentes de verdad, implementación de Domain y cierres S3.1–S3.4 disponibles | Aplicar [requirements-review → «Entity sheet»](../../skills/requirements-review/SKILL.md#entity-sheet) a `User`, `Collection` y `Link`; contrastar las fuentes citadas y el material de checklist aplicable | Existen las tres fichas y cada una cubre las reglas encontradas que afectan a su entidad, con referencias `archivo#Lnn` | Fichas y comprobación de las referencias de fuente por entidad |
| V02 | CA02 | Revisión técnica de ubicación y fuente única | V01 | Revisar el árbol de `specs/s3.5-fichas-entidades/` y comparar las afirmaciones de las fichas con los documentos propietarios | Las tres fichas están en la carpeta prevista; no hay reglas nuevas, cambios en fuentes ni duplicación de valores como autoridad | Lista de archivos y revisión del diff; confirmación de que los documentos fuente no se modificaron |
| V03 | CA03 | Revisión editorial y de referencias | V01–V02 | Comprobar enlaces Markdown y números de línea citados, y leer el apartado de observaciones finales de cada ficha | Las referencias resuelven y cada ficha indica los hallazgos detectados o que no se detectaron | Resultado por archivo de la revisión de enlaces y observaciones |

Estas comprobaciones validan la cobertura documental y sus referencias; no demuestran que el código cumpla las invariantes ni que el modelo esté aprobado. No se requiere prueba automatizada de código para este trabajo documental. La comparación de comportamiento y la aceptación humana pertenecen a S3.6.

## Orden de ejecución y cierre

- Crear `tasks.md` solo después de la aprobación explícita de este plan.
- Con las tareas aprobadas, ejecutar la búsqueda por entidad, redactar las fichas y validar cada referencia antes de presentar la entrega.
- Cerrar S3.5 solo con evidencia de todos sus criterios y aceptación humana registrada en `tasks.md`; la aprobación del modelo sigue siendo S3.6.
- Ante una contradicción o regla nueva, detener el criterio afectado y tramitar el cambio en su documento propietario antes de actualizar las fichas.