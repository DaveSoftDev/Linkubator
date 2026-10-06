# SDD: S0.2 - Crear solución y referencias entre capas

## Propósito

Definir la estructura inicial de la solución en los proyectos Domain, Application, Infrastructure, Web y Tests, junto con sus referencias permitidas y la estrategia para verificarlas antes de añadir comportamiento de producto.

## Fuente funcional

Este SDD se apoya en:

- [architecture.md → «Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos): matriz de referencias y definición del grafo productivo.
- [architecture.md → «Capas»](../../context/architecture.md#capas): responsabilidad de cada capa.
- [mvp0-plan.md → «S0: Fundaciones (M)»](../../plans/mvp0-plan.md#s0-fundaciones-m): tareas de creación de la solución y de su test de dependencias.
- [AGENTS.md](../../AGENTS.md): restricciones de documentación e invariantes críticas.

## Alcance

Incluye:

- la solución y sus cinco proyectos;
- las referencias entre los proyectos de producción;
- el uso de Infrastructure desde Web en el composition root;
- las referencias de Tests para validar los proyectos de producción;
- la verificación de dependencias prevista para la fundación técnica.

Excluye:

- configuración del entorno, analizadores y convenciones de repositorio;
- implementación de adaptadores, casos de uso, páginas o reglas de dominio;
- el mecanismo concreto que demuestre el límite interno del composition root, distinto del test de referencias entre proyectos.

## Decisiones técnicas

### Estructura de proyectos

El grafo productivo, la condición de Tests y la matriz vinculante de referencias se definen en [architecture.md → «Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos). Cada proyecto conserva la responsabilidad que le asigna [architecture.md → «Capas»](../../context/architecture.md#capas).

### Composition root

Web registra las implementaciones de Infrastructure durante el arranque en el composition root, conforme a [architecture.md → «Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos). El componente concreto que delimita ese punto de composición sigue pendiente; el flujo de negocio continúa por Application.

### Verificación de capas

El proyecto Tests incorpora una comprobación de referencias que debe detectar las dependencias de producción prohibidas y cualquier referencia desde producción hacia Tests. La validación específica de que Web solo consume Infrastructure en el composition root requiere un mecanismo adicional, pendiente de decidir.

## Flujo de ejecución

1. Se crean los cinco proyectos de la solución con las referencias de [architecture.md → «Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos).
2. Web construye el host y registra Infrastructure desde el composition root.
3. Las páginas alcanzan los casos de uso a través de Application.
4. Tests ejecuta la comprobación de dependencias sin convertirse en dependencia de producción.

## Persistencia y dependencias

Esta fundación no implementa persistencia ni adaptadores. Solo deja preparada la separación de dependencias necesaria para incorporarlos en etapas posteriores.

## Invariantes de implementación

- El grafo productivo y la condición de Tests cumplen [architecture.md → «Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos).
- La comprobación de dependencias no añade referencias de producción fuera de la matriz.
- La comprobación del límite interno del composition root se incorpora cuando exista una decisión que lo defina.

## Trazabilidad

- La creación de la solución responde a [mvp0-plan.md → «S0: Fundaciones (M)»](../../plans/mvp0-plan.md#s0-fundaciones-m).
- La validación de referencias corresponde al test de dependencias descrito en esa misma sección.
- [fundaciones.md](../fundaciones.md) conserva el diseño transversal de entorno, observabilidad y arranque, y enlaza a esta spec para el diseño de las capas.

## Riesgos y decisiones pendientes

- Queda por definir el límite concreto del composition root y el mecanismo que demuestre que Web solo usa tipos de Infrastructure dentro de él.
- Una comprobación de referencias de ensamblado no demuestra por sí sola ese límite interno de Web.

## Verificación

Se considera correcto cuando:

- existen los cinco proyectos definidos por este SDD;
- sus referencias cumplen [architecture.md → «Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos);
- el test de dependencias detecta referencias de producción prohibidas y cualquier referencia de producción a Tests;
- cuando se acuerde el límite del composition root, su mecanismo de comprobación demuestra que Web lo respeta;
- la solución compila y la trazabilidad con las fuentes permanece clara;
- la matriz de referencias sigue definida únicamente en [architecture.md → «Dependencias entre proyectos»](../../context/architecture.md#dependencias-entre-proyectos).
