# Plan técnico: S0.8 — README de desarrollo

## Estado y aprobación del plan

- Estado: aprobado.
- Especificación: [spec.md](spec.md).
- Aprobaciones:
  - Especificación: DLG, 07-X-2026 («Readme completado»).
  - Plan y tareas: DLG, 07-X-2026 («Readme completado»).

## Diseño de implementación

### Componentes y contratos

El README de raíz sirve de guía operativa para el repositorio. Debe enlazar a `context/`, `plans/`, `specs/` y archivos locales cuando sea necesario, sin duplicar reglas funcionales ni valores de configuración. La estructura de proyectos se contrasta con `Linkubator.sln` y los directorios productivos/test existentes.

Los comandos se ejecutan desde la raíz y usan los proyectos/runners reales del repositorio. La configuración local se describe por ubicación y propósito; no se copian sus valores. El archivo SQLite se menciona como destino indicado por la configuración, sin afirmar que exista.

### Flujos internos

1. Revisar el estado del README, la solución, perfiles Web, appsettings y pruebas.
2. Contrastar los comandos documentados con la ayuda del SDK y la estructura de proyectos.
3. Comprobar que los enlaces Markdown a documentos y rutas locales resuelven.
4. Ejecutar build y suite de pruebas como validación integrada del repositorio.
5. Registrar evidencia y sus límites en `tasks.md`.

### Persistencia y dependencias

- El README no crea ni abre la base local.
- Depende de la solución .NET, configuración Web, pruebas existentes y contenido documental enlazado.
- No requiere servicios, credenciales ni entorno productivo.

### Garantías técnicas

- No se muestran valores de `appsettings.Development.json`, logs ni archivos de datos locales.
- Los comandos deben seguir ejecutables desde el repositorio con el SDK .NET instalado.
- Una prueba automatizada de build/test no verifica por sí sola la claridad editorial; se revisa manualmente la estructura del README.
- No se introduce información de despliegue ni capacidades futuras como ya disponibles.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Mantener una guía operativa de raíz con enlaces a fuentes técnicas | Confirmada por la definición de S0.8 | [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m) | Evita replicar reglas funcionales en el README. |
| El archivo SQLite configurado puede no existir aún | Observación de alcance | [S0.6 → CA02](../s0.6-configuracion-local/spec.md#criterios-de-aceptación) | Describir el destino configurado, no su existencia ni datos. |
| Una tabla/árbol de directorios puede quedar obsoleta al archivar specs | Riesgo editorial | Estructura del repositorio | Comprobar índice y árbol README después del movimiento de S0.8. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Revisión técnica/editorial | README y árbol existentes | Contrastar listado de proyectos con `dotnet sln Linkubator.sln list` y directorios reales | Propósito y estructura coinciden con el repositorio. | Proyectos listados, fecha, sin asumir cronología de README. |
| V02 | CA02 | Revisión de comandos | SDK y perfil Web disponibles | Verificar opciones con `dotnet restore --help`, `dotnet build --help`, `dotnet test --help`, `dotnet run --help`; confirmar rutas de proyecto y perfiles | Los comandos documentados son sintácticamente válidos y aplicables. | Opciones/rutas contrastadas. |
| V03 | CA03, CA04 | Revisión técnica/privacidad | Archivos config y logger disponibles | Revisar referencias a appsettings/logs/DB en README contra configuración y `.gitignore`; no volcar valores locales | Ubicaciones y estados correctamente descritos, sin valores sensibles. | Claves/archivos revisados y discrepancias. |
| V04 | CA05 | Enlaces Markdown | Documentos y rutas referenciados presentes | Resolver enlaces relativos locales en `README.md` | Todos los enlaces resuelven. | Número y resultado del chequeo. |
| V05 | CA05 | Build | SDK instalado | `dotnet build Linkubator.sln --nologo --verbosity minimal` | Cero warnings/errors. | Resumen, SDK y fecha. |
| V06 | CA05 | Tests | Build correcto; xUnit/VSTest | `dotnet test Linkubator.sln --no-build --no-restore --logger 'console;verbosity=minimal'` | Todos los tests descubiertos pasan. | Total/pass/fail/skip y fecha. |

El build puede ejecutar Tailwind según la configuración disponible. Los tests no validan que el README sea claro; esa parte se comprueba mediante lectura técnica. No se ejecutará `dotnet run` como parte de la validación para evitar procesos persistentes/puertos; la sintaxis y el perfil se verifican contra `launchSettings.json` y `--help`.

## Orden de ejecución y puerta de salida

- [Tareas](tasks.md) relaciona edición/revisión, enlaces, build, tests y aceptación.
- Cerrar con criterios respaldados por revisión/evidencia, sin valores locales expuestos, y aceptación explícita de DLG.
- Archivar tras aceptación y después de actualizar el índice y las referencias entrantes/salientes.
