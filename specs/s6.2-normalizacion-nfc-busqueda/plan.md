# Plan técnico: S6.2 — Normalización NFC de búsqueda

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.
  - Plan: DLG, 19-X-2026 («Aprobar»), respuesta explícita al formulario de aprobación del plan.

## Diseño de implementación

### Componentes y contratos

- Infrastructure añadirá un normalizador reutilizable para texto de búsqueda, dentro de un módulo de búsqueda propio. Su implementación usará la normalización Unicode Form C disponible en .NET, sin dependencias nuevas.
- El contrato de normalización devolverá el texto en NFC y no modificará la instancia ni las propiedades de las entidades. La ubicación del componente en Infrastructure mantiene la transformación acotada a la búsqueda FTS5 y evita introducir conocimiento de persistencia en Domain.
- El normalizador ofrece el componente que consumirán las etapas posteriores al procesar `q` y al preparar campos para las proyecciones. Su uso desde el constructor de consulta y desde las escrituras se implementa en S6.3 y S6.4, respectivamente; no se anticipa en esta unidad.
- Las entradas ya normalizadas deben poder procesarse repetidamente sin cambios observables. El tratamiento de valores nulos se mantendrá compatible con los campos opcionales y el contrato existente de cada llamador; esta unidad no asignará valores por defecto.

### Flujos internos

1. El llamador invoca el normalizador con una cadena de entrada.
2. El componente devuelve una representación en NFC sin mutar la cadena original.
3. Los llamadores productivos que procesan `q` o campos de proyección se implementan en S6.3/S6.4; no forman parte del flujo de S6.2.
4. Las pruebas llaman directamente al componente y cubren cadenas en NFC, secuencias descompuestas, texto mixto, entradas vacías o nulas y normalización repetida; comprueban que el valor de entrada no cambie.

### Persistencia y dependencias

- No se modifica el esquema ni se añaden scripts SQL: S6.1 ya proporciona las proyecciones base.
- No hay transacciones en esta operación de texto; no se realizan escrituras en la base de datos desde el normalizador.
- Se reutiliza la plataforma .NET existente y el proyecto de pruebas `Linkubator.Tests`; no se añaden paquetes.
- S6.3 y S6.4 integrarán el componente con la consulta y las escrituras cuando se implementen. Esa integración futura no es un prerrequisito para validar ni cerrar el componente de S6.2.

### Garantías técnicas

- Para entradas de cadena, el componente devuelve el texto normalizado en NFC; para `null`, conserva `null`.
- La normalización se limita a la representación de búsqueda; no se persisten cambios en los valores originales de `Link` o `Collection`.
- La equivalencia de acentos y la tokenización siguen a cargo del tokenizador FTS5 y de las etapas posteriores; normalizar a NFC no redefine esas reglas.
- Esta spec valida el componente directamente. La integración productiva con consulta e indexación y sus pruebas conjuntas pertenecen a S6.3, S6.4 y S6.7, y no bloquean el cierre de S6.2.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Mantener un normalizador de texto reutilizable en Infrastructure y sin dependencias nuevas. | confirmado | [architecture.md → «FTS5»](../../context/architecture.md#fts5), implementación comprobada en `Search/SearchTextNormalizer.cs` | Componente común de búsqueda para etapas posteriores. |
| Limitar S6.2 al normalizador y sus tests unitarios; no integrar consulta ni escrituras en esta unidad. | confirmado | [mvp0-plan.md → S6.2–S6.7](../../plans/mvp0-plan.md#s6-persistencia-ii-fts5-l), [spec.md → «Alcance»](spec.md#alcance) | La integración se cubre en S6.3/S6.4 y sus tests en S6.7; no bloquea este cierre. |
| Preservar `null` sin producir una cadena por defecto. | confirmado | test `Normalize_WhenTextIsNull_ReturnsNull` y validación focalizada | Mantiene la semántica de valores opcionales. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA04 | Automatizada: test unitario llama la API pública del normalizador con Unicode descompuesto y verifica salida NFC y preservación de la entrada. | Proyecto `Linkubator.Tests` y SDK declarado por la solución. | `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --filter FullyQualifiedName~SearchTextNormalizerTests` | La salida es equivalente a la forma precompuesta y está en NFC; la entrada original permanece intacta. | Salida del test y número de casos ejecutados. |
| V02 | CA02 | Automatizada: tests unitarios con entradas NFC/precompuestas, ASCII, vacía y normalización repetida. | Proyecto `Linkubator.Tests`. | Mismo filtro de V01. | Conserva contenido equivalente y NFC; repetir no cambia la salida ni produce error. | Salida y casos cubiertos. |
| V03 | CA03 | Automatizada: test unitario con entrada `null`. | Proyecto `Linkubator.Tests`. | Mismo filtro de V01. | Retorna `null` sin error ni texto por defecto. | Resultado del test. |
| V04 | CA01–CA04 | Revisión técnica/documental del cambio y de su trazabilidad. | V01–V03. | `git diff --check`; revisión de `spec.md`, `plan.md` y pruebas. | No hay alteraciones de reglas propietarias, dependencias posteriores para el cierre ni cambios a valores de entidades. | Resultado del comando, documentos y archivos revisados. |

Las pruebas llaman directamente a la función productiva y verifican su comportamiento. No demuestran integración con consultas, escrituras, tokenización FTS5 o SQL; esas validaciones pertenecen a S6.3, S6.4 y S6.7, y no son prerrequisito del cierre de S6.2.

## Orden de ejecución y cierre

- [Tareas](tasks.md) aprobadas por DLG el 19-X-2026 («Aprobar»). La implementación y los seis tests focalizados están comprobados; su evidencia está en las tareas.
- CA01–CA04 cuentan con evidencia satisfactoria y T01–T05 están completadas. DLG aceptó y cerró S6.2 con «Completado», 20-X-2026.
- El cierre no depende de S6.3/S6.4/S6.7. S6.3 no se inicia ni se archiva automáticamente.
- Ante discrepancias sobre valores originales o forma Unicode, detener la tarea afectada y tramitar cualquier cambio funcional en su documento propietario.
