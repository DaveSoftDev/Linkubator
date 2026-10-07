# Especificación: S0.8 — README de desarrollo

## Estado y aprobación

- Estado: Aprobado por DLG el 7-X-2026.
- [Especificación](spec.md): Readme completado.
- Este documento define el resultado esperado; las comprobaciones del estado actual y sus límites se registran en [tasks.md](tasks.md).

## Objetivo

Proporcionar una guía de desarrollo local que permita a una persona identificar los requisitos del entorno, compilar, ejecutar pruebas y arrancar la aplicación sin confundir configuración local con producción ni revelar datos locales.

## Fuentes

- [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m): tarea S0.8 y Definition of Done del bloque.
- [architecture.md → «Plataforma»](../../../context/architecture.md#plataforma): tecnologías previstas.
- [architecture.md → «Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos): estructura y separación por proyectos.
- [architecture.md → «Logs»](../../../context/architecture.md#logs): ubicación/uso técnico de logs.
- [S0.5 → «Criterios de aceptación»](../s0.5-configuracion-web/spec.md#criterios-de-aceptación): arranque Web local y Tailwind.
- [S0.6 → «Criterios de aceptación»](../s0.6-configuracion-local/spec.md#criterios-de-aceptación): configuración de Development.
- [S0.7 → «Criterios de aceptación»](../s0.7-pruebas-arquitectura/spec.md#criterios-de-aceptación): build y pruebas base.

## Alcance

- Incluye: propósito/estado del repositorio, estructura de proyectos, requisitos de desarrollo local, comandos actuales de restore/build/test/arranque, referencias a configuración local, logs y base SQLite local; enlaces a documentación técnica pertinente.
- Excluye: instrucciones de despliegue o producción, documentación funcional detallada, afirmaciones de que existen datos o servicios aún no implementados, y reproducción de valores locales o secretos.
- Trabajo vecino: S0.2–S0.7 aportan solución, configuración, host, entorno local y tests; S0.8 documenta su uso. Las convenciones y backlog de S0.9 tienen propietario separado.

## Dependencias y prerrequisitos

- S0.2–S0.7 están archivadas.
- Deben existir la solución, proyecto Web, archivos de configuración y suite base para que los comandos descritos sean utilizables.
- La guía debe reflejar el estado del repositorio comprobado al redactarse; no deduce que un archivo de base SQLite exista por el hecho de que haya una cadena configurada.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [Plan principal → S0.8](../../../plans/mvp0-plan.md#s0-fundaciones-m) | Una persona nueva en el repositorio. | Consulta el README desde la raíz. | Identifica el propósito, los proyectos principales y los requisitos locales sin necesitar una secuencia de documentos dispersa para comenzar. |
| CA02 | [Plan principal → S0.8](../../../plans/mvp0-plan.md#s0-fundaciones-m); [S0.5 → CA02/CA03](../s0.5-configuracion-web/spec.md#criterios-de-aceptación) | El árbol de solución y comandos disponibles. | Sigue los pasos de comprobación descritos. | Los comandos de restore/build/test/arranque coinciden con la solución y el proyecto Web actuales. |
| CA03 | [S0.6 → Criterios de aceptación](../s0.6-configuracion-local/spec.md#criterios-de-aceptación); [AGENTS.md → «Invariantes críticas»](../../../AGENTS.md#invariantes-críticas) | La configuración del entorno local. | El README explica dónde reside y qué datos de desarrollo se usan. | Distingue configuración base y local, no copia valores privados y no afirma que la base SQLite esté creada si no existe. |
| CA04 | [architecture.md → «Logs»](../../../context/architecture.md#logs); [S0.4 → Criterios de aceptación](../s0.4-serilog/spec.md#criterios-de-aceptación) | La configuración del logger y estructura local. | El README describe dónde consultar logs y la configuración que los gobierna. | La indicación de logs coincide con la configuración y no revela contenido de log ni datos sensibles. |
| CA05 | [Plan principal → «3. Definition of Done común (aplica a todos los sprints)»](../../../plans/mvp0-plan.md#3-definition-of-done-común-aplica-a-todos-los-sprints) | Los enlaces Markdown y comandos del README. | Se revisan referencias y se ejecutan las comprobaciones pertinentes. | Los enlaces locales citados resuelven, y build y pruebas pasan. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| No se detectan bloqueos funcionales para escribir la guía. La existencia actual de una base SQLite no está afirmada; solo se documenta su destino configurado. | [S0.6 → CA02](../s0.6-configuracion-local/spec.md#criterios-de-aceptación) | Ninguno. | Ninguna para el alcance de S0.8. |

## Artefactos relacionados

- [Plan técnico](plan.md).
- [Tareas y evidencia](tasks.md).
