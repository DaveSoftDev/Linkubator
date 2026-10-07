# Tareas: S0.3 — Configuración base del repositorio

## Referencias y autorización

- [Especificación](spec.md): aprobada por DLG el 7-X-2026 mediante la instrucción «Cumple lo esperado» de esta conversación.
- [Plan técnico y validaciones](plan.md): aprobado por DLG el 7-X-2026 mediante la misma instrucción.
- Las aprobaciones documentales no acreditan que se hayan ejecutado los entregables. La validación posterior comprueba el estado presente, no su fecha histórica de creación.

## Descomposición

Estados: pendiente, en curso, completada o bloqueada. Las tareas de esta reescritura están redactadas como si precedieran al código, conforme al orden del [plan de implementación](../../../plans/mvp0-plan.md#s0-fundaciones-m).

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar la [especificación](spec.md) y el [plan](plan.md). | [CA01 a CA04](spec.md#criterios-de-aceptación); [diseño](plan.md#diseño-de-implementación) | Ninguna | Aprobaciones humanas expresas registradas. | completada | DLG, 7-X-2026: «Cumple lo esperado» para los documentos S0.3; no acredita implementación histórica. |
| T02 | Habilitar `nullable` y analizadores en los proyectos. | [CA01](spec.md#criterios-de-aceptación); [V01](plan.md#estrategia-de-validación) | T01 | Consultar propiedades efectivas de los proyectos y comprobar análisis y nulabilidad. | completada | V01, 7-X-2026: `dotnet msbuild <proyecto> -getProperty:EnableNETAnalyzers,AnalysisLevel,Nullable -nologo` devuelve `true`, `latest` y `enable` en los cinco proyectos; SDK 10.0.401. La comprobación no acredita cuándo se configuraron. |
| T03 | Definir el estilo compartido en `.editorconfig`. | [CA02](spec.md#criterios-de-aceptación); [V02](plan.md#estrategia-de-validación) | T01 | Revisar secciones y reglas de formato aplicables. | completada | V02, 7-X-2026: `.editorconfig` de raíz define ámbito raíz, UTF-8, LF, nueva línea final, sangría por espacios y secciones para C# y proyectos; revisión de reglas declaradas, no prueba de cada editor. |
| T04 | Definir las exclusiones locales en `.gitignore`. | [CA03](spec.md#criterios-de-aceptación); [V03](plan.md#estrategia-de-validación) | T01 | Comprobar cobertura de los patrones y conservación de fuentes. | completada | V03, 7-X-2026: patrones para bin/obj, logs, SQLite y configuración local revisados; DLG confirmó que logs, artefactos de compilación y configuración local quedan fuera de Git y que `.cs`/`.csproj` se incluyen. No hay base de datos todavía: su exclusión solo se comprueba por el patrón declarado, no por observación en Git. |
| T05 | Compilar y consolidar evidencia de S0.3. | [CA01 a CA04](spec.md#criterios-de-aceptación); [V01 a V04](plan.md#estrategia-de-validación) | T02, T03, T04 | Build sin advertencias ni errores; evidencia suficiente por criterio. | completada | V04, 7-X-2026: `dotnet build Linkubator.sln --nologo --verbosity minimal` terminó con 0 advertencias y 0 errores; `dotnet test Linkubator.sln --no-restore --logger 'console;verbosity=normal'` pasó 3/3. CA01 a CA04 consolidados a continuación. |
| T06 | Revisar los resultados y solicitar aceptación del trabajo. | [Criterios](spec.md#criterios-de-aceptación); [puerta de salida](plan.md#orden-de-ejecución-y-puerta-de-salida) | T05 | Aceptación humana solo con evidencia completa y sin bloqueos. | completada | DLG, 7-X-2026: «Cumple lo esperado» y «Confirmado» tras solicitar evidencia de CA03. Aceptación del estado comprobado; no acredita fecha histórica de implementación ni observación de una BD inexistente. |

T02, T03 y T04 podían verificarse en paralelo tras T01. Las verificaciones actuales no acreditan cuándo se creó la configuración. No se atribuyen a S0.3 tareas de registro de eventos, interfaz web ni valores de desarrollo. La base de datos aún no existe: no se afirma que Git haya excluido un fichero real.

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T05 | [V01](plan.md#estrategia-de-validación) | Conforme: analizadores del SDK habilitados y `nullable` activo en cinco proyectos. | 7-X-2026; .NET SDK 10.0.401, Windows. | Valores de MSBuild: `EnableNETAnalyzers=true`, `AnalysisLevel=latest`, `Nullable=enable` por proyecto. |
| CA02 | T03, T05 | [V02](plan.md#estrategia-de-validación) | Conforme como configuración declarada: `.editorconfig` raíz con reglas compartidas. | 7-X-2026; revisión de archivo de raíz. | Ámbito raíz, UTF-8, LF, nueva línea final, espacios y secciones C#/proyectos; sin ejecución de un editor. |
| CA03 | T04, T05 | [V03](plan.md#estrategia-de-validación) | Conforme dentro del alcance: patrones declarados; confirmación de DLG sobre logs, artefactos de compilación y configuración local excluidos, y `.cs`/`.csproj` incluidos. | 7-X-2026; revisión de `.gitignore` y confirmaciones manuales de DLG en esta conversación. | Patrones de bin/obj, logs, SQLite y configuración local; sin BD existente, la cobertura de datos SQLite es declarada, no verificada en Git. |
| CA04 | T05 | [V04](plan.md#estrategia-de-validación) | Conforme: compilación sin errores ni advertencias. | 7-X-2026; .NET SDK 10.0.401, Windows. | `dotnet build Linkubator.sln --nologo --verbosity minimal`: 0 advertencias, 0 errores; tests de solución 3/3 correctos como corroboración de la DoD. |

Una salida de build satisfactoria no verifica automáticamente las reglas de `.gitignore` ni la aplicación por el editor de `.editorconfig`. Registrar limitaciones y no presentar evidencia indirecta como prueba de otro mecanismo.

## Aceptación y cierre

- Estado del trabajo: resultado aceptado por DLG y archivado el 7-X-2026.
- Criterios sin evidencia satisfactoria registrada: ninguno, dentro de los límites consignados para CA03.
- Bloqueos y riesgos restantes: la BD aún no existe; su exclusión se sustenta en el patrón de `.gitignore` y no en una observación de Git. Los tests mostraron un aviso de redirección HTTPS ajeno al build de S0.3, que terminó sin advertencias.
- Aceptación del resultado: DLG, 7-X-2026, «Cumple lo esperado»; solicitud previa de aprobación y archivo de S0.3 y confirmación «Confirmado» sobre las exclusiones y fuentes en esta conversación.
- Archivado: completado el 7-X-2026 en `specs/archive/s0.3-configuracion-base/`; la carpeta activa ya no existe.
- Comprobación de enlaces: 7-X-2026; 9 enlaces entrantes y 57 enlaces/anclas relacionados resueltos, sin referencias a la ruta anterior.
- Siguiente paso: continuar con el siguiente trabajo del plan; este cierre no acepta otros bloques.