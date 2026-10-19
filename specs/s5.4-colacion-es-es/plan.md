# Plan técnico: S5.4 — Registro de la colación `es-ES` en las conexiones SQLite

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»).
	- Plan: aprobado por DLG el 19-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

- **Registro por conexión.** Ampliar `SqliteConnectionFactory.CreateOpenConnection()` de S5.1 para registrar la colación en el objeto `SqliteConnection` después de abrirlo y antes de devolverlo. Reutilizar la fábrica común; no añadir otro mecanismo de apertura ni cambiar su contrato.
- **Comparador.** Implementar un comparador usando la cultura `es-ES` y las opciones especificadas en [architecture.md → «Persistencia»](../../context/architecture.md#persistencia). El callback de `CreateCollation` delega la comparación al comparador; el nombre SQL será una constante interna, uniforme y específica de Linkubator, sujeto a confirmación en la aprobación del plan.
- **Ámbito.** El registro es local a cada conexión. La colación solo se usa cuando la consulta la declara explícitamente. No modificar scripts, columnas, índices, unicidad ni opciones globales de SQLite.
- **Pruebas.** Añadir pruebas focalizadas de infraestructura con la fábrica y conexiones SQLite independientes. Las pruebas pueden usar consultas temporales, sin ejecutar migraciones ni depender de las tablas de dominio.

### Flujos internos

1. La fábrica crea y abre la conexión como hasta ahora.
2. En esa instancia abierta, registra el callback de comparación con `CreateCollation`.
3. Solo después del registro, devuelve la conexión al consumidor.
4. Las pruebas ejecutan comparaciones SQL explícitas con la colación en una o más conexiones obtenidas independientemente de la fábrica y contrastan los casos documentados.

### Persistencia y dependencias

- Reutilizar `Microsoft.Data.Sqlite` y la configuración de conexión de S5.1. La colación es una función registrada en cada objeto de conexión y no requiere migración ni estado persistido.
- No requiere el esquema de S5.3, el ejecutor de S5.2 ni el Unit of Work/repositorios de S5.5. El uso por las consultas de repositorio queda a cargo de S5.5; las comprobaciones integradas del orden completo quedan a cargo de S5.8.
- La comparación requiere datos de cultura del runtime; conservar la configuración de globalización prevista por arquitectura y verificar el entorno de pruebas con la cultura disponible.

### Garantías técnicas

- Una conexión solo se entrega al consumidor después de registrar la colación, y cada conexión independiente recibe su propio callback.
- La comparación implementa la cultura y las opciones aprobadas; los tests verifican equivalencias sin inventar una regla especial para `ñ`.
- El registro no establece una colación por defecto para columnas o consultas ni interviene en restricciones de unicidad o búsqueda.
- El trabajo se cierra con pruebas aisladas de la capacidad, sin esperar los repositorios ni las pruebas integradas posteriores.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Registrar la colación en la fábrica común de conexiones | Propuesta | CA01–CA03 de [spec.md](spec.md#criterios-de-aceptación), arquitectura de persistencia y fábrica de S5.1 | Asegura que cada consumidor reciba una conexión configurada sin replicar la inicialización. Aprobar con el plan. |
| Crear una instancia de `CultureInfo` para `es-ES` y comparar con `CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace` | Confirmada por la arquitectura; forma concreta propuesta | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) | Hace reproducible el contrato de comparación. Verificar disponibilidad de datos culturales en el runtime de pruebas. |
| Nombre SQL constante `LINKUBATOR_ES_ES` para `COLLATE` | Aprobado por DLG el 19-X-2026 («LINKUBATOR_ES_ES») | CA01 de [spec.md](spec.md#criterios-de-aceptación) | Los consumidores deberán usar el mismo identificador al escribir consultas. |
| Sin alteración de DDL o semántica de comparación no declarada | Confirmada por la arquitectura | CA03 de [spec.md](spec.md#criterios-de-aceptación) | Evita cambiar unicidad, búsqueda o comportamiento de columnas; comprobar por inspección y test que una comparación sin `COLLATE` no recibe el callback propio. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada: disponibilidad del nombre SQL en una conexión | Fábrica S5.1 y callback registrado | Test xUnit focalizado que abre conexión de la fábrica y ejecuta una comparación explícita con `COLLATE` | La consulta se ejecuta y devuelve el resultado de comparación, sin error de colación desconocida | Test, valores comparados, resultado y runtime. |
| V02 | CA02 | Automatizada: configuración independiente y comportamiento cultural | V01; datos de cultura disponibles en el runtime | Test que abre dos conexiones separadas con la fábrica y comprueba en ambas equivalencia de pares diferenciados por mayúsculas y marcas diacríticas; añadir caso `n`/`ñ` solo para confirmar que no se exige distinción | Ambas conexiones aplican la comparación esperada de `es-ES`; no dependen de registro en la primera conexión | Test, conexiones verificadas, pares y resultado. |
| V03 | CA03 | Automatizada/técnica: alcance explícito del registro | Fábrica implementada | Test comparativo de SQL con `COLLATE` explícito frente a comparación sin él y revisión de migraciones/DDL e índices del cambio | Solo la expresión que declara la colación usa el comparador propio; no hay cambios de esquema ni se asigna la colación a unicidad | Test, archivos de persistencia inspeccionados y resultado. |
| V04 | CA01–CA03 | Integrada: tests focalizados, build y diff | Cambios y pruebas de S5.4 listos | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteCollation --verbosity minimal`, `dotnet build Linkubator.sln --no-restore --verbosity minimal` y `git diff --check` | Tests focalizados y build correctos; diff sin errores de whitespace | Comandos, SDK, códigos de salida y resumen. |
| V05 | CA01–CA03 | Técnica: aislamiento del alcance | Implementación y pruebas completadas | Revisar que la fábrica es el único punto modificado para el registro, no se cambian consultas de repositorio ni DDL, y la aplicación conserva los datos de cultura requeridos | La implementación se mantiene en el límite de S5.4 y no incluye entregables de S5.5 o S5.8 | Archivos revisados, límites confirmados y desviaciones. |

V01–V02 prueban el registro y la comparación sobre conexiones creadas por la fábrica; no validan el orden de todos los listados ni su desempate por `Id`, asignados a S5.8. V03 comprueba que el registro no se convierte en una regla por defecto y no reemplaza los tests futuros de unicidad, búsqueda o consultas de repositorio.

## Orden de ejecución y cierre

- El plan y las tareas fueron aprobados por DLG el 19-X-2026 («Siguiente» en cada artefacto); el identificador SQL aprobado es `LINKUBATOR_ES_ES`.
- La implementación y la validación están completadas. DLG aceptó el resultado el 19-X-2026 («Colación añadida»); la evidencia consolidada consta en [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre).
- S5.4 queda cerrada para su alcance. Las pruebas integradas de orden de listados de S5.8 siguen siendo responsabilidad de esa etapa y no son necesarias para este cierre.
- Si las comprobaciones revelan que el runtime no ofrece los datos de cultura necesarios, detener la tarea y resolverlo en configuración/arquitectura antes de adoptar una comparación alternativa.
