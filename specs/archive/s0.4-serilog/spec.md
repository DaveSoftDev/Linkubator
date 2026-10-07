# Especificación: S0.4 — Serilog y observabilidad base

## Estado y aprobación

- Estado: Aprobado por DLG el 7-X-2026.
- Referencia: Cumple expectativas.
- Este documento describe el resultado esperado; la evidencia obtenida tras construirlo pertenece a [tasks.md](tasks.md).

## Objetivo

Integrar Serilog en la capa Web con salida por consola y archivo rotativo, manteniendo la configuración en `appsettings` y la ruta local de logs fuera del repositorio, sin introducir lógica de negocio ni depender de ningún caso de uso.

## Fuentes

- [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m): trabajo S0.4 y su Definition of Done.
- [architecture.md → «Logs»](../../../context/architecture.md#logs): principios de observabilidad y restricciones del registro.
- [architecture.md → «Plataforma»](../../../context/architecture.md#plataforma): uso de Serilog como proveedor de logs.
- [AGENTS.md → «Reglas de documentación»](../../../AGENTS.md#reglas-de-documentación): límites y trazabilidad de este documento.

## Alcance

- Incluye: integración de Serilog en la capa Web, configuración externa desde `appsettings`, salida por consola y archivo rotativo, registro con `ILogger` y validación de contenido sensible.
- Excluye: lógica de negocio, reglas del dominio, reglas de proceso ni almacenamiento de secretos en el repositorio.
- No se exige en este trabajo que cada recomendación del logger se convierta en error del build; la compilación sin advertencias se comprueba con la Definition of Done del plan principal.

## Dependencias y prerrequisitos

- La solución y la infraestructura base están preparadas según [S0.3](../s0.3-configuracion-base/spec.md) (trabajo archivado).
- La capa Web de ASP.NET Core ya existe en la estructura entregada en [S0.2](../s0.2-crear-solucion/spec.md).

## Criterios de aceptación

| ID | Fuente propietaria | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [Plan → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m) | La aplicación ASP.NET Core y la solución preparada en S0.2/S0.3. | Se arranca la aplicación con la configuración del host. | Serilog queda registrado como proveedor de logs del host y `ILogger` sigue siendo el canal de registro usado por la capa Web. |
| CA02 | [Plan → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m) | Los archivos de configuración del proyecto. | Se revisa la configuración del entorno. | `appsettings.json` y `appsettings.Development.json` configuran al menos consola y archivo rotativo con una ruta local de logs y nivel mínimo configurable. |
| CA03 | [specifications.md → «Registro de eventos»](../../../context/specifications.md#registro-de-eventos) | El sistema en ejecución. | Se comprueba el contenido de los logs. | Los mensajes no incluyen contraseñas, tokens, secretos ni contenido privado del usuario. |
| CA04 | [Plan → «3. Definition of Done común (aplica a todos los sprints)»](../../../plans/mvp0-plan.md#3-definition-of-done-común-aplica-a-todos-los-sprints) | La solución con la configuración base correcta. | Se compila y ejecuta la solución con el logger integrado. | El proyecto finaliza sin errores ni advertencias y la prueba de la solución sigue pasando. |

## Bloqueos y preguntas pendientes

- Las comprobaciones reales y el cierre se registran en [tasks.md](tasks.md); esta aprobación documental no reemplaza la evidencia del trabajo ejecutado.

## Artefactos relacionados

- [Plan técnico](plan.md).
- [Tareas y evidencia](tasks.md).