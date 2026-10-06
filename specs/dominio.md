# SDD: Dominio

## Propósito

Describir la estructura técnica del núcleo de negocio: entidades, validaciones, errores de dominio y pruebas mínimas para asegurar que las reglas funcionales no se rompen por el diseño de implementación.

## Fuente funcional

Este SDD se apoya en:

- [domain-model.md](../context/domain-model.md)
- [specifications.md](../context/specifications.md)
- [architecture.md](../context/architecture.md)
- [decisions.md](../context/decisions.md)

## Alcance

Incluye:

- entidades y tipos del dominio;
- validaciones internas y errores del modelo;
- responsabilidad de la lógica de transformación y del normalizado técnico;
- estrategia de pruebas del núcleo.

Excluye:

- la especificación de mensajes de usuario exactos;
- los valores concretos del producto;
- decisiones de infraestructura o SQL que corresponden a otras capas.

## Decisiones técnicas

### Aislamiento del dominio

La capa de dominio vive de manera autónoma y no depende de infraestructura ni de la capa web. Las decisiones de persistencia, http y UI se adaptan al dominio mediante contratos y adaptadores.

### Validación interna

Las reglas complicadas se agrupan por responsabilidad: transformaciones de texto, validación de URLs, identidad y invariantes del modelo. La validación técnica se usa para garantizar que el dominio solo acepta estados consistentes, y cada error queda trazado a su caso funcional de referencia.

### Modelado de errores

Los errores de dominio son específicos del modelo y sirven para distinguir entre:

- dato inválido;
- estado no permitido;
- conflicto funcional;
- inconsistencia de propiedad o visibilidad.

No deben reutilizar mensajes de la interfaz ni reglas de negocio que vivan en `context/specifications.md` como texto exacto.

## Flujo de ejecución

1. La entrada llega a la capa de dominio ya validada por la capa de aplicación o por un caso de uso.
2. El dominio aplica sus invariantes y decisiones de normalización.
3. Si hay conflicto o invalidez, devuelve error de dominio.
4. El caso de uso o el adaptador externo decide si traduce ese error a una respuesta o a un resultado de aplicación.

## Persistencia y dependencias

- El dominio no conoce detalles de persistencia ni de conexiones.
- Si hace falta, se introduce una interfaz o un puerto para abstraer el acceso a datos.
- La estructura del dominio queda estable aunque cambien repositorios o motores de almacenamiento.

## Invariantes de implementación

- La lógica de dominio no se mezcla con queries o SQL.
- Los valores que dependen del usuario identificado se validan según el contexto de ejecución, no por un dato recibido del cliente.
- Las comprobaciones de visibilidad y de propiedad deben mantenerse coherentes con el modelo de dominio.
- La edición o la creación de una entidad debe respetar el único estado válido del sistema.

## Trazabilidad

Este SDD se usa para validar que la lógica del dominio concuerda con:

- [domain-model.md](../context/domain-model.md)
- [specifications.md](../context/specifications.md)
- [architecture.md](../context/architecture.md)
- [roadmap.md](../context/roadmap.md)

## Riesgos y decisiones pendientes

- Cambios de regla funcional o de límite pueden obligar a ajustar validaciones y errores de dominio.
- Si una validación local se vuelve un contrato específico del producto, debe moverse a la fuente funcional adecuada antes de seguir implementando.

## Verificación

Se considera correcto cuando:

- la lógica del dominio se prueba sin depender de infraestructura;
- los errores son claros y los invariantes no pueden romperse por evolución técnica;
- la capa de aplicación solo coordina y no redefine la lógica del dominio;
- la trazabilidad con `context/` se mantiene sin duplicado de reglas.
