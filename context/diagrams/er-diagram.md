# Diagrama ER

Vista derivada del modelo de dominio y de la arquitectura. Las propiedades e invariantes tienen como fuente de verdad [domain-model.md](../domain-model.md); la persistencia y los índices, [architecture.md](../architecture.md).

```mermaid
erDiagram
    User {
        INTEGER Id PK
        TEXT Email UK
        TEXT Name "NULL mientras el registro esta incompleto"
        TEXT Alias UK "NULL mientras el registro esta incompleto"
        TEXT Password "Hash PHC; NULL mientras el registro esta incompleto"
        DATETIME EmailConfirmedAt "NULL mientras el registro esta incompleto"
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
        TEXT ScrapingStatus "NULL si no se ha solicitado"
        INTEGER ScrapingAttempts
        DATETIME NextScrapingAt "NULL si no hay otro intento"
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

## Restricciones e índices

Un usuario con el registro incompleto todavía no tiene colecciones; una vez completado, tiene al menos una. Cada enlace pertenece a un usuario y a una colección. La propiedad compartida entre ambos se garantiza además con la clave foránea compuesta `Link (UserId, CollectionId)` → `Collection (UserId, Id)`. Cada etiqueta y cada token pertenecen a un usuario. La relación entre enlaces y etiquetas usa la clave compuesta `LinkTag (LinkId, TagId)`.

Índices y restricciones únicos definidos:

- `User (Email)` y `User (Alias)`.
- `Collection (UserId, Slug)`.
- `Collection (UserId, Id)`, índice único requerido por la clave foránea compuesta desde `Link`.
- `Link (UserId, UrlNormalized)`.
- `Tag (UserId, Slug)`.
- `LinkTag (LinkId, TagId)`, clave primaria compuesta.
- `UserToken (Token)`.

También están previstos índices por usuario, colección, estado público y fecha, y un índice de `UserToken` por usuario y propósito. La documentación aún no especifica las columnas concretas ni el orden de esos índices no únicos. FTS5 es una proyección de búsqueda aparte, no una relación ER normalizada.

Los identificadores son `INTEGER PRIMARY KEY`, salvo la clave compuesta de `LinkTag`. Los demás tipos del diagrama son descriptivos: la documentación no fija aún su declaración SQLite exacta.