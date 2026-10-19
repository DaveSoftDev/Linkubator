# Plan técnico: S6.3 — Constructor de consultas FTS5

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.
  - Plan: DLG, 20-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- Añadir en Infrastructure un constructor reutilizable dentro del módulo de búsqueda existente. Recibirá texto de búsqueda ya normalizado y devolverá una expresión de consulta FTS5 o la representación de ausencia de consulta.
- Segmentar de acuerdo con las categorías de letras y números Unicode establecidas por la especificación funcional; no se requiere que este componente vuelva a normalizar NFC.
- Escribir cada término como literal citado de FTS5 y unir los términos mediante el AND implícito. El constructor no elige entidad ni proyección, no incorpora filtros SQL y no ejecuta la expresión.
- Las pruebas unitarias del proyecto existente de tests invocarán directamente el contrato público o visible a tests, siguiendo sus convenciones actuales. El componente conserva inalterada la cadena recibida.

### Flujos internos

1. El llamador prepara `q` y aplica el normalizador NFC de S6.2.
2. El constructor segmenta el texto ya normalizado en secuencias Unicode de letras o números; los caracteres restantes funcionan como separadores y no se interpretan como sintaxis FTS5.
3. Si no quedan términos, devuelve el estado sin consulta; de lo contrario genera un término citado por cada secuencia, concatenados para la combinación AND implícita.
4. La consulta de las proyecciones y la decisión de no llamar a FTS5 cuando el resultado está vacío se integran en S6.5. S6.3 se valida sin depender de ese consumidor.

### Persistencia y dependencias

- No modifica esquema, scripts de migración ni datos; S6.1 ya proporciona las proyecciones consumidoras.
- No abre conexiones ni transacciones, y no ejecuta SQL. La selección de proyección, usuario y filtros forma parte del trabajo de consulta.
- Reutiliza la solución .NET y el proyecto de pruebas existentes; no se prevén dependencias de paquetes nuevas.
- S6.2 proporciona el normalizador NFC; S6.5 integra el constructor con consultas y S6.7 proporciona pruebas integradas. Estos últimos entregables no son prerrequisitos de cierre de S6.3.

### Garantías técnicas

- Los operadores aparentes y la puntuación de la entrada no pueden aportar sintaxis u operadores a la expresión: los términos se pasan como texto literal según la arquitectura FTS5.
- No se añade búsqueda por prefijo ni se crea una consulta si la segmentación no produce términos.
- El constructor no altera el valor original ni reinterpreta la normalización Unicode fuera de su responsabilidad.
- La prueba directa permite comprobar segmentación y expresión. No prueba ejecución real de consultas, coincidencias del tokenizador, aislamiento por usuario, sincronización de índices, relevancia ni paginación; estas garantías pertenecen a los trabajos integrados posteriores y no bloquean S6.3.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Ubicar el componente reutilizable en el módulo Search de Infrastructure y no en Domain. | propuesta | [architecture.md → «FTS5»](../../context/architecture.md#fts5), patrón de S6.2 en [plan técnico](../s6.2-normalizacion-nfc-busqueda/plan.md#componentes-y-contratos) | Mantiene la sintaxis de consulta en la infraestructura de búsqueda; validar la ubicación concreta al inspeccionar la estructura antes de implementar. |
| Recibir texto normalizado y no aplicar NFC dentro del constructor. | confirmado | [spec.md → «Alcance»](spec.md#alcance), [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros) | Evita duplicar el paso de S6.2; el llamador de integración garantiza el orden en S6.5. |
| Validar el contrato mediante tests directos sin esperar la integración SQL de S6.5. | confirmado | [spec.md → «Dependencias y prerrequisitos»](spec.md#dependencias-y-prerrequisitos), [mvp0-plan.md → S6.3–S6.7](../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l) | S6.3 puede cerrarse con sus propias evidencias; las pruebas integradas se registran en sus etapas propietarias. |
| Las secuencias Unicode de letras y números conservan los criterios definidos para categorías en las reglas de búsqueda. | confirmado | [specifications.md → «Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros) | Evita segmentación ASCII y que símbolos sean operadores; los casos representativos se cubren con tests. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA05 | Automatizada: tests unitarios del constructor con secuencias Unicode y separadores, más inspección que asegure que la entrada no se muta. | Proyecto `Linkubator.Tests` y constructor de Infrastructure. | `dotnet test .\tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~SearchQueryBuilderTests` | Cada secuencia Unicode válida queda separada de puntuación/separadores conforme a la regla fuente; texto de entrada intacto. | Salida del test, casos ejecutados y SDK usado. |
| V02 | CA02 | Automatizada: tests unitarios con varios términos y términos idénticos a operadores FTS5, verificando el formato literal esperado. | Proyecto `Linkubator.Tests`. | Mismo filtro de V01. | Todos los términos aparecen citados y enlazados por la forma AND implícita, sin operatoria introducida por la entrada. | Salida de tests y ejemplos cubiertos. |
| V03 | CA03 | Automatizada y revisión técnica del resultado generado para términos de uno y varios elementos. | Proyecto `Linkubator.Tests`. | Mismo filtro de V01; revisar la expresión producida en los tests. | No se añade operador de prefijo ni sintaxis que amplíe los resultados; los términos se mantienen completos. | Resultado de tests y revisión del formato. |
| V04 | CA04 | Automatizada: tests unitarios para texto vacío y texto que, tras segmentación, no contiene términos. | Proyecto `Linkubator.Tests`. | Mismo filtro de V01. | El constructor señala ausencia de expresión y no produce consulta FTS5 vacía. | Salida de tests y casos cubiertos. |
| V05 | CA01–CA05 | Revisión técnica/documental de trazabilidad y alcance. | V01–V04. | `git diff --check`; revisión de implementación, tests, spec y plan. | Sin reglas funcionales duplicadas, sin integración SQL fuera de alcance, y todos los criterios tienen comprobación directa. | Resultado del comando y archivos revisados. |

Los tests directos no demuestran que SQLite FTS5 ejecute la expresión con las proyecciones reales ni que filtros de usuario y entidad se apliquen correctamente. Esas garantías requieren la integración de consultas y se mantienen fuera del cierre de esta spec. La sensibilidad de las pruebas de formato se demuestra con aserciones negativas ante operadores no citados, falta de AND implícito o inclusión de un asterisco de prefijo; no se requiere infraestructura de pruebas adicional.

## Orden de ejecución y cierre

- [Tareas](tasks.md#aceptación-y-cierre): aprobadas por DLG el 20-X-2026 («Vamos»); T01–T05 están completadas.
- CA01–CA05 tienen evidencia focalizada satisfactoria. DLG aceptó y cerró S6.3 con «Completado» el 20-X-2026; la evidencia consta en [tasks.md → «Evidencia por criterio»](tasks.md#evidencia-por-criterio).
- La integración con las consultas de S6.5 y la cobertura integrada de S6.7 no formaban parte de las condiciones de cierre de S6.3.
- Ante una contradicción futura con las reglas fuente, detener la tarea afectada y solicitar la decisión en el documento propietario antes de cambiar el contrato.
