# Linkubator

Aplicación web para gestionar enlaces en colecciones, con visibilidad pública o privada y un modelo de dominio centrado en el usuario propietario.

## Estado del proyecto

La documentación funcional fue aceptada el 2026-10-06. Está autorizada su implementación según el alcance y el orden de [roadmap.md](context/roadmap.md); las reglas de producto y las invariantes de [AGENTS.md](AGENTS.md) siguen siendo obligatorias.

## Objetivo del repositorio

Este repositorio contiene la base técnica de Linkubator. La intención es dejar funcionando:

- una solución .NET 10 con capas bien separadas;
- ASP.NET Core con Razor Pages;
- Serilog para consola y archivo de log;
- base de datos SQLite local de desarrollo;
- Tailwind standalone para los estilos base de la Web;
- pruebas de arquitectura y de arranque en el proyecto de tests.

La implementación funcional del producto se desarrolla según el orden del [roadmap](context/roadmap.md).

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

```text
.
├── context/
│   ├── architecture.md
│   ├── decisions.md
│   ├── domain-model.md
│   ├── requirements.md
│   ├── roadmap.md
│   ├── specifications.md
│   └── diagrams/
├── plans/
│   └── mvp0-plan.md
├── skills/
├── specs/
│   ├── specs-index.md
│   ├── archive/
│   │   ├── s0.2-crear-solucion/
│   │   ├── s0.3-configuracion-base/
│   │   ├── s0.4-serilog/
│   │   ├── s0.5-configuracion-web/
│   │   ├── s0.6-configuracion-local/
│   │   ├── s0.7-pruebas-arquitectura/
│   │   ├── s0.8-readme-desarrollo/
│   │   └── s0.9-convenciones-y-backlog/
├── src/
│   ├── Linkubator.Application/
│   ├── Linkubator.Domain/
│   ├── Linkubator.Infrastructure/
│   └── Linkubator.Web/
├── tests/
│   └── Linkubator.Tests/
├── AGENTS.md
├── README.md
├── .editorconfig
├── .gitignore
└── Linkubator.sln
```

## Requisitos del entorno

Para trabajar con este repositorio en desarrollo local necesitas:

- .NET SDK 10
- certificado HTTPS local de desarrollo
- acceso a la ruta del ejecutable de Tailwind standalone en Windows
- permisos para crear y escribir ficheros locales en la carpeta del proyecto

### Tailwind standalone

En este entorno se usa el ejecutable Windows disponible en:

```text
C:\Repos\Tools\TailwindCSS\tailwindcss-windows-x64.exe
```

La configuración del proyecto está preparada para invocar ese binario desde la capa Web durante el build.

## Arranque rápido

Desde la raíz del repositorio:

```powershell
dotnet restore

dotnet build Linkubator.sln

dotnet test Linkubator.sln --no-restore
```

Para arrancar la aplicación Web en desarrollo:

```powershell
dotnet run --project .\src\Linkubator.Web\Linkubator.Web.csproj --launch-profile https
```

El sitio se sirve con HTTPS local y usa la configuración del entorno de desarrollo.

## Configuración local

La configuración base vive en:

- [src/Linkubator.Web/appsettings.json](src/Linkubator.Web/appsettings.json)

La configuración local de desarrollo se mantiene en el archivo no versionado `src/Linkubator.Web/appsettings.Development.json`; su exclusión está declarada en [`.gitignore`](.gitignore). La configuración prevista se describe en [S0.6 — Configuración local](specs/archive/s0.6-configuracion-local/spec.md).

Los valores locales actuales incluyen:

- `PublicOrigin`: origen externo de la app en local
- `ConnectionStrings:Default`: SQLite local de desarrollo
- `Development:Email`: dirección de correo de entorno local

La configuración local indica un fichero SQLite para desarrollo; la base de datos aún no se ha creado.

## Logs

La aplicación usa Serilog con salida por:

- consola;
- archivo rotativo dentro de la carpeta `logs/` del proyecto.

La configuración de logs se define en [src/Linkubator.Web/appsettings.json](src/Linkubator.Web/appsettings.json).

## Pruebas del proyecto

Las pruebas actuales cubren:

- arranque de la capa Web;
- validación del composition root;
- aserciones de dependencia entre proyectos.

Ejecuta:

```powershell
dotnet test Linkubator.sln --no-restore
```

## Convenciones de desarrollo

- La fuente de verdad es la documentación funcional en [context/](context/).
- Todo cambio funcional debe estar trazado a una regla de [specifications.md](context/specifications.md) o [domain-model.md](context/domain-model.md).
- Las capas deben respetar la matriz de dependencias de [architecture.md](context/architecture.md).
- No se mezcla lógica de negocio en la Web ni se usan `UserId`, alias o slug del cliente como autorización.
- No se guardan contraseñas ni tokens en claro.

## Convenciones de trabajo

- Ramas: `feature/<id>-<descripcion>`, `fix/<id>-<descripcion>`, `docs/<id>-<descripcion>` y `chore/<id>-<descripcion>`.
- Commits: prefijos `feat:`, `fix:`, `docs:`, `chore:` y `test:`.
- Pull requests: resumen claro, trazabilidad con la spec o documento afectado y verificación ejecutada.
- Backlog principal: [plans/mvp0-plan.md](plans/mvp0-plan.md).

## Siguientes pasos

La base técnica ya está en marcha y el siguiente bloque continúa con la implementación funcional del MVP0 siguiendo el orden del [roadmap.md](context/roadmap.md) y la trazabilidad del [índice de specs](specs/specs-index.md).

