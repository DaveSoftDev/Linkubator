using DomainEnums = Linkubator.Domain.Enums;

namespace Linkubator.Tests.Domain.Enums;

public class DomainEnumerationsTests
{
    [Fact]
    public void ScrapingStatusContainsExactlyTheSpecifiedValues()
    {
        Assert.Equal(
            new[] { "NotRequested", "Pending", "Processing", "Completed", "Failed" },
            Enum.GetNames<DomainEnums.ScrapingStatus>());
    }

    [Fact]
    public void PurposeContainsExactlyTheSpecifiedValues()
    {
        Assert.Equal(
            new[] { "CompleteRegistration", "PasswordReset", "EmailChange", "AccountDeletion" },
            Enum.GetNames<DomainEnums.Purpose>());
    }
}