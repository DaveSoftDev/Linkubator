# Especificación: S0.6 — Configuración local de entorno

## Estado y aprobación

- Estado: Aprobado por DLG el 7-X-2026.
- [Especificación](spec.md): Configuración OK.
- Este documento define el resultado esperado; las comprobaciones del estado actual y sus límites se registran en [tasks.md](tasks.md).

## Objetivo

Definir los valores técnicos locales necesarios para ejecutar la aplicación durante el desarrollo, separados de la configuración base y sin incorporar datos de producción ni comportamiento de producto.

## Fuentes

- [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m): alcance y Definition of Done del trabajo S0.6.
- [architecture.md → «Autenticación»](../../../context/architecture.md#autenticación): uso local del email de desarrollo.
- [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia): ubicación de los datos locales.
- [decisions.md → «Identidad de desarrollo»](../../../context/decisions.md#identidad-de-desarrollo): propósito del usuario de desarrollo.
- [AGENTS.md → «Invariantes críticas»](../../../AGENTS.md#invariantes-críticas): protección de credenciales y secretos.

## Alcance

- Incluye: origen público local, ruta de archivo de la base SQLite de desarrollo y dirección de correo de desarrollo; separación de estos valores respecto de la configuración base y de producción.
- Excluye: creación o migración de la base de datos, aprovisionamiento del usuario de desarrollo, envío de correo, credenciales, despliegue y configuración de producción.
- `AllowedHosts` pertenece a S0.5 según [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m); S0.6 no es responsable de ese ajuste.
- Trabajo vecino: S0.5 es responsable de la configuración Web; S0.7 de las pruebas de arquitectura/arranque; S5 es responsable de persistencia SQLite y scripts locales de datos, según [mvp0-plan.md → «Resumen de sprints»](../../../plans/mvp0-plan.md#2-resumen-de-sprints).

## Dependencias y prerrequisitos

- S0.2, S0.3, S0.4 y S0.5 están archivadas; la aplicación Web y sus archivos de configuración existen.
- La configuración local debe poder leerse desde el entorno Development de ASP.NET Core.
- S0.6 no presupone que la base SQLite ya exista ni que haya scripts de aprovisionamiento.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [Plan principal → S0.6](../../../plans/mvp0-plan.md#s0-fundaciones-m); [architecture.md → «Enrutamiento»](../../../context/architecture.md#enrutamiento) | La configuración local del host. | Se comprueba el origen usado para enlaces públicos durante el desarrollo. | Hay un origen público local definido en la configuración del entorno Development, separado de la configuración base. |
| CA02 | [architecture.md → «Persistencia»](../../../context/architecture.md#persistencia); [Plan principal → S0.6](../../../plans/mvp0-plan.md#s0-fundaciones-m) | La conexión de datos del entorno Development. | Se inspecciona su destino sin abrir ni crear una base de datos. | La conexión apunta a un archivo local SQLite, no a un servidor compartido ni a una ubicación de producción. |
| CA03 | [architecture.md → «Autenticación»](../../../context/architecture.md#autenticación); [decisions.md → «Identidad de desarrollo»](../../../context/decisions.md#identidad-de-desarrollo) | La configuración local del usuario de desarrollo. | Se comprueba el dato de contacto utilizado por la identidad de desarrollo. | La dirección de desarrollo está declarada bajo configuración local y no se traslada a la configuración base ni a producción. |
| CA04 | [Plan principal → S0.6](../../../plans/mvp0-plan.md#s0-fundaciones-m); [specifications.md → «Registro de eventos»](../../../context/specifications.md#registro-de-eventos) | La configuración base y la de Development. | Se validan estructura JSON y distribución de valores. | Los valores locales de S0.6 están en configuración de Development; la configuración base permanece independiente y no se añaden credenciales o secretos. |
| CA05 | [Plan principal → «3. Definition of Done común (aplica a todos los sprints)»](../../../plans/mvp0-plan.md#3-definition-of-done-común-aplica-a-todos-los-sprints) | La solución configurada. | Se ejecutan build y suite de pruebas existentes. | La solución compila sin warnings/errors y las pruebas disponibles pasan. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguna pregunta pendiente dentro del alcance de S0.6. La propiedad de `AllowedHosts` está asignada a S0.5 por el plan principal y se documenta en la enmienda de [S0.5 → «Criterios de aceptación»](../s0.5-configuracion-web/spec.md#criterios-de-aceptación). | [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m) | Ninguno. | No se requiere otra decisión para S0.6. |

## Artefactos relacionados

- [Plan técnico](plan.md).
- [Tareas y evidencia](tasks.md).
