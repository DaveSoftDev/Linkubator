# Plan técnico: S11.5 — Variantes de URL y respuestas 404 públicas

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 27-X-2026 («Siguiente»).
  - Plan: aprobado por DLG el 27-X-2026 con la referencia «Siguiente».

## Diseño de implementación

### Componentes y contratos

- Reutilizar las Razor Pages y los `PageModel` públicos existentes: [User.cshtml.cs](../../src/Linkubator.Web/Pages/Public/User.cshtml.cs), [Collections.cshtml.cs](../../src/Linkubator.Web/Pages/Public/Collections.cshtml.cs) y [Collection.cshtml.cs](../../src/Linkubator.Web/Pages/Public/Collection.cshtml.cs).
- Normalizar alias y slug a minúsculas antes de las lecturas públicas: [SqlitePublicReadRepository.cs](../../src/Linkubator.Infrastructure/Repositories/SqlitePublicReadRepository.cs) compara esos valores exactamente, mientras que las fuentes definen sus variantes en minúsculas. Tras una lectura satisfactoria, comparar la ruta recibida con su ruta canónica y redirigir si difiere. La landing en `/` no tiene variantes cubiertas por estos criterios.
- Si la lectura devuelve `ResourceNotAccessible`, conservar el `404` antes de considerar una redirección. Los errores técnicos siguen el tratamiento actual y no se convierten en redirecciones ni en respuestas `404`.
- Para una variante válida, proponer una redirección temporal local a la ruta canónica, conservando la query string. Para la página de colección, el destino final mantiene la interpretación de `p` y la canonical ya construida por el `PageModel`.
- No se requieren cambios en Application, Infrastructure, contratos de lectura, persistencia ni configuración de despliegue. Los cambios de producción, si son necesarios, se limitan a las páginas públicas.
- Extender las pruebas HTTP en [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs), reutilizando `ConfiguredWebApplicationFactory`, SQLite temporal y los datos de prueba existentes.

### Flujos internos

1. La petición se enruta a una Razor Page pública aunque la ruta use una variante de mayúsculas o de barra final.
2. El `PageModel` normaliza alias y slug a minúsculas y consulta el servicio de lectura pública existente. Si el recurso no está disponible, responde `404` sin emitir una redirección.
3. Si la lectura es satisfactoria, compara la ruta solicitada con la ruta canónica correspondiente a la página. Si difieren, responde con la redirección local a la ruta normalizada y conserva la query string.
4. El navegador solicita el destino canónico. La página procesa el contenido y la paginación con sus reglas existentes, y genera la canonical con el origen configurado.
5. Si el recurso válido no presenta una variante, se sirve la página normalmente. Una colección pública sin enlaces públicos continúa respondiendo `404`.

### Persistencia y dependencias

