# Plan técnico: S1.3 — Generación de alias

## Estado y aprobación del plan

- Estado: aprobado por DLG el 09-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md) aprobada por DLG, 09-X-2026 («Requisitos entendidos»).
  - Plan: aprobado por DLG, 09-X-2026 («Listo para tareas»).

## Diseño de implementación

### Componentes y contratos

- Se añadirá en `Linkubator.Domain/Policies` una política estática dedicada a transformar y validar localmente un candidato de alias. Su contrato devolverá el valor transformado y distinguirá los rechazos propios de S1.3 mediante excepciones específicas de Domain, sin recibir ni consultar servicios de disponibilidad.
- La política realizará la transformación de [specifications.md → «Generación del alias»](../../context/specifications.md#generación-del-alias) con operaciones Unicode: minúsculas invariantes, descomposición para suprimir marcas diacríticas, filtrado del resultado a ASCII, consolidación de guiones y recorte de guiones extremos. La implementación no reutilizará las reglas diferentes de slugs.
- Se añadirán en `Linkubator.Domain/Exceptions` los subtipos sellados de `DomainException` que representen resultado vacío, fuera de rango y reservado. Sus constructores no recibirán el texto introducido ni expondrán valores sensibles; sus identificadores estables se decidirán de forma coherente en la implementación tras aprobar este plan.
- La comprobación del rango se apoyará en `UserTextPolicy.IsWithinMaximumLength`; los límites aplicables se expresarán como constantes internas de la política o en un componente de Domain con responsabilidad específica, sin dispersar números mágicos.
- No se definirá contrato de repositorio ni se introducirá una abstracción para comprobar disponibilidad: la colisión requiere `User` y persistencia, que pertenecen a los trabajos señalados en [spec.md → «Alcance»](spec.md#alcance).

### Flujos internos

1. El consumidor proporciona el texto candidato a la política de alias.
2. La política aplica la transformación completa definida en la fuente, conservando únicamente el resultado ASCII permitido.
3. Si el resultado no cumple las condiciones locales —vacío, longitud o reserva—, la política comunica el rechazo mediante el subtipo de `DomainException` correspondiente y no trunca el resultado.
4. Si el resultado es admisible localmente, la política lo entrega para que S3 lo asigne a `User.Alias` y los trabajos posteriores comprueben la unicidad global.
5. La política no trata una colisión como éxito con sufijo ni cambia el resultado generado; tampoco determina si existe una cuenta que lo use.

### Persistencia y dependencias

- No hay persistencia, transacciones, servicios externos ni paquetes NuGet en este trabajo.
- `Linkubator.Domain` mantiene su aislamiento respecto a Application, Infrastructure y Web; `Linkubator.Tests` consume Domain mediante la referencia ya existente.
- S3 integrará el valor en `User`; S4 comprobará la disponibilidad al ejecutar los casos de uso y S5 impondrá la unicidad en SQLite. Ninguno de esos componentes se adelanta en S1.3.

### Garantías técnicas

- Las pruebas comprobarán todos los ejemplos de alias y casos negativos que pertenecen a S1.3, preservando que S1.5 complete la cobertura conjunta con slugs.
- Los rechazos no incluyen el valor original, transformado, contraseñas ni tokens en su contrato, conforme a [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación).
- La validación de longitud se hace sobre el resultado de la transformación, con el conteo de puntos de código de `UserTextPolicy` y sin truncado, conforme a [specifications.md → «Longitudes máximas»](../../context/specifications.md#longitudes-máximas).
- Una palabra se considera reservada únicamente tras la transformación y por coincidencia exacta con la fuente; las comparaciones no dependen de cultura del entorno.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Usar una política estática de Domain para transformar y validar localmente alias | Propuesta | [spec.md → «Alcance»](spec.md#alcance) | Centraliza la lógica reutilizable sin adelantar la entidad ni la persistencia. |
| Usar normalización Unicode de descomposición y filtrado explícito a ASCII | Propuesta | [specifications.md → «Generación del alias»](../../context/specifications.md#generación-del-alias) | Permite eliminar diacríticos y descartar letras no ASCII, emojis y símbolos según la fuente. |
| Crear un subtipo de `DomainException` por cada rechazo local de alias | Propuesta | [spec.md → «Alcance»](spec.md#alcance), [specs/s1.2-errores-y-enumeradores/plan.md → «Componentes y contratos»](../s1.2-errores-y-enumeradores/plan.md#componentes-y-contratos) | Mantiene errores distinguibles sin exponer el texto; los nombres y códigos concretos requieren aprobación del plan. |
| Mantener la comprobación de alias ocupado fuera de S1.3 | Confirmada | [spec.md → «Alcance»](spec.md#alcance) | No se añade una dependencia de repositorio ni se simula unicidad global antes de S3–S5. |
| Reutilizar `UserTextPolicy.IsWithinMaximumLength` para el rango transformado | Propuesta | [spec.md → «Dependencias y prerrequisitos»](spec.md#dependencias-y-prerrequisitos) | Evita duplicar el conteo Unicode de S1.1; una prueba protegerá que no se trunca el valor. |

## Estrategia de validación

Las pruebas automatizadas se añadirán como tests unitarios de Domain en `tests/Linkubator.Tests/Domain/Policies` y `tests/Linkubator.Tests/Domain/Exceptions`, siguiendo la estructura existente. No se requieren pruebas de UI, integración HTTP, persistencia ni revisión manual para validar este bloque aislado.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA02 | Automatizada: transformación de alias | SDK .NET 10 y proyecto de tests disponible | Tests parametrizados de la política con los ejemplos de alias y entradas equivalentes | Cada entrada genera exactamente el resultado transformado de la fuente | Tests ejecutados, resultado y SDK. |
| V02 | CA03 | Automatizada: rechazos locales y ausencia de truncado | Política y subtipos de excepción disponibles | Tests de resultado vacío y de límites antes y después del rango | Cada caso lanza el subtipo esperado, sin aceptar ni truncar el valor | Tests ejecutados, resultado y códigos comprobados. |
| V03 | CA04 | Automatizada: palabras reservadas | Política disponible | Test parametrizado para cada palabra reservada y prueba de una palabra próxima no reservada | Solo las coincidencias exactas transformadas se rechazan con el subtipo esperado | Tests ejecutados y casos comprobados. |
| V04 | CA03, CA04 | Revisión técnica y automatizada: contrato de errores | Subtipos implementados | Revisar constructores y propiedades; construir los subtipos en tests | Los errores no aceptan ni exponen texto de usuario, contraseñas o tokens | Revisión, tests y resultado. |
| V05 | CA05 | Revisión técnica | Diseño de S1.3 implementado | Revisar el contrato de la política y sus dependencias | No consulta ocupación, no añade sufijos y no depende de repositorios ni entidades inexistentes | Revisión registrada. |
| V06 | CA06 | Automatizada y revisión técnica: aislamiento de Domain | Solución y test de matriz disponibles | Ejecutar `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~ProjectReferenceMatrixTests` y revisar `Linkubator.Domain.csproj` | Pasa la matriz y Domain no declara referencias de proyecto ni paquetes externos | Comando, código de salida, SDK y revisión. |
| V07 | CA01–CA05 | Automatizada selectiva | Dependencias restauradas | Ejecutar los filtros de los tests nuevos de políticas y excepciones de alias | Las pruebas de S1.3 pasan | Comandos, código de salida y resumen. |
| V08 | CA01–CA06 | Automatizada integrada | Dependencias restauradas | Ejecutar `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Todos los tests pasan | Comando, código de salida, resumen y advertencias. |
| V09 | CA01–CA06 | Build integrado | SDK y dependencias disponibles | Ejecutar `dotnet build Linkubator.sln --no-restore --verbosity minimal` | Build correcto, sin errores ni warnings conforme a la DoD común | Comando, SDK, código de salida, errores y warnings. |

V01–V04 prueban la transformación y los rechazos locales de S1.3, pero no prueban que `User` la aplique ni que otro usuario ya use el alias; esos comportamientos pertenecen a S3–S5. V05 es una revisión del límite de responsabilidad y no demuestra la unicidad persistente. Como comprobación negativa de sensibilidad, se añadirá temporalmente una referencia no permitida desde Domain para confirmar que la matriz arquitectónica la detecta; el cambio temporal se descartará. V07–V09 no sustituyen la revisión de código ni la aceptación humana.

## Orden de ejecución y cierre

- Tareas: crear `tasks.md` tras aprobar este plan; contendrá las tareas de política, excepciones, tests unitarios, comprobaciones integradas y revisión humana.
- S1.3 se cerrará cuando CA01–CA06 tengan la evidencia indicada, no queden bloqueos y DLG acepte el resultado. La integración con `User` y la garantía de alias ocupado se trazarán en S3, S4 y S5.
- Si durante la implementación se requiere transformar o rechazar un caso no cubierto por la fuente, detener la tarea afectada y tramitar una decisión en el documento propietario antes de continuar.