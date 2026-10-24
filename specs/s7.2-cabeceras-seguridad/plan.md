# Plan técnico: S7.2 — Cabeceras de seguridad web

## Estado y aprobación del plan

- Estado: aprobado.
- Aprobaciones:
  - Especificación: [spec.md](spec.md), aprobada por DLG el 22-X-2026 («Siguiente»).
  - Plan: DLG, aprobado el 22-X-2026 («Siguiente»).

## Diseño de implementación

### Componentes y contratos

Configurar en el pipeline del host Web las cabeceras requeridas por [architecture.md → «Seguridad web»](../../context/architecture.md#seguridad-web), limitando el cambio a Web. Aplicar los encabezados de forma uniforme a las respuestas de la aplicación y evitar duplicar valores si el host ya los configura. No añadir paquetes, reglas funcionales ni cambios a las capas Domain, Application o Infrastructure.

### Flujos internos

1. El host Web recibe y procesa la petición mediante el pipeline existente.
2. Antes de completar la respuesta, el pipeline incorpora las cabeceras configuradas para seguridad web.
3. La respuesta conserva su comportamiento y contenido funcional existentes junto con los encabezados.

### Persistencia y dependencias

No se modifica la persistencia ni se requieren dependencias de paquetes. Se reutiliza el host y la configuración Web existentes. La suite amplia de integración HTTP pertenece a S7.9, según [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l); S7.2 mantiene una comprobación focalizada propia, por lo que no espera ni depende de esa etapa.

### Garantías técnicas

- Las respuestas del host incluyen las tres cabeceras cubiertas por CA01–CA03 con los valores fijados por su fuente propietaria.
- El cambio no relaja las protecciones de S7.1 ni altera la ruta funcional de las peticiones.
- La comprobación negativa confirma que la validación falla si se elimina o cambia una cabecera requerida.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| Añadir la configuración en el pipeline Web existente, sin introducir dependencias | Propuesta | [architecture.md → «Seguridad web»](../../context/architecture.md#seguridad-web) | Confirmar el punto del pipeline al preparar las tareas; evitar duplicar cabeceras ya emitidas por el host. |
| Validar los encabezados en respuestas HTTP reales del host, además de cualquier test unitario de configuración | Propuesta | Criterios CA01–CA03 de [spec.md](spec.md) | Permite comprobar valores presentes en la respuesta; una inspección estática por sí sola no demuestra ejecución del middleware. |
| Las pruebas integradas HTTP amplias son responsabilidad de S7.9 | Confirmado en el plan de implementación | [mvp0-plan.md → «S7: Web base, seguridad y accesibilidad (L)»](../../plans/mvp0-plan.md#s7-web-base-seguridad-y-accesibilidad-l) | No bloquea las comprobaciones focalizadas ni el cierre de esta spec. |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01–CA03 | Integración HTTP focalizada contra el host Web; comprueba simultáneamente las tres cabeceras | Host Web ejecutable y una ruta de respuesta estable | Ejecutar la comprobación HTTP focalizada añadida por esta tarea y verificar los nombres y valores de cabecera de la respuesta. Si el repositorio aún no tiene un punto adecuado para esta comprobación, detectarlo antes de implementar y decidir en tasks si se añade un test acotado con las herramientas existentes. | Las tres cabeceras están presentes con sus valores esperados en una respuesta generada por la aplicación. | Identificador del test o petición, código HTTP, nombres/valores observados, comando, fecha y resultado; no registrar datos sensibles. |
| V02 | CA01–CA03 | Comprobación negativa de sensibilidad del test, sin conservar cambios | V01 implementada | En un contexto temporal de test, omitir o alterar por separado cada cabecera esperada y confirmar que la aserción correspondiente falla; revertir el cambio temporal inmediatamente. | La comprobación detecta la ausencia o el valor incorrecto de cada cabecera; al restaurarlas, V01 pasa. | Resultado por cada perturbación temporal y confirmación de que no quedó alterado el código productivo. |
| V03 | CA01–CA03 | Revisión técnica del pipeline y respuesta | Cambio de implementación disponible | Revisar el orden del pipeline, posibles valores duplicados y `git diff --check`; ejecutar la validación focalizada de V01. | La configuración se ejecuta para la respuesta de la aplicación, no duplica encabezados y no introduce cambios ajenos al alcance. | Archivos revisados, resultado de diff-check y de V01, fecha. |

## Orden de ejecución y cierre

- Tras aprobar este plan, preparar `tasks.md` con tareas acotadas para confirmar el harness disponible, implementar el comportamiento y ejecutar las validaciones V01–V03. No iniciar cambios productivos antes de aprobar las tareas.
- Los criterios CA01–CA03 se cierran con la comprobación focalizada de esta spec; la cobertura amplia asignada a S7.9 es seguimiento independiente y no bloquea el cierre.
- Cerrar solo cuando exista evidencia satisfactoria para todos los criterios, las tareas propias estén completadas y la aceptación humana esté registrada en `tasks.md`.
