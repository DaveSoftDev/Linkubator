DELETE FROM CollectionSearch;

INSERT INTO CollectionSearch (rowid, UserId, Name, Description)
SELECT Id, UserId, NormalizeSearchText(Name), NormalizeSearchText(Description)
FROM Collection;
