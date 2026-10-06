# SDD: Fundaciones

## Propósito

Establecer la base técnica del proyecto: estructura, convenios, arranque y validación de la solución antes de añadir funcionalidad de dominio o de interfaz.

## Fuente funcional

Este SDD se apoya en:

- [architecture.md](../context/architecture.md): plataforma, capas, seguridad técnica y persistencia.
- [roadmap.md](../context/roadmap.md): orden de construcción y criterios de calidad.
- [AGENTS.md](../AGENTS.md): restricciones de edición y reglas de documentación.

## Alcance

Incluye:

- configuración de entorno local;
- logging y observabilidad básica;
- coordenadas de arranque y pruebas base;
- convenciones de revisión y trazabilidad.

Excluye:

- reglas de negocio del producto;
- mensajes de usuario completos;
- algoritmos de dominio;
- cualquier detalle que esté definido en `context/` como regla funcional.

## Decisiones técnicas

### Estructura base

La estructura de la solución, sus referencias entre capas y su validación se definen en [s0.2-crear-solucion/spec.md](s0.2-crear-solucion/spec.md). Este SDD cubre los elementos transversales que completan la base técnica alrededor de esa estructura.

### Entorno de desarrollo

Se prepara un entorno local controlado con configuración explícita para la URL pública, la base de datos local y los servicios auxiliares. La preparación no se convierte en regla funcional del producto; solo sirve a la implementación y a la validación.

### Observabilidad

Los eventos de aplicación se registran con nivel y contexto suficientes para diagnosticar fallos sin incluir contenido sensible ni datos privados en calidad de texto de log.

## Flujo de ejecución

1. Se prepara el entorno de ejecución local.
2. La solución arranca con la configuración de desarrollo.
3. La estructura de proyectos y la composición de dependencias se aplican según [s0.2-crear-solucion/spec.md](s0.2-crear-solucion/spec.md).
4. Las pruebas base validan el arranque mínimo antes de ejecutar las pruebas de comportamiento que correspondan.
5. El cierre del trabajo requiere revisión de trazabilidad y de coherencia con la fuente funcional.

## Persistencia y dependencias

- La infraestructura se comunica con la base de datos mediante adaptadores explícitos.
- Los scripts y la configuración local quedan separados del código de reglas de negocio.
- Los datos sensibles no se almacenan como parte de la lógica de ejecución ni se registran en text logs.

## Invariantes de implementación

- No se crea funcionalidad fuera del alcance documentado.
- La capa web no define reglas del dominio.
- Las pruebas se ejecutan sobre comportamiento real y no sobre mocks sin necesidad.
- La identidad de acceso llega a los casos de uso mediante la abstracción de Application, nunca desde datos enviados por el cliente.

## Trazabilidad

Este SDD soporta la preparación de la base del proyecto y se usa antes de cada sprint como validación de entrada. Debe revisarse junto con:

- [roadmap.md](../context/roadmap.md)
- [AGENTS.md](../AGENTS.md)
- [plans/mvp0-plan.md](../plans/mvp0-plan.md)
- [s0.2-crear-solucion/spec.md](s0.2-crear-solucion/spec.md)

## Riesgos y decisiones pendientes

- Cambios en la pila o las convenciones de arranque pueden requerir adaptar el patrón de validación de capas.
- La configuración local no debe mezclarse con el estado del dominio ni con datos de producción.

## Verificación

Se considera correcto cuando:

- el arranque mínimo funciona en entorno local;
- la observabilidad es útil y no revela contenido sensible;
- la trazabilidad con `context/` permanece clara y sin duplicados.
