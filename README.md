# Linkubator

Aplicación web para guardar y organizar enlaces favoritos en colecciones, buscarlos y compartir de forma selectiva colecciones y enlaces públicos. Dispone de un área privada de gestión y páginas públicas renderizadas en servidor.

La base está preparada para separar los datos por usuario, pero actualmente el acceso privado utiliza una identidad de desarrollo compartida: **no hay login real y cualquier persona con acceso a la aplicación puede gestionar los datos del usuario demo**. No debe confundirse este modo con autenticación ni utilizarse para exponer datos privados a personas no autorizadas. El alcance y el riesgo aceptado están en [decisions.md → «Identidad de desarrollo»](context/decisions.md#identidad-de-desarrollo).

## Estado del proyecto

La gestión de colecciones y enlaces, la búsqueda y el área pública del MVP0 están implementadas. El [roadmap](context/roadmap.md) sitúa el refactor del área pública antes del cierre y hardening; no se declara el MVP0 terminado. Las reglas de producto y las invariantes de [AGENTS.md](AGENTS.md) siguen siendo obligatorias.

## Funcionalidades principales

- Dashboard con enlaces recientes, creación rápida y presentación en tarjetas o lista.
- Colecciones: crear, editar nombre y descripción, establecer su estado público o privado, buscar y eliminar cuando las reglas lo permiten.
- Enlaces: crear desde una URL, editar título, descripción e imagen, mover entre colecciones, eliminar y decidir su publicación. La URL guardada es inmutable y se detectan duplicados por usuario.
- Listados privados con búsqueda de texto mediante FTS5, filtro de enlaces por colección, orden y paginación.
- Landing, página pública de usuario, listado público de colecciones y páginas de colección con sus enlaces públicos, metadatos, canonical y tratamiento de rutas no accesibles.
- Formularios con validación, confirmación de eliminación, protección antiforgery y componentes de presentación accesible. La revisión global de calidad y accesibilidad sigue formando parte del cierre.
- Persistencia SQLite con migraciones al arrancar, operaciones transaccionales y aprovisionamiento automático del usuario demo y su colección inicial.

Los comportamientos exactos se definen en [specifications.md](context/specifications.md) y [domain-model.md](context/domain-model.md), no en este resumen.

## Stack tecnológico

| Área | Implementado actualmente |
|---|---|
| Plataforma | .NET 10 y C#, con nullable y advertencias tratadas como errores |
| Arquitectura | Domain, Application, Infrastructure y Web, con dependencias delimitadas |
| Web | ASP.NET Core Razor Pages, renderizado en servidor y JavaScript para la interacción |
| Estilos | Tailwind CSS mediante CLI standalone para Windows y hojas CSS del proyecto |
| Datos | SQLite, `Microsoft.Data.Sqlite`, Dapper y scripts SQL versionados |
| Búsqueda | SQLite FTS5, con proyecciones separadas para enlaces y colecciones |
| Registro | Serilog, con consola y archivo rotativo |
| Pruebas | xUnit, VSTest, `WebApplicationFactory`, NetArchTest y coverlet |

El proyecto ya incorpora Argon2id para el soporte criptográfico de la identidad demo; esto no implica que exista un flujo de login. La autenticación por cookies, MailKit y smtp4dev forman parte de la arquitectura prevista para las etapas de cuenta, no de los servicios necesarios para ejecutar la aplicación actual. Véase [architecture.md → «Tecnologías»](context/architecture.md#tecnologías).

## Documentación del proyecto

- [Requisitos de Linkubator](context/requirements.md)
- [Especificaciones de Linkubator](context/specifications.md)
- [Modelo de dominio de Linkubator](context/domain-model.md)
- [Arquitectura prevista para Linkubator](context/architecture.md)
- [Decisiones, riesgos, pendientes y fuera de alcance](context/decisions.md)
- [Roadmap de Linkubator](context/roadmap.md)
- [Guía para agentes y colaboradores](AGENTS.md)
- [Skill de revisión de requisitos](skills/requirements-review/SKILL.md)
- [Índice de specs](specs/specs-index.md)

## Estructura de la solución

Vista resumida de las carpetas actuales; se omiten archivos internos, metadatos de proyecto, salidas de compilación y logs. Las responsabilidades de cada capa se describen en [architecture.md → «Arquitectura»](context/architecture.md#arquitectura).

```text
.
├── .github/
├── context/
│   ├── architecture.md
│   ├── decisions.md
│   ├── domain-model.md
│   ├── requirements.md
│   ├── roadmap.md
│   ├── specifications.md
│   └── diagrams/
├── data/
├── plans/
│   ├── mvp0-acceptance-criteria.md
│   ├── mvp0-plan.md
│   └── test-audit-plan.md
├── skills/
├── specs/
│   ├── specs-index.md
│   └── archive/
├── src/
│   ├── Linkubator.Domain/
│   │   ├── Constants/
│   │   ├── Entities/
│   │   │   └── Bases/
│   │   ├── Enums/
│   │   ├── Exceptions/
│   │   └── Policies/
│   ├── Linkubator.Application/
│   │   ├── Constants/
│   │   ├── Contracts/
│   │   ├── Dtos/
│   │   │   ├── Models/
│   │   │   └── Requests/
│   │   ├── Enums/
│   │   ├── Logging/
│   │   ├── Models/
│   │   ├── Results/
│   │   ├── Services/
│   │   ├── UseCases/
│   │   └── Validation/
│   ├── Linkubator.Infrastructure/
│   │   ├── Constants/
│   │   ├── Contracts/
│   │   ├── DevelopmentIdentity/
│   │   ├── Options/
│   │   ├── Repositories/
│   │   ├── Search/
│   │   ├── Security/
│   │   └── Sqlite/
│   │       └── Migrations/
│   └── Linkubator.Web/
│       ├── Constants/
│       ├── DevelopmentIdentity/
│       ├── Extensions/
│       ├── Logging/
│       ├── Models/
│       ├── Navigation/
│       ├── Options/
│       ├── Pages/
│       └── wwwroot/
├── tests/
│   └── Linkubator.Tests/
│       ├── Application/
│       ├── Architecture/
│       ├── Domain/
│       ├── Infrastructure/
│       ├── Integration/
│       ├── Structure/
│       └── Web/
├── tools/
│   ├── brand-assets/
│   ├── s510-manual-review/
│   └── s66-rebuild-search-projections/
├── AGENTS.md
├── README.md
├── .editorconfig
├── .gitignore
└── Linkubator.sln
```

## Requisitos del entorno

Para trabajar con este repositorio en desarrollo local necesitas:

- Git y .NET SDK 10.
- Windows para reproducir la configuración de Tailwind incluida actualmente.
- Certificado HTTPS local de desarrollo.
- Acceso a NuGet durante la restauración inicial de dependencias.
- Permisos de escritura en las carpetas de la base de datos y de logs.

No se necesita instalar un servidor de base de datos, Node.js ni un servidor SMTP para las funcionalidades actuales.

### Tailwind standalone

En este entorno se usa el ejecutable Windows disponible en:

```text
C:\Repos\Tools\TailwindCSS\tailwindcss-windows-x64.exe
```

El target `BuildTailwindCss` de [Linkubator.Web.csproj](src/Linkubator.Web/Linkubator.Web.csproj) busca el ejecutable mediante una ruta relativa a su carpeta. Esa ruta coincide con la anterior en la ubicación de trabajo actual, pero puede resolver a otro sitio si se clona el repositorio en otra carpeta.

El binario no se descarga automáticamente. Para regenerar estilos, instala Tailwind CLI standalone y adapta esa ruta si es necesario. Si el ejecutable no existe, el target omite la generación: la aplicación utiliza [styles.css](src/Linkubator.Web/wwwroot/css/styles.css), que está versionado, pero los cambios en los estilos fuente no se regenerarán.

## Arranque rápido

Clona el repositorio y entra en su carpeta:

```powershell
git clone https://github.com/DaveSoftDev/Linkubator.git
Set-Location Linkubator
```

Prepara el certificado, restaura y compila:

```powershell
dotnet dev-certs https --trust
dotnet restore Linkubator.sln
dotnet build Linkubator.sln --no-restore
```

Antes de arrancar, establece el origen público para que coincida con el perfil HTTPS versionado:

```powershell
$env:PublicOrigin = 'https://localhost:7263'
dotnet run --project .\src\Linkubator.Web\Linkubator.Web.csproj --launch-profile https
```

Abre `https://localhost:7263/` para la landing y `https://localhost:7263/app` para el dashboard. El perfil también escucha HTTP en el puerto 5228; la aplicación redirige a HTTPS. Los valores proceden de [launchSettings.json](src/Linkubator.Web/Properties/launchSettings.json).

Al arrancar se crea la base de datos si no existe, se aplican las migraciones pendientes y se aprovisiona la identidad demo. No hay que ejecutar manualmente scripts de esquema ni introducir credenciales de login. Si falla la inicialización de la base de datos o de la identidad, el arranque no se completa.

Detén la aplicación con `Ctrl+C`. La variable de entorno anterior se aplica a esa sesión de PowerShell; repítela en una terminal nueva o utiliza la configuración local indicada a continuación.

## Configuración local

La configuración base vive en:

- [src/Linkubator.Web/appsettings.json](src/Linkubator.Web/appsettings.json)

[appsettings.Development.json](src/Linkubator.Web/appsettings.Development.json) no es un lugar para guardar secretos. Para ajustes propios, utiliza variables de entorno sin modificar los valores compartidos:

```powershell
$env:PublicOrigin = 'https://localhost:7263'
$env:ConnectionStrings__Linkubator = 'Data Source=C:\LinkubatorLocal\Linkubator.db'
```

- `PublicOrigin`: origen usado para construir las URL públicas y canonical. Los archivos de configuración versionados indican actualmente el puerto 5001, distinto del perfil HTTPS 7263; el arranque anterior lo sobrescribe explícitamente.
- `ConnectionStrings:Linkubator`: conexión SQLite utilizada por la aplicación. El valor base es relativo; una ruta absoluta evita depender del directorio de trabajo y permite elegir qué base se utiliza.
- `AllowedHosts`: hosts admitidos, sin esquema ni puerto.
- `DevelopmentIdentity:PasswordHash`: hash inicial de una contraseña aleatoria descartada, usado para preparar el demo; no es una contraseña que deba introducirse para entrar.

El email inicial demo es fijo, no una opción configurable. El aprovisionamiento y la resolución de la identidad se describen en [architecture.md → «Autenticación»](context/architecture.md#autenticación).

Los archivos SQLite y logs generados se excluyen mediante [`.gitignore`](.gitignore). Conserva una copia de tu base antes de actualizar o ejecutar herramientas de mantenimiento; no uses para pruebas una base con datos que necesites conservar.

## Logs

La aplicación usa Serilog con salida por:

- consola;
- archivo rotativo dentro de la carpeta `logs/` del proyecto.

La configuración de logs se define en [src/Linkubator.Web/appsettings.json](src/Linkubator.Web/appsettings.json).

## Pruebas del proyecto

Existen pruebas de políticas y entidades de Domain, casos de uso, persistencia SQLite y FTS5, presentación Web, integración HTTP, identidad demo, seguridad Web, logging y dependencias entre capas.

Ejecuta desde la raíz:

```powershell
dotnet test Linkubator.sln --no-restore
```

La existencia de esos tests no equivale a que la auditoría exhaustiva esté cerrada ni garantiza el resultado de una ejecución futura. El estado y las evidencias de la revisión se mantienen en [test-audit-plan.md](plans/test-audit-plan.md); no se fija aquí un número de tests que pueda quedar obsoleto.

## Mantenimiento de la búsqueda

Para reconstruir las proyecciones FTS5 de enlaces y colecciones de una base existente, después de respaldarla:

```powershell
dotnet run --project .\tools\s66-rebuild-search-projections\S66RebuildSearchProjections.csproj -- 'C:\LinkubatorLocal\Linkubator.db'
```

Indica la misma base configurada para la Web. La herramienta rechaza rutas de archivos inexistentes; no es un comando de creación ni de migración de esquema.

## Convenciones de desarrollo

- La fuente de verdad es la documentación funcional en [context/](context/).
- Todo cambio funcional debe estar trazado a una regla de [specifications.md](context/specifications.md) o [domain-model.md](context/domain-model.md).
- Las capas deben respetar la matriz de dependencias de [architecture.md](context/architecture.md).
- No se mezcla lógica de negocio en la Web ni se usan `UserId`, alias o slug del cliente como autorización.
- No se guardan contraseñas ni tokens en claro.

## Convenciones de trabajo

- Ramas: `feature/<id>-<descripcion>`, `fix/<id>-<descripcion>`, `docs/<id>-<descripcion>` y `chore/<id>-<descripcion>`.
- Commits: prefijos `fea:`, `fix:`, `doc:`, `chore:` y `test:`.
- Pull requests: resumen claro, trazabilidad con la spec o documento afectado y verificación ejecutada.
- Backlog principal: [plans/mvp0-plan.md](plans/mvp0-plan.md).

## Siguientes pasos

Completar el refactor del área pública y el cierre y hardening del MVP0 según [roadmap.md → «Now»](context/roadmap.md#now) y [«Next»](context/roadmap.md#next), incluyendo las comprobaciones de calidad y la revisión de tests pendientes. Las funcionalidades futuras se mantienen separadas en [«Later»](context/roadmap.md#later).

Quedan mejoras de reproducibilidad en el entorno de desarrollo: eliminar la dependencia de la ubicación del ejecutable de Tailwind y alinear el origen público versionado con el perfil de ejecución. Las instrucciones anteriores permiten trabajar con la configuración actual sin presentar esas mejoras como ya implementadas.

