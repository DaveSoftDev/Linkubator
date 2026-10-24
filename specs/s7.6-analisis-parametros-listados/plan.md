# Plan técnico: S7.6 — Análisis de parámetros de listados

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 23-X-2026 («Siguiente»).
  - Plan: aprobado por DLG el 23-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

- Incorporar un analizador de consulta en la frontera Web, separado en un tipo propio y con modelos/estados de resultado en sus ubicaciones por responsabilidad. El analizador recibe el tipo de listado y la consulta; devuelve valores aplicables validados o estados de error. La comprobación de accesibilidad de `c` se inyecta como predicado delegado por el llamador; no se integra con Application en esta base técnica.
- La responsabilidad de esta spec se limita a `p`, `c` y `q`. La matriz de parámetros aceptados por cada listado y las reglas de parseo y respuesta pertenecen a [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas); los límites de texto pertenecen a [«Longitudes máximas»](../../context/specifications.md#longitudes-máximas), y la semántica de búsqueda/filtros, a [«Búsqueda y filtros»](../../context/specifications.md#búsqueda-y-filtros).
- El analizador no ejecuta consultas, no pagina resultados, no aplica autorización de recursos y no elige rutas o códigos HTTP. Los componentes disponibles de S7.5 sirven a la integración de presentación, sin hacer de los componentes una dependencia del mecanismo de parseo.
- `e` queda ignorado por la cobertura de esta unidad conforme a la delimitación de [spec.md → «Alcance»](spec.md#alcance); no se modifica la regla fuente ni se anticipa el soporte futuro de etiquetas.

### Flujos internos

1. La página consumidora indica el tipo de listado y entrega la colección de valores de query ya parseada por ASP.NET Core.
2. El analizador restringe la interpretación a parámetros admitidos para ese listado y a los incluidos en S7.6; usa el primer valor cuando hay repeticiones.
3. Valida e interpreta `p`, `c` y `q` según las fuentes funcionales. Parámetros ajenos al listado o no disponibles se descartan sin validación.
4. Devuelve datos tipados o errores de validación para que la página aplique el comportamiento definido por su flujo. No llama a Application ni a persistencia desde una transacción, ni transforma por sí mismo la respuesta HTTP.

### Persistencia y dependencias

- No se introducen cambios de esquema, persistencia ni transacciones.
- La comprobación de que el filtro `c` identifica una colección del usuario debe resolverse con identidad de servidor y los contratos de Application; el analizador no recibe un `UserId` del cliente ni autoriza datos.
- La base técnica actual delega esa comprobación a un predicado opcional del llamador y omite el valor cuando no hay una comprobación disponible. La integración con un contrato de Application específico queda pendiente; no se invoca el caso de uso de obtención/edición ni se consulta persistencia desde Web.
- Prerrequisitos disponibles: Razor Pages y el harness de pruebas Web. La integración en rutas consumidoras y validación transversal corresponden a los trabajos que las incorporen.

### Garantías técnicas

- Se conservan las invariantes de identidad y propiedad descritas en [AGENTS.md → «Invariantes críticas»](../../AGENTS.md#invariantes-críticas) y [domain-model.md → «Propiedad de los datos»](../../context/domain-model.md#propiedad-de-los-datos).
- Una query desconocida o no aplicable no activa validación, consulta ni error, conforme a [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas).
- La unidad no escribe ni registra el texto de búsqueda; el registro de peticiones corresponde a S7.8 y queda fuera de esta entrega.
- Los errores de entrada no pasan a la capa de datos como filtros válidos. Los estados de error y las combinaciones de parámetros se prueban para asegurar que la página puede presentar la respuesta funcional sin filtrar el dato a una consulta.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Separar el análisis de la PageModel en una responsabilidad Web comprobable de forma aislada. | confirmado | Aprobación de base técnica por DLG («parámetros validados»). | El analizador puro puede probarse sin implementar listados consumidores ni presentar resultados HTTP. |
| La validación de propiedad de `c` queda tras una delegación a un predicado hasta contar con un punto de integración Application apropiado. | pendiente | [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas) y contratos Application existentes; evidencia técnica registrada en [tasks.md → T01/T04](tasks.md#descomposición). | Sin predicado disponible, `c` se omite. Un contrato específico y su integración quedan pendientes; no se añade lectura ad hoc desde Web. |
| `e` se ignora en este trabajo y no se presenta como capacidad implementada. | confirmado | Delimitación indicada por DLG al preparar [spec.md](spec.md). | Evita adelantar gestión de etiquetas; no cambia la regla funcional de su documento propietario. |
| Los detalles exactos de representación del error en cada listado dependen del flujo de página que integra S7.7. | confirmado | [specifications.md → «Rutas privadas»](../../context/specifications.md#rutas-privadas); alcance de S7.7 en [mvp0-plan.md](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l). | Esta spec valida el estado de error del analizador y su consumo aislado; no bloquea su cierre por implementación futura de las páginas. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01–CA02 | Automatizada; pruebas unitarias del analizador para parámetros reconocidos, claves desconocidas/no aplicables y valores repetidos. | Proyecto Tests y analizador Web accesibles. | `dotnet test tests/Linkubator.Tests/Linkubator.Tests.csproj --filter PrivateListingQueryAnalyzerTests` | Solo se interpretan parámetros aplicables y se toma el primer valor repetido; `e` queda ignorado. | Resultado real y nombres de pruebas en tasks.md. |
| V02 | CA03–CA05 | Automatizada; prueba del default, enteros positivos ASCII, ceros iniciales, vacíos, formato inválido y fuera de rango. | Resultado del analizador y predicado de accesibilidad opcional. | Mismo filtro focalizado `PrivateListingQueryAnalyzerTests`. | Los inválidos producen errores sin ser tratados como filtros válidos; `c` vacío/no accesible/no verificado se omite preservando otros valores. No demuestra propiedad real en Application. | Resultado real y límite de integración registrado en tasks.md. |
| V03 | CA06 | Automatizada; pruebas de `q` vacío, espacios, texto y longitud medida en puntos de código Unicode. | Regla fuente de límite y normalización. | Mismo filtro focalizado `PrivateListingQueryAnalyzerTests`. | Vacíos no activan búsqueda; texto se recorta; el exceso se rechaza y no se entrega como filtro. | Resultado real y casos de borde en tasks.md. |
| V04 | CA07 | Automatizada; probar que un error no borra valores válidos independientes del resultado del análisis. | Modelo de resultado del analizador. | Mismo filtro focalizado `PrivateListingQueryAnalyzerTests`. | Los valores válidos se conservan junto al estado de error; no se valida aquí renderizado final ni respuesta HTTP. | Nombres de pruebas y límites en tasks.md. |
| V05 | CA01–CA07 | Automatizada y revisión de alcance. | V01–V04. | Ejecutar filtro focalizado, suite completa y `git diff --check`; revisar el diff final. | Build/tests pasan; no afirmar consulta real de colección, integración de rutas ni sensibilidad negativa si no se ejecutaron. | Resultados y límites reales en tasks.md. |

Las pruebas unitarias demuestran el análisis y la delegación de estados, no una experiencia de navegación completa, la verificación real de propiedad de `c`, el comportamiento HTTP final de las páginas, la búsqueda de resultados ni el logging. Estos límites se mantienen explícitos en [tasks.md → «Evidencia por criterio»](tasks.md#evidencia-por-criterio).

## Orden de ejecución y cierre

- Las tareas aprobadas se registran en [tasks.md](tasks.md) con dependencias, checks y evidencia por criterio.
- La aceptación humana registrada («parámetros validados») cubre la base técnica del analizador y sus pruebas focalizadas; no declara completados los criterios de integración que requieren rutas consumidoras, comprobación Application de `c` o respuestas HTTP.
- El cierre funcional de S7.6 requiere completar los puntos pendientes explicitados en [tasks.md → «Aceptación y cierre»](tasks.md#aceptación-y-cierre), con evidencia y aceptación humana de cierre.
- El logging de S7.8 y la suite HTTP transversal de S7.9 siguen siendo entregables vecinos; las comprobaciones HTTP de S7.9 no deben confundirse con las pruebas unitarias de esta base técnica.
- Ante una contradicción o cambio de regla, detener solo la tarea afectada y resolverlo mediante el documento propietario antes de continuar.
