CREATE TABLE IF NOT EXISTS User (
    Id INTEGER PRIMARY KEY,
    Email TEXT NOT NULL UNIQUE,
    Name TEXT NULL,
    Alias TEXT NULL UNIQUE,
    Password TEXT NULL,
    EmailConfirmedAt TEXT NULL,
    SecurityStamp TEXT NOT NULL,
    FailedLoginAttempts INTEGER NOT NULL DEFAULT 0,
    LockoutEnd TEXT NULL,
    LastEmailSentAt TEXT NULL,
    LastExistingAccountNoticeAt TEXT NULL,
    CreatedAt TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now'))
);

CREATE TABLE IF NOT EXISTS Collection (
    Id INTEGER PRIMARY KEY,
    UserId INTEGER NOT NULL,
    Name TEXT NOT NULL,
    Slug TEXT NOT NULL,
    Description TEXT NULL,
    IsPublic INTEGER NOT NULL DEFAULT 0 CHECK (IsPublic IN (0, 1)),
    CreatedAt TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now')),
    UNIQUE (UserId, Slug),
    UNIQUE (UserId, Id),
    FOREIGN KEY (UserId) REFERENCES User (Id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS Link (
    Id INTEGER PRIMARY KEY,
    UserId INTEGER NOT NULL,
    CollectionId INTEGER NOT NULL,
    UrlOriginal TEXT NOT NULL,
    UrlNormalized TEXT NOT NULL,
    Title TEXT NULL,
    Description TEXT NULL,
    Image TEXT NULL,
    IsPublic INTEGER NOT NULL DEFAULT 0 CHECK (IsPublic IN (0, 1)),
    ScrapingStatus INTEGER NOT NULL DEFAULT 0,
    CreatedAt TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now')),
    UNIQUE (UserId, UrlNormalized),
    FOREIGN KEY (UserId) REFERENCES User (Id) ON DELETE RESTRICT,
    FOREIGN KEY (UserId, CollectionId) REFERENCES Collection (UserId, Id) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS idx_collection_is_public
    ON Collection (IsPublic, UserId);

CREATE INDEX IF NOT EXISTS idx_link_user_id
    ON Link (UserId, CreatedAt DESC);

CREATE INDEX IF NOT EXISTS idx_link_collection_id
    ON Link (CollectionId, UserId);

CREATE INDEX IF NOT EXISTS idx_link_is_public
    ON Link (IsPublic, UserId);

CREATE INDEX IF NOT EXISTS idx_link_scraping_status
    ON Link (ScrapingStatus, UserId);
