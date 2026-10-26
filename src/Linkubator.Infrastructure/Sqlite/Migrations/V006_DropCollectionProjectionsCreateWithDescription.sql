DROP TABLE IF EXISTS CollectionSearch;

CREATE VIRTUAL TABLE IF NOT EXISTS CollectionSearch USING fts5 (
    UserId UNINDEXED,
    Name,
    Description,
    tokenize = "unicode61 remove_diacritics 2 categories 'L* N*'"
);

INSERT INTO CollectionSearch (rowid, UserId, Name, Description)
SELECT
    c.Id,
    c.UserId,
    c.Name,
    COALESCE(c.Description, '')
FROM Collection AS c;
