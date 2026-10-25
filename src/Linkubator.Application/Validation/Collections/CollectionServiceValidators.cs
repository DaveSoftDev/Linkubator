using Linkubator.Application.Dtos.Requests;

namespace Linkubator.Application.Validation.Collections;

public sealed record CollectionServiceValidators(
    IValidator<CollectionGetRequest>? GetCollection = null,
    IValidator<CollectionCreateRequest>? CreateCollection = null,
    IValidator<CollectionUpdateRequest>? UpdateCollection = null,
    IValidator<CollectionRenameRequest>? RenameCollection = null,
    IValidator<CollectionChangeVisibilityRequest>? ChangeCollectionVisibility = null,
    IValidator<CollectionDeleteRequest>? DeleteCollection = null)
{
    public CollectionServiceValidators(
        IValidator<CollectionGetRequest>? getCollection,
        IValidator<CollectionCreateRequest>? createCollection,
        IValidator<CollectionRenameRequest>? renameCollection,
        IValidator<CollectionChangeVisibilityRequest>? changeCollectionVisibility,
        IValidator<CollectionDeleteRequest>? deleteCollection)
        : this(getCollection, createCollection, null, renameCollection, changeCollectionVisibility, deleteCollection)
    {
    }

    public IValidator<CollectionGetRequest> CollectionGetValidator { get; } = GetCollection ?? new CollectionGetValidator();
    public IValidator<CollectionCreateRequest> CollectionCreateValidator { get; } = CreateCollection ?? new CollectionCreateValidator();
    public IValidator<CollectionUpdateRequest> CollectionUpdateValidator { get; } = UpdateCollection ?? new CollectionUpdateValidator();
    public IValidator<CollectionRenameRequest> CollectionRenameValidator { get; } = RenameCollection ?? new CollectionRenameValidator();
    public IValidator<CollectionChangeVisibilityRequest> CollectionChangeVisibilityValidator { get; } = ChangeCollectionVisibility ?? new CollectionChangeVisibilityValidator();
    public IValidator<CollectionDeleteRequest> CollectionDeleteValidator { get; } = DeleteCollection ?? new CollectionDeleteValidator();
}

