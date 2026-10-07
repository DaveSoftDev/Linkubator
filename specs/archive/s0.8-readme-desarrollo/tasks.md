# Tareas: S0.8 — README de desarrollo

## Referencias y autorización

- [Especificación](spec.md): aprobada por DLG el 7-X-2026.
- [Plan técnico](plan.md): aprobado por DLG el 7-X-2026.
- Referencia: instrucción de DLG «Readme completado» para cada documento S0.8.
- Las aprobaciones documentales no demuestran que la guía haya sido escrita antes del código ni sustituyen la aceptación del resultado.

## Descomposición

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Aprobar especificación y plan. | [CA01 a CA05](spec.md#criterios-de-aceptación); [diseño](plan.md#diseño-de-implementación) | Ninguna | Aprobación de DLG registrada para ambos documentos. | completada | DLG, 7-X-2026: «Readme completado», instrucción expresa para cada documento S0.8. |
| T02 | Revisar contenido, estructura y comandos del README. | [CA01, CA02](spec.md#criterios-de-aceptación); [V01, V02](plan.md#estrategia-de-validación) | T01 | Contrastar proyectos, comandos y perfil local con la solución, archivos y ayuda del SDK. | completada | 7-X-2026: README incluye propósito, estructura, requisitos, restore/build/test y arranque Web. Rutas de proyectos y enlaces revisados; `dotnet run` corregido para seleccionar perfil `https`, confirmado por `launchSettings.json` y `dotnet run --help`. Tailwind standalone está presente en la ruta documentada. No se arrancó una segunda app. |
| T03 | Verificar configuración local, logs y privacidad del texto. | [CA03, CA04](spec.md#criterios-de-aceptación); [V03](plan.md#estrategia-de-validación) | T01 | Contrastar descripción con la configuración, sin revelar valores ni afirmar que la BD existe. | completada | 7-X-2026: README enlaza el archivo Development como no versionado y remite a S0.6; base/Development, perfil y configuración Serilog inspeccionados sin copiar valores. El texto dice que la base aún no se ha creado; configuración apunta a un fichero local. No se inspeccionó contenido de logs. |
| T04 | Verificar enlaces del README. | [CA05](spec.md#criterios-de-aceptación); [V04](plan.md#estrategia-de-validación) | T02, T03 | Resolver todos los enlaces relativos locales del README. | completada | 7-X-2026: 23 enlaces locales examinados; 0 rutas rotas. |
| T05 | Ejecutar build y suite completa. | [CA05](spec.md#criterios-de-aceptación); [V05, V06](plan.md#estrategia-de-validación) | T02–T04 | Build limpio y todos los tests descubiertos pasan. | completada | 7-X-2026, Windows/.NET SDK 10.0.401: `dotnet build Linkubator.sln --nologo --verbosity minimal` terminó con 0 warnings y 0 errores; Tailwind v4.3.3 ejecutó. `dotnet test Linkubator.sln --no-build --no-restore --logger 'console;verbosity=minimal'`: 3 pasadas, 0 fallidas, 0 omitidas. |
| T06 | Aceptar resultado y archivar. | [puerta de salida](plan.md#orden-de-ejecución-y-puerta-de-salida) | T05 | Aceptación de DLG según autorización condicional; movimiento y enlaces verificados. | completada | DLG, 7-X-2026: «Readme completado». Carpeta movida a `specs/archive/s0.8-readme-desarrollo/`; enlaces entrantes/salientes, destino y ausencia de ruta activa verificados. |

El chequeo del README no ejecuta la aplicación ni crea base de datos; la claridad editorial se determina mediante revisión del documento. Cualquier aviso del test host HTTPS no constituye una validación de HTTPS live.

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T05 | [V01](plan.md#estrategia-de-validación) | Conforme por revisión editorial | 7-X-2026; repositorio en Windows. | README contiene propósito, estructura de proyectos y requisitos locales. |
| CA02 | T02, T05 | [V02](plan.md#estrategia-de-validación) | Conforme | 7-X-2026; .NET SDK 10.0.401. | Comandos presentes; opciones y perfil `https` contrastados con herramientas y archivos actuales. |
| CA03 | T03, T05 | [V03](plan.md#estrategia-de-validación) | Conforme | 7-X-2026; revisión estructural de configuración. | Rutas de configuración descritas; valores locales no copiados; estado de BD correctamente expresado. |
| CA04 | T03, T05 | [V03](plan.md#estrategia-de-validación) | Conforme con límite de verificación | 7-X-2026; appsettings/README revisados. | La guía coincide con rutas y sinks configurados; no se inspeccionó el contenido real de logs. |
| CA05 | T04, T05 | [V04 a V06](plan.md#estrategia-de-validación) | Conforme | 7-X-2026; .NET SDK 10.0.401, Windows. | 23 enlaces/0 rotos; build 0/0; suite 3/3. |

## Aceptación y cierre

- Estado del trabajo: archivado; documentos aprobados; CA01 a CA05 verificados dentro de los límites anotados; T01–T06 completadas y resultado aceptado por DLG.
- Criterios sin evidencia satisfactoria registrada: ninguno en alcance; el contenido real de los logs no fue auditado y no se atribuye a la guía.
- Bloqueos y riesgos: no se arrancó la app durante la revisión; el README documenta el perfil HTTPS. La BD aún no existe y se describe como destino.
- Aprobación documental: DLG, 7-X-2026, «Readme completado» para `spec.md` y `plan.md`.
- Aceptación del resultado: DLG, 7-X-2026, «Readme completado», en aplicación de la instrucción expresa de aprobar la tarea si sus validaciones pasan y archivar.
- Archivado: completado el 7-X-2026 en `specs/archive/s0.8-readme-desarrollo/`.
- Comprobación de enlaces: 7-X-2026; enlaces locales entrantes/salientes en cinco documentos, destino con tres archivos y ausencia de la ruta activa verificados.
- Siguiente paso: ninguno para S0.8.
