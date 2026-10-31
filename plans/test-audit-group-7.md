# Auditoría individual del grupo 7: identidad de desarrollo y seguridad Web

## Alcance y estado

**Estado actual:** revisión individual automatizada cerrada: 82/82 tests focalizados y 1.827/1.827 en la suite global. El inventario actualizado procede del runner. Los bloqueos y la clasificación descritos a continuación pertenecen al diagnóstico inicial.

Diagnóstico del 2026-10-30, sin cambios en tests ni producción. Se revisaron preparación, ejecución y aserciones de **35 métodos**: identidad y contrato en Application; proveedor, aprovisionamiento y verificador en Infrastructure; middleware y arranque Web; HTTPS, filtrado de host, cabeceras y antiforgery. La revisión es de validez de tests, no una auditoría de vulnerabilidades ni de la autenticación futura.

El inventario inicial contenía **53 casos provisionales**: **44 vigentes y 9 a reforzar**, sin duplicados demostrados. No hay solapamientos por archivo y método con grupos 1–6. Los tests de antiforgery de formularios reales ya asignados a grupos 3–4 se usan como evidencia cruzada, no se reasignan. Se excluyen dos casos de presentación del sidebar y el endpoint auxiliar de análisis de listado, que no son de este grupo.

**Bloqueo histórico del diagnóstico:** dos intentos de ejecución Debug con `--no-restore` fallaron con CS1001 en `LinkService.cs:40` y, en el segundo intento, también en `CollectionService.cs:25`. Son cambios concurrentes ajenos a esta revisión; no se modificaron. El inventario procede de un TRX del binario anterior, con marca **2026-10-30 15:53:23**; se verificó que todos los métodos siguen en los archivos actuales, pero deben redescubrirse las variantes al recompilar. Ese binario anterior ejecutó 56/56 en el filtro amplio, incluidos tres casos excluidos; **no constituye una validación del código actual**.

La clasificación «vigente» describe la garantía observada en el código del test, no un resultado acreditado de la compilación actual. El cierre requiere recompilar, regenerar el inventario, resolver las acciones aprobadas y ejecutar la selección y la suite global.

## Fuentes y límites

