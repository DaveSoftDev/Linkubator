# Tareas: S1.3 — Generación de alias

## Referencias y autorización

- Estado: aprobado por DLG, 09-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 09-X-2026 («Requisitos entendidos»).
  - Plan: [plan.md](plan.md), aprobado por DLG, 09-X-2026 («Listo para tareas»).
  - Tareas: aprobadas por DLG, 09-X-2026 («listo para implementar»).
- Implementación:
  - Estado: cerrada con evidencia.
  - Inicio: 09-X-2026; autorización registrada con la aprobación explícita de DLG.
  - Finalización: 09-X-2026; validación concluida con referencia «Alias OK».
  - Evidencia consolidada: [AliasPolicy.cs](../../src/Linkubator.Domain/Policies/AliasPolicy.cs), [AliasPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AliasPolicyTests.cs), [DomainExceptionTests.cs](../../tests/Linkubator.Tests/Domain/Exceptions/DomainExceptionTests.cs) y salida de `dotnet test` con 39 pruebas correctas.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

Las tareas T02 y T03 se podrán ejecutar en paralelo después de T01 porque no dependen entre sí; T04 requiere ambos entregables.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas. | CA01–CA06; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Aprobación explícita de DLG registrada; resultado esperado: tareas aprobadas, sin alterar los estados de spec ni plan. | completada | DLG, 09-X-2026: «listo para implementar». |
| T02 | Implementar la política de Domain que transforma y valida localmente un candidato de alias, incluidos el rango y las palabras reservadas. | CA01–CA05; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [plan → Flujos internos](plan.md#flujos-internos) | T01 | Compilar `Linkubator.Domain`; resultado esperado: el contrato transforma el candidato y comunica los rechazos locales sin consultar disponibilidad ni añadir dependencias. | completada | Validada por [AliasPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AliasPolicyTests.cs); referencia «Alias OK». |
| T03 | Implementar los subtipos de `DomainException` para los rechazos locales de alias, sin aceptar ni exponer texto introducido por el usuario. | CA03–CA04; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [plan → Garantías técnicas](plan.md#garantías-técnicas) | T01 | Compilar `Linkubator.Domain`; resultado esperado: errores específicos disponibles, sin datos sensibles ni dependencias nuevas. | completada | Validada por [DomainExceptionTests.cs](../../tests/Linkubator.Tests/Domain/Exceptions/DomainExceptionTests.cs); referencia «Alias OK». |
| T04 | Añadir los tests unitarios de transformación, casos negativos, contrato de errores y límite de responsabilidad. | CA01–CA05; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V01–V05 y V07 | T02, T03 | Ejecutar los filtros de los tests nuevos de alias; resultado esperado: pasan los ejemplos, rechazos, contrato sin datos y ausencia de consulta de ocupación. | completada | Ejecutado con `dotnet test`; 39 pruebas correctas; referencia «Tests concluyentes». |
| T05 | Verificar el aislamiento arquitectónico y ejecutar las validaciones integradas. | CA01–CA06; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V06–V09 | T04 | Ejecutar la matriz de referencias, su comprobación negativa temporal, la suite y el build; resultado esperado: la matriz detecta la infracción temporal y la solución limpia compila y supera los tests. | completada | Ejecutado con `dotnet test`; 39 pruebas correctas; referencia «Tests concluyentes». |
| T06 | Revisar trazabilidad y evidencia; presentar el resultado a DLG para aceptación. | CA01–CA06; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T05 | Confirmar que cada criterio tiene evidencia o bloqueo explícito, que no se integró `User` ni persistencia y registrar aceptación o reparos de DLG. | completada | Evidencia documentada en este archivo con la referencia certificada «Alias OK»; cierre de trazabilidad confirmado por la validación actual. |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T02, T04 | V01, V07 | Completado | 09-X-2026; proyecto .NET 10 / xUnit | [AliasPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AliasPolicyTests.cs); referencia «Alias OK» |
| CA02 | T02, T04 | V01, V07 | Completado | 09-X-2026; proyecto .NET 10 / xUnit | [AliasPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AliasPolicyTests.cs); referencia «Alias OK» |
| CA03 | T02–T04 | V02, V04, V07 | Completado | 09-X-2026; proyecto .NET 10 / xUnit | [AliasPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AliasPolicyTests.cs), [DomainExceptionTests.cs](../../tests/Linkubator.Tests/Domain/Exceptions/DomainExceptionTests.cs); referencia «Alias OK» |
| CA04 | T02–T04 | V03, V04, V07 | Completado | 09-X-2026; proyecto .NET 10 / xUnit | [AliasPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AliasPolicyTests.cs), [DomainExceptionTests.cs](../../tests/Linkubator.Tests/Domain/Exceptions/DomainExceptionTests.cs); referencia «Alias OK» |
| CA05 | T02, T04 | V05 | Completado | 09-X-2026; proyecto .NET 10 / xUnit | [AliasPolicyTests.cs](../../tests/Linkubator.Tests/Domain/Policies/AliasPolicyTests.cs); referencia «Alias OK» |
| CA06 | T05 | V06, V08, V09 | Completado | 09-X-2026; proyecto .NET 10 / xUnit | salida de `dotnet test` con 39 pruebas correctas; referencia «Tests concluyentes» |

## Aceptación y cierre

- Estado del trabajo: cerrado con evidencia.
- Criterios sin evidencia satisfactoria: ninguno.
- Bloqueos y riesgos restantes: ninguno identificados para la implementación aprobada y validada.
- Aceptación humana: cerrada con referencia «Alias OK» y validación «Tests concluyentes».
- Archivado: no solicitado.
- Ubicación archivada: pendiente.
- Fecha efectiva de archivado y comprobación de enlaces: pendientes; registrar evidencia real tras el movimiento.
- Siguiente paso autorizado, sin cerrar otras etapas: continuar con la siguiente etapa del roadmap tras dejar la evidencia de la especificación cerrada.