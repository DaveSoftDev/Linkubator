# Plan técnico: S0.9 — Convenciones de trabajo y referencia al backlog

## Estado y aprobación del plan

- Estado: aprobado por DLG el 07-X-2026.
- Especificación: [spec.md](spec.md), aprobada por DLG el 07-X-2026.
- Primera puerta: superada por aprobación expresa «Convenciones correctas» de DLG.
- Aprobación humana del plan y tareas: DLG, 07-X-2026, «Convenciones correctas», conforme a la instrucción de esta conversación.
- Modalidad: reescritura excepcional prospectiva; se valida el estado presente sin inferir cuándo se acordaron o escribieron las convenciones.

## Diseño de implementación

### Componentes y contratos

`CONTRIBUTING.md` contiene la guía operativa de ramas, commits, pull requests y referencia al backlog. README conserva un resumen breve y enlaza a los documentos propietarios. `plans/mvp0-plan.md` contiene el backlog de sprints; `context/roadmap.md` aporta orden/criterios de finalización. Esta tarea no replica esas etapas.

### Flujos internos

1. Leer las secciones pertinentes de CONTRIBUTING, README, mvp0-plan y roadmap.
2. Comparar las pautas y detectar diferencias editoriales o referencias que no resuelvan.
3. Comprobar la cobertura documental de ramas, commits y PR, y que el backlog se enlaza a su fuente.
4. Ejecutar validación de enlaces, build y tests de la solución.
5. Registrar los resultados presentes sin usar Git ni atribuir historial de acuerdos.

### Persistencia y dependencias

La tarea modifica solo documentación si se requieren ajustes de sincronización. No crea archivos de backlog ni mueve contenido de `mvp0-plan.md`; no inspecciona estado Git. La verificación técnica se limita a archivos legibles, existencia de rutas referenciadas, build y suite.

### Garantías técnicas

- El plan/roadmap conserva la autoridad sobre el orden y el contenido del backlog.
- README no redefine las convenciones detalladas, y CONTRIBUTING no incorpora reglas de producto.
- No se infiere ni afirma que se consultó o cambió una rama, commit o pull request real.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Mantener el backlog en `mvp0-plan.md` y enlazarlo desde guías | Confirmado por la distribución documental existente | [Plan principal → «Resumen de sprints»](../../../plans/mvp0-plan.md#2-resumen-de-sprints); [CONTRIBUTING.md](../../../CONTRIBUTING.md) | Evita duplicar el backlog y que diverja. |
| Aprobación presente no equivale a evidencia de acuerdo histórico | Límite de trazabilidad | [Plan principal → S0.9](../../../plans/mvp0-plan.md#s0-fundaciones-m) | Registrar fecha y autor reales de la aprobación actual solamente. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V-01 | CA-01 | Revisión documental | README y CONTRIBUTING presentes | Comparar convenciones de ramas/commits/PR en ambos documentos | Resumen coherente y detalle operativo disponible, sin contradicciones. | Secciones revisadas y hallazgos. |
| V-02 | CA-02 | Revisión de fuente/enlaces | Plan y roadmap presentes | Confirmar backlog bajo `plans/mvp0-plan.md` y referencias desde CONTRIBUTING/README | Enlaces al backlog y roadmap sin duplicar tabla de sprints. | Rutas y enlaces resueltos. |
| V-03 | CA-03 | Validación de enlaces y alcance | Markdown del trabajo disponible | Resolver enlaces Markdown locales de README, CONTRIBUTING y los tres artefactos S0.9; revisar que no contienen listado duplicado de sprints ni contenido funcional nuevo | Cero enlaces rotos y límites respetados. | Número de enlaces comprobados y revisión de alcance. |
| V-04 | CA-04 | Build | SDK .NET disponible | `dotnet build Linkubator.sln --nologo --verbosity minimal` | Build correcto con 0 advertencias y 0 errores. | Resumen, SDK, fecha. |
| V-05 | CA-04 | Test automatizado | Build completo | `dotnet test Linkubator.sln --no-build --no-restore --logger 'console;verbosity=minimal'` | Todos los tests descubiertos pasan. | Total/pasados/fallidos/omitidos, fecha. |

No se ejecutan operaciones Git reales; por tanto, no se afirma que las convenciones hayan sido ejercidas en una rama o PR. Build/tests son corroboración de la solución, no prueban la calidad editorial: V-01 a V-03 la verifican por lectura y rutas.

## Orden de ejecución y puerta de salida

- [Tareas](tasks.md) relaciona revisión documental, build, tests y cierre.
- Completar tras evidencia satisfactoria de CA-01 a CA-04 y aceptación explícita de DLG.
- Antes de archivar, actualizar enlaces entrantes/salientes, índice y árbol del README; verificar la carpeta destino y las rutas tras mover.
