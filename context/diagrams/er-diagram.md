# Diagrama ER

Vista derivada del modelo de dominio y de la arquitectura. Las propiedades e invariantes tienen como fuente de verdad [domain-model.md](../domain-model.md); la persistencia y los índices, [architecture.md → «Persistencia»](../architecture.md#persistencia).

La multiplicidad User–Collection contempla todos los estados de la cuenta; la condición para las cuentas completadas está en [domain-model.md → «User»](../domain-model.md#user).

```mermaid
erDiagram
    User {
        INTEGER Id PK
        TEXT Email UK
        TEXT Name "NULL mientras el registro está incompleto"
        TEXT Alias UK "NULL mientras el registro está incompleto"
        TEXT Password "Hash PHC; NULL mientras el registro está incompleto"
        DATETIME EmailConfirmedAt "NULL mientras el registro está incompleto"
        TEXT SecurityStamp
        INTEGER FailedLoginAttempts
        DATETIME LockoutEnd "NULL si no hay bloqueo activo"
        DATETIME LastEmailSentAt "NULL si aun no se ha enviado un correo con token"
        DATETIME LastExistingAccountNoticeAt "NULL si aun no se ha enviado el aviso"
        DATETIME CreatedAt
    }

    Collection {
        INTEGER Id PK
        INTEGER UserId FK
        TEXT Name
        TEXT Slug
        TEXT Description "NULL si no se informa"
        BOOLEAN IsPublic
        DATETIME CreatedAt
    }

    Link {
        INTEGER Id PK
        INTEGER UserId FK
        INTEGER CollectionId FK
        TEXT UrlOriginal
        TEXT UrlNormalized
        TEXT Title "NULL si no se informa"
        TEXT Description "NULL si no se informa"
        TEXT Image "NULL si no se informa"
        BOOLEAN IsPublic
        TEXT ScrapingStatus
        DATETIME CreatedAt
    }

    Tag {
        INTEGER Id PK
        INTEGER UserId FK
        TEXT Name
        TEXT Slug
        DATETIME CreatedAt
    }

    LinkTag {
        INTEGER LinkId PK, FK
        INTEGER TagId PK, FK
    }

    UserToken {
        INTEGER Id PK
        INTEGER UserId FK
        TEXT Purpose
        TEXT Token UK "Hash del token"
        TEXT NewEmail "NULL salvo en cambio de email"
        DATETIME ExpiresAt
        DATETIME UsedAt "NULL mientras no se haya usado"
        DATETIME InvalidatedAt "NULL mientras no se haya invalidado"
        DATETIME CreatedAt
    }

    User ||--o{ Collection : posee
    User ||--o{ Link : posee
    User ||--o{ Tag : posee
    User ||--o{ UserToken : tiene
    Collection ||--o{ Link : contiene
    Link ||--o{ LinkTag : se_asocia
    Tag ||--o{ LinkTag : se_asocia
```
