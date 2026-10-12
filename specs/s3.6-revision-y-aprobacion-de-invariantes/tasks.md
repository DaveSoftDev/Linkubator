# Tareas: S3.6 — Revisión y aprobación de invariantes

## Referencias y autorización

- Estado: aprobada por DLG el 12-X-2026 («Vamos»).
- Aprobaciones:
    - Especificación: [spec.md](spec.md), aprobada por DLG el 12-X-2026 («Validado»).
    - Plan: [plan.md](plan.md), aprobado por DLG el 12-X-2026 («Adelante»).
    - Tareas: aprobadas por DLG el 12-X-2026 («Vamos»).
- Implementación:
    - Estado: completada la implementación y validación técnica de la precondición contractual de `Collection.ChangePrivacy`.
    - Inicio: 12-X-2026; referencia real: «Vamos».
    - Finalización técnica: 12-X-2026; validación ejecutada con `dotnet test Linkubator.sln --no-restore --verbosity minimal` y `dotnet build Linkubator.sln --no-restore -warnaserror --verbosity minimal`; código en [src/Linkubator.Domain/Entities/Collection.cs](../../src/Linkubator.Domain/Entities/Collection.cs) y [src/Linkubator.Domain/Entities/Link.cs](../../src/Linkubator.Domain/Entities/Link.cs).
    - Evidencia consolidada: 553/553 pruebas correctas, 0 errores; compilación correcta sin advertencias. Domain rechaza los conjuntos incompletos y duplicados antes de cambiar el estado.
    - Cierre: aceptado por DLG el 12-X-2026 («Asumido»); CA03 se aplica al contrato de Domain y la integración de Application/Web queda fuera de S3.6.
- La preparación de tareas no inventa aprobaciones ni evidencias de implementación.

## Descomposición

T02–T04 separan las revisiones por entidad y por áreas técnicas, y dependen de T01. T05 confirma la exhaustividad del conjunto de enlaces asociado y documenta el caso límite cuando el llamador pasa un subconjunto. T06 integra los resultados y registra la finalización técnica; T07 presenta la evidencia a DLG para su decisión.

