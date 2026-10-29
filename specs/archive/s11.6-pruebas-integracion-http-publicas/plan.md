# Plan técnico: S11.6 — Pruebas de integración HTTP del área pública

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»).
  - Plan: aprobado por DLG el 27-X-2026 con la referencia «Siguiente».

## Diseño de implementación

### Componentes y contratos

- Extender la suite de integración Web existente en [WebStartupTests.cs](../../../tests/Linkubator.Tests/Integration/WebStartupTests.cs). Esta clase ya verifica respuestas públicas, contenido, metadatos, variantes de ruta, paginación y canonical; se reutilizarán o ampliarán los casos existentes en lugar de duplicarlos.
- Reutilizar `ConfiguredWebApplicationFactory`, la base SQLite temporal y los métodos de preparación de datos de `WebStartupTests`. Mantener los marcadores públicos y privados distinguibles para poder comprobar tanto la presencia esperada como la ausencia de contenido privado.
- Inspeccionar las respuestas HTTP y su HTML renderizado. Para los casos `404` y las variantes de URL, configurar el cliente para no seguir redirecciones automáticamente, de forma que el estado y la cabecera `Location` se comprueben directamente.
- No se prevén cambios en los componentes de producción. Si una prueba revela una discrepancia funcional, se registrará contra el trabajo propietario; no se cambiará el comportamiento como parte de esta suite sin la autorización correspondiente.

### Flujos internos

1. Crear una base temporal aislada y arrancar la aplicación mediante la fábrica Web de pruebas.
2. Preparar datos diferenciables para cada caso: contenido público y privado, recursos inexistentes o no accesibles, y suficientes enlaces públicos para comprobar el orden y la paginación.
3. Enviar solicitudes a las rutas públicas pertinentes y comprobar códigos HTTP, redirecciones, contenido y canonical conforme a CA01–CA05.
4. En los casos negativos, comprobar que el test detecta contenido privado, redirecciones indebidas y respuestas distintas de las fuentes.
5. Ejecutar el conjunto focalizado de `WebStartupTests` y registrar el resultado y cualquier bloqueo de los trabajos previos.

### Persistencia y dependencias