- No hay cambios de esquema, migraciones, transacciones ni consultas nuevas. La comprobación de existencia se apoya en el resultado de las lecturas públicas existentes.
- La validación HTTP reutiliza la fábrica, los datos de prueba y la base SQLite temporal de `WebStartupTests`.
- La ruta pública de usuario de S11.2 sigue en curso según el [roadmap](../../context/roadmap.md#área-pública-y-seo). La validación integrada de todas las rutas debe esperar a que ese trabajo esté disponible y cerrado; S11.1 y S11.3 constan completadas.
- S11.4 y S11.6–S11.8 no son dependencias para probar ni cerrar los criterios propios de esta spec.

### Garantías técnicas

- La respuesta a una ruta inexistente, un alias antiguo o un slug antiguo sigue siendo `404`; la normalización se aplica únicamente después de confirmar que el recurso público existe, conforme a [spec.md](spec.md) y a los criterios de [AC-USER-01](../../plans/mvp0-acceptance-criteria.md#ac-user-01--página-pública-de-usuario), [AC-COLLS-01](../../plans/mvp0-acceptance-criteria.md#ac-colls-01--página-pública-de-colecciones) y [AC-COLLECTION-01](../../plans/mvp0-acceptance-criteria.md#ac-collection-01--página-pública-de-una-colección).
- La redirección se construye con una ruta local, no con el host recibido en la petición.
- La query string se conserva durante la redirección; la respuesta final sigue normalizando la canonical de paginación según [specifications.md → «Paginación de las páginas públicas»](../../context/specifications.md#paginación-de-las-páginas-públicas).
- Las pruebas HTTP demuestran el comportamiento observable de la aplicación; no demuestran rastreabilidad real en buscadores ni sustituyen las revisiones manuales de S11.7 y S11.8.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Comprobar la disponibilidad del recurso mediante la lectura pública existente antes de redirigir variantes. | Propuesta | [spec.md](spec.md), criterios CA01 y CA03 | Evita redirigir alias o slugs antiguos. Confirmar con la aprobación del plan. |
| Usar una redirección temporal local (HTTP 302) y preservar la query string. | Propuesta | [spec.md](spec.md), [decisions.md → «Producto y alcance»](../../context/decisions.md#producto-y-alcance) | La fuente exige una redirección, no fija el código, y excluye 301 del MVP0. El plan propone 302; requiere aprobación antes de implementar. |
| Reutilizar los `PageModel` y las pruebas HTTP existentes; no añadir persistencia ni servicios de aplicación. | Propuesta | Estructura existente en [Pages/Public](../../src/Linkubator.Web/Pages/Public/) y [WebStartupTests.cs](../../tests/Linkubator.Tests/Integration/WebStartupTests.cs) | Mantiene el cambio localizado; se revisará si las pruebas demuestran que las rutas variantes no llegan a esos `PageModel`. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Automatizada, integración HTTP de las rutas públicas de usuario, colecciones y colección | Fábrica Web y datos públicos disponibles; cliente configurado para no seguir redirecciones automáticamente | Añadir casos a `WebStartupTests` para mayúsculas y barra final incorrecta; ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"` | Cada variante de un recurso existente produce la redirección local a la ruta canónica en minúsculas y con la barra final correspondiente. El test no presupone un código distinto del aprobado en el plan. | Variantes solicitadas, status y `Location` de cada respuesta HTTP y resultado del filtro. |
| V02 | CA02 | Automatizada, integración HTTP de una página de colección paginada | Fixture con una colección pública y al menos dos páginas de enlaces; cliente sin redirección automática | Añadir un caso que combine una variante de ruta con `p`, comprobar el destino y solicitarlo; ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"` | La respuesta final muestra la página elegida y su canonical normalizada según la fuente; la query necesaria no se pierde durante la redirección. | Ruta y query solicitadas, `Location`, enlace esperado en la respuesta final y canonical. |
| V03 | CA03, CA04 | Automatizada, integración HTTP negativa de alias y slugs antiguos y colección pública vacía | Fixture con un alias distinto del solicitado, una colección bajo su slug actual y una colección pública sin enlaces públicos; cliente sin redirección automática | Ampliar los casos existentes de `WebStartupTests` para inspeccionar el `404` directo y la ausencia de `Location`; ejecutar `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter "FullyQualifiedName~WebStartupTests"` | Las rutas de recursos obsoletos y de la colección vacía responden `404` sin redirección ni exposición de su contenido. | URLs solicitadas, status, cabeceras de redirección y aserciones de ausencia de contenido público. |

V01 y V03 detectan de forma negativa tanto una implementación que deja variantes válidas en `200` como una que redirige rutas obsoletas; la inspección de estado y `Location` evita que el seguimiento automático del cliente oculte esos errores. Estas pruebas no sustituyen la revisión general de HTML de S11.7 ni la navegación manual de S11.8.

## Orden de ejecución y cierre

- Las [tareas](tasks.md) fueron aprobadas por DLG el 27-X-2026 («Vamos»). DLG inició la implementación el 27-X-2026 con la referencia «Inicia la implementación».
- Las tareas propias cubrirán la normalización de rutas válidas, la conservación de la paginación, los `404` sin redirección y la validación HTTP integrada.
- La validación completa de las páginas públicas requiere que S11.2 esté disponible y cerrado; esta condición no incorpora evidencia de ese trabajo a los criterios de S11.5.
- El cierre requerirá evidencia satisfactoria de CA01–CA04, tareas propias completadas y aceptación humana registrada en `tasks.md`.
- Si una comprobación revela una contradicción en una regla funcional, detener el cambio afectado y solicitar su resolución en el documento propietario antes de continuar.
