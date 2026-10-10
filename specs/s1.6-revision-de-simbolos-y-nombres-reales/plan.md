# Plan técnico: S1.6 — Revisión de la tabla de símbolos y prueba con nombres reales

## Estado y aprobación del plan

- Estado: aprobado por DLG el 10-X-2026.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG, 10-X-2026 («Listo»).
  - Plan: aprobado por DLG, 10-X-2026 («Aprobado»).

## Diseño de implementación

### Componentes y contratos

- **Sin código de producción ni pruebas nuevas.** S1.6 no modifica `Linkubator.Domain` ni `Linkubator.Tests`. La revisión consume los puntos de entrada públicos ya aceptados: el generador común (`AsciiTransformationPolicy.Transform`) y la política de slugs (`SlugPolicy.Generate`).
- **Ejecución fuera del repositorio.** Las comprobaciones que requieren ejecutar Domain se realizan con un script desechable que referencia el ensamblado compilado de Domain y no se guarda en el repositorio. Su salida se registra en `tasks.md` como evidencia. La disponibilidad de esa vía (por ejemplo, `dotnet fsi`) se comprueba al inicio; si no está disponible, se propone a DLG la alternativa antes de continuar (ver [«Decisiones técnicas y riesgos»](#decisiones-técnicas-y-riesgos)).
- **Revisión de la tabla contra el código (CA01).** Se cruzan las entradas de [«Tabla de conversión de símbolos»](../../context/specifications.md#tabla-de-conversión-de-símbolos) con el código de [AsciiTransformationPolicy.cs](../../src/Linkubator.Domain/Policies/AsciiTransformationPolicy.cs) en dos sentidos:
  - De la fuente al código: cada entrada y cada par texto/resultado se ejecuta con el generador y su resultado se compara con el de la fuente.
  - Del código a la fuente: la lectura del código lista cada símbolo, separador y letra que sustituye, porque las estructuras que los contienen son privadas, y se comprueba que cada uno figura en la fuente.
- **Cobertura de bloques Latin-1 y Latin Extended-A (CA02).** Se enumeran las letras de ambos bloques, se transforma cada una con el generador y se clasifica el resultado como reducida a ASCII, sustituida por la tabla o rechazada. Los dos caracteres que la fuente declara ausentes sirven de control: deben aparecer como rechazados. Un resultado distinto de las tres clases, como una letra eliminada en silencio o convertida en vacío, se registra como hallazgo.
- **Prueba con nombres reales (CA03, CA04).** Con los 20 nombres que aporta DLG, se ejecuta `SlugPolicy.Generate` y se registra para cada uno el slug o el tipo de error de dominio. El resultado esperado por la fuente se obtiene aplicando a mano los pasos de [«Transformación común a ASCII»](../../context/specifications.md#transformación-común-a-ascii) y las reglas de [«Generación de slugs»](../../context/specifications.md#generación-de-slugs), sin reutilizar el código revisado.
- **Juicio y hallazgos (CA05, CA06).** `tasks.md` recoge una tabla de los 20 nombres con su origen, resultado real, resultado según la fuente y juicio de DLG (aceptable o no aceptable, con su referencia), y una lista de hallazgos con su sección fuente. Los hallazgos no aceptados o ambiguos quedan como entrada de S1.7 sin resolverlos.
- **Excepciones reutilizadas.** `TextBelowMinimumLengthException`, `TextExceedsMaximumLengthException` y `TextContainsUnsupportedCharactersException`, sin tipos nuevos.

### Flujos internos

1. Comprobar que Domain compila y que la vía de ejecución del script está disponible.
2. Revisar la tabla contra el código en ambos sentidos y registrar el resultado de cada entrada.
3. Enumerar y clasificar las letras de Latin-1 y Latin Extended-A, con los dos caracteres ausentes como control.
4. Recibir los 20 nombres de DLG, sin modificarlos ni completarlos, y registrar su origen.
5. Ejecutar cada nombre, derivar a mano el resultado según la fuente y contrastar ambos.
6. Presentar a DLG la tabla de resultados y hallazgos, y registrar su juicio por nombre.
7. Ante una divergencia entre el código y la fuente, detener la tarea afectada y registrar el bloqueo: no se ajusta el código ni la fuente para que coincidan.
8. Ejecutar las validaciones integradas y comprobar que no queda ningún cambio de código ni artefactos temporales en el repositorio.

### Persistencia y dependencias

- No hay persistencia, transacciones, servicios externos ni paquetes NuGet nuevos.
- Dependen de este trabajo los nombres reales de DLG ([spec.md → «Bloqueos y preguntas pendientes»](spec.md#bloqueos-y-preguntas-pendientes)) y, para S1.7, los hallazgos que se registren.
- La detección de colisiones y la integración con las entidades pertenecen a S3–S5.

### Garantías técnicas

- Cada nombre se ejecuta con su texto exacto, sin recortarlo ni normalizarlo antes, y su resultado se registra tal como lo devuelve el código.
- La salida registrada indica la versión del SDK y el estado del código revisado (confirmación o rama), para que sea reproducible.
- El resultado esperado según la fuente no procede del código revisado, para que la comparación no sea circular.
- No se registran datos que DLG no haya aportado para ese fin; los nombres se guardan solo con el consentimiento indicado en [«Decisiones técnicas y riesgos»](#decisiones-técnicas-y-riesgos).
- Aislamiento de Domain según [architecture.md → «Arquitectura»](../../context/architecture.md#arquitectura): sin cambios en su código ni en sus dependencias.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| La revisión no añade pruebas ni código al repositorio y se ejecuta con un script desechable | Propuesta | Esta propuesta; [spec.md → «Alcance»](spec.md#alcance) | Mantiene S1.6 como revisión y no amplía la suite. Si DLG prefiere conservar la enumeración de Latin-1 y Latin Extended-A como prueba permanente, es un cambio de alcance que requeriría actualizar la spec. |
| La vía de ejecución del script (`dotnet fsi` u otra) no está comprobada | Pendiente | Esta propuesta | Se detecta en la primera tarea. Si no está disponible, se propone una alternativa a DLG antes de continuar. |
| Los 20 nombres reales pueden contener datos personales o de otras personas | Pendiente | [spec.md → «Bloqueos y preguntas pendientes»](spec.md#bloqueos-y-preguntas-pendientes) | DLG indica si se registran tal cual en `tasks.md` o de forma anonimizada. No se registran hasta que lo confirme. |
| El resultado esperado según la fuente se deriva a mano | Propuesta | Esta propuesta | Evita la comprobación circular con el código; puede contener errores de derivación, por lo que DLG revisa las diferencias. |
| Una divergencia entre la fuente y el código, o un resultado no aceptado, se registra y no se corrige | Confirmada | [spec.md → «Alcance»](spec.md#alcance) | El cambio se tramita en el documento propietario y se resuelve en S1.7. |
| Los casos de DLG pueden revelar un comportamiento ya aceptado como riesgo | Confirmada | [decisions.md → «Riesgos aceptados»](../../context/decisions.md#riesgos-aceptados) | Se registra como hallazgo y DLG decide en S1.7 si lo acepta o lo propone para otra etapa. |

## Estrategia de validación

Las comprobaciones de V01–V05 son revisión técnica y ejecución puntual de Domain, sin UI, HTTP ni persistencia. V06–V08 usan los comandos de la solución que ya emplearon S1.4 y S1.5.

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | Revisión técnica y ejecución puntual: tabla contra código | Domain compilado; vía de ejecución disponible | Ejecutar cada entrada y par de la fuente con el generador y listar las sustituciones del código | Cada entrada de la fuente produce su resultado; cada sustitución del código figura en la fuente | Tabla entrada → resultado de la fuente → resultado del código, y lista de sustituciones del código. |
| V02 | CA02 | Ejecución puntual: letras de Latin-1 y Latin Extended-A | V01 | Enumerar las letras de ambos bloques y transformarlas con el generador | Cada una queda clasificada como reducida, de la tabla o rechazada; los dos caracteres ausentes figuran como rechazados | Resumen por clase, lista de rechazadas y de hallazgos, y confirmación del control. |
| V03 | CA03 | Ejecución puntual: nombres reales | Los 20 nombres de DLG y su política de registro | Ejecutar `SlugPolicy.Generate` con cada nombre | Cada nombre tiene slug o error de dominio registrado | Tabla nombre → origen → resultado real. |
| V04 | CA04 | Revisión técnica: contraste con la fuente | V03 | Derivar a mano el resultado de cada nombre según la fuente y compararlo con el real | Cada diferencia queda registrada como hallazgo con su sección fuente | Columna «según la fuente» y lista de diferencias. |
| V05 | CA05, CA06 | Revisión humana y técnica | V01–V04 | DLG revisa la tabla y los hallazgos; se verifica que cada fila tiene fecha, estado del código y enlace a su fuente | Juicio de DLG por nombre; entradas para S1.7 registradas; sin copiar reglas | Tabla con juicio, referencia de DLG y lista de hallazgos para S1.7. |
| V06 | CA07 | Automatizada integrada | Dependencias restauradas | `dotnet test Linkubator.sln --no-restore --verbosity minimal` | Todos los tests pasan | Comando, código de salida y resumen. |
| V07 | CA07 | Build integrado | SDK y dependencias | `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal` | Build correcto, sin errores ni warnings | Comando, SDK, código de salida, errores y warnings. |
| V08 | CA07 | Revisión técnica: sin cambios de producción | V01–V07 | `git status` y `git diff` de `src/Linkubator.Domain`, y comprobar que no quedan scripts temporales | Sin cambios en Domain ni archivos de la revisión fuera de la carpeta de la spec | Salida de `git status` y revisión del diff. |

V01 y V02 comprueban el comportamiento actual del código frente a la fuente, pero no demuestran que las letras fuera de Latin-1 y Latin Extended-A ni los alfabetos no latinos tengan el tratamiento deseado: esa decisión es del riesgo aceptado ([decisions.md → «Riesgos aceptados»](../../context/decisions.md#riesgos-aceptados)). V03–V05 prueban 20 nombres, no la representatividad de los nombres de los usuarios. V06 y V07 no sustituyen el juicio humano de DLG. No se planifica una comprobación negativa de sensibilidad porque no se añaden pruebas; el control con los dos caracteres ausentes de V02 solo demuestra que la enumeración detecta rechazos.

## Orden de ejecución y cierre

- Tareas: crear `tasks.md` tras aprobar este plan; contendrá la comprobación de la vía de ejecución, la revisión de la tabla, la enumeración, la recepción de los nombres, la ejecución y contraste, el juicio de DLG, las validaciones integradas, la sincronización del roadmap y la aceptación, con dependencias y checks inmediatos. Las tareas de ejecución de nombres dependen de que DLG los aporte.
- S1.6 se cerrará cuando CA01–CA07 tengan la evidencia indicada, no queden bloqueos y DLG acepte el resultado. La resolución de ambigüedades y la aprobación del comportamiento (S1.7) son tareas humanas posteriores que este trabajo no cierra.
- Ante una divergencia o un cambio de regla, detener la tarea afectada y tramitar su aprobación en el documento propietario antes de continuar.
