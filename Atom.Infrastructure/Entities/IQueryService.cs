namespace Genius.Atom.Infrastructure.Entities;

public interface IQueryService<TEntityDto>
{
    Task<IEnumerable<TEntityDto>> GetAllAsync();
    Task<TEntityDto?> FindByIdAsync(Guid entityId);
}
