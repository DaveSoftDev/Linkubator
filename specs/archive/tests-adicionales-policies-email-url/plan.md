# Plan técnico: Tests adicionales de `EmailPolicy`, `ImageUrlPolicy` y `LinkUrlPolicy`

## Estado y aprobación del plan

- Estado: aprobado por DLG el 12-X-2026.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 12-X-2026 («Adelante»).
	- Plan: aprobado por DLG el 12-X-2026 («Adelante»).

## Diseño de implementación

### Componentes y contratos

| Componente | Ubicación | Responsabilidad | Criterios |
| --- | --- | --- | --- |
| Tests de `EmailPolicy` | `tests/Linkubator.Tests/Domain/Policies/EmailPolicyTests.cs` | Ampliar los tests existentes para las formas admitidas de email, límites y tipos de rechazo diferenciados. | CA01–CA03 |
| Tests de `ImageUrlPolicy` | `tests/Linkubator.Tests/Domain/Policies/ImageUrlPolicyTests.cs` | Ampliar los tests existentes con el máximo exacto y el recorte de espacios exteriores. | CA04–CA05 |
| Tests de `LinkUrlPolicy` | `tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs` | Ampliar los tests de ajuste de URL ya agrupados en `UrlPoliciesTests`, con el máximo exacto y el recorte de espacios exteriores. | CA06–CA07 |

No se crean clases, utilidades ni paquetes. No se modifica `src/Linkubator.Domain` ni el comportamiento de las policies.

### Flujos internos

1. **Email.** Añadir casos para las alternativas de `quoted-string` y `domain-literal` que admite la fuente. Construir emails ASCII válidos en el máximo y uno por encima; usar aserciones de tipo de excepción para distinguir entrada ausente, longitud y sintaxis.
2. **Imagen.** Construir URL absolutas `https` con ruta ASCII cuya longitud ajustada sea el máximo y lo supere en un carácter. Al ser absolutas `https`, la entrada ya coincide con la forma ajustada y la prueba aísla el límite. Añadir un caso absoluto rodeado de espacios y comparar el resultado exacto.
3. **URL de enlace.** Aplicar el mismo enfoque de URL absoluta `https` para el límite, y comprobar por separado el recorte de espacios exteriores sin alterar el resto de la URL.

### Persistencia y dependencias

- No hay persistencia, integraciones ni cambios de dependencias.
- Se reutilizan xUnit, las suites existentes y las policies actuales.
- La longitud de los valores de frontera se construye con caracteres ASCII para que la longitud de la entrada y la unidad de medida de la fuente coincidan sin casos de parejas sustitutas.

### Garantías técnicas

- Las aserciones usan resultados y excepciones esperados derivados de la fuente, no del resultado producido por el código bajo prueba.
- Los cambios se limitan a tests; no se altera el alcance de S3.1 ni se reabren los cierres aceptados de S2.1, S2.2 o S2.5.
- Los ejemplos funcionales de URL ya cubiertos no se duplican.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| D01. Mantener los tests de `LinkUrlPolicy` en `UrlPoliciesTests.cs`, donde ya se encuentran, en lugar de separar o renombrar la suite. | Confirmado | Aprobación de DLG, 12-X-2026 («Adelante»); CA06–CA07 | No añade archivos ni cambia los filtros existentes. |
| D02. Usar URL absolutas `https` en los casos de frontera de longitud, de modo que la representación ajustada tenga la misma longitud que la entrada. | Confirmado | Aprobación de DLG, 12-X-2026 («Adelante»); [spec.md → CA04 y CA06](spec.md#criterios-de-aceptación) | Aísla el límite y evita introducir cambios de producción. Los tests no pretenden volver a probar los ejemplos de prefijo ni la resolución de referencias relativas. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01–CA03 | Automatizada: tests de email | SDK y dependencias existentes | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~EmailPolicyTests" --verbosity minimal` | Pasan las formas admitidas, los dos lados del límite y las excepciones diferenciadas. | Comando, conteo y código de salida. |
| V02 | CA04–CA05 | Automatizada: tests de imagen | V01 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~ImageUrlPolicyTests" --verbosity minimal` | Se acepta el límite exacto, se rechaza el exceso y el recorte produce el resultado esperado. | Comando, conteo y código de salida. |
| V03 | CA06–CA07 | Automatizada: tests de URL de enlace | V01–V02 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --no-restore --filter "FullyQualifiedName~UrlPoliciesTests" --verbosity minimal` | Se acepta el límite exacto, se rechaza el exceso y el recorte produce el resultado esperado; los tests preexistentes de host y esquema también pasan. | Comando, conteo y código de salida. |
| V04 | CA01–CA08 | Automatizada integrada | V01–V03 | `dotnet test Linkubator.sln --no-restore --verbosity minimal` | La suite completa pasa y no se modificó código de producción. | Comando, conteo, código de salida y revisión del diff de `src/Linkubator.Domain`. |

V01–V03 comprueban los casos especificados, pero no prueban todas las formas posibles de la gramática de email ni sustituyen las suites de ejemplos y validación de URL ya existentes. Las pruebas de frontera usan URL cuya forma de entrada ya es la forma ajustada.

## Orden de ejecución y cierre

- Tareas y evidencia de cierre: [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre), aprobadas por DLG el 12-X-2026 («Adelante») y aceptadas el 13-X-2026 («Aceptado»).
- Los criterios CA01–CA07 deben tener evidencia satisfactoria y CA08 debe pasar la suite integrada antes de presentar el resultado a DLG.
- Ante una discrepancia entre la fuente y el comportamiento de producción, detener la tarea afectada; no cambiar producción ni las reglas sin aprobación expresa.