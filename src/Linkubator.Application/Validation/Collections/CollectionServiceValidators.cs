using Linkubator.Application.Dtos.Requests;

namespace Linkubator.Application.Validation.Collections;

public sealed record CollectionServiceValidators(
    IValidator<CollectionGetRequest>? GetCollection = null,
    IValidator<CollectionCreateRequest>? CreateCollection = null,
    IValidator<CollectionUpdateRequest>? UpdateCollection = null,
    IValidator<CollectionDeleteRequest>? DeleteCollection = null)
{
    public IValidator<CollectionGetRequest> CollectionGetValidator { get; } = GetCollection ?? new CollectionGetValidator();
    public IValidator<CollectionCreateRequest> CollectionCreateValidator { get; } = CreateCollection ?? new CollectionCreateValidator();
    public IValidator<CollectionUpdateRequest> CollectionUpdateValidator { get; } = UpdateCollection ?? new CollectionUpdateValidator();
    public IValidator<CollectionDeleteRequest> CollectionDeleteValidator { get; } = DeleteCollection ?? new CollectionDeleteValidator();
}

