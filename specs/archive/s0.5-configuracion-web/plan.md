# Plan técnico: S0.5 — Configuración Web y base de Razor Pages

## Estado y aprobación del plan

- Estado: aprobado.
- Especificación: [spec.md](spec.md).
- Aprobaciones:
  - Especificación: DLG, 07-X-2026 («Cumple requisitos»).
  - Plan y tareas: DLG, 07-X-2026 («Cumple requisitos»). La aprobación documental no acredita la ejecución ni aceptación de las tareas.

## Diseño de implementación

### Componentes y contratos

La capa Web usa `WebApplication.CreateBuilder`, registra Razor Pages y mapea sus endpoints en el pipeline. La página raíz sirve como respuesta técnica mínima, no como pantalla funcional. Mantener el composition root y el uso de Infrastructure conforme a [S0.2](../s0.2-crear-solucion/spec.md).

El host redirige HTTP a HTTPS durante el desarrollo y restringe las peticiones al `Host` configurado según [architecture.md → «Seguridad web»](../../../context/architecture.md#seguridad-web).

El proyecto Web compila Tailwind antes del build de MSBuild, usando el archivo de entrada CSS del proyecto y una salida generada en `wwwroot/css`. No se exige compilar el frontend mediante el build para comprobar el resto de los criterios si la herramienta local no está instalada: esa condición se registra como limitación del criterio correspondiente, no se oculta.

### Flujos internos

1. El builder registra Razor Pages y los servicios Web.
2. El pipeline aplica redirección HTTPS, routing y endpoints Razor Pages.
3. El endpoint raíz devuelve la página mínima.
4. El target de build ejecuta Tailwind sobre el input configurado.
5. La verificación de S0.5 comprueba el host y el build sin asumir configuración de negocio.

### Persistencia y dependencias

No se añade acceso a datos ni dependencia de dominio. Tailwind requiere su ejecutable local según la configuración actual del proyecto. La ejecución HTTPS requiere un certificado de desarrollo disponible; no se instala ni se confía automáticamente un certificado dentro de la tarea documental.

### Garantías técnicas

- Web no incorpora reglas de dominio ni páginas de producto.
- La relación de Web con Infrastructure conserva el límite definido en [S0.2](../s0.2-crear-solucion/spec.md).
- Los valores concretos del entorno pertenecen a S0.6 según [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m).

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado | Fuente o aprobación | Comprobación o resolución |
| --- | --- | --- | --- |
| Activar Razor Pages y una página mínima para comprobar el host | Diseño técnico aprobado | [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m) | Probar ruta raíz por HTTP/HTTPS y no confundirla con una página de producto. |
| Compilar Tailwind desde el target de build de Web | Diseño técnico aprobado | [architecture.md → «Tecnologías»](../../../context/architecture.md#tecnologías) | Build con la herramienta disponible; si falta, anotar limitación sin ocultar el resultado. |
| `AllowedHosts` forma parte de S0.5; otros valores de entorno corresponden a S0.6 | Asignación confirmada por el plan principal y aclaración DLG del 7-X-2026 | [mvp0-plan.md → «S0: Fundaciones (M)»](../../../plans/mvp0-plan.md#s0-fundaciones-m) | La comprobación live de filtrado de host se registra como V06 en esta enmienda; no se presume superada. |
| El redirect de HTTPS depende del certificado local | Riesgo de entorno | [architecture.md → «Seguridad web»](../../../context/architecture.md#seguridad-web) | Registrar si el certificado está disponible y separar error de entorno del comportamiento de la app. |

## Estrategia de validación

Registrar todas las pruebas en [tasks.md](tasks.md). Las pruebas automáticas de arranque pertenecen a S0.7 y pueden usarse como evidencia actual, sin atribuir su autoría a S0.5.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comprobación | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Revisión de composición | SDK disponible | Revisar `AddRazorPages()` y `MapRazorPages()` en `Program.cs` | Razor Pages registrado y mapeado. | Referencias revisadas y fecha. |
| V02 | CA02 | Build y artefacto CSS | Ejecutable Tailwind indicado por el proyecto disponible | Ejecutar `dotnet build Linkubator.sln --nologo --verbosity minimal`; comprobar CSS de salida actualizado/generado | Target Tailwind invocado y build correcto. | Build, salida del target y existencia/fecha de CSS. |
| V03 | CA03 | Integración HTTP local | Certificado de desarrollo disponible; perfil HTTPS identificado | Solicitar HTTP y observar redirect; solicitar endpoint por HTTPS | HTTP redirige a HTTPS y endpoint HTTPS responde según lo esperado. | Esquema, estado/código de redirección, estado final y ambiente, sin secretos. |
| V04 | CA04 | Integración HTTP de arranque | Runner/test de arranque disponible | Ejecutar `WebStartupTests.HomePageReturnsSuccess` o comprobación HTTP equivalente | Ruta raíz responde satisfactoriamente. | Test descubierto/resultado o solicitud/respuesta HTTP. |
| V05 | CA05 | Revisión arquitectónica | Código fuente Web disponible | Revisar el consumo de tipos de Infrastructure según S0.2 | Ningún consumo nuevo fuera de `Program`. | Archivos de Web revisados y resultado. |
| V06 | CA06 | Integración HTTP local | Perfil local Web disponible | Solicitar el endpoint con un encabezado `Host` no permitido y, como control, con el host local permitido | La solicitud ajena se rechaza y el endpoint local continúa respondiendo. | Host usado, códigos HTTP y fecha; no registrar secretos. |

La prueba de arranque puede corroborar la ruta pero no prueba HTTPS local si `WebApplicationFactory` usa TestServer; no afirmar esa garantía a partir de una respuesta HTTP simulada. Si falta certificado o binario, registrar el criterio bloqueado y no ampliar el alcance con instalaciones o cambios productivos no aprobados.

## Orden de ejecución y puerta de salida

- [Tareas](tasks.md) separa inspección, validaciones y aceptación.
- Cerrar solo cuando los criterios estén comprobados con evidencia adecuada, incluidos los que requieren entorno local, y DLG acepte el resultado.
- Archivar después del cierre y de una orden expresa, comprobando rutas y enlaces.
