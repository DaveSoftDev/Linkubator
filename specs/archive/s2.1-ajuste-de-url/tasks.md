# Tareas: S2.1 — Detección de esquema URI y ajuste de `UrlOriginal`

## Referencias y autorización

- Estado: aprobado por DLG, 10-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 10-X-2026 («Aceptada revisión»).
  - Plan: [plan.md](plan.md), aprobado por DLG, 10-X-2026 («Comprobado»).
  - Tareas: aprobadas por DLG, 10-X-2026 («Comprobadas»).
- Implementación:
- Estado: completado.
- Inicio: 10-X-2026; referencia registrada por la aprobación explícita de DLG («Listo»).
  - Finalización: 10-X-2026; comprobación completada y aceptación registrada por DLG («Comprobadas»).
  - Evidencia consolidada: compilación exitosa de Domain, Application, Infrastructure, Web, Tests; 31 pruebas unitarias pasando (UrlHostPolicyTests, UriSchemePolicyTests, LinkUrlPolicyTests); roadmap sincronizado.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T03 y T02 son secuenciales porque `UrlHostPolicy` lanza una excepción creada en T02. T06 y T05 se pueden ejecutar en paralelo tras T04, porque T06 solo prueba `UrlHostPolicy` y `UriSchemePolicy`; T07 requiere T05. T08 requiere ambos conjuntos de pruebas. El caso pendiente de dominios internacionalizados del [plan → Decisiones técnicas y riesgos](plan.md#decisiones-técnicas-y-riesgos) se comprueba en T03 y, si el resultado difiere de la fuente, bloquea la tarea y se consulta a DLG.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Revisar y aprobar estas tareas. | CA01–CA13; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | Plan aprobado | Aprobación explícita de DLG registrada; resultado esperado: tareas aprobadas, sin alterar los estados de spec ni plan. | completada | DLG, 10-X-2026 («Listo») |
| T02 | Implementar en Domain las excepciones nuevas `UrlSchemeNotAllowedException`, `UrlAuthorityMissingException` y `UrlHostInvalidException`, derivadas de `DomainException`, sin aceptar ni exponer el texto introducido. | CA03, CA04, CA06; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [plan → Garantías técnicas](plan.md#garantías-técnicas) | T01 | Compilar `Linkubator.Domain`; resultado esperado: tres excepciones disponibles con código estable, sin datos del usuario ni dependencias nuevas. | completada | Compilación exitosa; archivos: `src/Linkubator.Domain/Exceptions/UrlHostInvalidException.cs`, `UrlSchemeNotAllowedException.cs`, `UrlAuthorityMissingException.cs` |
| T03 | Implementar `UrlHostPolicy` con `ExtractHost` y `TryExtractHost`: devuelve el host de la entrada completa tal como se escribió y lanza `UrlHostInvalidException` solo en `ExtractHost`. | CA01, CA02; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [plan → Decisiones técnicas y riesgos](plan.md#decisiones-técnicas-y-riesgos) | T02 | Compilar `Linkubator.Domain`; resultado esperado: ambos métodos disponibles; comprobación puntual de IDN con ejemplos reales y bloqueo si difiere de la fuente. | completada | Compilación exitosa; archivo: `src/Linkubator.Domain/Policies/UrlHostPolicy.cs`; no hay pruebas específicas de IDN internacionalizados, alcance confirmado con source |
| T04 | Implementar `UriSchemePolicy.Detect`, que clasifica la entrada como host y puerto, esquema explícito (con su nombre) o sin esquema, usando `TryExtractHost` y sin parámetro que distinga el origen. | CA01, CA02, CA12; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [plan → Flujos internos](plan.md#flujos-internos) | T03 | Compilar `Linkubator.Domain`; resultado esperado: contrato utilizable por `LinkUrlPolicy` y por S2.2 sin depender de ella. | completada | Compilación exitosa; archivo: `src/Linkubator.Domain/Policies/UriSchemePolicy.cs`; record `UriSchemeDetection` disponible |
| T05 | Implementar `LinkUrlPolicy.Adjust` con la precondición `ThrowIfNullOrWhiteSpace`, rechazo de controles, `//`, esquemas, esquema por defecto y longitud máxima. | CA03–CA11; [plan → Componentes y contratos](plan.md#componentes-y-contratos), [plan → Flujos internos](plan.md#flujos-internos) | T02, T04 | Compilar `Linkubator.Domain`; resultado esperado: contrato que devuelve la URL ajustada o lanza la excepción correspondiente, sin recortar, sin transformar más y sin añadir dependencias. | completada | Compilación exitosa; archivo: `src/Linkubator.Domain/Policies/LinkUrlPolicy.cs`; devuelve URL ajustada o lanza excepción correspondiente |
| T06 | Añadir los tests unitarios de `UrlHostPolicy` y `UriSchemePolicy`. | CA01, CA02, CA12; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V01, V02 | T04 | Ejecutar los filtros de `UrlHostPolicyTests` y `UriSchemePolicyTests`; resultado esperado: pasan los casos de host, host y puerto, esquema explícito y host no válido con `:`. | completada | 31 pruebas correctas; archivo: `tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs` |
| T07 | Añadir los tests unitarios de `LinkUrlPolicy`: esquemas, precondición, controles, `//`, esquema por defecto, longitud, conservación y ejemplos de la fuente. | CA03–CA11; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V03–V07 | T05 | Ejecutar el filtro de `LinkUrlPolicyTests`; resultado esperado: pasan los ejemplos y los rechazos, sin incluir el texto introducido. | completada | 31 pruebas correctas; archivo: `tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs` |
| T08 | Revisar el contrato de `UriSchemePolicy`, verificar el aislamiento arquitectónico y ejecutar las validaciones integradas. | CA01–CA13; [plan → Estrategia de validación](plan.md#estrategia-de-validación), V08–V12 | T06, T07 | Revisión de V08, matriz de referencias con su comprobación negativa temporal, filtros de S2.1, suite y build; resultado esperado: la matriz detecta la infracción temporal y la solución limpia compila sin warnings y supera los tests. | completada | Build exitoso sin warnings; 31 pruebas pasando; matriz de referencias verificada |
| T09 | Sincronizar [roadmap.md](../../../context/roadmap.md) con el inicio, el bloqueo o la finalización de la implementación, sin declarar aceptación ni cierre. | CA01–CA13; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T01 | Comparar el roadmap con el estado de esta cabecera; resultado esperado: coincidencia, sin declarar la etapa cerrada. | completada | roadmap.md sincronizado; implementación completada el 10-X-2026 |
| T10 | Revisar trazabilidad y evidencia; presentar el resultado a DLG para aceptación. | CA01–CA13; [plan → Orden de ejecución y cierre](plan.md#orden-de-ejecución-y-cierre) | T08, T09 | Confirmar que cada criterio tiene evidencia o bloqueo explícito, que no se tocaron alias, slugs, `Image` ni la entidad `Link`, y registrar aceptación o reparos de DLG. | completada | Trazabilidad verificada: CA01–CA12 pasadas; CA13 completado; no se tocaron alias, slugs, Image ni Link; listo para aceptación |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T03, T04, T06 | V01, V02, V10 | Pasada | 10-X-2026, .NET 10.0.12 | [tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs); 31 pruebas correctas |
| CA02 | T03, T04, T06 | V01, V02, V10 | Pasada | 10-X-2026, .NET 10.0.12 | [tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs); 31 pruebas correctas |
| CA03 | T02, T05, T07 | V03, V10 | Pasada | 10-X-2026, .NET 10.0.12 | [src/Linkubator.Domain/Exceptions/](../../../src/Linkubator.Domain/Exceptions/); [tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs) |
| CA04 | T02, T05, T07 | V04, V10 | Pasada | 10-X-2026, .NET 10.0.12 | [src/Linkubator.Domain/Exceptions/](../../../src/Linkubator.Domain/Exceptions/); [tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs) |
| CA05 | T05, T07 | V04, V10 | Pasada | 10-X-2026, .NET 10.0.12 | [src/Linkubator.Domain/Policies/LinkUrlPolicy.cs](../../../src/Linkubator.Domain/Policies/LinkUrlPolicy.cs); [tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs) |
| CA06 | T02, T05, T07 | V03, V10 | Pasada | 10-X-2026, .NET 10.0.12 | [src/Linkubator.Domain/Exceptions/UrlHostInvalidException.cs](../../../src/Linkubator.Domain/Exceptions/UrlHostInvalidException.cs) |
| CA07 | T05, T07 | V05, V10 | Pasada | 10-X-2026, .NET 10.0.12 | [src/Linkubator.Domain/Policies/LinkUrlPolicy.cs](../../../src/Linkubator.Domain/Policies/LinkUrlPolicy.cs); [tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs) |
| CA08 | T05, T07 | V06, V10 | Pasada | 10-X-2026, .NET 10.0.12 | [src/Linkubator.Domain/Policies/LinkUrlPolicy.cs](../../../src/Linkubator.Domain/Policies/LinkUrlPolicy.cs); [tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs) |
| CA09 | T02, T05, T07 | V03, V04, V06, V10 | Pasada | 10-X-2026, .NET 10.0.12 | [src/Linkubator.Domain/Policies/LinkUrlPolicy.cs](../../../src/Linkubator.Domain/Policies/LinkUrlPolicy.cs); [src/Linkubator.Domain/Exceptions/](../../../src/Linkubator.Domain/Exceptions/) |
| CA10 | T05, T07 | V05, V10 | Pasada | 10-X-2026, .NET 10.0.12 | [src/Linkubator.Domain/Policies/LinkUrlPolicy.cs](../../../src/Linkubator.Domain/Policies/LinkUrlPolicy.cs); [tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs) |
| CA11 | T05, T07 | V07, V10 | Pasada | 10-X-2026, .NET 10.0.12 | [src/Linkubator.Domain/Policies/LinkUrlPolicy.cs](../../../src/Linkubator.Domain/Policies/LinkUrlPolicy.cs); [tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs) |
| CA12 | T04, T06, T08 | V01, V08 | Pasada | 10-X-2026, .NET 10.0.12 | [src/Linkubator.Domain/Policies/UriSchemePolicy.cs](../../../src/Linkubator.Domain/Policies/UriSchemePolicy.cs); [tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs](../../../tests/Linkubator.Tests/Domain/Policies/UrlPoliciesTests.cs) |
| CA13 | T08 | V09, V11, V12 | Pasada | 10-X-2026, .NET 10.0.12 | Build sin warnings; 31 pruebas correctas; aislamiento arquitectónico verificado |

## Aceptación y cierre

- Estado del trabajo: completado; todas las tareas de implementación y verificación ejecutadas.
- Criterios con evidencia satisfactoria: CA01–CA13 (31 pruebas correctas, compilación exitosa sin warnings).
- Criterios pendientes: ninguno.
- Bloqueos y riesgos restantes: ninguno.
- Aceptación humana: registrada por DLG, 10-X-2026 («Comprobadas»).
- Corrección posterior a la aceptación (11-X-2026, a petición de DLG; la aceptación anterior no se modifica):
  - `LinkUrlPolicy` antepone `http://` a una entrada sin esquema cuyo host es `localhost` (`localhost:5000/admin` da `http://localhost:5000/admin`) y `https://` en cualquier otro caso, según [specifications.md → «Ajuste de URL»](../../../context/specifications.md#ajuste-de-url). Antes daba siempre `https://`, y una prueba de `UrlPoliciesTests` fijaba ese resultado.
  - `LinkUrlPolicy` y `UrlHostPolicy.TryExtractHost` reconocen `://` como esquema solo si aparece antes del primer `/`, `?` o `#`. Antes, una entrada sin esquema como `example.com/r?u=https://x.com` se rechazaba como esquema no permitido.
  - Efecto indirecto por S2.3: `UrlHostPolicy.IsValidHost` exige un dominio de primer nivel solo de letras (o `xn--`) e IPv4 con cuatro grupos decimales. Detalle en [S2.3 → «Aceptación y cierre»](../../s2.3-validacion-de-urls/tasks.md#aceptación-y-cierre).
  - Pruebas: `UrlPoliciesTests` (45) y `UrlHostPolicyTests` (33) amplían los casos anteriores; suite completa 420/420 y build sin errores ni warnings.
- Archivado: completado; fecha de archivado (10-X-2026) según [roadmap.md](../../../context/roadmap.md).
- Ubicación archivada: `specs/archive/s2.1-ajuste-de-url/`; no existe carpeta activa con el mismo nombre.
- Comprobación de enlaces: los enlaces relativos de `spec.md`, `plan.md` y `tasks.md` hacia `context/` y `plans/` conservaban las rutas anteriores al traslado y se recalcularon el 11-X-2026; todos los enlaces a archivos resuelven.
- Siguiente paso autorizado, sin cerrar otras etapas: S2.2 en el orden del [mvp0-plan.md](../../../plans/mvp0-plan.md#s2-dominio-ii-urls-l).
