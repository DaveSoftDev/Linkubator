# Plan técnico: [Identificador y nombre del trabajo]

## Estado y aprobación del plan

- Estado: propuesta / aprobado / bloqueado.
- Aprobaciones:
	- Especificación: `spec.md`, aprobada por [responsable, fecha]. Al generar el plan en `specs/<slug>/`, enlazar `spec.md` solo si el archivo existe en su ubicación final.
	- Plan: pendiente; registrar responsable, fecha y referencia real cuando se apruebe.

## Diseño de implementación

### Componentes y contratos

Responsabilidades, interfaces, resultados y límites entre capas. Enlazar las reglas fuente; no copiar sus valores ni listas cerradas.

### Flujos internos

Orden de ejecución y manejo técnico de errores, sin redefinir flujos funcionales.

### Persistencia y dependencias

Mecanismos, transacciones, integraciones y prerrequisitos cuando apliquen. No incluir SQL ejecutable. Identificar el trabajo propietario de dependencias externas a este bloque.

### Garantías técnicas

Garantías necesarias para preservar los criterios e invariantes fuente, con enlaces a sus documentos propietarios.

## Decisiones técnicas y riesgos

| Decisión o riesgo | Estado: confirmado / propuesta / pendiente | Fuente o aprobación | Impacto y resolución necesaria |
| --- | --- | --- | --- |
| [Descripción técnica] | [Estado real] | [Referencia] | [Impacto] |

## Estrategia de validación

| ID | Criterios cubiertos | Tipo y alcance | Prerrequisitos | Comando, test o revisión | Resultado esperado | Evidencia a registrar |
| --- | --- | --- | --- | --- | --- | --- |
| V01 | CA01 | [Automatizada / técnica / manual] | [Entorno y dependencias] | [Comprobación concreta] | [Resultado discriminante] | [Registro verificable] |

Identificar lo que cada comprobación no demuestra. Cuando corresponda, describir una comprobación negativa segura que confirme la sensibilidad del test; no asumir que un fallo de compilación demuestra una infracción detectada por un test.

## Orden de ejecución y cierre

- Tareas: crear `tasks.md` tras aprobar este plan; contendrá dependencias, paralelismo justificado y checks inmediatos.
- Criterios con evidencia satisfactoria, sin bloqueos y aceptación humana registrada en las tareas.
- Ante un cambio de regla, detener la tarea afectada y tramitar su aprobación en la fuente propietaria antes de continuar.