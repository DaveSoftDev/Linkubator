# Revisión de impacto tras cambios concurrentes

## Alcance y evidencia

Revisión del 2026-10-30 de los **31 casos** que la reconciliación anterior marcó como no revisados y de los cambios identificables en los contratos de lectura pública, servicios y repositorios. No se han modificado tests ni producción. El diagnóstico compara preparación y aserciones con las fuentes vigentes; no equivale a una auditoría exhaustiva de todos los cambios sin confirmar.

La selección focalizada con código recompilado ejecutó **41/41**, sin omitidos: incluye los 31 casos y diez variantes vecinas de las mismas teorías. La última suite global recompilada, anterior a esta revisión documental, fue **1.833/1.833**.

## Resultado de los 31 casos

| Grupo | Casos | Clasificación | Evidencia |
|---|---:|---|---|
| 1 | 12 | Vigentes | Lectura pública por colección: ausencia, colección sin enlaces públicos, extremos de paginación, límites de conteo y slug compartido. Se prueban los 12 resultados del contrato actual; el caso que exige solo `PageCount` no se presenta como prueba completa del contenido. |
| 1 | 6 | Vigentes | Variantes de tema privado y presentación compartida: respuesta/HTML y ruta esperada. No sustituyen revisión visual y accesible. |
| 3 | 3 | Vigentes | Validación de nombre, fecha de creación y GET de confirmación sin borrado; este último compara snapshot. |
| 3 | 1 | A reforzar: R2 | Fábricas de error de colección. |
| 4 | 1 | Vigente | Configuración de validadores personalizados y predeterminados. |
| 4 | 2 | A reforzar: R2 y R3 | Fábricas de error y pretendida semántica de valor de validadores. |
| 5 | 1 | A reforzar: R4 | Endpoint auxiliar que invoca el analizador de listados. |
| 8 | 5 | Vigentes | Existencia aislada por propietario, rollback de entidad/proyecciones e inserción fallida sin asignar id. El rollback está provocado explícitamente por el test: no se atribuye al repositorio por sí solo. |

Los CSV correspondientes reflejan la revisión y clasifican **27 vigentes y cuatro a reforzar**. No se han retirado ni modificado los tests.

## Hallazgos y propuestas

| ID | Prioridad | Evidencia | Acción propuesta |
|---|---|---|---|
| R1 | Media | El contrato de lectura pública ya no tiene un método independiente de conteo: el repositorio obtiene metadatos y enlaces en `GetCollectionPage`, y las pruebas correspondientes se sustituyeron. Los nuevos tests de límites e integración ejercitan esa API, pero algunos solo verifican cantidad. | Conservar pruebas de contenido, aislamiento y paginación completas ya existentes; revisar específicamente los metadatos y contenido ante los cambios de la colección entre las dos lecturas SQL, sin asumir una instantánea transaccional que no esté definida. |
| R2 | Baja | `CollectionErrors_AllFactoryMethods_ReturnExpectedMetadata` repite cinco veces el mismo `CommonErrors.NotAuthenticated`; el equivalente de enlaces lo repite seis. El nombre «AllFactoryMethods» no demuestra que se enumeren todas las fábricas. | Mantener una comprobación por error distinto, verificar las fábricas realmente cubiertas y renombrar o añadir escenarios faltantes según el contrato, sin borrar cobertura por parecer repetida. |
| R3 | Baja | `LinkServiceValidators_RecordHasExpectedValueSemantics` no compara igualdad, hash ni copias; exige tipos y texto. No demuestra lo que su nombre promete. | Renombrar a la garantía real o comprobar explícitamente el contrato de valor que se desee mantener, sin fijar accidentalmente igualdad de instancias nuevas. |
| R4 | Baja | `PrivateListingQueryEndpoint_UsesProductionAnalyzer` ejecuta el analizador de producción en un endpoint **de test**, con callback artificial. Comprueba integración de ese endpoint con el analizador, no el pipeline HTTP de listados reales. | Renombrar para delimitar su alcance y contrastar sus casos con los tests de rutas productivas del grupo 5; reforzar allí solo un escenario que realmente falte. |

