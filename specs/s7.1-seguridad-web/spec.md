# Especificación: S7.1 — Seguridad web básica

## Estado y aprobación

- Estado: aprobada.
- Aprobación: DLG, 22-X-2026 («Siguiente»).

## Objetivo

Dejar el host Web preparado para aplicar las protecciones de transporte, validación de origen y antiforgery previstas, con un comportamiento comprobable mediante peticiones HTTP.

## Fuentes

- [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l): tarea S7.1 y su alcance.
- [architecture.md → «Seguridad web»](../../context/architecture.md#seguridad-web): comportamiento técnico esperado.

## Alcance

- Incluye: redirección de HTTP a HTTPS, rechazo de peticiones con `Host` ajeno y protección antiforgery de las operaciones que modifican datos, según las fuentes enlazadas.
- Excluye: cabeceras de seguridad adicionales (S7.2), autenticación y resolución de identidad (S7.3), páginas y layout accesible (S7.4 en adelante), y despliegue.
- Trabajo vecino y responsable de sus entregables: S7.2 implementa las demás cabeceras; S7.9 añade pruebas de integración HTTP de la seguridad web, ambas según [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l).

## Dependencias y prerrequisitos

- La configuración inicial de Razor Pages y `AllowedHosts` se entregó en [S0.5 — Configuración Web](../archive/s0.5-configuracion-web/spec.md); S7.1 amplía la seguridad del host sin redefinir esa configuración.
- La verificación HTTPS en desarrollo requiere que el entorno disponga del certificado de desarrollo previsto por la arquitectura.
- La suite de integración HTTP amplia está asignada a S7.9, como trabajo independiente; esto no impide ejecutar en S7.1 las comprobaciones focalizadas necesarias para sus criterios.

## Criterios de aceptación

| ID | Fuente o criterio aprobado propietario | Dado | Cuando | Entonces |
| --- | --- | --- | --- | --- |
| CA01 | [Arquitectura → «Seguridad web»](../../context/architecture.md#seguridad-web) | El host Web local está en ejecución y el certificado de desarrollo está disponible. | Se solicita una ruta por HTTP. | La solicitud se redirige a HTTPS; al solicitarla por HTTPS, el host puede atenderla. |
| CA02 | [Arquitectura → «Seguridad web»](../../context/architecture.md#seguridad-web) | El host Web acepta el nombre de host configurado. | Se envía una petición con `Host` ajeno. | La petición se rechaza y una petición de control con el `Host` permitido sigue atendiendo la ruta. |
| CA03 | [Arquitectura → «Seguridad web»](../../context/architecture.md#seguridad-web) | Existe una operación Web que modifica datos mediante formulario. | Se envía el formulario sin una prueba antiforgery válida y, como control, con una prueba válida. | La petición sin prueba válida se rechaza y la petición de control puede continuar según su resultado funcional. |

## Bloqueos y preguntas pendientes

| Pregunta o contradicción | Fuente afectada | Criterio bloqueado | Decisión humana necesaria |
| --- | --- | --- | --- |
| Ninguno identificado. La comprobación live de HTTPS depende de que esté disponible el certificado de desarrollo. | [Arquitectura → «Seguridad web»](../../context/architecture.md#seguridad-web) | CA01 | Ninguna; registrar la limitación ambiental si el certificado no está disponible. |

## Artefactos relacionados

Aún no hay plan técnico ni tareas: se crearán en las etapas de aprobación correspondientes.
