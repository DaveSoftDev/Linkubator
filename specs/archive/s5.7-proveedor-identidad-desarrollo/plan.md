# Plan técnico: S5.7 — Proveedor de identidad de desarrollo

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
	- Especificación: [spec.md](spec.md), aprobada por DLG el 19-X-2026 («Siguiente»).
	- Plan: aprobado por DLG el 19-X-2026 («Siguiente»), instrucción explícita en esta conversación.

## Diseño de implementación

### Componentes y contratos

- **Proveedor de Infrastructure.** Implementar `IDevelopmentIdentityProvider` en Infrastructure. Recibe la configuración `DevelopmentIdentityOptions` y la fábrica SQLite existente; la consulta por email configurado devuelve el `UserId` solo si la cuenta existe y está completada. No modifica la cuenta ni acepta datos de identidad desde la petición.
- **Validación al arrancar.** Tras las migraciones y el aprovisionamiento ya ejecutados por el composition root, resolver el proveedor y llamar su operación. Si no se obtiene una cuenta elegible, propagar el fallo e impedir que el host empiece a atender peticiones.
- **Resolución por petición.** Registrar el proveedor en todos los entornos y usarlo en cada petición privada según el contrato de S4.3. Mantener la construcción/consumo de `ICurrentUser` en su límite existente; no cambiar los casos de uso en S5.7.
- **Opciones.** Reutilizar las opciones enlazadas desde `DevelopmentIdentity`; el email es la clave de resolución según las fuentes funcionales. Los valores de perfil y el hash son responsabilidad del aprovisionamiento de S5.6, no de este proveedor.
- **Ubicación y estructura.** Mantener la implementación bajo el módulo de identidad de desarrollo en Infrastructure, en un archivo propio por tipo público, siguiendo la estructura existente.

### Flujos internos

1. El composition root registra Infrastructure y configura los servicios como actualmente.
2. Antes de iniciar el host, completa las migraciones y el aprovisionamiento de S5.6.
3. En el mismo límite de inicialización, resuelve `IDevelopmentIdentityProvider` y ejecuta `GetUserId`; cualquier fallo aborta el arranque.
4. En cada petición privada, el consumidor usa `GetUserId` para obtener la identidad desde el proveedor; los datos de la solicitud no pueden reemplazarla.
5. El proveedor abre y libera los recursos de lectura con el mecanismo común. Una cuenta inexistente o sin completar no produce un identificador utilizable.

### Persistencia y dependencias

- Reutilizar `ISqliteConnectionFactory` para que las conexiones mantengan la configuración común de SQLite. La lectura de la cuenta queda en Infrastructure y no requiere modificar el esquema, el catálogo SQL, el aprovisionador ni los repositorios de colecciones/enlaces.
- La resolución por petición debe usar una conexión acotada a esa operación; no mantener una conexión SQLite singleton ni cachear el `UserId` sin una garantía explícita de vigencia.
- El proveedor es independiente del entorno de host y se registra en todos ellos hasta LoginV0, como requiere el alcance aprobado.
- S5.1–S5.6 aportan conexión, esquema, contratos y aprovisionamiento. S5.8 conserva las pruebas integradas de las garantías de persistencia; no es prerrequisito para cerrar los criterios propios de S5.7.

### Garantías técnicas