| ID | Acción y entregable | Criterios y diseño | Depende de | Check inmediato y resultado esperado | Estado | Evidencia o bloqueo |
| --- | --- | --- | --- | --- | --- | --- |
| T01 | Confirmar fuentes, estado de fichas y criterios aprobados S3.1–S3.5; iniciar el trabajo solo tras la aprobación explícita de estas tareas. | CA01–CA05; [plan.md → «Flujos internos»](plan.md#flujos-internos), V01 | Aprobación de estas tareas | Revisar rutas de fuentes, código y tests; resultado esperado: artefactos localizables y alcance S3 delimitado, sin atribuir evidencias históricas a una ejecución actual. | realizado | Verificado con la suite y las rutas documentadas. |
| T02 | Revisar las afirmaciones de la ficha `User` frente a sus fuentes propietarias, la entidad y tests; registrar invariantes del alcance, cobertura y observaciones. | CA01; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), V01 | T01 | Comprobar referencias y afirmaciones aplicables a S3; resultado esperado: matriz de `User` con fuentes, código, tests y elementos fuera de alcance. | realizado | Revisión técnica de `User` contra entidad y tests; sin discrepancias relevantes del alcance S3. |
| T03 | Revisar las afirmaciones de las fichas `Collection` y `Link` frente a sus fuentes propietarias, entidades y tests; registrar invariantes del alcance, cobertura y observaciones. | CA01, CA02, CA03; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), V01–V03 | T01 | Contrastar propiedad, creación, edición, movimiento y privacidad; resultado esperado: matriz por entidad con discrepancias y límites de cobertura observados. | realizado | Verificado con el código y los tests relevantes de `Collection` y `Link`. |
| T04 | Revisar la cobertura de propiedad de `Link` y `Collection`, incluidos rechazos por distinto propietario y conservación de estado. | CA02; [plan.md → «Componentes y contratos»](plan.md#componentes-y-contratos), V02 | T01 | Inspeccionar los tests aprobados y ejecutar los pertinentes si procede; resultado esperado: cada afirmación distingue test inspeccionado de prueba ejecutada y registra brechas reales. | realizado | Suite completa ejecutada; los casos de propiedad y rechazo están cubiertos. |
| T05 | Revisar el contrato Domain de `Collection.ChangePrivacy`, inspeccionar sus referencias y comprobar mediante tests que exige el conjunto exacto y rechaza omisiones y duplicados antes de mutar. | CA03, CA04; [plan.md → «Garantías técnicas»](plan.md#garantías-técnicas), V03–V04 | T01 | Revisar usos en Domain y tests; resultado esperado: cobertura de transiciones y de conjuntos inválidos; la ausencia de consumidor en Application/Web se registra como límite de etapa, no como bloqueo de S3.6. | completada | `Collection.ChangePrivacy` exige el conjunto exacto; tests discriminantes de subconjunto y duplicado pasan. La búsqueda confirmó que no hay consumidor Application/Web; su implementación corresponde a S4/S5. |
| T06 | Integrar las matrices y observaciones, revisar la trazabilidad fuente → invariante → código → test y documentar hallazgos sin modificar las fuentes ni las fichas. | CA01–CA04; [plan.md → «Garantías técnicas»](plan.md#garantías-técnicas), V01–V04 | T02, T03, T04, T05 | Revisar cobertura y enlaces; resultado esperado: informe coherente que separa confirmado, no cubierto y fuera del alcance de S3; CA03/CA04 se evalúan según el contrato de Domain y el alcance aprobado, no por la existencia de un consumidor de Application/Web. | realizado | Evidencia consolidada y trazabilidad registrada. |
| T07 | Presentar la matriz y la evidencia a DLG y registrar su decisión humana sobre las invariantes. | CA05; [plan.md → «Orden de ejecución y cierre»](plan.md#orden-de-ejecución-y-cierre), V05 | T06 | Registrar aprobación, rechazo o cambios solicitados con fecha y referencia real; resultado esperado: decisión inequívoca, sin confundir validación técnica con aceptación. | completada | DLG aprueba las invariantes de Domain y el cierre de S3.6 el 12-X-2026 («Asumido»). |

## Evidencia por criterio

| Criterio | Tareas | Validación | Resultado real | Fecha y entorno o versión | Referencia a evidencia |
| --- | --- | --- | --- | --- | --- |
| CA01 | T01, T02, T03, T06 | V01 | Correcto | 12-X-2026; .NET 10.0.12 | Revisión documental y de código de `User`, `Collection` y `Link`; suite completa 553/553 pruebas correctas |
| CA02 | T01, T04, T06 | V02 | Correcto | 12-X-2026; .NET 10.0.12 | Verificación de propiedad, movimiento y rechazo por propietario ajeno en tests de dominio |
| CA03 | T01, T03, T05, T06 | V03, V04 | Correcto | 12-X-2026; .NET 10.0.12 | Contrato de Domain revisado; tests de transiciones, conjuntos incompletos y duplicados; suite completa 553/553 |
| CA04 | T01, T05, T06 | V04 | Correcto | 12-X-2026; .NET 10.0.12 | Referencias inspeccionadas; los únicos usos externos son tests y la llamada interna de Domain. Application/Web y persistencia quedan fuera de S3.6 según el plan del sprint. |
| CA05 | T06, T07 | V05 | Aprobado | 12-X-2026; DLG | Aprobación de invariantes de Domain y cierre de S3.6: «Asumido». |

## Aceptación y cierre

- Estado del trabajo: aceptado y cerrado por DLG.
- Criterios sin evidencia satisfactoria: ninguno dentro del alcance S3.6 (Domain).
- Bloqueos y riesgos restantes: ninguno para S3.6. La integración de Application/Web y persistencia se realizará en las etapas que las poseen; no forma parte de este cierre.
- Aceptación humana: DLG, 12-X-2026 («Asumido»).
- Archivado: no solicitado.
- Ubicación archivada: no aplica.
- Fecha efectiva de archivado y comprobación de enlaces: no aplica.
- Siguiente paso autorizado: continuar con la siguiente etapa del roadmap; S3.6 queda cerrado y no archivado.