- No hay cambios de esquema, migraciones, transacciones ni consultas de aplicación. Los datos se preparan en la base SQLite temporal ya usada por la fábrica de pruebas.
- La validación integrada depende de que estén disponibles las páginas de S11.1–S11.3 y el tratamiento de URL de S11.5. Según el [roadmap](../../../context/roadmap.md#área-pública-y-seo), S11.2 sigue en curso y la validación integrada de S11.5 está bloqueada hasta su cierre. No se incorporará evidencia de esos trabajos al cierre de S11.6.
- La revisión manual del HTML de S11.7 y la navegación pública de S11.8 no son prerrequisitos ni están cubiertas por estas pruebas.

### Garantías técnicas

- CA01 se comprueba con contenido público identificable en la respuesta y marcadores privados que deben estar ausentes; cuando se verifica orden, los marcadores permiten comparar su posición, no solo su presencia.
- CA02 comprueba respuesta `404` sin `Location` para los recursos no accesibles que definen las fuentes y confirma la ausencia de contenido que pudiera revelar el recurso.
- CA03 desactiva el seguimiento automático de redirecciones y verifica su destino; en la variante paginada se solicita además el destino y se comprueba el resultado de la página.
- CA04 compara el contenido y la canonical de la respuesta con la página resultante de interpretar el parámetro según la fuente.
- CA05 envía una cabecera `Host` distinta del origen configurado y comprueba la canonical renderizada. La prueba demuestra el comportamiento HTTP de la aplicación, no su indexación o rastreo efectivo por buscadores.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Reutilizar `WebStartupTests`, su fábrica Web y la base SQLite temporal, extendiendo los casos que ya cubren parte de CA01–CA05. | Propuesta | Estructura existente en [WebStartupTests.cs](../../../tests/Linkubator.Tests/Integration/WebStartupTests.cs); [spec.md](spec.md) | Evita duplicar infraestructura. Confirmar con la aprobación del plan; revisar la cobertura existente al descomponer las tareas. |
| No modificar producción como parte de S11.6; las discrepancias detectadas se devuelven al trabajo propietario antes de cambiar reglas o comportamiento. | Propuesta | [spec.md → «Alcance»](spec.md#alcance) | Mantiene el trabajo enfocado en pruebas; un defecto descubierto podría requerir una decisión o trabajo separado. |
| La ejecución integrada queda condicionada por el estado de S11.2 y S11.5. | Confirmado | [roadmap.md → «Área pública y SEO»](../../../context/roadmap.md#área-pública-y-seo), [S11.5 → «Persistencia y dependencias»](../s11.5-variantes-url-publicas/plan.md#persistencia-y-dependencias) | No se declara satisfactoria la suite integrada antes de que las rutas de esos trabajos estén disponibles. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada, integración HTTP de las páginas de usuario, colecciones y colección; contenido público, marcadores privados y orden | Fábrica Web, base temporal y datos públicos/privados distinguibles; páginas integradas disponibles | Ampliar los casos aplicables de `WebStartupTests`; ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"` | Las respuestas contienen solo los elementos públicos previstos, en el orden establecido, y excluyen marcadores privados, nombre y email del usuario. | Casos y marcadores sembrados, aserciones de presencia/ausencia y orden, resultado del filtro y entorno. |
| V02 | CA02 | Automatizada, integración HTTP negativa de alias y recursos de colección no accesibles | Datos para alias desconocido o antiguo y colecciones inexistentes, privadas, renombradas o sin enlaces públicos; cliente sin redirección automática | Ampliar los casos de `404` de `WebStartupTests`; ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"` | Cada caso definido responde `404` directamente, sin `Location` ni contenido que revele el recurso. | Rutas solicitadas, status, `Location`, marcadores que deben faltar y resultado del filtro. |
| V03 | CA03 | Automatizada, integración HTTP de variantes de rutas existentes y variante de colección paginada | Páginas integradas, datos de recurso público y cliente sin seguimiento automático | Ampliar los casos de redirección de `WebStartupTests`; ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"` | La respuesta redirige a la ruta canónica; en el caso paginado se conserva la página y la respuesta final corresponde a ella. | Ruta pedida, status, `Location`, contenido del destino y resultado del filtro. |
| V04 | CA04 | Automatizada, integración HTTP de páginas de colección y canonical | Colección con enlaces en más de una página y valores de `p` definidos por los casos | Ampliar los casos de paginación y canonical de `WebStartupTests`; ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"` | El contenido corresponde a la página resultante y la canonical representa esa página normalizada, incluida la primera. | Valor de consulta, marcador de contenido esperado, canonical observada y resultado del filtro. |
| V05 | CA05 | Automatizada, integración HTTP de canonical en landing y páginas públicas con `Host` de petición no coincidente | Origen público configurado en la fábrica; páginas disponibles; `Host` sobrescrito en el cliente | Ampliar la comprobación de canonical de `WebStartupTests` para cubrir las rutas incluidas en CA05; ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"` | Todas las canonical usan el origen configurado y la ruta canónica esperada, no el `Host` recibido. | Origen configurado, `Host` enviado, canonical de cada ruta y resultado del filtro. |
| V06 | CA01–CA05 | Automatizada, suite integrada focalizada | Cierre de los prerrequisitos de S11.2 y S11.5 | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"` | Pasan todos los casos de S11.6 en `WebStartupTests`; no se altera producción ni se atribuye cobertura a las revisiones manuales de S11.7–S11.8. | Salida completa del comando, versión del SDK y framework, total de pruebas y resultado por criterio. |

V01 y V02 deben incluir aserciones negativas con marcadores deliberadamente sembrados; un resultado `200` por sí solo no demuestra que los tests detectarían una filtración o una redirección indebida. Las pruebas no sustituyen la revisión manual del HTML ni la prueba de navegación pública.

## Orden de ejecución y cierre

- Las [tareas](tasks.md) están pendientes de aprobación. Concretan la cobertura existente, los huecos por criterio, sus checks inmediatos y el bloqueo de la validación integrada.
- Completar la suite por CA01–CA05 sin duplicar pruebas que ya cubran el comportamiento; después ejecutar V06 cuando S11.2 y S11.5 permitan la validación integrada.
- El cierre requiere evidencia satisfactoria para CA01–CA05, las tareas propias completadas y aceptación humana registrada en `tasks.md`.
- Si una comprobación revela una contradicción en una regla funcional, detener el cambio afectado y tramitar su resolución en el documento propietario antes de continuar.
