namespace Genius.Atom.Data.Ef;

/// <summary>
/// Defines a generic repository interface for performing CRUD operations on
/// entities of type <typeparamref name="TEntity"/> with reference IDs of
/// type <typeparamref name="TReference"/>, and projecting results to DTOs
/// of type <typeparamref name="TGetDto"/>. The repository also supports
/// creating entities from DTOs of type <typeparamref name="TCreateDto"/>
/// and updating entities from DTOs of type <typeparamref name="TUpdateDto"/>.
/// The update DTO must include the primary ID and last modified timestamp
/// for concurrency control.
/// </summary>
/// <typeparam name="TKey">The type of the primary key.</typeparam>
/// <typeparam name="TReference">The type of the reference ID.</typeparam>
/// <typeparam name="TGetDto">The type of the DTO used for retrieving entities.</typeparam>
/// <typeparam name="TCreateDto">The type of the DTO used for creating entities.</typeparam>
/// <typeparam name="TUpdateDto">The type of the DTO used for updating entities.</typeparam>
public interface IRepository<TKey, TReference, TGetDto, TCreateDto, TUpdateDto>
    where TKey : notnull
    where TReference : IReference<TKey, TReference>
    where TUpdateDto: IPrimaryId<TKey, TReference>, ITimeStamped
{
    /// <summary>
    /// Gets an entity of type <typeparamref name="TEntity"/> by its reference ID projected to <typeparamref name="TGetDto"/>.
    /// </summary>
    /// <param name="id">The reference ID of the entity.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entity projected to <typeparamref name="TGetDto"/>, or null if not found.</returns>
    Task<TGetDto?> GetByIdAsync(TReference id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity of type <typeparamref name="TEntity"/> by its reference ID projected to <typeparamref name="TGetDto"/>.
    /// </summary>
    /// <param name="id">The reference ID of the entity.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="InvalidOperationException">Thrown when an entity with the specified ID is not found.</exception>
    /// <returns>The entity projected to <typeparamref name="TGetDto"/>.</returns>
    Task<TGetDto> GetByIdOrThrowAsync(TReference id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple entities of type <typeparamref name="TEntity"/> by their reference IDs projected to <typeparamref name="TGetDto"/>.
    /// </summary>
    /// <param name="ids">The reference IDs of the entities.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entities projected to <typeparamref name="TGetDto"/>.</returns>
    Task<IEnumerable<TGetDto>> GetByIdsAsync(IEnumerable<TReference> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities of type <typeparamref name="TEntity"/> projected to <typeparamref name="TGetDto"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The entities projected to <typeparamref name="TGetDto"/>.</returns>
    Task<IEnumerable<TGetDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new entity of type <typeparamref name="TEntity"/> from the provided <typeparamref name="TCreateDto"/>, saves it to the database, and returns its ID and last modified timestamp.
    /// </summary>
    /// <param name="createDto">The DTO containing the data for the new entity.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A DTO containing the ID and last modified timestamp of the created entity.</returns>
    Task<CreatedEntityDto<TKey, TReference>> CreateAsync(TCreateDto createDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity of type <typeparamref name="TEntity"/> based on the provided <typeparamref name="TUpdateDto"/>, saves the changes to the database, and returns its ID and last modified timestamp.
    /// </summary>
    /// <param name="updateDto">The DTO containing the data for the entity to be updated.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A DTO containing the ID and last modified timestamp of the updated entity.</returns>
    Task<UpdatedEntityDto<TKey, TReference>> UpdateAsync(TUpdateDto updateDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an existing entity of type <typeparamref name="TEntity"/> identified by the provided reference ID from the database.
    /// </summary>
    /// <param name="id">The reference ID of the entity to be deleted.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    Task DeleteAsync(TReference id, CancellationToken cancellationToken = default);
}
