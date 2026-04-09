namespace Genius.Atom.Data.Ef;

public interface IRepository<TKey, TReference, TGetDto, TCreateDto, TUpdateDto>
    where TKey : notnull
    where TReference : IReference<TKey, TReference>
    where TUpdateDto: IPrimaryId<TKey, TReference>, ITimeStamped
{
    Task<TGetDto> GetByIdAsync(TReference id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TGetDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CreatedEntityDto<TKey, TReference>> CreateAsync(TCreateDto createDto, CancellationToken cancellationToken = default);
    Task<UpdatedEntityDto<TKey, TReference>> UpdateAsync(TUpdateDto updateDto, CancellationToken cancellationToken = default);
    Task DeleteAsync(TReference id, CancellationToken cancellationToken = default);
}
