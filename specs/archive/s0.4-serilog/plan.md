# Plan técnico: S0.4 — Serilog y observabilidad base

## Estado y aprobación del plan

- Estado: aprobado.
- Especificación: [spec.md](spec.md).
- Aprobaciones:
  - Especificación: DLG, 07-X-2026 («Cumple expectativas»).
  - Plan y tareas: DLG, 07-X-2026 («Cumple expectativas»). La aprobación documental no acredita la ejecución ni aceptación de las tareas.

## Diseño de implementación

### Componentes y contratos

Sobre la solución preparada en [S0.3](../s0.3-configuracion-base/spec.md) (trabajo archivado), eliminar la necesidad de un logger ad hoc y dejar Serilog como proveedor del host de ASP.NET Core. La capa Web registra el host y mantiene la separación de capas: `Program.cs` usa `UseSerilog`, pero no introduce reglas de negocio ni persistencia. La configuración vive en `appsettings.json` y `appsettings.Development.json`, con los niveles y la salida en un directorio local de logs fuera del repositorio.

### Flujos internos

1. Registrar Serilog con `UseSerilog` en el host antes de construir la aplicación.
2. Configurar `ReadFrom.Configuration` y `ReadFrom.Services` para usar la configuración del entorno.
3. Definir `WriteTo` para consola y archivo rotativo con nivel mínimo configurable.
4. Validar el contenido del logger y compilar la solución con evidencias del trabajo.

### Persistencia y dependencias

No se crea persistencia ni dependencias de dominio. El logger es un componente de infraestructura del host, con salidas de consola y archivo. La ruta del log se almacena bajo `logs/` y se excluye del repositorio por la regla general de `.gitignore`, no por un cambio funcional del producto.

### Garantías técnicas

- Serilog opera detrás de `ILogger` y no modifica la lógica del dominio.
- La configuración sigue viva en `appsettings`, sin hardcodear rutas o niveles.
- La salida de logs no incluye secrets ni contenido privado del usuario.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado | Fuente o referencia | Comprobación o resolución |
| --- | --- | --- | --- |
| Registrar Serilog con el host de ASP.NET Core | Diseño técnico aprobado | [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m) | Verificar `UseSerilog` en `Program.cs` y configuración del entorno. |
| Archivo rotativo bajo `logs/` | Diseño técnico aprobado | [architecture.md → «Logs»](../../../context/architecture.md#logs) | Comprobar `logs/` en `appsettings` y la regla de `.gitignore`. |
| No registrar contenido sensible | Requisito técnico crítico | [specifications.md → «Registro de eventos»](../../../context/specifications.md#registro-de-eventos) | Revisión del contenido de mensajes y de la configuración. |

## Estrategia de validación

Los resultados van en [tasks.md](tasks.md). La validación se basa en la configuración efectiva del host y en la compilación de la solución; no sustituye la revisión funcional del dominio ni de la capa de aplicación.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comprobación | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Configuración del host | Solución construida y SDK disponible | Verificar `UseSerilog` y `ReadFrom.Configuration` en `Program.cs` | Serilog activo como proveedor del host de ASP.NET Core. | Captura del `Program.cs` y entorno de arranque. |
| V02 | CA02 | Configuración externa | Archivos `appsettings` existentes | Revisar `WriteTo` y `MinimumLevel` en `appsettings*.json` | Consola + archivo rotativo + nivel configurable por entorno. | Archivos de configuración y ruta local de logs. |
| V03 | CA03 | Revisión de contenido | Logs de ejecución disponibles | Revisar mensajes y contenido emitido | Sin contraseñas, tokens o datos privados. | Resultado de la revisión de contenido. |
| V04 | CA04 | Compilación integrada | SDK y dependencias disponibles | `dotnet build Linkubator.sln --nologo --verbosity minimal` y `dotnet test Linkubator.sln --no-restore --logger 'console;verbosity=normal'` | Build sin errores ni advertencias y pruebas correctas. | Comando, salida y resultado de la solución. |

## Orden de ejecución y puerta de salida

- [Tareas](tasks.md): aprobación documental, comprobaciones técnicas y cierre del trabajo.
- Cerrar solo con evidencia suficiente para CA01 a CA04, tareas completadas y aceptación humana explícita.
- No se declara la tarea aceptada si la comprobación de contenido sensible o del build está pendiente.