# Proceso de transiciones público y privado

Diagrama derivado de las invariantes de [domain-model.md → «Público y privado»](../domain-model.md#público-y-privado) y su garantía transaccional descrita en [architecture.md → «Persistencia»](../architecture.md#persistencia).

```mermaid
flowchart TD
    start([El usuario envía un formulario])
    form{¿Qué formulario envía?}

    createCollection[Crea una colección]
    collectionInitial{¿La marca pública al crearla?}
    collectionPrivate[Guardar colección privada]
    collectionPublic[Guardar colección pública]

    editCollection[Edita una colección]
    collectionPublicState{¿El estado elegido es público?}
    preserveLinks[Marcar colección pública y conservar estado de enlaces]
    makeCollectionPrivate[Guardar colección privada]
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
    editLink[Edita un enlace]
    linkPublicState{¿El estado elegido es público?}
    keepLinkPrivate[Conservar el enlace privado]

    start --> form
    form -->|Crear colección| createCollection --> collectionInitial
    collectionInitial -->|No| collectionPrivate
    collectionInitial -->|Sí| collectionPublic

    form -->|Editar colección| editCollection --> collectionPublicState
    collectionPublicState -->|Sí| preserveLinks
    collectionPublicState -->|No| makeCollectionPrivate --> privateAll

    form -->|Crear enlace| createLink --> creationCollectionPublic
    creationCollectionPublic -->|Sí| linkInitial
    creationCollectionPublic -->|No| publicOptionDisabled
    publicOptionDisabled -->|Crear privado| linkPrivate
    publicOptionDisabled -->|Petición manipulada| rejectCreationPublic
    linkInitial -->|No| linkPrivate
    linkInitial -->|Sí| checkCollection
    form -->|Editar enlace| editLink --> linkPublicState
    linkPublicState -->|Sí| checkCollection
    linkPublicState -->|No| keepLinkPrivate
    checkCollection -->|Sí| linkPublic
    checkCollection -->|No| rejectPublic

    form -->|Mover enlace| moveLink --> movePrivate
```
