# Plan técnico: S1.2 — Errores de dominio base y enumeradores

## Estado y aprobación del plan

- Estado: aprobado por DLG el 09-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md) aprobada por DLG, 09-X-2026 («Excepciones de dominio identificadas»).
  - Plan: aprobado por DLG, 09-X-2026 («Listo para tareas»).
  - Actualización del contrato: aprobada por DLG, 09-X-2026 («hazlo con la base y las específicas»).

## Diseño de implementación

### Componentes y contratos

- Se incorporarán en `Linkubator.Domain`, sin dependencias nuevas, los siguientes tipos públicos:
  - `DomainException`, clase abstracta que heredará de `Exception` y expondrá en `Code` el identificador estable establecido por una excepción específica. Su constructor protegido no aceptará datos introducidos por el usuario, contraseñas ni tokens.
  - `TextExceedsMaximumLengthException`, excepción específica y sellada, con constructor sin argumentos y código `TextExceedsMaximumLength`.
  - `ScrapingStatus`, enumeración con `NotRequested`, `Pending`, `Processing`, `Completed` y `Failed`.
  - `Purpose`, enumeración con `CompleteRegistration`, `PasswordReset`, `EmailChange` y `AccountDeletion`.
- La base permite capturar en conjunto las excepciones de dominio y cada subtipo permite capturar una regla concreta. S1.2 no modifica `UserTextPolicy` ni incorpora entidades que consulten `IsWithinMaximumLength`; S3 será propietaria de lanzar la excepción específica al proteger las invariantes de cada propiedad.
- Los enumeradores solo representan conjuntos cerrados de valores internos. No se añadirán errores de dominio para valores desconocidos de `ScrapingStatus` o `Purpose`: Infrastructure tratará los datos corruptos durante el mapeo de persistencia cuando esa capa exista.
- Las firmas, herencia y nombres anteriores son decisiones técnicas propuestas en este plan; no redefinen las reglas fuente.

### Flujos internos

1. S3 normalizará y comprobará el texto con `UserTextPolicy` al modificar una entidad.
2. Si el predicado de longitud devuelve `false`, S3 construirá `TextExceedsMaximumLengthException` y no aplicará el cambio inválido.
3. Application o Web podrán reconocer la regla por el subtipo, o capturar cualquier excepción de dominio por la base, sin inspeccionar mensajes ni recibir el texto rechazado.
4. Las operaciones internas asignarán uno de los valores de `ScrapingStatus` o `Purpose` definidos por sus flujos propietarios; S1.2 no implementa transiciones de estado ni emisión de tokens.

### Persistencia y dependencias

- No hay persistencia, transacciones, servicios externos ni paquetes NuGet en este trabajo.
- `Linkubator.Domain` no debe referenciar Application, Infrastructure ni Web. `Linkubator.Tests` conserva su referencia existente a Domain.
- Los valores desconocidos recuperados desde SQLite no se transformarán en errores de dominio. Su detección contextual y su tratamiento técnico pertenecen a la futura implementación de Infrastructure.

### Garantías técnicas

