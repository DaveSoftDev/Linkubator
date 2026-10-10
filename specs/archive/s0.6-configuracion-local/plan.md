# Plan técnico: S0.6 — Configuración local de entorno

## Estado y aprobación del plan

- Estado: aprobado.
- Especificación: [spec.md](spec.md).
- Aprobaciones:
  - Especificación: DLG, 07-X-2026 («Configuración OK»).
  - Plan y tareas: DLG, 07-X-2026 («Configuración OK»).

## Diseño de implementación

### Componentes y contratos

La configuración de Development de la aplicación Web suministra el origen público, la ruta SQLite local y la dirección de correo de desarrollo. ASP.NET Core combina configuración base y configuración específica del entorno; este trabajo mantiene separados ambos ámbitos y no añade resolución de secretos ni credenciales.

El formato y la ubicación del usuario de desarrollo se enlazan con [architecture.md → «Autenticación»](../../../context/architecture.md#autenticación) y [decisions.md → «Identidad de desarrollo»](../../../context/decisions.md#identidad-de-desarrollo).

### Flujos internos

1. ASP.NET Core carga la configuración base y las opciones de Development.
2. La aplicación obtiene los valores locales desde el proveedor de configuración de Development.
3. La conexión SQLite indica un archivo local; su creación y su esquema quedan a cargo del trabajo de persistencia.
4. La validación comprueba JSON, ubicación y distribución de los valores, sin escribir datos ni conectarse a servicios compartidos.

### Persistencia y dependencias

- No se crea ni se abre una base de datos en S0.6; el criterio verifica únicamente la cadena de conexión y su destino local.
- La ejecución depende del host ASP.NET Core y de la configuración cargada desde archivos.
- S5 define proveedor SQLite, scripts y datos locales; no se adelantan esos entregables.

### Garantías técnicas

- Los valores específicos de desarrollo no se incorporan a `appsettings.json` ni se interpretan como configuración de producción.
- No se almacenan contraseñas, tokens ni credenciales en los archivos versionados.
- No se cambia el ajuste de `AllowedHosts` ni la configuración de despliegue.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Mantener valores locales en el proveedor de configuración Development | Confirmada por la separación de entorno descrita en la arquitectura | [architecture.md → «Tecnologías»](../../../context/architecture.md#tecnologías) | Evita mezclar valores de ejecución local con configuración base. |
| S0.6 valida la ruta, no crea la base SQLite | Confirmada por el reparto S0/S5 | [mvp0-plan.md → «Resumen de sprints»](../../../plans/mvp0-plan.md#2-resumen-de-sprints) | El arranque del host no prueba conectividad con SQLite. |

## Estrategia de validación

Registrar todas las comprobaciones en [tasks.md](tasks.md). Las pruebas de arranque existentes pertenecen a S0.7 y sirven solo como evidencia corroborativa.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01, CA03, CA04 | Revisión técnica/JSON | Archivos appsettings presentes | Parsear `appsettings.json` y `appsettings.Development.json`; revisar que las claves de origen y correo solo estén en Development. | JSON válido y valores locales en el archivo de entorno correcto. | Resultado estructural sin copiar valores potencialmente locales al informe. |
| V02 | CA02, CA04 | Revisión técnica | Archivo de entorno disponible | Interpretar la cadena `ConnectionStrings:Default` y verificar proveedor/ruta de archivo local; no crear ni abrir la BD. | Destino SQLite local, no compartido ni de producción. | Tipo de origen y propiedad local, sin volcar datos innecesarios. |
| V03 | CA04 | Revisión de seguridad documental | Configuración base y local presentes | Comprobar que la configuración base no contiene claves de S0.6 ni secretos; revisar `.gitignore` solo como texto si es necesario, sin acceder a directorios ocultos ni Git. | Separación correcta y sin secretos declarados. | Claves examinadas y hallazgos, no valores de entorno. |
| V04 | CA05 | Build | SDK .NET 10 | `dotnet build Linkubator.sln --nologo --verbosity minimal` | Código compila sin errores ni advertencias. | Resumen de build, SDK y fecha. |
| V05 | CA05 | Pruebas automatizadas | Build correcto; runner VSTest/xUnit disponible | `dotnet test Linkubator.sln --no-build --no-restore --logger 'console;verbosity=minimal'` | Todas las pruebas descubiertas pasan. | Total/pasadas/fallidas y fecha; no atribuir tests de S0.7 a S0.6. |
| V06 | CA01, CA03, CA05 | Corroboración de arranque | Host y perfil local disponibles | Ejecutar `WebStartupTests.HomePageReturnsSuccess` si la suite lo incluye; documentar que `WebApplicationFactory` no prueba lectura de BD ni conectividad externa. | El host responde sin excepción con configuración disponible. | Resultado del test y límites de lo que demuestra. |

Las pruebas automatizadas disponibles no acreditan que exista una base de datos, que haya usuario provisionado ni que se envíe correo. No se añadirá tal comportamiento para esta validación.

## Orden de ejecución y puerta de salida

- [Tareas](tasks.md): aprobación documental, comprobaciones de configuración, build, suite y aceptación/archivo.
- Cerrar cuando CA01 a CA05 tengan evidencia satisfactoria y DLG acepte explícitamente el resultado.
- Ante una contradicción que afecte la configuración local o una petición de cambiar `AllowedHosts`, detener esa modificación y actualizar primero su fuente propietaria.
- Archivar tras aceptar el resultado y verificar enlaces y referencias.
