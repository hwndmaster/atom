using Genius.Atom.Data;
using Genius.Atom.Data.Ef;
using Genius.Atom.Data.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Genius.Atom.Web.Controllers;

public abstract class BaseCrudController<TKey, TReference, TData, TRepository, TCreateRequest, TUpdateRequest> : BaseController
    where TKey : notnull
    where TReference : IReference<TKey, TReference>
    where TData : class
    where TRepository : IRepository<TKey, TReference, TData, TCreateRequest, TUpdateRequest>
    where TUpdateRequest : IPrimaryId<TKey, TReference>, ITimeStamped
{
    private readonly IRequestValidators _requestValidators;

    protected BaseCrudController(TRepository repository, IRequestValidators requestValidators)
    {
        Repository = repository.NotNull();
        _requestValidators = requestValidators.NotNull();
    }

    protected TRepository Repository { get; }

    [HttpGet]
    public async Task<IEnumerable<TData>> GetAll(CancellationToken cancellationToken)
    {
        return await Repository.GetAllAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    [HttpPost]
    public async Task<ActionResult<CreatedEntityDto<TKey, TReference>>> Create([FromBody] TCreateRequest createRequest, CancellationToken cancellationToken)
    {
        if (createRequest is null)
        {
            return BadRequest("The request message cannot be null.");
        }

        var validationResults = await _requestValidators.ValidateAsync(createRequest, cancellationToken).ToArrayAsync(cancellationToken);
        if (validationResults.Length > 0)
        {
            return ValidationProblem(validationResults.ToValidationProblem());
        }

        var preAddResult = await PreAddAsync(createRequest, cancellationToken).ConfigureAwait(false);
        if (!preAddResult.Success)
        {
            return BadRequest(preAddResult.ErrorMessage ?? "Pre-add checks failed.");
        }

        CreatedEntityDto<TKey, TReference> createdEntity = await Repository.CreateAsync(createRequest, cancellationToken: cancellationToken).ConfigureAwait(false);
        await PostAddAsync(createRequest, createdEntity.EntityId, cancellationToken).ConfigureAwait(false);
        return createdEntity;
    }

    [HttpPut]
    public async Task<ActionResult<UpdatedEntityDto<TKey, TReference>>> Update([FromBody] TUpdateRequest updateRequest, CancellationToken cancellationToken)
    {
        if (updateRequest is null)
        {
            return BadRequest("The request message cannot be null.");
        }

        var validationResults = await _requestValidators.ValidateAsync(updateRequest, cancellationToken).ToArrayAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (validationResults.Length > 0)
        {
            return ValidationProblem(validationResults.ToValidationProblem());
        }

        var preUpdateResult = await PreUpdateAsync(updateRequest, cancellationToken).ConfigureAwait(false);
        if (!preUpdateResult.Success)
        {
            return BadRequest(preUpdateResult.ErrorMessage ?? "Pre-update checks failed.");
        }

        var updatedEntity = await Repository.UpdateAsync(updateRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        await PostUpdateAsync(updateRequest, cancellationToken).ConfigureAwait(false);

        return Ok(updatedEntity);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute] TReference id, CancellationToken cancellationToken)
    {
        await Repository.DeleteAsync(id, cancellationToken: cancellationToken).ConfigureAwait(false);
        return Ok();
    }

    /// <summary>
    /// Called before adding a new <typeparamref name="TData"/> record to the repository.
    /// Override this method to perform custom logic before the add operation.
    /// </summary>
    /// <param name="createRequest">The create request message.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="PreOperationResult"/> indicating success or failure with an optional error message.</returns>
    protected virtual Task<PreOperationResult<TCreateRequest>> PreAddAsync(TCreateRequest createRequest, CancellationToken cancellationToken)
    {
        return Task.FromResult(PreOperationResult<TCreateRequest>.Ok);
    }

    /// <summary>
    /// Called after a new <typeparamref name="TData"/> record is added to the repository.
    /// Override this method to perform custom logic after the add operation.
    /// </summary>
    /// <param name="createRequest">The create request message.</param>
    /// <param name="createdId">The ID of the newly created item.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task PostAddAsync(TCreateRequest createRequest, IReference<TKey> createdId, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called before updating a <typeparamref name="TData"/> record in the repository.
    /// Override this method to perform custom logic before the update operation.
    /// </summary>
    /// <param name="updateRequest">The update request message.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="PreOperationResult"/> indicating success or failure with an optional error message.</returns>
    protected virtual Task<PreOperationResult<TUpdateRequest>> PreUpdateAsync(TUpdateRequest updateRequest, CancellationToken cancellationToken)
    {
        return Task.FromResult(PreOperationResult<TUpdateRequest>.Ok);
    }

    /// <summary>
    /// Called after updating an existing <typeparamref name="TData"/> record in the repository.
    /// Override this method to perform custom logic after the update operation.
    /// </summary>
    /// <param name="updateRequest">The update request message.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task PostUpdateAsync(TUpdateRequest updateRequest, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Represents the result of a pre-operation validation (PreAdd or PreUpdate).
    /// </summary>
    /// <typeparam name="TRequest">The type of the request message.</typeparam>
    /// <param name="Success">Indicates whether the validation succeeded.</param>
    /// <param name="ErrorMessage">Optional error message when validation fails.</param>
    /// <param name="UpdatedRequest">Optional updated request message.</param>
    protected record PreOperationResult<TRequest>(bool Success, string? ErrorMessage = null, TRequest? UpdatedRequest = default)
    {
        /// <summary>
        /// Gets a successful result.
        /// </summary>
        /// <returns>A <see cref="PreOperationResult"/> indicating success.</returns>
#pragma warning disable S2743 // Static fields should not be used in generic types
        public static PreOperationResult<TRequest> Ok { get; } = new(true);
#pragma warning restore S2743 // Static fields should not be used in generic types

        /// <summary>
        /// Creates a failed result with an error message.
        /// </summary>
        /// <param name="errorMessage">The error message describing why the operation failed.</param>
        /// <returns>A <see cref="PreOperationResult"/> indicating failure.</returns>
        public static PreOperationResult<TRequest> Fail(string errorMessage) => new(false, errorMessage);

        public static PreOperationResult<TRequest> RequestUpdated(TRequest updatedRequest)
            => new(true, null, updatedRequest);
    }
}