- El tipo y el código distinguen el rechazo funcional sin depender de mensajes ni de datos sensibles, conforme a [spec.md → «Criterios de aceptación»](spec.md#criterios-de-aceptación).
- `ScrapingStatus` y `Purpose` contienen solo los valores definidos en [specifications.md → «Estados del scraping»](../../../context/specifications.md#estados-del-scraping) y [specifications.md → «Tokens»](../../../context/specifications.md#tokens).
- Domain se mantiene aislado conforme a [architecture.md → «Arquitectura»](../../../context/architecture.md#arquitectura).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Usar una `DomainException` abstracta y un subtipo específico por regla, conservando `Code` | Confirmada por DLG, 09-X-2026 («hazlo con la base y las específicas») | [spec.md → «Alcance»](spec.md#alcance) | Permite capturar errores conjuntamente o por regla concreta y conserva un identificador estable para su mapeo. |
| Limitar los identificadores de error de S1.2 a `TextExceedsMaximumLength` | Definido | [spec.md → «Alcance»](spec.md#alcance) | Los errores de alias y slug se añadirán en sus trabajos propietarios, sin ampliar este contrato por anticipado. |
| Usar `Purpose` para los propósitos de token | Propuesta | [specifications.md → «Tokens»](../../../context/specifications.md#tokens) | Sus valores técnicos serán `CompleteRegistration`, `PasswordReset`, `EmailChange` y `AccountDeletion`; requiere aprobación del plan. |
| No lanzar excepciones desde entidades en S1.2 | Definido | [spec.md → «Alcance»](spec.md#alcance) | S1.2 prueba la jerarquía; S3 prueba que las entidades lanzan la excepción específica ante exceso de longitud. |
| El criterio CA02 incluye una integración de entidad excluida de S1.2 | Aclarado | [spec.md → «Alcance»](spec.md#alcance), [«Criterios de aceptación»](spec.md#criterios-de-aceptación) | S1.2 verifica que el código puede transportarse en `DomainException`; la evidencia de lanzamiento desde una entidad se completará en S3. |

## Estrategia de validación

Las pruebas automatizadas se añadirán a `tests/Linkubator.Tests` como tests unitarios de Domain, en `DomainExceptionTests` y `DomainEnumerationsTests`. `DomainExceptionTests` comprobará que la base es abstracta, que deriva de `Exception`, que conserva el código recibido por una derivada de prueba y que el subtipo específico se captura por ambos tipos. No se requieren pruebas de UI, persistencia ni infraestructura.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada: identificación por tipo | SDK .NET 10 y proyecto de tests disponible | Construir `TextExceedsMaximumLengthException`, capturarla como `DomainException` y comprobar el subtipo | Se reconoce el subtipo y su base sin usar el mensaje | Test ejecutado, resultado y SDK. |
| V02 | CA02 | Automatizada: código estable del subtipo | Jerarquía de `DomainException` aprobada | Construir la excepción específica sin argumentos y comprobar `Code` | El código es exactamente `TextExceedsMaximumLength` | Test ejecutado y resultado. |
| V03 | CA03 | Revisión técnica y automatizada: contrato sin datos sensibles | Contrato aprobado | Revisar el constructor protegido de la base, el constructor público específico y las propiedades | El subtipo no acepta datos y la base no expone propiedades para texto, contraseñas o tokens | Revisión, test ejecutado y resultado. |
| V04 | CA04 | Automatizada: enumerador de scraping | SDK .NET 10 | Comparar todos los valores de `ScrapingStatus` con la lista prevista | Existen exactamente `NotRequested`, `Pending`, `Processing`, `Completed` y `Failed` | Test ejecutado y valores comprobados. |
| V05 | CA05 | Automatizada: enumerador de propósitos | SDK .NET 10 | Comparar todos los valores de `Purpose` con la lista prevista | Existen exactamente `CompleteRegistration`, `PasswordReset`, `EmailChange` y `AccountDeletion` | Test ejecutado y valores comprobados. |
| V06 | CA06 | Automatizada y revisión técnica: aislamiento de Domain | Solución y test de matriz disponibles | Ejecutar `dotnet test Linkubator.sln --no-restore --filter FullyQualifiedName~ProjectReferenceMatrixTests` y revisar `Linkubator.Domain.csproj` | Pasa la matriz y Domain no declara referencias de proyecto ni paquetes externos | Comando, código de salida, SDK y revisión. |
| V07 | CA01–CA05 | Automatizada selectiva | Dependencias restauradas | Ejecutar los filtros de `DomainExceptionTests` y `DomainEnumerationsTests` | Las pruebas nuevas pasan | Comandos, código de salida y resumen. |
| V08 | CA01–CA06 | Automatizada integrada | Dependencias restauradas | Ejecutar `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Todos los tests pasan | Comando, código de salida, resumen y advertencias. |
| V09 | CA01–CA06 | Build integrado | SDK y dependencias disponibles | Ejecutar `dotnet build Linkubator.sln --no-restore --verbosity minimal` | Build correcto, sin errores ni warnings conforme a la DoD común | Comando, SDK, código de salida, errores y warnings. |

V01–V05 demuestran los contratos entregados por S1.2, pero no demuestran que una entidad los consuma al cambiar una propiedad; esa integración y el lanzamiento efectivo de CA02 pertenecen a S3. V06 comprueba la matriz existente. Como comprobación negativa de sensibilidad, se verificará en una copia temporal que una referencia no permitida desde Domain hace fallar la matriz; el cambio temporal se descartará. V07–V09 no sustituyen revisión de código ni aprobación humana.

## Orden de ejecución y cierre

- Tras aprobar este plan, crear [tasks.md](tasks.md); contendrá las tareas de tipos de Domain, tests unitarios, comprobaciones integradas y revisión humana.
- S1.2 se cerrará cuando CA01–CA06 tengan la evidencia indicada, no queden bloqueos y DLG acepte el resultado. La integración de `TextExceedsMaximumLength` en una entidad se trazará y comprobará en S3.
- Si durante la implementación se requiere otro código de error o enumerador, detener la tarea afectada y preparar una spec o actualización aprobada en su trabajo propietario antes de continuar.