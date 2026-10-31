# Auditoría individual del grupo 2: propiedad, privacidad e invariantes

## Alcance e inventario

Revisión del 2026-10-30 de preparación, llamada y aserciones de **223 casos ejecutables de 168 métodos**, clasificados en el [inventario individual](test-audit-group-2.csv): **199 vigentes, 23 a reforzar y 1 duplicado demostrado**. Los escenarios nuevos se registran aparte como huecos. «Vigente» acredita la garantía observada, no todas las posibles garantías sugeridas por el nombre.

Distribución: Domain 133 casos, Application 26, Infrastructure 11 e integración HTTP/SQLite 53. Incluye todos los tests actuales de `CollectionTests`, `LinkTests`, `UserTests` y `EntityPublicSurfaceTests`, y la selección por método de propiedad y privacidad de las clases mixtas identificadas en el CSV. Los tests de campos de entidades quedan asignados aquí por ser invariantes del agregado; los tests independientes de políticas y límites de entrada permanecen en el grupo 6. Los tests artificiales de soporte de superficie pública se incluyen para no confundirlos con pruebas del comportamiento del producto.

La selección de clases mixtas no se hace por coincidencia del nombre al ejecutar: el CSV enumera los métodos asignados. CRUD general, búsqueda, identidad de desarrollo y atomicidad de infraestructura permanecen en sus grupos 3–9; los casos transversales de carrera y clave compuesta seleccionados aquí se usan también como evidencia de persistencia. Las lecturas públicas ya inventariadas en el grupo 1 no vuelven a asignarse aquí. La revisión individual de los demás grupos deberá completar la asignación global de los casos todavía no inventariados.

`UserTests` comprueba contratos de entidad que ya existen, incluidos estado incompleto y compleción atómica. No equivale a registro HTTP, autenticación, confirmación de email real o eliminación de cuenta implementados. No se exigen esos flujos futuros como huecos actuales; el aprovisionamiento de desarrollo y su excepción técnica corresponden al grupo 7.

## Fuentes y garantías