| Fuente | Aplicación a la revisión |
|---|---|
| [specifications.md → «Identidad de desarrollo»](../context/specifications.md#identidad-de-desarrollo) | Selección, compleción, persistencia, colecciones, atomicidad e idempotencia. |
| [architecture.md → «Autenticación»](../context/architecture.md#autenticación) | Resolución del perfil persistido e identidad independiente de datos enviados en la petición; no se presupone autenticación real. |
| [architecture.md → «Seguridad web»](../context/architecture.md#seguridad-web) | Cabeceras, HTTPS, hosts y formularios reales con antiforgery. |
| [specifications.md → «Hash de la contraseña»](../context/specifications.md#hash-de-la-contraseña) | Formato y parámetros; separar las garantías actuales del aprovisionamiento y las capacidades de login futuras. |
| [decisions.md → «Identidad de desarrollo»](../context/decisions.md#identidad-de-desarrollo) | Riesgo de identidad compartida aceptado mientras no exista autenticación real. |

## Refuerzos propuestos

| ID | Prioridad | Hallazgo | Acción propuesta |
|---|---|---|---|
| A1 | Baja | `Constructor_WhenPasswordHashIsNotCryptographicallyVerifiableAsGenericHash_StillAcceptsStructuralPhc` no demuestra que su hash no corresponda a una contraseña concreta; solo demuestra que se acepta una cadena estructural válida. | Renombrar según lo que verifica, sin atribuir propiedades criptográficas que el test no observa. |
| A2 | Media | `Provision_WhenUserIsIncomplete_CompletesOnlyMissingProfileFields` prepara todos los campos incompletos a la vez; no demuestra que se preserven los que ya estaban informados. | Sembrar un estado parcial distinguible, comprobar valores conservados y completados, identidad y ausencia de cambios en otra cuenta. |
| A3 | Media | `Provision_WhenConcurrentCallsTargetSameDatabase_CreatesSingleUserAndCollection` usa `Task.WhenAll`, pero no demuestra que ambas operaciones lleguen a competir antes de la primera confirmación. El nombre presupone más que la preparación. | Renombrar a concurrencia solicitada o añadir una barrera observable en el límite SQLite que pruebe solapamiento; evitar sleeps y no reemplazar pruebas de atomicidad. |
| A4 | Baja | `ProvisioningSqlResource_IsEmbeddedOutsideVersionedMigrationCatalog` acopla una prueba de exclusión del SQL de identidad a una lista cerrada de todas las versiones actuales. Una migración legítima obligaría a modificar este test aunque la exclusión siga siendo correcta. | Afirmar presencia del recurso y exclusión explícita del catálogo, dejando la lista versionada a sus pruebas propietarias. |
| A5 | Media | Las dos pruebas de fallo de arranque usan `ThrowsAny<Exception>`: un error ajeno podría satisfacer la aserción. Los conteos posteriores no identifican el motivo. | Comprobar tipo y causa/mensaje específico de configuración inválida o fallo del proveedor, conservando los efectos persistidos comprobados. |
| A6 | Media | Las dos pruebas antiforgery validan explícitamente `IAntiforgery` en un middleware auxiliar que ellas mismas registran: no prueban por sí solas la aplicación automática de antiforgery por Razor Pages. Las pruebas reales de formularios ya cubren ausencia de token en grupos 3–4, pero no estos casos alterados o de otro cliente. | Mantener el test técnico con nombre y alcance explícitos, y probar token alterado y cruzado contra un formulario Razor real con snapshots de datos y control válido. |
| A7 | Media | `HostFiltering_RejectsUnapprovedHostAndAllowsConfiguredHost` comprueba estados, pero no las cabeceras obligatorias en el rechazo. El middleware de host se ejecuta antes que el de cabeceras, de modo que el caso no permite acreditar la regla de todas las respuestas. | Añadir aserciones exactas en respuesta de host rechazado y corregir el orden solo si el test confirma la discrepancia con la regla vigente. |

## Huecos propuestos

| ID | Prioridad | Escenario | Acción propuesta |
|---|---|---|---|
| H1 | Media | El fallo forzado de inserción de colección comprueba rollback de una cuenta nueva, pero no de la compleción de una cuenta existente ni de la proyección asociada. | Forzar el fallo tras sembrar cuenta incompleta; contrastar antes/después de cuenta, colecciones y proyecciones. |
| H2 | Media | El proveedor prueba cuenta ausente e incompleta con una única combinación de valores nulos; no discrimina cada campo indispensable ni perfil en blanco con el resto válido. | Matriz por campo con controles completos y selección por email, sin asumir que los nulos de todos los campos son válidos en la misma etapa de dominio. |
| H3 | Media | La configuración del hash tiene un rechazo genérico; el verificador tiene cuatro casos malformados, pero faltan fronteras aisladas de parámetros y tamaños y su integración con el constructor. | Teorías directas de valores límite y adyacentes, versión/formato y tamaño de sal y digest; comprobar aceptación/rechazo estructural sin calcular hashes costosos para cada caso. |
| H4 | Media | Las cabeceras se prueban en respuestas 200 y 404, pero no en redirección HTTPS ni en rechazo de host; el último caso se aborda en A7. | Añadir control de las tres cabeceras en la redirección HTTP sin seguimiento y en host rechazado. |
| H5 | Baja | El middleware prueba identidad por ruta y query, pero no la propagación del token de cancelación ni que un fallo del proveedor impida invocar el siguiente componente. | Usar proveedor controlado y comprobar token recibido, excepción y que el siguiente delegado no se ejecutó. |
| H6 | Baja | El arranque se prueba en un entorno predeterminado, no explícitamente en un entorno distinto; la regla prescribe todos los entornos. | Arrancar un host de pruebas con entorno no Development, comprobar perfil y colección persistidos y que `/app` recibe la identidad; sin atribuir autenticación real. |

## Resolución y verificación

Tras la aprobación se aplicaron A1–A7 y H1–H6:

- Renombrados los casos de hash, concurrencia solicitada y validación antiforgery explícita; la exclusión del SQL de identidad ya no depende de una lista cerrada de migraciones.
- Compleción parcial con snapshots y otra cuenta; rollback de cuenta incompleta, colecciones y proyecciones; proveedor con campos incompletos aislados.
- Fronteras estructurales del PHC, formato y longitudes, tanto en verificador como constructor.
- Excepciones de arranque específicas; middleware con cancelación y fallo del proveedor; arranque explícito en Production.
- Formulario Razor real con tokens alterado, cruzado y válido, y snapshots de colección y proyección. Cabeceras exactas en redirección y host rechazado.

**Corrección de producción confirmada por test:** el rechazo de host se producía antes de las cabeceras. Se ordenó el middleware para que la respuesta incluya las tres cabeceras sin desactivar el filtrado; también se retiró el filtro automático anterior al pipeline para mantener la comprobación explícita en su nueva posición.

El [inventario actualizado](test-audit-group-7.csv) recoge **82 casos de 45 métodos** con identificadores del runner y sin solapamientos con grupos 1–6. Incluye 43 casos conservados, nueve reforzados y 30 nuevos; un caso de la teoría anterior se sustituyó por otros más discriminantes. La selección focalizada sobre código definitivo recompilado obtuvo **82/82**; los identificadores coinciden exactamente con el inventario.

Las dos regresiones globales iniciales obtuvieron **1.811/1.813**, sin omitidos, por fallos ajenos al grupo en `CollectionServiceTests.Operations_WhenCustomValidatorsAcceptNull_ReturnRequestRequired` y `LinkServiceTests.Operations_WhenCustomValidatorsAcceptNull_ReturnRequestRequired`. Ambos construían un resultado fallido sin errores cuando un validador personalizado aceptaba una solicitud nula.

Tras corregir ambos servicios, la selección de sus tests pasó **124/124** y la suite global recompilada pasó **1.827/1.827**, sin omitidos. **Revisión individual automatizada cerrada.**
