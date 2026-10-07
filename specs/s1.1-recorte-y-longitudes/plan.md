# Plan técnico: S1.1 — Recorte y validación de longitudes

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md) aprobada por DLG, 07-X-2026.
	- Plan: aprobado por DLG, 08-X-2026 («Confirmado»).

## Diseño de implementación

### Componentes y contratos

- El componente se llamará `UserTextPolicy`. Se propone incorporarlo como componente pequeño y reutilizable dentro de `Linkubator.Domain`, sin crear dependencias entre proyectos.
- API propuesta para revisión:
	- `string? TrimToNull(string? value)`: recorta el texto; devuelve `null` si la entrada es `null` o si queda vacía tras el recorte.
	- `int CountCodePoints(string value)`: devuelve el número de puntos de código Unicode; una pareja sustituta válida cuenta como un punto de código y las marcas combinantes se cuentan individualmente.
	- `bool IsWithinMaximumLength(string value, int maximum)`: devuelve `true` si el conteo por puntos de código no supera `maximum`; devuelve `false` si lo supera. No trunca ni modifica el texto.
- `UserTextPolicy` será una clase estática y sus métodos serán estáticos y sin estado. Las firmas y el carácter estático forman parte de la propuesta técnica, no de la especificación funcional.
- El componente no conocerá propiedades ni máximos propios del producto, no tendrá políticas de obligatoriedad por campo y no definirá ni lanzará errores de dominio. `IsWithinMaximumLength` comunica el resultado con su valor booleano; la integración con entidades y sus errores queda para S3.
- La integración en propiedades y constructores de entidades, incluida la decisión de rechazar un `null` obligatorio o aceptar uno opcional, queda para S3 según [spec.md → «Alcance»](spec.md#alcance). S1.1 prueba el contrato común sin implementar esas entidades.
- S1.1 no consume ni define los errores base de dominio de S1.2; las reglas de error se aplicarán cuando las entidades se implementen en S3.

### Flujos internos

1. El consumidor solicita la normalización común del texto; la operación recorta los extremos y representa el resultado vacío como `null`.
2. Si hay texto, el consumidor obtiene el conteo en puntos de código y lo compara con el máximo aplicable recibido.
3. Si se excede el límite, `IsWithinMaximumLength` devuelve `false`; no modifica ni trunca el texto.
4. S1.1 entrega y prueba las operaciones comunes. En S3, las entidades integran el resultado al proteger sus invariantes y aplican los errores que correspondan. La generación de alias y slugs conserva su propio orden de transformaciones en S1.3 y S1.4.

### Persistencia y dependencias

- No hay persistencia, transacciones ni servicios externos en este trabajo.
- No se añaden paquetes NuGet. Se propone implementar el conteo con APIs Unicode del runtime de .NET 10; la API concreta se confirma durante la implementación y se contrasta con los tests de puntos de código.
- `Linkubator.Domain` no debe referenciar Application, Infrastructure ni Web. `Linkubator.Tests` puede referenciar Domain según la matriz ya configurada.
- Prerrequisitos comprobados: existen `Linkubator.Domain` y `Linkubator.Tests`; el proyecto de tests referencia Domain y contiene `ProjectReferenceMatrixTests`.

### Garantías técnicas

- El conteo distingue puntos de código de unidades UTF-16 y grafemas, según [specifications.md → «Longitudes máximas»](../../context/specifications.md#longitudes-máximas).
- La normalización y el conteo no incorporan reglas específicas de entidades ni máximos fijos de propiedades.
- Domain permanece aislado de las demás capas, conforme a [architecture.md → «Capas»](../../context/architecture.md#capas).
- S1.1 no implementa el truncado de contenido obtenido por scraper; está fuera del alcance de esta tarea.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Llamar `UserTextPolicy` al componente común | Definido | Instrucción de DLG en esta conversación | El nombre queda fijado. |
| Exponer `TrimToNull`, `CountCodePoints` e `IsWithinMaximumLength` con las firmas descritas en «Componentes y contratos» | Definido | [spec.md → «Objetivo»](spec.md#objetivo) y [specifications.md → «Textos introducidos por el usuario»](../../context/specifications.md#textos-introducidos-por-el-usuario) | Las operaciones devuelven normalización y valores booleanos/numéricos; no definen ni lanzan errores de dominio. |
| Representar como `null` el texto vacío tras normalizar y dejar al consumidor la política obligatorio/opcional | Propuesta derivada del contrato funcional | [specifications.md → «Textos introducidos por el usuario»](../../context/specifications.md#textos-introducidos-por-el-usuario) | La utilidad queda independiente de las entidades; S3 probará que cada entidad aplica la política de su campo. |
| Recibir el máximo como argumento y no codificar máximos de propiedades en la utilidad | Propuesta técnica | [specifications.md → «Longitudes máximas»](../../context/specifications.md#longitudes-máximas) | El consumidor suministra el máximo apropiado en el punto de validación; confirmar al aprobar el plan. |
| Cadenas UTF-16 malformadas con sustitutos aislados | No especificado por las fuentes consultadas | [specifications.md → «Longitudes máximas»](../../context/specifications.md#longitudes-máximas) | No introducir una regla de producto nueva. Si aparece un caso que requiera una decisión, detenerlo y proponer aclaración en la fuente propietaria. |

## Estrategia de validación

Las pruebas automatizadas se añadirán a `tests/Linkubator.Tests` como tests unitarios de Domain. Se propone agruparlas en `UserTextPolicyTests`. No se requieren pruebas de UI ni de infraestructura.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comprobación | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada: normalización común | SDK .NET 10 y proyecto de tests disponible | Probar texto sin espacios extremos y con espacios a ambos extremos; comprobar el valor normalizado | El texto normalizado no conserva espacios extremos y no altera el interior | Test ejecutado, resultado y SDK. |
| V02 | CA02 | Automatizada: normalización común; no cubre integración en entidades | Contrato de normalización aprobado | Probar `null`, texto vacío y texto que solo contiene espacios | El resultado común representa la ausencia como `null`; obligatoriedad/opcionalidad se validarán en S3 | Test ejecutado y limitación anotada. |
| V03 | CA03 | Automatizada: límite aceptado | Contrato de máximo recibido aprobado | Probar una longitud igual al máximo y otra menor, con conteo por puntos de código | Los valores dentro del límite no fallan por longitud | Casos, test ejecutado y resultado. |
| V04 | CA04 | Automatizada: resultado booleano ante exceso, sin integrar entidades | Contrato de comparación aprobado | Probar una longitud superior al máximo; comprobar `false` y que el contenido original no se trunca ni modifica | `IsWithinMaximumLength` devuelve `false`; el consumidor aplicará el rechazo al integrarse en las entidades de S3 | Caso, test ejecutado y resultado. |
| V05 | CA05 | Automatizada: carácter fuera del BMP | SDK .NET 10 | Contar un carácter representado por pareja sustituta y comparar con puntos de código | La pareja cuenta como un punto de código, no dos unidades UTF-16 | Entrada, conteo y resultado observado. |
| V06 | CA06 | Automatizada: marcas combinantes | SDK .NET 10 | Contar una letra seguida de una marca combinante | Se cuenta cada punto de código por separado, no el grafema como una sola unidad | Entrada, conteo y resultado observado. |
| V07 | CA07 | Automatizada y revisión técnica: aislamiento de Domain | Solución y test de matriz disponibles | Ejecutar `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~ProjectReferenceMatrixTests` y revisar `Linkubator.Domain.csproj` | Pasa la matriz de referencias y Domain no declara referencias de proyecto | Comando, código de salida, SDK y revisión. |
| V08 | CA01–CA06 | Automatizada integrada | Dependencias restauradas | Ejecutar `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Todos los tests pasan, incluidos los de `UserTextPolicyTests` | Comando, código de salida, resumen y advertencias. |
| V09 | CA01–CA07 | Build integrado | SDK y dependencias disponibles | Ejecutar `dotnet build Linkubator.sln --no-restore --verbosity minimal` | Build correcto, sin errores ni warnings conforme a la DoD común del plan | Comando, SDK, código de salida, errores y warnings. |

V01–V06 prueban el comportamiento común y no demuestran que una entidad concreta aplique sus límites o su política de obligatoriedad; eso queda para S3. V07 comprueba la matriz estructural existente. Como comprobación negativa de sensibilidad, se verificará en una copia temporal que una referencia no permitida desde Domain hace fallar la matriz; el cambio temporal se descartará y no se conservará en el repositorio. V08 y V09 cubren la solución integrada, pero no sustituyen revisión de código ni aprobación humana.

El filtro de V07 usa una clase de test que ya existe. `UserTextPolicyTests` es el fixture previsto para las pruebas nuevas; si se necesita un filtro selectivo, se verificará su compatibilidad con el runner. Si no funciona, se ejecutará el proyecto o solución completos y se registrará la limitación, sin atribuir un resultado selectivo.

## Orden de ejecución y cierre

- Ejecutar las tareas aprobadas de [tasks.md](tasks.md): T02 y T03 pueden avanzar en paralelo; después, completar las comprobaciones integradas y registrar evidencia para cada criterio.
- S1.1 se cierra cuando CA01–CA07 tienen evidencia satisfactoria y DLG acepta el resultado. Este trabajo prueba la normalización y el resultado booleano de longitud; no implementa entidades ni rechazos por error. La aplicación de esas reglas a `User`, `Collection` y `Link` pertenece a S3.
- Si durante la implementación surge una ambigüedad que requiera una regla de producto, detener la tarea afectada y tramitar la aclaración en el documento propietario antes de continuar.
