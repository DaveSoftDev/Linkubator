# Plan técnico: S5.2 — Ejecutor de scripts SQL versionados

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»).
	- Plan: aprobado por DLG el 19-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

- **Scripts versionados.** Propuesta: guardar los scripts de base de datos versionados como recursos incrustados del proyecto Infrastructure. Cada archivo tendrá una versión numérica en su nombre; el ejecutor enumerará los recursos, interpretará la versión y aplicará los scripts en orden numérico ascendente. Los scripts no dependerán de rutas de trabajo locales ni de que los archivos SQL se copien por separado al directorio de ejecución.
- **Registro de aplicación.** El ejecutor creará, si no existe, una tabla técnica de historial de migraciones, con la versión aplicada como clave única y una marca temporal UTC. Este registro no forma parte del esquema de dominio de S5.3.
- **Ejecutor.** Añadir en Infrastructure un servicio explícito de migración que use `ISqliteConnectionFactory` y Dapper sobre la conexión abierta de S5.1. El servicio consultará las versiones registradas, omitirá las ya aplicadas y ejecutará las pendientes según el orden definido. El contrato, nombres y forma de invocación son propuestas de este plan.
- **Registro DI.** Registrar el servicio desde `AddInfrastructure`; no duplicar configuración ni construcción de conexiones. El ejecutor no se invoca a sí mismo ni se acopla al host Web. La composición del arranque lo invoca explícitamente antes del aprovisionamiento de identidad, conforme a la decisión posterior de DLG documentada en [architecture.md → «Persistencia»](../../context/architecture.md#persistencia); S5.9 conserva la revisión humana del esquema y SQL.
- **Pruebas.** Usar scripts de prueba incrustados, con versiones y cambios de esquema aislados, para validar el orden, la repetición y el tratamiento de un script inválido sin depender de las migraciones de entidades de S5.3.

### Flujos internos

1. El consumidor resuelve y llama explícitamente al servicio de migración.
2. El servicio abre una conexión mediante `ISqliteConnectionFactory` y asegura la existencia del registro de versiones.
3. Enumera y ordena los scripts disponibles, consulta cuáles constan como aplicados y salta esas versiones.
4. Para cada versión pendiente, ejecuta el contenido SQL y registra su versión solo tras completar correctamente el script. Propuesta: ejecución del script y escritura de su registro dentro de una misma transacción SQLite, para que un error no deje una versión marcada como aplicada.
5. Si hay un error SQL, la operación termina con fallo y se propaga la excepción de infraestructura; no se continúa aplicando versiones posteriores.

### Persistencia y dependencias

- Reutilizar la fábrica de conexiones de S5.1, sus PRAGMA y las dependencias SQLite/Dapper ya incorporadas. No crear conexiones con una configuración paralela.
- Incrustar los scripts en el ensamblado de Infrastructure para que la lectura no dependa del directorio actual ni de copiar archivos al desplegar. El mecanismo exacto de descubrimiento y validación de nombres se confirma con la aprobación del plan.
- La tabla de historial es infraestructura técnica; los scripts de S5.3 introducirán las tablas de dominio y sus restricciones. No se incluye SQL de esas entidades en este trabajo.
- El script especial de aprovisionamiento de desarrollo de S5.6 no se incorpora al catálogo ni al historial de migraciones. La aplicación coordina explícitamente la ejecución del migrador y, después, el aprovisionamiento en el arranque.
- Una transacción por script mantiene acotado el rollback de cada versión. Este plan no fija el tamaño ni el contenido de futuras migraciones.

### Garantías técnicas

- Los scripts disponibles se aplican en un orden determinista por versión.
- Una versión registrada no vuelve a ejecutarse en invocaciones posteriores.
- El fallo al ejecutar un script no registra su versión como aplicada ni permite continuar silenciosamente con versiones posteriores.
- La prueba de repetición compara el estado de una base temporal antes y después de la segunda invocación; CA03 no afirma por sí sola que cualquier secuencia de sentencias SQLite revierta efectos parciales fuera de la transacción acordada.
- El ejecutor permanece en Infrastructure y respeta el grafo de dependencias de [architecture.md → «Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Mantener scripts SQL versionados como mecanismo de migración | Confirmada por arquitectura | [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) | La etapa utiliza scripts SQL o herramienta ligera equivalente; este plan concreta la primera opción. |
| Incrustar scripts en Infrastructure y versionarlos mediante nombres numéricos | Aprobada por DLG con el plan («Migraciones ready», 19-X-2026) | CA01–CA02 de [spec.md](spec.md#criterios-de-aceptación) | Evita dependencia de rutas de archivos en ejecución; los nombres tendrán versión numérica ordenable. |
| Guardar las versiones aplicadas en una tabla técnica con versión única y fecha UTC | Aprobada por DLG con el plan («Migraciones ready», 19-X-2026) | CA02 de [spec.md](spec.md#criterios-de-aceptación) | Permite idempotencia; la tabla técnica queda separada del esquema de dominio. |
| Ejecutar script y registrar versión en una única transacción por script | Aprobada por DLG con el plan («Migraciones ready», 19-X-2026) | CA03 de [spec.md](spec.md#criterios-de-aceptación) y [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) | Ante error se revierten las sentencias transaccionales del script y no se registra la versión. |
| El host invoca el servicio de migraciones al arrancar, antes del aprovisionamiento de identidad | Confirmada por DLG el 19-X-2026 («debe ser la aplicación la que las ejecute») y documentada en [architecture.md → «Persistencia»](../../context/architecture.md#persistencia) | Sustituye la propuesta inicial de mantener manual la invocación en producción. El servicio de S5.2 permanece independiente del host; la orquestación del arranque se integra en S5.6. |
| S5.2 no incorpora esquema de entidades ni aprovisionamiento local | Confirmada por el reparto de S5 | [mvp0-plan.md → tareas S5.2, S5.3, S5.6 y S5.9](../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l), [spec.md → Alcance](spec.md#alcance) | Mantiene el cierre de este bloque independiente del esquema y del SQL de desarrollo. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada: orden y aplicación de scripts de prueba | Ejecutor y scripts de test disponibles; base SQLite temporal | Test xUnit con dos o más scripts que dejan marcas consultables; invocar migración y leer las marcas desde SQLite | Todas las marcas están presentes y reflejan el orden numérico esperado | Nombre del test, estado, SDK y resultado observado. |
| V02 | CA02 | Automatizada: idempotencia | V01 pasa; misma base temporal | Capturar el estado final, volver a invocar el ejecutor y comprobar el estado y el historial | No hay una segunda aplicación de scripts ni cambios adicionales en los datos/estado de prueba | Nombre del test, invocaciones y comparación observada. |
| V03 | CA03 | Automatizada: error SQL e historial | Ejecutor y recurso de prueba inválido disponibles; base SQLite temporal | Ejecutar un conjunto donde una versión falla; comprobar excepción/fallo, ausencia de registro de esa versión y que las versiones posteriores no se hayan aplicado | El error es observable, la versión fallida no queda marcada y no se aplica silenciosamente una versión posterior | Nombre del test, versión fallida, excepción observada y contenido del historial. |
| V04 | CA01–CA03 | Integrada: compilación y suite focalizada | Implementación y tests completos; paquetes restaurados | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteMigration --verbosity minimal` y `dotnet build Linkubator.sln --no-restore --verbosity minimal` | Filtro focalizado y build de solución correctos | Comandos, SDK, códigos de salida y resumen. |
| V05 | CA01–CA03 | Técnica: revisión de empaquetado y límites | Recurso de migración integrado | Revisar que los scripts se descubren desde el ensamblado y que el ejecutor reutiliza `ISqliteConnectionFactory` | No se depende del directorio actual; no se duplican conexiones/configuración ni se incorpora el esquema de S5.3 | Archivos revisados, resultado y cualquier desviación. |

V01–V03 comprueban el ejecutor con scripts de prueba y una base temporal; no validan el esquema final, restricciones, la aplicación manual completa ni las carreras, que corresponden a S5.3, S5.8 y S5.9. La revisión V05 confirma límites de diseño, no sustituye tests de comportamiento.

## Orden de ejecución y cierre

- El plan fue aprobado por DLG el 19-X-2026 («Siguiente»). Se crea `tasks.md` desde su plantilla con tareas acotadas que cubren CA01–CA03 y validaciones V01–V05; la aprobación de tareas queda confirmada con la misma referencia y la implementación ya verificada.
- La implementación de S5.2 debe consumir el contrato real de la fábrica de S5.1; esta última ya está implementada y aceptada según sus tareas. Si el contrato cambia, registrar la dependencia y actualizar el diseño antes de avanzar.
- No implementar las tablas de dominio de S5.3 ni el SQL de aprovisionamiento local de S5.6 en este trabajo. La aplicación del ejecutor durante el arranque, antes del aprovisionamiento, queda integrada en S5.6.
- S5.2 podrá cerrarse cuando todos sus criterios tengan evidencia satisfactoria, sus tareas estén completadas y DLG acepte explícitamente el resultado en `tasks.md`. La aplicación manual del conjunto final de scripts permanece en S5.9 y no bloquea este cierre.
- Si la implementación revela una necesidad de cambiar reglas de persistencia fuera de este alcance, detener la tarea afectada y tramitar el cambio en su documento propietario antes de continuar.
