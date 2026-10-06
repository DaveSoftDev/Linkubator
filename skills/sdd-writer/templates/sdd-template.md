# SDD: [Nombre del subsistema]

## Propósito

Qué hace este subsistema dentro del sistema y qué problema resuelve en la implementación.

## Fuente funcional

Al crear el SDD, conserva estas rutas relativas desde su ubicación en `specs/` y enlaza las reglas que lo justifican en `context/`:

- [requirements.md](../context/requirements.md)
- [specifications.md](../context/specifications.md)
- [domain-model.md](../context/domain-model.md)
- [architecture.md](../context/architecture.md)
- [decisions.md](../context/decisions.md)

## Alcance

- Incluye:
- Excluye:

## Decisiones técnicas

Describe la arquitectura interna y las decisiones de implementación que no son reglas del producto.

### Componentes

- Qué capa o entidad técnica participa.
- Qué responsabilidad ocupa cada componente.
- Qué dependencias tienen entre sí.

### Contratos

- Interfaces, resultados, DTOs, entradas y salidas relevantes.
- Regla de manejo de errores internos y de validación técnica.

## Flujo de ejecución

Explica el flujo principal en pasos, manteniendo la lógica técnica y evitando repetir reglas del dominio.

## Persistencia y dependencias

- Tablas, tolerancias, índices o SQL cuando sea necesario.
- Transacciones, boundaries y orden de ejecución.
- Dependencias externas, infraestructura y configuración.

## Invariantes de implementación

Enumera las garantías técnicas que el código debe mantener, por ejemplo:

- Limitación de acceso por identidad inyectada.
- No esperar usuario ni llamadas externas dentro de transacciones.
- No registrar secretos, contraseñas ni tokens en claro.

## Trazabilidad

Relaciona este SDD con los criterios o pruebas que deben verificarlo.

- Documento funcional de origen:
- Criterios de aceptación o checklist asociados:
- Pruebas o validaciones esperadas:

## Riesgos y decisiones pendientes

- Qué sigue abierto.
- Qué decisión requiere confirmación humana antes de implementarse.
- Qué podría cambiar si la regla funcional cambia.

## Verificación

Qué se comprobará para darlo por correcto:

- validación de capas;
- pruebas de integración/contrato;
- revisión de coherencia con la fuente funcional;
- enlace a la regla funcional que lo respalda.
