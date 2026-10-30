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
        Assert.Equal("anamaria2026", user.Alias);
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
    [InlineData(null, "Ana", "AnaMaria2026", "hash", "NotValidInput")]
    [InlineData("", "Ana", "AnaMaria2026", "hash", "NotValidInput")]
    [InlineData("   ", "Ana", "AnaMaria2026", "hash", "NotValidInput")]
    [InlineData("user@example.com", null, "AnaMaria2026", "hash", "NotValidInput")]
    [InlineData("user@example.com", "", "AnaMaria2026", "hash", "NotValidInput")]
    [InlineData("user@example.com", "Ana", "", "hash", "NotValidInput")]
    [InlineData("user@example.com", "Ana", "  ", "hash", "NotValidInput")]
    [InlineData("user@example.com", "Ana", "AnaMaria2026", "", "NotValidInput")]
    [InlineData("user@example.com", "Ana", "AnaMaria2026", "   ", "NotValidInput")]
    public void Create_WhenRequiredDataIsMissing_Throws(string? email, string? name, string? aliasCandidate, string? passwordHash, string expectedCode)
    {
        DateTime confirmedAt = new(2026, 10, 6, 8, 30, 0, DateTimeKind.Utc);

        DomainExceptions.DomainException exception = Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            DomainEntities.User.Create(email!, name!, aliasCandidate!, passwordHash!, confirmedAt));

        Assert.Equal(expectedCode, exception.Code);
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
                "AnaMaria2026",
                "hash",
                new DateTime(2026, 10, 6, 8, 30, 0, DateTimeKind.Utc)));
    }

    [Fact]
    public void CreateIncompleteRegistration_WhenValid_SetsOnlyEmailAndSecurityStamp()
    {
        DomainEntities.User user = DomainEntities.User.CreateIncompleteRegistration("  Nuevo@Example.com  ");

        Assert.Equal("nuevo@example.com", user.Email);
        Assert.Null(user.Name);
        Assert.Null(user.Alias);
        Assert.Null(user.Password);
        Assert.Null(user.EmailConfirmedAt);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEnd);
        Assert.Null(user.LastEmailSentAt);
        Assert.Null(user.LastExistingAccountNoticeAt);
        Assert.NotEmpty(user.SecurityStamp);
        Assert.Matches("^[A-Za-z0-9_-]{43}$", user.SecurityStamp);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    public void CreateIncompleteRegistration_WhenEmailIsMissingOrInvalid_Throws(string? email)
    {
        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            DomainEntities.User.CreateIncompleteRegistration(email!));
    }

    [Fact]
    public void CompleteRegistration_WhenRegistrationIsIncomplete_SetsTheFourValuesAtOnce()
    {
        DomainEntities.User user = DomainEntities.User.CreateIncompleteRegistration("user@example.com");

        string stamp = user.SecurityStamp;
        string email = user.Email;
        DateTime confirmedAt = new(2026, 10, 6, 8, 30, 0, DateTimeKind.Utc);

        user.CompleteRegistration("  Ana María  ", "AnaMaria2026", "hashed-password", confirmedAt);

        Assert.Equal("Ana María", user.Name);
        Assert.Equal("anamaria2026", user.Alias);
        Assert.Equal("hashed-password", user.Password);
        Assert.Equal(confirmedAt, user.EmailConfirmedAt);
        Assert.Equal(stamp, user.SecurityStamp);
        Assert.Equal(email, user.Email);
    }

    [Fact]
    public void CompleteRegistration_WhenRegistrationIsAlreadyCompleted_ThrowsAndKeepsTheValues()
    {
        DomainEntities.User user = DomainEntities.User.Create(
            "user@example.com",
            "Ana",
            "AnaMaria2026",
            "hash",
            new DateTime(2026, 10, 6, 8, 30, 0, DateTimeKind.Utc));

        string? alias = user.Alias;

        Assert.Throws<DomainExceptions.NotValidInputException>(() =>
            user.CompleteRegistration("Other", "OtherName2026", "hash-2", new DateTime(2026, 10, 7, 8, 30, 0, DateTimeKind.Utc)));

        Assert.Equal("Ana", user.Name);
        Assert.Equal(alias, user.Alias);
        Assert.Equal("hash", user.Password);
        Assert.Equal(new DateTime(2026, 10, 6, 8, 30, 0, DateTimeKind.Utc), user.EmailConfirmedAt);
    }

    [Theory]
    [InlineData("user@example.com", null, "AnaMaria2026", "hash")]
    [InlineData("user@example.com", "", "AnaMaria2026", "hash")]
    [InlineData("user@example.com", "Ana", null, "hash")]
    [InlineData("user@example.com", "Ana", "AnaMaria2026", null)]
    [InlineData("user@example.com", "Ana", "AnaMaria2026", "   ")]
    public void CompleteRegistration_WhenRequiredDataIsMissing_ThrowsAndKeepsTheRegistrationIncomplete(
        string email,
        string? name,
        string? aliasCandidate,
        string? passwordHash)
    {
        DomainEntities.User user = DomainEntities.User.CreateIncompleteRegistration(email);

        DateTime confirmedAt = new(2026, 10, 6, 8, 30, 0, DateTimeKind.Utc);

        Assert.ThrowsAny<DomainExceptions.DomainException>(() =>
            user.CompleteRegistration(name!, aliasCandidate!, passwordHash!, confirmedAt));

        Assert.Null(user.Name);
        Assert.Null(user.Alias);
        Assert.Null(user.Password);
        Assert.Null(user.EmailConfirmedAt);
    }

    [Fact]
    public void CompleteRegistration_WhenEmailConfirmationDateIsDefault_Throws()
    {
        DomainEntities.User user = DomainEntities.User.CreateIncompleteRegistration("user@example.com");

        Assert.Throws<DomainExceptions.NotValidInputException>(() =>
            user.CompleteRegistration("Ana", "AnaMaria2026", "hash", default));
    }
}
