using Microsoft.EntityFrameworkCore;

namespace Genius.Atom.Data.Ef;

public interface IRepository<TKey, TReference, TGetDto, TCreateDto, TUpdateDto>
    where TKey : notnull
    where TReference : IReference<TKey, TReference>
    where TUpdateDto: IPrimaryId<TKey, TReference>, ITimeStamped
{
    Task<TGetDto> GetByIdAsync(TReference id, DbContext? context = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<TGetDto>> GetAllAsync(DbContext? context = null, CancellationToken cancellationToken = default);
    Task<CreatedEntityDto<TKey, TReference>> CreateAsync(TCreateDto createDto, DbContext? context = null, CancellationToken cancellationToken = default);
    Task<UpdatedEntityDto<TKey, TReference>> UpdateAsync(TUpdateDto updateDto, DbContext? context = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(TReference id, DbContext? context = null, CancellationToken cancellationToken = default);
}
