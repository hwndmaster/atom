namespace Genius.Atom.Infrastructure.Entities;

public interface IQueryService<TEntityDto>
{
    //IEnumerable<TEntityDto> GetAll();
    Task<IEnumerable<TEntityDto>> GetAllAsync();
    //TEntityDto? FindById(Guid entityId);
    Task<TEntityDto?> FindByIdAsync(Guid entityId);
}
