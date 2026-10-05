# Diagrama de clases

Vista derivada del modelo de dominio. Las propiedades e invariantes tienen como fuente de verdad [domain-model.md](../domain-model.md). Solo se muestran atributos documentados: no se especifican aquí tipos de implementación ni métodos de clase.

La multiplicidad User–Collection contempla todos los estados de la cuenta; la condición para las cuentas completadas está en [domain-model.md → «User»](../domain-model.md#user).

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
        InvalidatedAt
        CreatedAt
    }

    class ScrapingStatus {
        <<enumeration>>
    }

    User "1" --> "0..*" Collection : posee
    User "1" --> "0..*" Link : posee
    User "1" --> "0..*" Tag : posee
    User "1" --> "0..*" UserToken : tiene
    Collection "1" --> "0..*" Link : contiene
    Link "1" --> "0..*" LinkTag : se_asocia
    Tag "1" --> "0..*" LinkTag : se_asocia
    Link ..> ScrapingStatus : `NotRequested`
```
