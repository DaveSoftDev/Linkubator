INSERT INTO User (Email, Name, Alias, Password, EmailConfirmedAt, SecurityStamp)
SELECT @Email, @Name, @Alias, @PasswordHash, @EmailConfirmedAt, @SecurityStamp
WHERE NOT EXISTS (
    SELECT 1
    FROM User
    WHERE Email = @Email
);

UPDATE User
SET Name = COALESCE(Name, @Name),
    Alias = COALESCE(Alias, @Alias),
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
