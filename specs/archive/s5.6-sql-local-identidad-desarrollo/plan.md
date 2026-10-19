# Plan técnico: S5.6 — Aprovisionamiento SQL de identidad de desarrollo

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Validado y confirmado»).
	- Plan: aprobado por DLG el 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- **Inicialización del host Web.** En el composition root, antes de iniciar el servidor y aceptar peticiones, resolver el servicio `ISqliteMigrationService` existente, llamar `ApplyPendingMigrations`, y solo tras su éxito ejecutar el aprovisionamiento de identidad. Si un paso falla, el host no empieza a atender solicitudes.
- **Aprovisionador de Infrastructure.** Añadir un servicio dedicado, registrado por `AddInfrastructure`, que reciba la configuración de identidad, use `ISqliteConnectionFactory` y ejecute el recurso SQL de aprovisionamiento fuera del catálogo de `SqliteMigrationCatalog`. Mantener la consulta y los cambios de datos en SQL parametrizado y en Infrastructure, sin añadir responsabilidad de persistencia a Web.
- **Configuración.** Leer el email, nombre, alias y hash inicial desde opciones/configuración inyectada. Los valores funcionales iniciales y su política se rigen por [specifications.md → «Identidad de desarrollo»](../../../context/specifications.md#identidad-de-desarrollo); el archivo que suministra esos valores en cada entorno se decidirá al aprobar este plan. No incluir contraseñas en claro.
- **Hash inicial.** Generar fuera de línea un hash PHC Argon2id válido para una contraseña aleatoria descartada y fijarlo como valor de configuración no secreto; el SQL lo usará solo al insertar una cuenta o completar una incompleta. No calcular un hash lento durante cada arranque ni reemplazar el hash de una cuenta ya completada. `Argon2idPhcVerifier` valida criptográficamente un PHC contra una contraseña presentada mediante recálculo Argon2id y comparación en tiempo constante; se prueba con un vector conocido de tests. No se conserva la contraseña de semilla ni se pretende verificarla en el arranque. La preparación explícita de la contraseña elegida para LoginV0 permanece separada.
- **Proveedor de identidad.** No implementarlo aquí. S5.7 consume la cuenta preparada por este flujo, conforme al contrato de S4.3.

### Flujos internos

1. Web construye el host y obtiene la instancia de `ISqliteMigrationService`.
2. El host aplica las migraciones pendientes. Un fallo interrumpe el arranque.
3. El host obtiene el aprovisionador y configura sus opciones; el servicio abre la conexión común.
4. En una única transacción `BEGIN IMMEDIATE`, el SQL localiza la cuenta por email. Si no existe, inserta perfil completo; si existe incompleta, actualiza los campos de perfil ausentes; si ya está completa, no ejecuta una escritura sobre la cuenta.
5. En esa misma transacción, solo si la cuenta no tiene colecciones, inserta «Bandeja de entrada» privada. Si ya tiene una o más, no ejecuta un insert ni altera ninguna.
6. El servicio confirma la transacción. En error, revierte y propaga el fallo; el host no comienza a escuchar.
7. Tras completar migración y aprovisionamiento, el host continúa el arranque normal; S5.7 registrará/resolverá el proveedor para peticiones privadas.

### Persistencia y dependencias

- Reutilizar la fábrica de conexión SQLite configurada en S5.1, con sus PRAGMA y colación donde aplique. No mantener conexiones o transacciones abiertas tras finalizar la inicialización.
- El recurso SQL especial será accesible desde el ensamblado publicado y quedará excluido del catálogo y del historial versionado de migraciones. **Propuesta:** almacenarlo como recurso incrustado separado de la carpeta/catálogo de migraciones para no depender del directorio actual ni de archivos locales presentes en despliegue.
- Ejecutar el aprovisionamiento dentro de `BEGIN IMMEDIATE`, con comprobaciones y escrituras mínimas en una sola transacción. Esto serializa inicios concurrentes de instancias contra la misma base SQLite y evita duplicados/estado parcial; las restricciones del esquema siguen siendo garantía final.
- La cadena de conexión y la configuración de identidad deben estar disponibles en cualquier entorno; `appsettings.Development.json` no puede ser el único origen requerido por staging, beta o producción. El modo de distribuir/configurar los valores en esos entornos se define sin cambiar la regla de producto.
- S5.2 aporta el migrador ya implementado; S5.3 el esquema; S5.1 la fábrica SQLite. S5.7 aporta el proveedor de identidad y no se incluye aquí.

### Garantías técnicas

- El servidor no atiende solicitudes antes de completar migraciones y aprovisionamiento (CA01).
- Repeticiones y arranques concurrentes no duplican la cuenta ni la colección requerida, y las entidades existentes que la regla ordena conservar no se reescriben (CA02, CA03, CA05).
- Un fallo revierte todos los cambios del aprovisionamiento, y evita servir el host con una identidad no preparada (CA01, CA06).
- El hash de la cuenta nueva/incompleta tiene el formato requerido y no se persiste contraseña en claro; la cuenta ya completa conserva su contraseña actual (CA04).
- El SQL de identidad nunca se registra como migración versionada ni altera el historial de S5.2 (CA05).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Ejecutar las migraciones en el arranque antes del aprovisionamiento | Confirmada | DLG, 19-X-2026 («debe ser la aplicación la que las ejecute»); [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) | S5.6 integra en Web la llamada al servicio existente; no amplía el catálogo de migraciones. |
| Aprovisionamiento atómico mediante `BEGIN IMMEDIATE` | Confirmada por [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia) y [specifications.md → «Identidad de desarrollo»](../../../context/specifications.md#identidad-de-desarrollo) | CA05–CA06 | Serializa escritores SQLite durante la operación breve y permite rollback completo ante fallo. |
| Recurso SQL fuera del catálogo, incrustado en Infrastructure | Confirmada al aprobar el plan; implementada | [spec.md → «Alcance»](spec.md#alcance), S5.2; validación T07 en [tasks.md](tasks.md#descomposición) | El recurso publicado queda separado del namespace/catalogación de migraciones versionadas. |
| Usar un hash inicial válido de una contraseña aleatoria descartada, sin conservar esa contraseña | Confirmada por DLG («Pues genera el hash que quieras y úsalo») | [specifications.md → «Identidad de desarrollo»](../../../context/specifications.md#identidad-de-desarrollo), [«Hash de la contraseña»](../../../context/specifications.md#hash-de-la-contraseña) y [architecture.md → «Autenticación»](../../../context/architecture.md#autenticación) | Este plan propone provisionar un hash PHC Argon2id estable para inserciones/completado; no se recalcula en cada arranque y no reemplaza el hash de una cuenta ya completada. |
| Nombre/alias/email y hash en valores configurados por entorno | Confirmada al aprobar el plan; implementada con la configuración base | [specifications.md → «Identidad de desarrollo»](../../../context/specifications.md#identidad-de-desarrollo), [specs S0.6](../s0.6-configuracion-local/spec.md); validación T02/T07 en [tasks.md](tasks.md#descomposición) | Las opciones se enlazan desde la configuración no limitada a Development y se validan al construir el aprovisionador. |
| Unicidad de alias ante una cuenta incompleta preexistente | Propuesta técnica | [domain-model.md → «User»](../../../context/domain-model.md#user), [specifications.md → «Generación del alias»](../../../context/specifications.md#generación-del-alias) | La unicidad global se conserva; si el alias configurado ya está ocupado por otro usuario, abortar y revertir el aprovisionamiento, sin alterar la otra cuenta. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA05 | Automatizada: orden de inicialización | Host Web; migrador y aprovisionador registrados; base SQLite temporal | Test de arranque con migración que crea una tabla requerida por aprovisionamiento; comprobar orden mediante estado observable y que no hay listening/host iniciado si una fase falla | Migraciones finalizadas antes de ejecutar el aprovisionamiento; error impide servir solicitudes | Test, estado inicial y final, excepción/fase observada. |
| V02 | CA02–CA04 | Automatizada: aprovisionamiento y estados | Esquema S5.3 aplicado, configuración de prueba y recurso SQL | Tests en SQLite temporal para cuenta ausente, incompleta, completada; validar perfil completado, email confirmado, hash válido y preservación de datos de cuenta completa | Cada rama coincide con la regla propietaria y no se modifica una cuenta ya completa | Nombres de tests, estado de filas y resultado de validación del hash sin revelar el secreto original. |
| V03 | CA03, CA05 | Automatizada: colecciones | Fixtures con cuenta sin colecciones y con colección existente renombrada o pública | Ejecutar aprovisionador y comparar colecciones antes/después | Solo se inserta «Bandeja de entrada» privada cuando el usuario no tenía colección; de lo contrario estado idéntico | Tests y snapshots de estado relevante. |
| V04 | CA05 | Automatizada: idempotencia y concurrencia | Base temporal y dos invocaciones/procesos sobre el mismo archivo | Ejecutar varias invocaciones secuenciales y concurrentes; comprobar conteo de usuario/colecciones y consistencia final | No hay duplicados ni estado parcial; se respetan unicidades de esquema | Test, número de invocaciones y observación de bloqueo/resultado. |
| V05 | CA06 | Automatizada: rollback | Punto de fallo controlado dentro de la transacción | Provocar fallo entre escritura de usuario y colección, leer con conexión nueva | Ningún cambio parcial del intento fallido es visible | Test, punto de fallo y lectura posterior. |
| V06 | CA01–CA06 | Integrada: suite, build y diff | Implementación integrada, paquetes restaurados | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~DevelopmentIdentity --verbosity minimal`; `dotnet build Linkubator.sln --no-restore --verbosity minimal`; `git diff --check` | Tests focalizados, build y diff check correctos | Comandos, SDK, códigos de salida y recuentos. |
| V07 | CA01, CA04, CA05 | Revisión técnica de empaquetado/configuración | Publicación/ensamblado Web e Infrastructure y fuentes de configuración | Inspeccionar que SQL no se enumera como migración, está disponible en el artefacto publicado, opciones se enlazan en todos los entornos y no hay contraseña en claro | Recursos presentes sin mezclarse con migraciones; no hay configuración obligatoria solo en Development ni credenciales en claro | Archivos/recursos revisados, resultado y cualquier hallazgo. |

V01–V05 prueban el aprovisionador y el orden del host en entornos de test; no prueban la disponibilidad de la ruta SQLite ni secretos de los entornos desplegados. La revisión manual de migraciones/esquema y la ejecución manual repetida siguen siendo S5.9/S5.10. La identidad por petición y su registro siguen siendo S5.7.

## Orden de ejecución y cierre

- La especificación fue aprobada por DLG el 19-X-2026 («Validado y confirmado»), este plan por DLG el 19-X-2026 («Siguiente») y las tareas por DLG el 19-X-2026 («Ready»). La implementación se ejecutó tras la aprobación de las tareas; validación y evidencia constan en [tasks.md → «Descomposición»](tasks.md#descomposición) y [«Evidencia por criterio»](tasks.md#evidencia-por-criterio).
- La validación puede usar una base temporal migrada por el servicio real; no depende de la revisión manual de S5.9 ni de la verificación manual S5.10 para cerrar los criterios técnicos propios.
- El riesgo de identidad compartida hasta LoginV0 está aceptado por DLG en [decisions.md → «Identidad de desarrollo»](../../../context/decisions.md#identidad-de-desarrollo); no se amplía aquí ni se presenta como autenticación.
- DLG aceptó el resultado el 19-X-2026 («Tenemos demo user»), registrado en [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre). DLG confirmó el 19-X-2026 («Sí bastan, ahora mismo no es necesario que realmente funcione») que para CA04 es suficiente la verificación criptográfica general del formato PHC mediante vectores conocidos; no se exige conocer la contraseña aleatoria descartada para validar el hash seed concreto.