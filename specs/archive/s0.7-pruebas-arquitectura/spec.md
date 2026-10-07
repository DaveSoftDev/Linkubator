# Especificación: S0.7 — Pruebas de arquitectura y arranque Web

## Estado y aprobación

- Estado: aprobada por DLG el 07-X-2026.
- Modalidad: reescritura excepcional prospectiva de spec heredada.
- Aprobación humana: DLG, 07-X-2026, «Pruebas completadas», conforme a la instrucción expresa de aprobar cada documento S0.7 en esta conversación.
- Este documento expresa el resultado esperado; la comprobación actual de los artefactos y sus límites se registra en [tasks.md](tasks.md).

## Objetivo

Establecer una suite automática en el proyecto Tests que detecte regresiones en la matriz de referencias entre proyectos, el límite de dependencia de Infrastructure en Web y el arranque mínimo del host.

## Fuentes

- [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m): tareas y Definition of Done de S0.7.
- [architecture.md → «Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos): matriz y límite del composition root.
- [S0.2 → «Criterios de aceptación»](../s0.2-crear-solucion/spec.md#criterios-de-aceptación): estructura y referencias de solución provistas por el trabajo previo.
- [S0.5 → «Criterios de aceptación»](../s0.5-configuracion-web/spec.md#criterios-de-aceptación): base Web y endpoint mínimo.
- [AGENTS.md → «Invariantes críticas»](../../../AGENTS.md#invariantes-críticas): restricciones generales del repositorio.

## Alcance

- Incluye: xUnit en el proyecto Tests existente; pruebas de matriz de referencias; prueba de que los tipos Web dependientes de Infrastructure se limitan al composition root; prueba de arranque mínimo mediante `WebApplicationFactory`.
- Excluye: comportamiento de negocio, acceso real a base de datos, HTTPS live, validación con navegador, autoría de criterios de casos de uso y tests funcionales.
- Trabajo vecino y responsable: S0.2 aporta la solución y sus referencias; S0.5 configura el host y la página base; S0.7 aporta las pruebas automatizadas; S0.8 documenta los comandos de desarrollo.

## Dependencias y prerrequisitos

- S0.2 y S0.5 están archivadas; S0.6 también está archivada.
- Deben estar disponibles la solución .NET y el runner de pruebas configurado en el proyecto Tests.
- Los tests pueden referenciar proyectos productivos para inspeccionarlos, pero no se convierten en dependencia productiva según la matriz de [architecture.md → «Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos).

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA-01 | [Plan principal → S0.7](../../../plans/mvp0-plan.md#s0-fundaciones-m) | El proyecto Tests creado por S0.2. | Se restaura y compila el proyecto de pruebas. | xUnit y el runner de pruebas quedan configurados para ejecutar la suite de S0.7. |
| CA-02 | [architecture.md → «Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos); [S0.2 → «CA-03/CA-04»](../s0.2-crear-solucion/spec.md#criterios-de-aceptación) | La solución y sus proyectos. | Se ejecuta el test de matriz de referencias. | Las referencias reales coinciden con la matriz permitida y ningún proyecto productivo depende de Tests. |
| CA-03 | [architecture.md → «Dependencias entre proyectos»](../../../context/architecture.md#dependencias-entre-proyectos); [S0.2 → «CA-05»](../s0.2-crear-solucion/spec.md#criterios-de-aceptación) | Los tipos compilados de Web. | Se ejecuta la prueba arquitectónica de consumidores de Infrastructure. | La prueba permite el uso de Infrastructure en el composition root y falla si un tipo fuera de este introduce esa dependencia. |
| CA-04 | [S0.5 → «CA-01 y CA-04»](../s0.5-configuracion-web/spec.md#criterios-de-aceptación); [Plan principal → S0.7](../../../plans/mvp0-plan.md#s0-fundaciones-m) | El host Web configurado. | `WebApplicationFactory` consulta la ruta mínima. | El host se construye y la ruta responde satisfactoriamente; esta prueba no afirma conectividad HTTPS live. |
| CA-05 | [Plan principal → «3. Definition of Done común (aplica a todos los sprints)»](../../../plans/mvp0-plan.md#3-definition-of-done-común-aplica-a-todos-los-sprints) | La solución con las pruebas incorporadas. | Se ejecutan build y suite completa. | El build termina sin warnings/errors y todos los tests descubiertos pasan. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| No se detectan bloqueos de alcance para S0.7. | [Plan principal → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m) | Ninguno. | Ninguna para este trabajo. |

## Artefactos relacionados

- [Plan técnico](plan.md).
- [Tareas y evidencia](tasks.md).