| Fuente única | Evidencia revisada | Límite |
|---|---|---|
| [domain-model.md → «Collection»](../context/domain-model.md#collection) | Creación, textos, slug, estado privado por defecto, rechazo de borrado con enlaces o de última colección en aplicación y HTTP. | Los rechazos no equivalen a probar conservación de todos los datos: A8. |
| [domain-model.md → «Link»](../context/domain-model.md#link) | Relación con colección, estado inicial, URL ajustada/normalizada, edición opcional y rechazo atómico en entidad. | Políticas exhaustivas de URL permanecen en grupo 6; no se copia su matriz. |
| [domain-model.md → «User»](../context/domain-model.md#user) | Estado inicial y sello, compleción y rechazo de nueva compleción. | No se acredita cuenta funcional ni formato Argon2id mediante cadenas sintéticas de tests de entidad. |
| [domain-model.md → «Propiedad de los datos»](../context/domain-model.md#propiedad-de-los-datos) | Recursos propios/ajenos, identidad no sustituible en creación, clave compuesta SQLite, relaciones entre entidades y rechazo HTTP. | Casos que solo ejercitan relaciones diferentes pueden no aislar el propietario: H1. |
| [domain-model.md → «Público y privado»](../context/domain-model.md#público-y-privado) | Matriz de colección en Application, publicación rechazada en colección privada, movimientos, propagación y conservación de privados. | Matriz HTTP inicial/final y no-publicación en colección pública tienen huecos: H2–H3. |
| [architecture.md → «Persistencia»](../context/architecture.md#persistencia) | Escritura masiva selectiva, clave compuesta y carreras seleccionadas. | Sin prueba de todos los órdenes de concurrencia: H4 y A9. |
| [decisions.md → «Pendiente de hidratación de la relación inversa de enlaces»](../context/decisions.md#pendiente-de-hidratación-de-la-relación-inversa-de-enlaces) | Rehidratación completa/parcial, navegación descargada y reasignación entre instancias. | No se exige que toda entidad cargada tenga todos sus enlaces; no convertir esa limitación aceptada en defecto. |

## Hallazgos de pruebas existentes

| ID | Prioridad | Hallazgo demostrado | Acción propuesta |
|---|---|---|---|
| A1 | Media | `RemoveLink_WhenLinkDoesNotExist_DoesNothing` crea el enlace con `Link.Create`, que lo asocia a la colección. `AddLink_WhenLinkIsValidAndNotPresent_AddsToCollection` también parte de enlace ya asociado. Los nombres describen precondiciones que no existen. | Preparar ausencia de pertenencia de forma explícita y comprobarla antes de actuar; mantener cobertura independiente de idempotencia. |
| A2 | Media | `RehydrateCollection_WhenCollectionMatchesLink_RebindsTheBidirectionalReference` pasa la misma instancia ya asociada y solo comprueba `CollectionId`: un no-op pasa. `Collection_RehydrateLinks_WhenLinksBelongToCollection_UpdatesBackReferencesAndAllowsPrivacyChange` exige cantidad y privacidad, no las referencias que promete. | Usar otra instancia del mismo identificador; comprobar referencia del enlace, pertenencia exacta y ausencia en la instancia anterior antes de probar privacidad. |
| A3 | Baja | `MoveToCollection_WhenDestinationIsTheSameInstanceButItsIdChanges_UsesReferenceBranch` altera mediante reflexión un identificador ya establecido, escenario no alcanzable por la API pública; el enlace comienza privado y no se comprueban las referencias. | Renombrar como defensa frente a estado artificial y precisar sus aserciones, o retirar justificadamente si no es contrato soportado; no presentar cobertura de flujo ordinario. |
| A4 | Media | En `UserTests.Create_WhenRequiredDataIsMissing_Throws`, el alias `Ana 1` es demasiado corto incluso en las variantes cuyo objetivo es contraseña vacía. El rechazo del alias ocultaría una regresión en la comprobación de contraseña. | Usar valores válidos para los campos no sujetos a prueba y exigir excepción o código adecuado por campo. |
| A5 | Media | `EntityPropertiesDoNotExposePublicSettersForSensitiveMembers` omite silenciosamente propiedades inexistentes. El test puede pasar si desaparece una propiedad esperada de la superficie pública. | Enumerar pares tipo/propiedad aplicables y exigir su existencia además de la ausencia de setter público; no exigir propiedades que la entidad nunca tuvo. |
| A6 | Media | `GetCollections_WhenOtherUsersCollectionExists_DoesNotExposeItOrAcceptUserIdOverride` envía `userId=2` sin obtener el propietario real de la colección ajena sembrada. | Enviar el identificador real y comparar contenido propio y ajeno; mantener el caso de parámetro no aplicable separado de identidad. |
| A7 | Media | `GetLinkManagementPages_WithForeignLink_ShowsGenericUnavailableResource` comprueba estado y mensaje compartidos, pero no equivalencia de respuesta entre recurso ajeno e inexistente. Un dato diferenciador añadido fuera del mensaje pasaría. | Comparar respuesta por operación, normalizando solo datos volátiles demostrados, además de los textos que no deben exponerse. |
| A8 | Media | El borrado HTTP de colección con enlaces solo compara recuento de colecciones, no conservación del enlace. En SQLite, el borrado ajeno de colección solo comprueba existencia; el de enlace comprueba ausencia de una proyección del usuario incorrecto que ya no existía, no la del propietario real. | Comparar instantáneas del recurso y su proyección real antes/después; verificar que permanece el enlace que bloquea el borrado. |
| A9 | Media | `DeleteCollections_WhenDeletingLastCollectionsConcurrently_PreservesOneCollection` exige un éxito y un fallo, pero no el motivo del fallo ni que sobreviva la colección de la operación rechazada. | Exigir el código de última colección y relacionar resultado con identificadores persistidos. |
| A10 | Baja | El alias de usuario completo o recién completado solo se exige no nulo/no vacío, no su valor exacto. La compleción tampoco afirma conservación de email y sello inicial. | Comparar alias exacto y campos que no deben cambiar; no pedir regeneración de sello por completar si no está especificada. |
| D1 | Baja | `ChangePrivacy_WhenLinkBelongsToDifferentCollection_ThrowsWithoutPartialMutation` reproduce preparación, llamada y aserciones de `ChangePrivacy_WhenLinkDoesNotBelongToCollection_ThrowsWithoutPartialMutation`. | Eliminar uno de los dos tras conservar la garantía compartida. |

Los 23 casos a reforzar incluyen las variantes parametrizadas completas del método donde se debe corregir la preparación; no significan 23 fallos actuales de producción. Ningún test está clasificado como obsoleto solo por cubrir un contrato técnico.

## Huecos de escenarios

| ID | Prioridad | Escenario sin cobertura discriminante en el grupo | Acción propuesta |
|---|---|---|---|
| H1 | Media | Rehidratación de colección rechazada por usuario ajeno prepara también un identificador de colección distinto. Así no aísla la comprobación de propietario, a diferencia de algunas pruebas de `Link`. | Misma identidad numérica de colección en dos instancias de propietarios distintos; exigir rechazo y estado intacto en ambas. |
| H2 | Media | La matriz de colección está cubierta con fakes, pero la edición HTTP válida siempre parte de colección pública con un único enlace público. | Probar HTTP privada → pública y privada → privada con enlaces privados, además de pública → pública con mezcla, y comprobar persistencia y no publicación accidental. |
| H3 | Media | Falta un caso de entidad que omita la opción de privacidad al crear un enlace en colección pública. Crear en colección privada podría ocultar una implementación que heredase la privacidad de la colección. | Exigir enlace privado por defecto en colección pública, con pertenencia conservada. |
| H4 | Media | Las carreras seleccionadas sincronizan el inicio, pero no fuerzan ambos órdenes de adquisición transaccional. El caso de carrera de creación permite rechazo y puede no llegar a comprobar una creación ganadora. | Dos recorridos controlados de publicación/privatización con orden observable y resultados explícitos; no sustituir por bucles ni sleeps. Coordinar con grupo 8. |

## Ejecución y estado

Checkout inicial `0ffbf41` con cambios ya existentes de grupos anteriores y trabajo simultáneo en entidades y páginas privadas. No se cambiaron tests ni producción en esta auditoría; solo documentos de seguimiento. Se recompiló Debug antes del descubrimiento.

- Regresión amplia de las clases relacionadas: **597/597** superados; incluye casos de otros grupos y no equivale a inventario auditado.
- Filtro exacto generado desde los métodos del CSV: **223/223** superados, sin omitidos. Se incluyen todas las variantes descubiertas de cada método seleccionado.

Comando reproducible de selección exacta:

```powershell
$cases = Import-Csv plans\test-audit-group-2.csv
$methods = $cases.Caso | ForEach-Object { ($_ -split '\(')[0] } | Sort-Object -Unique
$filter = ($methods | ForEach-Object { 'FullyQualifiedName=' + $_ }) -join '|'
dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-build --no-restore --filter $filter -v minimal
```

**Diagnóstico inicial conservado como historial.** Todas sus acciones se aprobaron y resolvieron; el cierre actualizado figura a continuación. El CSV conserva identificador de caso, archivo, método, clasificación y hallazgos. Los grupos 3–9 deberán conservar sus propios inventarios para completar la asignación global; las revisiones manuales y los flujos futuros no se dan por realizados aquí.


## Cierre automatizado

Acciones A1-A10, D1 y H1-H4 resueltas el 2026-10-30:

- A1-A2: pertenencia ausente preparada antes de actuar; referencias bidireccionales y desvinculado de la instancia anterior comprobados.
- A3: caso artificial renombrado, enlace inicialmente publico y pertenencia conservada comprobados.
- A4/A10: campos no sujetos a prueba validos, codigo exacto de rechazo, alias exacto y email/sello conservados al completar.
- A5: existencia obligatoria de cada par tipo/propiedad y ausencia de setters publicos.
- A6-A7: identidad ajena real y equivalencia completa HTTP por operacion; no fue necesario normalizar datos volatiles.
- A8-A9: estado y proyecciones del propietario real preservados; enlace que bloquea borrado intacto; rechazo de ultima coleccion con codigo y superviviente exactos.
- D1: duplicado retirado, conservando su prueba equivalente.
- H1: rechazo de propietario distinto con el mismo identificador numerico y referencias intactas.
- H2-H3: matriz HTTP ampliada y enlace privado por defecto incluso en coleccion publica.
- H4: cuatro variantes de crear/publicar antes o despues de privatizar, coordinadas con la factoria existente mediante senal explicita; resultados, cantidades y estados finales exigidos sin sleeps ni reintentos.

Inventario actualizado: **232 casos vigentes y unicos**, con `EstadoInicial` para conservar el historial: 223 iniciales menos un duplicado mas diez casos/variantes nuevos. El total global incluye cambios simultaneos de otros grupos y no se deduce solo de este balance.

Validacion Debug: **232/232** del filtro exacto del CSV y **1.725/1.725** de la suite global, sin omitidos. Comando global: `dotnet test tests\Linkubator.Tests\Linkubator.Tests.csproj --configuration Debug --no-restore -v minimal`. Solo se modificaron tests y documentos de este lote; no se atribuyen a esta tarea los cambios previos de produccion del checkout.

**Cerrada la revision individual automatizada del alcance del grupo 2.** No certifica ausencia universal de defectos ni implementa flujos futuros de cuenta. Los cambios posteriores requieren revisar los casos afectados.
