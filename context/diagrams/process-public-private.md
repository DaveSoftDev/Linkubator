# Proceso de transiciones público y privado

Diagrama derivado de las invariantes de [domain-model.md → «Público y privado»](../domain-model.md#público-y-privado) y su garantía transaccional descrita en [architecture.md → «Persistencia»](../architecture.md#persistencia).

```mermaid
flowchart TD
    start([Acción sobre colección o enlace])
    action{¿Qué acción realiza el usuario?}

    createCollection[Crea una colección]
    collectionInitial{¿La marca pública al crearla?}
    collectionPrivate[Guardar colección privada]
    collectionPublic[Guardar colección pública]

    makeCollectionPublic[Hace pública una colección existente]
    preserveLinks[Marcar colección pública y conservar estado de enlaces]
    makeCollectionPrivate[Hace privada una colección]
    privateAll[Marcar colección y todos sus enlaces como privados]

    createLink[Crea un enlace en una colección]
    creationCollectionPublic{¿La colección está pública?}
    publicOptionDisabled[Deshabilitar la opción pública<br/>e informar al usuario]
    rejectCreationPublic[Rechazar la creación pública<br/>e informar al usuario]
    linkInitial{¿Lo marca público al crearlo?}
    checkCollection{¿La colección está pública?}
    linkPrivate[Guardar enlace privado]
    linkPublic[Guardar enlace público]
    rejectPublic[Rechazar el cambio a público<br/>e informar al usuario]

    moveLink[Mueve un enlace a otra colección]
    movePrivate[Actualizar colección y dejar el enlace privado]
    makeLinkPublic[Hace público un enlace existente]
    keepLinkPrivate[Conservar el enlace privado]

    start --> action
    action -->|Crear colección| createCollection --> collectionInitial
    collectionInitial -->|No| collectionPrivate
    collectionInitial -->|Sí| collectionPublic

    action -->|Hacer pública una colección| makeCollectionPublic --> preserveLinks
    action -->|Hacer privada una colección| makeCollectionPrivate --> privateAll

    action -->|Crear enlace| createLink --> creationCollectionPublic
    creationCollectionPublic -->|Sí| linkInitial
    creationCollectionPublic -->|No| publicOptionDisabled
    publicOptionDisabled -->|Crear privado| linkPrivate
    publicOptionDisabled -->|Petición manipulada| rejectCreationPublic
    linkInitial -->|No| linkPrivate
    linkInitial -->|Sí| checkCollection
    action -->|Hacer público un enlace| makeLinkPublic --> checkCollection
    checkCollection -->|Sí| linkPublic
    checkCollection -->|No| rejectPublic

    action -->|Mover enlace| moveLink --> movePrivate
    action -->|Hacer privado un enlace| keepLinkPrivate
```
