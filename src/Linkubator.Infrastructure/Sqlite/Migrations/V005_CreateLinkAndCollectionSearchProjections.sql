CREATE VIRTUAL TABLE IF NOT EXISTS LinkSearch USING fts5 (
    UserId UNINDEXED,
    UrlOriginal,
    Title,
    Description,
    CollectionName,
    tokenize = "unicode61 remove_diacritics 2 categories 'L* N*'"
);

CREATE VIRTUAL TABLE IF NOT EXISTS CollectionSearch USING fts5 (
    UserId UNINDEXED,
    Name,
    tokenize = "unicode61 remove_diacritics 2 categories 'L* N*'"
);
