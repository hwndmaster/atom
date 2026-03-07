namespace Genius.Atom.Data.JsonPersistence;

public interface IQueryService<TEntityDto>
{
    Task<IEnumerable<TEntityDto>> GetAllAsync();
    Task<TEntityDto?> FindByIdAsync(Guid entityId);
}