- La implementación permanece en Infrastructure y respeta la matriz de referencias; Application conserva solo el contrato de S4.3 (CA01).
- El usuario se identifica por el email configurado y la cuenta debe estar completada; un estado ausente o incompleto falla cerrado en la validación de inicio (CA02).
- El proveedor está registrado en todos los entornos y la comprobación inicial ocurre después de migrar y aprovisionar (CA03, CA05).
- En cada petición privada se obtiene la identidad mediante el contrato, sin aceptar identificadores enviados por el cliente (CA04).
- La lectura no altera datos de la cuenta ni mantiene recursos más allá de la operación (CA05).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Implementar el contrato existente mediante consulta SQLite por email configurado y comprobar que el perfil esté completo | Propuesta | [spec.md → CA01 y CA02](spec.md#criterios-de-aceptación), [architecture.md → «Autenticación»](../../../context/architecture.md#autenticación) | Conserva la autoridad de configuración y excluye cuentas no elegibles; revisar consulta y criterio de perfil al aprobar el plan. |
| Validar el proveedor al arranque después del aprovisionamiento y propagar cualquier error antes de iniciar el servidor | Confirmada por las fuentes; integración concreta propuesta | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia), [S5.6 → CA01](../s5.6-sql-local-identidad-desarrollo/spec.md#criterios-de-aceptación) | Evita atender peticiones con una identidad no resuelta; requiere un test de arranque negativo. |
| Registrar el proveedor concreto en todos los entornos | Confirmada por el alcance | [mvp0-plan.md → tarea S5.7](../../../plans/mvp0-plan.md#s5-persistencia-i-sqlite-y-transacciones-l), [S4.3 → CA05](../s4.3-proveedor-identidad-desarrollo/spec.md#criterios-de-aceptación) | No condicionar el registro al entorno Development. |
| Consultar en cada llamada, sin guardar conexión ni cachear el identificador | Propuesta técnica | [S4.3 → «Componentes y contratos»](../s4.3-proveedor-identidad-desarrollo/plan.md#componentes-y-contratos) | Evita una identidad obsoleta y mantiene recursos acotados; evaluar coste y ciclo de vida al revisar el diseño. |
| Ajustar los casos de uso o la construcción de `ICurrentUser` | Fuera de alcance | [spec.md → «Alcance»](spec.md#alcance), [S4.2 → contratos](../s4.2-contratos-repositorios-identidad/spec.md) | No se alteran contratos ni flujos de Application en este trabajo. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada y técnica: contrato y composición | Implementación del proveedor y proyectos restaurados | Ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~SqliteInfrastructureRegistrationTests --verbosity minimal` y revisar las referencias de proyecto | El contrato se resuelve desde el contenedor de Infrastructure y Application no adquiere dependencias de infraestructura | Test, resultado y matriz de referencias revisada. |
| V02 | CA02 | Automatizada: resolución | Base SQLite temporal migrada; configuraciones de cuenta completada, inexistente e incompleta | Tests focalizados del proveedor; consultar el resultado y el estado de la base después de cada llamada | Solo se devuelve el `UserId` de la cuenta completada configurada; ausente/incompleta falla y la consulta no modifica filas | Tests, estados de datos y error observado sin registrar datos sensibles. |
| V03 | CA03, CA04 | Automatizada: registro y autoridad | Contenedor y adaptador de usuario identificado en el host de pruebas | Resolver `IDevelopmentIdentityProvider` con distintos entornos y enviar petición de prueba con identidad arbitraria cuando exista un endpoint privado de prueba aplicable | El registro funciona independientemente del entorno y la identidad usada no varía según datos aportados por la petición | Configuraciones de entorno y test; dejar claro qué frontera del sistema se ejercitó. |
| V04 | CA05 | Automatizada: arranque ordenado y fallo cerrado | Host de pruebas, SQLite temporal y aprovisionador S5.6 | Tests de arranque: comprobar migraciones/aprovisionamiento antes de la resolución; forzar una cuenta no elegible y verificar que `WebApplicationFactory` no sirve solicitudes | El host disponible tiene identidad resoluble; cualquier fallo de resolución impide servir solicitudes | Test, fase observada, respuesta/exception y estado de base. |
| V05 | CA01–CA05 | Integrada: build y suite focalizada | Cambios integrados; SDK y dependencias restaurados | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter FullyQualifiedName~DevelopmentIdentity --verbosity minimal` y `dotnet build Linkubator.sln --no-restore --verbosity minimal` | Tests focalizados y build correctos | Comandos, versión SDK, códigos de salida y recuentos reales. |

V01 y V03 no sustituyen la comprobación de la propiedad de cada operación privada, que pertenece a los casos de uso y a sus criterios propietarios. V04 verifica el límite de arranque de S5.7; no sustituye las comprobaciones integradas de persistencia de S5.8 ni las revisiones manuales de S5.9/S5.10. El test de petición arbitraria de V03 solo se añade si puede probarse en el límite existente sin implementar una ruta nueva ni ampliar el alcance.

## Orden de ejecución y cierre

- Crear `tasks.md` solo tras la aprobación explícita de este plan; las tareas asignarán checks inmediatos a todos los criterios y no dependerán de S5.8.
- La aprobación del plan no aprueba las tareas ni autoriza la implementación. No iniciar cambios productivos hasta que `tasks.md` exista y DLG lo apruebe.
- El cierre requiere evidencia satisfactoria de CA01–CA05 y aceptación humana explícita; no declara completada la etapa S5 ni sustituye las responsabilidades de S5.8–S5.10.
- Ante un cambio de reglas de identidad, detener la tarea afectada y tramitar la modificación en su documento propietario antes de continuar.
