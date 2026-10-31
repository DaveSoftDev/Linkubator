namespace Linkubator.Application.Dtos.Requests;

public sealed record LinkMetadataValidationRequest(string? Title, string? Description, string? Image, string BaseUrl);
