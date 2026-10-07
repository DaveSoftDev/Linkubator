# Especificación: S0.5 — Configuración Web y base de Razor Pages

## Estado y aprobación

- Estado: Aprobado por DLG el 7-X-2026.
- Referencia: Cumple requisitos.
- Este documento define el resultado esperado; las comprobaciones actuales y sus límites se registran en [tasks.md](tasks.md).

## Objetivo

Preparar la capa Web para servir Razor Pages mediante HTTPS en desarrollo, compilar los estilos Tailwind durante el build y responder con una página mínima, sin introducir páginas ni comportamiento de producto.

## Fuentes

- [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m): tarea S0.5 y Definition of Done del bloque.
- [architecture.md → «Plataforma»](../../../context/architecture.md#plataforma): ASP.NET Core Razor Pages y Tailwind.
- [architecture.md → «Web»](../../../context/architecture.md#web): responsabilidades y límites de la capa Web.
- [architecture.md → «Seguridad web»](../../../context/architecture.md#seguridad-web): HTTPS local y política de hosts.
- [AGENTS.md → «Reglas de documentación»](../../../AGENTS.md#reglas-de-documentación): trazabilidad y fuente única.

## Alcance

- Incluye: habilitar Razor Pages en el host, servir una página técnica mínima, configurar redirección HTTPS para desarrollo, compilar Tailwind durante el build y configurar `AllowedHosts` conforme a la política de [architecture.md → «Seguridad web»](../../../context/architecture.md#seguridad-web).
- Excluye: `PublicOrigin` y los demás valores locales asignados a S0.6 en [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m); seguridad web adicional, formularios o páginas funcionales, reglas de negocio y despliegue.
- El composition root y la referencia de Web a Infrastructure se rigen por [S0.2](../s0.2-crear-solucion/spec.md); S0.5 no redefine su límite.

## Dependencias y prerrequisitos

- La solución y la estructura de proyectos proceden de [S0.2](../s0.2-crear-solucion/spec.md).
- Los analizadores y convenciones comunes proceden de [S0.3](../s0.3-configuracion-base/spec.md).
- Serilog pertenece a [S0.4](../s0.4-serilog/spec.md); no se redefine aquí.
- La configuración del host para desarrollo local se coordina con S0.6 según [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m), sin adelantar sus valores.

## Criterios de aceptación

| ID | Fuente propietaria | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [Arquitectura → «Web»](../../../context/architecture.md#web) | El host ASP.NET Core de la aplicación. | Se configura y construye la aplicación. | Razor Pages queda registrado y mapeado en el pipeline Web. |
| CA02 | [Plan → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m) | El proyecto Web y sus archivos de estilos. | Se ejecuta el build. | El target de build ejecuta Tailwind y genera el CSS de salida sin errores cuando está disponible la herramienta prevista. |
| CA03 | [Arquitectura → «Seguridad web»](../../../context/architecture.md#seguridad-web) | El host Web ejecutándose en desarrollo. | Se solicita una página por HTTP y después por HTTPS local. | HTTP se redirige a HTTPS y el host puede servir la página técnica mínima por HTTPS, sin requerir páginas funcionales. |
| CA04 | [Plan → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m) | La aplicación Web configurada. | Se solicita la ruta base. | La página técnica mínima responde satisfactoriamente y no contiene comportamiento funcional del producto. |
| CA05 | [S0.2 → «Criterios de aceptación»](../s0.2-crear-solucion/spec.md#criterios-de-aceptación) | El ensamblado Web. | Se inspeccionan los consumidores de Infrastructure. | Solo el composition root consume Infrastructure; S0.5 no introduce otros consumidores. |
| CA06 (enmienda 7-X-2026) | [Plan → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m); [Arquitectura → «Seguridad web»](../../../context/architecture.md#seguridad-web) | El host Web configurado. | Se comprueba la configuración de hosts permitidos y se solicita una petición con `Host` ajeno. | El host aplica la lista configurada y rechaza el `Host` que no corresponde a la aplicación. |

## Bloqueos y preguntas pendientes

- Las verificaciones y su evidencia se registran en [tasks.md](tasks.md). La aprobación documental no equivale a aceptación de los entregables.
- La validación efectiva de la redirección HTTPS depende del certificado de desarrollo disponible en el entorno.

## Artefactos relacionados

- [Plan técnico](plan.md).
- [Tareas y evidencia](tasks.md).
