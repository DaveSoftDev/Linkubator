INSERT INTO User (Email, Name, Alias, Password, EmailConfirmedAt, SecurityStamp)
SELECT @Email, 'Demo User', 'my-demo-user', @PasswordHash, @EmailConfirmedAt, @SecurityStamp
WHERE NOT EXISTS (
    SELECT 1
    FROM User
    WHERE Email = @Email
);

UPDATE User
SET Name = COALESCE(Name, 'Demo User'),
    Alias = COALESCE(Alias, 'my-demo-user'),
    Password = COALESCE(Password, @PasswordHash),
    EmailConfirmedAt = COALESCE(EmailConfirmedAt, @EmailConfirmedAt)
WHERE Email = @Email
  AND (Name IS NULL
       OR Alias IS NULL
       OR Password IS NULL
       OR EmailConfirmedAt IS NULL);

INSERT INTO Collection (UserId, Name, Slug, Description, IsPublic)
SELECT Id, @CollectionName, @CollectionSlug, NULL, 0
FROM User
WHERE Email = @Email
  AND NOT EXISTS (
      SELECT 1
      FROM Collection
      WHERE Collection.UserId = User.Id
  );

INSERT INTO CollectionSearch (rowid, UserId, Name, Description)
SELECT Collection.Id, Collection.UserId, NormalizeSearchText(Collection.Name), NormalizeSearchText(Collection.Description)
FROM Collection
INNER JOIN User ON User.Id = Collection.UserId
WHERE User.Email = @Email
  AND Collection.Slug = @CollectionSlug
  AND NOT EXISTS (
      SELECT 1
      FROM CollectionSearch
      WHERE CollectionSearch.rowid = Collection.Id
        AND CollectionSearch.UserId = Collection.UserId
  );
