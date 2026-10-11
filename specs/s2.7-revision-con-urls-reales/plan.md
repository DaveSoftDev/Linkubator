# Plan técnico: S2.7 — Revisión de la normalización con 30 URLs reales

## Estado y aprobación del plan

- Estado: aprobado por DLG el 11-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 11-X-2026 («Aprobada»).
  - Plan: aprobado por DLG, 11-X-2026 («Aprobado»).

## Diseño de implementación

### Componentes y contratos

- **Sin código de producción ni pruebas nuevas.** S2.7 no modifica `Linkubator.Domain` ni `Linkubator.Tests`. La revisión consume los puntos de entrada públicos ya aceptados: `LinkUrlPolicy.Adjust`, `UrlValidationPolicy.Validate` y `UrlNormalizationPolicy.Normalize`, en ese orden, tal como los encadena el flujo de [«URLs de los enlaces»](../../context/specifications.md#urls-de-los-enlaces).
- **Ejecución fuera del repositorio.** Las comprobaciones que requieren ejecutar Domain se realizan con un script desechable que referencia el ensamblado compilado de Domain y no se guarda en el repositorio. Su salida se registra en `tasks.md` como evidencia. La disponibilidad de esa vía (por ejemplo, `dotnet fsi`) se comprueba al inicio; si no está disponible, se propone una alternativa a DLG antes de continuar.
- **Registro de las 30 URLs (CA01).** `tasks.md` recoge una tabla con el texto exacto de cada URL y su origen. La política de registro (tal cual, anonimizada o fuera del repositorio) la fija DLG antes de escribir ninguna URL; hasta entonces no se registra ninguna.
- **Ajuste y validación (CA02).** Para cada URL se ejecuta el ajuste y, si no la rechaza, la validación del resultado. Se registra la URL ajustada o el tipo de error de dominio devuelto, sin el texto validado ni credenciales en la evidencia.
- **Normalización (CA03).** Para cada URL aceptada se ejecuta `UrlNormalizationPolicy.Normalize` con la URL ajustada y se registra la clave tal como la devuelve el código.
- **Contraste con la fuente (CA04).** El resultado esperado se deriva a mano aplicando los pasos de [«Ajuste de URL»](../../context/specifications.md#ajuste-de-url), [«Validación»](../../context/specifications.md#validación) y [«Normalización para duplicados»](../../context/specifications.md#normalización-para-duplicados), sin reutilizar el código revisado, y se compara con el obtenido. Cada diferencia se registra como hallazgo con su sección fuente.
- **Colisiones (CA05).** Se comparan las claves de todas las URLs aceptadas y se registran los grupos con la misma clave junto con sus URL originales, o la constatación de que no hay ninguno. Se contrastan con las colisiones aceptadas en [decisions.md → «Riesgos aceptados»](../../context/decisions.md#riesgos-aceptados) sin resolverlas.
- **Juicio y hallazgos (CA06, CA07).** `tasks.md` recoge, por URL, el origen, el resultado real, el resultado según la fuente y el juicio de DLG (aceptable o no aceptable, con su referencia), más la lista de hallazgos con su sección fuente. Los resultados no aceptables o dudosos quedan como entrada de S2.8 sin resolverlos.
- **Excepciones reutilizadas.** Los tipos de error de dominio existentes de `Linkubator.Domain/Exceptions`, sin tipos nuevos.

### Flujos internos

1. Comprobar que Domain compila y que la vía de ejecución del script está disponible.
2. Recibir de DLG la política de registro y las 30 URLs, sin modificarlas ni completarlas, y registrar su origen.
3. Ejecutar el ajuste, la validación y la normalización de cada URL y registrar el resultado tal como lo devuelve el código.
4. Derivar a mano el resultado según la fuente y contrastarlo con el real.
5. Comparar las claves entre sí y registrar las colisiones.
6. Presentar a DLG la tabla de resultados, los hallazgos y las colisiones, y registrar su juicio por URL.
7. Ante una divergencia entre el código y la fuente, detener la tarea afectada y registrar el bloqueo: no se ajusta el código ni la fuente para que coincidan.
8. Ejecutar las validaciones integradas y comprobar que no queda ningún cambio de código ni artefactos temporales en el repositorio.

### Persistencia y dependencias

- No hay persistencia, transacciones, servicios externos ni paquetes NuGet nuevos. Las URLs no se abren ni se descargan: el script solo procesa su texto.
- Dependen de este trabajo las URLs de DLG ([spec.md → «Bloqueos y preguntas pendientes»](spec.md#bloqueos-y-preguntas-pendientes)) y, para S2.8, los hallazgos que se registren.
- La integración con la entidad `Link` pertenece a S3, y la detección de duplicados entre enlaces guardados, a S4 y S5.

### Garantías técnicas

- Cada URL se ejecuta con su texto exacto, sin recortarla ni normalizarla antes, y su resultado se registra tal como lo devuelve el código.
- La salida registrada indica la versión del SDK y el estado del código revisado (confirmación o rama), para que sea reproducible.
- El resultado esperado según la fuente no procede del código revisado, para que la comparación no sea circular.
- Los datos que puedan contener información personal, tokens o credenciales no se registran en el repositorio sin la política que fije DLG.
- Aislamiento de Domain según [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura): sin cambios en su código ni en sus dependencias.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| La revisión no añade pruebas ni código al repositorio y se ejecuta con un script desechable | Propuesta | Esta propuesta; [spec.md → «Alcance»](spec.md#alcance) | Mantiene S2.7 como revisión y no amplía la suite. Si DLG prefiere conservar las 30 URLs como prueba permanente, es un cambio de alcance que requeriría actualizar la spec. |
| La vía de ejecución del script (`dotnet fsi` u otra) no está comprobada | Pendiente | Esta propuesta | Se detecta en la primera tarea. Si no está disponible, se propone una alternativa a DLG antes de continuar. |
| Las URLs reales pueden contener datos personales, tokens o credenciales | Pendiente | [spec.md → «Bloqueos y preguntas pendientes»](spec.md#bloqueos-y-preguntas-pendientes) | DLG indica si se registran tal cual en `tasks.md`, anonimizadas o fuera del repositorio. No se registran hasta que lo confirme. |
| El resultado esperado según la fuente se deriva a mano | Propuesta | Esta propuesta | Evita la comprobación circular con el código; puede contener errores de derivación, por lo que DLG revisa las diferencias. |
| Una divergencia entre la fuente y el código, un resultado no aceptado o una colisión se registra y no se corrige | Confirmada | [spec.md → «Alcance»](spec.md#alcance) | El cambio se tramita en el documento propietario y se decide en S2.8. |
| Las URLs de DLG pueden revelar un comportamiento ya aceptado como riesgo | Confirmada | [decisions.md → «Riesgos aceptados»](../../context/decisions.md#riesgos-aceptados) | Se registra como hallazgo y DLG decide en S2.8 si lo acepta o lo propone para otra etapa. |

## Estrategia de validación

Las comprobaciones de V01–V06 son revisión técnica y ejecución puntual de Domain, sin UI, HTTP ni persistencia. V07 y V08 usan los comandos de la solución que ya emplearon S2.5 y S2.6.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Revisión técnica: registro de las URLs | Las 30 URLs y la política de registro de DLG | Comprobar que constan 30 URLs distintas, con su texto exacto y su origen, según la política fijada | 30 filas, sin omitir, modificar ni sustituir ninguna | Tabla de URLs con su origen y la política aplicada. |
| V02 | CA02 | Ejecución puntual: ajuste y validación | V01; Domain compilado; vía de ejecución disponible | Ejecutar el ajuste y la validación de cada URL con el script desechable | Cada URL tiene su URL ajustada o su error de dominio | Tabla URL → URL ajustada o error. |
| V03 | CA03 | Ejecución puntual: normalización | V02 | Ejecutar la normalización de cada URL aceptada | Cada URL aceptada tiene su clave | Columna de clave en la tabla. |
| V04 | CA04 | Revisión técnica: contraste con la fuente | V02, V03 | Derivar a mano el resultado de cada URL según la fuente y compararlo con el real | Cada diferencia queda registrada como hallazgo con su sección fuente | Columna «según la fuente» y lista de diferencias. |
| V05 | CA05 | Revisión técnica: colisiones | V03 | Comparar las claves entre sí y agrupar las coincidentes | Grupos de colisión con sus URL originales, o constancia de que no hay | Lista de grupos o su ausencia. |
| V06 | CA06, CA07 | Revisión humana y técnica | V01–V05 | DLG revisa la tabla, los hallazgos y las colisiones; se verifica que cada fila tiene fecha, estado del código y enlace a su fuente | Juicio de DLG por URL; entradas para S2.8 registradas; sin copiar reglas | Tabla con juicio, referencia de DLG y lista de hallazgos para S2.8. |
| V07 | CA08 | Automatizada integrada | Dependencias restauradas | `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Todos los tests pasan | Comando, código de salida y resumen. |
| V08 | CA08 | Build integrado y revisión técnica | SDK y dependencias; V01–V07 | `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal`; `git status` y `git diff` de `src/Linkubator.Domain`; comprobar que no quedan scripts temporales | Build sin errores ni warnings; sin cambios en Domain ni archivos de la revisión fuera de la carpeta de la spec | Comando, SDK, código de salida y salida de `git status`. |

V02 y V03 comprueban el comportamiento actual del código con 30 URLs, no la representatividad de las URLs que guardarán los usuarios ni que la clave detecte todos los duplicados deseados: esa limitación es el riesgo aceptado de [decisions.md → «Riesgos aceptados»](../../context/decisions.md#riesgos-aceptados). V05 solo detecta colisiones entre las 30 URLs. V07 y V08 no sustituyen el juicio humano de DLG. No se planifica una comprobación negativa de sensibilidad porque no se añaden pruebas.

## Orden de ejecución y cierre

- Tareas: crear `tasks.md` tras aprobar este plan; contendrá la comprobación de la vía de ejecución, la recepción de las URLs y de la política de registro, la ejecución y el contraste, las colisiones, el juicio de DLG, las validaciones integradas, la sincronización del roadmap y la aceptación, con dependencias y checks inmediatos. Las tareas de ejecución dependen de que DLG aporte las URLs.
- S2.7 se cerrará cuando CA01–CA08 tengan la evidencia indicada, no queden bloqueos y DLG acepte el resultado. La decisión de los casos dudosos (S2.8) es una tarea humana posterior que este trabajo no cierra.
- Ante una divergencia o un cambio de regla, detener la tarea afectada y tramitar su aprobación en el documento propietario antes de continuar.
