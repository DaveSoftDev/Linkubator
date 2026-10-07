# Tareas: S0.6 — Configuración local de entorno

## Referencias y autorización

- [Especificación](spec.md): aprobada por DLG el 07-X-2026.
- [Plan técnico](plan.md): aprobado por DLG el 07-X-2026.
- Aprobación registrada por instrucción expresa de DLG: «Configuración OK» para cada documento S0.6.
- La reescritura es prospectiva; las comprobaciones registran el estado actual y no atribuyen fecha histórica a la implementación.

## Descomposición

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Aprobar especificación y plan. | [CA-01 a CA-05](spec.md#criterios-de-aceptación); [diseño](plan.md#diseño-de-implementación) | Ninguna | Aprobación explícita de DLG para ambos documentos. | completada | DLG, 07-X-2026: «Configuración OK», instrucción expresa de esta conversación. |
| T02 | Definir el origen público local. | [CA-01](spec.md#criterios-de-aceptación); [V-01](plan.md#estrategia-de-validación) | T01 | Parsear JSON y comprobar presencia solo en configuración Development. | completada | 07-X-2026: ambos JSON parsean; el origen está solo en Development y su forma es absoluta HTTP(S), autoridad localhost/loopback y sin path/query. Valor omitido deliberadamente. |
| T03 | Definir la ruta de base SQLite local. | [CA-02](spec.md#criterios-de-aceptación); [V-02](plan.md#estrategia-de-validación) | T01 | Revisar proveedor y destino sin crear/abrir la base. | completada | 07-X-2026: `ConnectionStrings:Default` contiene un `Data Source` relativo a archivo SQLite local, no UNC; el archivo no existe actualmente. No se abrió ni creó la BD y no se imprimió la ruta. |
| T04 | Definir email de desarrollo. | [CA-03](spec.md#criterios-de-aceptación); [V-01](plan.md#estrategia-de-validación) | T01 | Comprobar que el dato está bajo la configuración local. | completada | 07-X-2026: el email está bajo `Development` únicamente en `appsettings.Development.json`; el dominio corresponde a un dominio local/reservado. No se reprodujo el valor. |
| T05 | Verificar distribución y ausencia de secretos. | [CA-04](spec.md#criterios-de-aceptación); [V-03](plan.md#estrategia-de-validación) | T02, T03, T04 | Comparar configuración base y Development; confirmar separación y no presencia de credenciales. | completada | 07-X-2026: `appsettings.json` no contiene `PublicOrigin`, `ConnectionStrings` ni `Development`; revisión de nombres de claves no detectó claves de contraseña/token/secret/API key/clave privada. `.gitignore` declara exclusión de `appsettings.Development.json`, `*.db`, `*.sqlite` y `*.sqlite3`. Revisión textual, sin inspeccionar Git ni afirmar que una BD real esté excluida por Git. |
| T06 | Ejecutar build y pruebas disponibles. | [CA-05](spec.md#criterios-de-aceptación); [V-04 a V-06](plan.md#estrategia-de-validación) | T02, T03, T04, T05 | Build limpio y suite completa con resultado verde; anotar alcance de tests de S0.7. | completada | 07-X-2026, Windows/.NET SDK 10.0.401: `dotnet build Linkubator.sln --nologo --verbosity minimal` terminó con 0 advertencias y 0 errores; Tailwind ejecutó como parte del build. `dotnet test Linkubator.sln --no-build --no-restore --logger 'console;verbosity=minimal'`: 3 superadas, 0 fallidas, 0 omitidas. `WebStartupTests.HomePageReturnsSuccess`: 1 superada. El aviso de puerto HTTPS desconocido en TestServer no prueba ni refuta el HTTPS live de S0.5. |
| T07 | Revisar evidencia, aceptar resultado y archivar. | [puerta de salida](plan.md#orden-de-ejecución-y-puerta-de-salida) | T06 | Aceptación expresa de DLG; traslado y comprobación de rutas/enlaces. | completada | DLG, 07-X-2026: «puedes archivar S0.6». Carpeta trasladada; enlaces y ubicación comprobados tras el movimiento. |

La base local, los scripts SQL y el usuario de desarrollo no se crean en S0.6; corresponden a persistencia posterior. `AllowedHosts` corresponde a S0.5 según el plan principal y la enmienda posterior documentada en [S0.5 → «Criterios de aceptación»](../s0.5-configuracion-web/spec.md#criterios-de-aceptación).

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA-01 | T02, T05, T06 | [V-01](plan.md#estrategia-de-validación) | Conforme | 07-X-2026; Windows; JSON parseado. | Origen local presente solo en Development y de forma HTTP(S) localhost/loopback; valor omitido. |
| CA-02 | T03, T05, T06 | [V-02](plan.md#estrategia-de-validación) | Conforme para la configuración; la BD aún no existe | 07-X-2026; Windows. | Cadena parseada: Data Source relativa, archivo SQLite local no UNC; BD no abierta ni creada. |
| CA-03 | T04, T05, T06 | [V-01](plan.md#estrategia-de-validación) | Conforme | 07-X-2026; JSON parseado. | Email de desarrollo solo en Development, dominio local/reservado; valor omitido. |
| CA-04 | T02–T05, T06 | [V-01 a V-03](plan.md#estrategia-de-validación) | Conforme dentro del alcance | 07-X-2026; revisión estructural de archivos y `.gitignore`. | Valores de S0.6 no están en configuración base; no se hallaron claves de secretos por nombres; patrones locales declarados en `.gitignore`. No se usó Git ni se verificó exclusión de un fichero real. |
| CA-05 | T06 | [V-04 a V-06](plan.md#estrategia-de-validación) | Conforme | 07-X-2026; Windows; .NET SDK 10.0.401. | Build: 0 warnings/0 errors; suite: 3/3; test de arranque: 1/1. |

## Aceptación y cierre

- Estado del trabajo: archivado; documentos aprobados; CA-01 a CA-05 verificados; T01–T07 completadas; resultado aceptado por DLG.
- Criterios sin evidencia satisfactoria registrada: ninguno dentro de los límites indicados. CA-02 acredita la ruta configurada, no la existencia o conectividad de la BD.
- Bloqueos y riesgos: no queda discrepancia documental sobre la asignación de `AllowedHosts`; la validación adicional de S0.5 está registrada como enmienda posterior y no pertenece al alcance de S0.6. La base SQLite no existe y no se probó acceso a datos.
- Aprobación documental: DLG, 07-X-2026, «Configuración OK» para `spec.md` y `plan.md`.
- Aceptación del resultado: DLG, 07-X-2026, «Configuración OK»; instrucción expresa «puedes archivar S0.6» tras validaciones satisfactorias.
- Archivado: completado el 07-X-2026 en `specs/archive/s0.6-configuracion-local/`.
- Comprobación de enlaces: 07-X-2026; referencias locales entrantes/salientes e índice verificados después del traslado; la carpeta activa ya no existe.
- Siguiente paso: ninguno para S0.6. La verificación live añadida por la enmienda de S0.5 no reabre ni cambia la aceptación histórica de S0.6.
