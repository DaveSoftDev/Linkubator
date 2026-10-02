# Diagrama de clases

Vista derivada del modelo de dominio. Las propiedades e invariantes tienen como fuente de verdad [domain-model.md](../domain-model.md). Solo se muestran atributos documentados: no se especifican aquí tipos de implementación ni métodos de clase.

`Tag` y `LinkTag` se muestran como entidades previstas para el MVP1; no forman parte del MVP0.

```mermaid
classDiagram
    class User {
        <<entity>>
        Id
        Email
        Name
        Alias
        Password
        EmailConfirmedAt
        SecurityStamp
        FailedLoginAttempts
        LockoutEnd
        LastEmailSentAt
        LastExistingAccountNoticeAt
        CreatedAt
    }

    class Collection {
        <<entity>>
        Id
        UserId
        Name
        Slug
        Description
        IsPublic
        CreatedAt
    }

    class Link {
        <<entity>>
        Id
        UserId
        CollectionId
        UrlOriginal
        UrlNormalized
        Title
        Description
        Image
        IsPublic
        ScrapingStatus
        ScrapingAttempts
        NextScrapingAt
        CreatedAt
    }

    class Tag {
        <<entity>>
        Id
        UserId
        Name
        Slug
        CreatedAt
    }

    class LinkTag {
        <<entity>>
        LinkId
        TagId
    }

    class UserToken {
        <<entity>>
        Id
        UserId
        Purpose
        Token
        NewEmail
        ExpiresAt
        UsedAt
        CreatedAt
    }

    class ScrapingStatus {
        <<enumeration>>
        Pending
        Processing
        Completed
        Failed
    }

    User "1" --> "0..*" Collection : posee
    User "1" --> "0..*" Link : posee
    User "1" --> "0..*" Tag : posee
    User "1" --> "0..*" UserToken : tiene
    Collection "1" --> "0..*" Link : contiene
    Link "1" --> "0..*" LinkTag : se_asocia
    Tag "1" --> "0..*" LinkTag : se_asocia
    Link ..> ScrapingStatus : estado opcional
```

Un usuario con el registro incompleto puede no tener colecciones; una vez completado el registro, tiene al menos una. Cada `LinkTag` asocia exactamente un enlace con una etiqueta, y la pareja `LinkId + TagId` es única. Las reglas de propiedad entre usuarios y de público y privado se detallan en [domain-model.md](../domain-model.md).

Los valores de `ScrapingStatus` son los definidos en [specifications.md → «Estados del scraping»](../specifications.md#estados-del-scraping). `Purpose` se mantiene como atributo porque la documentación describe sus usos, pero no define nombres técnicos para una enumeración.
