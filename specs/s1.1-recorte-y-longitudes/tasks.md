# Tareas: S1.1 — Recorte y validación de longitudes

## Referencias y autorización

- Estado: aprobadas por DLG el 08-X-2026.
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG, 07-X-2026 («La he aprobado yo»).
    - Plan: [plan.md](plan.md), aprobado por DLG, 07-X-2026 («Confirmado»).
    - Tareas: aprobadas por DLG, 08-X-2026 («Listo para implementar»).
- Implementación:
    - Estado: completada.
    - Inicio: 08-X-2026.
    - Finalización: 08-X-2026; tareas de implementación y validación T02–T05 completadas.
    - Evidencia consolidada: [T02–T05](#descomposición) y [evidencia por criterio](#evidencia-por-criterio).

## Descomposición

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas. | CA01–CA07; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Aprobación explícita de DLG registrada; resultado esperado: tasks aprobado, sin alterar estados de spec/plan. | completada | DLG, 08-X-2026: «Listo para implementar». |
| T02 | Implementar `UserTextPolicy.TrimToNull` en Domain y sus tests unitarios. | CA01–CA02; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [V01–V02](plan.md#estrategia-de-validación) | T01 | Ejecutar tests para `null`, entrada vacía, solo espacios, texto recortado y espacios interiores; resultado esperado: comportamiento conforme al contrato del plan. | completada | 08-X-2026, .NET SDK 10.0.401: test selectivo `UserTextPolicyTests`, 14/14; incluye todos los casos de normalización. |
| T03 | Implementar `UserTextPolicy.CountCodePoints` e `IsWithinMaximumLength` en Domain, con tests de límites Unicode. | CA03–CA06; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [V03–V06](plan.md#estrategia-de-validación) | T01 | Ejecutar tests con longitud menor, igual y superior al máximo, pareja sustituta fuera del BMP y marca combinante; resultado esperado: recorrido UTF-16 cuenta la pareja válida como un punto, cuenta marcas individualmente y devuelve `false` por exceso sin truncar/modificar el texto. | completada | 08-X-2026, .NET SDK 10.0.401: test selectivo `UserTextPolicyTests`, 14/14; no hay prueba definida para sustitutos aislados. |
| T04 | Revisar la sensibilidad de la matriz de referencias entre proyectos en una copia temporal. | CA07; [plan → Garantías técnicas](plan.md#garantías-técnicas), [V07](plan.md#estrategia-de-validación) | T01 | Introducir una referencia prohibida desde Domain solo en la copia y ejecutar `ProjectReferenceMatrixTests`; resultado esperado: el test falla por la referencia, y el cambio temporal se descarta. | completada | 08-X-2026: sandbox temporal con referencia Domain→proyecto independiente Linkubator.Probe; `dotnet test Linkubator.sln --filter FullyQualifiedName~ProjectReferenceMatrixTests --logger 'console;verbosity=normal'` descubrió 1 test y falló en `Assert.Equal()` de `ProjectReferencesMatchTheAllowedMatrix`, con referencia actual en Domain y lista esperada vacía. Sandbox eliminado. |
| T05 | Ejecutar pruebas y build integrados; registrar evidencia por criterio. | CA01–CA06, CA07 | T02–T04 | Ejecutar los comandos del plan; resultado esperado: tests funcionales y build pasan; registrar SDK, comandos y salida. La evidencia arquitectónica de CA07 queda separada en T04. | completada | 08-X-2026, .NET SDK 10.0.401: test selectivo `UserTextPolicyTests` 14/14; `dotnet test Linkubator.sln --no-restore --verbosity minimal` pasa 17/17 con aviso existente de redirección HTTPS; `dotnet build Linkubator.sln --no-restore --verbosity minimal` termina con 0 errores y 0 warnings. |
| T06 | Revisar el diff y aceptar el resultado de S1.1. | CA01–CA07; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T04–T05 | Comprobar trazabilidad regla→criterio→test, que no se introdujeron errores de dominio ni entidades de S3 y que todos los checks tienen evidencia; registrar aceptación o reparos de DLG. | completada | DLG, 08-X-2026: «el diff es correcto». |

T02 y T03 pueden ejecutarse en paralelo tras T01. T04 es independiente de los cambios funcionales una vez aprobadas las tareas. T05 integra los resultados y T06 registra la revisión final. S1.1 no implementa consumidores ni errores de dominio; esa integración se hará al implementar las entidades en S3.

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T05 | V01 | Conforme | 08-X-2026; .NET SDK 10.0.401 | `UserTextPolicyTests`, 14/14; `TrimToNull` recorta extremos y conserva interior. |
| CA02 | T02, T05 | V02 | Conforme | 08-X-2026; .NET SDK 10.0.401 | `UserTextPolicyTests`, 14/14; `TrimToNull` devuelve null para null/vacío/solo espacios. Política por entidad queda para S3. |
| CA03 | T03, T05 | V03 | Conforme | 08-X-2026; .NET SDK 10.0.401 | `UserTextPolicyTests`, 14/14; máximo inclusivo comprobado. |
| CA04 | T03, T05 | V04 | Conforme | 08-X-2026; .NET SDK 10.0.401 | `UserTextPolicyTests`, 14/14; exceso devuelve false sin truncar/modificar. Sin prueba de rechazo de entidad, reservado a S3. |
| CA05 | T03, T05 | V05 | Conforme | 08-X-2026; .NET SDK 10.0.401 | `UserTextPolicyTests`, 14/14; pareja sustituta válida fuera del BMP cuenta un punto de código. |
| CA06 | T03, T05 | V06 | Conforme | 08-X-2026; .NET SDK 10.0.401 | `UserTextPolicyTests`, 14/14; letra y marca combinante cuentan como puntos separados. |
| CA07 | T04 | V07 | Conforme | 08-X-2026; .NET SDK 10.0.401, Windows | En sandbox temporal, `ProjectReferenceMatrixTests` detectó la referencia prohibida mediante `Assert.Equal()`; copia eliminada tras la prueba. |

Un resultado fallido o no disponible permanece pendiente/bloqueado; no registrar el resultado esperado como evidencia.

## Aceptación y cierre

- Estado del trabajo: aceptado; implementación funcional, verificaciones técnicas y aceptación humana registradas. No archivado.
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos y riesgos restantes: ninguno identificado para S1.1; no se definen ni lanzan errores de dominio.
- Aceptación humana: DLG, 08-X-2026 («el diff es correcto»).
- Archivado: no solicitado.
- Siguiente paso: ninguno para S1.1. El archivado no se ha solicitado.