## Qué ha cambiado y qué sigue siendo incierto

- El historial disponible en Git contiene un commit previo a la segunda ronda y un único conjunto de modificaciones sin confirmar. El diff mezcla cambios del usuario y de la auditoría; **no permite fechar ni atribuir con certeza cada edición respecto a cada cierre de grupo**.
- Lectura pública: se retiraron 17 variantes de métodos de conteo y se introdujeron 12 casos de la API integrada, revisados arriba. El nuevo contrato se valida en repositorio y hay pruebas de errores y DTO en el servicio, aunque no es una equivalencia uno a uno de las pruebas retiradas.
- Colecciones, enlaces y persistencia: hay cambios en contratos, entidades, repositorios, servicios y tests, incluidos los cinco casos de persistencia revisados arriba. También se corrigieron durante la auditoría la precedencia del id de ruta y las peticiones nulas con validadores personalizados. No puede concluirse solo a partir de nombres idénticos que los tests previamente clasificados no hayan cambiado.
- Grupos 6 y 7 mantienen sus evidencias focalizadas históricas; el resultado global reciente comprueba que se ejecutan, pero no certifica que cada comportamiento siga siendo exhaustivo ni reemplaza revisión de las dependencias modificadas.
- Las revisiones funcionales, manuales y el cierre de [S12](mvp0-plan.md#s12-cierre-y-hardening-m) siguen pendientes; esta auditoría no los sustituye.

## Acciones aplicadas y repaso focalizado

- **R2:** retirada la repetición de `NotAuthenticated`, añadidos metadatos de fábricas de colección que faltaban y renombrados ambos tests para que no prometan exhaustividad automática.
- **R3:** renombrado el test para describir los tipos expuestos, sin fijar una semántica de igualdad accidental.
- **R4:** renombrado el endpoint auxiliar para explicitar su alcance. El caso de parámetros repetidos se contrasta con `GetLinks_WithRepeatedAndUnsupportedParameters_UsesOnlyFirstApplicableValues` sobre la ruta productiva.
- **R1:** tras consultar al usuario, se sustituyeron las dos consultas por una sentencia SQL que devuelve colección, paginación y enlaces desde una misma instantánea de lectura. No abre una transacción explícita de aplicación; el resultado puede representar el estado anterior si un cambio se confirma después de comenzar la lectura. Se comprueban datos y metadatos antes y después de cambiar privacidad, además de los extremos, aislamiento y HTTP existentes. El test de cambios entre peticiones no fuerza una carrera dentro de la sentencia; la garantía de instantánea se apoya en la semántica de SQLite de una sola sentencia, no en una carrera simulada.

El repaso de los diffs identifica cobertura actual de los cambios más relevantes: rehidratación y separación de enlaces en Domain; proyección con colección descargada; comprobaciones de existencia por propietario; rollback y proyecciones; y precedencia del id de ruta en formularios. Los tests de validadores personalizados cubren peticiones nulas tras el ajuste de `Result`. Este repaso identifica evidencia existente, no establece que cada línea modificada se haya auditado individualmente ni permite atribuir los cambios a una persona.

Verificación de los refuerzos: **5/5** focalizados y suite completa **1.833/1.833**. Tras unificar la lectura pública: **71/71** de repositorio, servicio e integración HTTP y **1.834/1.834** globales, sin omitidos. El inventario del grupo 1 incluye el nuevo caso. Los cambios no confirmados siguen mezclados y la ausencia de una instantánea por grupo limita la comparación histórica.

**Estado:** los cuatro refuerzos están resueltos, los 31 casos tienen clasificación vigente y R1 se ha resuelto mediante una sentencia única. S12 y la revisión humana continúan pendientes. La suite verde no garantiza ausencia de errores ni completa por sí sola la auditoría.
