using DomainExceptions = Linkubator.Domain.Exceptions;
using DomainEntities = Linkubator.Domain.Entities;

namespace Linkubator.Tests.Domain.Entities;

[Trait("Category", "Users")]
public class UserTests
{
    [Fact]
    public void Create_WhenValid_SetsNormalizedValuesAndInitialState()
    {
        DateTime confirmedAt = new(2026, 10, 6, 8, 30, 0, DateTimeKind.Utc);

        DomainEntities.User user = DomainEntities.User.Create(
            " Ana+Tag@Example.com ",
            "  Ana María  ",
            "AnaMaria2026",
            "hashed-password",
            confirmedAt);

        Assert.Equal("ana+tag@example.com", user.Email);
        Assert.Equal("Ana María", user.Name);
        Assert.NotNull(user.Alias);
        Assert.NotEmpty(user.Alias);
        Assert.Equal("hashed-password", user.Password);
        Assert.Equal(confirmedAt, user.EmailConfirmedAt);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEnd);
        Assert.Null(user.LastEmailSentAt);
        Assert.Null(user.LastExistingAccountNoticeAt);
        Assert.NotEmpty(user.SecurityStamp);
        Assert.Matches("^[A-Za-z0-9_-]{43}$", user.SecurityStamp);

        DomainEntities.User other = DomainEntities.User.Create(
            "other@example.com",
            "Other",
            "OtherName2026",
            "hash-2",
            confirmedAt.AddMinutes(1));

        Assert.NotEqual(user.SecurityStamp, other.SecurityStamp);
    }

    [Theory]
    [InlineData(null, "Ana", "Ana 1", "hash")]
    [InlineData("", "Ana", "Ana 1", "hash")]
    [InlineData("   ", "Ana", "Ana 1", "hash")]
    [InlineData("user@example.com", null, "Ana 1", "hash")]
    [InlineData("user@example.com", "", "Ana 1", "hash")]
    [InlineData("user@example.com", "Ana", "", "hash")]
    [InlineData("user@example.com", "Ana", "  ", "hash")]
    [InlineData("user@example.com", "Ana", "Ana 1", "")]
    [InlineData("user@example.com", "Ana", "Ana 1", "   ")]
    public void Create_WhenRequiredDataIsMissing_Throws(string? email, string? name, string? aliasCandidate, string? passwordHash)
    {
        DateTime confirmedAt = new(2026, 10, 6, 8, 30, 0, DateTimeKind.Utc);

        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            DomainEntities.User.Create(email!, name!, aliasCandidate!, passwordHash!, confirmedAt));
    }

    [Fact]
    public void Create_WhenEmailConfirmationDateIsDefault_Throws()
    {
        Assert.Throws<DomainExceptions.NotValidInputException>(() =>
            DomainEntities.User.Create("user@example.com", "Ana", "AnaMaria2026", "hash", default));
    }

    [Fact]
    public void Create_WhenNameIsAtMaximumLength_AcceptsTrimmedName()
    {
        string name = new('n', 100);

        DomainEntities.User user = DomainEntities.User.Create(
            "user@example.com",
            $"  {name}  ",
            "AnaMaria2026",
            "hash",
            new DateTime(2026, 10, 6, 8, 30, 0, DateTimeKind.Utc));

        Assert.Equal(name, user.Name);
    }

    [Fact]
    public void Create_WhenNameExceedsMaximumLength_Throws()
    {
        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            DomainEntities.User.Create(
                "user@example.com",
                new string('n', 101),
                "Ana 1",
                "hash",
                new DateTime(2026, 10, 6, 8, 30, 0, DateTimeKind.Utc)));
    }
}
