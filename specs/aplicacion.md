# SDD: Aplicación

## Propósito

Definir la coordinación técnica entre las capas de dominio, infraestructura y web, con foco en casos de uso, identidad, resultados y contratos de ejecución.

## Fuente funcional

Este SDD conecta con:

- [requirements.md](../context/requirements.md)
- [specifications.md](../context/specifications.md)
- [domain-model.md](../context/domain-model.md)
- [architecture.md](../context/architecture.md)
- [roadmap.md](../context/roadmap.md)

## Alcance

Incluye:

- contratos de caso de uso y sus resultados;
- orquestación de aplicación y adaptadores;
- identidad de ejecución y limitación por usuario identificado;
- coordinación con repositorios y unit of work;
- pruebas de integración de la capa de aplicación.

Excluye:

- mensajes exactos y requisitos funcionales del producto;
- detalles de UI de la web;
- reglas de persistencia que pertenecen a infraestructura; 
- decisiones de negocio que están exclusivamente definidas en `context/`.

## Decisiones técnicas

### Resultados de aplicación

La capa de aplicación usa un patrón de resultado claro para distinguir entre:

- éxito;
- validación fallida;
- conflicto de estado;
- error funcional o de infraestructura.

Esto permite mantener la web y la infraestructura desacopladas y evita que la UI interprete errores del dominio como si fueran mensajes del producto.

### Contratos de acceso

Los repositorios y los adaptadores se presentan mediante interfaces bien definidas. La identidad operativa del usuario se resuelve en capas superiores y se inyecta en la lógica de aplicación. La capa de aplicación nunca aceptará un identificador del cliente como autoridad de autorización.

### Coordinación de transacciones

Los casos de uso usan una unidad de trabajo o un equivalente para encapsular cambios relacionados. Las operaciones largas o externas se limitan a lo que la especificación técnica permite, y las transacciones se mantienen breves y aisladas de la interacción del usuario.

## Flujo de ejecución

1. La petición o la acción llega a la capa web o a la capa de aplicación.
2. La capa de aplicación valida el contrato de entrada y la identidad disponible.
3. Se invoca el caso de uso correspondiente con los servicios o repositorios necesarios.
4. El dominio y la infraestructura resuelven los datos y devuelven un resultado.
5. La capa web transforma el resultado en la respuesta o acción correspondiente.

## Persistencia y dependencias

- La capa de aplicación no contiene SQL ni lógica de conexión.
- Llama a contratos de persistencia y usa unit of work solo para garantizar consistencia del cambio.
- Las decisiones de acceso a base de datos, caché, índices y proyecciones están en infraestructura.

## Invariantes de implementación

- La identidad del usuario debe ser inyectada o resuelta por contexto, no por `UserId` del cliente.
- Los casos de uso no pueden hacer llamadas HTTP ni esperar al usuario dentro de una transacción.
- Si un caso de uso falla, no debe dejar el sistema en un estado parcialmente actualizado.
- La capa de aplicación no crea mensajes ni valores de negocio como si fueran reglas.

## Trazabilidad

Este SDD se usa para asegurar que los casos de uso están alineados con:

- [architecture.md](../context/architecture.md)
- [domain-model.md](../context/domain-model.md)
- [specifications.md](../context/specifications.md)
- [plans/mvp0-plan.md](../plans/mvp0-plan.md)

La validación debe revisarse contra la puerta de sprint y la checklist de calidad del roadmap.

## Riesgos y decisiones pendientes

- Un cambio en el modelo o en los criterios de aceptación puede exigir reestructurar contratos antes de implementar.
- La capa de aplicación debe gestionar errores de infraestructura sin convertirlos en reglas del producto.

## Verificación

Se considera correcto cuando:

- los casos de uso son trazables a la lógica funcional y al diseño de dominio;
- la identidad se resuelve desde contexto y no desde cliente;
- la capa de aplicación mantiene el comportamiento de coordinación, no de negocio;
- las pruebas de resultado y de integración confirman la coherencia con la fuente funcional.
