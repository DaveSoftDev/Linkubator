# Plan técnico: S2.8 — Resolución de hallazgos de URLs reales

## Estado y aprobación del plan

- Estado: aprobado por DLG el 12-X-2026.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 12-X-2026 («Aprobada»).
	- Plan: aprobado por DLG el 12-X-2026 («Listo»).

## Diseño de implementación

### Componentes y contratos

- **Fuente propietaria.** La regla y la lista viven en [specifications.md → «Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados). H01 amplía esa lista con `gclsrc`; no requiere una entrada en [decisions.md](../../context/decisions.md), pues no añade una justificación de producto distinta de incluir el parámetro en la lista.
- **Domain.** [UrlNormalizationPolicy.cs](../../src/Linkubator.Domain/Policies/UrlNormalizationPolicy.cs) incorpora `gclsrc` a su conjunto de parámetros de marketing. No cambia el tratamiento de puertos, de los caracteres Unicode ni la firma pública de la política.
- **Tests H01.** Se actualiza el caso existente de la URL de la fila 6 en [UrlNormalizationPolicyTests](../../tests/Linkubator.Tests/Domain/Policies/UrlNormalizationPolicyTests.cs), y se conserva su resultado esperado derivado de la regla: el parámetro nuevo no contribuye a la clave.
- **Diagnóstico H02.** Se añade un test para la URL percent-encoded de la fila 25 y su equivalente con la ruta Unicode. El test compara cada salida con la clave derivada de la fuente y entre sí, evitando pasar el resultado por la consola o por una codificación de salida intermedia. Si falla, se inspecciona el valor en memoria y se corrige solo la divergencia en Domain; si pasa, no se cambia Domain y se registra que el mojibake pertenece a la captura, almacenamiento o representación de la salida de S2.7. En ambos casos, la semántica Unicode de la fuente se conserva.
- **H03.** No requiere cambio de regla, código ni una nueva decisión. La equivalencia de `https://fotok.es/?aff=y2336`, `https://fotok.es:443/?aff=y2336` y `https://fotok.es/?aff=y2336&utm_source=error` se deriva de dos reglas existentes: descartar el puerto HTTPS predeterminado y quitar parámetros de marketing. Se registra el cierre enlazando el paso del puerto, el ejemplo de puertos predeterminados y la lista de marketing de [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados), más el riesgo de clave plana en [decisions.md → «Riesgos aceptados»](../../context/decisions.md#riesgos-aceptados). No se añade una entrada redundante a `decisions.md`.
- **Histórico S2.7.** No se reescriben las filas aceptadas de S2.7. El resultado verificable de H02 y la conclusión sobre la capa que produjo el mojibake quedan como evidencia nueva en las tareas de S2.8.

### Flujos internos

1. Ejecutar la suite enfocada de normalización como línea base y comprobar SDK y filtro.
2. Añadir `gclsrc` a la lista fuente y actualizar el test de la URL real; ejecutar el test con Domain sin modificar y comprobar que falla por incluir todavía `gclsrc`.
3. Añadir `gclsrc` al conjunto de Domain y repetir el test de la URL real; debe pasar.
4. Añadir y ejecutar la prueba de H02 con ambas representaciones de la ruta; inspeccionar y comparar el resultado dentro del test, sin depender de la codificación de consola.
5. Si la prueba demuestra una divergencia de Domain respecto de la fuente, corregir esa divergencia y repetir la prueba. Si pasa sin cambio productivo, documentar que la distorsión se produjo al registrar o representar la salida.
6. Derivar las tres claves relacionadas con H03 según la fuente y dejar el hallazgo cerrado por referencia, sin editar la regla ni el código para H03.
7. Ejecutar los tests de normalización, la suite integrada y el build; revisar el diff para confirmar el alcance.

### Persistencia y dependencias

- No hay persistencia, transacciones, integraciones externas ni paquetes nuevos.
- S2.7 proporciona la entrada; S2.8 no modifica retroactivamente sus resultados.
- La integración de `UrlNormalized` en `Link` pertenece a S3; la detección contra enlaces almacenados pertenece a S4 y S5.

### Garantías técnicas

- Las claves esperadas se derivan de la fuente y no de la salida de la implementación bajo prueba.
- La comprobación de H02 compara valores en memoria y no interpreta el texto mostrado por la consola como evidencia del contenido devuelto por Domain.
- H02 no altera la conservación de caracteres Unicode que determina [specifications.md → «Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados). Si el análisis apunta a un cambio de esa regla, se detiene el trabajo y se solicita una decisión explícita antes de cambiarla.
- No se modifican otras políticas de Domain ni se introducen dependencias nuevas.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| `gclsrc` se incorpora a la lista de parámetros de marketing | Confirmado | Instrucción de DLG, 12-X-2026; [spec.md → «Alcance»](spec.md#alcance) | Actualizar fuente, política y test de la URL de la fila 6. |
| H02 puede originarse en Domain o en la captura/representación de la salida | Pendiente de comprobación | [S2.7 → «Hallazgos para S2.8»](../s2.7-revision-con-urls-reales/tasks.md#hallazgos-para-s28) | El test independiente separa ambos casos; solo se corrige Domain si el valor en memoria contradice la fuente. |
| H03 es una equivalencia ya especificada y no una decisión nueva | Confirmado | [specifications.md → «Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados); [decisions.md → «Riesgos aceptados»](../../context/decisions.md#riesgos-aceptados) | Cerrar por referencia; no añadir decisión ni cambio de comportamiento. |
| La línea base y el filtro de tests deben verificarse en el entorno actual | Pendiente | Esta propuesta | Detectarlo al inicio; si el filtro no funciona, ejecutar el proyecto de tests completo. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA05 | Automatizada: línea base de normalización | SDK .NET y dependencias restauradas | `dotnet test tests/Linkubator.Tests --no-restore --filter "FullyQualifiedName~UrlNormalization"` | Filtro válido y tests actuales pasan antes de los cambios | Comando, SDK, conteo y código de salida. |
| V02 | CA01 | Automatizada: sensibilidad del test H01 | V01; fuente y test actualizados, código sin modificar | Ejecutar el test de la URL de la fila 6 | Falla porque Domain aún incluye `gclsrc` en la clave | Aserción fallida y diff de fuente y test. |
| V03 | CA01 | Automatizada: H01 en verde | V02; conjunto de marketing de Domain actualizado | Repetir el test de la URL de la fila 6 | Pasa y la clave ya no incluye la contribución de `gclsrc` | Test, código de salida y diff acotado. |
| V04 | CA02, CA03 | Automatizada: diagnóstico H02 | Fuente vigente y test nuevo | Ejecutar el test que compara URL percent-encoded, equivalente Unicode y clave esperada derivada de la fuente | Las dos entradas producen la clave esperada. Un fallo muestra una divergencia en Domain; un pase sin cambios productivos indica problema de captura/representación en la evidencia previa | Valores esperados, aserciones y resultado del test, no salida de consola como fuente única. |
| V05 | CA04 | Revisión técnica: cierre H03 | Fuente vigente | Derivar la clave de las tres URLs citadas en Componentes y contratos, aplicando el puerto predeterminado y la eliminación de parámetros de marketing | Las tres producen la misma clave por las reglas ya documentadas; no se requiere decisión nueva | Derivación y enlaces a las reglas fuente. |
| V06 | CA05 | Automatizada integrada | V03–V05 | `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Todos los tests pasan | Comando, código de salida y resumen. |
| V07 | CA05 | Build y revisión técnica | V06 | `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal`; revisar `git diff` | Build sin errores ni warnings; cambios de producción limitados a `UrlNormalizationPolicy` si H02 prueba un defecto real; sin cambio productivo por H03 | Comando, SDK, código de salida y resumen del diff. |
| V08 | CA06 | Revisión de trazabilidad | V02–V07 | Comprobar que cada hallazgo tiene resolución y evidencia enlazada, sin editar S2.7 | H01 corregido; H02 diagnosticado y corregido o registrado como problema de captura; H03 cerrado por referencia | Tabla de cobertura y referencias a evidencia en `tasks.md`. |

V04 no afirma que toda ruta Unicode posible esté cubierta: se limita a la URL de la fila 25 y su representación equivalente. V05 comprueba que H03 se deduce de las reglas existentes; no es una prueba del código. V06 y V07 no sustituyen la aceptación humana de DLG.

## Orden de ejecución y cierre

- Crear `tasks.md` solo tras la aprobación de este plan. Desglosará línea base, rojo/verde de H01, diagnóstico condicional de H02, cierre documental de H03, validaciones, sincronización del roadmap y aceptación, con checks inmediatos.
- El código solo se corrige por H02 si la prueba demuestra que el valor devuelto por Domain contradice la fuente. Una prueba que pasa no autoriza reinterpretar la regla ni editar el riesgo Unicode.
- S2.8 se cierra cuando CA01–CA06 tengan evidencia, no queden bloqueos y DLG acepte el resultado.
- Si aparece una discrepancia que exija cambiar una regla propietaria, detenerse y pedir la decisión correspondiente antes de editarla o implementarla.